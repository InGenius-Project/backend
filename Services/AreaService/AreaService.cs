namespace IngBackend.Services.AreaService;
using AutoMapper;
using IngBackend.Enum;
using IngBackend.Exceptions;
using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class AreaService(IUnitOfWork unitOfWork, IMapper mapper, IRepositoryWrapper repository) : Service<Area, AreaDTO, Guid>(unitOfWork, mapper)
{
    private readonly IMapper _mapper = mapper;
    private readonly IRepositoryWrapper _repository = repository;

    public async Task<AreaDTO?> GetAreaIncludeAllById(Guid areaId)
    {
        var area = _repository.Area.GetAreaByIdIncludeAll(areaId).AsNoTracking();
        return await _mapper.ProjectTo<AreaDTO>(area).FirstOrDefaultAsync();
    }

    public async Task CheckAreaOwnership(Guid areaId, UserInfoDTO req)
    {
        var area = await _repository.Area.GetAreaByIdIncludeUser(areaId).AsNoTracking().FirstOrDefaultAsync();
        if (area == null || area.User?.Id != req.Id)
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

    public async Task<List<AreaTypeDTO>?> GetAreaTypeByRole(UserRole[]? roles)
    {
        var query = await _repository.AreaType
            .GetAll(at => roles.Any(r => at.UserRole.Contains(r)))
            .ToListAsync();

        return _mapper.Map<List<AreaTypeDTO>>(query);
    }


    public async Task<List<AreaTypeDTO>> GetAllAreaTypes(UserRole[] userRoles)
    {
        var areaTypes = _repository.AreaType
            .GetAll()
            .Where(a => a.UserRole.Any(ur => userRoles.Contains(ur)));
        return await _mapper.ProjectTo<AreaTypeDTO>(areaTypes).ToListAsync();
        ;
    }

    public async Task<AreaTypeDTO?> GetAreaTypeByIdAsync(int id)
    {
        var query = _repository.Area.GetAreaTypeByIdIncludeAll(id);
        return await _mapper.ProjectTo<AreaTypeDTO>(query).FirstOrDefaultAsync();
    }

    public async Task PostArea(AreaPostDTO req, UserInfoDTO user)
    {
        var parsedAreaId = req.Id;
        var area = await _repository.Area.GetAreaByIdIncludeAll(parsedAreaId).FirstOrDefaultAsync();

        // Add Area 
        if (area == null)
        {
            var newArea = _mapper.Map<Area>(req);

            if (req.ListLayout != null)
            {
                foreach (var listTag in req.ListLayout.Items)
                {
                    var tag = await _repository.Tag.GetByIdAsync(listTag.Id, false);
                    _repository.Tag.SetEntityState(tag, EntityState.Modified);
                    newArea.ListLayout.Items.Add(tag);
                };

            }
            await _repository.Area.AddAsync(newArea);
        }

        await CheckAreaOwnership(area.Id, user);
        _mapper.Map(req, area);
        await _repository.Area.UpdateAsync(area);
    }





    public async Task PostAreaType(AreaTypePostDTO req)
    {
        var areaType = await _repository.Area.GetAreaTypeByIdIncludeAll(req.Id.GetValueOrDefault()).FirstOrDefaultAsync();
        if (areaType == null)
        {
            areaType ??= _mapper.Map<AreaType>(req);
            areaType.ListTagTypes = await _repository.Tag.GetTagTypes(req.ListTagTypeIds).ToListAsync();
            await _repository.AreaType.AddAsync(areaType);
            return;
        }


        _mapper.Map(req, areaType);
        areaType.ListTagTypes = await _repository.Tag.GetTagTypes(req.ListTagTypeIds).ToListAsync();
        await _repository.AreaType.UpdateAsync(areaType);

    }

}
