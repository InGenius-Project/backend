using IngBackend.Enum;
using IngBackend.Models.DBEntity;

namespace IngBackend.Models.DTO;
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

}


// TODO: May change this class.
// By Danny
public class AreaPostDTO
{
    public required Guid Id { get; set; }
    public required int Sequence { get; set; }
    public required bool IsDisplayed { get; set; }
    public required string Title { get; set; }
    public LayoutType? Arrangement { get; set; }
    public int? AreaTypeId { get; set; }
    public TextLayoutDTO? TextLayout { get; set; }
    public ImageTextLayoutDTO? ImageTextLayout { get; set; }
    public ListLayoutDTO? ListLayout { get; set; }
    public KeyValueListLayoutDTO? KeyValueListLayout { get; set; }
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

public class AreaTypePostDTO : AreaTypeDTO
{
    public List<int> ListTagTypeIds { get; set; } = new List<int>();
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
    public required string Filename { get; set; }
    public required string ContentType { get; set; }
    public required string Content { get; set; }
}

public class ImageTextLayoutDTO
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public ImageDTO? Image { get; set; }
}


public class ListLayoutDTO
{
    public Guid Id { get; set; }
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