using IngBackend.Models.DBEntity;

namespace IngBackend.Interfaces.Repository;

public interface IUserRepository : IRepository<User, Guid>
{
    IQueryable<User> GetUserByIdIncludeAll(Guid id);
    IQueryable<User> GetUserByEmail(string email);
    IQueryable<Resume> GetResumesByUserId(Guid id);

}