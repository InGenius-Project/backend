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
    private readonly IRepository<AreaType, int> _areaTypeRepository = unitOfWork.Repository<AreaType, int>();

    public async Task<AreaDTO?> GetAreaIncludeAllById(Guid areaId)
    {
        var area = _repository.Area.GetAreaByIdIncludeAll(areaId);
        return await _mapper.ProjectTo<AreaDTO>(area).FirstOrDefaultAsync();
    }

    public async Task CheckAreaOwnership(Guid areaId, UserInfoDTO req)
    {
        var area = await _repository.Area.GetByIdAsync(areaId);
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


    public async Task<List<AreaTypeDTO>> GetAllAreaTypes(UserRole[] userRoles)
    {
        var areaTypes = _areaTypeRepository
            .GetAll()
            .Where(a => a.UserRole.Any(ur => userRoles.Contains(ur)));
        return await _mapper.ProjectTo<AreaTypeDTO>(areaTypes).ToListAsync();
        ;
    }

}
