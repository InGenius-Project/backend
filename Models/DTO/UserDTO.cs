namespace IngBackend.Models.DTO;

public class UserDTO
{
    public UserInfoDTO User { get; set; }
}

public class UserInfoDTO
{
    public Guid Id { get; set; }

    public string Email { get; set; }

    public string Username { get; set; }

    public string HashedPassword { get; set; }

    public List<ResumeDTO>? Resumes { get; set; }
}

public class UserSignUpDTO
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}
public class UserSignInDTO
{
    public string Email { get; set; }
    public string Password { get; set; }
}

