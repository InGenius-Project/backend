
using IngBackend.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IngBackend.Repository;

public class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    private DbContext _context { get; set; }
    private readonly DbSet<TEntity> _dbSet;

    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }
    public async Task AddAsync(TEntity entity)
    {
        await _context.Set<TEntity>().AddAsync(entity);
    }
    public async Task DeleteByIdAsync(TKey id)
    {
        var entityToDelete = await _dbSet.FindAsync(id);

        if (entityToDelete != null)
        {
            _dbSet.Remove(entityToDelete);
            await SaveAsync();
        }
    }
    public async Task<TEntity?> GetByIdAsync(TKey id)
    {
        return await _context.Set<TEntity>().FindAsync(id);
    }

    public IQueryable<TEntity> GetAll()
    {
        IQueryable<TEntity> query = _dbSet;
        return query;
    }

    /// <inheritdoc />
    public IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate)
    {
        IQueryable<TEntity> query = _dbSet;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return query;
    }


    public IQueryable<TEntity> GetAll(params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return query;
    }


    public IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return query;
    }

    public async Task UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        await SaveAsync();
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    // /// <summary>
    // /// 建構EF一個Entity的Repository，需傳入此Entity的Context。
    // /// </summary>
    // /// <param name="context">Entity所在的Context</param>
    // public Repository(DbContext context)
    // {
    //     _context = context;
    // }

    // /// <inheritdoc />
    // public void Add(TEntity entity)
    // {
    //     _context.Set<TEntity>().Add(entity);
    // }

    // /// <inheritdoc />
    // public async Task AddAsync(TEntity entity)
    // {
    //     await _context.Set<TEntity>().AddAsync(entity);
    // }

    // /// <inheritdoc />
    // public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    // {
    //     await _context.Set<TEntity>().AddRangeAsync(entities);
    // }

    // /// <inheritdoc />
    // public void Update(TEntity entity)
    // {
    //     _context.Entry(entity).State = EntityState.Modified;
    // }

    // /// <inheritdoc />
    // public void Update(TEntity entity, Expression<Func<TEntity, object>>[] updateProperties)
    // {
    //     _context.Entry(entity).State = EntityState.Unchanged;

    //     if (updateProperties != null)
    //     {
    //         foreach (var property in updateProperties)
    //         {
    //             _context.Entry(entity).Property(property).IsModified = true;
    //         }
    //     }
    // }

    // /// <inheritdoc />
    // public void Delete(TEntity entity)
    // {
    //     _context.Entry(entity).State = EntityState.Deleted;
    // }

    // /// <inheritdoc />
    // public void RemoveRange(IEnumerable<TEntity> entities)
    // {
    //     _context.Set<TEntity>().RemoveRange(entities);
    // }

    // /// <inheritdoc />
    // public IQueryable<TEntity> Query()
    // {
    //     return _context.Set<TEntity>().AsQueryable();
    // }

    // /// <inheritdoc />
    // public TEntity GetById(TKey id)
    // {
    //     return _context.Set<TEntity>().Find(id);
    // }

    // /// <inheritdoc />
    // public async Task<TEntity> GetByIdAsync(TKey id)
    // {
    //     return await _context.Set<TEntity>().FindAsync(id);
    // }

    /// <inheritdoc />
    // public IQueryable<TEntity> GetAll()
    // {
    //     return _context.Set<TEntity>().AsQueryable();
    // }

    // /// <inheritdoc />
    // public IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate)
    // {
    //     return _context.Set<TEntity>().Where(predicate).AsQueryable();
    // }

    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> CollectionAsync<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>>> navigationProperty)
    {
        var query = _context.Set<TEntity>().AsQueryable();
        var result = await query.Include(navigationProperty).ToListAsync();
        return result;
    }

    /// <inheritdoc />
    public void SetEntityState(TEntity entity, EntityState state)
    {
        _context.Entry(entity).State = state;
    }

    /// <inheritdoc />
    public IEnumerable<TEntity> GetLocal()
    {
        return _context.Set<TEntity>().Local.AsEnumerable();
    }
}
