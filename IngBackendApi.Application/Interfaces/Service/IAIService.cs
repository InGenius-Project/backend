namespace IngBackendApi.Interfaces.Service;

public interface IAIService
{
    Task<string[]> GetKeywordsByAIAsync(Guid recruitmentId);
    Task<string[]> GetKeywordsByAIAsync(string content);
    Task SetKeywordsAsync(string[] keywords, Guid recruitmentId);
}
