namespace IngBackendApi.Models.Settings;

public class UnsplashSetting : Setting
{
    public class AuthArea
    {
        public string ClientId { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }

    public class ApiArea
    {
        public string Root { get; set; }
        public string Search { get; set; }
    }

    public new string Name { get; set; } = "Unsplash Setting";
    public AuthArea Auth { get; set; }
    public ApiArea Api { get; set; }
}
