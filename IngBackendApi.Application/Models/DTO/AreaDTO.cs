namespace IngBackendApi.Models.DTO;
using System.Text.Json.Serialization;
using IngBackendApi.Enum;
using IngBackendApi.Models.DBEntity;

public class AreaDTO
{
    public Guid Id { get; set; }
    public required int Sequence { get; set; }
    public required bool IsDisplayed { get; set; }
    public required string Title { get; set; }
    public LayoutType? LayoutType { get; set; }
    public int? AreaTypeId { get; set; }
    public TextLayoutDTO? TextLayout { get; set; }
    public ImageTextLayoutDTO? ImageTextLayout { get; set; }
    public ListLayoutDTO? ListLayout { get; set; }
    public KeyValueListLayoutDTO? KeyValueListLayout { get; set; }

    // Relation
    public Guid? ResumeId { get; set; }
    public Guid? RecruitmentId { get; set; }
    public Guid? UserId { get; set; }
}

public class AreaPostDTO
{
    public Guid? Id { get; set; }
    public required int Sequence { get; set; }
    public required bool IsDisplayed { get; set; }
    public required string Title { get; set; }
    public LayoutType? LayoutType { get; set; }
    public int? AreaTypeId { get; set; }

    // relation
    [JsonIgnore]
    public Guid? UserId { get; set; }
    public Guid? ResumeId { get; set; }
    public Guid? RecruitmentId { get; set; }
}

public class AreaTypeDTO
{
    public int? Id { get; set; }
    public required string Name { get; set; }
    public required string Value { get; set; } // unique
    public required string Description { get; set; }
    public required List<UserRole> UserRole { get; set; }
    public required LayoutType LayoutType { get; set; }
    public List<TagTypeDTO>? ListTagTypes { get; set; }
}

public class AreaTypePostDTO
{
    public int? Id { get; set; }
    public required string Name { get; set; }
    public required string Value { get; set; } // unique
    public required string Description { get; set; }
    public required List<UserRole> UserRole { get; set; }
    public required LayoutType LayoutType { get; set; }
    public List<TagTypeDTO>? ListTagTypes { get; set; }
}

public class AreaFormDataDTO
{
    public IFormFile? Image { get; set; }
    public required AreaPostDTO AreaPost { get; set; }
}

public class TextLayoutDTO
{
    public Guid Id { get; set; }
    public required string Content { get; set; } = "";
}

public class ImageDTO
{
    public Guid Id { get; set; }
    public string? Uri { get; set; }
    public required string Filepath { get; set; }
    public required string ContentType { get; set; }
}

public class ImageInfo
{
    public Guid Id { get; set; }
    public string? Uri { get; set; }
    public required string ContentType { get; set; }
}

public class ImageTextLayoutDTO
{
    public Guid Id { get; set; }
    public string AltContent { get; set; } = "";
    public ImageInfo? Image { get; set; }
}

public class ListLayoutDTO
{
    public Guid? Id { get; set; }
    public List<TagDTO>? Items { get; set; }
}

public class KeyValueListLayoutDTO
{
    public Guid Id { get; set; }
    public List<KeyValueItemDTO>? Items { get; set; }
}

public class KeyValueItemDTO
{
    public Guid Id { get; set; }
    public Tag? Key { get; set; }
    public string Value { get; set; } = "";
}


// POST DTO

public class ListLayoutPostDTO
{
    public List<TagPostDTO>? Items { get; set; }
}

public class TextLayoutPostDTO
{
    public required string Content { get; set; } = "";
}

public class ImageTextLayoutPostDTO
{
    public Guid Id { get; set; }
    public string AltContent { get; set; } = "";
    public required IFormFile Image { get; set; }
}