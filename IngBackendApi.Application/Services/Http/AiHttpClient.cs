namespace IngBackendApi.Services.Http;

using System.Text;
using IngBackendApi.Exceptions;
using IngBackendApi.Models.DTO;
using IngBackendApi.Models.DTO.HttpResponse;
using IngBackendApi.Models.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AiHttpClient : IDisposable
{
    private readonly HttpClient _client;
    private readonly AiSetting _setting;

    public AiHttpClient(SettingsFactory settingsFactory)
    {
        _setting = settingsFactory.GetSetting<AiSetting>();
        // Apis
        _client = new HttpClient { BaseAddress = new Uri(_setting.Api.Root) };
    }

    public async Task<IEnumerable<AiGeneratedAreaDTO>> PostGenerateAreasAsync(
        object requestBody,
        bool byTitle = false
    )
    {
        var api = byTitle ? _setting.Api.GenerateAreaByTitle : _setting.Api.GenerateArea;
        var requestContent = ParseJsonContent(requestBody);
        var response = await _client.PostAsync(api, requestContent);
        if (!response.IsSuccessStatusCode)
        {
            throw new ServerNetworkException("PostGenerateAreasAsync response failed");
        }
        var responseContent = await response.Content.ReadFromJsonAsync<List<AiGeneratedAreaDTO>>();
        return responseContent ?? [];
    }

    public async Task<string[]> PostKeyExtractionAsync(string content)
    {
        var requestBody = new Dictionary<string, string>() { ["content"] = content };
        var body = ParseJsonContent(requestBody);
        var response = await _client.PostAsync(_setting.Api.KeywordExtraction, body);
        var stringArray = await response.Content.ReadFromJsonAsync<string[]>();
        return stringArray ?? [];
    }

    public async Task<SafetyReportResponse> PostSaftyReportAsync(SafetyReportPost safetyReportPost)
    {
        var body = ParseJsonContent(safetyReportPost);
        var response = await _client.PostAsync(_setting.Api.AnalysisRecruitment, body);
        var report =
            await response.Content.ReadFromJsonAsync<SafetyReportResponse>()
            ?? throw new ServerNetworkException("PostSaftyReportAsync response failed");
        return report;
    }

    private static StringContent ParseJsonContent(object body)
    {
        var jsonBody = JsonConvert.SerializeObject(body);
        var requestContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        return requestContent;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _client.Dispose();
        }
    }
}
