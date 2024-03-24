using IngBackendApi.Models.DBEntity;

namespace IngBackendApi.Interfaces.Repository;

public interface IRepositoryWrapper
{
    IUserRepository User { get; }
    IAreaRepository Area { get; }
    IResumeRepository Resume { get; }
    IRecruitmentRepository Recruitment { get; }
    IRepository<AreaType, int> AreaType { get; }
    IRepository<TagType, int> TagType { get; }
    void Save();
}

