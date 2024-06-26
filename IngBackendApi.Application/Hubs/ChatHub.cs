﻿namespace IngBackendApi.Application.Hubs;

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
public class ChatHub : Hub, IChatHub
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IRepository<User, Guid> _userRepository;
    private readonly IRepository<ChatGroup, Guid> _chatGroupRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IGroupMapService _groupMapService;

    public ChatHub(
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IGroupMapService groupMapService
    )
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userRepository = unitOfWork.Repository<User, Guid>();
        _chatGroupRepository = unitOfWork.Repository<ChatGroup, Guid>();
        _httpContextAccessor = httpContextAccessor;
        _groupMapService = groupMapService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var user =
            _userRepository
            .GetAll()
            .Include(x => x.ChatRooms)
                .ThenInclude(c => c.Messages)
            .SingleOrDefault(x => x.Id == userId)
            ?? throw new NotFoundException("User not found");
        user.ChatRooms.ToList()
            .ForEach(async g =>
                await Groups.AddToGroupAsync(Context.ConnectionId, g.Id.ToString())
            );

        foreach (var chatRoom in user.ChatRooms)
        {
            var lastMessage = chatRoom.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
            if (lastMessage != null)
            {
                var messageDTO = new ChatMessageDTO()
                {
                    Message = lastMessage.Message,
                    GroupId = lastMessage.GroupId,
                    SenderId = userId,
                    Sender = _mapper.Map<OwnerUserDTO>(user),
                    SendTime = lastMessage.CreatedAt,
                };

                await SendToCaller(ChatReceiveMethod.LastMessage, messageDTO);
            }
        }

        await base.OnConnectedAsync();
    }

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
        var user =
            await _userRepository
                .GetAll(u => u.Id == userId)
                .Include(u => u.Avatar)
                .AsNoTracking()
                .FirstOrDefaultAsync() ?? throw new UserNotFoundException();

        var messageDTO = new ChatMessageDTO()
        {
            Message = message,
            GroupId = groupId,
            SenderId = userId,
            Sender = _mapper.Map<OwnerUserDTO>(user),
            SendTime = DateTime.UtcNow,
        };

        // check connection id if in group
        await SendToGroup(ChatReceiveMethod.Message, messageDTO, groupId);
        await PushMessageToDBAsync(groupId, userId, message);
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
            Private = isPrivate,
            Users = [user]
        };
        user.ChatRooms.Add(newGroup);
        await _unitOfWork.SaveChangesAsync();

        var chatGroupDTO = _mapper.Map<ChatGroupInfoDTO>(newGroup);

        // add to signalR group
        await Groups.AddToGroupAsync(Context.ConnectionId, newGroup.Id.ToString());
        _groupMapService.AddGroup(newGroup.Id);
        _groupMapService.JoinGroup(newGroup.Id, Context.ConnectionId);

        await SendToGroup(ChatReceiveMethod.BroadCast, "New Group Created", newGroup.Id);
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

        await SendToCaller(ChatReceiveMethod.BroadCast, $"User {userId} Joined.");

        // send message to group
        await SendToGroup(ChatReceiveMethod.BroadCast, $"User {userId} Joined.", groupId);

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

        var group =
            _chatGroupRepository.GetAll(g => g.Id == groupId).Include(x => x.Users).FirstOrDefault()
            ?? throw new NotFoundException("group not found");
        var checkIfUserInGroup = group.Users.Any(u => u.Id == userId);
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

    private async Task PushMessageToDBAsync(Guid groupId, Guid userId, string message)
    {
        var group =
            await _chatGroupRepository
                .GetAll(g => g.Id == groupId)
                .Include(c => c.Messages)
                .SingleOrDefaultAsync() ?? throw new NotFoundException("Group not found");

        var user = await _userRepository.GetByIdAsync(userId) ?? throw new UserNotFoundException();
        group.Messages.Add(
            new ChatMessage()
            {
                Message = message,
                GroupId = groupId,
                SenderId = userId
            }
        );
        await _unitOfWork.SaveChangesAsync();
    }
}
