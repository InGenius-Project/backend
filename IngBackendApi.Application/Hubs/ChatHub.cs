namespace IngBackendApi.Application.Hubs;

using System.Security.Claims;
using IngBackendApi.Application.Interfaces;
using IngBackendApi.Context;
using IngBackendApi.Models.DBEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class ChatHub(IngDbContext context, IHttpContextAccessor httpContextAccessor) : Hub<IChatHub>
{
    private readonly IngDbContext _context = context;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    // public async Task ReceiveMessage(string message)
    //     => await Clients.All.

    public async Task SendMessage(string message)
        => await Clients.All.ReceiveMessage(message);

    public async Task SendMessageToCaller(string message)
        => await Clients.Caller.ReceiveMessage(message);

    public async Task SendMessageToGroup(string groupName, string message)
        => await Clients.Group(groupName).ReceiveMessage(message);

    public async Task AddGroup(string message, string groupName)
    {
        var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
        {

            var user = await _context.User
                .Include(u => u.Connections)
                .FirstOrDefaultAsync(u => u.Id == userId);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            user.Connections.Add(new Connection
            {
                ConnectionId = Context.ConnectionId,
                GroupName = groupName,
                Connected = true
            });

            await _context.SaveChangesAsync();
            await Clients.Group(groupName).ReceiveMessage(message);
        }
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).ReceiveMessage($"{Context.ConnectionId} has joined the group {groupName}.");
    }


}
