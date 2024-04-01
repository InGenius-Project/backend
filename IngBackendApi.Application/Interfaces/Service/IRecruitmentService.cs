namespace IngBackendApi.Application.Interfaces.Service;

using System.Collections.Generic;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;

public interface IRecruitmentService : IService<Recruitment, RecruitmentDTO, Guid>
{
    Task<List<RecruitmentDTO>> GetUserRecruitementsAsync(Guid userId);
    Task<RecruitmentDTO?> GetRecruitmentByIdIncludeAllAsync(Guid recruitmentId);
    Task<RecruitmentDTO> AddOrUpdateAsync(RecruitmentDTO recruitmentDTO, Guid userId);
}
