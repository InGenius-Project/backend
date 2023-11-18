using IngBackend.Context;
using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Services.Repository;
using System.Collections;

namespace IngBackend.Services.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IngDbContext _context;
        private Hashtable _repositories;

        private bool _disposed;
        public UnitOfWork(IngDbContext context)
        {
            _context = context;
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public IRepository<TEntity, TKey> Repository<TEntity, TKey>() where TEntity : class, IEntity<TKey>
        {
            if (_repositories == null)
            {
                _repositories = new Hashtable();
            }
            var entityType = typeof(TEntity);
            if (!_repositories.ContainsKey(entityType))
            {
                var repositoryType = typeof(Repository<,>).MakeGenericType(entityType, typeof(TKey));
                var repository = Activator.CreateInstance(repositoryType, _context);
                _repositories.Add(entityType, repository);
            }

            return (IRepository<TEntity, TKey>)_repositories[entityType];
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                foreach (var repository in _repositories.Values.OfType<IDisposable>())
                {
                    repository.Dispose();
                }
                _context.Dispose();
            }

            _disposed = true;

        }



    }
}
