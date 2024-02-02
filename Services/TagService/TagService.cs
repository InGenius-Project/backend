using System.Runtime.CompilerServices;
using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Models.DBEntity;
using Microsoft.EntityFrameworkCore;

namespace IngBackend.Services.TagService;

public class TagService : Service<Tag, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Tag, Guid> _tagRepository;
    private readonly IRepository<TagType, Guid> _tagTypeRepository;

    public TagService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _tagRepository = unitOfWork.Repository<Tag, Guid>();
        _tagTypeRepository = unitOfWork.Repository<TagType, Guid>();
    }   

    public async Task<List<Tag>> GetAllTagsByType(string? type)
    {
        var tags = await _tagRepository.GetAll()
        .Where(t => t.Type.Value == type)
        .Include(t => t.Type)
        .ToListAsync();
        
        if( tags == null)
        {
            return new List<Tag>();
        }
        return tags;
    }
    public async Task<List<TagType>> GetAllTagTypes()
    {
        var tagTypes = await _tagTypeRepository.GetAll().ToListAsync();
        return tagTypes;
    }

    public async Task<TagType> GetTagTypeById(Guid id)
    {
        var tagType = await _tagTypeRepository.GetByIdAsync(id);
        return tagType;
    }
        
    public async Task<TagType> AddTagTypeAsync(TagType tagType)
    {
        await _tagTypeRepository.AddAsync(tagType);
        await _unitOfWork.SaveChangesAsync();
        return tagType;
    }

        
    public async Task<TagType> UpdateTagTypeAsync(TagType tagType)
    {
       _tagTypeRepository.Update(tagType);
        await _unitOfWork.SaveChangesAsync();
        return tagType;
    }

    public async Task DeleteTagTypeAsync(TagType tagType)
    {
        if (tagType != null)
        {
            _tagTypeRepository.Delete(tagType);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

    