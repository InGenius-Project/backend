using IngBackend.Context;
using IngBackend.Interfaces.Repository;
using IngBackend.Models.DBEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace IngBackend.Repository;

public class ResumeRepository : Repository<Resume, Guid>, IResumeRepository
{
    private readonly IngDbContext _context;

    public ResumeRepository(IngDbContext context) : base(context)
    {
        _context = context;
    }

    public IQueryable<Resume> GetResumeByIdIncludeAll(Guid id)
    {
        var query = _context.Resume
            .Where(r => r.Id == id)
            .Include(r => r.Areas)
                .ThenInclude(a => a.LayoutType)
            .Include(r => r.Areas)
                .ThenInclude(a => a.AreaType)
            .Include(r => r.Areas)
                .ThenInclude(a => a.TextLayout)
            .Include(r => r.Areas)
                .ThenInclude(a => a.ImageTextLayout)
            .Include(r => r.Areas)
                .ThenInclude(a => a.ListLayout)
            .Include(r => r.Areas)
                .ThenInclude(a => a.KeyValueListLayout)
            .Include(r => r.User)
            .Include(r => r.Recruitments);
        return query;

    }
}