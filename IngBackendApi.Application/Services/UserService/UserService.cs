namespace IngBackendApi.Services.UserService;

using System.Linq.Expressions;
using AutoMapper;
using IngBackendApi.Enum;
using IngBackendApi.Exceptions;
using IngBackendApi.Helpers;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using IngBackendApi.Models.Settings;
using Microsoft.EntityFrameworkCore;

public class UserService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IRepositoryWrapper repository,
    IPasswordHasher passwordHasher,
    IWebHostEnvironment env,
    IConfiguration config,
    ISettingsFactory settingsFactory
) : Service<User, UserInfoDTO, Guid>(unitOfWork, mapper), IUserService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IRepositoryWrapper _repository = repository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IWebHostEnvironment _env = env;
    private readonly IConfiguration _config = config;
    private readonly IRepository<Image, Guid> _imageRepository = unitOfWork.Repository<
        Image,
        Guid
    >();

    private readonly IRepository<User, Guid> _userRepository = unitOfWork.Repository<User, Guid>();

    private readonly PathSetting _pathSetting = settingsFactory.GetSetting<PathSetting>();

    public async Task<UserInfoDTO?> GetUserByIdIncludeAllAsync(Guid userId)
    {
        var user = _repository.User.GetUserByIdIncludeAll(userId);
        return await _mapper.ProjectTo<UserInfoDTO>(user).FirstOrDefaultAsync();
    }

    public async Task PostUser(UserInfoPostDTO req, Guid userId)
    {
        var user = await _repository.User.GetByIdAsync(userId) ?? throw new UserNotFoundException();
        _mapper.Map(req, user);
        await _repository.User.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<UserInfoDTO> AddAsync(UserSignUpDTO req)
    {
        var user = _mapper.Map<User>(req);
        await _repository.User.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<UserInfoDTO>(user);
    }

    /// <summary>
    /// Asynchronously finds the user information that matches the specified email address.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve</param>
    /// <param name="includes">(optional) A list of related properties to retrieve</param>
    /// <returns>A `User` object containing the user information that matches the email address `email`, or null if no matching user exists</returns>
    public async Task<UserInfoDTO?> GetUserByEmailAsync(
        string email,
        params Expression<Func<User, object>>[] includes
    )
    {
        var query = _repository.User.GetUserByEmail(email);

        foreach (var include in includes)
        {
            query.Include(include);
        }

        return await _mapper.ProjectTo<UserInfoDTO>(query).FirstOrDefaultAsync();
    }

    public async Task<UserInfoDTO> CheckAndGetUserIncludeAllAsync(Guid userId)
    {
        var user = await GetUserByIdIncludeAllAsync(userId);
        return user ?? throw new UserNotFoundException();
    }

    /// <summary>
    /// Asynchronously checks if a user exists with the specified ID and retrieves the user information if found.
    /// </summary>
    /// <param name="userId">The ID of the user to check and retrieve (Guid).</param>
    /// <returns>A `User` object containing the user information if found.</returns>
    /// <exception cref="UserNotFoundException">Throws a `UserNotFoundException` if no user exists with the specified ID.</exception>
    public async Task<UserInfoDTO> CheckAndGetUserAsync(
        Guid userId,
        params Expression<Func<User, object>>[] includes
    )
    {
        var user = await GetByIdAsync(userId, includes) ?? throw new UserNotFoundException();
        return user;
    }

    /// <summary>
    /// Asynchronously checks if a user exists with the specified ID, retrieves the user information, and verifies if the user has the required role.
    /// </summary>
    /// <param name="userId">The ID of the user to check and retrieve (Guid).</param>
    /// <param name="allowedRole">The required role the user must have (UserRole).</param>
    /// <returns>A `User` object containing the user information if found and has the allowed role.</returns>
    /// <exception cref="UserNotFoundException">Throws a `UserNotFoundException` if no user exists with the specified ID.</exception>
    /// <exception cref="ForbiddenException">Throws a `ForbiddenException` if the user exists but does not have the required role.</exception>
    public async Task<UserInfoDTO> CheckAndGetUserAsync(
        Guid userId,
        UserRole allowedRole,
        params Expression<Func<User, object>>[] includes
    )
    {
        var user = await GetByIdAsync(userId, includes) ?? throw new UserNotFoundException();

        if (user.Role != allowedRole)
        {
            throw new ForbiddenException();
        }

        return user;
    }

    /// <summary>
    /// Asynchronously checks if a user exists with the specified ID, retrieves the user information, and verifies if the user belongs to any of the allowed roles.
    /// </summary>
    /// <param name="userId">The ID of the user to check and retrieve (Guid).</param>
    /// <param name="allowedRoles">An enumerable collection of allowed roles the user must belong to (IEnumerable<UserRole>).</param>
    /// <returns>A `User` object containing the user information if found and belongs to any of the allowed roles.</returns>
    /// <exception cref="UserNotFoundException">Throws a `UserNotFoundException` if no user exists with the specified ID.</exception>
    /// <exception cref="ForbiddenException">Throws a `ForbiddenException` if the user exists but does not belong to any of the allowed roles.</exception>
    public async Task<UserInfoDTO> CheckAndGetUserAsync(
        Guid userId,
        IEnumerable<UserRole> allowedRoles,
        params Expression<Func<User, object>>[] includes
    )
    {
        var user = await GetByIdAsync(userId, includes) ?? throw new UserNotFoundException();

        if (!allowedRoles.Contains(user.Role))
        {
            throw new ForbiddenException();
        }

        return user;
    }

    public async Task<UserInfoDTO> VerifyHashedPasswordAsync(UserSignInDTO req)
    {
        var query = _repository.User.GetUserByEmail(
            req.Email.ToLower(System.Globalization.CultureInfo.CurrentCulture)
        );
        var user = await query.FirstOrDefaultAsync() ?? throw new BadRequestException("帳號或密碼錯誤");
        var passwordValid = _passwordHasher.VerifyHashedPassword(user.HashedPassword, req.Password);
        if (!passwordValid)
        {
            throw new BadRequestException("帳號或密碼錯誤");
        }
        return _mapper.Map<UserInfoDTO>(user);
    }

    public async Task AddUserResumeAsync(UserInfoDTO userDTO, ResumeDTO resumeDTO)
    {
        var user =
            await _repository.User.GetByIdAsync(userDTO.Id) ?? throw new UserNotFoundException();

        user.Resumes.Add(_mapper.Map<Resume>(resumeDTO));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> VerifyEmailVerificationCode(UserInfoDTO req, string token)
    {
        var user =
            await _repository.User.GetUserByIdIncludeAll(req.Id).FirstOrDefaultAsync()
            ?? throw new UserNotFoundException();

        if (user.EmailVerifications == null)
        {
            return false;
        }

        var result = user.EmailVerifications.Any(e =>
            e.Code == token && e.ExpiresTime > DateTime.UtcNow
        );

        if (result)
        {
            user.EmailVerifications.ToList()
                .RemoveAll(e => e.Code == token || e.ExpiresTime > DateTime.UtcNow);
        }
        return result;
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(Guid userId)
    {
        var user =
            _repository
                .User.GetAll(a => a.Id == userId)
                .Include(u => u.EmailVerifications)
                .SingleOrDefault() ?? throw new UserNotFoundException();
        Random random = new();
        var length = 6;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string token;

        do
        {
            token = new string(
                Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray()
            );
        } while (!IsEmailVerificationCodeAvailable(user, token));

        user.EmailVerifications ??= [];
        user.EmailVerifications.Add(
            new VerificationCode { Code = token, ExpiresTime = DateTime.UtcNow.AddMinutes(10) }
        );
        await _repository.User.UpdateAsync(user);

        return token;
    }

    public bool VerifyEducationEmail(string email)
    {
        // TODO: need third-party service to handle
        // if (!email.Contains("edu"))
        // {
        //     return false;
        // }

        return true;
    }

    public static bool IsEmailVerificationCodeAvailable(User user, string token)
    {
        if (user.EmailVerifications == null)
        {
            return true;
        }

        return !user.EmailVerifications.Any(e => e.Code == token);
    }

    public async Task SaveUserAvatarAsync(Guid userId, IFormFile image)
    {
        var user =
            await _repository
                .User.GetAll()
                .Where(u => u.Id == userId)
                .Include(u => u.Avatar)
                .FirstOrDefaultAsync() ?? throw new UserNotFoundException();

        var newImage = await CreateImageFromFileAsync(image);

        if (user.Avatar != null)
        {
            var fullpath = Path.Combine(_env.WebRootPath, user.Avatar.Filepath);
            if (File.Exists(fullpath))
            {
                File.Delete(fullpath);
            }
            _imageRepository.Delete(user.Avatar);
        }

        user.Avatar = newImage;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ImageDTO?> GetImageByIdAsync(Guid imageId)
    {
        var image = await _imageRepository.GetByIdAsync(imageId);
        return _mapper.Map<ImageDTO>(image);
    }

    public async Task<List<RecruitmentDTO>> GetFavoriteRecruitmentsAsync(Guid userId)
    {
        var recruitmentIds = await _repository
            .User.GetAll()
            .Where(u => u.Id == userId)
            .Include(a => a.FavoriteRecruitments)
            .SelectMany(u => u.FavoriteRecruitments)
            .Select(r => r.Id)
            .ToListAsync();

        var query = _repository
            .Recruitment.GetIncludeAll()
            .Where(r => recruitmentIds.Contains(r.Id));

        var result = _mapper.Map<List<RecruitmentDTO>>(query);

        var favRecruitmentIds = _repository
            .User.GetAll(u => u.Id == userId)
            .Include(u => u.FavoriteRecruitments)
            .SelectMany(u => u.FavoriteRecruitments.Select(fr => fr.Id));

        result.ForEach(r => r.IsUserFav = favRecruitmentIds.Any(id => id == r.Id));
        result.ForEach(r =>
        {
            r.Areas = [];
            r.Keywords = r.Keywords.Take(5);
        });
        return result;
    }

    public async Task AddFavoriteRecruitmentAsync(Guid userId, List<Guid> recruitmentIds)
    {
        var user =
            await _repository
                .User.GetAll()
                .Include(a => a.FavoriteRecruitments)
                .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new UserNotFoundException();
        var existIds = user.Recruitments?.Select(r => r.Id).ToArray() ?? [];
        recruitmentIds.RemoveAll(existIds.Contains);

        var recruitments =
            await _repository.Recruitment.GetAll(a => recruitmentIds.Contains(a.Id)).ToListAsync()
            ?? throw new NotFoundException($"No Recruitment was found");
        recruitments.ForEach(user.FavoriteRecruitments.Add);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveFavoriteRecruitmentAsync(Guid userId, List<Guid> recruitmentIds)
    {
        var user =
            await _repository
                .User.GetAll()
                .Include(a => a.FavoriteRecruitments)
                .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new UserNotFoundException();

        user.FavoriteRecruitments.Where(r => recruitmentIds.Contains(r.Id))
            .ToList()
            .ForEach(e => user.FavoriteRecruitments.Remove(e));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateUserBackgroundAsync(Guid userId, ImagePostDTO req)
    {
        var user =
            await _userRepository
                .GetAll()
                .Where(u => u.Id == userId)
                .Include(u => u.BackgroundImage)
                .SingleOrDefaultAsync() ?? throw new UserNotFoundException();
        var newImage =
            req.Image != null
                ? await CreateImageFromFileAsync(req.Image)
                : await CreateImageFromUriAsync(req.Uri ?? throw new BadRequestException());
        user.BackgroundImage = newImage;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveUserBackgroundAsync(Guid userId)
    {
        var user =
            await _userRepository
                .GetAll()
                .Where(u => u.Id == userId)
                .Include(u => u.BackgroundImage)
                .SingleOrDefaultAsync() ?? throw new UserNotFoundException();
        if (user.BackgroundImage == null)
        {
            await _unitOfWork.SaveChangesAsync();
            return;
        }
        if (user.BackgroundImage.Filepath == "")
        {
            user.BackgroundImage = null;
            await _unitOfWork.SaveChangesAsync();
            return;
        }

        var imagePath = Path.Combine(env.WebRootPath, user.BackgroundImage.Filepath);
        if (Directory.Exists(imagePath))
        {
            Directory.Delete(imagePath);
        }
        user.BackgroundImage = null;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> CheckUserIsPremium(Guid userId)
    {
        // FIXME: remove the testing value
        return true;

        var user =
            await _userRepository.GetAll(u => u.Id == userId).AsNoTracking().SingleOrDefaultAsync()
            ?? throw new UserNotFoundException();
        return user.Premium;
    }

    public async Task<IEnumerable<Guid>> GetUserFavRecuitmentId(Guid userId)
    {
        var user =
            await _userRepository
                .GetAll(u => u.Id == userId)
                .Include(u => u.FavoriteRecruitments)
                .AsNoTracking()
                .FirstOrDefaultAsync() ?? throw new UserNotFoundException();

        return user.FavoriteRecruitments.Select(u => u.Id);
    }

    private async Task<Image> CreateImageFromFileAsync(IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new ArgumentException("File is empty");
        }
        Helper.CheckImage(file);

        var path = _pathSetting.Image.Avatar;
        var newImage = new Image { Filepath = "", ContentType = file.ContentType };
        await _imageRepository.AddAsync(newImage);

        var fileId = newImage.Id;
        var fileName = fileId.ToString();
        var fullPath = Path.Combine(_env.WebRootPath, path, fileName);
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            file.CopyTo(stream);
        }
        newImage.Filepath = Path.Combine(path, fileName);
        return newImage;
    }

    private async Task<Image> CreateImageFromUriAsync(string uri, string contentType = "image/jpg")
    {
        var newImage = new Image
        {
            Uri = uri,
            Filepath = "",
            ContentType = contentType
        };
        await _imageRepository.AddAsync(newImage);
        return newImage;
    }
}
