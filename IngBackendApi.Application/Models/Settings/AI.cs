namespace IngBackendApi.Models.Settings;

public class AiSetting : Setting
{
    public class ApiArea
    {
        public string Root { get; set; }
        public string KeywordExtraction { get; set; }
        public string GenerateArea { get; set; }
        public string GenerateAreaByTitle { get; set; }
    }

    public new string Name { get; set; } = "Ai Settings";
    public ApiArea Api { get; set; }
}
