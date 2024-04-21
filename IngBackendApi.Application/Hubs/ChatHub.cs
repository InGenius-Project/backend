namespace IngBackendApi.Application.Hubs;

using System.Security.Claims;
using IngBackendApi.Application.Interfaces;
using IngBackendApi.Context;
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
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task SendMessage(string message) => await Clients.All.ReceiveMessage(message);

    public async Task SendMessageToCaller(string message) =>
        await Clients.Caller.ReceiveMessage(message);

    public async Task SendMessageToGroup(string groupName, string message) =>
        await Clients.Group(groupName).ReceiveMessage(message);

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
                .Include(u => u.Connections)
                .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new UserNotFoundException();

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        user.Connections.Add(
            new Connection
            {
                Id = Context.ConnectionId,
                GroupName = groupName,
                Connected = true
            }
        );

        await _unitOfWork.SaveChangesAsync();
        await Clients.Group(groupName).ReceiveMessage(message);
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients
            .Group(groupName)
            .ReceiveMessage($"{Context.ConnectionId} has joined the group {groupName}.");
    }
}
