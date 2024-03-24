namespace IngBackendApi.Controllers;

using AutoMapper;
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
    IAreaTypeService areaTypeService
    ) : BaseController
{
    private readonly IUserService _userService = userService;
    private readonly IAreaService _areaService = areaService;
    private readonly IMapper _mapper = mapper;
    private readonly IAreaTypeService _areaTypeService = areaTypeService;

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

        return _mapper.Map<AreaDTO>(req);
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
            await _areaTypeService.GetByIdAsync(id)
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

        if (req.Id is null or 0)
        {
            var newAreaTypeDto = _mapper.Map<AreaTypeDTO>(req);
            await _areaTypeService.AddAsync(newAreaTypeDto);
            return new ApiResponse("新增成功");
        }

        var areaType = await _areaTypeService.GetByIdAsync((int)req.Id);
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

    [HttpPost("image")]
    public async Task<IActionResult> UploadAreaImage([FromQuery] Guid areaId, IFormFile image)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserIncludeAllAsync(userId);

        await _areaService.CheckAreaOwnership(areaId, user.Id);

        var area =
            await _areaService.GetAreaIncludeAllById(areaId)
            ?? throw new NotFoundException("area 不存在");

        if (area.ImageTextLayout == null)
        {
            throw new BadRequestException("此 Area 沒有 Image Text Layout");
        }

        byte[] imageData;
        using (MemoryStream memoryStream = new())
        {
            await image.CopyToAsync(memoryStream);
            imageData = memoryStream.ToArray();
        }

        area.ImageTextLayout.Image = new ImageDTO()
        {
            Filename = image.FileName,
            ContentType = image.ContentType,
            Data = imageData
        };

        await _areaService.UpdateAsync(area);

        return Accepted();
    }

    [HttpGet("image")]
    public async Task<IActionResult> GetImage([FromQuery] Guid areaId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserIncludeAllAsync(userId);

        // Check CheckAreaOwnership
        await _areaService.CheckAreaOwnership(areaId, user.Id);

        var area =
            await _areaService.GetAreaIncludeAllById(areaId)
            ?? throw new NotFoundException("area 不存在");

        _ = area.ImageTextLayout ?? throw new BadRequestException("此 Area 沒有 Image Text Layout");
        var image = area.ImageTextLayout.Image ?? throw new NotFoundException("沒有圖片資料");

        MemoryStream imageStream = new(image.Data);
        return File(imageStream, image.ContentType);
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
}