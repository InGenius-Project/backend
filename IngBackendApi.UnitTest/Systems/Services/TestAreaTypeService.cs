namespace IngBackendApi.UnitTest.Systems.Services;

using AutoMapper;
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
    private readonly Mock<IRepositoryWrapper> _repository;
    private readonly IRepository<AreaType, int> _areaTypeRepository;

    private readonly IngDbContext context;
    public TestAreaTypeService()
    {
        context = MemoryContextFixture.Generate();
        _unitofWork = new UnitOfWork(context);
        _repository = MockRepositoryWrapper.GetMock(context);

        MappingProfile mappingProfile = new();
        MapperConfiguration configuration = new(cfg => cfg.AddProfile(mappingProfile));
        _mapper = new Mapper(configuration);

        _areaTypeService = new AreaTypeService(_unitofWork, _mapper, _repository.Object);

        _areaTypeRepository = _unitofWork.Repository<AreaType, int>();

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
        var areaType = _areaTypeRepository.GetAll().First();
        areaType.Should().NotBeNull();

    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_ShouldUpdateArea()
    {
        // Arrange
        var areaType = _repository.Object.AreaType.GetAll().First();

        context.Set<TagType>().Add(new TagType()
        {
            Name = "Test",
            Value = "Test",
            Color = "#123"
        });

        await context.SaveChangesAsync();
        var tagType = _repository.Object.TagType.GetAll().First();

        var areaFixture = new AreaFixture();
        var updateAreaTypeDto = areaFixture.Fixture.Create<AreaTypeDTO>();
        updateAreaTypeDto.Id = areaType.Id;
        updateAreaTypeDto.ListTagTypes = [
           _mapper.Map<TagTypeDTO>(tagType)
        ];

        _areaTypeRepository.SetEntityState(areaType, EntityState.Detached);
        _repository.Object.TagType.SetEntityState(tagType, EntityState.Detached);

        await context.SaveChangesAsync();
        // Act
        await _areaTypeService.UpdateAsync(updateAreaTypeDto);

        // Assert
        var updatedAreaType = _repository.Object.AreaType.GetAll().First();
        updatedAreaType.Name.Should().Be(updateAreaTypeDto.Name);
        updatedAreaType.Value.Should().Be(updateAreaTypeDto.Value);
        updatedAreaType.Description.Should().Be(updateAreaTypeDto.Description);
        updatedAreaType.LayoutType.Should().Be(updateAreaTypeDto.LayoutType);
        if (updateAreaTypeDto.UserRole != null)
        {
            var result = updatedAreaType.UserRole?.All(x => updateAreaTypeDto.UserRole.Any(y => y == x));
            result.Should().BeTrue();
        }

        if (updateAreaTypeDto.ListTagTypes != null)
        {
            var result = updatedAreaType.ListTagTypes?.All(x => updateAreaTypeDto.ListTagTypes.Any(y => y.Id == x.Id));
            result.Should().BeTrue();
        }

    }



    public void Dispose() => GC.SuppressFinalize(this);
}
