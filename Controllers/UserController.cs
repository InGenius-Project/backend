using AutoMapper;
using AutoWrapper.Models;
using AutoWrapper.Wrappers;
using IngBackend.Exceptions;
using IngBackend.Interfaces.Service;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;
using IngBackend.Services.TokenServices;
using IngBackend.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngBackend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController : BaseController
{
    private readonly TokenService _tokenService;
    private readonly UserService _userService;

    private readonly IMapper _mapper;

    public UserController(TokenService tokenService, UserService userService, IMapper mapper)
    {
        _tokenService = tokenService;
        _userService = userService;
        _mapper = mapper;
    }

    /// <summary>
    /// 取得當前已登入的使用者資訊
    /// </summary>
    /// <returns>使用者資訊</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDTO<UserInfoDTO>), StatusCodes.Status200OK)]
    public async Task<UserInfoDTO> GetUser()
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.GetUserByIdAsync(userId);

        if (user == null)
        {
            throw new UserNotFoundException();
        }

        return user;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseDTO<UserInfoDTO>), StatusCodes.Status200OK)]
    public async Task<UserInfoDTO> PostUser(UserInfoPostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.CheckAndGetUserAsync(userId);

        await _userService.UpdateAsync(user);
        await _userService.SaveChangesAsync();
        var userDTO = _mapper.Map<UserInfoDTO>(user);
        return userDTO;
    }



    [AllowAnonymous]
    [HttpPost("signup")]
    [ProducesResponseType(typeof(ResponseDTO<TokenDTO>), StatusCodes.Status200OK)]
    public async Task<TokenDTO> SignUp([FromBody] UserSignUpDTO req)
    {
        if (await _userService.GetUserByEmailAsync(req.Email.ToLower()) != null)
        {
            throw new BadRequestException("帳號已經存在");
        }

        var user = _mapper.Map<UserInfoDTO>(req);
        await _userService.AddAsync(user);
        await _userService.SaveChangesAsync();

        // TODO: send auth email

        await _userService.UpdateAsync(user);
        await _userService.SaveChangesAsync();

        // TODO: add send email process to background job

        var TokenDTO = _tokenService.GenerateToken(user);

        return TokenDTO;

    }


    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ResponseDTO<TokenDTO>), StatusCodes.Status200OK)]
    public async Task<TokenDTO> Login(UserSignInDTO req)
    {
        // 驗證密碼
        // TODO : VerifyHashedPassword input userdto 
        var user = await _userService.VerifyHashedPasswordAsync(req);

        // 轉換為 UserDTO 回傳
        var tokenDTO = _tokenService.GenerateToken(user);
        return tokenDTO;
    }

}
