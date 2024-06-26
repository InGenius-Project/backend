namespace IngBackendApi.Application.Interfaces.Service;

using System.Collections.Generic;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;

public interface IRecruitmentService : IService<Recruitment, RecruitmentDTO, Guid>
{
    Task<List<RecruitmentDTO>> GetPublisherRecruitmentsAsync(Guid userId);
    Task<RecruitmentDTO?> GetRecruitmentByIdIncludeAllAsync(Guid recruitmentId);
    Task<RecruitmentDTO> AddOrUpdateAsync(RecruitmentDTO recruitmentDTO, Guid userId);
    Task<RecruitmentSearchResultDTO> SearchRecruitmentsAsync(
        RecruitmentSearchPostDTO searchDTO,
        Guid? userId
    );
    Task ApplyRecruitmentAsync(Guid recruitmentId, Guid resumeId, Guid userId);
    Task<SafetyReport?> GetSafetyReportAsync(Guid recruitmentId);
    Task<IEnumerable<ResumeDTO>> SearchRelativeResumeAsync(
        Guid recruitmentId,
        bool searchAll = false
    );
    Task<bool> CheckRecruitmentOwnershipAsync(Guid userId, Guid recruitmentId);
    Task<IEnumerable<Guid>> GetNotAnalyzedApplyedResumeIdAsync(Guid recruitmentId);
}
