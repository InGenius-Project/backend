namespace IngBackend.Models.DBEntity;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using IngBackend.Interfaces.Repository;

public class Tag : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public required string Name { get; set; }

    [Required]
    public virtual TagType Type { get; set; }

    [Required]
    [ForeignKey("TagType")]
    public int TypeId { get; set; }

    public int Count { get; set; }

    [JsonIgnore]
    public virtual List<ListLayout>? ListLayouts { get; set; }

    [JsonIgnore]
    public virtual List<KeyValueItem>? KeyValueItems { get; set; }

    [JsonIgnore]
    public virtual List<User>? User { get; set; }
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
}
