using AutoMapper;
using IngBackend.Controllers;
using IngBackend.Exceptions;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;
using IngBackend.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lng_backend.Controllers;

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
    public async Task<ActionResult<IEnumerable<ResumeDTO>>> GetResume()
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.GetByIdAsync(userId);

        if (user == null)
        {
            throw new UserNotFoundException();
        }

        var resumes = _resumeService
            .GetResumeByUser(userId)
            .DefaultIfEmpty()
            .ToList();

        var resumeDTO = _mapper.Map<List<ResumeDTO>>(resumes);

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

        var newResume = _mapper.Map<Resume>(req);

        var existResume = user.Resumes?.FirstOrDefault(x => x.Id == req.Id);

        // if resume exist, update it
        if (existResume != null)
        {
            existResume.TextLayouts = newResume.TextLayouts;
            existResume.ImageTextLayouts = newResume.ImageTextLayouts;
            existResume.Title = newResume.Title;
            _userService.Update(user);
            await _userService.SaveChangesAsync();
            return Ok();
        }

        user.Resumes ??= new List<Resume> { };
        user.Resumes.Add(newResume);

        _userService.Update(user);

        await _userService.SaveChangesAsync();
        return Ok();
    }
}
