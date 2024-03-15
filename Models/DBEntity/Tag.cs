using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using IngBackend.Interfaces.Repository;

namespace IngBackend.Models.DBEntity;

public class Tag : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public int TagTypeId { get; set; }
    public required TagType Type { get; set; }

    public int Count { get; set; }

    [JsonIgnore]
    public List<ListLayout>? ListLayouts { get; set; }

    [JsonIgnore]
    public List<KeyValueItem>? KeyValueItems { get; set; }

    [JsonIgnore]
    public List<User>? User { get; set; }
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
    public List<AreaType>? AreaTypes { get; set; }
}
