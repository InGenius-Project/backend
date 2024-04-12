namespace IngBackendApi.Application.Interfaces.Service;

using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;

public interface IResumeService : IService<Resume, ResumeDTO, Guid>
{
    Task<List<ResumeDTO>> GetUserResumesAsync(Guid userId);
    Task<ResumeDTO?> GetResumeByIdIncludeAllAsync(Guid resumeId);
    Task<ResumeDTO> AddOrUpdateAsync(ResumeDTO resumeDTO, Guid userId);
    IQueryable<Resume> GetResumeByUser(Guid userId);
    Task<ResumeDTO?> CheckAndGetResumeAsync(Guid id);
    Task<ResumeDTO> CheckAndGetResumeAsync(Guid id, UserInfoDTO user);
}
