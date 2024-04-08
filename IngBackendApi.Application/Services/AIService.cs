namespace IngBackendApi.Services;

using System.Collections.Generic;
using IngBackendApi.Exceptions;
using IngBackendApi.Helpers;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

public class AIService(IConfiguration configuration, IUnitOfWork unitOfWork) : IAIService
{
    private readonly string _aiAPI =
        configuration.GetSection("AI").GetSection("Api").Get<string>()
        ?? throw new NotFoundException("AI API not found");
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IRepository<KeywordRecord, string> _keywordRepository = unitOfWork.Repository<
        KeywordRecord,
        string
    >();
    private readonly IRepository<Recruitment, Guid> _recruitmentRepository = unitOfWork.Repository<
        Recruitment,
        Guid
    >();

    public async Task<string[]> GetKeywordsByAIAsync(string content)
    {
        var all_keywords = await _keywordRepository.GetAll().Select(k => k.Id).ToArrayAsync();

        var requestBody = new Dictionary<string, object>() { ["content"] = content };

        var response = await Helper.SendRequestAsync(_aiAPI, requestBody);
        var responseContent = await response.Content.ReadAsStringAsync();

        var jsonArray =
            JArray.Parse(responseContent)
            ?? throw new JsonParseException("AI response parse failed");

        return jsonArray.ToObject<string[]>()
            ?? throw new JsonParseException("AI response parse failed");
    }

    public async Task<string[]> GetKeywordsByAIAsync(Guid recruitmentId)
    {
        var recruitmentContentArray =
            await _recruitmentRepository
                .GetAll(r => r.Id == recruitmentId)
                .AsNoTracking()
                .Include(r => r.Areas)
                .ThenInclude(a => a.ListLayout)
                .Include(r => r.Areas)
                .ThenInclude(a => a.ImageTextLayout)
                .Include(r => r.Areas)
                .ThenInclude(a => a.TextLayout)
                .Include(r => r.Areas)
                .ThenInclude(a => a.KeyValueListLayout)
                .SelectMany(r => r.Areas.Select(area => ChoosingLayout(area)))
                .FirstOrDefaultAsync() ?? throw new NotFoundException("Recruitment not found");

        var content = string.Join(" ", recruitmentContentArray);
        return await GetKeywordsByAIAsync(content);
    }

    public async Task SetKeywordsAsync(string[] keywords, Guid recruitmentId)
    {
        var recruitment =
            await _recruitmentRepository
                .GetAll(r => r.Id == recruitmentId)
                .Include(r => r.Keywords)
                .FirstOrDefaultAsync() ?? throw new NotFoundException("Recruitment not found");

        await Parallel.ForEachAsync(
            keywords,
            new ParallelOptions { MaxDegreeOfParallelism = 10 },
            async (keyword, _) =>
            {
                var keywordEntity = await _keywordRepository
                    .GetAll(a => a.Id == keyword)
                    .Include(a => a.Recruitments)
                    .FirstOrDefaultAsync(_);
                if (keywordEntity != null)
                {
                    keywordEntity.Recruitments.Add(recruitment);
                    return;
                }
                await _keywordRepository.AddAsync(
                    new KeywordRecord { Id = keyword, Recruitments = [recruitment] }
                );
            }
        );

        await _unitOfWork.SaveChangesAsync();
    }

    private static string ChoosingLayout(Area area)
    {
        if (area.TextLayout != null)
        {
            return area.TextLayout.Content;
        }
        if (area.ImageTextLayout != null)
        {
            return area.ImageTextLayout.TextContent ?? "";
        }
        return "";
    }
}
