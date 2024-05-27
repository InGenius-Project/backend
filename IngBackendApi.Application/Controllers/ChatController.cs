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
using MimeKit.Encodings;

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

    [HttpPost("groups/join/{groupId}")]
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
            throw new BadRequestException("使用者已經在群組中");
        }

        var user = await _userRepository.GetByIdAsync(userId) ?? throw new UserNotFoundException();

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
        throw new ForbiddenException("使用者沒有權限加入群組");
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
                .FirstOrDefaultAsync() ?? throw new ChatGroupNotFoundException();

        var inviteUser =
            await _userRepository.GetByIdAsync(userId) ?? throw new UserNotFoundException();

        if (!group.Users.Any(u => u.Id == currentUserId))
        {
            throw new ForbiddenException("使用者不在群組中");
        }

        if (group.Users.Any(u => u.Id == userId))
        {
            throw new BadRequestException("使用者已經在群組中");
        }

        if (group.InvitedUsers.Any(u => u.Id == userId))
        {
            throw new BadRequestException("使用者已經被邀請過");
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
                .FirstOrDefaultAsync() ?? throw new UserNotFoundException();

        return _mapper.Map<IEnumerable<ChatGroupInfoDTO>>(user.ChatRooms);
    }

    [HttpGet("groups/{groupId}")]
    public async Task<ChatGroupDTO> GetChatGroup(Guid groupId)
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;

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
                .FirstOrDefaultAsync() ?? throw new ChatGroupNotFoundException();

        if (
            !IsUserInGroup(userId, groupId)
            && !IsUserInInviteList(userId, groupId)
            && chatGroup.Private
        )
        {
            throw new ForbiddenException("您沒有權限訪問此群組");
        }

        return _mapper.Map<ChatGroupDTO>(chatGroup);
    }

    [HttpDelete("groups/{groupId}")]
    public async Task<ApiResponse> DeleteChatGroup(Guid groupId)
    {
        var currentUserId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user =
            await _userRepository.GetByIdAsync(currentUserId) ?? throw new UserNotFoundException();

        var group =
            await _chatGroupRepository
                .GetAll(g => g.Id == groupId)
                .Include(u => u.Users)
                .Include(u => u.Owner)
                .FirstOrDefaultAsync() ?? throw new ChatGroupNotFoundException();

        if (group.Owner.Id != currentUserId)
        {
            throw new ForbiddenException();
        }

        await _chatGroupRepository.DeleteByIdAsync(group.Id);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse();
    }

    [HttpGet("groups/invited")]
    public async Task<IEnumerable<ChatGroupInfoDTO>> GetInvitedChatGroups()
    {
        var userId = (Guid?)ViewData["UserId"];
        var user =
            await _userRepository
                .GetAll(u => u.Id == userId)
                .Include(u => u.InvitedChatRooms)
                .ThenInclude(c => c.Owner)
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
                .Include(u => u.Owner)
                .Include(g => g.InvitedUsers)
                .ThenInclude(u => u.Avatar)
                .AsNoTracking()
                .FirstOrDefaultAsync() ?? throw new ChatGroupNotFoundException();

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
                .FirstOrDefaultAsync() ?? throw new ChatGroupNotFoundException();

        if (
            !group.Users.Any(u => u.Id == currentUserId)
            && !group.InvitedUsers.Any(u => u.Id == currentUserId)
        )
        {
            throw new ForbiddenException();
        }

        if (!group.InvitedUsers.Any(u => u.Id == userId))
        {
            throw new BadRequestException("使用者未被邀請");
        }

        var invitedUser =
            await _userRepository.GetByIdAsync(userId)
            ?? throw new ChatGroupInvitationNotFoundException();

        group.InvitedUsers.Remove(invitedUser);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse("ok");
    }

    [AllowAnonymous]
    [HttpGet("groups/public")]
    public async Task<IEnumerable<ChatGroupInfoDTO>> GetPublicGroups()
    {
        var groups = await _chatGroupRepository
            .GetAll(c => !c.Private)
            .Include(c => c.Users)
            .ThenInclude(u => u.Avatar)
            .Include(c => c.Owner)
            .ThenInclude(o => o.Avatar)
            .Include(c => c.Messages)
            .AsNoTracking()
            .ToArrayAsync();
        return _mapper.Map<IEnumerable<ChatGroupInfoDTO>>(groups);
    }

    [HttpGet("groups/private")]
    public async Task<IEnumerable<ChatGroupInfoDTO>> GetPrivateGroups()
    {
        var userId = (Guid?)ViewData["UserId"] ?? Guid.Empty;
        var user =
            await _userRepository
                .GetAll()
                .Where(u => u.Id == userId)
                .Include(u => u.ChatRooms)
                .ThenInclude(c => c.Users)
                .ThenInclude(u => u.Avatar)
                .Include(u => u.ChatRooms)
                .ThenInclude(c => c.Owner)
                .ThenInclude(o => o.Avatar)
                .Include(u => u.ChatRooms)
                .ThenInclude(c => c.Messages)
                .FirstOrDefaultAsync() ?? throw new UserNotFoundException();

        return _mapper.Map<IEnumerable<ChatGroupInfoDTO>>(user.ChatRooms.Where(c => c.Private));
    }

    private bool IsUserInGroup(Guid userId, Guid groupId)
    {
        var chatGroup =
            _chatGroupRepository
                .GetAll(g => g.Id == groupId)
                .Include(g => g.Users)
                .AsNoTracking()
                .FirstOrDefault() ?? throw new ChatGroupNotFoundException();
        return chatGroup.Users.Any(u => u.Id == userId);
    }

    private bool IsUserInInviteList(Guid userId, Guid groupId)
    {
        var chatGroup =
            _chatGroupRepository
                .GetAll(g => g.Id == groupId)
                .Include(g => g.InvitedUsers)
                .AsNoTracking()
                .FirstOrDefault() ?? throw new ChatGroupNotFoundException();
        return chatGroup.InvitedUsers.Any(u => u.Id == userId);
    }
}
