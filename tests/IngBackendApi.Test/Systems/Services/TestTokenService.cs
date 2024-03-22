using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IngBackendApi.Models.DTO;
using IngBackendApi.Services.TokenServices;
using Microsoft.Extensions.Configuration;


namespace IngBackendApi.Test.Systems.Services
{
    public class TestTokenService
    {

        [Fact]
        public void GenerateToken_ReturnsValidToken()
        {
            // Arrange
            UserInfoDTO userInfo = new()
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Role = Enum.UserRole.Admin
            };

            Dictionary<string, string> initialData = new()
            {
                        {"Secrets:JwtSecretKey", "q5J8hXoNj0lGPMDeQZ-PJCjc99bYc-UeC6NAVEDvt8"},
                        {"Jwt:Issuer", "ing-backend"},
                        {"Jwt:Audience", "ing-user"}
                };
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddInMemoryCollection(initialData)
                .Build();
            TokenService tokenService = new(config);


            // Act
            TokenDTO tokenDto = tokenService.GenerateToken(userInfo);

            // Assert
            Assert.NotNull(tokenDto.AccessToken);
            Assert.True(tokenDto.ExpireAt > DateTime.UtcNow);

            JwtSecurityTokenHandler tokenHandler = new();
            JwtSecurityToken token = tokenHandler.ReadJwtToken(tokenDto.AccessToken);

            // Assert claims
            Assert.Equal(userInfo.Id.ToString(), token.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Equal(userInfo.Email, token.Claims.First(c => c.Type == ClaimTypes.Email).Value);
            Assert.Equal(userInfo.Role.ToString(), token.Claims.First(c => c.Type == ClaimTypes.Role).Value);

            Claim iatClaim = token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Iat);
            DateTimeOffset iat = DateTimeOffset.FromUnixTimeSeconds(long.Parse(iatClaim.Value));
            Assert.True(iat <= DateTime.UtcNow && iat > DateTime.UtcNow.AddSeconds(-10)); // Allow 10 seconds clock skew
        }

    }
}
