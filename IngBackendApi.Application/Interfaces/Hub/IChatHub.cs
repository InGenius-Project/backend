namespace IngBackendApi.Application.Interfaces;

public interface IChatHub
{
    public Task AddGroup(string groupName, string user);
    public Task ReceiveMessage(string message);
}
