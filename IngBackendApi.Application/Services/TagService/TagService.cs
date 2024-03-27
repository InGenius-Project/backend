namespace IngBackendApi.Services.TagService;
using AutoMapper;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using Microsoft.EntityFrameworkCore;

public class TagService(IUnitOfWork unitOfWork, IMapper mapper) :
Service<Tag, TagDTO, Guid>(unitOfWork, mapper), ITagService
{
    private readonly IRepository<Tag, Guid> _tagRepository = unitOfWork.Repository<Tag, Guid>();
    private readonly IMapper _mapper = mapper;

    public async Task<List<TagDTO>?> GetAllTagsByType(string[]? type)
    {
        var tags = await _tagRepository.GetAll()
            .Include(t => t.Type)
            .Where(t => type == null || type.Contains(t.TagTypeId.ToString()))
            .ToListAsync();

        return _mapper.Map<List<TagDTO>>(tags);
    }

    public new async Task<TagDTO> AddAsync(TagDTO tagDto)
    {
        var tag = _mapper.Map<Tag>(tagDto);
        await _tagRepository.AddAsync(tag);
        await _tagRepository.SaveAsync();
        return _mapper.Map<TagDTO>(tag);
    }
}

