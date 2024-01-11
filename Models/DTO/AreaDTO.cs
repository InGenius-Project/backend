using IngBackend.Models.DBEntity;

namespace IngBackend.Models.DTO;
public class AreaDTO
{
    public Guid Id { get; set; }
    public required int Sequence { get; set; }
    public required bool IsDisplayed { get; set; }
    public required string Title { get; set; }
    public required string Arrangement { get; set; }
    public required string Type { get; set; }
    public TextLayoutDTO? TextLayout { get; set; }
    public ImageTextLayoutDTO? ImageTextLayout { get; set; }
    public ListLayoutDTO? ListLayout { get; set; }
    public KeyValueListLayoutDTO? KeyValueListLayout { get; set; }

}

public class AreaPostDTO
{
    public required int Sequence { get; set; }
    public required bool IsDisplayed { get; set; }
    public required string Title { get; set; }
    public required string Arrangement { get; set; }
    public required string Type { get; set; }
    public TextLayoutDTO? TextLayout { get; set; }
    public ImageTextLayoutDTO? ImageTextLayout { get; set; }
    public ListLayoutDTO? ListLayout { get; set; }
    public KeyValueListLayoutDTO? KeyValueListLayout { get; set; }
}


public class TextLayoutDTO
{
    public Guid Id { get; set; }
    public required string Content { get; set; } = "";
}


public class ImageDTO
{
    public Guid Id { get; set; }
    public required byte[] Content { get; set; }
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