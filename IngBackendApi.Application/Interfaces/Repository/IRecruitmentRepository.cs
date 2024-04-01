using IngBackendApi.Models.DBEntity;

namespace IngBackendApi.Interfaces.Repository;

public interface IRecruitmentRepository : IRepository<Recruitment, Guid>
{
    IQueryable<Recruitment> GetRecruitmentByIdIncludeAll(Guid id);

    IQueryable<Recruitment> GetIncludeAll();
}