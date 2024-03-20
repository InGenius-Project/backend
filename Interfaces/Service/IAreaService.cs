namespace IngBackend.Interfaces.Service
{
    using IngBackend.Enum;
    using IngBackend.Models.DBEntity;
    using IngBackend.Models.DTO;

    public interface IAreaService : IService<Area, AreaDTO, Guid>
    {
        Task<AreaDTO?> GetAreaIncludeAllById(Guid areaId);
        Task CheckAreaOwnership(Guid areaId, Guid userId);
        void ClearArea(AreaDTO req);
        Task<List<AreaTypeDTO>> GetAllAreaTypes(UserRole[] userRoles);
        Task UpdateLayoutAsync(Guid areaId, ListLayoutDTO listLayoutDTO);
    }

}
