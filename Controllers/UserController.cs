using AutoMapper;
using IngBackend.Exceptions;
using IngBackend.Interfaces.Service;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;
using IngBackend.Services.TokenServices;
using IngBackend.Services.UserService;
using IngBackend.Services;
using IngBackend.Exceptions;
using IngBackend.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hangfire;

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
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly EmailService _emailService;

    public UserController(
        TokenService tokenService, 
        UserService userService, 
        IMapper mapper, 
        IPasswordHasher passwordHasher,
        IBackgroundJobClient backgroundJobClient,
        EmailService emailService
        )
    {
        _tokenService = tokenService;
        _userService = userService;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _backgroundJobClient = backgroundJobClient;
        _emailService = emailService;
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
        var user = await _userService.GetUserByIdIncludeAll(userId);

        if (user == null)
        {
            throw new UserNotFoundException();
        }

        var userInfoDTO = _mapper.Map<UserInfoDTO>(user);
        return Ok(userInfoDTO);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserInfoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserInfoDTO>> PostUser(UserInfoPostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userService.GetByIdAsync(userId);

        if (user == null)
        {
            throw new UserNotFoundException();
        }

        _mapper.Map(req, user);
        _userService.Update(user);

        await _userService.SaveChangesAsync();
        var userDTO = _mapper.Map<UserInfoDTO>(user);
        return userDTO;
    }

    [HttpGet("verifyEmail")]
    public async Task<ActionResult> VerifyEmail(string verifyCode)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        if (string.IsNullOrEmpty(verifyCode))
        {
            throw new BadRequestException("驗證碼不得為空");
        }

        var user = await _userService.GetByIdAsync(userId,
            user => user.EmailVerifications);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        var result = _userService.VerifyEmailVerificationCode(user, verifyCode);
        if (!result)
        { 
            throw new BadRequestException("驗證碼不正確或已失效");
        }

        user.Verified = true;
        _userService.Update(user);
        await _userService.SaveChangesAsync();

        return Ok("電子郵件驗證成功");
    }


    [AllowAnonymous]
    [HttpPost("signup")]
    [ProducesResponseType(typeof(UserDTO), 201)]
    [ProducesResponseType(typeof(string), 400)]
    public async Task<ActionResult<TokenDTO>> SignUp([FromBody] UserSignUpDTO req)
    {
        if (await _userService.GetUserByEmailAsync(req.Email.ToLower()) != null)
        {
            throw new BadRequestException("帳號已經存在");
        }

        var user = _mapper.Map<User>(req);
        await _userService.AddAsync(user);
        await _userService.SaveChangesAsync();

        // TODO: Add student verification function
        if (user.Role == UserRole.Intern){
            var eduResult = _userService.VerifyEducationEmail(user.Email);
            if (!eduResult) {
                throw new BadRequestException("實習生電子郵件驗證失敗");
            }
        }

        // TODO: send auth email
        var token = await _userService.GenerateEmailConfirmationTokenAsync(user);
        var subject = "[noreply] InG 註冊驗證碼";
        var message = $"<h1>您的驗證碼是: {token}，此驗證碼於10分鐘後失效</h1>";


        _userService.Update(user);
        await _userService.SaveChangesAsync();

        // TODO: add send email process to background job
        _backgroundJobClient.Enqueue(() => _emailService.SendEmailAsync(user.Email, subject, message));
        _backgroundJobClient.Enqueue(() => Console.WriteLine($"Email sent: {user.Email}"));

        //var userInfoDTO = _mapper.Map<UserInfoDTO>(user);
        var TokenDTO = _tokenService.GenerateToken(user);
        //var userDTO = _mapper.Map<UserDTO>(userInfoDTO);
        //_mapper.Map(TokenDTO, userDTO);

        return TokenDTO;

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
