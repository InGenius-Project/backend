using IngBackend.Context;
using IngBackend.Interfaces.Repository;
using IngBackend.Models.DBEntity;
using Microsoft.EntityFrameworkCore;

namespace IngBackend.Repository;

public class RecruitmentRepository : Repository<Recruitment, Guid>, IRecruitmentRepository
{
    private readonly IngDbContext _context;

    public RecruitmentRepository(IngDbContext context) : base(context)
    {
        _context = context;
    }

    public IQueryable<Recruitment> GetRecruitmentByIdIncludeAll(Guid id)
    {
        return _context.Recruitment
            .Include(r => r.Areas)
                .ThenInclude(a => a.TextLayout)
            .Include(r => r.Areas)
                .ThenInclude(a => a.ImageTextLayout)
            .Include(r => r.Areas)
                .ThenInclude(a => a.ListLayout)
                    .ThenInclude(l => l.Items)
            .Include(r => r.Areas)
                .ThenInclude(a => a.KeyValueListLayout)
                    .ThenInclude(kv => kv.Items)
                        .ThenInclude(kvi => kvi.Key)
            .Where(x => x.Id.Equals(id));

    }



}