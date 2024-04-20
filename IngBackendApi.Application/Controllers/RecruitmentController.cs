namespace IngBackend.Controllers;

using AutoMapper;
using AutoWrapper.Wrappers;
using IngBackendApi.Application.Interfaces.Service;
using IngBackendApi.Controllers;
using IngBackendApi.Enum;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class RecruitmentController(
    IMapper mapper,
    IAreaService areaService,
    IUserService userService,
    IRecruitmentService recruitmentService,
    IResumeService resumeService,
    IAIService aiService
) : BaseController
{
    private readonly IMapper _mapper = mapper;
    private readonly IUserService _userService = userService;
    private readonly IRecruitmentService _recruitmentService = recruitmentService;
    private readonly IAreaService _areaService = areaService;
    private readonly IAIService _aiService = aiService;
    private readonly IResumeService _resumeService = resumeService;

    [HttpGet]
    [ProducesResponseType(typeof(List<RecruitmentDTO>), 200)]
    [Authorize(Roles = "Company")]
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
    [Authorize(Roles = "Company")]
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
    [Authorize(Roles = "Company")]
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
    [Authorize(Roles = "Intern")]
    public async Task<ApiResponse> SendRecruitmentApply(Guid recruitmentId, Guid resumeId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);

        await _recruitmentService.ApplyRecruitmentAsync(recruitmentId, resumeId, userId);
        return new ApiResponse("申請成功");
    }
}
