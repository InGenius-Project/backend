namespace IngBackend.Controllers;
using AutoMapper;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;
using IngBackend.Exceptions;
using IngBackend.Services.TagService;
using IngBackend.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IngBackend.Enum;
using AutoWrapper.Wrappers;
using IngBackend.Interfaces.Service;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class TagController(
    IMapper mapper,
    UserService userService,
    TagService tagService,
    IService<TagType, TagTypeDTO, int> tagTypeService) : BaseController
{
    private readonly UserService _userService = userService;
    private readonly IMapper _mapper = mapper;
    private readonly TagService _tagService = tagService;
    private readonly IService<TagType, TagTypeDTO, int> _tagTypeService = tagTypeService;

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseDTO<TagDTO>), StatusCodes.Status200OK)]
    public async Task<TagDTO> GetTag(Guid id)
    {
        var tag = await _tagService.GetByIdAsync(id) ?? throw new NotFoundException("標籤不存在");
        return tag;
    }
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDTO<List<TagDTO>>), StatusCodes.Status200OK)]
    public async Task<List<TagDTO>> GetTags([FromQuery] string? type)
    {
        var tags = new List<TagDTO>();
        if (type == null)
        {
            tags = [.. _tagService.GetAll(t => t.Type)];
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
        await _tagService.PostTag(req);
        return new ApiResponse("標籤已新增");
    }

    [HttpGet("type/{id}")]
    [ProducesResponseType(typeof(ResponseDTO<TagTypeDetailDTO>), StatusCodes.Status200OK)]
    public async Task<TagTypeDetailDTO> GetTagType(int id)
    {
        var tagType = await _tagService.GetTagTypeIncludeTags(id)
            ?? throw new NotFoundException($"標籤類型: {id}");
        return tagType;
    }

    [HttpGet("type")]
    [ProducesResponseType(typeof(ResponseDTO<List<TagTypeDTO>>), StatusCodes.Status200OK)]
    public List<TagTypeDTO> GetTagTypes()
    {
        var tagTypes = _tagTypeService.GetAll().ToList();
        return tagTypes;
    }

    [HttpPost("type")]
    [ProducesResponseType(typeof(ResponseDTO<>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> PostTagType([FromBody] TagTypeDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId, [UserRole.Admin, UserRole.InternalUser]);


        var tagType = await _tagTypeService.GetByIdAsync(req.Id);
        if (tagType == null)
        {
            await _tagTypeService.AddAsync(req);
            return new ApiResponse("新增成功");
        }
        _mapper.Map(req, tagType);

        await _tagTypeService.UpdateAsync(tagType);
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
            var tagType = await _tagTypeService.GetByIdAsync(id);
            if (tagType != null)
            {
                await _tagTypeService.DeleteByIdAsync(tagType.Id);
            }
        }

        return new ApiResponse("刪除成功");
    }

}
