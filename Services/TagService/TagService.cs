using System.Runtime.CompilerServices;
using AutoMapper;
using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace IngBackend.Services.TagService;

public class TagService : Service<Tag, TagDTO, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Tag, Guid> _tagRepository;
    private readonly IMapper _mapper;
    private readonly IRepository<TagType, int> _tagTypeRepository;

    public TagService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _tagRepository = unitOfWork.Repository<Tag, Guid>();
        _tagTypeRepository = unitOfWork.Repository<TagType, int>();
    }

    public async Task<List<TagDTO>?> GetAllTagsByType(string? type)
    {
        var tags = _tagRepository.GetAll()
            .Include(t => t.Type)
            .Where(t => t.Type.Value == type);

        if (tags == null)
        {
            return new List<TagDTO>();
        }
        return await _mapper.ProjectTo<List<TagDTO>>(tags).FirstOrDefaultAsync();
    }
}

