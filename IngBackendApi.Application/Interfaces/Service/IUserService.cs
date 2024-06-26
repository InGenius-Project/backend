namespace IngBackendApi.Interfaces.Service;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IngBackendApi.Enum;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;

public interface IUserService : IService<User, UserInfoDTO, Guid>
{
    Task<UserInfoDTO?> GetUserByIdIncludeAllAsync(Guid userId);
    Task PostUser(UserInfoPostDTO req, Guid userId);
    Task<UserInfoDTO> AddAsync(UserSignUpDTO req);
    Task<UserInfoDTO?> GetUserByEmailAsync(
        string email,
        params Expression<Func<User, object>>[] includes
    );
    Task<UserInfoDTO> CheckAndGetUserIncludeAllAsync(Guid userId);
    Task<UserInfoDTO> CheckAndGetUserAsync(
        Guid userId,
        params Expression<Func<User, object>>[] includes
    );
    Task<UserInfoDTO> CheckAndGetUserAsync(
        Guid userId,
        UserRole allowedRole,
        params Expression<Func<User, object>>[] includes
    );
    Task<UserInfoDTO> CheckAndGetUserAsync(
        Guid userId,
        IEnumerable<UserRole> allowedRoles,
        params Expression<Func<User, object>>[] includes
    );
    Task<UserInfoDTO> VerifyHashedPasswordAsync(UserSignInDTO req);
    Task AddUserResumeAsync(UserInfoDTO userDTO, ResumeDTO resumeDTO);
    Task<bool> VerifyEmailVerificationCode(UserInfoDTO req, string token);
    Task<string> GenerateEmailConfirmationTokenAsync(Guid userId);
    bool VerifyEducationEmail(string email);
    Task SaveUserAvatarAsync(Guid userId, IFormFile image);
    Task<ImageDTO?> GetImageByIdAsync(Guid imageId);
    Task AddFavoriteRecruitmentAsync(Guid userId, List<Guid> recruitmentIds);
    Task RemoveFavoriteRecruitmentAsync(Guid userId, List<Guid> recruitmentIds);
    Task<List<RecruitmentDTO>> GetFavoriteRecruitmentsAsync(Guid userId);
    Task UpdateUserBackgroundAsync(Guid userId, ImagePostDTO req);
    Task RemoveUserBackgroundAsync(Guid userId);
    Task<bool> CheckUserIsPremium(Guid userId);
    Task<IEnumerable<Guid>> GetUserFavRecuitmentId(Guid userId);
}
