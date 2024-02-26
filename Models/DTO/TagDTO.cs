namespace IngBackend.Models.DTO;

public class TagDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public TagTypeDTO Type { get; set; } = new TagTypeDTO();
}


public class TagTypeDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
    public string Color { get; set; }
}