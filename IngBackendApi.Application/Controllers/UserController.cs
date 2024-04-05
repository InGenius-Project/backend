namespace IngBackendApi.Controllers;

using AutoMapper;
using AutoWrapper.Wrappers;
using Hangfire;
using IngBackendApi.Enum;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DTO;
using IngBackendApi.Services;
using IngBackendApi.Services.TokenServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController(
    TokenService tokenService,
    IUserService userService,
    IMapper mapper,
    IPasswordHasher passwordHasher,
    IBackgroundJobClient backgroundJobClient,
    EmailService emailService
) : BaseController
{
    private readonly TokenService _tokenService = tokenService;
    private readonly IUserService _userService = userService;
    private readonly IMapper _mapper = mapper;
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;
    public readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly EmailService _emailService = emailService;

    /// <summary>
    /// 取得當前已登入的使用者資訊
    /// </summary>
    /// <returns>使用者資訊</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDTO<UserInfoDTO>), StatusCodes.Status200OK)]
    public async Task<UserInfoDTO> GetUser()
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user =
            await _userService.GetUserByIdIncludeAllAsync(userId)
            ?? throw new UserNotFoundException();

        return user;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseDTO<UserInfoDTO>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> PostUser(UserInfoPostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.PostUser(req, userId);
        return new ApiResponse("Post success");
    }

    [HttpGet("verifyEmail")]
    public async Task<ActionResult> VerifyEmail(string verifyCode)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        if (string.IsNullOrEmpty(verifyCode))
        {
            throw new BadRequestException("驗證碼不得為空");
        }

        var user =
            await _userService.GetByIdAsync(userId, user => user.EmailVerifications)
            ?? throw new UserNotFoundException();

        var result = await _userService.VerifyEmailVerificationCode(user, verifyCode);
        if (!result)
        {
            throw new BadRequestException("驗證碼不正確或已失效");
        }

        user.Verified = true;
        await _userService.UpdateAsync(user);

        return Ok("電子郵件驗證成功");
    }

    [AllowAnonymous]
    [HttpPost("signup")]
    [ProducesResponseType(typeof(ResponseDTO<TokenDTO>), StatusCodes.Status200OK)]
    public async Task<TokenDTO> SignUp([FromBody] UserSignUpDTO req)
    {
        if (
            await _userService.GetUserByEmailAsync(
                req.Email.ToLower(System.Globalization.CultureInfo.CurrentCulture)
            ) != null
        )
        {
            throw new BadRequestException("帳號已經存在");
        }

        var user = await _userService.AddAsync(req);

        // TODO: Add student verification function
        if (user.Role == UserRole.Intern)
        {
            var eduResult = _userService.VerifyEducationEmail(user.Email);
            if (!eduResult)
            {
                throw new BadRequestException("實習生電子郵件驗證失敗");
            }
        }

        // TODO: send auth email
        var token = await _userService.GenerateEmailConfirmationTokenAsync(user);
        var subject = "[noreply] InG 註冊驗證碼";
        var message = $"<h1>您的驗證碼是: {token}，此驗證碼於10分鐘後失效</h1>";

        // TODO: add send email process to background job
        _backgroundJobClient.Enqueue(
            () => _emailService.SendEmailAsync(user.Email, subject, message)
        );
        _backgroundJobClient.Enqueue(() => Console.WriteLine($"Email sent: {user.Email}"));

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
