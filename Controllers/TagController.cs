using AutoMapper;
using IngBackend.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngBackend.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class TagController : BaseController
{
    private readonly UserService _userService;
    private readonly IMapper _mapper;


    public TagController(IMapper mapper, UserService userService)
    {
        _userService = userService;
        _mapper = mapper;
    }



}
