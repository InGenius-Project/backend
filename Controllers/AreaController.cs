﻿namespace IngBackend.Controllers;
using AutoMapper;
using AutoWrapper.Wrappers;
using IngBackend.Enum;
using IngBackend.Exceptions;
using IngBackend.Interfaces.Service;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;
using IngBackend.Services.AreaService;
using IngBackend.Services.TagService;
using IngBackend.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class AreaController(
    IMapper mapper,
    UserService userService,
    ResumeService resumeService,
    AreaService areaService,
    TagService tagService,
    IService<AreaType, AreaTypeDTO, int> areaTypeService,
    IService<TagType, TagTypeDTO, int> tagTypeService
        ) : BaseController
{
    private readonly ResumeService _resumeService = resumeService;
    private readonly UserService _userService = userService;
    private readonly AreaService _areaService = areaService;
    private readonly TagService _tagService = tagService;
    private readonly IMapper _mapper = mapper;
    private readonly IService<AreaType, AreaTypeDTO, int> _areaTypeService = areaTypeService;
    private readonly IService<TagType, TagTypeDTO, int> _tagTypeService = tagTypeService;

    [HttpGet("{areaId}")]
    [ProducesResponseType(typeof(ResponseDTO<AreaDTO>), StatusCodes.Status200OK)]
    public async Task<AreaDTO> GetAreaById(Guid areaId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);

        var area = await _areaService.GetAreaIncludeAllById(areaId)
            ?? throw new NotFoundException("找不到區塊");

        return area;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseDTO<AreaDTO>), StatusCodes.Status200OK)]
    public async Task<AreaDTO?> PostArea([FromBody] AreaPostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId);

        await _areaService.PostArea(req, user);
        return await _areaService.GetAreaIncludeAllById(req.Id);
    }

    // [HttpPut("{areaId}")]
    // [ProducesResponseType(typeof(ResponseDTO<AreaDTO>), StatusCodes.Status200OK)]
    // public async Task<AreaDTO> PutArea(Guid areaId, [FromForm] AreaFormDataDTO req)
    // {
    //     var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
    //     var user = await _userService.CheckAndGetUserIncludeAllAsync(userId);

    //     // 確定 area 所有權
    //     _areaService.CheckAreaOwnership(areaId, user);
    //     var area = _areaService.GetAreaIncludeAllById(areaId) ?? throw new NotFoundException("Area not found.");

    //     // 清空 Entity
    //     _areaService.ClearArea(area);
    //     _mapper.Map(req, area);
    //     await _areaService.UpdateAsync(area);
    //     await _areaService.SaveChangesAsync();

    //     var areaDTO = _mapper.Map<AreaDTO>(area);
    //     return areaDTO;
    // }

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
        var areaType = await _areaService.GetAreaTypeByIdAsync(id)
            ?? throw new NotFoundException("Area type not found");
        return areaType;
    }

    [HttpGet("type")]
    [ProducesResponseType(typeof(ResponseDTO<List<AreaTypeDTO>>), StatusCodes.Status200OK)]
    public async Task<List<AreaTypeDTO>?> GetAreaTypes([FromQuery] UserRole[]? roles)
    {
        var areaTypes = await _areaService.GetAreaTypeByRole(roles);
        return areaTypes;
    }

    [HttpPost("type")]
    [ProducesResponseType(typeof(ResponseDTO<>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> PostAreaType([FromBody] AreaTypePostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId, [UserRole.Admin, UserRole.InternalUser]);
        await _areaService.PostAreaType(req);
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
        Guid userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        UserInfoDTO user = await _userService.CheckAndGetUserIncludeAllAsync(userId);

        await _areaService.CheckAreaOwnership(areaId, user);

        var area =
            await _areaService.GetAreaIncludeAllById(areaId) ?? throw new NotFoundException("area 不存在");

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
        await _areaService.CheckAreaOwnership(areaId, user);

        AreaDTO? area =
           await _areaService.GetAreaIncludeAllById(areaId) ?? throw new NotFoundException("area 不存在");

        _ = area.ImageTextLayout ?? throw new BadRequestException("此 Area 沒有 Image Text Layout");
        ImageDTO? image = area.ImageTextLayout.Image ?? throw new NotFoundException("沒有圖片資料");

        MemoryStream imageStream = new(image.Data);
        return File(imageStream, image.ContentType);
    }
}
