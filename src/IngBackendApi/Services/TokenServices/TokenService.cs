namespace IngBackendApi.Services.TokenServices;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DTO;
using Microsoft.IdentityModel.Tokens;

public class TokenService(IConfiguration config) : ITokenService
{
    private readonly IConfiguration _config = config;

    public virtual TokenDTO GenerateToken(UserInfoDTO user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(
                JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
            )
        };

        var expiresIn = 7;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Secrets:JwtSecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddDays(expiresIn);
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );
        return new TokenDTO
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpireAt = expires
        };
    }
}
