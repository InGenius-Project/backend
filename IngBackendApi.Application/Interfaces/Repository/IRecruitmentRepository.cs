namespace IngBackendApi.Interfaces.Repository;

using IngBackendApi.Models.DBEntity;

public interface IRecruitmentRepository : IRepository<Recruitment, Guid>
{
    IQueryable<Recruitment> GetIncludeAll();
}
