namespace IngBackendApi.Models.DBEntity;

using IngBackendApi.Interfaces.Repository;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Resume : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(50)]
    public required string Title { get; set; }

    public bool Visibility { get; set; }

    [JsonIgnore]
    public List<Area> Areas { get; set; } = [];

    [JsonIgnore]
    [Required]
    public required User User { get; set; }
    public Guid UserId { get; set; }


    // Related Recruitment
    [JsonIgnore]
    public List<Recruitment> Recruitments { get; set; } = [];
}


