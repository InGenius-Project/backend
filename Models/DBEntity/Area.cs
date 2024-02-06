using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using IngBackend.Interfaces.Repository;

namespace IngBackend.Models.DBEntity;

public class Area : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }

    public required int Sequence { get; set; }
    public required bool IsDisplayed { get; set; }

    [JsonIgnore]
    public Resume? Resume { get; set; }

    [JsonIgnore]
    public User? User { get; set; }
    public Guid UserId { get; set; }

    public required string Title { get; set; }
    public required string Arrangement { get; set; }
    public required string Type { get; set; }

    [JsonIgnore]
    public TextLayout? TextLayout { get; set; }

    [JsonIgnore]
    public ImageTextLayout? ImageTextLayout { get; set; }

    [JsonIgnore]
    public ListLayout? ListLayout { get; set; }

    [JsonIgnore]
    public KeyValueListLayout? KeyValueListLayout { get; set; }
}

public class TextLayout : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
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
    public required string Filename { get; set; }
    public required string ContentType { get; set; }

    // Save as base64
    public required byte[] Data { get; set; }
}

public class ImageTextLayout : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public string Content { get; set; } = "";
    public Image? Image { get; set; }

    [Required]
    [JsonIgnore]
    public required Area Area { get; set; }

    [ForeignKey("Area")]
    [Required]
    public Guid AreaId;
}

public class ListLayout : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }

    public List<Tag>? Items { get; set; }

    [JsonIgnore]
    [Required]
    public required Area Area { get; set; }

    [ForeignKey("Area")]
    [Required]
    public Guid AreaId;
}

public class KeyValueListLayout : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public List<KeyValueItem>? Items { get; set; }

    [JsonIgnore]
    [Required]
    public required Area Area { get; set; }

    [ForeignKey("Area")]
    [Required]
    public Guid AreaId;
}

public class KeyValueItem : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public Tag? Key { get; set; }
    public string Value { get; set; } = "";
}
