using IngBackend.Interfaces.Repository;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IngBackend.Models.DBEntity;


public class Resume : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(50)]
    public required string Title { get; set; }

    public List<ResumeArea<TextLayout>>? TextLayouts { get; set; }
    public List<ResumeArea<ImageTextLayout>>? ImageTextLayouts { get; set; }

    [JsonIgnore]
    [Required]
    public User User { get; set; }
    public Guid UserId { get; set; }

}
public class ResumeArea<TLayout> : BaseEntity, IEntity<Guid>
    where TLayout : ILayout
{
    [Key]
    public Guid Id { get; set; }
    public Guid ResumeId { get; set; }

    public required TLayout Layout { get; set; }
    public required int Sequence { get; set; }

    [JsonIgnore]
    [Required]
    public required Resume Resume { get; set; }
}


public interface ILayout
{
    string Title { get; set; }
    string Name { get; set; }
    string Type { get; set; }
}

public class Image : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public required byte[] Content { get; set; }
}

public class TextLayout : BaseEntity, ILayout, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Name { get; set; } = "文字";
    public string Type { get; set; } = "custom";
    public string Content { get; set; } = "";
}

public class ImageTextLayout : BaseEntity, ILayout, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Name { get; set; } = "圖片文字";
    public string Type { get; set; } = "custom";
    public string Content { get; set; } = "";
    public Image? Image { get; set; }
}
