namespace IngBackend.Interfaces.Service
{
    using IngBackend.Models.DBEntity;
    using IngBackend.Models.DTO;

    public interface ITagService : IService<Tag, TagDTO, Guid>
    {
        Task<List<TagDTO>?> GetAllTagsByType(string? type);
    }

}
