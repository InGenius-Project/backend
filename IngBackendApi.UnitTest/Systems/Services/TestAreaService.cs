namespace IngBackendApi.Test.Systems.Services;

using AutoMapper;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Profiles;
using IngBackendApi.Services.AreaService;
using IngBackendApi.Test.Mocks;

public class TestAreaService : IDisposable
{
    private readonly Mock<IUnitOfWork> _mockUnitofWork;
    private readonly IMapper _mapper;
    private readonly AreaService _areaService;
    private readonly Mock<IRepositoryWrapper> _repository;

    public TestAreaService()
    {

        _mockUnitofWork = new Mock<IUnitOfWork>();
        _repository = MockRepositoryWrapper.GetMock();

        MappingProfile mappingProfile = new();
        MapperConfiguration configuration = new(cfg => cfg.AddProfile(mappingProfile));
        _mapper = new Mapper(configuration);
        _areaService = new AreaService(_mockUnitofWork.Object, _mapper, _repository.Object);

    }

    [Fact]
    public async void CheckAreaOwnership_InvalidUser_ThrowForbiddenException()
    {
        // Arrange
        var existUser = await _repository.Object.User.GetByIdAsync(Guid.Empty);

        // Act
        Action act = () => _areaService.CheckAreaOwnership(existUser.Id, Guid.NewGuid());

        // Assert
        act.Should().Throw<ForbiddenException>();
    }


    public void Dispose() => GC.SuppressFinalize(this);
}



