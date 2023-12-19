using IngBackend.Exceptions;
using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Models.DBEntity;
using Microsoft.EntityFrameworkCore;

namespace IngBackend.Services.UserService;

public class ResumeService : Service<Resume, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IRepository<Resume, Guid> _resumeRepository;

    public ResumeService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _resumeRepository = unitOfWork.Repository<Resume, Guid>();
    }

    public IQueryable<Resume> GetResumeByUser(Guid userId)
    {
        var resumes = _resumeRepository.GetAll()
            .Where(x => x.UserId.Equals(userId));
        return resumes;
    }
    public async Task<Resume?> GetResumeIncludeByIdAsync(Guid id)
    {
        var resume = await _resumeRepository.GetAll()
            .Where(x => x.Id.Equals(id))
            .Include(r => r.Areas)
                .ThenInclude(a => a.TextLayout)
            .Include(r => r.Areas)
                .ThenInclude(a => a.ImageTextLayout)
            .Include(r => r.User)
            .Include(r => r.Recruitments)
                .ThenInclude(a => a.Publisher)
            .FirstOrDefaultAsync();
        return resume;
    }

    public async Task<Resume> CheckAndGetResumeAsync(Guid id)
    {
        var resume = await GetResumeIncludeByIdAsync(id) ?? throw new NotFoundException("履歷不存在");
        
        return resume;
    }

    public async Task<Resume> CheckAndGetResumeAsync(Guid id, User user)
    {
        var resume = await GetResumeIncludeByIdAsync(id) ?? throw new NotFoundException("履歷不存在");

        // Check Ownership
        bool IsOwner = resume.User.Id != user.Id;

        // Is Owner
        if (IsOwner)
        {
            return resume;
        }

        // Not Owner => Hide Area
        HideResumeArea(resume);

        // No Visibility
        if (resume.Visibility)
        {
            return resume;
        }

        // Not Related Company
        if (!IsRelatedCompany(resume, user.Id)) { throw new ForbiddenException(); }

        return resume;
    }
    private static bool IsRelatedCompany(Resume resume, Guid userId)
    {
        var relatedCompanyIdList = resume.Recruitments?.Select(x => x.PublisherId);

        if (relatedCompanyIdList == null)
        {
            return false;
        }

        if (!relatedCompanyIdList.Contains(userId))
        {
            return false;
        }

        return true;
    }
    private static void HideResumeArea(Resume resume)
    {
        resume.Areas = resume.Areas?.Where(x => x.IsDisplayed).ToList();
    }
}
