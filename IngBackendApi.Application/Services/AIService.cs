namespace IngBackendApi.Services;

using System.Collections.Generic;
using System.Text;
using AutoMapper;
using IngBackendApi.Exceptions;
using IngBackendApi.Helpers;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AIService(IConfiguration configuration, IUnitOfWork unitOfWork, IMapper mapper)
    : IAIService
{
    private readonly string _aiKeywordExctractionApi =
        configuration.GetSection("AI").GetSection("KeywordExtractionApi").Get<string>()
        ?? throw new NotFoundException("Keyword Extraction API not found");

    private readonly string _generateAreaApi =
        configuration.GetSection("AI").GetSection("GenerateAreaApi").Get<string>()
        ?? throw new NotFoundException("Generate Area API not found");
    private readonly string _generateAreaByTitleApi =
        configuration.GetSection("AI").GetSection("GenerateAreaByTitle").Get<string>()
        ?? throw new NotFoundException("Generate Area By Title API not found");

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IRepository<KeywordRecord, string> _keywordRepository = unitOfWork.Repository<
        KeywordRecord,
        string
    >();
    private readonly IRepository<Recruitment, Guid> _recruitmentRepository = unitOfWork.Repository<
        Recruitment,
        Guid
    >();

    private readonly IEnumerable<string> _generatedAreaFilterList = ["技能", "教育背景", "作品"];

    private readonly IRepository<User, Guid> _userRepository = unitOfWork.Repository<User, Guid>();

    private readonly IMapper _mapper = mapper;

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
            _recruitmentRepository
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
                .ToList()
                .SelectMany(r =>
                {
                    var stringList = r.Areas.Select(area => ChoosingLayout(area)).ToList();
                    // Add Recruitment Name
                    stringList.Add(r.Name);
                    return stringList;
                })
                .ToArray() ?? throw new NotFoundException("Recruitment not found");

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
        keywords
            .ToList()
            .ForEach(async keyword =>
            {
                var keywordEntity = await _keywordRepository
                    .GetAll(a => a.Id == keyword)
                    .Include(a => a.Recruitments)
                    .FirstOrDefaultAsync();
                if (keywordEntity != null)
                {
                    keywordEntity.Recruitments.Add(recruitment);
                    return;
                }
                await _keywordRepository.AddAsync(
                    new KeywordRecord { Id = keyword, Recruitments = [recruitment] }
                );
            });

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<AreaDTO>> GenerateResumeAreaAsync(
        Guid userId,
        string resumeTitle,
        int areaNum = 5,
        bool titleOnly = false
    )
    {
        var userInfoAreaList = await GetUserInfoAreas(userId);
        // Generate Post DTO
        var userResumeGenerationDto = new UserResumeGenerationDTO()
        {
            TitleOnly = titleOnly,
            AreaNum = areaNum,
            ResumeTitle = resumeTitle,
            Areas = _mapper
                .Map<List<UserInfoAreaDTO>>(userInfoAreaList)
                .Where(i => i != null)
                .ToList()
        };

        // Post And Get Response
        var generatedArea = await GenerateAreaAsync(userResumeGenerationDto);

        // Classify Generated Data
        var areaDTOs = _mapper.Map<List<AreaDTO>>(generatedArea);
        areaDTOs.RemoveAll(a => _generatedAreaFilterList.Any(f => a.Title.Contains(f)));
        var defaultArea = _generatedAreaFilterList
            .Select(a =>
                userInfoAreaList.FirstOrDefault(ar =>
                    ar.AreaType != null && a.Contains(ar.AreaType.Name)
                )
            )
            .Where(i => i != null)
            .ToList();
        areaDTOs.AddRange(_mapper.Map<List<AreaDTO>>(defaultArea));

        // Set Sequence
        var sequence = 0;
        areaDTOs.ForEach(a => a.Sequence = sequence++);

        return areaDTOs;
    }

    public async Task<IEnumerable<AreaDTO>> GenerateResumeAreaByTitleAsync(
        Guid userId,
        string resumeTitle,
        IEnumerable<string> areaTitles
    )
    {
        var userInfoAreaList = await GetUserInfoAreas(userId);

        // Generate Post DTO
        var userResumeGenerationDto = new UserResumeGenerationDTO()
        {
            ResumeTitle = resumeTitle,
            Areas = _mapper
                .Map<List<UserInfoAreaDTO>>(userInfoAreaList)
                .Where(i => i != null)
                .ToList()
        };

        var postDTO = new GenerateAreaByTitleDTO
        {
            UserResumeInfo = userResumeGenerationDto,
            AreaTitles = areaTitles
        };

        var generatedArea = await GenerateAreaByTitleAsync(postDTO);
        return _mapper.Map<IEnumerable<AreaDTO>>(generatedArea);
    }

    public async Task<IEnumerable<AiGeneratedAreaDTO>> GenerateAreaAsync(object requestBody)
    {
        var response = await Helper.SendRequestAsync(_generateAreaApi, requestBody);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new BadRequestException("AI response failed");
        }
        var jsonString = new StringBuilder(responseContent.Trim("\\\"".ToCharArray()));
        jsonString.Replace("\\", "");

        var generatedArea =
            JsonConvert.DeserializeObject<IEnumerable<AiGeneratedAreaDTO>>(jsonString.ToString())
            ?? throw new JsonParseException("AI response parse failed");
        return generatedArea;
    }

    public async Task<IEnumerable<AiGeneratedAreaDTO>> GenerateAreaByTitleAsync(object requestBody)
    {
        var response = await Helper.SendRequestAsync(_generateAreaByTitleApi, requestBody);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new BadRequestException("AI response failed");
        }
        var jsonString = new StringBuilder(responseContent.Trim("\\\"".ToCharArray()));
        jsonString.Replace("\\", "");

        var generatedAreas =
            JsonConvert.DeserializeObject<IEnumerable<AiGeneratedAreaDTO>>(jsonString.ToString())
            ?? throw new JsonParseException("AI response parse failed");
        return generatedAreas;
    }

    private async Task<IEnumerable<Area>> GetUserInfoAreas(Guid userId)
    {
        var user =
            await _userRepository
                .GetAll(u => u.Id == userId)
                .Include(u => u.Areas)
                .ThenInclude(a => a.AreaType)
                .Include(u => u.Areas)
                .ThenInclude(u => u.ListLayout)
                .ThenInclude(l => l.Items)
                .ThenInclude(i => i.Type)
                .Include(u => u.Areas)
                .ThenInclude(u => u.TextLayout)
                .Include(u => u.Areas)
                .ThenInclude(u => u.ImageTextLayout)
                .Include(u => u.Areas)
                .ThenInclude(u => u.KeyValueListLayout)
                .ThenInclude(k => k.Items)
                .ThenInclude(i => i.Key)
                .ThenInclude(k => k.Type)
                .AsNoTracking()
                .FirstOrDefaultAsync() ?? throw new NotFoundException("User not found");

        // Map selected area
        var areaMap = new Dictionary<IEnumerable<string>, IEnumerable<Area>?>
        {
            { ["簡介", "自我介紹"], null },
            { ["技能"], null },
            { ["經驗"], null },
            { ["教育"], null },
            { ["作品"], null }
        };

        // Get Area By Key Name
        foreach (var key in areaMap.Keys)
        {
            var areas = user.Areas.Where(a => key.Any(k => a.AreaType.Name.Contains(k))).ToList();
            areas.ForEach(a =>
            {
                if (a.ListLayout != null)
                {
                    a.ListLayout.Id = Guid.Empty;
                }
                if (a.TextLayout != null)
                {
                    a.TextLayout.Id = Guid.Empty;
                }
                if (a.ImageTextLayout != null)
                {
                    a.ImageTextLayout.Id = Guid.Empty;
                }
                if (a.KeyValueListLayout != null)
                {
                    a.KeyValueListLayout.Id = Guid.Empty;
                }
            });
            areaMap[key] = areas;
        }
        var returnValue = areaMap.Values.SelectMany(a => a.Select(aa => aa));
        return returnValue;
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
