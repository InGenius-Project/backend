namespace IngBackendApi.Models.DTO;

using System.ComponentModel.DataAnnotations;
using IngBackendApi.Models.DBEntity;

public class RecruitmentDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool Enable { get; set; }
    public List<AreaDTO> Areas { get; set; }
    public IEnumerable<ResumeDTO> Resumes { get; set; }
    public OwnerUserDTO Publisher { get; set; }
    public Guid PublisherId { get; set; }
    public bool IsUserFav { get; set; }
    public List<KeywordRecord> Keywords { get; set; }
}

public class RecruitmentPostDTO
{
    public Guid? Id { get; set; }
    public string Name { get; set; }
    public bool Enable { get; set; }
}

public class RecruitmentSearchPostDTO
{
    public string? Query { get; set; }
    public List<Guid>? TagIds { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "CreatedTime";
    public string OrderBy { get; set; } = "desc";
}

public class RecruitmentSearchResultDTO
{
    public string? Query { get; set; }
    public List<Guid>? TagIds { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int MaxPage { get; set; }
    public int Total { get; set; }
    public string SortBy { get; set; } = "CreatedTime";
    public string OrderBy { get; set; } = "desc";
    public List<RecruitmentDTO> result { get; set; } = [];
}
