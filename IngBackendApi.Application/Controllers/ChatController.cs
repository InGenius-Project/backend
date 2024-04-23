namespace IngBackendApi.Controllers;

using AutoMapper;
using AutoWrapper.Wrappers;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class ChatController(IUnitOfWork unitOfWork, IMapper mapper) : BaseController
{
    // TODO: Change the Repository usage to service
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IRepository<ChatGroup, Guid> _chatGroupRepository = unitOfWork.Repository<
        ChatGroup,
        Guid
    >();

    private readonly IRepository<User, Guid> _userRepository = unitOfWork.Repository<User, Guid>();

    [HttpGet("groups/join/{groupId}")]
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

    [HttpGet("groups/invite/{groupId}/{userId}")]
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

    [HttpGet("groups")]
    public async Task<IEnumerable<ChatGroupInfoDTO>> GetChatGroups()
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user =
            await _userRepository
                .GetAll(u => u.Id == userId)
                .Include(g => g.ChatRooms)
                .ThenInclude(c => c.Owner.Avatar)
                .Include(g => g.ChatRooms)
                .ThenInclude(c => c.Users)
                .ThenInclude(u => u.Avatar)
                .FirstOrDefaultAsync() ?? throw new NotFoundException("User not found");

        return _mapper.Map<IEnumerable<ChatGroupInfoDTO>>(user.ChatRooms);
    }

    [HttpGet("groups/{groupId}")]
    public async Task<ChatGroupDTO> GetChatGroup(Guid groupId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        if (!IsUserInGroup(userId, groupId))
        {
            throw new ForbiddenException("User not in group");
        }
        var chatGroup =
            await _chatGroupRepository
                .GetAll(g => g.Id == groupId)
                .Include(g => g.Owner)
                .ThenInclude(u => u.Avatar)
                .Include(g => g.Users)
                .ThenInclude(u => u.Avatar)
                .Include(g => g.Messages)
                .ThenInclude(m => m.Sender.Avatar)
                .AsNoTracking()
                .FirstOrDefaultAsync() ?? throw new NotFoundException("Group not found");

        return _mapper.Map<ChatGroupDTO>(chatGroup);
    }

    [HttpGet("groups/invited")]
    public async Task<IEnumerable<ChatGroupInfoDTO>> GetInvitedChatGroups()
    {
        var userId = (Guid?)ViewData["UserId"];
        var user =
            await _userRepository
                .GetAll(u => u.Id == userId)
                .Include(u => u.InvitedChatRooms)
                .AsNoTracking()
                .FirstOrDefaultAsync() ?? throw new UserNotFoundException();

        return _mapper.Map<IEnumerable<ChatGroupInfoDTO>>(user.InvitedChatRooms);
    }

    [HttpGet("groups/{groupId}/invitations/users")]
    public async Task<IEnumerable<OwnerUserDTO>> GetInvitedUsers(Guid groupId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user = await _userRepository.GetByIdAsync(userId) ?? throw new UserNotFoundException();

        var group =
            await _chatGroupRepository
                .GetAll(g => g.Id == groupId)
                .Include(u => u.Users)
                .Include(g => g.InvitedUsers)
                .ThenInclude(u => u.Avatar)
                .AsNoTracking()
                .FirstOrDefaultAsync() ?? throw new NotFoundException("group not found");

        if (!group.Users.Any(u => u.Id == userId))
        {
            throw new ForbiddenException();
        }

        return _mapper.Map<IEnumerable<OwnerUserDTO>>(group.InvitedUsers);
    }

    [HttpDelete("groups/{groupId}/users/{userId}/invitations")]
    public async Task<ApiResponse> CancelInvitation(Guid groupId, Guid userId)
    {
        var currentUserId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user =
            await _userRepository.GetByIdAsync(currentUserId) ?? throw new UserNotFoundException();

        var group =
            await _chatGroupRepository
                .GetAll(g => g.Id == groupId)
                .Include(u => u.InvitedUsers)
                .Include(u => u.Users)
                .FirstOrDefaultAsync() ?? throw new NotFoundException("group not found");

        if (!group.Users.Any(u => u.Id == currentUserId))
        {
            throw new ForbiddenException();
        }

        if (!group.InvitedUsers.Any(u => u.Id == userId))
        {
            throw new BadRequestException("User has not been invited");
        }

        var invitedUser =
            await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("The user invitation to deleted not found");

        group.InvitedUsers.Remove(invitedUser);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse("ok");
    }

    [AllowAnonymous]
    [HttpGet("groups/public")]
    public async Task<IEnumerable<ChatGroupInfoDTO>> GetPublicGroup()
    {
        var groups = await _chatGroupRepository
            .GetAll(c => !c.Private)
            .Include(c => c.Users)
            .ThenInclude(u => u.Avatar)
            .Include(c => c.Owner)
            .ThenInclude(o => o.Avatar)
            .AsNoTracking()
            .ToArrayAsync();
        return _mapper.Map<IEnumerable<ChatGroupInfoDTO>>(groups);
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
