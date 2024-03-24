namespace IngBackendApi.UnitTest.Mocks;

using Moq;
using IngBackendApi.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public static class MockRepository
{
    public static Mock<IRepository<TEntity, TKey>> SetupMockRepository<TEntity, TKey>(DbContext context)
        where TEntity : class, IEntity<TKey>
    {
        var mockRepository = new Mock<IRepository<TEntity, TKey>>();

        // Setup for AddAsync method
        mockRepository.Setup(repo => repo.AddAsync(It.IsAny<TEntity>()))
            .Callback((TEntity entity) => context.Set<TEntity>().AddAsync(entity));

        // Setup for DeleteByIdAsync method
        mockRepository.Setup(repo => repo.DeleteByIdAsync(It.IsAny<TKey>()))
            .Callback((TKey id) => context.Set<TEntity>().Remove(context.Set<TEntity>().Find(id)));

        // Setup for GetByIdAsync method
        mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<TKey>()))
            .Returns((TKey id) => Task.FromResult(context.Set<TEntity>().Find(id)));

        // Setup for GetAll method
        mockRepository.Setup(repo => repo.GetAll())
            .Returns(context.Set<TEntity>().AsQueryable());

        // Setup for UpdateAsync method
        mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<TEntity>()))
            .Callback((TEntity entity) => context.Set<TEntity>().Update(entity));

        // Setup for SaveAsync method
        mockRepository.Setup(repo => repo.SaveAsync())
            .Callback(() => context.SaveChangesAsync());

        // Setup for Attach method
        mockRepository.Setup(repo => repo.Attach(It.IsAny<TEntity>()))
            .Callback((TEntity entity) => context.Attach(entity));

        // Setup for CollectionAsync method
        // mockRepository.Setup(repo => repo
        //     .CollectionAsync(
        //         It.IsAny<Expression<Func<TEntity, IEnumerable<TProperty>>>>()))
        //     .Returns((Expression<Func<TEntity, IEnumerable<TProperty>>> navigationProperty) =>
        //         Task.FromResult(context.Set<TEntity>().Include(navigationProperty).ToList().AsEnumerable()));

        // Setup for SetEntityState method
        mockRepository.Setup(repo => repo.SetEntityState(It.IsAny<TEntity>(), It.IsAny<EntityState>()))
            .Callback((TEntity entity, EntityState state) => context.Entry(entity).State = state);

        // Setup for GetLocal method
        mockRepository.Setup(repo => repo.GetLocal())
            .Returns(context.Set<TEntity>().Local.AsEnumerable());

        return mockRepository;
    }
}
