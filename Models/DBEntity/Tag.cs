using IngBackend.Interfaces.Repository;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IngBackend.Models.DBEntity;
public class Tag : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }

    [JsonIgnore]
    public List<ListLayout>? ListLayouts { get; set; }
    [JsonIgnore]
    public List<KeyValueItem>? KeyValueItems { get; set; }

}


