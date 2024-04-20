namespace IngBackendApi.Models.DBEntity;

using System.ComponentModel.DataAnnotations;
using IngBackendApi.Interfaces.Repository;

public class BackgroundTask : BaseEntity, IEntity<Guid>
{
    [Key]
    public required Guid Id { get; set; }
    public required string TaskId { get; set; }
}
