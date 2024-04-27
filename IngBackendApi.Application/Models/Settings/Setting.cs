namespace IngBackendApi.Models.Settings;

using IngBackendApi.Interfaces.Models;

public class Setting : ISetting
{
    public string Name { get; set; } = "Setting Name";
    public DateTime LoadTime { get; } = DateTime.Now;
}
