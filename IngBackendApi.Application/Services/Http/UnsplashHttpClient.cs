namespace IngBackendApi.Services.Http;

using System.Net.Http.Headers;
using IngBackendApi.Exceptions;
using IngBackendApi.Models.DTO.HttpResponse;
using IngBackendApi.Models.Settings;

public class UnsplashHttpClient : IDisposable
{
    private readonly HttpClient _client;
    private readonly UnsplashSetting _setting;

    public UnsplashHttpClient(SettingsFactory settingsFactory)
    {
        _setting = settingsFactory.GetSetting<UnsplashSetting>();
        _client = new HttpClient() { BaseAddress = new Uri(_setting.Api.Root) };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Client-ID",
            _setting.Auth.AccessKey
        );
    }

    public async Task<UnsplashResponse> GetSearchAsync(string[] query)
    {
        var queryString = string.Join("+", query);
        queryString = queryString.Replace(" ", "+");
        var parameterString = $"?query={queryString}&";
        var unsplashResponse =
            await _client.GetFromJsonAsync<UnsplashResponse>(_setting.Api.Search + parameterString)
            ?? throw new ServerNetworkException("Parse failed or Unsplash api cannot be reached.");

        return unsplashResponse;
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
