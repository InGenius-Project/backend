namespace IngBackend.Interfaces.Repository;
using IngBackend.Models.DBEntity;

public interface IRepositoryWrapper
{
    IUserRepository User { get; }
    IAreaRepository Area { get; }
    IResumeRepository Resume { get; }
    ITagRepository Tag { get; }
    IRecruitmentRepository Recruitment { get; }
    IRepository<AreaType, int> AreaType { get; }
    IRepository<TagType, int> TagType { get; }
    void Save();
}