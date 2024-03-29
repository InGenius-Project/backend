namespace IngBackendApi.Interfaces.Service
{
    using IngBackendApi.Enum;
    using IngBackendApi.Models.DBEntity;
    using IngBackendApi.Models.DTO;

    public interface IAreaService : IService<Area, AreaDTO, Guid>
    {
        Task<AreaDTO?> GetAreaIncludeAllById(Guid areaId);
        Task CheckAreaOwnership(Guid areaId, Guid userId);
        void ClearArea(AreaDTO req);
        Task<List<AreaTypeDTO>> GetAllAreaTypes(UserRole[] userRoles);
        Task UpdateLayoutAsync(Guid areaId, ListLayoutDTO listLayoutDTO);
        Task UpdateLayoutAsync(Guid areaId, TextLayoutDTO textLayoutDTO);
        new Task UpdateAsync(AreaDTO areaDto);
    }

}
