using IngBackend.Enum;
using IngBackend.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace IngBackend.Models.DBEntity;

[Index(nameof(Email), IsUnique = true)]
public class User : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }

    [Column(TypeName = "varchar(512)")]
    [Required]
    public required string Email { get; set; }

    public bool Verified { get; set; } = false;

    public UserRole Role { get; set; }

    public Image? Avatar { get; set; }

    [MaxLength(124)]
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string HashedPassword { get; set; }

    [JsonIgnore]
    public List<Tag> Tags { get; set; } = new List<Tag> { };

    [JsonIgnore]
    public List<Area> Areas { get; set; } = new List<Area> { };
    [JsonIgnore]
    public List<Resume> Resumes { get; set; } = new List<Resume> { };

    [JsonIgnore]
    public List<Recruitment>? Recruitments { get; set; } = new List<Recruitment> { };

    public List<VerificationCode>? EmailVerifications { get; set; }
}


public class Recruitment : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(15)]
    public string Name { get; set; }

    public bool Enable { get; set; }

    [JsonIgnore]
    public List<Area> Areas { get; set; }

    [JsonIgnore]
    public IEnumerable<Resume> Resumes { get; set; } = new List<Resume>() { };

    [Required]
    public required User Publisher { get; set; }
    public required Guid PublisherId { get; set; }
}

public class VerificationCode : BaseEntity, IEntity<int>
{
    [Key]

    public int Id { get; set; }
    public Guid UserId { get; set; }
    [Required]
    public User User { get; set; }

    [Column(TypeName = "char(6)")]
    public string Code { get; set; }
    public DateTime ExpiresTime { get; set; }
}
