namespace IngBackendApi.Interfaces.Service;

using IngBackendApi.Models.DTO;

public interface IAIService
{
    Task<string[]> GetKeywordsByAIAsync(Guid recruitmentId);
    Task<string[]> GetKeywordsByAIAsync(string content);
    Task SetKeywordsAsync(string[] keywords, Guid recruitmentId);
    Task<string> GenerateResumeArea(Guid userId);
    Task<List<AiGeneratedAreaDTO>> GenerateArea(string content);
}
