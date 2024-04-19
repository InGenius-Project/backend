namespace IngBackendApi.UnitTest.Systems.Services;

using AutoMapper;
using IngBackendApi.Context;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DTO;
using IngBackendApi.Profiles;
using IngBackendApi.Services.UserService;
using IngBackendApi.UnitTest.Fixtures;
using IngBackendApi.UnitTest.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

public class TestUserService : IDisposable
{
    private readonly Mock<IUnitOfWork> _mockUnitofWork;
    private readonly IMapper _mapper;
    private readonly Mock<IRepositoryWrapper> _repository;
    private readonly Mock<IPasswordHasher> _passwordHasher;
    private readonly UserService _userService;
    private readonly Fixture _fixture;
    private readonly IngDbContext _context;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IWebHostEnvironment> _env;

    public void Dispose() => GC.SuppressFinalize(this);

    public TestUserService()
    {
        _mockUnitofWork = new Mock<IUnitOfWork>();

        MappingProfile mappingProfile = new();
        MapperConfiguration configuration = new(cfg => cfg.AddProfile(mappingProfile));
        _mapper = new Mapper(configuration);
        _context = MemoryContextFixture.Generate();

        _repository = MockRepositoryWrapper.GetMock(_context);
        _env = new Mock<IWebHostEnvironment>();
        _mockConfiguration = new Mock<IConfiguration>();

        _passwordHasher = new Mock<IPasswordHasher>();

        _userService = new UserService(
            _mockUnitofWork.Object,
            _mapper,
            _repository.Object,
            _passwordHasher.Object,
            _env.Object,
            _mockConfiguration.Object
        );

        _fixture = new Fixture();
    }

    [Fact]
    public async Task PostUser_UpdateExistedUser_PostSuccess()
    {
        // Arrange
        var existedUser = await _repository.Object.User.GetByIdAsync(Guid.Empty);

        // TODO: test Avatar and Tags
        UserInfoPostDTO req = new() { Username = "updateTest" };

        // Act
        await _userService.PostUser(req, existedUser.Id); // update exist user

        // Assert
        existedUser.Username.Should().Be(req.Username);
    }
}
