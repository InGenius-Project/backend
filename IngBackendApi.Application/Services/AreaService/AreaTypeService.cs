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

public class AreaTypeService(IUnitOfWork unitOfWork, IMapper mapper, IRepositoryWrapper repository)
    : Service<AreaType, AreaTypeDTO, int>(unitOfWork, mapper),
        IAreaTypeService
{
    private readonly IMapper _mapper = mapper;
    private readonly IRepositoryWrapper _repository = repository;
    private readonly IRepository<AreaType, int> _areaTypeRepository = unitOfWork.Repository<
        AreaType,
        int
    >();

    public new async Task AddAsync(AreaTypeDTO areaTypeDto)
    {
        var areaType = _mapper.Map<AreaType>(areaTypeDto);

        // TODO: 新增 TagType 的權限問題
        areaType.ListTagTypes?.ForEach(a => _repository.TagType.Attach(a));

        await _areaTypeRepository.AddAsync(areaType);
        await _areaTypeRepository.SaveAsync();
    }

    public new async Task UpdateAsync(AreaTypeDTO areaTypeDto)
    {
        var areaType = await _areaTypeRepository
            .GetAll()
            .Include(a => a.ListTagTypes)
            .FirstOrDefaultAsync(a => a.Id.Equals(areaTypeDto.Id));
        if (areaType == null)
        {
            throw new NotFoundException("areaType not found.");
        }

        _mapper.Map(areaTypeDto, areaType);

        await _areaTypeRepository.SaveAsync();
    }

    public List<AreaTypeDTO> GetByRoles(IEnumerable<UserRole> roles)
    {
        if (roles.ToArray().Length == 0)
        {
            var allAreaType = _areaTypeRepository.GetAll().Include(a => a.ListTagTypes);
            return _mapper.Map<List<AreaTypeDTO>>(allAreaType);
        }
        var areaType = _areaTypeRepository
            .GetAll()
            .Include(a => a.ListTagTypes)
            .Where(x => x.UserRole.Any(a => roles.Contains(a)));
        return _mapper.Map<List<AreaTypeDTO>>(areaType);
    }

    public async Task CheckOwnerShip(int areaTypeId, UserRole userRole)
    {
        var area = await _areaTypeRepository
            .GetAll()
            .AsNoTracking()
            .Include(a => a.UserRole)
            .FirstOrDefaultAsync(a => a.Id == areaTypeId) ?? throw new NotFoundException("area not found.");

        if (!area.UserRole.Contains(userRole))
        {
            throw new ForbiddenException();
        }
    }
}
