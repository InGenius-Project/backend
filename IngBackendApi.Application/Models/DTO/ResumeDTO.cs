namespace IngBackendApi.Models.DTO;

using IngBackendApi.Models.DBEntity;

public class ResumeDTO
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<AreaDTO>? Areas { get; set; }
    public bool Visibility { get; set; }
    public required UserInfoDTO User { get; set; }
    public IEnumerable<RecruitmentDTO> Recruitments { get; set; } = [];
    public IEnumerable<KeywordRecord> Keywords { get; set; } = [];
}

public class ResumePostDTO
{
    public Guid? Id { get; set; }
    public string? Title { get; set; }
    public bool Visibility { get; set; }
}
