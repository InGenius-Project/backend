namespace IngBackendApi.UnitTest.Systems.Services;

using AutoMapper;
using IngBackend.Repository;
using IngBackendApi.Context;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using IngBackendApi.Profiles;
using IngBackendApi.Services.AreaService;
using IngBackendApi.Services.UnitOfWork;
using IngBackendApi.UnitTest.Fixtures;
using IngBackendApi.UnitTest.Mocks;
using Microsoft.EntityFrameworkCore;

public class TestAreaTypeService : IDisposable
{
    private readonly IUnitOfWork _unitofWork;
    private readonly IMapper _mapper;
    private readonly AreaTypeService _areaTypeService;
    private readonly RepositoryWrapper _repository;
    private readonly IRepository<AreaType, int> _areaTypeRepository;

    private readonly AreaFixture _areaFixture;
    private readonly TagFixture _tagFixture;
    private readonly IngDbContext context;

    public TestAreaTypeService()
    {
        _areaFixture = new AreaFixture();
        _tagFixture = new TagFixture();

        context = MemoryContextFixture.Generate();
        _unitofWork = new UnitOfWork(context);
        _repository = new RepositoryWrapper(context);

        MappingProfile mappingProfile = new();
        MapperConfiguration configuration = new(cfg => cfg.AddProfile(mappingProfile));
        _mapper = new Mapper(configuration);

        _areaTypeService = new AreaTypeService(_unitofWork, _mapper, _repository);

        _areaTypeRepository = _unitofWork.Repository<AreaType, int>();
    }

    [Fact]
    public async Task AddAsync_WhenCalled_ShouldAddArea()
    {
        // Arrange
        var areaTypeDto = _areaFixture.Fixture.Create<AreaTypeDTO>();

        // Act
        await _areaTypeService.AddAsync(areaTypeDto);

        // Assert
        var areaType = _areaTypeRepository.GetAll().First();
        areaType.Should().NotBeNull();
    }

    public void Dispose() => GC.SuppressFinalize(this);
}
