namespace IngBackendApi.Services;

using System.Collections.Generic;
using System.Text;
using IngBackendApi.Exceptions;
using IngBackendApi.Helpers;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using Microsoft.EntityFrameworkCore;
using MimeKit.Encodings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AIService(IConfiguration configuration, IUnitOfWork unitOfWork) : IAIService
{
    private readonly string _aiKeywordExctractionApi =
        configuration.GetSection("AI").GetSection("KeywordExtractionApi").Get<string>()
        ?? throw new NotFoundException("Keyword Extraction API not found");

    private readonly string _generateAreaApi =
        configuration.GetSection("AI").GetSection("GenerateAreaApi").Get<string>()
        ?? throw new NotFoundException("Generate Area API not found");

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IRepository<KeywordRecord, string> _keywordRepository = unitOfWork.Repository<
        KeywordRecord,
        string
    >();
    private readonly IRepository<Recruitment, Guid> _recruitmentRepository = unitOfWork.Repository<
        Recruitment,
        Guid
    >();

    private readonly IRepository<User, Guid> _userRepository = unitOfWork.Repository<User, Guid>();

    public async Task<string[]> GetKeywordsByAIAsync(string content)
    {
        var all_keywords = await _keywordRepository.GetAll().Select(k => k.Id).ToArrayAsync();

        var requestBody = new Dictionary<string, object>() { ["content"] = content };

        var response = await Helper.SendRequestAsync(_aiKeywordExctractionApi, requestBody);
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

        // Delete old keywords
        recruitment.Keywords.Clear();

        // Add new keywords
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

    public async Task<string> GenerateResumeArea(Guid userId)
    {
        var user =
            await _userRepository
                .GetAll(u => u.Id == userId)
                .Include(u => u.Areas)
                .ThenInclude(a => a.AreaType)
                .Include(u => u.Areas)
                .ThenInclude(u => u.ListLayout)
                .Include(u => u.Areas)
                .ThenInclude(u => u.TextLayout)
                .Include(u => u.Areas)
                .ThenInclude(u => u.ImageTextLayout)
                .Include(u => u.Areas)
                .ThenInclude(u => u.KeyValueListLayout)
                .ThenInclude(k => k.Items)
                .ThenInclude(i => i.Key)
                .AsNoTracking()
                .FirstOrDefaultAsync() ?? throw new NotFoundException("User not found");

        // Extract user info from areas
        var selfIntro = user.Areas.FirstOrDefault(a => a.AreaType.Name.Contains("簡介"));
        var userSkills = user.Areas.FirstOrDefault(a => a.AreaType.Name.Contains("技能"));
        var userExperience = user.Areas.FirstOrDefault(a => a.AreaType.Name.Contains("經驗"));
        var userEducation = user.Areas.FirstOrDefault(a => a.AreaType.Name.Contains("教育"));

        // intro
        var userIntroString = selfIntro?.TextLayout?.Content ?? "";

        // skill
        var skillArray = userSkills?.ListLayout?.Items?.Select(x => x.Name);
        var skillString = skillArray != null ? string.Join(", ", skillArray) : "";

        // experiences
        var experienceString = userExperience?.TextLayout?.Content ?? "";

        // education
        var educationArray = userEducation
            ?.KeyValueListLayout?.Items?.SelectMany(i => i.Key?.Select(k => k.Name))
            .Where(i => i != null);
        var educationString = educationArray != null ? string.Join(", ", educationArray) : "";

        var content = new StringBuilder();
        content.Append("簡介: ").Append(userIntroString).AppendLine();
        content.Append("技能: ").Append(skillString).AppendLine();
        content.Append("經驗: ").Append(experienceString).AppendLine();
        content.Append("教育背景: ").Append(educationString).AppendLine();

        return content.ToString();
    }

    public async Task<List<AiGeneratedAreaDTO>> GenerateArea(string content)
    {
        var response = await Helper.SendRequestAsync(
            _generateAreaApi,
            new Dictionary<string, object> { ["content"] = content }
        );
        var responseContent = await response.Content.ReadAsStringAsync();
        var generatedArea =
            JsonConvert.DeserializeObject<List<AiGeneratedAreaDTO>>(responseContent)
            ?? throw new JsonParseException("AI response parse failed");
        return generatedArea;
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
