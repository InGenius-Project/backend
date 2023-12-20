using AutoMapper;
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
    public async Task<ActionResult<List<ResumeDTO>>> GetResumes()
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);

        var resume = await _userService.GetUserResumes(userId);
        var resumeDTO = _mapper.Map<List<ResumeDTO>>(resume);
        return resumeDTO;
    }

    [HttpGet("{resumeId}")]
    public async Task<ActionResult<ResumeDTO>> GetResume(Guid resumeId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;

        var user = await _userService.CheckAndGetUserAsync(userId);
        var resumes = await _resumeService.CheckAndGetResumeAsync(resumeId, user);

        var resumeDTO = _mapper.Map<ResumeDTO>(resumes);
        return resumeDTO;
    }

    [HttpPost]
    public async Task<ActionResult<ResumeDTO>> PostResume([FromBody] ResumePostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId, u => u.Resumes);

        var resume = await _resumeService.GetByIdAsync(req.Id ?? Guid.Empty);

        // Add new resume
        if (resume == null)
        {
            var newResume = _mapper.Map<Resume>(req);
            user.Resumes.Add(newResume);
            _userService.Update(user);
            await _userService.SaveChangesAsync();
            return _mapper.Map<ResumeDTO>(newResume);
        }
       
        // Patch
        _mapper.Map(req, resume);
        _userService.Update(user);
        await _userService.SaveChangesAsync();
        var resumeDTO = _mapper.Map<ResumeDTO>(resume);
        return resumeDTO;
    }

    [HttpDelete("{resumeId}")]
    public async Task<IActionResult> DeleteResume(Guid resumeId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId, u => u.Resumes);

        if (user.Resumes == null)
        {
            throw new NotFoundException("找不到履歷");
        }

        var existResume = user.Resumes.FirstOrDefault(x => x.Id == resumeId) ?? throw new NotFoundException("找不到履歷");

        user.Resumes.Remove(existResume);
        _userService.Update(user);
        await _userService.SaveChangesAsync();
        return Ok();
    }

}
