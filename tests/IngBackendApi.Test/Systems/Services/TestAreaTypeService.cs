namespace IngBackendApi.Test.Systems.Services;

using AutoMapper;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using IngBackendApi.Profiles;
using IngBackendApi.Services.AreaService;
using IngBackendApi.Test.Fixtures;
using IngBackendApi.Test.Mocks;

public class TestAreaTypeService : IDisposable
{
    private readonly Mock<IUnitOfWork> _mockUnitofWork;
    private readonly IMapper _mapper;
    private readonly AreaTypeService _areaTypeService;
    private readonly Mock<IRepositoryWrapper> _repository;
    private readonly Mock<IRepository<AreaType, int>> _areaTypeRepository;
    public TestAreaTypeService()
    {

        _mockUnitofWork = new Mock<IUnitOfWork>();
        _repository = MockRepositoryWrapper.GetMock();

        MappingProfile mappingProfile = new();
        MapperConfiguration configuration = new(cfg => cfg.AddProfile(mappingProfile));
        _mapper = new Mapper(configuration);

        _areaTypeService = new AreaTypeService(_mockUnitofWork.Object, _mapper, _repository.Object);

        _areaTypeRepository = new Mock<IRepository<AreaType, int>>();

    }
    [Fact]
    public async Task AddAsync_WhenCalled_ShouldAddArea()
    {
        // Arrange
        var areaFixture = new AreaFixture();
        var areaTypeDto = areaFixture.Fixture.Create<AreaTypeDTO>();

        // Act
        await _areaTypeService.AddAsync(areaTypeDto);

        // Assert
        var areaType = _areaTypeRepository.Object.GetAll().First();
        areaType.Should().NotBeNull();

    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_ShouldUpdateArea()
    {
        // Arrange
        var areaFixture = new AreaFixture();
        var areaType = areaFixture.Fixture.Create<AreaType>();
        var context = MemoryContextFixture.Generate();
        context.Add(areaType);
        await context.SaveChangesAsync();

        var updateAreaTypeDto = areaFixture.Fixture.Create<AreaTypeDTO>();
        updateAreaTypeDto.Id = areaType.Id;

        // Act
        await _areaTypeService.UpdateAsync(updateAreaTypeDto);

        // Assert
        areaType.Should().Match<AreaType>(src =>
            src.Name == updateAreaTypeDto.Name &&
            src.Value == updateAreaTypeDto.Value &&
            src.Description == updateAreaTypeDto.Description &&
            src.UserRole == updateAreaTypeDto.UserRole &&
            src.LayoutType == updateAreaTypeDto.LayoutType);

        if (updateAreaTypeDto.ListTagTypes != null)
        {
            var result = areaType.ListTagTypes?.All(x => updateAreaTypeDto.ListTagTypes.Any(y => y.Id == x.Id));
            result.Should().BeTrue();
        }

    }



    public void Dispose() => GC.SuppressFinalize(this);
}
