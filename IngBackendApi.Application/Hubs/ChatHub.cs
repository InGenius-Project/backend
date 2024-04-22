namespace IngBackendApi.Application.Hubs;

using System.Security.Claims;
using IngBackendApi.Application.Interfaces;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class ChatHub(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    : Hub<IChatHub>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IRepository<User, Guid> _userRepository = unitOfWork.Repository<User, Guid>();
    private readonly IRepository<ChatGroup, Guid> _chatGroupRepository = unitOfWork.Repository<
        ChatGroup,
        Guid
    >();
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    // Initialize GroupName and ConnectionIds
    private readonly Dictionary<Guid, List<string>> _groupConnectionMap = unitOfWork
        .Repository<ChatGroup, Guid>()
        .GetAll()
        .ToDictionary(x => x.Id, x => new List<string>());

    // send message to all users
    public async Task SendMessage(string message) => await Clients.All.ReceiveMessage(message);

    public async Task SendMessageToCaller(string message) =>
        await Clients.Caller.ReceiveMessage(message);

    // user send message to group
    public async Task SendMessageToGroup(string message, Guid groupId)
    {
        var group = Clients.Group(groupId.ToString());
        if (group == null)
        {
            throw new NotFoundException("Group not found");
        }
        // check connection id if in group
        await Clients.Group(groupId.ToString()).ReceiveMessage(message);
    }

    // Add new chat room
    public async Task AddGroup(string message, string groupName)
    {
        var userId = GetUserId();
        var user =
            await _userRepository
                .GetAll()
                .Include(x => x.ChatRooms)
                .FirstOrDefaultAsync(x => x.Id == userId)
            ?? throw new NotFoundException("User not found");

        // create new group
        var groupId = Guid.NewGuid();
        var newGroup = new ChatGroup()
        {
            Id = groupId,
            ChatRoomName = groupName,
            OwnerId = userId
        };
        user.ChatRooms.Add(newGroup);
        await _unitOfWork.SaveChangesAsync();

        // add to signalR group
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());

        await Clients.Group(groupId.ToString()).ReceiveMessage(message);
        await Clients.Caller.ReceiveMessage(message);
    }

    // user join chat room
    public async Task JoinGroup(Guid groupId)
    {
        var userId = GetUserId();

        // group not exist
        if (!CheckIfGroupExist(groupId))
        {
            throw new NotFoundException("Group not found");
        }

        // User already in group
        if (IsUserInGroup(userId, groupId))
        {
            throw new BadRequestException("User already in group");
        }

        // User not in invite List
        if (!IsUserWasInviteList(userId, groupId))
        {
            throw new ForbiddenException("User not in invite list");
        }

        // Add to ConnectionId To Group
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
        _groupConnectionMap[groupId].Add(Context.ConnectionId);

        // send message to group
        await Clients
            .Group(groupId.ToString())
            .ReceiveMessage($"{Context.ConnectionId} has joined the group {groupId}.");

        //  Add to DB Group
        await AddUserToDBGroup(userId, groupId);
    }

    private bool IsUserWasInviteList(Guid userId, Guid groupId) =>
        _chatGroupRepository
            .GetAll(g => g.Id == groupId)
            .Include(x => x.InvitedUsers)
            .AsNoTracking()
            .ToArray()
            .Any(u => u.Id == userId);

    private async Task AddUserToDBGroup(Guid userId, Guid groupId)
    {
        var group =
            await _chatGroupRepository
                .GetAll(g => g.Id == groupId)
                .Include(x => x.Users)
                .Include(x => x.InvitedUsers)
                .FirstOrDefaultAsync() ?? throw new NotFoundException("Group not found");

        //  user in group
        if (group.Users.Any(u => u.Id == userId))
        {
            throw new BadRequestException("User already in group");
        }

        // user not in invite list
        if (!group.InvitedUsers.Any(u => u.Id == userId))
        {
            throw new ForbiddenException("User not in invite list");
        }

        var user =
            await _userRepository.GetAll().FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException("User not found");

        // remove invited user
        group.InvitedUsers.Remove(group.InvitedUsers.First(u => u.Id == userId));
        // add user to group
        group.Users.Add(user);
        await _unitOfWork.SaveChangesAsync();
    }

    private bool IsUserInGroup(Guid userId, Guid groupId)
    {
        if (!_groupConnectionMap.TryGetValue(groupId, out var connectionList))
        {
            throw new NotFoundException("Group not found");
        }

        if (connectionList.Contains(Context.ConnectionId))
        {
            return true;
        }

        var isUserInGroup = _chatGroupRepository
            .GetAll(g => g.Id == groupId)
            .Include(x => x.Users)
            .ToArray()
            .Any(u => u.Id == userId);
        if (isUserInGroup)
        {
            // add to map cache
            _groupConnectionMap[groupId].Add(Context.ConnectionId);
        }
        return isUserInGroup;
    }

    private bool CheckIfGroupExist(Guid groupId) => _groupConnectionMap.ContainsKey(groupId);

    private Guid GetUserId()
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new UnauthorizedAccessException();
        }

        var userIdClaim =
            _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException();
        }
        return userId;
    }
}
