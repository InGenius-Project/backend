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
using AutoWrapper.Wrappers;

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
    [ProducesResponseType(typeof(ResponseDTO<TagDTO>), StatusCodes.Status200OK)]
    public async Task<TagDTO> GetTag(Guid id)
    {
        var tag = await _tagService.GetByIdAsync(id)
         ?? throw new NotFoundException("標籤不存在");

        var tagDTO = _mapper.Map<TagDTO>(tag);

        return tagDTO;
    }
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDTO<List<TagDTO>>), StatusCodes.Status200OK)]
    public async Task<List<TagDTO>> GetTags([FromQuery] string? type)
    {
        var tags = new List<Tag>();
        if (type == null)
        {
            tags = [.. _tagService.GetAll().Include(t => t.Type)];
        }
        else
        {
            tags = await _tagService.GetAllTagsByType(type);
        }

        var tagsDTO = _mapper.Map<List<TagDTO>>(tags);
        return tagsDTO;
    }
    [HttpPost]
    [ProducesResponseType(typeof(ResponseDTO<>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> PostTag([FromBody] TagDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);

        var tagType = await _tagService.GetTagTypeById(req.Type.Id);
        if (tagType == null)
        {
            throw new TagNotFoundException(req.Id.ToString());
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
        return new ApiResponse("標籤已新增");
    }

    [HttpGet("type/{id}")]
    [ProducesResponseType(typeof(ResponseDTO<TagTypeDTO>), StatusCodes.Status200OK)]
    public async Task<TagTypeDTO> GetTagType(int id)
    {
        var tagType = await _tagService.GetTagTypeById(id)
            ?? throw new NotFoundException("標籤");

        var tagTypeDTO = _mapper.Map<TagTypeDTO>(tagType);

        return tagTypeDTO;
    }

    [HttpGet("type")]
    [ProducesResponseType(typeof(ResponseDTO<List<TagTypeDTO>>), StatusCodes.Status200OK)]
    public async Task<List<TagTypeDTO>> GetTagTypes()
    {
        var tagTypes = await _tagService.GetAllTagTypes();
        var tagTypesDTO = _mapper.Map<List<TagTypeDTO>>(tagTypes);
        return tagTypesDTO;
    }

    [HttpPost("type")]
    [ProducesResponseType(typeof(ResponseDTO<>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> PostTagType([FromBody] TagTypeDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId, [UserRole.Admin, UserRole.InternalUser]);

        var tagType = await _tagService.GetTagTypeById(req.Id);
        if (tagType == null)
        {
            var newTagType = _mapper.Map<TagType>(req);
            await _tagService.AddTagTypeAsync(newTagType);
            await _tagService.SaveChangesAsync();
            return new ApiResponse("新增成功");
        }
        _mapper.Map(req, tagType);
        await _tagService.UpdateTagTypeAsync(tagType);
        await _tagService.SaveChangesAsync();
        return new ApiResponse("更新成功");
    }

    [HttpDelete("type")]
    [ProducesResponseType(typeof(ResponseDTO<>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> DeleteTagTypes([FromBody] List<int> ids)
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
        return new ApiResponse("刪除成功");
    }





}
