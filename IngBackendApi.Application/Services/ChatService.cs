namespace IngBackendApi.Services;

using System.Data.Entity;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;

// NOTE: not Implemented
public class GroupMapService : IGroupMapService
{
    private readonly Dictionary<Guid, List<string>> _groupConnectionMap = [];
    private readonly IServiceProvider _services;

    public GroupMapService(IServiceProvider services)
    {
        _services = services;
        Sync();
    }

    public void JoinGroup(Guid groupId, string connectionId) =>
        _groupConnectionMap[groupId].Add(connectionId);

    public void AddGroup(Guid groupId) => _groupConnectionMap.Add(groupId, []);

    public bool IsConnectionInGroup(Guid groupId, string connectionId) =>
        _groupConnectionMap[groupId].Contains(connectionId);

    public bool IsGroupExist(Guid groupId) => _groupConnectionMap.ContainsKey(groupId);

    public void Sync()
    {
        using var scope = _services.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        _groupConnectionMap.Clear();
        unitOfWork
            .Repository<ChatGroup, Guid>()
            .GetAll()
            .AsNoTracking()
            .ToList()
            .ForEach(c => _groupConnectionMap.Add(c.Id, []));
    }
}
