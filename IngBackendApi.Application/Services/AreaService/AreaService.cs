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

public class AreaService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IRepositoryWrapper repository,
    IWebHostEnvironment env,
    IConfiguration config
) : Service<Area, AreaDTO, Guid>(unitOfWork, mapper), IAreaService
{
    private readonly IMapper _mapper = mapper;
    private readonly IRepositoryWrapper _repository = repository;
    private readonly IRepository<AreaType, int> _areaTypeRepository = unitOfWork.Repository<
        AreaType,
        int
    >();
    private readonly IConfiguration _config = config;
    private readonly IRepository<Image, Guid> _imageRepository = unitOfWork.Repository<
        Image,
        Guid
    >();
    private readonly IRepository<Tag, Guid> _tagRepository = unitOfWork.Repository<Tag, Guid>();
    private readonly IWebHostEnvironment _env = env;

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

    public async Task<List<AreaTypeDTO>> GetAllAreaTypes(UserRole[] userRoles)
    {
        var areaTypes = _areaTypeRepository
            .GetAll()
            .Where(a => a.UserRole.Any(ur => userRoles.Contains(ur)));
        return await _mapper.ProjectTo<AreaTypeDTO>(areaTypes).ToListAsync();
        ;
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
            area.TextLayout.AreaId = area.Id;
        }
        await _repository.Area.SaveAsync();
    }

    public async Task UpdateLayoutAsync(Guid areaId, ImageTextLayoutPostDTO imageTextLayoutPostDTO)
    {
        var area =
            _repository.Area.GetAreaByIdIncludeAllLayout(areaId)
            ?? throw new NotFoundException("area not found.");

        area.ClearLayoutsExclude(a => a.ImageTextLayout);

        // TODO: 保存圖片
        var image = imageTextLayoutPostDTO.Image;
        var filepath = _config["ImageSavePath:Area"] ?? "images/area";
        // 保存圖片
        var newImage = await SaveImageAsync(image, filepath);
        newImage.AltContent = imageTextLayoutPostDTO.AltContent;

        if (area.ImageTextLayout == null)
        {
            // new image layout
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
                _imageRepository.Delete(area.ImageTextLayout.Image);
                var fullpath = Path.Combine(_env.WebRootPath, area.ImageTextLayout.Image.Filepath);
                if (File.Exists(fullpath))
                {
                    File.Delete(fullpath);
                }
            }
            area.ImageTextLayout.Image = newImage;
            area.ImageTextLayout.TextContent = imageTextLayoutPostDTO.TextContent;
            area.ImageTextLayoutId = area.ImageTextLayout.Id;
            area.ImageTextLayout.AreaId = area.Id;
        }
        await _repository.Area.SaveAsync();
    }

    public async Task UpdateLayoutAsync(Guid areaId, KeyValueListLayoutDTO keyValueListLayoutDTO)
    {
        var area =
            _repository.Area.GetAreaByIdIncludeAllLayout(areaId)
            ?? throw new NotFoundException("area not found.");

        area.ClearLayoutsExclude(a => a.KeyValueListLayout);

        if (area.KeyValueListLayout == null)
        {
            area.KeyValueListLayout = _mapper.Map<KeyValueListLayout>(keyValueListLayoutDTO);
        }
        else
        {
            _mapper.Map(keyValueListLayoutDTO, area.KeyValueListLayout);
            area.KeyValueListLayout.AreaId = area.Id;
        }
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
        areas.ForEach(async a =>
        {
            await CheckAreaOwnership(a.Id, userId);
            a.Sequence = areaSequencePostDTOs.First(s => s.Id == a.Id)?.Sequence ?? a.Sequence;
        });
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
        var area = await _repository.Area.GetByIdAsync(areaDTO.Id);
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

    private async Task<Image> SaveImageAsync(IFormFile file, string path)
    {
        if (file.Length == 0)
        {
            throw new ArgumentException("File is empty");
        }

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
