using AutoMapper;
using AutoWrapper.Wrappers;
using IngBackend.Exceptions;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;
using IngBackend.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngBackend.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class ResumeController : BaseController
{
    private readonly ResumeService _resumeService;
    private readonly UserService _userService;
    private readonly IMapper _mapper;


    public ResumeController(IMapper mapper, UserService userService, ResumeService resumeService)
    {
        _resumeService = resumeService;
        _userService = userService;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResponseDTO<ResumeDTO>), StatusCodes.Status200OK)]
    public async Task<List<ResumeDTO>> GetResumes()
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);

        var resume = await _userService.GetResumesByUserId(userId);
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
    public async Task<ApiResponse> PostResume([FromBody] ResumePostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId, u => u.Resumes);

        var resume = await _resumeService.GetByIdAsync(req.Id ?? Guid.Empty);
        // Add new resume
        if (resume == null)
        {
            await _userService.AddUserResumeAsync(user, _mapper.Map<ResumeDTO>(req));
        }

        // Patch
        _mapper.Map(req, resume);
        await _resumeService.UpdateAsync(resume);
        return new ApiResponse("更新成功");
    }

    [HttpDelete("{resumeId}")]
    public async Task<ApiResponse> DeleteResume(Guid resumeId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var resume = await _resumeService.GetByIdAsync(resumeId);

        if (resume == null)
        {
            throw new NotFoundException("履歷");
        }

        await _resumeService.DeleteByIdAsync(resumeId);
        return new ApiResponse("刪除成功");
    }

}
