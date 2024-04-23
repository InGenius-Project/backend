namespace IngBackendApi.Application.Hubs;

using System.Security.Claims;
using AutoMapper;
using IngBackendApi.Application.Attribute;
using IngBackendApi.Application.Interfaces;
using IngBackendApi.Enum;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class ChatHub(
    IHttpContextAccessor httpContextAccessor,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IGroupMapService groupMapService
) : Hub, IChatHub
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IRepository<User, Guid> _userRepository = unitOfWork.Repository<User, Guid>();
    private readonly IRepository<ChatGroup, Guid> _chatGroupRepository = unitOfWork.Repository<
        ChatGroup,
        Guid
    >();
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IGroupMapService _groupMapService = groupMapService;

    // send message to all users
    [UserAuthorize(UserRole.Admin, UserRole.InternalUser)]
    public async Task SendMessage(string message) => await Clients.All.SendAsync(message);

    // user send message to group
    public async Task SendMessageToGroup(string message, Guid groupId)
    {
        var userId = GetUserId();
        if (!CheckIfUserInGroup(userId, groupId))
        {
            throw new ForbiddenException("User not in group");
        }
        // check connection id if in group
        await SendToGroup(ChatReceiveMethod.Message, message, groupId);
    }

    // Add new chat room
    public async Task AddGroup(string groupName, bool isPrivate = true)
    {
        var userId = GetUserId();
        var user =
            await _userRepository
                .GetAll()
                .Include(x => x.ChatRooms)
                .FirstOrDefaultAsync(x => x.Id == userId)
            ?? throw new NotFoundException("User not found");

        // create new group
        var newGroup = new ChatGroup()
        {
            GroupName = groupName,
            OwnerId = userId,
            Private = isPrivate
        };
        user.ChatRooms.Add(newGroup);
        await _unitOfWork.SaveChangesAsync();

        var chatGroupDTO = _mapper.Map<ChatGroupInfoDTO>(newGroup);

        // add to signalR group
        await Groups.AddToGroupAsync(Context.ConnectionId, newGroup.Id.ToString());
        _groupMapService.AddGroup(newGroup.Id);
        _groupMapService.JoinGroup(newGroup.Id, Context.ConnectionId);

        await SendToGroup(ChatReceiveMethod.Message, "New Group Created", newGroup.Id);
        await SendToCaller(ChatReceiveMethod.NewGroup, chatGroupDTO);
    }

    // user join chat room
    public async Task JoinGroup(Guid groupId)
    {
        var userId = GetUserId();

        // User already in group
        if (CheckIfUserInGroup(userId, groupId))
        {
            throw new BadRequestException("User already in group");
        }

        // User not in invite List
        if (!IsUserAbleToJoinGroup(userId, groupId))
        {
            throw new ForbiddenException("User not able to join the group");
        }

        // Add to ConnectionId To Group
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
        _groupMapService.JoinGroup(groupId, Context.ConnectionId);

        // send message to group
        await SendToGroup(ChatReceiveMethod.Message, $"User {userId} Joined.", groupId);

        //  Add to DB Group
        await AddUserToDBGroup(userId, groupId);
    }

    private async Task SendToCaller(ChatReceiveMethod method, object obj) =>
        await Clients.Caller.SendAsync(method.ToString(), obj);

    private async Task SendToGroup(ChatReceiveMethod method, object obj, Guid groupId) =>
        await Clients.Group(groupId.ToString()).SendAsync(method.ToString(), obj);

    private async Task SendToAll(ChatReceiveMethod method, object obj) =>
        await Clients.All.SendAsync(method.ToString(), obj);

    private bool IsUserAbleToJoinGroup(Guid userId, Guid groupId)
    {
        var chatGroup = _chatGroupRepository
            .GetAll(g => g.Id == groupId)
            .Include(x => x.InvitedUsers)
            .AsNoTracking()
            .FirstOrDefault();
        if (chatGroup == null)
        {
            return false;
        }
        if (!chatGroup.Private)
        {
            return true;
        }
        return chatGroup.InvitedUsers.Any(u => u.Id == userId);
    }

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

    private bool CheckIfUserInGroup(Guid userId, Guid groupId)
    {
        if (!_groupMapService.IsGroupExist(groupId))
        {
            throw new NotFoundException("Group not found");
        }

        if (_groupMapService.IsConnectionInGroup(groupId, Context.ConnectionId))
        {
            return true;
        }

        var checkIfUserInGroup = _chatGroupRepository
            .GetAll(g => g.Id == groupId)
            .Include(x => x.Users)
            .ToArray()
            .Any(u => u.Id == userId);
        if (checkIfUserInGroup)
        {
            // add to map cache
            _groupMapService.JoinGroup(groupId, Context.ConnectionId);
        }
        return checkIfUserInGroup;
    }

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
