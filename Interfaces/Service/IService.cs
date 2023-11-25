using IngBackend.Interfaces.Repository;
using System.Linq.Expressions;

namespace IngBackend.Interfaces.Service;

public interface IService<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    // 根據主鍵查詢實體
    TEntity? GetById(TKey id, params Expression<Func<TEntity, object>>[] includes);
    Task<TEntity>? GetByIdAsync(TKey id, params Expression<Func<TEntity, object>>[] includes);

    // 新增實體
    void Add(TEntity entity);
    Task AddAsync(TEntity entity);

    // 更新實體
    void Update(TEntity entity);

    // 刪除實體
    void Delete(TEntity entity);

    // 取得所有實體
    IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);
}
