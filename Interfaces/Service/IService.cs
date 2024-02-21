using IngBackend.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IngBackend.Interfaces.Service;
public interface IService<TEntity, TDto, TKey> where TEntity : IEntity<TKey> where TDto : class
{
    IEnumerable<TDto> GetAll();
    IEnumerable<TDto> GetAll(params Expression<Func<TEntity, object>>[] includes);
    IEnumerable<TDto> GetAll(Expression<Func<TEntity, bool>> predicate);

    IEnumerable<TDto> GetAll(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);

    Task<TDto?> GetByIdAsync(TKey id);
    Task<TDto?> GetByIdAsync(TKey id, params Expression<Func<TEntity, object>>[] includes);
    Task AddAsync(TDto dto);
    Task DeleteByIdAsync(TKey key);
    Task UpdateAsync(TDto dto);
    Task SaveChangesAsync();

    // Task AddRangeAsync(IEnumerable<TEntity> entities);
    /// <summary>
    /// 非同步載入指定實體的集合屬性。
    /// </summary>
    /// <typeparam name="TProperty">集合屬性的型別。</typeparam>
    /// <param name="entity">要載入集合屬性的實體。</param>
    /// <param name="navigationProperty">指定要載入的集合屬性。</param>
    /// <returns>表示非同步載入操作的 <see cref="Task"/> 物件。</returns>
    Task LoadCollectionAsync<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> navigationProperty) where TProperty : class;

    /// <summary>
    /// 非同步加載多個實體對象集合的導覽屬性，透過提供的導覽屬性表達式和一個實體對象集合。
    /// </summary>
    /// <typeparam name="TProperty">導覽屬性類型。</typeparam>
    /// <param name="entities">實體對象集合。</param>
    /// <param name="navigationProperty">導覽屬性表達式。</param>
    /// <returns>異步操作任務。</returns>
    /// <exception cref="ArgumentNullException">當 entities 或 navigationProperty 為 null 時拋出。</exception>
    /// <exception cref="ArgumentException">當 navigationProperty 表達式不是有效的導覽屬性表達式時拋出。</exception>
    Task LoadCollectionAsync<TProperty>(IEnumerable<TProperty> entities, Expression<Func<TProperty, IEnumerable<TEntity>>> navigationProperty) where TProperty : class;

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
