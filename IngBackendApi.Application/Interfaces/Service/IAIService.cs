namespace IngBackendApi.Interfaces.Service;

using IngBackendApi.Enum;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;

public interface IAIService
{
    Task<string[]> GetKeywordsByAIAsync(Guid id, AreaGenType type);
    Task SetKeywordsAsync(string[] keywords, Guid id, AreaGenType type);
    Task<IEnumerable<AreaDTO>> GenerateAreaAsync(
        Guid userId,
        string title,
        AreaGenType genType,
        int areaNum = 5,
        bool titleOnly = false
    );
    Task<IEnumerable<AreaDTO>> GenerateAreaByTitleAsync(
        Guid userId,
        string title,
        IEnumerable<string> areaTitles,
        AreaGenType genType
    );

    Task<SafetyReport> GenerateSafetyReportAsync(Guid recruitmentId);
    Task SaveSafetyReportAsync(SafetyReport safetyReport);
}
