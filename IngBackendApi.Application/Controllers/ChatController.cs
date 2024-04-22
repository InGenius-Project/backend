namespace IngBackendApi.Controllers;

using System.Data.Entity;
using System.Text.RegularExpressions;
using AutoWrapper.Wrappers;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class ChatController(IUnitOfWork unitOfWork) : BaseController
{
    // TODO: Change the Repository usage to service
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IRepository<ChatGroup, Guid> _chatGroupRepository = unitOfWork.Repository<
        ChatGroup,
        Guid
    >();
    private readonly IRepository<User, Guid> _userRepository = unitOfWork.Repository<User, Guid>();

    [HttpGet("group/join/{groupId}")]
    public async Task<ApiResponse> JoinGroup(Guid groupId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var chatGroup =
            await _chatGroupRepository
                .GetAll(g => g.Id == groupId)
                .Include(g => g.InvitedUsers)
                .Include(g => g.Users)
                .FirstOrDefaultAsync() ?? throw new NotFoundException("Group not found");

        if (chatGroup.Users.Any(u => u.Id == userId))
        {
            throw new BadRequestException("User already in group");
        }

        var user =
            await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        if (!chatGroup.Private)
        {
            chatGroup.Users.Add(user);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse("ok");
        }

        if (chatGroup.InvitedUsers.Any(u => u.Id == userId))
        {
            chatGroup.InvitedUsers.Remove(user);
            chatGroup.Users.Add(user);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse("ok");
        }
        throw new ForbiddenException("User not in invite list");
    }

    [HttpGet("group/invite/{groupId}/{userId}")]
    public async Task<ApiResponse> InviteUserToGroup(Guid groupId, Guid userId)
    {
        var currentUserId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var group =
            await _chatGroupRepository
                .GetAll(g => g.Id == groupId)
                .Include(g => g.Owner)
                .Include(g => g.InvitedUsers)
                .Include(g => g.Users)
                .FirstOrDefaultAsync() ?? throw new NotFoundException("Group not found");

        var inviteUser =
            await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        if (!group.Users.Any(u => u.Id == currentUserId))
        {
            throw new ForbiddenException("User no invite permission.");
        }

        if (group.Users.Any(u => u.Id == userId))
        {
            throw new BadRequestException("User already in group.");
        }

        if (group.InvitedUsers.Any(u => u.Id == userId))
        {
            throw new BadRequestException("User already in invite list.");
        }

        group.InvitedUsers.Add(inviteUser);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse("ok");
    }

    private bool IsUserInGroup(Guid userId, Guid groupId)
    {
        var chatGroup =
            _chatGroupRepository
                .GetAll(g => g.Id == groupId)
                .Include(g => g.Users)
                .AsNoTracking()
                .FirstOrDefault() ?? throw new NotFoundException("Group not found");
        return chatGroup.Users.Any(u => u.Id == userId);
    }
}
