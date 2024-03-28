namespace IngBackendApi.Interfaces.Service;

using IngBackendApi.Enum;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;

public interface IAreaTypeService : IService<AreaType, AreaTypeDTO, int>
{
    new Task AddAsync(AreaTypeDTO areaTypeDto);
    new Task UpdateAsync(AreaTypeDTO areaTypeDto);
    List<AreaTypeDTO> GetByRoles(IEnumerable<UserRole> roles);
    Task CheckOwnerShip(int areaTypeId, UserRole userRole);
}
