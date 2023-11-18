using IngBackend.Controllers;
using IngBackend.Models.DTO;
using IngBackend.Services.TokenServices;
using IngBackend.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngBackend.Controllersp;

[ApiController]
[Authorize]
[Route("api/[controller]")]

public class UserController : BaseController
{
    private readonly TokenService _tokenService;
    private readonly UserService _userService;

    public UserController(TokenService tokenService, UserService userService)
    {
        _tokenService = tokenService;
        _userService = userService;
    }

    [HttpPost("signup")]
    public async Task<ActionResult<UserDTO>> SignUp([FromBody] UserSignUpDTO req)
    {
        if (_userService.GetUserByEmail(req.Email.ToLower()) != null)
        {
            return BadRequest("Bye");
        }

        return Ok();

    }
}
