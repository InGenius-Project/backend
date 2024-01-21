using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Services.AreaService;
using IngBackend.Models.DBEntity;
using Microsoft.EntityFrameworkCore;

namespace IngBackend.Services.RecruitmentService;

public class RecruitmentService : Service<Recruitment, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Recruitment, Guid> _recruitmentRepository;

    public RecruitmentService(IUnitOfWork unitOfWork) : base(unitOfWork)

    {
        _unitOfWork = unitOfWork;
        _recruitmentRepository = unitOfWork.Repository<Recruitment, Guid>();
    }
    public List<Recruitment> GetUserRecruitements(Guid userId)
    {
        var recruitment = _recruitmentRepository.GetAll()
            .Where(x => x.PublisherId.Equals(userId))
            .ToList();
        return recruitment ?? [];
    }

    public Recruitment GetRecruitmentIncludeAllById(Guid recruitmentId)
    {
        var recruitment = _recruitmentRepository.GetAll()
            .Where(x => x.Id.Equals(recruitmentId))
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
            .FirstOrDefault();
        return recruitment;
    }



}