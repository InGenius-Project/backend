namespace IngBackendApi.Models.DBEntity;

using System.ComponentModel.DataAnnotations;
using IngBackendApi.Interfaces.Repository;

public class BackgroundTask : BaseEntity, IEntity<string>
{
    [Key]
    public required string Id { get; set; }
    public required string TaskId { get; set; }
}
