namespace IngBackend.Repository;

using IngBackend.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class Repository<TEntity, TKey>(DbContext context) : IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    private DbContext Context { get; set; } = context;
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public async Task AddAsync(TEntity entity)
    {
        await Context.Set<TEntity>().AddAsync(entity);
        await SaveAsync();
    }
    public async Task DeleteByIdAsync(TKey key)
    {
        var entityToDelete = await _dbSet.FindAsync(key);

        if (entityToDelete != null)
        {
            _dbSet.Remove(entityToDelete);
            await SaveAsync();
        }
    }
    public async Task<TEntity?> GetByIdAsync(TKey id) => await Context.Set<TEntity>().FindAsync(id);

    public async Task<TEntity?> GetByIdAsync(TKey id, bool tracking = true)
    {
        if (!tracking)
        {
            return await Context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(e => e.Id.Equals(id));
        }
        return await Context.Set<TEntity>().FindAsync(id);
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

    public async Task SaveAsync() => await Context.SaveChangesAsync();

    /// <inheritdoc />
    public async Task AddRangeAsync(IEnumerable<TEntity> entities) => await Context.Set<TEntity>().AddRangeAsync(entities);


    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> CollectionAsync<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>>> navigationProperty)
    {
        var query = Context.Set<TEntity>().AsQueryable();
        var result = await query.Include(navigationProperty).ToListAsync();
        return result;
    }

    /// <inheritdoc />
    public void SetEntityState(TEntity entity, EntityState state) => Context.Entry(entity).State = state;

    /// <inheritdoc />
    public IEnumerable<TEntity> GetLocal() => Context.Set<TEntity>().Local.AsEnumerable();

}
