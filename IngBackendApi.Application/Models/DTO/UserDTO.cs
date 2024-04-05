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
    public ImageDTO Avatar { get; set; }
}

public class UserInfoDTO
{
    public Guid Id { get; set; }

    public string Email { get; set; }
    public bool Verified { get; set; }

    public string Username { get; set; }
    public ImageDTO Avatar { get; set; }
    public UserRole Role { get; set; }
    public List<AreaDTO> Areas { get; set; }
    public List<TagDTO>? Tags { get; set; }

    public List<ResumeDTO>? Resumes { get; set; }
    public List<RecruitmentDTO>? Recruitments { get; set; }
}

public class UserInfoPostDTO
{
    public ImageDTO? Avatar { get; set; }
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
