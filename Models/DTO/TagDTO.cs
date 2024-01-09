namespace IngBackend.Models.DTO;

public class TagDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "CUSTOM";
}