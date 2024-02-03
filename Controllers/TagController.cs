using AutoMapper;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;
using IngBackend.Exceptions;
using IngBackend.Services.TagService;
using IngBackend.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IngBackend.Enum;
using Microsoft.EntityFrameworkCore;

namespace IngBackend.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class TagController : BaseController
{
    private readonly UserService _userService;
    private readonly IMapper _mapper;
    private readonly TagService _tagService;


    public TagController(IMapper mapper, UserService userService, TagService tagService)
    {
        _userService = userService;
        _mapper = mapper;
        _tagService = tagService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDTO>> GetTag(Guid id)
    {
        var tag = await _tagService.GetByIdAsync(id)
         ?? throw new NotFoundException("標籤不存在");
        
        var tagDTO = _mapper.Map<TagDTO>(tag) ;
        
        return tagDTO;
    }
    [HttpGet]
    public async Task<ActionResult<List<TagDTO>>> GetTags([FromQuery] string? type)
    {
        var tags = new List<Tag>();
        if(type == null)
        {
           tags = [.. _tagService.GetAll().Include(t =>t.Type)];
        }
        else{
            tags =  await _tagService.GetAllTagsByType(type);
        }
        
        var tagsDTO = _mapper.Map<List<TagDTO>>(tags);
        return tagsDTO;
    }
    [HttpPost]
    public async Task<IActionResult> PostTag([FromBody] TagDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);

        var tagType = await _tagService.GetTagTypeById(req.Type.Id);
        if (tagType == null)
        {
            throw new NotFoundException("Tag type not found");
        }

        var existingTag = await _tagService.GetByIdAsync(req.Id);
        if (existingTag == null)
        {
            var newTag = _mapper.Map<Tag>(req);
            newTag.Type = tagType;

            await _tagService.AddAsync(newTag);
        }
        else
        {
            _mapper.Map(req, existingTag);
            existingTag.Type = tagType;

           _tagService.Update(existingTag);
        }

        await _tagService.SaveChangesAsync();
        return Ok();
    }
    
    [HttpGet("type/{id}")]
    public async Task<ActionResult<TagTypeDTO>> GetTagType(int id)
    {
        var tagType = await _tagService.GetTagTypeById(id)
            ?? throw new NotFoundException("Tag type not found");

        var tagTypeDTO = _mapper.Map<TagTypeDTO>(tagType);

        return tagTypeDTO;
    }

    [HttpGet("type")]
    public async Task<ActionResult<List<TagTypeDTO>>> GetTagTypes()
    {
        var tagTypes = await _tagService.GetAllTagTypes();
        var tagTypesDTO = _mapper.Map<List<TagTypeDTO>>(tagTypes);
        return tagTypesDTO;
    }

    [HttpPost("type")]
    public async Task<IActionResult> PostTagType([FromBody] TagTypeDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId, [UserRole.Admin, UserRole.InternalUser]);

        var tagType = await _tagService.GetTagTypeById(req.Id);
        if(tagType == null)
        {
            var newTagType = _mapper.Map<TagType>(req);
            await _tagService.AddTagTypeAsync(newTagType);
            await _tagService.SaveChangesAsync();
            return Ok();
        }
        _mapper.Map(req, tagType);
        await _tagService.UpdateTagTypeAsync(tagType);
        await _tagService.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("type")]
    public async Task<IActionResult> DeleteTagTypes([FromBody] List<int> ids)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId, [UserRole.Admin, UserRole.InternalUser]);

        foreach (var id in ids)
        {
            var tagType = await _tagService.GetTagTypeById(id);
            if (tagType != null)
            {
                await _tagService.DeleteTagTypeAsync(tagType);
            }
        }

        await _tagService.SaveChangesAsync();
        return Ok();
}





}
