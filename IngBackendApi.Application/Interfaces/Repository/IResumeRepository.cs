using IngBackendApi.Models.DBEntity;

namespace IngBackendApi.Interfaces.Repository;

public interface IResumeRepository : IRepository<Resume, Guid>
{
    IQueryable<Resume> GetIncludeAll();
}
