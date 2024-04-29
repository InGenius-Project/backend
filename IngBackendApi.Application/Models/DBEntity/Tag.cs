using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using IngBackendApi.Interfaces.Repository;

namespace IngBackendApi.Models.DBEntity;

public class Tag : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public int TagTypeId { get; set; }

    [Required]
    public TagType Type { get; set; }

    public int Count { get; set; }

    [JsonIgnore]
    public ICollection<ListLayout> ListLayouts { get; set; } = [];

    [JsonIgnore]
    public ICollection<KeyValueItem> KeyValueItems { get; set; } = [];

    [JsonIgnore]
    public ICollection<User> Owners { get; set; } = [];
}

public class TagType : BaseEntity, IEntity<int>
{
    [Key]
    public int Id { get; set; }

    [StringLength(20)]
    public required string Name { get; set; }

    [StringLength(20)]
    public required string Value { get; set; } // unique
    public required string Color { get; set; }

    [JsonIgnore]
    public ICollection<AreaType> AreaTypes { get; set; } = [];
}
