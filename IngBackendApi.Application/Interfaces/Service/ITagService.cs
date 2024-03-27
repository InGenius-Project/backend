namespace IngBackendApi.Interfaces.Service
{
    using IngBackendApi.Models.DBEntity;
    using IngBackendApi.Models.DTO;

    public interface ITagService : IService<Tag, TagDTO, Guid>
    {
        Task<List<TagDTO>?> GetAllTagsByType(string[]? type);
    }

}
