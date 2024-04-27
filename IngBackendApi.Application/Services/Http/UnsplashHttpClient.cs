namespace IngBackendApi.Services.Http;

using IngBackendApi.Models.Settings;

public class UnsplashHttpClient : IDisposable
{
    private readonly HttpClient _client;
    private readonly UnsplashSetting _setting;

    public UnsplashHttpClient(SettingsFactory settingsFactory)
    {
        _setting = settingsFactory.GetSetting<UnsplashSetting>();
        _client = new HttpClient() { BaseAddress = new Uri(_setting.Api.Root) };
    }

    public async Task PostSearchAsync()
    {
        throw new NotImplementedException();
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
