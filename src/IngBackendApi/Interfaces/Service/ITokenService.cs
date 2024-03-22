namespace IngBackendApi.Interfaces.Service
{
    using IngBackendApi.Models.DTO;

    public interface ITokenService
    {
        TokenDTO GenerateToken(UserInfoDTO user);
    }
}