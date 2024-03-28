namespace IngBackendApi.Services.TagService;

using System.Globalization;
using AutoMapper;
using IngBackendApi.Exceptions;
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
            .Where(t => type == null || type.Contains(t.TagTypeId.ToString(CultureInfo.InvariantCulture)))
            .ToListAsync();

        return _mapper.Map<List<TagDTO>>(tags);
    }

    public async Task CheckOwnerShip(Guid tagId, Guid userId)
    {
        var tag = await _tagRepository.GetAll()
            .AsNoTracking()
            .Include(t => t.User)
            .Where(t => t.Id == tagId)
            .FirstOrDefaultAsync() ?? throw new NotFoundException("標籤不存在");

        if (tag.User == null)
        {
            throw new UnauthorizedException();
        }

        var userExist = tag.User.Select(u => u.Id).Contains(userId);
        if (!userExist)
        {
            throw new UnauthorizedException();
        }
    }
}

