namespace IngBackend.Interfaces.Repository;

using IngBackend.Models.DBEntity;

public interface IAreaRepository : IRepository<Area, Guid>
{
    IQueryable<Area> GetAreaByIdIncludeAll(Guid id);

    IQueryable<Area> GetAreaByIdIncludeUser(Guid id);

    IQueryable<AreaType> GetAreaTypeByIdIncludeAll(int id);

    Task PostArea(Area postArea, Guid userId);


}