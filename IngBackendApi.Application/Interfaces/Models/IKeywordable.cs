namespace IngBackendApi.Interfaces.Models;

using IngBackendApi.Models.DBEntity;

public interface IKeywordable
{
    ICollection<KeywordRecord> Keywords { get; set; }
}
