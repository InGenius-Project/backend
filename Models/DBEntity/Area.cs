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

    public Guid? ResumeId { get; set; }

    [JsonIgnore]
    public Resume? Resume { get; set; }

    public Guid? UserId { get; set; }

    [JsonIgnore]
    public User? User { get; set; }

    public Guid? RecruitmentId { get; set; }

    [JsonIgnore]
    public Recruitment? Recruitment { get; set; }

    [JsonIgnore]
    [ForeignKey("AreaType")]
    public int? AreaTypeId { get; set; }
    public AreaType? AreaType { get; set; } // default area

    public Guid? TextLayoutId { get; set; }

    [JsonIgnore]
    public virtual TextLayout? TextLayout { get; set; }

    public Guid? ImageTextLayoutId { get; set; }

    [JsonIgnore]
    public virtual ImageTextLayout? ImageTextLayout { get; set; }

    public Guid? ListLayoutId { get; set; }

    [JsonIgnore]
    public virtual ListLayout? ListLayout { get; set; }

    public Guid? KeyValueListLayoutId { get; set; }

    [JsonIgnore]
    public virtual KeyValueListLayout? KeyValueListLayout { get; set; }

    public void ClearLayouts()
    {
        KeyValueListLayoutId = null;
        ImageTextLayoutId = null;
        ListLayoutId = null;
        TextLayoutId = null;

        if (KeyValueListLayout != null)
        {
            KeyValueListLayout.AreaId = null;
            KeyValueListLayout.Area = null;
        }
        if (ImageTextLayout != null)
        {
            ImageTextLayout.AreaId = null;
            ImageTextLayout.Area = null;
        }
        if (ListLayout != null)
        {
            ListLayout.AreaId = null;
            ListLayout.Area = null;
        }
        if (TextLayout != null)
        {
            TextLayout.AreaId = null;
            TextLayout.Area = null;
        }
    }
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
    public Guid? AreaId;
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
    public Guid? AreaId;
}

public class ListLayout : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }

    public List<Tag>? Items { get; set; }

    [Required]
    public Guid? AreaId;

    [JsonIgnore]
    [Required]
    public virtual required Area Area { get; set; }
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
    public Guid? AreaId;
}

public class KeyValueItem : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public Tag? Key { get; set; }
    public string Value { get; set; } = "";
}
