namespace IngBackendApi.Controllers;

using AutoMapper;
using AutoWrapper.Filters;
using AutoWrapper.Wrappers;
using IngBackendApi.Enum;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class AreaController(
    IMapper mapper,
    IUserService userService,
    IAreaService areaService,
    IAreaTypeService areaTypeService,
    IWebHostEnvironment env
    ) : BaseController
{
    private readonly IUserService _userService = userService;
    private readonly IAreaService _areaService = areaService;
    private readonly IMapper _mapper = mapper;
    private readonly IAreaTypeService _areaTypeService = areaTypeService;
    private readonly IWebHostEnvironment _env = env;

    [HttpGet("{areaId}")]
    [ProducesResponseType(typeof(ResponseDTO<AreaDTO>), StatusCodes.Status200OK)]
    public async Task<AreaDTO> GetAreaById(Guid areaId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);

        var area =
            await _areaService.GetAreaIncludeAllById(areaId)
            ?? throw new NotFoundException("找不到區塊");

        return area;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseDTO<AreaDTO>), StatusCodes.Status200OK)]
    public async Task<AreaDTO> PostArea([FromBody] AreaPostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId);

        var parsedAreaId = req.Id ?? Guid.Empty;
        var area = await _areaService.GetAreaIncludeAllById(parsedAreaId);

        // Add User Relationship
        req.UserId = userId;

        // Add Area
        if (area == null)
        {
            var newAreaDTO = _mapper.Map<AreaDTO>(req);
            newAreaDTO = await _areaService.AddAsync(newAreaDTO);
            var areaEntity = await _areaService.GetAreaIncludeAllById(newAreaDTO.Id);
            return areaEntity;
        }

        // Patch Area
        await _areaService.CheckAreaOwnership(area.Id, user.Id);
        _mapper.Map(req, area);
        await _areaService.UpdateAsync(area);
        var areaDto = await _areaService.GetAreaIncludeAllById(area.Id) ?? throw new NotFoundException("找不到area");
        return areaDto;
    }

    [HttpDelete("{areaId}")]
    [ProducesResponseType(typeof(ResponseDTO<>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> DeleteArea(Guid areaId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);

        var area = await _areaService.GetByIdAsync(areaId) ?? throw new NotFoundException("找不到區塊");
        await _areaService.DeleteByIdAsync(area.Id);

        return new ApiResponse("區塊已刪除");
    }

    [HttpGet("type/{id}")]
    [ProducesResponseType(typeof(ResponseDTO<AreaTypeDTO>), StatusCodes.Status200OK)]
    public async Task<AreaTypeDTO> GetAreaType(int id)
    {
        var areaType =
            await _areaTypeService.GetByIdAsync(id, a => a.ListTagTypes)
            ?? throw new NotFoundException("Area type not found");
        var areaTypeDTO = _mapper.Map<AreaTypeDTO>(areaType);

        return areaTypeDTO;
    }

    [HttpGet("type")]
    [ProducesResponseType(typeof(ResponseDTO<List<AreaTypeDTO>>), StatusCodes.Status200OK)]
    public List<AreaTypeDTO> GetAreaTypes([FromQuery] UserRole[] roles)
    {
        var areaTypes = _areaTypeService.GetByRoles(roles);
        return areaTypes;
    }

    [HttpPost("type")]
    [Authorize(Roles = "Admin, InternalUser")]
    [ProducesResponseType(typeof(ResponseDTO<>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> PostAreaType([FromBody] AreaTypePostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);
        var tagTypeId = req.Id.GetValueOrDefault(0);

        if (tagTypeId == 0)
        {
            var newAreaTypeDto = _mapper.Map<AreaTypeDTO>(req);
            await _areaTypeService.AddAsync(newAreaTypeDto);
            return new ApiResponse("新增成功");
        }

        var areaType = await _areaTypeService.GetByIdAsync(tagTypeId);
        if (areaType == null)
        {
            var newAreaTypeDto = _mapper.Map<AreaTypeDTO>(req);
            await _areaTypeService.AddAsync(newAreaTypeDto);
            return new ApiResponse("新增成功");
        }

        _mapper.Map(req, areaType);

        await _areaTypeService.UpdateAsync(areaType);
        return new ApiResponse("更新成功");
    }

    [HttpDelete("type")]
    [Authorize(Roles = "Admin, InternalUser")]
    [ProducesResponseType(typeof(ResponseDTO<>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> DeleteAreaTypes([FromBody] List<int> ids)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId, [UserRole.Admin, UserRole.InternalUser]);

        foreach (var id in ids)
        {
            var areaType = await _areaTypeService.GetByIdAsync(id);
            if (areaType != null)
            {
                await _areaTypeService.DeleteByIdAsync(id);
            }
        }

        return new ApiResponse("刪除成功");
    }

    [HttpPost("listlayout")]
    public async Task<IActionResult> PostListLayout(Guid areaId, [FromBody] ListLayoutPostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId);

        // Check Ownership
        await _areaService.CheckAreaOwnership(areaId, user.Id);

        var listLayoutDTO = _mapper.Map<ListLayoutDTO>(req);
        await _areaService.UpdateLayoutAsync(areaId, listLayoutDTO);
        return Ok();
    }

    [HttpPost("textlayout")]
    public async Task<IActionResult> PostTextLayout(Guid areaId, [FromBody] TextLayoutPostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId);

        // Check Ownership
        await _areaService.CheckAreaOwnership(areaId, user.Id);

        var textLayoutDTO = _mapper.Map<TextLayoutDTO>(req);
        await _areaService.UpdateLayoutAsync(areaId, textLayoutDTO);
        return Ok();
    }

    [HttpPost("imagetextlayout")]
    public async Task<IActionResult> PostImageTextLayout(Guid areaId, [FromForm] ImageTextLayoutPostDTO imageTextLayoutPostDTO)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId);

        // Check Ownership
        await _areaService.CheckAreaOwnership(areaId, user.Id);

        // Check Image
        CheckImage(imageTextLayoutPostDTO.Image);
        await _areaService.UpdateLayoutAsync(areaId, imageTextLayoutPostDTO);
        return Ok();
    }

    [HttpPost("keyvaluelistlayout")]
    public async Task<IActionResult> PostKeyValueListLayout(Guid areaId, KeyValueListLayoutPostDTO keyValueListLayoutPostDTO)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId);

        // Check Ownership
        await _areaService.CheckAreaOwnership(areaId, user.Id);

        var keyValueListLayoutDTO = _mapper.Map<KeyValueListLayoutDTO>(keyValueListLayoutPostDTO);
        await _areaService.UpdateLayoutAsync(areaId, keyValueListLayoutDTO);
        return Ok();
    }

    [HttpPost("sequence")]
    public async Task<IActionResult> PostSequence(List<AreaSequencePostDTO> areaSequencePostDTOs)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);
        await _areaService.UpdateAreaSequenceAsync(areaSequencePostDTOs, userId);
        return Ok();
    }

    [AutoWrapIgnore]
    [AllowAnonymous]
    [HttpGet("image")]
    public async Task<IActionResult> GetImage(Guid id)
    {
        var imageDto = await _areaService.GetImageByIdAsync(id) ?? throw new NotFoundException("Image not found");

        var fullpath = Path.Combine(_env.WebRootPath, imageDto.Filepath);
        Console.WriteLine(fullpath);
        if (!System.IO.File.Exists(fullpath))
        {
            throw new NotFoundException("Image not found");
        }
        return PhysicalFile(fullpath, imageDto.ContentType);
    }

    private static void CheckImage(IFormFile image)
    {
        if (image.ContentType is not "image/jpeg" and not "image/png")
        {
            throw new BadRequestException("Image format must be JPEG or PNG");
        }

        if (image.Length > 10 * 1024 * 1024)
        {
            throw new BadRequestException("Image file size cannot exceed 10MB");
        }
    }
}
