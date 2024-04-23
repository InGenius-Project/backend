namespace IngBackendApi.Interfaces.Service;

public interface IGroupMapService
{
    void JoinGroup(Guid groupId, string connectionId);
    void AddGroup(Guid groupId);
    bool IsConnectionInGroup(Guid groupId, string connectionId);
    bool IsGroupExist(Guid groupId);
    void Sync();
}
