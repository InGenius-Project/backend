
using IngBackend.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IngBackend.Services.Repository;

public class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    private DbContext _context { get; set; }

    public Repository(DbContext context)
    {
        _context = context;
    }

    public TEntity? GetById(TKey id)
    {
        return _context.Set<TEntity>().Find(id);
    }

    public void Add(TEntity entity)
    {
        _context.Set<TEntity>().Add(entity);
    }
    public void Update(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
    }
    public void Delete(TEntity entity)
    {
        _context.Entry(entity).State = EntityState.Deleted;
    }
    public IQueryable<TEntity> GetAll()
    {
        return _context.Set<TEntity>().AsQueryable();
    }

    public IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate)
    {
        return _context.Set<TEntity>().Where(predicate).AsQueryable();
    }

}