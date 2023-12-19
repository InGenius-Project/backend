﻿using IngBackend.Enum;
using IngBackend.Exceptions;
using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Models.DBEntity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Web.Http.Results;


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

    public async Task<User>? GetUserByEmailAsync(string email, params Expression<Func<User, object>>[] includes)
    {
        var query = _userRepository.GetAll();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.FirstOrDefaultAsync(e => e.Email == email);
    }

    public async Task<User>? CheckAndGetUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId) ?? throw new UserNotFoundException();
        return user ;
    }

    public async Task<User?> CheckAndGetUserAsync(Guid userId, UserRole allowedRole)
    {
        var user = await _userRepository.GetByIdAsync(userId) ?? throw new UserNotFoundException();
        
        if (user.Role != allowedRole) 
        {
            throw new ForbiddenException();
        }

        return user;
    }

    public async Task<User?> CheckAndGetUserAsync(Guid userId, IEnumerable<UserRole> allowedRoles)
    {
        var user = await _userRepository.GetByIdAsync(userId) ?? throw new UserNotFoundException();

        if (!allowedRoles.Contains(user.Role))
        {
            throw new ForbiddenException();
        }

        return user;
    }


}
