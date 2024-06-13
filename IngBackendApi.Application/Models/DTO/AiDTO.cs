namespace IngBackendApi.Models.DTO;

using System.ComponentModel.DataAnnotations;
using IngBackendApi.Enum;

public class AreaGenerationDTO
{
    public bool TitleOnly { get; set; }
    public int AreaNum { get; set; } = 5;
    public required string Title { get; set; }
    public required ICollection<UserInfoAreaDTO> Areas { get; set; }
    public required AreaGenType Type { get; set; }
}

public class UserInfoAreaDTO
{
    public required string Title { get; set; }
    public string? Content { get; set; }
}

public class AiGeneratedAreaDTO
{
    public required string AreaTitle { get; set; }
    public string? Content { get; set; }
}

public class GenerateAreaPostDTO
{
    public bool TitleOnly { get; set; }
    public int AreaNum { get; set; } = 5;
    public required string Title { get; set; }
    public required AreaGenType Type { get; set; }
}

public class GenerateAreaByTitleDTO
{
    public required IEnumerable<string> AreaTitles { get; set; }
    public required AreaGenerationDTO UserInfo { get; set; }
}

public class GenerateAreaByTitlePostDTO
{
    public required AreaGenType Type { get; set; }
    public required string Title { get; set; }
    public required ICollection<string> AreaTitles { get; set; }
}
