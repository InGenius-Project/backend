using IngBackend.Interfaces.Repository;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IngBackend.Models.DBEntity;

public class Area : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }

    public required int Sequence { get; set; }
    public required bool IsDisplayed { get; set; }


    [JsonIgnore]
    public Resume? Resume { get; set; }
    public Guid ResumeId { get; set; }
    [JsonIgnore]
    public TextLayout? TextLayout { get; set; }
    [JsonIgnore]
    public ImageTextLayout? ImageTextLayout { get; set; }
}

public interface ILayout
{
    string Title { get; set; }
    string Arrangement { get; set; }
    string Type { get; set; }
}

public class TextLayout : BaseEntity, ILayout, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Arrangement { get; set; } = "TEXT";
    public string Type { get; set; } = "CUSTOM";
    public string Content { get; set; } = "";

    [JsonIgnore]
    [Required]
    public required Area Area { get; set; }
    [ForeignKey("Area")]
    [Required]
    public Guid AreaId;
}


public class Image : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public required byte[] Content { get; set; }
}

public class ImageTextLayout : BaseEntity, ILayout, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Arrangement { get; set; } = "IMAGETEXT";
    public string Type { get; set; } = "CUSTOM";
    public string Content { get; set; } = "";
    public Image? Image { get; set; }

    [Required]
    [JsonIgnore]
    public required Area Area { get; set; }
    [ForeignKey("Area")]
    [Required]
    public Guid AreaId;
}