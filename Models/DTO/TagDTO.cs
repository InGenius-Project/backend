namespace IngBackend.Models.DTO;

public class TagPostDTO
{
    public Guid? Id { get; set; }
    public string Name { get; set; }
    public int TagTypeId { get; set; }
}

public class TagTypePostDTO
{
    public int? Id { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
    public string Color { get; set; }
}

public class TagDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int TagTypeId { get; set; }
    public TagTypeDTO Type { get; set; }
}

public class TagTypeDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
    public string Color { get; set; }
}
