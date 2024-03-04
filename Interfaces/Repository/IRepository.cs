namespace IngBackend.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public interface IRepository<TEntity, TKey> where TEntity : IEntity<TKey>
{

    Task<TEntity?> GetByIdAsync(TKey id);
    Task<TEntity?> GetByIdAsync(TKey id, bool tracking = true);
    IQueryable<TEntity> GetAll();
    IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate);
    IQueryable<TEntity> GetAll(params Expression<Func<TEntity, object>>[] includes);
    IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);
    Task AddRangeAsync(IEnumerable<TEntity> entities);
    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteByIdAsync(TKey key);
    Task SaveAsync();
    Task<IEnumerable<TEntity>> CollectionAsync<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>>> navigationProperty);
    void SetEntityState(TEntity entity, EntityState state);
    IEnumerable<TEntity> GetLocal();
}
