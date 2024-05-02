namespace IngBackendApi.Models.DBEntity;

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using IngBackendApi.Interfaces.Repository;

public class Recruitment : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }

    public string Name { get; set; }

    public bool Enable { get; set; }

    [JsonIgnore]
    public List<Area> Areas { get; set; }

    [JsonIgnore]
    public List<Resume> Resumes { get; set; } = [];

    [JsonIgnore]
    [Required]
    public User Publisher { get; set; }
    public Guid PublisherId { get; set; }

    [JsonIgnore]
    public List<User> FavoriteUsers { get; set; } = [];

    [JsonIgnore]
    public ICollection<KeywordRecord> Keywords { get; set; } = [];

    [JsonIgnore]
    public SafetyReport? SafetyReport { get; set; }
    public Guid? SafetyReportId { get; set; }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(CultureInfo.GetCultureInfo("zh-TW"), $"標題: {Name}");
        Areas.ForEach(a => stringBuilder.AppendLine(a.ToString()));
        return stringBuilder.ToString();
    }
}

public class SafetyReport : BaseEntity, IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public bool IsCompanyExist { get; set; }
    public string? AverageSalary { get; set; }
    public required string Content { get; set; }

    [JsonIgnore]
    [Required]
    public Recruitment? Recruitment { get; set; }
    public Guid RecruitmentId { get; set; }
}
