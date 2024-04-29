namespace IngBackendApi.Interfaces.Service;

using IngBackendApi.Interfaces.Models;

public interface ISettingsFactory
{
    T GetSetting<T>()
        where T : ISetting;
}
