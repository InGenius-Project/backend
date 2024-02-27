namespace IngBackend.Interfaces.Repository;

using IngBackend.Models.DBEntity;

public interface ITagRepository : IRepository<Tag, Guid>
{
    IQueryable<Tag> GetTagsIncludeAll(List<Guid> ids);
    IQueryable<TagType> GetTagTypes(List<int> ids);
    IQueryable<Tag> GetTagByType(int typeId);
}