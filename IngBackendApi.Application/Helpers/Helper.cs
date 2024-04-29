namespace IngBackendApi.Helpers;

using System.Net.Http;
using System.Text;
using IngBackendApi.Exceptions;
using Newtonsoft.Json;

public static class Helper
{
    public static bool IsInDocker()
    {
        // 檢查環境變數是否存在
        if (Environment.GetEnvironmentVariable("DOCKER_HOST") != null)
        {
            // 容器中存在 DOCKER_HOST 環境變數
            return true;
        }
        else
        {
            // 容器中不存在 DOCKER_HOST 環境變數
            return false;
        }
    }

    public static string GetSAPassword()
    {
        var pw = Environment.GetEnvironmentVariable("SA_PASSWORD");
        if (pw == null)
        {
            return "";
        }

        return pw;
    }

    public static void CheckImage(IFormFile image)
    {
        if (image.ContentType is not "image/jpeg" and not "image/png")
        {
            throw new BadRequestException("Image format must be JPEG or PNG");
        }

        if (image.Length > 10 * 1024 * 1024)
        {
            throw new BadRequestException("Image file size cannot exceed 10MB");
        }
    }

    public static async Task<HttpResponseMessage> SendRequestAsync(
        string url,
        object requestBody,
        string applicationType = "application/json"
    )
    {
        var jsonBody = JsonConvert.SerializeObject(requestBody);
        using var httpClient = new HttpClient();
        var requestContent = new StringContent(jsonBody, Encoding.UTF8, applicationType);
        return await httpClient.PostAsync(url, requestContent);
    }
}
