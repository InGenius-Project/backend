namespace IngBackendApi.Interfaces.Models;

public interface ISetting
{
    string Name { get; set; }
    DateTime LoadTime { get; }
}
