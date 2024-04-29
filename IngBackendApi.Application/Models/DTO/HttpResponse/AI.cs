namespace IngBackendApi.Models.DTO.HttpResponse;

public class SafetyReportResponse
{
    public bool IsCompanyExist { get; set; }
    public string? AverageSalary { get; set; }
    public required string Content { get; set; }
}

public class SafetyReportPost
{
    public string CompanyName { get; set; }
    public string Content { get; set; }
}
