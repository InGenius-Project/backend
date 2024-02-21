using IngBackend.Models.DBEntity;

namespace IngBackend.Interfaces.Repository;

public interface IResumeRepository : IRepository<Resume, Guid>
{
    IQueryable<Resume> GetResumeByIdIncludeAll(Guid id);


}