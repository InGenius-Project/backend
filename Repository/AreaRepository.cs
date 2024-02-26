namespace IngBackend.Repository;

using IngBackend.Context;
using IngBackend.Interfaces.Repository;
using IngBackend.Models.DBEntity;
using Microsoft.EntityFrameworkCore;

public class AreaRepository(IngDbContext context) : Repository<Area, Guid>(context), IAreaRepository
{
    private readonly IngDbContext _context = context;

    public IQueryable<Area> GetAreaByIdIncludeAll(Guid id) => _context.Area
            .Include(a => a.TextLayout)
            .Include(a => a.ImageTextLayout)
                .ThenInclude(it => it.Image)
            .Include(a => a.ListLayout)
                .ThenInclude(l => l.Items)
             .Include(a => a.KeyValueListLayout)
                .ThenInclude(kv => kv.Items)
            .Include(a => a.AreaType)
            .Where(a => a.Id == id);

    public IQueryable<Area> GetAreaByIdIncludeUser(Guid id) => _context.Area.Include(a => a.User).Where(a => a.Id == id);





}