using IngBackend.Models.DBEntity;

namespace IngBackend.Interfaces.Repository;

public interface IUserRepository : IRepository<User, Guid>
{
    IQueryable<User> GetUserById(Guid id);
}