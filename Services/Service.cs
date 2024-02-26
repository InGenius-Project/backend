using AutoMapper;
using AutoWrapper.Wrappers;
using IngBackend.Exceptions;
using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.Service;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IngBackend.Services;

/// <summary>
/// 泛型的 Service 實作類別，提供基本的 CRUD 操作
/// </summary>
/// <typeparam name="TEntity">實體類別</typeparam>
public class Service<TEntity, TDto, TKey> : IService<TEntity, TDto, TKey>
    where TEntity : class, IEntity<TKey>
    where TDto : class
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public Service(IUnitOfWork unitOfWork, IMapper mapper) : base()
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public IEnumerable<TDto> GetAll()
    {
        try
        {
            var query = _unitOfWork.Repository<TEntity, TKey>().GetAll();

            if (query.Any())
            {
                return _mapper.Map<IEnumerable<TDto>>(query);
            }
            else
            {
                throw new EntityNotFoundException(typeof(TDto).Name);
            }

        }
        catch (EntityNotFoundException ex)
        {
            var message = $"Error retrieving all {typeof(TDto).Name}s";

            throw new EntityNotFoundException(message, ex);
        }
    }

    public IEnumerable<TDto> GetAll(params Expression<Func<TEntity, object>>[] includes)
    {
        var query = _unitOfWork.Repository<TEntity, TKey>().GetAll(includes).AsNoTracking();

        return _mapper.Map<IEnumerable<TDto>>(query);
    }

    public IEnumerable<TDto> GetAll(Expression<Func<TEntity, bool>> predicate)
    {
        var query = _unitOfWork.Repository<TEntity, TKey>().GetAll(predicate).AsNoTracking();

        return _mapper.Map<IEnumerable<TDto>>(query);
    }

    /// <inheritdoc />
    public IEnumerable<TDto> GetAll(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = _unitOfWork.Repository<TEntity, TKey>().GetAll(predicate, includes).AsNoTracking();

        return _mapper.Map<IEnumerable<TDto>>(query);
    }

    public async Task<TDto?> GetByIdAsync(TKey id)
    {
        try
        {
            var result = await _unitOfWork.Repository<TEntity, TKey>().GetByIdAsync(id, false);

            return _mapper.Map<TDto>(result);
        }

        catch (Exception ex)
        {
            throw new ApiException(ex);
        }
    }

    public async Task<TDto?> GetByIdAsync(TKey id, params Expression<Func<TEntity, object>>[] includes)
    {
        var entity = await _unitOfWork.Repository<TEntity, TKey>().GetAll(includes).FirstOrDefaultAsync(e => e.Id.Equals(id));

        var dto = _mapper.Map<TDto>(entity);

        if (entity == null)
        {
            return null;
        }
        _unitOfWork.Repository<TEntity, TKey>().SetEntityState(entity, EntityState.Detached);
        return dto;
    }


    public virtual async Task AddAsync(TDto dto)
    {
        await _unitOfWork.Repository<TEntity, TKey>().AddAsync(_mapper.Map<TEntity>(dto));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteByIdAsync(TKey id)
    {
        await _unitOfWork.Repository<TEntity, TKey>().DeleteByIdAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAsync(TDto dto)
    {
        var entity = _mapper.Map<TEntity>(dto);

        await _unitOfWork.Repository<TEntity, TKey>().UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SaveChangesAsync() => await _unitOfWork.SaveChangesAsync();


    // /// <inheritdoc />
    // public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
    // {
    //     await _unitOfWork.Repository<TEntity, TKey>().AddRangeAsync(entities);
    // }


    // /// <inheritdoc />
    // public virtual void RemoveRange(IEnumerable<TEntity> entities)
    // {
    //     _unitOfWork.Repository<TEntity, TKey>().RemoveRange(entities);
    // }


    // /// <inheritdoc />
    // public IQueryable<TEntity> GetAll(params Expression<Func<TEntity, object>>[] includes)
    // {
    //     var query = _unitOfWork.Repository<TEntity, TKey>().Query();

    //     foreach (var include in includes)
    //     {
    //         query = query.Include(include);
    //     }
    //     return query;
    // }

    // /// <inheritdoc />
    // public IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
    // {
    //     var query = _unitOfWork.Repository<TEntity, TKey>().Query();

    //     foreach (var include in includes)
    //     {
    //         query = query.Include(include);
    //     }
    //     return query.Where(predicate);
    // }

    // /// <inheritdoc />
    // public TEntity? GetById(TKey id, params Expression<Func<TEntity, object>>[] includes)
    // {
    //     var query = _unitOfWork.Repository<TEntity, TKey>().Query();

    //     foreach (var include in includes)
    //     {
    //         query = query.Include(include);
    //     }

    //     return query.FirstOrDefault(e => e.Id.Equals(id));
    // }

    // /// <inheritdoc />
    // public async Task<TEntity?> GetByIdAsync(TKey id, params Expression<Func<TEntity, object>>[] includes)
    // {
    //     var query = _unitOfWork.Repository<TEntity, TKey>().Query();

    //     foreach (var include in includes)
    //     {
    //         query = query.Include(include);
    //     }

    //     var entity = await query.FirstOrDefaultAsync(e => e.Id.Equals(id));

    //     return entity;
    // }


    /// <inheritdoc />
    public async Task LoadCollectionAsync<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> navigationProperty)
    where TProperty : class => await _unitOfWork.LoadCollectionAsync(entity, navigationProperty);

    /// <inheritdoc />
    public async Task LoadCollectionAsync<TProperty>(IEnumerable<TProperty> entities, Expression<Func<TProperty, IEnumerable<TEntity>>> navigationProperty)
    where TProperty : class => await _unitOfWork.LoadCollectionAsync(entities, navigationProperty);

    /// <inheritdoc />
    public void SetEntityState(TEntity entity, EntityState state) => _unitOfWork.Repository<TEntity, TKey>().SetEntityState(entity, state);

    /// <inheritdoc />
    public IEnumerable<TEntity> GetLocal() => _unitOfWork.Repository<TEntity, TKey>().GetLocal();
}