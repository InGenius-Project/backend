namespace IngBackendApi.Models.DTO;

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

public class UserResumeGenerationDTO
{
    public required string ResumeTitle { get; set; }
    public required ICollection<UserInfoAreaDTO> Areas { get; set; }
}

public class UserInfoAreaDTO
{
    public required string Title { get; set; }
    public string? Content { get; set; }
}

public class AiGeneratedAreaDTO
{
    public required string AreaTitle { get; set; }
    public required string Content { get; set; }
}

public class GenerateAreaPostDTO
{
    public required string Title { get; set; }

    [RegularExpression(
        "^(resume|recruitment)$",
        ErrorMessage = "Type must be 'resume' or 'recruitment'."
    )]
    public required string Type { get; set; }
}

public class GenerateAreaByTitleDTO
{
    public required IEnumerable<string> AreaTitles { get; set; }
    public required UserResumeGenerationDTO UserResumeInfo { get; set; }
}

public class GenerateAreaByTitlePostDTO
{
    public required string ResumeTitle { get; set; }
    public required ICollection<string> AreaTitles { get; set; }
}
