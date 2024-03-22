namespace IngBackend.Interfaces.Service;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IngBackend.Enum;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;

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
    Task<ResumeDTO?> GetResumesByUserId(Guid userId);
    Task<UserInfoDTO> VerifyHashedPasswordAsync(UserSignInDTO req);
    Task AddUserResumeAsync(UserInfoDTO userDTO, ResumeDTO resumeDTO);
    Task<bool> VerifyEmailVerificationCode(UserInfoDTO req, string token);
    Task<string> GenerateEmailConfirmationTokenAsync(UserInfoDTO req);
    bool VerifyEducationEmail(string email);
}
