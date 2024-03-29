using IngBackend.Repository;
using IngBackendApi.Context;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Models.DBEntity;
using Microsoft.EntityFrameworkCore;

namespace IngBackendApi.Repository;

public class AreaRepository : Repository<Area, Guid>, IAreaRepository
{
    private readonly IngDbContext _context;

    public AreaRepository(IngDbContext context)
        : base(context)
    {
        _context = context;
    }

    public IQueryable<Area> GetAreaByIdIncludeAll(Guid id) => _context
            .Area.Include(a => a.TextLayout)
            .Include(a => a.ImageTextLayout)
                .ThenInclude(it => it.Image)
            .Include(a => a.ListLayout)
                .ThenInclude(l => l.Items)
                    .ThenInclude(t => t.Type)
            .Include(a => a.KeyValueListLayout)
                .ThenInclude(kv => kv.Items)
            .Include(a => a.AreaType)
            .Where(a => a.Id == id);

    public Area GetAreaByIdIncludeAllLayout(Guid id) => _context
            .Area.Include(a => a.TextLayout)
            .Include(a => a.ImageTextLayout)
                .ThenInclude(it => it.Image)
            .Include(a => a.ListLayout)
                .ThenInclude(l => l.Items)
            .Include(a => a.KeyValueListLayout)
                .ThenInclude(kv => kv.Items)
            .FirstOrDefault(a => a.Id == id);
}

