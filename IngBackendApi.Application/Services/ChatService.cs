namespace IngBackendApi.Services;

using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;

// NOTE: not Implemented
public class ChatService(IUnitOfWork unitOfWork) : IChatService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IRepository<User, Guid> _userRepository = unitOfWork.Repository<User, Guid>();
    private readonly IRepository<ChatGroup, Guid> _chatGroupRepository = unitOfWork.Repository<
        ChatGroup,
        Guid
    >();

    public async Task JoinGroup(Guid groupId) { }

    public async Task AddGroup(string groupName)
    {
        throw new NotImplementedException();
    }
}
