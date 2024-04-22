namespace IngBackendApi.Application.Interfaces;

public interface IChatHub
{
    Task SendMessage(string message);
    Task AddGroup(string groupName, bool isPrivate = true);
    Task JoinGroup(Guid groupId);
}
