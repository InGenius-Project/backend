namespace IngBackendApi.UnitTest.Systems.Services;

using AutoMapper;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using IngBackendApi.Profiles;
using IngBackendApi.Services.AreaService;
using IngBackendApi.UnitTest.Fixtures;
using IngBackendApi.UnitTest.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

public class TestAreaService : IDisposable
{
    private readonly Mock<IUnitOfWork> _mockUnitofWork;
    private readonly IMapper _mapper;
    private readonly AreaService _areaService;
    private readonly Mock<IRepositoryWrapper> _repository;
    private readonly Mock<IWebHostEnvironment> _env;
    private readonly Mock<IConfiguration> _mockConfiguration;

    private readonly IRepository<AreaType, int> _areaTypeRepository;

    public TestAreaService()
    {
        var context = MemoryContextFixture.Generate();
        _mockUnitofWork = new Mock<IUnitOfWork>();
        _repository = MockRepositoryWrapper.GetMock(context);
        var _config = _mockConfiguration.Object;

        MappingProfile mappingProfile = new();
        MapperConfiguration configuration = new(cfg => cfg.AddProfile(mappingProfile));
        _mapper = new Mapper(configuration);

        // TODO: fix mock
        Mock<IWebHostEnvironment> env = new();

        _areaService = new AreaService(
            _mockUnitofWork.Object,
            _mapper,
            _repository.Object,
            env.Object,
            _config
        );

        _areaTypeRepository = _mockUnitofWork.Object.Repository<AreaType, int>();
    }

    [Fact]
    public void CheckAreaOwnership_InvalidOwnerId_ShouldThrowForbiddenException()
    {
        // Arrange
        var existArea = _repository.Object.Area.GetByIdAsync(Guid.Empty).Result;

        // Act
        Task act() => _areaService.CheckAreaOwnership(existArea.Id, Guid.NewGuid());

        // Assert
        Assert.ThrowsAsync<ForbiddenException>(act);
    }

    public void Dispose() => GC.SuppressFinalize(this);
}
