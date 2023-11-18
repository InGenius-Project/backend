using System.Linq.Expressions;

namespace IngBackend.Interfaces.Repository;

public interface IRepository<TEntity, TKey> where TEntity : IEntity<TKey>
{
    // 根據主鍵查詢實體
    TEntity? GetById(TKey id);

    // 新增實體
    void Add(TEntity entity);

    // 更新實體
    void Update(TEntity entity);

    // 刪除實體
    void Delete(TEntity entity);


    // 取得所有實體
    IQueryable<TEntity> GetAll();
    IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate);
}
