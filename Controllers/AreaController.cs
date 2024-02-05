using AutoMapper;
using IngBackend.Enum;
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



    public AreaController(IMapper mapper, UserService userService, ResumeService resumeService, AreaService areaService)
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

        var area = _areaService.GetAreaIncludeAllById(areaId)
            ?? throw new NotFoundException("找不到區塊");

        var areaDTO = _mapper.Map<AreaDTO>(area);
        return areaDTO;
    }

    [HttpPost("{areaId}")]
    public async Task<ActionResult<AreaDTO>> PostArea(Guid? areaId, [FromBody] AreaPostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId);

        var parsedAreaId = areaId ?? Guid.Empty;
        var area = _areaService.GetAreaIncludeAllById(parsedAreaId);


        // Add Area
        if (area == null)
        {
            var newArea = _mapper.Map<Area>(req);
            await _areaService.AddAsync(newArea);
            await _areaService.SaveChangesAsync();

            var newAreaDTO = _mapper.Map<AreaDTO>(newArea);
            return newAreaDTO;
        }

        // Patch Area
        _areaService.CheckAreaOwnership(area.Id, user);
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
        var area = _areaService.GetAreaIncludeAllById(areaId) ?? throw new NotFoundException("Area not found.");

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

    [HttpGet("type/{id}")]
    public async Task<ActionResult<TagTypeDTO>> GetAreaType(int id)
    {
        var areaType = await _areaService.GetAreaTypeById(id)
            ?? throw new NotFoundException("Area type not found");

        var areaTypeDTO = _mapper.Map<TagTypeDTO>(areaType);

        return areaTypeDTO;
    }

    [HttpGet("type")]
    public async Task<ActionResult<List<TagTypeDTO>>> GetAreaTypes()
    {
        var areaTypes = _areaService.GetAllAreaTypes();
        var areaTypesDTO = _mapper.Map<List<TagTypeDTO>>(areaTypes);
        return areaTypesDTO;
    }

    [HttpPost("type")]
    public async Task<IActionResult> PostAreaType([FromBody] TagTypeDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId, [UserRole.Admin, UserRole.InternalUser]);

        var areaType = await _areaService.GetAreaTypeById(req.Id);
        if (areaType == null)
        {
            var newAreaType = _mapper.Map<AreaType>(req);
            await _areaService.AddAreaTypeAsync(newAreaType);
            await _areaService.SaveChangesAsync();
            return Ok();
        }
        _mapper.Map(req, areaType);
        await _areaService.UpdateAreaTypeAsync(areaType);
        await _areaService.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("type")]
    public async Task<IActionResult> DeleteAreaTypes([FromBody] List<int> ids)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId, [UserRole.Admin, UserRole.InternalUser]);

        foreach (var id in ids)
        {
            var areaType = await _areaService.GetAreaTypeById(id);
            if (areaType != null)
            {
                await _areaService.DeleteAreaTypeAsync(areaType);
            }
        }

        await _areaService.SaveChangesAsync();
        return Ok();
    }
}
