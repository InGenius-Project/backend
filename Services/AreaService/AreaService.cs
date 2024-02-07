using IngBackend.Enum;
using IngBackend.Exceptions;
using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace IngBackend.Services.AreaService;

public class AreaService : Service<Area, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IRepository<Area, Guid> _areaRepository;
    private readonly IRepository<AreaType, int> _areaTypeRepository;

    public AreaService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _areaRepository = unitOfWork.Repository<Area, Guid>();
        _areaTypeRepository = unitOfWork.Repository<AreaType, int>();
    }

    public Area? GetAreaIncludeAllById(Guid areaId)
    {
        var area = _areaRepository.GetAll()
            .Where(x => x.Id.Equals(areaId))
            .Include(a => a.TextLayout)
            .Include(a => a.ImageTextLayout)
                .ThenInclude(it => it.Image)
            .Include(a => a.ListLayout)
                .ThenInclude(l => l.Items)
             .Include(a => a.KeyValueListLayout)
                .ThenInclude(kv => kv.Items)
                    .ThenInclude(kvi => kvi.Key)
            .FirstOrDefault();
        return area;
    }

    public void CheckAreaOwnership(Guid areaId, User user)
    {
        // 檢查 Resume 關聯
        var result = user.Resumes.SelectMany(x => x.Areas.Where(a => a.Id == areaId)).Any();
        if (result)
        {
            throw new ForbiddenException();
        }
    }

    public void ClearArea(Area area)
    {
        var properties = area.GetType().GetProperties();

        foreach (var property in properties)
        {
            if (property.Name != "Id")
            {
                property.SetValue(area, null);
            }
        }
    }


    public List<AreaType> GetAllAreaTypes(UserRole[]? userRoles)
    {
        var areaTypes = _areaTypeRepository
            .GetAll()
            .Where(a => a.UserRole.Any(ur => userRoles.Contains(ur)))
            .ToList();
        return areaTypes;
    }


    public async Task<AreaType> GetAreaTypeById(int id)
    {
        var areaType = await _areaTypeRepository.GetByIdAsync(id);
        return areaType;
    }

    public async Task<AreaType> AddAreaTypeAsync(AreaType areaType)
    {
        await _areaTypeRepository.AddAsync(areaType);
        await _unitOfWork.SaveChangesAsync();
        return areaType;
    }


    public async Task<AreaType> UpdateAreaTypeAsync(AreaType areaType)
    {
        _areaTypeRepository.Update(areaType);
        await _unitOfWork.SaveChangesAsync();
        return areaType;
    }

    public async Task DeleteAreaTypeAsync(AreaType areaType)
    {
        if (areaType != null)
        {
            _areaTypeRepository.Delete(areaType);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
