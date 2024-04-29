namespace IngBackendApi.Repository;

using IngBackendApi.Context;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Models.DBEntity;
using Microsoft.EntityFrameworkCore;

public class RecruitmentRepository(IngDbContext context)
    : Repository<Recruitment, Guid>(context),
        IRecruitmentRepository
{
    private readonly IngDbContext _context = context;

    public IQueryable<Recruitment> GetIncludeAll() => _context
            .Recruitment.Include(r => r.Publisher.Avatar)
            .Include(r => r.Areas)
                .ThenInclude(a => a.TextLayout)
            .Include(r => r.Areas)
                .ThenInclude(a => a.ImageTextLayout.Image)
            .Include(r => r.Areas)
                .ThenInclude(a => a.ListLayout.Items)
                .ThenInclude(t => t.Type)
            .Include(r => r.Areas)
                .ThenInclude(a => a.KeyValueListLayout.Items)
                .ThenInclude(kvi => kvi.Key);


}
