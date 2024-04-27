namespace IngBackendApi.Services.Http;

using System.Text;
using IngBackendApi.Exceptions;
using IngBackendApi.Models.DTO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AiHttpClient : IDisposable
{
    private readonly HttpClient _client;
    private readonly string _generateAreaApi;
    private readonly string _generateAreaByTitleApi;
    private readonly string _keyExtractionApi;

    public AiHttpClient(IConfiguration config)
    {
        var aiApiRoot =
            config.GetSection("AI").GetSection("Root").Get<string>()
            ?? throw new ConfigurationNotFoundException("AI:Root");

        _client = new HttpClient { BaseAddress = new Uri(aiApiRoot) };

        _generateAreaApi =
            config.GetSection("AI").GetSection("GenerateAreaApi").Get<string>()
            ?? throw new ConfigurationNotFoundException("AI:GenerateAreaApi");

        _generateAreaByTitleApi =
            config.GetSection("AI").GetSection("GenerateAreaByTitle").Get<string>()
            ?? throw new ConfigurationNotFoundException("AI:GenerateAreaByTitle");
        _keyExtractionApi =
            config.GetSection("AI").GetSection("KeywordExtractionApi").Get<string>()
            ?? throw new ConfigurationNotFoundException("AI:KeywordExtractionApi");
    }

    public async Task<IEnumerable<AiGeneratedAreaDTO>> PostGenerateAreasAsync(
        object requestBody,
        bool byTitle = false
    )
    {
        var api = byTitle ? _generateAreaByTitleApi : _generateAreaApi;
        var requestContent = ParseJsonContent(requestBody);
        var response = await _client.PostAsync(api, requestContent);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new ServerNetworkException("PostGenerateAreasAsync response failed");
        }
        var jsonString = new StringBuilder(responseContent.Trim("\\\"".ToCharArray()));
        jsonString.Replace("\\", "");

        var generatedArea =
            JsonConvert.DeserializeObject<IEnumerable<AiGeneratedAreaDTO>>(jsonString.ToString())
            ?? throw new JsonParseException("AI response parse failed");

        return generatedArea;
    }

    public async Task<string[]> PostKeyExtractionAsync(string content)
    {
        var requestBody = new Dictionary<string, object>() { ["content"] = content };
        var body = ParseJsonContent(requestBody);
        var response = await _client.PostAsync(_keyExtractionApi, body);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new ServerNetworkException("PostKeyExtractionAsync response failed");
        }

        var jsonArray =
            JArray.Parse(responseContent)
            ?? throw new JsonParseException("AI response parse failed");

        return jsonArray.ToObject<string[]>()
            ?? throw new JsonParseException("AI response parse failed");
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
