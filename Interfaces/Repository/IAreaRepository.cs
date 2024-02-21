using IngBackend.Models.DBEntity;

namespace IngBackend.Interfaces.Repository;

public interface IAreaRepository : IRepository<Area, Guid>
{
    IQueryable<Area> GetAreaByIdIncludeAll(Guid id);


}