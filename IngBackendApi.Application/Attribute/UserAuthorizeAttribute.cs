namespace IngBackendApi.Application.Attribute;

using IngBackendApi.Enum;
using Microsoft.AspNetCore.Authorization;

public class UserAuthorizeAttribute : AuthorizeAttribute
{
    public UserAuthorizeAttribute(params UserRole[] roles)
    {
        Roles = string.Join(", ", roles.Select(r => r.ToString()));
    }
}
