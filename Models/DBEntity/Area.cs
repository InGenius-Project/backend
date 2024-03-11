using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using IngBackend.Enum;
using IngBackend.Interfaces.Repository;

namespace IngBackend.Models.DBEntity;

public class Area : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }

    public required int Sequence { get; set; }
    public required bool IsDisplayed { get; set; }
    public required string Title { get; set; }
    public LayoutType? LayoutType { get; set; } // custom area

    [JsonIgnore]
    public Resume? Resume { get; set; }

    [JsonIgnore]
    public virtual User? User { get; set; }

    [JsonIgnore]
    public Guid? UserId { get; set; }

    [JsonIgnore]
    public Recruitment? Recruitment { get; set; }

    [JsonIgnore]
    [ForeignKey("AreaType")]
    public int? AreaTypeId { get; set; }
    public AreaType? AreaType { get; set; } // default area

    [JsonIgnore]
    public TextLayout? TextLayout { get; set; }

    [JsonIgnore]
    public ImageTextLayout? ImageTextLayout { get; set; }

    [JsonIgnore]
    public virtual ListLayout? ListLayout { get; set; }

    [JsonIgnore]
    public KeyValueListLayout? KeyValueListLayout { get; set; }
}

public class AreaType : BaseEntity, IEntity<int>
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Value { get; set; } // unique
    public required string Description { get; set; }
    public required List<UserRole> UserRole { get; set; }
    public required LayoutType LayoutType { get; set; }

    [JsonIgnore]
    public List<Area>? Areas { get; set; }

    [JsonIgnore]
    public List<TagType>? ListTagTypes { get; set; }
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
    // TODO: save as file
    [Required]
    public byte[] Data { get; set; }
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

    public virtual List<Tag> Items { get; set; } = new List<Tag>();

    [JsonIgnore]
    // [Required]
    public virtual Area Area { get; set; }

    [ForeignKey("Area")]
    // [Required]
    public Guid AreaId { get; set; }
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
