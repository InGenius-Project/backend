namespace IngBackendApi.Test.Mocks;

using IngBackendApi.Interfaces.Repository;

public class MockRepositoryWrapper
{
    public static Mock<IRepositoryWrapper> GetMock()
    {
        Mock<IRepositoryWrapper> mock = new();
        var mockUserRepository = MockUserRepository.GetMock();
        var mockAreaRepository = MockAreaRepository.GetMock();

        mock.Setup(m => m.User).Returns(() => mockUserRepository.Object);
        mock.Setup(m => m.Area).Returns(() => mockAreaRepository.Object);
        mock.Setup(m => m.Save()).Callback(() => { return; });

        return mock;
    }
}
