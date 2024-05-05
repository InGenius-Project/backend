namespace IngBackendApi.Models.DTO;

using IngBackendApi.Enum;

public class UserDTO
{
    public UserInfoDTO User { get; set; }
    public TokenDTO Token { get; set; }
}

public class OwnerUserDTO
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public ImageDTO? Avatar { get; set; }
    public ImageDTO? BackgroundImage { get; set; }
}

public class UserInfoDTO
{
    public Guid Id { get; set; }

    public string Email { get; set; }
    public bool Verified { get; set; }
    public bool Premium { get; set; }

    public string Username { get; set; }
    public ImageDTO? Avatar { get; set; }
    public ImageDTO? BackgroundImage { get; set; }

    public UserRole Role { get; set; }
    public List<AreaDTO> Areas { get; set; }
    public List<TagDTO>? Tags { get; set; }
}

public class ConnectionDTO
{
    public string ConnectionId { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public bool Connected { get; set; }
}

public class UserInfoPostDTO
{
    public string? Username { get; set; }
    public List<TagDTO>? Tags { get; set; }
}

public class UserSignUpDTO
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public UserRole Role { get; set; }
}

public class UserSignInDTO
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class ChatDTO
{
    public string Message { get; set; } = "";
}
