using AutoMapper;
using IngBackend.Exceptions;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;
using IngBackend.Services.AreaService;
using IngBackend.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Mvc;

namespace IngBackend.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class AreaController : BaseController
{
    private readonly ResumeService _resumeService;
    private readonly UserService _userService;
    private readonly AreaService _areaService;
    private readonly IMapper _mapper;

    public AreaController(
        IMapper mapper,
        UserService userService,
        ResumeService resumeService,
        AreaService areaService
    )
    {
        _resumeService = resumeService;
        _userService = userService;
        _areaService = areaService;
        _mapper = mapper;
    }

    [HttpGet("{areaId}")]
    public async Task<ActionResult<AreaDTO>> GetAreaById(Guid areaId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);

        var area =
            _areaService.GetAreaIncludeAllById(areaId) ?? throw new NotFoundException("找不到區塊");

        var areaDTO = _mapper.Map<AreaDTO>(area);
        return areaDTO;
    }

    [HttpPost]
    public async Task<ActionResult<AreaDTO>> PostArea(
        [FromQuery] Guid? areaId,
        [FromBody] AreaPostDTO req
    )
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserIncludeAllAsync(userId);

        var parsedAreaId = areaId ?? Guid.Empty;
        var area = await _areaService.GetByIdAsync(parsedAreaId);

        // Add Area
        if (area == null)
        {
            var newArea = _mapper.Map<Area>(req);
            newArea.User = user;
            await _areaService.AddAsync(newArea);
            await _areaService.SaveChangesAsync();

            var newAreaDTO = _mapper.Map<AreaDTO>(newArea);
            return newAreaDTO;
        }

        // Check Area Ownership
        _areaService.CheckAreaOwnership(parsedAreaId, user);

        // Update Area
        _mapper.Map(req, area);
        _areaService.Update(area);

        await _areaService.SaveChangesAsync();

        var areaDTO = _mapper.Map<AreaDTO>(area);
        return areaDTO;
    }

    [HttpPut("{areaId}")]
    public async Task<ActionResult<AreaDTO>> PutArea(Guid areaId, [FromForm] AreaFormDataDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserIncludeAllAsync(userId);

        // 確定 area 所有權
        _areaService.CheckAreaOwnership(areaId, user);
        var area =
            _areaService.GetAreaIncludeAllById(areaId)
            ?? throw new NotFoundException("Area not found.");

        // 清空 Entity
        _areaService.ClearArea(area);
        _mapper.Map(req, area);
        _areaService.Update(area);
        await _areaService.SaveChangesAsync();

        var areaDTO = _mapper.Map<AreaDTO>(area);
        return areaDTO;
    }

    [HttpDelete("{areaId}")]
    public async Task<IActionResult> DeleteArea(Guid areaId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);

        var area = await _areaService.GetByIdAsync(areaId) ?? throw new NotFoundException("找不到區塊");
        _areaService.Delete(area);
        await _areaService.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("image")]
    public async Task<IActionResult> UploadAreaImage([FromQuery] Guid areaId, IFormFile image)
    {
        Guid userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        User user = await _userService.CheckAndGetUserIncludeAllAsync(userId);

        // Check CheckAreaOwnership
        _areaService.CheckAreaOwnership(areaId, user);

        var area =
            _areaService.GetAreaIncludeAllById(areaId) ?? throw new NotFoundException("area 不存在");

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

        area.ImageTextLayout.Image = new Image()
        {
            Filename = image.FileName,
            ContentType = image.ContentType,
            Data = imageData
        };

        _areaService.Update(area);
        await _areaService.SaveChangesAsync();

        return Accepted();
    }

    [HttpGet("image")]
    public async Task<IActionResult> GetImage([FromQuery] Guid areaId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserIncludeAllAsync(userId);

        // Check CheckAreaOwnership
        _areaService.CheckAreaOwnership(areaId, user);

        Area? area =
            _areaService.GetAreaIncludeAllById(areaId) ?? throw new NotFoundException("area 不存在");

        _ = area.ImageTextLayout ?? throw new BadRequestException("此 Area 沒有 Image Text Layout");
        Image? image = area.ImageTextLayout.Image ?? throw new NotFoundException("沒有圖片資料");

        MemoryStream imageStream = new(image.Data);
        return File(imageStream, image.ContentType);
    }
}
