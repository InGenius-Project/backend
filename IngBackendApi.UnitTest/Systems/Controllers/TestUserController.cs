namespace IngBackendApi.UnitTest.Systems.Controllers;

using AutoMapper;
using AutoWrapper.Wrappers;
using Hangfire;
using IngBackendApi.Controllers;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using IngBackendApi.Profiles;
using IngBackendApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

public class TestUserController : IDisposable
{
    private readonly UserController _controller;
    private readonly Mock<IUserService> _mockUserService;
    private readonly IMapper _mapper;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IBackgroundJobClient> _mockBackgroundJobClient;
    private readonly Mock<EmailService> _mockEmailService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
    private readonly Fixture _fixture;

    public TestUserController()
    {
        _mockUserService = new Mock<IUserService>();

        MappingProfile mappingProfile = new();
        MapperConfiguration configuration = new(cfg => cfg.AddProfile(mappingProfile));
        _mapper = new Mapper(configuration);

        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
        Mock<IConfiguration> mockConfiguration = new();
        // Set up configuration as needed

        _mockEmailService = new Mock<EmailService>(mockConfiguration.Object);

        _controller = new UserController(
            null, // Pass null for TokenService as it's not used in GetUser action
            _mockUserService.Object,
            _mapper,
            _mockPasswordHasher.Object,
            _mockBackgroundJobClient.Object,
            _mockEmailService.Object,
            _mockWebHostEnvironment.Object
        );

        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetUserValidUserIdReturnsOkObjectResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        UserInfoDTO userInfoDto = new(); // Fill with test data as needed
        _mockUserService
            .Setup(x => x.GetUserByIdIncludeAllAsync(Guid.Empty))
            .ReturnsAsync(userInfoDto);

        // Act
        var result = await _controller.GetUser();

        // Assert
        Assert.IsType<UserInfoDTO>(result);
        Assert.Equal(userInfoDto, result);
    }

    [Fact]
    public async Task GetUserInvalidUserIdThrowsApiException()
    {
        // Arrange
        UserInfoDTO? nullValue = null;
        _mockUserService
            .Setup(x => x.GetUserByIdIncludeAllAsync(Guid.Empty))
            .ReturnsAsync(nullValue);

        // Act
        var exception = await Assert.ThrowsAsync<ApiException>(() => _controller.GetUser());

        // Assert
        Assert.Equal("使用者不存在", exception.Message);
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task PostUserValidUserInfoPostDTOReturnsSuccessfulResult()
    {
        // Arrange
        var userInfoPostDTO = _fixture.Create<UserInfoPostDTO>();

        var user = _mapper.Map<User>(userInfoPostDTO);

        _mockUserService.Setup(x => x.PostUser(userInfoPostDTO, Guid.Empty));

        // Act
        var result = await _controller.PostUser(userInfoPostDTO);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Fact]
    public async Task PostUserInValidUserInfoPostDTOReturnsSuccessfulResult()
    {
        // Arrange
        var userInfoPostDTO = _fixture.Create<UserInfoPostDTO>();
        var user = _mapper.Map<User>(userInfoPostDTO);

        _mockUserService.Setup(x => x.PostUser(userInfoPostDTO, Guid.Empty));

        // Act
        var result = await _controller.PostUser(userInfoPostDTO);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    public void Dispose() => GC.SuppressFinalize(this);
}
