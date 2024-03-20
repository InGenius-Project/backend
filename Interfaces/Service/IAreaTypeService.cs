namespace IngBackend.Interfaces.Service
{
    using IngBackend.Enum;
    using IngBackend.Models.DBEntity;
    using IngBackend.Models.DTO;

    public interface IAreaTypeService : IService<AreaType, AreaTypeDTO, int>
    {
        new Task AddAsync(AreaTypeDTO areaTypeDto);
        new Task UpdateAsync(AreaTypeDTO areaTypeDto);
        List<AreaTypeDTO> GetByRoles(IEnumerable<UserRole> roles);
    }

}
