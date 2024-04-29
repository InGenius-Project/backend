namespace IngBackendApi.UnitTest.Systems.Controllers;

using AutoMapper;
using IngBackend.Controllers;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using IngBackendApi.Profiles;
using IngBackendApi.UnitTest.Fixtures;

public class TestTagController : IDisposable
{
    private readonly IMapper _mapper;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ITagService> _mockTagService;
    private readonly Mock<IService<TagType, TagTypeDTO, int>> _mockTagTypeService;
    private readonly Mock<IAreaService> _mockAreaService;
    private readonly TagController _controller;

    public TestTagController()
    {
        _mockTagService = new Mock<ITagService>();
        _mockUserService = new Mock<IUserService>();
        _mockTagTypeService = new Mock<IService<TagType, TagTypeDTO, int>>();
        _mockAreaService = new Mock<IAreaService>();

        MappingProfile mappingProfile = new();
        MapperConfiguration configuration = new(cfg => cfg.AddProfile(mappingProfile));
        _mapper = new Mapper(configuration);

        _controller = new TagController(
            _mapper,
            _mockUserService.Object,
            _mockTagService.Object,
            _mockTagTypeService.Object,
            _mockAreaService.Object
        );
    }

    [Fact]
    public void GetTag_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockTagService.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((TagDTO)null);

        Task act() => _controller.GetTag(id);

        // Act and Assert
        Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task<TagDTO> GetTag_ReturnsTagDTO()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tag = new TagDTO
        {
            Id = id,
            Name = "Test Tag",
            TagTypeId = 1,
            Type = new TagTypeDTO
            {
                Id = 1,
                Name = "Test Tag Type",
                Value = "Test Value",
                Color = "Test Color"
            }
        };
        _mockTagService.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(tag);

        // Act
        var result = await _controller.GetTag(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tag.Id, result.Id);
        Assert.Equal(tag.Name, result.Name);

        return result;
    }

    [Fact]
    public async Task PostTag_UpdateTag_ReturnsApiResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var req = new TagPostDTO
        {
            Id = id,
            Name = "Test Tag",
            TagTypeId = 1,
        };
        var userId = Guid.NewGuid();
        var tag = new TagDTO
        {
            Id = id,
            Name = req.Name,
            TagTypeId = req.TagTypeId,
        };
        var userFixture = new UserFixture();
        _mockTagService.Setup(x => x.AddOrUpdateAsync(req, userId)).ReturnsAsync(tag);

        // Act
        var result = await _controller.PostTag(req);

        // Assert
        // TODO: correct the test
        // Assert.NotNull(result);
        // Assert.Equal(tag.Id, result.Id);
        // _mockTagService.Verify(x => x.UpdateAsync(tag), Times.Once);
    }

    [Fact]
    public async Task PostTag_AddTag_ReturnsApiResponse()
    {
        // Arrange
        var id = Guid.Empty;
        var req = new TagPostDTO
        {
            Id = id,
            Name = "Test Tag",
            TagTypeId = 1,
        };
        var userId = Guid.NewGuid();
        var userFixture = new UserFixture();

        var user = userFixture.Fixture.Create<UserInfoDTO>();
        _mockTagService
            .Setup(x => x.AddOrUpdateAsync(req, userId))
            .ReturnsAsync(
                new TagDTO
                {
                    Id = Guid.NewGuid(),
                    Name = req.Name,
                    TagTypeId = req.TagTypeId
                }
            );
        // Act
        var result = await _controller.PostTag(req);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        _mockTagService.Verify(x => x.AddAsync(It.IsAny<TagDTO>()), Times.Once);
    }

    public void Dispose() => GC.SuppressFinalize(this);
}
