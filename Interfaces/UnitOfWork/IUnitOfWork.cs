
using IngBackend.Interfaces.Repository;

namespace IngBackend.Interfaces.UnitOfWork;

public interface IUnitOfWork : IDisposable
{

    void SaveChanges();

    IRepository<TEntity, TKey> Repository<TEntity, TKey>() where TEntity : class, IEntity<TKey>;

}
