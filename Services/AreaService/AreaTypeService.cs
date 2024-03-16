namespace IngBackend.Services.AreaService;

using AutoMapper;
using IngBackend.Enum;
using IngBackend.Exceptions;
using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;
using Microsoft.EntityFrameworkCore;

public class AreaTypeService : Service<AreaType, AreaTypeDTO, int>
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repository;

    private readonly IRepository<AreaType, int> _areaTypeRepository;

    public AreaTypeService(IUnitOfWork unitOfWork, IMapper mapper, IRepositoryWrapper repository)
        : base(unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _repository = repository;
        _areaTypeRepository = unitOfWork.Repository<AreaType, int>();
    }

    public async Task AddAsync(AreaTypeDTO areaTypeDto)
    {
        var areaType = _mapper.Map<AreaType>(areaTypeDto);

        // TODO: 新增 TagType 的權限問題
        areaType.ListTagTypes?.ForEach(a => _repository.TagType.Attach(a));

        await _areaTypeRepository.AddAsync(areaType);
        await _areaTypeRepository.SaveAsync();
    }

    public async Task UpdateAsync(AreaTypeDTO areaTypeDto)
    {
        var areaType = await _areaTypeRepository
            .GetAll()
            .Include(a => a.ListTagTypes)
            .FirstOrDefaultAsync(a => a.Id.Equals(areaTypeDto.Id));

        if (areaType == null)
        {
            throw new NotFoundException("areaType not found.");
        }

        // TODO: 新增 TagType 的權限問題
        _mapper.Map(areaTypeDto, areaType);
        Console.WriteLine("{0}", areaTypeDto.Value);
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
}
