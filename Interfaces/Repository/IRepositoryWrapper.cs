namespace IngBackend.Interfaces.Repository;

public interface IRepositoryWrapper
{
    IUserRepository User { get; }
    IAreaRepository Area { get; }
    IResumeRepository Resume { get; }
    IRecruitmentRepository Recruitment { get; }
    void Save();
}