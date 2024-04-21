namespace IngBackendApi.Interfaces.Service;

using IngBackendApi.Models.DTO;

public interface IAIService
{
    Task<string[]> GetKeywordsByAIAsync(Guid recruitmentId);
    Task<string[]> GetKeywordsByAIAsync(string content);
    Task SetKeywordsAsync(string[] keywords, Guid recruitmentId);
    Task<IEnumerable<AreaDTO>> GenerateResumeAreaAsync(Guid userId, string resumeTitle);
    Task<IEnumerable<AiGeneratedAreaDTO>> GenerateAreaAsync(object requestBody);
    Task<IEnumerable<AiGeneratedAreaDTO>> GenerateAreaByTitleAsync(object requestBody);
    Task<IEnumerable<AreaDTO>> GenerateResumeAreaByTitleAsync(
        Guid userId,
        string resumeTitle,
        IEnumerable<string> areaTitles
    );
}
