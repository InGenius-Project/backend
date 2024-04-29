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

public class TagService(IUnitOfWork unitOfWork, IMapper mapper)
    : Service<Tag, TagDTO, Guid>(unitOfWork, mapper),
        ITagService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IRepository<Tag, Guid> _tagRepository = unitOfWork.Repository<Tag, Guid>();
    private readonly IRepository<User, Guid> _userRepository = unitOfWork.Repository<User, Guid>();
    private readonly IMapper _mapper = mapper;

    public async Task<List<TagDTO>> GetAllTagsByTypeIds(int[] typeIds)
    {
        var tags = await _tagRepository
            .GetAll()
            .Include(t => t.Type)
            .Where(t => typeIds.Contains(t.Type.Id))
            .ToListAsync();
        return _mapper.Map<List<TagDTO>>(tags);
    }

    public async Task<TagDTO> AddOrUpdateAsync(TagPostDTO tagDTO, Guid userId)
    {
        var parsedTagId = tagDTO.Id ?? Guid.Empty;
        if (parsedTagId.Equals(Guid.Empty))
        {
            // new Tag
            var newTag = _mapper.Map<Tag>(tagDTO);
            var user =
                await _userRepository.GetByIdAsync(userId) ?? throw new UserNotFoundException();
            newTag.Owners.Add(user);
            await _tagRepository.AddAsync(newTag);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TagDTO>(newTag);
        }

        // update tag
        await CheckOwnerShip(parsedTagId, userId);
        var tag = await _tagRepository.GetByIdAsync(parsedTagId);
        _mapper.Map(tagDTO, tag);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<TagDTO>(tag);
    }

    public async Task CheckOwnerShip(Guid tagId, Guid userId)
    {
        var tag =
            await _tagRepository
                .GetAll()
                .AsNoTracking()
                .Include(t => t.Owners)
                .Where(t => t.Id == tagId)
                .FirstOrDefaultAsync() ?? throw new NotFoundException("標籤不存在");

        if (tag.Owners.Count == 0)
        {
            throw new ForbiddenException();
        }

        var userExist = tag.Owners.Select(u => u.Id).Contains(userId);
        if (!userExist)
        {
            throw new ForbiddenException();
        }
    }
}
