namespace IngBackendApi.Interfaces.Service;

public interface IChatService
{
    Task JoinGroup(Guid groupId);
    Task AddGroup(string groupName);
}
