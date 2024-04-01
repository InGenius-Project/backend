using IngBackend.Repository;
using IngBackendApi.Context;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Models.DBEntity;
using Microsoft.EntityFrameworkCore;

namespace IngBackendApi.Repository;

public class RecruitmentRepository(IngDbContext context) : Repository<Recruitment, Guid>(context), IRecruitmentRepository
{
    private readonly IngDbContext _context = context;

    public IQueryable<Recruitment> GetRecruitmentByIdIncludeAll(Guid id)
    {
        return _context.Recruitment
            .Include(r => r.Areas)
                .ThenInclude(a => a.TextLayout)
            .Include(r => r.Areas)
                .ThenInclude(a => a.ImageTextLayout)
                    .ThenInclude(im => im.Image)
            .Include(r => r.Areas)
                .ThenInclude(a => a.ListLayout)
                    .ThenInclude(l => l.Items)
                        .ThenInclude(t => t.Type)
            .Include(r => r.Areas)
                .ThenInclude(a => a.KeyValueListLayout)
                    .ThenInclude(kv => kv.Items)
                        .ThenInclude(kvi => kvi.Key)
            .Where(x => x.Id.Equals(id));
    }

    public IQueryable<Recruitment> GetIncludeAll()
    {
        return _context.Recruitment
            .Include(r => r.Areas)
                .ThenInclude(a => a.TextLayout)
            .Include(r => r.Areas)
                .ThenInclude(a => a.ImageTextLayout)
                    .ThenInclude(im => im.Image)
            .Include(r => r.Areas)
                .ThenInclude(a => a.ListLayout)
                    .ThenInclude(l => l.Items)
                        .ThenInclude(t => t.Type)
            .Include(r => r.Areas)
                .ThenInclude(a => a.KeyValueListLayout)
                    .ThenInclude(kv => kv.Items)
                        .ThenInclude(kvi => kvi.Key);
    }


}
