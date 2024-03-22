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

    public async Task<List<TagDTO>?> GetAllTagsByType(string? type)
    {
        var tags = _tagRepository.GetAll()
            .Include(t => t.Type)
            .Where(t => t.Type.Value == type);

        if (tags == null)
        {
            return [];
        }
        return await _mapper.ProjectTo<List<TagDTO>>(tags).FirstOrDefaultAsync();
    }
}

