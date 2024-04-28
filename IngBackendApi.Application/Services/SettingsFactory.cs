namespace IngBackendApi.Services;

using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Models;
using IngBackendApi.Models.Settings;

public class SettingsFactory(IConfiguration configuration)
{
    private readonly IConfiguration _config = configuration;

    private string GetConfig(string[] pathnames)
    {
        if (pathnames.Length == 0)
        {
            throw new ArgumentException("Length of pathnames cannot be 0.");
        }
        var query = _config.GetRequiredSection(pathnames[0]);
        for (var i = 1; i < pathnames.Length; i++)
        {
            query = query.GetRequiredSection(pathnames[i]);
        }
        return query.Get<string>() ?? throw new ConfigurationNotFoundException();
    }

    public T GetSetting<T>()
        where T : ISetting
    {
        if (typeof(T) == typeof(AiSetting))
        {
            return (T)
                (object)
                    new AiSetting()
                    {
                        Api = new AiSetting.ApiArea
                        {
                            Root = GetConfig(["AI", "Api", "Root"]),
                            KeywordExtraction = GetConfig(["AI", "Api", "KeywordExtraction"]),
                            GenerateArea = GetConfig(["AI", "Api", "GenerateArea"]),
                            GenerateAreaByTitle = GetConfig(["AI", "Api", "GenerateAreaByTitle"]),
                            AnalysisRecruitment = GetConfig(["AI", "Api", "AnalysisRecruitment"])
                        }
                    };
        }
        else if (typeof(T) == typeof(UnsplashSetting))
        {
            return (T)
                (object)
                    new UnsplashSetting()
                    {
                        Auth = new UnsplashSetting.AuthArea
                        {
                            ClientId = GetConfig(["Unsplash", "Auth", "ClientId"]),
                            AccessKey = GetConfig(["Unsplash", "Auth", "AccessKey"]),
                            SecretKey = GetConfig(["Unsplash", "Auth", "SecretKey"]),
                        },
                        Api = new UnsplashSetting.ApiArea
                        {
                            Root = GetConfig(["Unsplash", "Api", "Root"]),
                            Search = GetConfig(["Unsplash", "Api", "Search"]),
                        }
                    };
        }
        else if (typeof(T) == typeof(PathSetting))
        {
            return (T)
                (object)
                    new PathSetting()
                    {
                        Image = new PathSetting.ImageArea
                        {
                            Avatar = GetConfig(["Path", "Image", "Avatar"]),
                            Area = GetConfig(["Path", "Image", "Area"]),
                        },
                    };
        }
        throw new NotFoundException($"Setting type {typeof(T)} not found.");
    }
}
