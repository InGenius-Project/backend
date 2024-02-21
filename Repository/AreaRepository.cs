using IngBackend.Context;
using IngBackend.Interfaces.Repository;
using IngBackend.Models.DBEntity;
using Microsoft.EntityFrameworkCore;

namespace IngBackend.Repository;

public class AreaRepository : Repository<Area, Guid>, IAreaRepository
{
    private readonly IngDbContext _context;

    public AreaRepository(IngDbContext context) : base(context)
    {
        _context = context;
    }
    public IQueryable<Area> GetAreaByIdIncludeAll(Guid id)
    {
        return _context.Area
            .Include(a => a.TextLayout)
            .Include(a => a.ImageTextLayout)
                .ThenInclude(it => it.Image)
            .Include(a => a.ListLayout)
                .ThenInclude(l => l.Items)
             .Include(a => a.KeyValueListLayout)
                .ThenInclude(kv => kv.Items)
            .Include(a => a.AreaType)
            .Where(a => a.Id == id);
    }




}