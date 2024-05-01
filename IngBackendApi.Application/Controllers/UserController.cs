namespace IngBackendApi.Controllers;

using AutoMapper;
using AutoWrapper.Filters;
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
    EmailService emailService,
    IWebHostEnvironment env
) : BaseController
{
    private readonly TokenService _tokenService = tokenService;
    private readonly IUserService _userService = userService;
    private readonly IMapper _mapper = mapper;
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;
    public readonly IPasswordHasher _passwordHasher = passwordHasher;
    public readonly IWebHostEnvironment _env = env;
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

    [HttpGet("Profile/{userId}")]
    [ProducesResponseType(typeof(ResponseDTO<UserInfoDTO>), StatusCodes.Status200OK)]
    public async Task<UserInfoDTO> GetUserProfile(Guid userId)
    {
        var user =
            await _userService.GetUserByIdIncludeAllAsync(userId)
            ?? throw new UserNotFoundException();

        if (user.Role != UserRole.Company)
        {
            throw new ForbiddenException();
        }

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
        var allowedRoles = new List<UserRole>() { UserRole.Intern, UserRole.Company };

        if (!allowedRoles.Contains(req.Role))
        {
            throw new BadRequestException($"{req.Role} not allowed");
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
        var token = await _userService.GenerateEmailConfirmationTokenAsync(user.Id);
        var subject = "[noreply] InG 註冊驗證碼";
        var message = $"<h1>您的驗證碼是: {token}，此驗證碼於10分鐘後失效</h1>";

        // TODO: add send email process to background job
        _backgroundJobClient.Enqueue(
            () => _emailService.SendEmailAsync(user.Email, subject, message)
        );
        _backgroundJobClient.Enqueue(() => Console.WriteLine($"Email sent: {user.Email}"));

        var tokenDTO = _tokenService.GenerateToken(user);

        return tokenDTO;
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

    [HttpPost("avatar")]
    public async Task<ApiResponse> UploadAvatar([FromForm] IFormFile image)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);
        await _userService.SaveUserAvatarAsync(userId, image);
        return new ApiResponse("Upload success");
    }

    [AutoWrapIgnore]
    [AllowAnonymous]
    [HttpGet("avatar")]
    public async Task<IActionResult> GetAvatar(Guid? userId, Guid? imageId)
    {
        if (userId != null)
        {
            var parsedUserId = userId ?? Guid.Empty;
            var userDTO =
                await _userService.GetByIdAsync(parsedUserId, u => u.Avatar)
                ?? throw new UserNotFoundException();

            if (userDTO.Avatar == null)
            {
                throw new NotFoundException("Avatar not found");
            }

            var fullpath = Path.Combine(_env.WebRootPath, userDTO.Avatar.Filepath);
            if (!System.IO.File.Exists(fullpath))
            {
                throw new NotFoundException("Avatar not found");
            }
            return PhysicalFile(fullpath, userDTO.Avatar.ContentType);
        }
        else if (imageId != null)
        {
            var parsedImageId = imageId ?? Guid.Empty;
            var imageDto =
                await _userService.GetImageByIdAsync(parsedImageId)
                ?? throw new NotFoundException("Image not found");
            var fullpath = Path.Combine(_env.WebRootPath, imageDto.Filepath);
            if (!System.IO.File.Exists(fullpath))
            {
                throw new NotFoundException("Image not found");
            }
            return PhysicalFile(fullpath, imageDto.ContentType);
        }
        throw new BadRequestException("userId and imageId cannot be null at the same time");
    }

    [HttpGet("fav/recruitment")]
    public async Task<List<RecruitmentDTO>> GetFavRecruitments()
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.CheckAndGetUserAsync(userId);
        var recruitments = await _userService.GetFavoriteRecruitmentsAsync(userId);
        return recruitments;
    }

    [HttpPost("fav/recruitment")]
    public async Task<ApiResponse> AddFavRecruitment([FromBody] List<Guid> recruitmentIds)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.AddFavoriteRecruitmentAsync(userId, recruitmentIds);
        return new ApiResponse("Add fav recruitment success");
    }

    [HttpDelete("fav/recruitment")]
    public async Task<ApiResponse> RemoveFavRecruitment([FromBody] List<Guid> recruitmentIds)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.RemoveFavoriteRecruitmentAsync(userId, recruitmentIds);
        return new ApiResponse("Remove fav recruitment success");
    }

    [HttpPost("background")]
    public async Task<ApiResponse> PostUserBackground([FromForm] ImagePostDTO req)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        req.ValidateModel();
        await _userService.UpdateUserBackgroundAsync(userId, req);
        return new ApiResponse("Ok");
    }

    [HttpDelete("background")]
    public async Task<ApiResponse> DeleteUserBackground()
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        await _userService.RemoveUserBackgroundAsync(userId);
        return new ApiResponse("Ok");
    }
}
