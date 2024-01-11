using IngBackend.Enum;
using IngBackend.Exceptions;
using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Models.DBEntity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace IngBackend.Services.UserService;

public class UserService : Service<User, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IRepository<User, Guid> _userRepository;

    public UserService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _userRepository = unitOfWork.Repository<User, Guid>();
    }

    /// <summary>
    /// Asynchronously finds the user information that matches the specified email address.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve</param>
    /// <param name="includes">(optional) A list of related properties to retrieve</param>
    /// <returns>A `User` object containing the user information that matches the email address `email`, or null if no matching user exists</returns>
    public async Task<User?> GetUserByEmailAsync(string email, params Expression<Func<User, object>>[] includes)
    {
        var query = _userRepository.GetAll();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.FirstOrDefaultAsync(e => e.Email == email);
    }

    /// <summary>
    /// Asynchronously checks if a user exists with the specified ID and retrieves the user information if found.
    /// </summary>
    /// <param name="userId">The ID of the user to check and retrieve (Guid).</param>
    /// <returns>A `User` object containing the user information if found.</returns>
    /// <exception cref="UserNotFoundException">Throws a `UserNotFoundException` if no user exists with the specified ID.</exception>
    public async Task<User> CheckAndGetUserAsync(Guid userId, params Expression<Func<User, object>>[] includes)
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
    public async Task<User> CheckAndGetUserAsync(Guid userId, UserRole allowedRole, params Expression<Func<User, object>>[] includes)
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
    public async Task<User> CheckAndGetUserAsync(Guid userId, IEnumerable<UserRole> allowedRoles, params Expression<Func<User, object>>[] includes)
    {
        var user = await GetByIdAsync(userId, includes) ?? throw new UserNotFoundException();

        if (!allowedRoles.Contains(user.Role))
        {
            throw new ForbiddenException();
        }

        return user;
    }

    public async Task<IEnumerable<Resume>> GetUserResumes(Guid userId)
    {
        var user = await _userRepository.GetAll().Where(u => u.Id == userId)
             .Include(u => u.Resumes)
                 .ThenInclude(r => r.Areas)
                     .ThenInclude(a => a.TextLayout)
             .Include(u => u.Resumes)
                 .ThenInclude(r => r.Areas)
                     .ThenInclude(a => a.ImageTextLayout)
                     .FirstOrDefaultAsync();

        if (user == null)
        {
            return new List<Resume>() { };
        }

        var resume = user.Resumes;
        return resume;
    }

    public async Task<User> GetUserByIdIncludeAll(Guid userId)
    {
        var user = await _userRepository.GetAll().Where(u => u.Id == userId)
            .Include(u => u.Areas)
            .ThenInclude(a => a.TextLayout)
            .Include(u => u.Areas)
                .ThenInclude(a => a.ImageTextLayout)
            .Include(u => u.Areas)
                .ThenInclude(a => a.ListLayout)
                    .ThenInclude(l => l.Items)
            .Include(u => u.Areas)
                .ThenInclude(a => a.KeyValueListLayout)
                    .ThenInclude(kv => kv.Items)
                    .ThenInclude(kvi => kvi.Key)
            .FirstOrDefaultAsync();


        if (user == null)
        {
            throw new UserNotFoundException();
        }
        return user;
    }
}

