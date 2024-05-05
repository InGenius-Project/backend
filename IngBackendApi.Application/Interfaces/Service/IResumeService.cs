namespace IngBackendApi.Application.Interfaces.Service;

using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;

public interface IResumeService : IService<Resume, ResumeDTO, Guid>
{
    Task<List<ResumeDTO>> GetUserResumesAsync(Guid userId);
    Task<ResumeDTO?> GetResumeByIdIncludeAllAsync(Guid resumeId);
    Task<List<ResumeDTO>> GetRecruitmentResumesAsync(Guid recruitmentId);
    Task<ResumeDTO> AddOrUpdateAsync(ResumeDTO resumeDTO, Guid userId);
    Task<ResumeDTO> CheckAndGetResumeAsync(Guid id, UserInfoDTO user);
    Task<IEnumerable<RecruitmentDTO>> SearchRelativeRecruitmentAsync(Guid resumeId);
    Task<bool> CheckResumeOwnership(Guid userId, Guid resumeId);
}
