using AutoMapper;
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
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public UserController(TokenService tokenService, UserService userService, IMapper mapper, IPasswordHasher passwordHasher)
    {
        _tokenService = tokenService;
        _userService = userService;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// 取得當前已登入的使用者資訊
    /// </summary>
    /// <returns>使用者資訊</returns>
    [HttpGet]
    [ProducesResponseType(typeof(UserInfoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserInfoDTO>> GetUser()
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.GetByIdAsync(userId);

        if (user == null)
        {
            throw new UserNotFoundException();
        }

        var userInfoDTO = _mapper.Map<UserInfoDTO>(user);
        return Ok(userInfoDTO);
    }

    [AllowAnonymous]
    [HttpPost("signup")]
    [ProducesResponseType(typeof(UserDTO), 201)]
    [ProducesResponseType(typeof(string), 400)]
    public async Task<ActionResult<UserDTO>> SignUp([FromBody] UserSignUpDTO req)
    {
        if (await _userService.GetUserByEmailAsync(req.Email.ToLower()) != null)
        {
            throw new BadRequestException("帳號已經存在");
        }

        var user = _mapper.Map<User>(req);
        await _userService.AddAsync(user);
        await _userService.SaveChangesAsync();

        // TODO: send auth email

        _userService.Update(user);
        await _userService.SaveChangesAsync();

        // TODO: add send email process to background job

        var userInfoDTO = _mapper.Map<UserInfoDTO>(user);
        var TokenDTO = _tokenService.GenerateToken(user);
        var userDTO = _mapper.Map<UserDTO>(userInfoDTO);
        _mapper.Map(TokenDTO, userDTO);

        return userDTO;

    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HttpResponseMessage>> Login(UserSignInDTO req)
    {
        // 從資料庫中尋找使用者
        var user = await _userService.GetUserByEmailAsync(req.Email.ToLower());
        if (user == null)
        {
            throw new BadRequestException("帳號或密碼錯誤");
        }

        // 驗證密碼
        var passwordValid = _passwordHasher.VerifyHashedPassword(user.HashedPassword, req.Password);
        if (!passwordValid)
        {
            throw new UnauthorizedException("帳號或密碼錯誤");
        }

        // 轉換為 UserDTO 回傳
        //var userInfoDTO = _mapper.Map<UserInfoDTO>(user);
        var tokenDTO = _tokenService.GenerateToken(user);
        //var userDTO = _mapper.Map<UserDTO>(userInfoDTO);
        //_mapper.Map(TokenDTO, userDTO);


        return Ok(tokenDTO);
    }




}
