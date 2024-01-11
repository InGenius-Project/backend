using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Models.DBEntity;
using Microsoft.EntityFrameworkCore;

namespace IngBackend.Services.AreaService;

public class AreaService : Service<Area, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IRepository<Area, Guid> _areaRepository;

    public AreaService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _areaRepository = unitOfWork.Repository<Area, Guid>();
    }

    public Area? GetAreaIncludeAllById(Guid areaId)
    {
        var area = _areaRepository.GetAll()
            .Where(x => x.Id.Equals(areaId))
            .Include(a => a.TextLayout)
            .Include(a => a.ImageTextLayout)
            .Include(a => a.ListLayout)
                .ThenInclude(l => l.Items)
             .Include(a => a.KeyValueListLayout)
                .ThenInclude(kv => kv.Items)
                    .ThenInclude(kvi => kvi.Key)
            .FirstOrDefault();
        return area;
    }





}
