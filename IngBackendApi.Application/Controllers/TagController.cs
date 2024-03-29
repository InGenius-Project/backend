namespace IngBackend.Controllers;

using IngBackendApi.Controllers;
using AutoMapper;
using AutoWrapper.Wrappers;
using IngBackendApi.Enum;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class TagController(
    IMapper mapper,
    IUserService userService,
    ITagService tagService,
    IService<TagType, TagTypeDTO, int> tagTypeService,
    IAreaService areaService
    ) : BaseController
{
    private readonly IUserService _userService = userService;
    private readonly IMapper _mapper = mapper;
    private readonly ITagService _tagService = tagService;
    private readonly IService<TagType, TagTypeDTO, int> _tagTypeService = tagTypeService;
    private readonly IAreaService _areaService = areaService;

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseDTO<TagDTO>), StatusCodes.Status200OK)]
    public async Task<TagDTO> GetTag(Guid id)
    {
        var tag = await _tagService.GetByIdAsync(id) ?? throw new NotFoundException("標籤不存在");

        return tag;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResponseDTO<List<TagDTO>>), StatusCodes.Status200OK)]
    public async Task<List<TagDTO>> GetTags([FromQuery] int[]? typeId)
    {
        if (typeId == null)
        {
            return _tagService.GetAll().ToList();
        }

        var tags = await _tagService.GetAllTagsByTypeIds(typeId);
        return tags;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseDTO<>), StatusCodes.Status200OK)]
    public async Task<TagDTO> PostTag([FromBody] TagPostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);
        var tagId = req.Id ?? Guid.Empty;

        var tagDto = await _tagService.GetByIdAsync(tagId);
        if (tagDto == null)
        {
            // new tag
            var newTag = _mapper.Map<TagDTO>(req);
            newTag = await _tagService.AddAsync(newTag);
            return newTag;
        }

        // check ownership
        await _tagService.CheckOwnerShip(tagId, userId);

        // update tag
        _mapper.Map(req, tagDto);
        await _tagService.UpdateAsync(tagDto);
        return tagDto;
    }

    [HttpGet("type/{id}")]
    [ProducesResponseType(typeof(ResponseDTO<TagTypeDTO>), StatusCodes.Status200OK)]
    public async Task<TagTypeDTO> GetTagType(int id)
    {
        var tagType = await _tagTypeService.GetByIdAsync(id) ?? throw new NotFoundException("標籤");

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
    public async Task<ApiResponse> PostTagType([FromBody] TagTypePostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId, [UserRole.Admin, UserRole.InternalUser]);

        if (req.Id == null)
        {
            var newTagType = _mapper.Map<TagTypeDTO>(req);
            await _tagTypeService.AddAsync(newTagType);
            return new ApiResponse("新增成功");
        }

        var tagType = await _tagTypeService.GetByIdAsync((int)req.Id);
        if (tagType == null)
        {
            var newTagType = _mapper.Map<TagTypeDTO>(req);
            await _tagTypeService.AddAsync(newTagType);
            return new ApiResponse("新增成功");
        }
        _mapper.Map(req, tagType);
        await _tagTypeService.AddAsync(tagType);
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
