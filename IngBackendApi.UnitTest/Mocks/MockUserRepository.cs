namespace IngBackendApi.UnitTest.Mocks;

using IngBackendApi.Context;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Repository;
using IngBackendApi.UnitTest.Fixtures;

internal class MockUserRepository
{
    public static Mock<IUserRepository> GetMock(IngDbContext context)
    {
        UserFixture userFixture = new();
        var user1 = userFixture.Create();
        var user2 = userFixture.Create();

        List<User> users = [
            user1, user2
        ];

        context.AddRange(users);
        context.SaveChanges();

        Mock<IUserRepository> mockUserRepository = new();
        UserRepository stubUserRepository = new(context);


        // Set up

        //TODO : test userrepository
        mockUserRepository.Setup(m => m.GetUserByIdIncludeAll(It.IsAny<Guid>()))
             .Returns(stubUserRepository.GetUserByIdIncludeAll(user1.Id));

        mockUserRepository.Setup(m => m.AddAsync(It.IsAny<User>()))
            .Callback(() => { return; });
        mockUserRepository.Setup(m => m.UpdateAsync(It.IsAny<User>()))
            .Callback(() => { return; });
        mockUserRepository.Setup(m => m.DeleteByIdAsync(It.IsAny<Guid>()))
            .Callback(() => { return; });
        mockUserRepository.Setup(m => m.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(context.User.Where(u => u.Id == user1.Id).First());

        return mockUserRepository;
    }
}
