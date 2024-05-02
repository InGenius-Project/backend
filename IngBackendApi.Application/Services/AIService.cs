namespace IngBackendApi.Services;

using System.Collections.Generic;
using AutoMapper;
using IngBackendApi.Enum;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using IngBackendApi.Models.DTO.HttpResponse;
using IngBackendApi.Services.Http;
using Microsoft.EntityFrameworkCore;

public class AIService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    AiHttpClient aiHttpClient,
    UnsplashHttpClient unsplashHttpClient
) : IAIService
{
    private readonly AiHttpClient _aiHttpClient = aiHttpClient;
    private readonly UnsplashHttpClient _unsplashHttpClient = unsplashHttpClient;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IRepository<KeywordRecord, string> _keywordRepository = unitOfWork.Repository<
        KeywordRecord,
        string
    >();
    private readonly IRepository<Recruitment, Guid> _recruitmentRepository = unitOfWork.Repository<
        Recruitment,
        Guid
    >();
    private readonly IRepository<SafetyReport, Guid> _safetyReportRepository =
        unitOfWork.Repository<SafetyReport, Guid>();

    private readonly IEnumerable<string> _generatedAreaFilterList = ["技能", "教育背景", "作品"];
    private readonly Dictionary<IEnumerable<string>, IEnumerable<Area>?> _userAreaMap =
        new()
        {
            { ["簡介", "自我介紹"], null },
            { ["技能"], null },
            { ["經驗"], null },
            { ["教育"], null },
            { ["作品"], null }
        };

    private readonly Dictionary<IEnumerable<string>, IEnumerable<Area>?> _companyAreaMap =
        new()
        {
            { ["簡介", "職缺介紹"], null },
            { ["技能"], null },
            { ["經驗"], null },
            { ["教育"], null },
            { ["聯絡"], null }
        };

    private readonly IRepository<User, Guid> _userRepository = unitOfWork.Repository<User, Guid>();
    private readonly IRepository<AreaType, int> _areaTypeRepository = unitOfWork.Repository<
        AreaType,
        int
    >();
    private readonly IMapper _mapper = mapper;

    public async Task<string[]> GetKeywordsByAIAsync(Guid recruitmentId)
    {
        var areaArray =
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
                .SelectMany(a => a.Areas)
                .ToArrayAsync() ?? throw new NotFoundException("Recruitment not found");

        var content = string.Join("\n", areaArray.Select(a => a.ToString()));
        return await _aiHttpClient.PostKeyExtractionAsync(content);
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

    public async Task<IEnumerable<AreaDTO>> GenerateAreaAsync(
        Guid userId,
        string title,
        AreaGenType genType,
        int areaNum = 5,
        bool titleOnly = false
    )
    {
        var userInfoAreaList = await GetUserInfoAreasAsync(
            userId,
            genType == AreaGenType.Resume ? _userAreaMap : _companyAreaMap
        );
        // Generate Post DTO
        var areaGenerationDto = new AreaGenerationDTO()
        {
            TitleOnly = titleOnly,
            AreaNum = areaNum,
            Title = title,
            Areas = _mapper
                .Map<List<UserInfoAreaDTO>>(userInfoAreaList)
                .Where(i => i != null)
                .ToList(),
            Type = genType
        };

        // Post And Get Response
        var generatedArea = await _aiHttpClient.PostGenerateAreasAsync(areaGenerationDto);

        // Classify Generated Data
        var areaDTOs = _mapper.Map<List<AreaDTO>>(generatedArea);
        await AfterGeneratedProcessAsync(areaDTOs, _mapper.Map<List<AreaDTO>>(userInfoAreaList));

        return areaDTOs;
    }

    public async Task<IEnumerable<AreaDTO>> GenerateAreaByTitleAsync(
        Guid userId,
        string title,
        IEnumerable<string> areaTitles,
        AreaGenType genType
    )
    {
        var userInfoAreaList = await GetUserInfoAreasAsync(
            userId,
            genType == AreaGenType.Resume ? _userAreaMap : _companyAreaMap
        );

        // Generate Post DTO
        var areaGenerationDto = new AreaGenerationDTO()
        {
            Title = title,
            Areas = _mapper
                .Map<List<UserInfoAreaDTO>>(userInfoAreaList)
                .Where(i => i != null)
                .ToList(),
            Type = genType
        };

        var postDTO = new GenerateAreaByTitleDTO
        {
            UserInfo = areaGenerationDto,
            AreaTitles = areaTitles
        };

        var generatedArea = await _aiHttpClient.PostGenerateAreasAsync(postDTO, true);

        // Area Filter
        var areaDTOs = _mapper.Map<List<AreaDTO>>(generatedArea);
        await AfterGeneratedProcessAsync(areaDTOs, _mapper.Map<List<AreaDTO>>(userInfoAreaList));

        return areaDTOs;
    }

    public async Task<ImageTextLayoutDTO> GenerateImageLayoutAreaAsync(
        string areaTitle,
        string textContent
    )
    {
        var keywords = await _aiHttpClient.PostKeyExtractionAsync(areaTitle);
        var imageSearchResults = await _unsplashHttpClient.GetSearchAsync(keywords);
        var result = imageSearchResults.Results.ToArray()[0];
        result.Urls.Download = result.Links.Download;
        return new ImageTextLayoutDTO
        {
            Id = Guid.Empty,
            TextContent = textContent,
            Image = new ImageInfo
            {
                Id = Guid.Empty,
                Uri = result.Urls.Raw,
                AltContent = result.Description,
                Urls = result.Urls,
                ContentType = "image/jpeg"
            }
        };
    }

    public async Task<SafetyReport> GenerateSafetyReportAsync(Guid recruitmentId)
    {
        var recruitment =
            await _recruitmentRepository
                .GetAll(r => r.Id == recruitmentId)
                .Include(r => r.Areas)
                .ThenInclude(a => a.TextLayout)
                .Include(r => r.Areas)
                .ThenInclude(a => a.ImageTextLayout)
                .Include(r => r.Areas)
                .ThenInclude(a => a.KeyValueListLayout)
                .Include(r => r.Areas)
                .ThenInclude(a => a.ListLayout)
                .Include(r => r.Publisher)
                .AsNoTracking()
                .FirstOrDefaultAsync() ?? throw new NotFoundException("Recruitment not found.");

        var companyName = recruitment.Publisher.Username;
        var safetyReportPost = new SafetyReportPost
        {
            CompanyName = companyName,
            Content = recruitment.ToString()
        };
        var response = await _aiHttpClient.PostSaftyReportAsync(safetyReportPost);
        var safetyReport = _mapper.Map<SafetyReport>(response);
        safetyReport.RecruitmentId = recruitmentId;
        return safetyReport;
    }

    public async Task SaveSafetyReportAsync(SafetyReport safetyReport)
    {
        var recruitment =
            await _recruitmentRepository
                .GetAll(r => r.Id == safetyReport.RecruitmentId)
                .SingleOrDefaultAsync() ?? throw new NotFoundException("Recruitment not found.");
        if (recruitment.SafetyReport != null)
        {
            _mapper.Map(safetyReport, recruitment.SafetyReport);
        }
        else
        {
            await _safetyReportRepository.AddAsync(safetyReport);
            recruitment.SafetyReport = safetyReport;
            recruitment.SafetyReportId = safetyReport.Id;
        }
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task AfterGeneratedProcessAsync(
        List<AreaDTO> areaDTOs,
        List<AreaDTO> userInfoAreaList
    )
    {
        // areaDTOs.RemoveAll(a => _generatedAreaFilterList.Any(f => a.Title.Contains(f)));

        // Determine AreaType
        foreach (var areaDTO in areaDTOs)
        {
            areaDTO.LayoutType = await GetAreaLayoutTypeAsync(areaDTO.Title);
            if (areaDTO.LayoutType == LayoutType.ImageText)
            {
                areaDTO.ImageTextLayout = await GenerateImageLayoutAreaAsync(
                    areaDTO.Title,
                    areaDTO.TextLayout?.Content ?? ""
                );
                areaDTO.TextLayout = null;
            }
        }

        var defaultArea = _generatedAreaFilterList
            .Select(a =>
                userInfoAreaList.FirstOrDefault(ar =>
                    ar.AreaType != null && a.Contains(ar.AreaType.Name)
                )
            )
            .Where(i => i != null)
            .ToList();

        areaDTOs.RemoveAll(a =>
            defaultArea.Any(f => f?.AreaType?.Name != null && a.Title.Contains(f.AreaType.Name))
        );

        areaDTOs.AddRange(_mapper.Map<List<AreaDTO>>(defaultArea));

        // Set Sequence
        var sequence = 0;
        areaDTOs.ForEach(a => a.Sequence = sequence++);
    }

    private async Task<LayoutType> GetAreaLayoutTypeAsync(string keyword)
    {
        var areaType = await _areaTypeRepository
            .GetAll(a => keyword.Contains(a.Name))
            .AsNoTracking()
            .FirstOrDefaultAsync();
        if (areaType == null)
        {
            return Enum.LayoutType.Text;
        }

        return areaType.LayoutType;
    }

    private async Task<IEnumerable<Area>> GetUserInfoAreasAsync(
        Guid userId,
        Dictionary<IEnumerable<string>, IEnumerable<Area>?> areaMap
    )
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

        // Get Area By Key Name
        foreach (var key in areaMap.Keys)
        {
            var areas = user
                .Areas.Where(a => a.AreaType != null)
                .Where(a => key.Any(k => a.AreaType.Name.Contains(k)))
                .ToList();
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
}
