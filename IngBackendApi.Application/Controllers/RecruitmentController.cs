namespace IngBackend.Controllers;

using AutoMapper;
using AutoWrapper.Wrappers;
using IngBackendApi.Application.Attribute;
using IngBackendApi.Application.Hubs;
using IngBackendApi.Application.Interfaces;
using IngBackendApi.Application.Interfaces.Service;
using IngBackendApi.Controllers;
using IngBackendApi.Enum;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class RecruitmentController(
    IMapper mapper,
    IUserService userService,
    IRecruitmentService recruitmentService,
    IResumeService resumeService,
    IAIService aiService,
    IBackgroundTaskService backgroundTaskService
) : BaseController
{
    private readonly IMapper _mapper = mapper;
    private readonly IUserService _userService = userService;
    private readonly IRecruitmentService _recruitmentService = recruitmentService;
    private readonly IAIService _aiService = aiService;
    private readonly IResumeService _resumeService = resumeService;
    private readonly IBackgroundTaskService _backgroundTaskService = backgroundTaskService;

    [HttpGet]
    [ProducesResponseType(typeof(List<RecruitmentDTO>), 200)]
    [UserAuthorize(UserRole.Company)]
    public async Task<List<RecruitmentDTO>> GetRecruitmentsByUser()
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId, UserRole.Company);

        var recruitments = await _recruitmentService.GetPublisherRecruitmentsAsync(userId);

        var recruitmentsDTO = _mapper.Map<List<RecruitmentDTO>>(recruitments);
        return recruitmentsDTO;
    }

    [HttpGet("{recruitmentId}")]
    [ProducesResponseType(typeof(RecruitmentDTO), 200)]
    public async Task<RecruitmentDTO?> GetRecruitmentById(Guid recruitmentId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId);

        var recruitment = await _recruitmentService.GetRecruitmentByIdIncludeAllAsync(
            recruitmentId
        );

        // Attach resumes to recruitment if user is the publisher
        if (recruitment.PublisherId == userId && user.Role == UserRole.Company)
        {
            recruitment.Resumes = await _resumeService.GetRecruitmentResumesAsync(recruitmentId);
        }

        return recruitment;
    }

    [HttpPost]
    [UserAuthorize(UserRole.Company, UserRole.Admin)]
    [ProducesResponseType(typeof(ResponseDTO<RecruitmentDTO>), StatusCodes.Status200OK)]
    public async Task<RecruitmentDTO> PostRecruitment([FromBody] RecruitmentPostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);

        var recruitmentDTO = await _recruitmentService.AddOrUpdateAsync(
            _mapper.Map<RecruitmentDTO>(req),
            userId
        );
        return recruitmentDTO;
    }

    [HttpDelete("{recruitmentId}")]
    [UserAuthorize(UserRole.Company)]
    [ProducesResponseType(typeof(ResponseDTO<>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> DeleteRecruitment(Guid recruitmentId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;

        // TODO: recruiment owner check  is missing
        await _userService.CheckAndGetUserAsync(userId);
        await _recruitmentService.DeleteByIdAsync(recruitmentId);
        return new ApiResponse("刪除成功");
    }

    [HttpPost("search")]
    [AllowAnonymous]
    public async Task<RecruitmentSearchResultDTO> SearchRecruitment(RecruitmentSearchPostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var result = await _recruitmentService.SearchRecruitmentsAsync(req, userId);
        return result;
    }

    [HttpPost("{recruitmentId}/apply/{resumeId}")]
    [UserAuthorize(UserRole.Intern)]
    public async Task<ApiResponse> SendRecruitmentApply(Guid recruitmentId, Guid resumeId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);

        await _recruitmentService.ApplyRecruitmentAsync(recruitmentId, resumeId, userId);

        return new ApiResponse("申請成功");
    }

    [HttpGet("report/{recruitmentId}")]
    public async Task<SafetyReport> GetSafetyReport(Guid recruitmentId) =>
        await _recruitmentService.GetSafetyReportAsync(recruitmentId)
        ?? throw new NotFoundException("Report not found.");

    [HttpGet("{recruitmentId}/relative")]
    public async Task<IEnumerable<ResumeDTO>> SearchRelativeResumes(
        Guid recruitmentId,
        bool searchAll = false
    )
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        if (!await _userService.CheckUserIsPremium(userId))
        {
            throw new UnauthorizedException("User have no access to premium feature.");
        }
        if (!await _recruitmentService.CheckRecruitmentOwnershipAsync(userId, recruitmentId))
        {
            throw new UnauthorizedException("Not recruitment owner");
        }
        return await _recruitmentService.SearchRelativeResumeAsync(recruitmentId, searchAll);
    }

    [HttpGet("{recruitmentId}/apply/resume/analyze")]
    public async Task<ApiResponse> AnalyzeApplyedResume(Guid recruitmentId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        if (!await _userService.CheckUserIsPremium(userId))
        {
            throw new UnauthorizedException("User have no access to premium feature.");
        }
        if (!await _recruitmentService.CheckRecruitmentOwnershipAsync(userId, recruitmentId))
        {
            throw new UnauthorizedException("Not recruitment owner");
        }
        var resumeIds = await _recruitmentService.GetNotAnalyzedApplyedResumeIdAsync(recruitmentId);
        foreach (var resumeId in resumeIds)
        {
            // Analyze Keywords & safety report
            await _backgroundTaskService.ScheduleTaskAsync(
                $"{resumeId}_keyword",
                () => AnalyzeKeywordAsync(resumeId, AreaGenType.Resume),
                TimeSpan.FromMinutes(5)
            );
        }

        return new ApiResponse("請求已接受");
    }

    [NonAction]
    public async Task AnalyzeKeywordAsync(Guid targetId, AreaGenType type)
    {
        var result = await _aiService.GetKeywordsByAIAsync(targetId, type);
        await _aiService.SetKeywordsAsync(result, targetId, type);
    }
}
