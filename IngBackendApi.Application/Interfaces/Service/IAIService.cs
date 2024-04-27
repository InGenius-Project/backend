namespace IngBackendApi.Interfaces.Service;

using IngBackendApi.Models.DTO;

public interface IAIService
{
    Task<string[]> GetKeywordsByAIAsync(Guid recruitmentId);
    Task SetKeywordsAsync(string[] keywords, Guid recruitmentId);
    Task<IEnumerable<AreaDTO>> GenerateResumeAreaAsync(
        Guid userId,
        string resumeTitle,
        int areaNum = 5,
        bool titleOnly = false
    );
    Task<IEnumerable<AreaDTO>> GenerateResumeAreaByTitleAsync(
        Guid userId,
        string resumeTitle,
        IEnumerable<string> areaTitles
    );
}
