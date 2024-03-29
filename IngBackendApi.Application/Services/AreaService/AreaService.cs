namespace IngBackendApi.Services.AreaService;

using AutoMapper;
using IngBackendApi.Enum;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using Microsoft.EntityFrameworkCore;

public class AreaService(IUnitOfWork unitOfWork, IMapper mapper, IRepositoryWrapper repository)
    : Service<Area, AreaDTO, Guid>(unitOfWork, mapper),
        IAreaService
{
    private readonly IMapper _mapper = mapper;
    private readonly IRepositoryWrapper _repository = repository;

    private readonly IRepository<AreaType, int> _areaTypeRepository = unitOfWork.Repository<
        AreaType,
        int
    >();

    public new async Task UpdateAsync(AreaDTO areaDto)
    {
        var area = await _repository.Area.GetAreaByIdIncludeAll(areaDto.Id).FirstAsync();
        _mapper.Map(areaDto, area);

        var tagTypes = _repository.TagType.GetAll();
        // remove tagType Entity
        area.ListLayout?.Items?.ForEach(i => i.Type = tagTypes.FirstOrDefault(t => t.Id == i.TagTypeId));

        await _repository.Area.UpdateAsync(area);
    }

    public async Task<AreaDTO?> GetAreaIncludeAllById(Guid areaId)
    {
        var area = await _repository.Area.GetAreaByIdIncludeAll(areaId).AsNoTracking().FirstOrDefaultAsync();
        return _mapper.Map<AreaDTO>(area);
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
        var area = await _repository.Area.GetByIdAsync(req.Id) ?? throw new AreaNotFoundException();

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
        var area = _repository.Area.GetAreaByIdIncludeAllLayout(areaId)
            ?? throw new NotFoundException("area not found.");

        area.ClearLayoutsExclude(a => a.ListLayout);
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

    public async Task UpdateLayoutAsync(Guid areaId, TextLayoutDTO textLayoutDTO)
    {
        var area = _repository.Area.GetAreaByIdIncludeAllLayout(areaId)
            ?? throw new NotFoundException("area not found.");

        area.ClearLayoutsExclude(a => a.TextLayout);
        if (area.TextLayout == null)
        {
            area.TextLayout = _mapper.Map<TextLayout>(textLayoutDTO);
            _repository.Area.Attach(area.TextLayout);
        }
        else
        {
            _mapper.Map(textLayoutDTO, area.TextLayout);
            area.TextLayoutId = area.TextLayout.Id;
            area.TextLayout.AreaId = area.Id;
        }
        await _repository.Area.SaveAsync();
    }
}
