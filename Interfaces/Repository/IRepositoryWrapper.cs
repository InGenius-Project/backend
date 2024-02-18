namespace IngBackend.Interfaces.Repository;

public interface IRepositoryWrapper
{
    IUserRepository User { get; }
    void Save();
}