namespace IngBackendApi.Controllers;

using AutoMapper;
using AutoWrapper.Wrappers;
using IngBackendApi.Application.Interfaces.Service;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class ResumeController(IMapper mapper, IUserService userService, IResumeService resumeService) : BaseController
{
    private readonly IResumeService _resumeService = resumeService;
    private readonly IUserService _userService = userService;
    private readonly IMapper _mapper = mapper;

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
