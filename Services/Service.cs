using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.Service;
using IngBackend.Interfaces.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IngBackend.Services;

public class Service<TEntity, TKey> : IService<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    private readonly IUnitOfWork _unitOfWork;

    public Service(IUnitOfWork unitOfWork)
    { _unitOfWork = unitOfWork; }

    public void Add(TEntity entity)
    {
        _unitOfWork.Repository<TEntity, TKey>().Add(entity);
    }
    public TEntity? GetById(TKey id, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = _unitOfWork.Repository<TEntity, TKey>().GetAll();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return query.FirstOrDefault(e => e.Id.Equals(id));

    }


    public IQueryable<TEntity>? GetAll(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = _unitOfWork.Repository<TEntity, TKey>().GetAll();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return query.Where(predicate);
    }

    public void Delete(TEntity entity)
    {
        _unitOfWork.Repository<TEntity, TKey>().Delete(entity);
    }

    public void Update(TEntity entity)
    {
        _unitOfWork.Repository<TEntity, TKey>().Update(entity);
    }


    public void SaveChange()
    {
        _unitOfWork.SaveChanges();
    }
}
