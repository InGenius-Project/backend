namespace IngBackend.Models.DTO;

public class ResumeDTO
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<AreaDTO>? Areas { get; set; }

}
public class ResumePostDTO
{
    public Guid? Id { get; set; } // Optional id provide
    public string? Title { get; set; }

    public List<AreaDTO>? Areas { get; set; }

}
