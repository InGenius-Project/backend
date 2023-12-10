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
        var user = await _userService.GetByIdAsync(userId);

        if (user == null)
        {
            throw new UserNotFoundException();
        }

        var resumes = _resumeService
            .GetResumeByUser(userId)
            .ToList();

        var resumeDTO = _mapper.Map<List<ResumeDTO>>(resumes);

        return resumeDTO;

    }


    [HttpGet("{resumeId}")]
    public async Task<ActionResult<ResumeDTO>> GetResume(Guid resumeId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;

        var user = await _userService.GetByIdAsync(userId);


        if (user == null)
        {
            throw new UserNotFoundException();
        }

        var resumes = await _resumeService.GetByIdAsync(resumeId);

        if (resumes == null)
        {
            throw new NotFoundException("履歷不存在");
        }

        var resumeDTO = _mapper.Map<ResumeDTO>(resumes);

        return resumeDTO;

    }


    [HttpPost]
    public async Task<IActionResult> PostResume([FromBody] ResumePostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.GetByIdAsync(userId, u => u.Resumes);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        var reqResume = _mapper.Map<Resume>(req);

        if (req.Id.HasValue && req.Id != Guid.Empty)
        {
            var existResume = await _resumeService.GetByIdAsync(req.Id ?? Guid.Empty);

            // if resume exist, update it
            if (existResume != null)
            {
                existResume.Title = reqResume.Title;
                existResume.Areas = reqResume.Areas;
                _resumeService.Update(existResume);
            }
            else
            {
                throw new NotFoundException("此履歷不存在");
            }
        }
        else
        {
            user.Resumes ??= new List<Resume> { };
            user.Resumes.Add(reqResume);
        }

        await _userService.SaveChangesAsync();
        var resume = await _resumeService.GetByIdAsync(req.Id ?? Guid.Empty);
        var resumeDTO = _mapper.Map<ResumeDTO>(resume);
        return Ok(resumeDTO);
    }

    [HttpDelete("{resumeId}")]
    public async Task<IActionResult> DeleteResume(Guid resumeId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.GetByIdAsync(userId, u => u.Resumes);
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        var existResume = user.Resumes?.FirstOrDefault(x => x.Id == resumeId);

        if (existResume == null)
        {
            throw new NotFoundException("找不到履歷");
        }

        user.Resumes.Remove(existResume);

        _userService.Update(user);
        await _userService.SaveChangesAsync();
        return Ok();
    }

}
