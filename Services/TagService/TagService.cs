namespace IngBackend.Services.TagService;

using AutoMapper;
using IngBackend.Exceptions;
using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Models.DBEntity;
using IngBackend.Models.DTO;
using Microsoft.EntityFrameworkCore;

public class TagService(IUnitOfWork unitOfWork, IMapper mapper, IRepositoryWrapper repository) : Service<Tag, TagDTO, Guid>(unitOfWork, mapper)
{
    private readonly IMapper _mapper = mapper;
    private readonly IRepositoryWrapper _repository = repository;

    public async Task<List<TagDTO>?> GetAllTagsByType(string? type)
    {
        var tags = _repository.Tag.GetAll()
            .Include(t => t.Type)
            .Where(t => t.Type.Value == type);

        if (tags == null)
        {
            return [];
        }
        return await _mapper.ProjectTo<List<TagDTO>>(tags).FirstOrDefaultAsync();
    }

    public async Task PostTag(TagDTO req)
    {

        var existingTag = await _repository.Tag.GetByIdAsync(req.Id ?? Guid.Empty);
        var existingTagType = await _repository.TagType.GetByIdAsync(req.Type.Id);

        if (existingTagType == null)
        {
            throw new TagTypeNotFoundExceptiuon();
        }

        if (existingTag == null)
        {
            var newTag = _mapper.Map<Tag>(req);
            newTag.Type = existingTagType;
            await _repository.Tag.AddAsync(newTag);
        }
        else
        {
            _mapper.Map(req, existingTag);
            existingTag.Type = existingTagType;
            await _repository.Tag.UpdateAsync(existingTag);
        }
    }

    public async Task<TagTypeDetailDTO?> GetTagTypeIncludeTags(int id)
    {
        var tagType = await _repository.TagType.GetByIdAsync(id);

        if (tagType == null)
        {
            throw new TagTypeNotFoundExceptiuon();
        }
        var tagTypeDetail = _mapper.Map<TagTypeDetailDTO>(tagType);
        tagTypeDetail.Tags = await _repository.Tag.GetAll()
            .Include(t => t.Type)
            .Where(t => t.Type.Id == id)
            .Select(t => _mapper.Map<TagDTO>(t)).ToListAsync();

        return tagTypeDetail;
    }


}

