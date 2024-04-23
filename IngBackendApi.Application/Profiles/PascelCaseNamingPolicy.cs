namespace IngBackendApi.Profiles;

using System.Text.Json;

public class PascelCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => name.ToUpper();
}
