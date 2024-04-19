namespace IngBackendApi.Repository;

using IngBackendApi.Context;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Models.DBEntity;
using Microsoft.EntityFrameworkCore;

public class ResumeRepository(IngDbContext context)
    : Repository<Resume, Guid>(context),
        IResumeRepository
{
    private readonly IngDbContext _context = context;

    public IQueryable<Resume> GetIncludeAll() =>
        _context
            .Resume.Include(r => r.Areas)
            .Include(r => r.Areas)
            .ThenInclude(a => a.AreaType)
            .Include(r => r.Areas)
            .ThenInclude(a => a.TextLayout)
            .Include(r => r.Areas)
            .ThenInclude(a => a.ImageTextLayout)
            .Include(r => r.Areas)
            .ThenInclude(a => a.ListLayout)
            .ThenInclude(l => l.Items)
            .Include(r => r.Areas)
            .ThenInclude(a => a.KeyValueListLayout)
            .Include(r => r.User)
            .Include(r => r.Recruitments);
}
