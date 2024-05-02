namespace IngBackendApi.Controllers;

using AutoMapper;
using AutoWrapper.Filters;
using AutoWrapper.Wrappers;
using IngBackendApi.Application.Attribute;
using IngBackendApi.Application.Interfaces.Service;
using IngBackendApi.Enum;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class AreaController(
    IMapper mapper,
    IUserService userService,
    IAreaService areaService,
    IAreaTypeService areaTypeService,
    IRecruitmentService recruitmentService,
    IResumeService resumeService,
    IWebHostEnvironment env,
    IAIService aiService,
    IBackgroundTaskService backgroundTaskService
) : BaseController
{
    private readonly IUserService _userService = userService;
    private readonly IAreaService _areaService = areaService;
    private readonly IRecruitmentService _recruitmentService = recruitmentService;
    private readonly IResumeService _resumeService = resumeService;
    private readonly IMapper _mapper = mapper;
    private readonly IAreaTypeService _areaTypeService = areaTypeService;
    private readonly IWebHostEnvironment _env = env;
    private readonly IAIService _aiService = aiService;
    private readonly IBackgroundTaskService _backgroundTaskService = backgroundTaskService;

    private readonly Dictionary<Guid, string?> _taskMap = [];

    [HttpGet("{areaId}")]
    [ProducesResponseType(typeof(ResponseDTO<AreaDTO>), StatusCodes.Status200OK)]
    public async Task<AreaDTO> GetAreaById(Guid areaId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);

        var area =
            await _areaService.GetAreaIncludeAllById(areaId)
            ?? throw new NotFoundException("找不到區塊");

        return area;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseDTO<AreaDTO>), StatusCodes.Status200OK)]
    public async Task<AreaDTO> PostArea([FromBody] AreaPostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);

        // check all id exist
        var parsedUserId = userId;
        var parsedRecruimentId = req.RecruitmentId ?? Guid.Empty;
        var parsedResumeId = req.ResumeId ?? Guid.Empty;
        var parsedAreaTypeId = req.AreaTypeId.GetValueOrDefault();

        if (parsedUserId != Guid.Empty)
        {
            await _userService.CheckAndGetUserAsync(parsedUserId);
        }

        if (parsedRecruimentId != Guid.Empty)
        {
            var _ =
                await _recruitmentService.GetByIdAsync(parsedRecruimentId)
                ?? throw new NotFoundException($"Recruitment {parsedRecruimentId} not found");
        }

        if (parsedResumeId != Guid.Empty)
        {
            var _ =
                await _resumeService.GetByIdAsync(parsedResumeId)
                ?? throw new NotFoundException($"Resume {parsedResumeId} not found");
        }

        if (parsedAreaTypeId != 0)
        {
            var _ =
                await _areaTypeService.GetByIdAsync(parsedAreaTypeId)
                ?? throw new NotFoundException($"AreaType {req.AreaTypeId} not found");
        }

        // Add User Relationship
        var areaDTO = _mapper.Map<AreaDTO>(req);

        areaDTO = await _areaService.AddOrUpdateAsync(areaDTO, parsedUserId);
        areaDTO =
            await _areaService.GetAreaIncludeAllById(areaDTO.Id)
            ?? throw new NotFoundException("This is a internalError. Contact the manager.");

        return areaDTO;
    }

    [HttpDelete("{areaId}")]
    [ProducesResponseType(typeof(ResponseDTO<>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> DeleteArea(Guid areaId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);

        var area = await _areaService.GetByIdAsync(areaId) ?? throw new NotFoundException("找不到區塊");
        await _areaService.DeleteByIdAsync(area.Id);

        return new ApiResponse("區塊已刪除");
    }

    [HttpGet("type/{id}")]
    [ProducesResponseType(typeof(ResponseDTO<AreaTypeDTO>), StatusCodes.Status200OK)]
    public async Task<AreaTypeDTO> GetAreaType(int id)
    {
        var areaType =
            await _areaTypeService.GetByIdAsync(id, a => a.ListTagTypes)
            ?? throw new NotFoundException("Area type not found");
        var areaTypeDTO = _mapper.Map<AreaTypeDTO>(areaType);

        return areaTypeDTO;
    }

    [HttpGet("type")]
    [ProducesResponseType(typeof(ResponseDTO<List<AreaTypeDTO>>), StatusCodes.Status200OK)]
    public List<AreaTypeDTO> GetAreaTypes([FromQuery] UserRole[] roles)
    {
        var areaTypes = _areaTypeService.GetByRoles(roles);
        return areaTypes;
    }

    [HttpPost("type")]
    [UserAuthorize(UserRole.Admin, UserRole.InternalUser)]
    [ProducesResponseType(typeof(ResponseDTO<>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> PostAreaType([FromBody] AreaTypePostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);
        var tagTypeId = req.Id.GetValueOrDefault(0);

        if (tagTypeId == 0)
        {
            var newAreaTypeDto = _mapper.Map<AreaTypeDTO>(req);
            await _areaTypeService.AddAsync(newAreaTypeDto);
            return new ApiResponse("新增成功");
        }

        var areaType = await _areaTypeService.GetByIdAsync(tagTypeId);
        if (areaType == null)
        {
            var newAreaTypeDto = _mapper.Map<AreaTypeDTO>(req);
            await _areaTypeService.AddAsync(newAreaTypeDto);
            return new ApiResponse("新增成功");
        }

        _mapper.Map(req, areaType);

        await _areaTypeService.UpdateAsync(areaType);
        return new ApiResponse("更新成功");
    }

    [HttpDelete("type")]
    [UserAuthorize(UserRole.Admin, UserRole.InternalUser)]
    [ProducesResponseType(typeof(ResponseDTO<>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> DeleteAreaTypes([FromBody] List<int> ids)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId, [UserRole.Admin, UserRole.InternalUser]);

        foreach (var id in ids)
        {
            var areaType = await _areaTypeService.GetByIdAsync(id);
            if (areaType != null)
            {
                await _areaTypeService.DeleteByIdAsync(id);
            }
        }

        return new ApiResponse("刪除成功");
    }

    [HttpPost("listlayout")]
    public async Task<IActionResult> PostListLayout(Guid areaId, [FromBody] ListLayoutPostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId);

        // Check Ownership
        await _areaService.CheckAreaOwnership(areaId, user.Id);

        var listLayoutDTO = _mapper.Map<ListLayoutDTO>(req);
        await _areaService.UpdateLayoutAsync(areaId, listLayoutDTO);

        await _backgroundTaskService.ScheduleTaskAsync(
            $"{areaId}_report",
            () => GenerateSafetyReportAsync(areaId),
            TimeSpan.FromMinutes(1)
        );

        return Ok();
    }

    [HttpPost("textlayout")]
    public async Task<IActionResult> PostTextLayout(Guid areaId, [FromBody] TextLayoutPostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId);

        // Check Ownership
        await _areaService.CheckAreaOwnership(areaId, user.Id);

        var textLayoutDTO = _mapper.Map<TextLayoutDTO>(req);
        await _areaService.UpdateLayoutAsync(areaId, textLayoutDTO);

        // Analyze Keywords & safety report
        await _backgroundTaskService.ScheduleTaskAsync(
            $"{areaId}_keyword",
            () => AnalyzeRecruitmentKeywordAsync(areaId),
            TimeSpan.FromMinutes(1)
        );

        await _backgroundTaskService.ScheduleTaskAsync(
            $"{areaId}_report",
            () => GenerateSafetyReportAsync(areaId),
            TimeSpan.FromMinutes(1)
        );

        return Ok();
    }

    [HttpPost("imagetextlayout")]
    public async Task<IActionResult> PostImageTextLayout(
        Guid areaId,
        [FromForm] ImageTextLayoutPostDTO imageTextLayoutPostDTO
    )
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId);

        // Check Ownership
        await _areaService.CheckAreaOwnership(areaId, user.Id);

        // Check Image
        await _areaService.UpdateLayoutAsync(areaId, imageTextLayoutPostDTO);

        // Analyze Keywords & safety report
        await _backgroundTaskService.ScheduleTaskAsync(
            $"{areaId}_keyword",
            () => AnalyzeRecruitmentKeywordAsync(areaId),
            TimeSpan.FromMinutes(1)
        );

        await _backgroundTaskService.ScheduleTaskAsync(
            $"{areaId}_report",
            () => GenerateSafetyReportAsync(areaId),
            TimeSpan.FromMinutes(1)
        );

        return Ok();
    }

    [HttpPost("keyvaluelistlayout")]
    public async Task<IActionResult> PostKeyValueListLayout(
        Guid areaId,
        KeyValueListLayoutPostDTO keyValueListLayoutPostDTO
    )
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId);

        // Check Ownership
        await _areaService.CheckAreaOwnership(areaId, user.Id);

        var keyValueListLayoutDTO = _mapper.Map<KeyValueListLayoutDTO>(keyValueListLayoutPostDTO);
        await _areaService.UpdateLayoutAsync(areaId, keyValueListLayoutDTO);

        await _backgroundTaskService.ScheduleTaskAsync(
            $"{areaId}_report",
            () => GenerateSafetyReportAsync(areaId),
            TimeSpan.FromMinutes(1)
        );

        return Ok();
    }

    [HttpPost("sequence")]
    public async Task<IActionResult> PostSequence(List<AreaSequencePostDTO> areaSequencePostDTOs)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);
        await _areaService.UpdateAreaSequenceAsync(areaSequencePostDTOs, userId);
        return Ok();
    }

    [AutoWrapIgnore]
    [AllowAnonymous]
    [HttpGet("image")]
    public async Task<IActionResult> GetImage(Guid id)
    {
        var imageDto =
            await _areaService.GetImageByIdAsync(id)
            ?? throw new NotFoundException("Image not found");

        var fullpath = Path.Combine(_env.WebRootPath, imageDto.Filepath);
        if (!System.IO.File.Exists(fullpath))
        {
            throw new NotFoundException("Image not found");
        }
        return PhysicalFile(fullpath, imageDto.ContentType);
    }

    [HttpPost("generation")]
    public async Task<IEnumerable<AreaDTO>> GenerateArea([FromBody] GenerateAreaPostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);

        return await _aiService.GenerateAreaAsync(
            userId,
            req.Title,
            req.Type,
            req.AreaNum,
            req.TitleOnly
        );
    }

    [HttpPost("generation/title")]
    public async Task<IEnumerable<AreaDTO>> GenerateAreaByAreaTitle(GenerateAreaByTitlePostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);
        return await _aiService.GenerateAreaByTitleAsync(
            userId,
            req.Title,
            req.AreaTitles,
            req.Type
        );
    }

    [HttpGet("areaType/{areaTypeId}")]
    public async Task<IEnumerable<AreaDTO>> GetUserAreaByAreaType(int areaTypeId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        return await _areaService.GetUserAreaByAreaTypeIdAsync(userId, areaTypeId);
    }

    [NonAction]
    public async Task AnalyzeRecruitmentKeywordAsync(Guid areaId)
    {
        var area = await _areaService.GetByIdAsync(areaId);
        if (area == null || area.RecruitmentId == null)
        {
            return;
        }
        var parsedRecruitmentId = area.RecruitmentId ?? Guid.Empty;

        var result = await _aiService.GetKeywordsByAIAsync(parsedRecruitmentId);
        await _aiService.SetKeywordsAsync(result, parsedRecruitmentId);
    }

    [NonAction]
    public async Task GenerateSafetyReportAsync(Guid areaId)
    {
        var area = await _areaService.GetAreaIncludeAllById(areaId);
        if (area == null || area.RecruitmentId == null)
        {
            return;
        }
        var entity = await _aiService.GenerateSafetyReportAsync((Guid)area.RecruitmentId);
        await _aiService.SaveSafetyReportAsync(entity);
    }
}
