namespace IngBackend.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public interface IRepository<TEntity, TKey> where TEntity : IEntity<TKey>
{
    Task AddAsync(TEntity entity);
    Task<TEntity?> GetByIdAsync(TKey id);
    Task<TEntity?> GetByIdAsync(TKey id, bool tracking = true);
    IQueryable<TEntity> GetAll();
    IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate);
    IQueryable<TEntity> GetAll(params Expression<Func<TEntity, object>>[] includes);
    IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);
    Task UpdateAsync(TEntity entity);
    Task DeleteByIdAsync(TKey key);
    Task SaveAsync();


    // /// <summary>
    // /// 新增多筆實體到資料庫。
    // /// </summary>
    // /// <param name="entities">要新增到資料庫的實體集合。</param>
    // /// <returns>表示執行新增作業的非同步作業。</returns>
    // Task AddRangeAsync(IEnumerable<TEntity> entities);

    // /// <summary>
    // /// 刪除一個實體集合。
    // /// </summary>
    // /// <param name="entities">要刪除的實體集合。</param>
    // void RemoveRange(IEnumerable<TEntity> entities);


    /// <summary>
    /// 取得包含指定導覽屬性的所有實體集合。
    /// </summary>
    /// <param name="includes">導覽屬性的表達式</param>
    /// <returns>包含指定導覽屬性的所有實體集合</returns>
    Task<IEnumerable<TEntity>> CollectionAsync<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>>> navigationProperty);

    /// <summary>
    /// 設定實體狀態
    /// </summary>
    /// <param name="entity">實體</param>
    /// <param name="state">狀態</param>
    void SetEntityState(TEntity entity, EntityState state);

    /// <summary>
    /// 取得已在當前 DbContext 中追蹤的 TEntity 實體集合。
    /// </summary>
    /// <returns>已在當前 DbContext 中追蹤的 TEntity 實體集合。</returns>
    IEnumerable<TEntity> GetLocal();
}
