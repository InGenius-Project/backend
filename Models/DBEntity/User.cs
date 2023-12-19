﻿using IngBackend.Enum;
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

    public UserRole Role { get; set; }

    [MaxLength(124)]
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string HashedPassword { get; set; }

    [JsonIgnore]
    public List<Resume> Resumes { get; set; } = new List<Resume> { };

    [JsonIgnore]
    public List<Recruitment>? Recruitments { get; set; } = new List<Recruitment> { };
}


public class Recruitment : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }

    public required string JobName { get; set; }
    public required string ContactEmail { get; set; }
    public required string ContactName { get; set; }

    [JsonIgnore]
    public IEnumerable<Resume> Resumes { get; set; } = new List<Resume>() { };

    [Required]
    public required User Publisher { get; set; }
    public required Guid PublisherId { get; set; }
}