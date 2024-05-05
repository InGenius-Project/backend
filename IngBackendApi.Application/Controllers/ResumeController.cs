namespace IngBackendApi.Controllers;

using AutoMapper;
using AutoWrapper.Wrappers;
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
public class ResumeController(
    IMapper mapper,
    IUserService userService,
    IResumeService resumeService,
    IBackgroundTaskService backgroundTaskService,
    IAIService aiService
) : BaseController
{
    private readonly IResumeService _resumeService = resumeService;
    private readonly IUserService _userService = userService;
    private readonly IMapper _mapper = mapper;
    private readonly IBackgroundTaskService _backgroundTaskService = backgroundTaskService;
    private readonly IAIService _aiService = aiService;

    [HttpGet]
    [ProducesResponseType(typeof(ResponseDTO<ResumeDTO>), StatusCodes.Status200OK)]
    public async Task<List<ResumeDTO>> GetResumesByUser()
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);

        var resume = await _resumeService.GetUserResumesAsync(userId);
        var resumeDTO = _mapper.Map<List<ResumeDTO>>(resume);
        return resumeDTO;
    }

    [HttpGet("{resumeId}")]
    [ProducesResponseType(typeof(ResponseDTO<ResumeDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumeDTO>> GetResume(Guid resumeId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId);

        var resumes = await _resumeService.CheckAndGetResumeAsync(resumeId, user);

        var resumeDTO = _mapper.Map<ResumeDTO>(resumes);
        return resumeDTO;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseDTO<ResumeDTO>), StatusCodes.Status200OK)]
    public async Task<ResumeDTO> PostResume([FromBody] ResumePostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId, u => u.Resumes);

        var resume = await _resumeService.AddOrUpdateAsync(_mapper.Map<ResumeDTO>(req), userId);
        return resume;
    }

    [HttpDelete("{resumeId}")]
    [ProducesResponseType(typeof(ResponseDTO<>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> DeleteResume(Guid resumeId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId);
        await _resumeService.CheckAndGetResumeAsync(resumeId, user);
        await _resumeService.DeleteByIdAsync(resumeId);
        return new ApiResponse("刪除成功");
    }

    [HttpGet("{resumeId}/analyze")]
    public async Task<ApiResponse> AnalyzeResume(Guid resumeId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        if (!await _userService.CheckUserIsPremium(userId))
        {
            throw new UnauthorizedException("User have no access to premium feature.");
        }
        if (!await _resumeService.CheckResumeOwnership(userId, resumeId))
        {
            throw new UnauthorizedException("Not resume owner.");
        }

        // Analyze Keywords & safety report
        await _backgroundTaskService.ScheduleTaskAsync(
            $"{resumeId}_keyword",
            () => AnalyzeKeywordAsync(resumeId, AreaGenType.Resume),
            TimeSpan.FromMinutes(5)
        );

        return new ApiResponse("請求已接受");
    }

    [HttpGet("{resumeId}/relative")]
    public async Task<IEnumerable<RecruitmentDTO>> SearchRelativeRecruitment(Guid resumeId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        if (!await _userService.CheckUserIsPremium(userId))
        {
            throw new UnauthorizedException("User have no access to premium feature.");
        }
        if (!await _resumeService.CheckResumeOwnership(userId, resumeId))
        {
            throw new UnauthorizedException("Not resume owner.");
        }

        return await _resumeService.SearchRelativeRecruitmentAsync(resumeId);
    }

    [NonAction]
    public async Task AnalyzeKeywordAsync(Guid targetId, AreaGenType type)
    {
        var result = await _aiService.GetKeywordsByAIAsync(targetId, type);
        await _aiService.SetKeywordsAsync(result, targetId, type);
    }
}
