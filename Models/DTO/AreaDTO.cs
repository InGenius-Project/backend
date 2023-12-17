
namespace IngBackend.Models.DTO;
public class AreaDTO
{
    public Guid Id { get; set; }
    public required int Sequence { get; set; }
    public required bool IsDisplayed { get; set; }
    public TextLayoutDTO? TextLayout { get; set; }
    public ImageTextLayoutDTO? ImageTextLayout { get; set; }
}

public class AreaPostDTO
{
    //public Guid? ProfileId ;
    public Guid? ResumeId { get; set; }
    public required int Sequence { get; set; }
    public required bool IsDisplayed { get; set; }
    public TextLayoutDTO? TextLayout { get; set; }
    public ImageTextLayoutDTO? ImageTextLayout { get; set; }
}


public interface ILayout
{
    string Title { get; set; }
    string Arrangement { get; set; }
    string Type { get; set; }
}

public class TextLayoutDTO
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Arrangement { get; set; }
    public required string Type { get; set; }
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
    public required string Title { get; set; }
    public required string Arrangement { get; set; }
    public required string Type { get; set; }
    public required string Content { get; set; }
    public ImageDTO? Image { get; set; }

}
