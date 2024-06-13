namespace IngBackendApi.Interfaces.Service;

using IngBackendApi.Enum;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;

public interface IAreaService : IService<Area, AreaDTO, Guid>
{
    Task<AreaDTO?> GetAreaIncludeAllById(Guid areaId);
    Task CheckAreaOwnership(Guid areaId, Guid userId);
    void ClearArea(AreaDTO req);

    Task<IEnumerable<AreaTypeDTO>> GetAllAreaTypesAsync(string? query);
    Task UpdateLayoutAsync(Guid areaId, ListLayoutDTO listLayoutDTO);
    Task UpdateLayoutAsync(Guid areaId, TextLayoutDTO textLayoutDTO);
    Task UpdateLayoutAsync(Guid areaId, ImageTextLayoutPostDTO imageTextLayoutPostDTO);
    Task UpdateLayoutAsync(Guid areaId, KeyValueListLayoutDTO keyValueListLayoutDTO);
    Task UpdateAreaSequenceAsync(List<AreaSequencePostDTO> areaSequencePostDTOs, Guid userId);
    new Task UpdateAsync(AreaDTO areaDto);
    Task<ImageDTO?> GetImageByIdAsync(Guid imageId);
    Task<AreaDTO> AddOrUpdateAsync(AreaDTO areaDTO, Guid userId);
    Task<IEnumerable<AreaDTO>> GetUserAreaByAreaTypeIdAsync(Guid userId, int areaTypeId);
    Task<IEnumerable<AreaDTO>> GetRecruitmentAreaByAreaTypeIdAsync(
        int areaTypeId,
        Guid recruitmentId
    );
}
