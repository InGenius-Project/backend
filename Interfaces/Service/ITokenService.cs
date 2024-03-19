namespace IngBackend.Interfaces.Service
{
    using IngBackend.Models.DTO;

    public interface ITokenService
    {
        TokenDTO GenerateToken(UserInfoDTO user);
    }
}