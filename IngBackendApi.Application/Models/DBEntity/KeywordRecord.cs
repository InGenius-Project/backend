namespace IngBackendApi.Models.DBEntity;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using IngBackendApi.Interfaces.Repository;

public class KeywordRecord : BaseEntity, IEntity<string>
{
    [Key]
    public required string Id { get; set; }

    [JsonIgnore]
    public ICollection<Recruitment> Recruitments { get; set; } = [];

    [JsonIgnore]
    public ICollection<Resume> Resumes { get; set; } = [];
}
