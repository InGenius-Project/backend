using IngBackendApi.Models.DBEntity;

namespace IngBackendApi.Interfaces.Repository;

public interface IAreaRepository : IRepository<Area, Guid>
{
    IQueryable<Area> GetAreaByIdIncludeAll(Guid id);


}