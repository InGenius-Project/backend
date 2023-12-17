﻿using AutoMapper;
using IngBackend.Exceptions;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;
using IngBackend.Services.AreaService;
using IngBackend.Services.UserService;
using Microsoft.AspNetCore.Authorization;
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
        var user = await _userService.GetByIdAsync(userId);

        if (user == null)
        {
            throw new UserNotFoundException();
        }

        var area = _areaService.GetAreaIncludeAllById(areaId);

        if (area == null)
        {
            throw new NotFoundException("找不到區塊");
        }
        var areaDTO = _mapper.Map<AreaDTO>(area);
        return areaDTO;
    }


    [HttpPost("{areaId?}")]
    public async Task<ActionResult<AreaDTO>> PostArea(Guid? areaId, AreaPostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.GetByIdAsync(userId);

        if (user == null)
        {
            throw new UserNotFoundException();
        }

        var area = _areaService.GetAreaIncludeAllById(areaId ?? Guid.Empty);

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
        var user = await _userService.GetByIdAsync(userId);

        if (user == null)
        {
            throw new UserNotFoundException();
        }

        var area = await _areaService.GetByIdAsync(areaId);

        // Add Area
        if (area == null)
        {
            throw new NotFoundException("找不到區塊");
        }

        _areaService.Delete(area);

        await _areaService.SaveChangesAsync();

        return Ok();
    }

}