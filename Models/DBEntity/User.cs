using IngBackend.Interfaces.Repository;
using lng_backend.Enum;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace IngBackend.Models.DBEntity;

[Index(nameof(Email), IsUnique = true)]
public class User : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }

    public UserRole Role { get; set; }

    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Username { get; set; }

    [Required]
    public required string HashedPassword { get; set; }

}

public class InternUser : User
{
    public InternUser() {
        Role = UserRole.Intern;
    }

    public List<Resume>? Resumes { get; set; } = new List<Resume> { };
}

public class CompanyUser : User
{
    public CompanyUser() {
        Role = UserRole.Company;
    }
}