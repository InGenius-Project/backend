﻿using IngBackend.Interfaces.Repository;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IngBackend.Models.DBEntity;
public class Resume : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(50)]
    public required string Title { get; set; }

    public List<Area>? Areas { get; set; }

    [JsonIgnore]
    [Required]
    public required User User { get; set; }
    public Guid UserId { get; set; }

}


