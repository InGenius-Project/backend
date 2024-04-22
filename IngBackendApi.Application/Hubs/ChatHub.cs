namespace IngBackendApi.Application.Hubs;

using System.Security.Claims;
using IngBackendApi.Application.Interfaces;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using Microsoft.AspNetCore.Authorization;
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
        var groupName = groupId.ToString();
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients
            .Group(groupName)
            .ReceiveMessage($"{Context.ConnectionId} has joined the group {groupName}.");
    }

    public bool CheckIfGroupExist(Guid groupId) => _groupConnectionMap.ContainsKey(groupId);
}
