namespace IngBackend.Repository;

using IngBackend.Context;
using IngBackend.Interfaces.Repository;
using IngBackend.Models.DBEntity;
using Microsoft.EntityFrameworkCore;

public class TagRepository(IngDbContext context) : Repository<Tag, Guid>(context), ITagRepository
{
    private readonly IngDbContext _context = context;


    public IQueryable<Tag> GetTagsIncludeAll(List<Guid> ids) => _context.Tag
        .Include(t => t.Type)
        .Where(t => ids.Contains(t.Id));

    public IQueryable<Tag> GetTagByType(int typeId) => _context.Tag
        .Include(t => t.Type)
        .Where(t => t.Type.Id == typeId);


    public IQueryable<TagType> GetTagTypes(List<int> ids) => _context.TagType
        .Where(tt => ids.Contains(tt.Id));

}