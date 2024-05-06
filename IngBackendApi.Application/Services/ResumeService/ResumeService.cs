namespace IngBackendApi.Services.UserService;

using AutoMapper;
using IngBackendApi.Application.Interfaces.Service;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.Service;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using Microsoft.EntityFrameworkCore;

public class ResumeService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IRepositoryWrapper repository,
    IBackgroundTaskService backgroundTaskService
) : Service<Resume, ResumeDTO, Guid>(unitOfWork, mapper), IResumeService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IRepositoryWrapper _repository = repository;
    private readonly IBackgroundTaskService _backgroundTaskService = backgroundTaskService;
    private readonly IRepository<Resume, Guid> _resumeRepository = unitOfWork.Repository<
        Resume,
        Guid
    >();

    public async Task<List<ResumeDTO>> GetUserResumesAsync(Guid userId)
    {
        var resumes = await _repository
            .Resume.GetIncludeAll()
            .Where(r => r.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
        return _mapper.Map<List<ResumeDTO>>(resumes);
    }

    public async Task<ResumeDTO?> GetResumeByIdIncludeAllAsync(Guid resumeId)
    {
        var resume = await _repository
            .Resume.GetIncludeAll()
            .Where(r => r.Id == resumeId)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        if (resume == null)
        {
            return null;
        }

        return _mapper.Map<ResumeDTO>(resume);
    }

    public async Task<List<ResumeDTO>> GetRecruitmentResumesAsync(Guid recruitmentId)
    {
        var resumes = await _repository
            .Resume.GetIncludeAll()
            .Where(r => r.Recruitments.Any(x => x.Id == recruitmentId))
            .AsNoTracking()
            .ToListAsync();

        resumes.ForEach(HideResumeArea);

        return _mapper.Map<List<ResumeDTO>>(resumes);
    }

    public async Task<ResumeDTO> AddOrUpdateAsync(ResumeDTO resumeDTO, Guid userId)
    {
        var resume = await _repository.Resume.GetByIdAsync(resumeDTO.Id);
        // Add new resume
        if (resume == null)
        {
            resume = _mapper.Map<Resume>(resumeDTO);
            resume.UserId = userId;
            await _repository.Resume.AddAsync(resume);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ResumeDTO>(resume);
        }
        // Update resume
        _mapper.Map(resumeDTO, resume);
        resume.UserId = userId;
        await _repository.Resume.UpdateAsync(resume);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<ResumeDTO>(resume);
    }

    /// <summary>
    /// Asynchronously checks if a resume exists with the specified ID, retrieves it with associated information, and applies access control based on the provided user.
    /// </summary>
    /// <param name="id">The ID of the resume to check and retrieve (Guid).</param>
    /// <param name="user">The user requesting access to the resume (User).</param>
    /// <returns>A `Resume` object containing all information about the resume and its related entities, with potentially hidden areas based on access control.</returns>
    /// <exception cref="NotFoundException">Throws a `NotFoundException` if no resume exists with the specified ID.</exception>
    /// <exception cref="ForbiddenException">Throws a `ForbiddenException` if the user requesting access does not have permission to view the resume.</exception>
    public async Task<ResumeDTO> CheckAndGetResumeAsync(Guid id, UserInfoDTO user)
    {
        var resume =
            await _repository
                .Resume.GetIncludeAll()
                .AsNoTracking()
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync() ?? throw new NotFoundException("Resume");

        // Is Owner
        if (resume.UserId == user.Id)
        {
            return _mapper.Map<ResumeDTO>(resume);
        }

        // Not Owner => Hide Area
        HideResumeArea(resume);

        // No Visibility
        if (resume.Visibility)
        {
            return _mapper.Map<ResumeDTO>(resume);
        }

        // Not Related Company
        if (!IsRelatedCompany(resume, user.Id))
        {
            throw new ForbiddenException();
        }

        var resumeDTO = _mapper.Map<ResumeDTO>(resume);
        resumeDTO
            .Recruitments.ToList()
            .ForEach(r =>
            {
                r.Areas = [];
                r.Keywords = r.Keywords.Take(5);
            });

        return resumeDTO;
    }

    public async Task<IEnumerable<RecruitmentDTO>> SearchRelativeRecruitmentAsync(Guid resumeId)
    {
        // TODO: Add page search
        var resume =
            await _resumeRepository
                .GetAll(r => r.Id == resumeId)
                .Include(r => r.Keywords)
                .ThenInclude(k => k.Recruitments)
                .AsNoTracking()
                .SingleOrDefaultAsync() ?? throw new NotFoundException("Resume not found");

        var recruitments = resume
            .Keywords.SelectMany(k => k.Recruitments)
            .Where(r => r.Enable)
            .GroupBy(r => r.Id)
            .OrderByDescending(g => g.Count())
            .Select(g => g.First());
        return _mapper.Map<IEnumerable<RecruitmentDTO>>(recruitments);
    }

    public async Task<bool> CheckResumeOwnership(Guid userId, Guid resumeId)
    {
        var resume =
            await _resumeRepository
                .GetAll(r => r.Id == resumeId)
                .AsNoTracking()
                .SingleOrDefaultAsync() ?? throw new NotFoundException("resume not found");
        return resume.UserId == userId;
    }

    /// <summary>
    /// Checks if the provided user is from a company related to the resume.
    /// </summary>
    /// <param name="resume">The resume object to check.</param>
    /// <param name="userId">The ID of the user to check.</param>
    /// <returns>True if the user is from a related company, false otherwise.</returns>
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

    /// <summary>
    /// Filters the "Areas" property of the provided resume object to only include areas marked as publicly visible.
    /// </summary>
    /// <param name="resume">The resume object to modify.</param>
    /// <remarks>
    /// This function modifies the resume object in-place by directly changing its "Areas" property.
    /// It filters the list of areas, keeping only those marked with the "IsDisplayed" flag set to true.
    /// </remarks>
    private static void HideResumeArea(Resume resume) =>
        resume.Areas = resume?.Areas?.Where(x => x.IsDisplayed).ToList();
}
