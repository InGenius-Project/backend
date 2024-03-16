using AutoMapper;
using IngBackend.Enum;
using IngBackend.Exceptions;
using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace IngBackend.Services.AreaService;

public class AreaService : Service<Area, AreaDTO, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repository;

    private readonly IRepository<AreaType, int> _areaTypeRepository;

    public AreaService(IUnitOfWork unitOfWork, IMapper mapper, IRepositoryWrapper repository)
        : base(unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _repository = repository;
        _areaTypeRepository = unitOfWork.Repository<AreaType, int>();
    }

    public async Task<AreaDTO?> GetAreaIncludeAllById(Guid areaId)
    {
        var area = _repository.Area.GetAreaByIdIncludeAll(areaId).AsNoTracking();
        return await _mapper.ProjectTo<AreaDTO>(area).FirstOrDefaultAsync();
    }

    public async Task CheckAreaOwnership(Guid areaId, Guid userId)
    {
        var area = await _repository
            .Area.GetAll()
            .Include(a => a.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id.Equals(areaId));

        if (area == null || area.UserId != userId)
        {
            throw new ForbiddenException();
        }
    }

    public async void ClearArea(AreaDTO req)
    {
        var area = await _repository.Area.GetByIdAsync(req.Id);

        if (area == null)
        {
            throw new AreaNotFoundException();
        }

        var properties = area.GetType().GetProperties();

        foreach (var property in properties)
        {
            if (property.Name != "Id")
            {
                property.SetValue(area, null);
            }
        }
    }

    public async Task<List<AreaTypeDTO>> GetAllAreaTypes(UserRole[] userRoles)
    {
        var areaTypes = _areaTypeRepository
            .GetAll()
            .Where(a => a.UserRole.Any(ur => userRoles.Contains(ur)));
        return await _mapper.ProjectTo<AreaTypeDTO>(areaTypes).ToListAsync();
        ;
    }

    public async Task UpdateLayoutAsync(Guid areaId, ListLayoutDTO listLayoutDTO)
    {
        var area = await _repository
            .Area.GetAll()
            .Include(a => a.ListLayout)
            .ThenInclude(l => l.Items)
            .FirstOrDefaultAsync(a => a.Id.Equals(areaId));

        if (area == null)
        {
            throw new NotFoundException("area not found.");
        }

        area.ClearLayouts();

        if (area.ListLayout == null)
        {
            area.ListLayout = _mapper.Map<ListLayout>(listLayoutDTO);
            _repository.Area.Attach(area.ListLayout);
        }
        else
        {
            _mapper.Map(listLayoutDTO, area.ListLayout);
            area.ListLayoutId = area.ListLayout.Id;
            area.ListLayout.AreaId = area.Id;
        }
        await _repository.Area.SaveAsync();
    }
}
