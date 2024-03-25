namespace IngBackendApi.UnitTest.Systems.Controllers;

using AutoMapper;
using AutoWrapper.Wrappers;
using IngBackendApi.Controllers;
using IngBackendApi.Enum;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DTO;
using IngBackendApi.Profiles;
using IngBackendApi.UnitTest.Fixtures;
using static Microsoft.AspNetCore.Http.StatusCodes;


public class TestAreaController : IDisposable
{
    private readonly AreaController _controller;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IAreaService> _mockAreaService;
    private readonly Mock<IAreaTypeService> _mockAreaTypeService;
    private readonly IMapper _mapper;

    public TestAreaController()
    {
        _mockAreaService = new Mock<IAreaService>();
        _mockUserService = new Mock<IUserService>();
        _mockAreaTypeService = new Mock<IAreaTypeService>();

        MappingProfile mappingProfile = new();
        MapperConfiguration configuration = new(cfg => cfg.AddProfile(mappingProfile));
        _mapper = new Mapper(configuration);

        _controller = new AreaController(
            _mapper,
            _mockUserService.Object,
            _mockAreaService.Object,
            _mockAreaTypeService.Object
        );

    }

    [Fact]
    public async Task GetAreaById_ShouldReturnAreaDTO_WhenAreaExists()
    {
        // Arrange
        UserInfoDTO user = new();
        var areaId = Guid.NewGuid();
        AreaFixture areaFixture = new();

        _mockUserService.Setup(x => x.CheckAndGetUserAsync(It.IsAny<Guid>())).ReturnsAsync(user);
        _mockAreaService.Setup(x => x.GetAreaIncludeAllById(It.IsAny<Guid>())).ReturnsAsync(
            areaFixture.Fixture.Create<AreaDTO>());

        // Act
        var result = await _controller.GetAreaById(areaId);

        // Assert
        result.Should().BeOfType<AreaDTO>();
    }

    [Fact]
    public async Task GetAreaById_ShouldThrowNotFoundException_WhenAreaDoesNotExist()
    {
        // Arrange
        UserInfoDTO user = new();
        var areaId = Guid.NewGuid();

        _mockUserService.Setup(x => x.CheckAndGetUserAsync(It.IsAny<Guid>())).ReturnsAsync(new UserInfoDTO());
        _mockAreaService.Setup(x => x.GetAreaIncludeAllById(areaId)).ReturnsAsync(null as AreaDTO);

        // Act
        Func<Task> act = async () => await _controller.GetAreaById(areaId);

        // Assert
        await act.Should().ThrowAsync<ApiException>();
    }

    [Fact]
    public async Task PostAreaType_ShouldReturnApiResponse_WhenRequestIsValid()
    {
        // Arrange
        AreaFixture areaFixture = new();
        UserFixture userFixture = new();
        var req = areaFixture.Fixture.Create<AreaTypePostDTO>();
        var user = userFixture.Fixture.Create<UserInfoDTO>();
        var area = areaFixture.Fixture.Create<AreaTypeDTO>();

        _mockUserService.Setup(x => x.CheckAndGetUserAsync(It.IsAny<Guid>())).ReturnsAsync(user);
        _mockAreaTypeService.Setup(x => x.GetByIdAsync(req.Id ?? 0)).ReturnsAsync((AreaTypeDTO)null);
        _mockAreaTypeService.Setup(x => x.AddAsync(It.IsAny<AreaTypeDTO>()));

        // Act
        var result = await _controller.PostAreaType(req);

        // Assert
        result.Should().BeOfType<ApiResponse>();
    }

    [Fact]
    public async Task PostAreaType_ShouldReturnApiResponse_WhenAreaTypeExists()
    {

        AreaFixture areaFixture = new();
        UserFixture userFixture = new();
        var req = areaFixture.Fixture.Create<AreaTypePostDTO>();
        var user = userFixture.Fixture.Create<UserInfoDTO>();
        var area = areaFixture.Fixture.Create<AreaTypeDTO>();

        _mockUserService.Setup(x => x.CheckAndGetUserAsync(It.IsAny<Guid>())).ReturnsAsync(user);
        _mockAreaTypeService.Setup(x => x.GetByIdAsync(req.Id ?? 0)).ReturnsAsync(area);
        _mockAreaTypeService.Setup(x => x.UpdateAsync(It.IsAny<AreaTypeDTO>()));

        // Act
        var result = await _controller.PostAreaType(req);

        // Assert
        result.Should().BeOfType<ApiResponse>();
    }

    [Fact]
    public async Task PostAreaType_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthorized()
    {
        // Arrange
        AreaFixture areaFixture = new();
        UserFixture userFixture = new();
        var req = areaFixture.Fixture.Create<AreaTypePostDTO>();
        var user = userFixture.Fixture.Build<UserInfoDTO>().With(x => x.Role, UserRole.Intern).Create();
        var area = areaFixture.Fixture.Create<AreaTypeDTO>();

        _mockUserService.Setup(x => x.CheckAndGetUserAsync(It.IsAny<Guid>())).ReturnsAsync(user);
        _mockAreaTypeService.Setup(x => x.GetByIdAsync(req.Id ?? 0)).ReturnsAsync(area);
        _mockAreaTypeService.Setup(x => x.UpdateAsync(It.IsAny<AreaTypeDTO>()));

        // Act
        var result = await _controller.PostAreaType(req);

        // Assert
        result.StatusCode.Should().Be(Status403Forbidden);
    }

    public void Dispose() => GC.SuppressFinalize(this);



}
