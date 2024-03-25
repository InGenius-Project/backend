namespace IngBackendApi.UnitTest.Mocks;

using IngBackendApi.Context;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.UnitTest.Mocks;

public class MockRepositoryWrapper
{
    public static Mock<IRepositoryWrapper> GetMock(IngDbContext context)
    {
        Mock<IRepositoryWrapper> mock = new();
        var mockUserRepository = MockUserRepository.GetMock(context);
        var mockAreaRepository = MockAreaRepository.GetMock(context);
        var mockAreaTypeRepository = MockRepository.SetupMockRepository<AreaType, int>(context);

        mock.Setup(m => m.User).Returns(() => mockUserRepository.Object);
        mock.Setup(m => m.Area).Returns(() => mockAreaRepository.Object);
        mock.Setup(m => m.AreaType).Returns(() => mockAreaTypeRepository.Object);
        mock.Setup(m => m.Save()).Callback(() => { return; });

        return mock;
    }
}
