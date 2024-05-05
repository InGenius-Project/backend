namespace IngBackendApi.Services.AreaService;

using AutoMapper;
using IngBackendApi.Enum;
using IngBackendApi.Exceptions;
using IngBackendApi.Helpers;
using IngBackendApi.Interfaces.Models;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using IngBackendApi.Models.Settings;
using Microsoft.EntityFrameworkCore;

public class AreaService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IRepositoryWrapper repository,
    IWebHostEnvironment env,
    IConfiguration config,
    IAIService aiService,
    ISettingsFactory settingsFactory
) : Service<Area, AreaDTO, Guid>(unitOfWork, mapper), IAreaService
{
    private readonly IMapper _mapper = mapper;
    private readonly PathSetting _pathSetting = settingsFactory.GetSetting<PathSetting>();
    private readonly IRepositoryWrapper _repository = repository;
    private readonly IRepository<AreaType, int> _areaTypeRepository = unitOfWork.Repository<
        AreaType,
        int
    >();
    private readonly IRepository<User, Guid> _userRepository = unitOfWork.Repository<User, Guid>();
    private readonly IConfiguration _config = config;
    private readonly IRepository<Image, Guid> _imageRepository = unitOfWork.Repository<
        Image,
        Guid
    >();
    private readonly IRepository<Tag, Guid> _tagRepository = unitOfWork.Repository<Tag, Guid>();
    private readonly IWebHostEnvironment _env = env;
    private readonly IAIService _aiService = aiService;

    public new async Task UpdateAsync(AreaDTO areaDto)
    {
        var area = await _repository.Area.GetAreaByIdIncludeAll(areaDto.Id).FirstAsync();
        _mapper.Map(areaDto, area);

        var tagTypes = _repository.TagType.GetAll();
        // remove tagType Entity
        area.ListLayout?.Items?.ForEach(i =>
            i.Type = tagTypes.FirstOrDefault(t => t.Id == i.TagTypeId)
        );

        await _repository.Area.UpdateAsync(area);
    }

    public async Task<AreaDTO?> GetAreaIncludeAllById(Guid areaId)
    {
        var area = await _repository
            .Area.GetAreaByIdIncludeAll(areaId)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        return _mapper.Map<AreaDTO>(area);
    }

    public async Task CheckAreaOwnership(Guid areaId, Guid userId)
    {
        var area = await _repository
            .Area.GetAll()
            .Include(a => a.User)
            .Include(a => a.Owner)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id.Equals(areaId));
        if (area == null || area.OwnerId != userId)
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

    public async Task<IEnumerable<AreaTypeDTO>> GetAllAreaTypesAsync(string? query)
    {
        IQueryable<AreaType> areaTypes = _areaTypeRepository.GetAll().Include(a => a.ListTagTypes);
        if (query != null)
        {
            areaTypes = areaTypes.Where(a => a.Name.Contains(query) || query.Contains(a.Name));
        }
        return _mapper.Map<IEnumerable<AreaTypeDTO>>(await areaTypes.ToArrayAsync());
    }

    public async Task UpdateLayoutAsync(Guid areaId, ListLayoutDTO listLayoutDTO)
    {
        var area =
            _repository.Area.GetAreaByIdIncludeAllLayout(areaId)
            ?? throw new NotFoundException("area not found.");

        area.ClearLayoutsExclude(a => a.ListLayout);
        if (area.ListLayout == null)
        {
            area.ListLayout = _mapper.Map<ListLayout>(listLayoutDTO);
            _repository.Area.Attach(area.ListLayout);
        }
        else
        {
            _mapper.Map(listLayoutDTO, area.ListLayout);
            area.ListLayoutId = area.ListLayout.Id;
            area.ListLayout.AreaId = area.Id;
        }
        area.ListLayout.AreaId = area.Id;
        area.LayoutType = LayoutType.List;
        await _repository.Area.SaveAsync();
    }

    public async Task UpdateLayoutAsync(Guid areaId, TextLayoutDTO textLayoutDTO)
    {
        var area =
            _repository.Area.GetAreaByIdIncludeAllLayout(areaId)
            ?? throw new NotFoundException("area not found.");

        area.ClearLayoutsExclude(a => a.TextLayout);
        if (area.TextLayout == null)
        {
            area.TextLayout = _mapper.Map<TextLayout>(textLayoutDTO);
            _repository.Area.Attach(area.TextLayout);
        }
        else
        {
            _mapper.Map(textLayoutDTO, area.TextLayout);
            area.TextLayoutId = area.TextLayout.Id;
        }
        area.LayoutType = LayoutType.Text;
        area.TextLayout.AreaId = area.Id;
        await _repository.Area.SaveAsync();
    }

    public async Task UpdateLayoutAsync(Guid areaId, ImageTextLayoutPostDTO imageTextLayoutPostDTO)
    {
        var area =
            _repository.Area.GetAreaByIdIncludeAllLayout(areaId)
            ?? throw new NotFoundException("area not found.");

        area.ClearLayoutsExclude(a => a.ImageTextLayout);

        var newImage =
            imageTextLayoutPostDTO.Uri != null
                ? await CreateImageFromUriAsync(imageTextLayoutPostDTO.Uri)
                : await CreateImageFromFileAsync(
                    imageTextLayoutPostDTO.Image
                        ?? throw new BadRequestException(
                            "Uri and Image cannot be null at same time."
                        )
                );
        newImage.AltContent = imageTextLayoutPostDTO.AltContent;

        if (area.ImageTextLayout == null)
        {
            // New image layout
            area.ImageTextLayout = new ImageTextLayout
            {
                Image = newImage,
                TextContent = imageTextLayoutPostDTO.TextContent,
                Area = area
            };
            _repository.Area.Attach(area.ImageTextLayout);
        }
        else
        {
            // delete old image if exist
            if (area.ImageTextLayout.Image != null)
            {
                if (area.ImageTextLayout.Image.Filepath != "")
                {
                    var fullpath = Path.Combine(
                        _env.WebRootPath,
                        area.ImageTextLayout.Image.Filepath
                    );
                    if (File.Exists(fullpath))
                    {
                        File.Delete(fullpath);
                    }
                }

                if (area.ImageTextLayout.Image.Uri != null)
                {
                    area.ImageTextLayout.Image.Uri = null;
                }
                _imageRepository.Delete(area.ImageTextLayout.Image);
            }

            area.ImageTextLayout.Image = newImage;
            area.ImageTextLayout.TextContent = imageTextLayoutPostDTO.TextContent;
            area.ImageTextLayoutId = area.ImageTextLayout.Id;
        }
        area.ImageTextLayout.AreaId = area.Id;
        area.LayoutType = LayoutType.ImageText;
        await _repository.Area.SaveAsync();
    }

    public async Task UpdateLayoutAsync(Guid areaId, KeyValueListLayoutDTO keyValueListLayoutDTO)
    {
        var area =
            _repository.Area.GetAreaByIdIncludeAllLayout(areaId)
            ?? throw new NotFoundException("area not found.");

        var tagIds = keyValueListLayoutDTO?.Items?.SelectMany(i => i.TagIds).ToList() ?? [];
        var allTags = _tagRepository.GetAll(t => tagIds.Contains(t.Id)).AsNoTracking().ToArray();
        var allTagDTOs = _mapper.Map<List<TagDTO>>(allTags);
        keyValueListLayoutDTO?.Items?.ForEach(i =>
            i.Key = allTagDTOs.Where(t => i.TagIds.Contains(t.Id)).ToList()
        );

        area.ClearLayoutsExclude(a => a.KeyValueListLayout);
        var keyValueListLayout = _mapper.Map<KeyValueListLayout>(keyValueListLayoutDTO);

        if (area.KeyValueListLayout == null)
        {
            area.KeyValueListLayout = keyValueListLayout;
        }
        else
        {
            _mapper.Map(keyValueListLayoutDTO, area.KeyValueListLayout);
            area.KeyValueListLayout.AreaId = area.Id;
        }
        area.KeyValueListLayout.AreaId = areaId;
        area.LayoutType = LayoutType.KeyValueList;
        await _repository.Area.SaveAsync();
    }

    public async Task UpdateAreaSequenceAsync(
        List<AreaSequencePostDTO> areaSequencePostDTOs,
        Guid userId
    )
    {
        var areaDtoIds = areaSequencePostDTOs.Select(a => a.Id).ToList();
        var areas = await _repository
            .Area.GetAll()
            .Where(a => areaDtoIds.Contains(a.Id))
            .ToListAsync();
        foreach (var a in areas)
        {
            await CheckAreaOwnership(a.Id, userId);
            a.Sequence = areaSequencePostDTOs.First(s => s.Id == a.Id)?.Sequence ?? a.Sequence;
        }
        await _repository.User.SaveAsync();
    }

    public async Task<ImageDTO?> GetImageByIdAsync(Guid imageId)
    {
        var image = await _imageRepository.GetByIdAsync(imageId);
        if (image == null)
        {
            return null;
        }
        var imageDto = _mapper.Map<ImageDTO>(image);
        return imageDto;
    }

    public async Task<AreaDTO> AddOrUpdateAsync(AreaDTO areaDTO, Guid userId)
    {
        var area = await _repository
            .Area.GetAll(a => a.Id == areaDTO.Id)
            .Include(a => a.Owner)
            .Include(a => a.User)
            .SingleOrDefaultAsync();

        if (area == null && areaDTO.Id != Guid.Empty)
        {
            throw new NotFoundException("Area not found");
        }
        if (area == null)
        {
            // add new area
            var newArea = _mapper.Map<Area>(areaDTO);
            newArea.OwnerId = userId;
            await _repository.Area.AddAsync(newArea);
            await _repository.Area.SaveAsync();
            return _mapper.Map<AreaDTO>(newArea);
        }

        await CheckAreaOwnership(area.Id, userId);

        _mapper.Map(areaDTO, area);
        await _repository.Area.SaveAsync();
        return _mapper.Map<AreaDTO>(area);
    }

    public async Task<IEnumerable<AreaDTO>> GetUserAreaByAreaTypeIdAsync(
        Guid userId,
        int areaTypeId
    )
    {
        var user =
            await _userRepository
                .GetAll()
                .Where(u => u.Id == userId)
                .Include(u => u.Areas)
                .ThenInclude(a => a.AreaType)
                .Include(u => u.Areas)
                .ThenInclude(a => a.ListLayout.Items)
                .Include(u => u.Areas)
                .ThenInclude(a => a.KeyValueListLayout.Items)
                .ThenInclude(i => i.Key)
                .Include(u => u.Areas)
                .ThenInclude(a => a.ImageTextLayout.Image)
                .Include(u => u.Areas)
                .ThenInclude(a => a.TextLayout)
                .SingleOrDefaultAsync() ?? throw new UserNotFoundException();
        if (user.Areas == null)
        {
            return [];
        }

        return _mapper.Map<IEnumerable<AreaDTO>>(user.Areas.Where(a => a.AreaTypeId == areaTypeId));
    }

    public async Task<IEnumerable<AreaDTO>> GetRecruitmentAreaByAreaTypeIdAsync(
        int areaTypeId,
        Guid recruitmentId
    )
    {
        var areas = await _repository
            .Area.GetAll()
            .Where(a => a.AreaTypeId == areaTypeId && a.RecruitmentId == recruitmentId)
            .Include(a => a.ListLayout.Items)
            .Include(a => a.KeyValueListLayout.Items)
            .ThenInclude(i => i.Key)
            .Include(a => a.ImageTextLayout.Image)
            .Include(a => a.TextLayout)
            .ToListAsync();
        return _mapper.Map<IEnumerable<AreaDTO>>(areas);
    }

    private async Task<Image> CreateImageFromUriAsync(string uri, string contentType = "image/jpg")
    {
        var newImage = new Image
        {
            Uri = uri,
            Filepath = "",
            ContentType = contentType
        };
        await _imageRepository.AddAsync(newImage);
        return newImage;
    }

    private async Task<Image> CreateImageFromFileAsync(IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new ArgumentException("File is empty");
        }
        Helper.CheckImage(file);

        var path = _pathSetting.Image.Area;
        var newImage = new Image { Filepath = "", ContentType = file.ContentType };
        await _imageRepository.AddAsync(newImage);

        var fileId = newImage.Id;
        var fileName = fileId.ToString();
        var fullPath = Path.Combine(_env.WebRootPath, path, fileName);
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            file.CopyTo(stream);
        }
        newImage.Filepath = Path.Combine(path, fileName);
        return newImage;
    }
}
