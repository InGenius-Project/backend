using IngBackend.Models.DBEntity;

namespace IngBackend.Interfaces.Repository;

public interface IRecruitmentRepository : IRepository<Recruitment, Guid>
{
    IQueryable<Recruitment> GetRecruitmentByIdIncludeAll(Guid id);


}