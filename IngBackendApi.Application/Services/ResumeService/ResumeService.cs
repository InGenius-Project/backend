namespace IngBackendApi.Services.UserService;

using AutoMapper;
using IngBackendApi.Application.Interfaces.Service;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using Microsoft.EntityFrameworkCore;

public class ResumeService(IUnitOfWork unitOfWork, IMapper mapper, IRepositoryWrapper repository)
    : Service<Resume, ResumeDTO, Guid>(unitOfWork, mapper),
        IResumeService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IRepositoryWrapper _repository = repository;
    private readonly IRepository<Resume, Guid> _resumeRepository = unitOfWork.Repository<
        Resume,
        Guid
    >();

    public IQueryable<Resume> GetResumeByUser(Guid userId)
    {
        var resumes = _resumeRepository.GetAll().Where(x => x.UserId.Equals(userId));
        return resumes;
    }

    /// <summary>
    /// Asynchronously checks for the existence of a resume with the specified ID, retrieves it with its associated information, and throws an exception if not found.
    /// </summary>
    /// <param name="id">The ID of the resume to check and retrieve (Guid).</param>
    /// <returns>A `Resume` object containing all information about the resume and its related entities.</returns>
    /// <exception cref="NotFoundException">Throws a `NotFoundException` if no resume exists with the specified ID.</exception>
    public async Task<ResumeDTO?> CheckAndGetResumeAsync(Guid id)
    {
        var resume =
            _repository.Resume.GetResumeByIdIncludeAll(id) ?? throw new NotFoundException("履歷不存在");
        return await _mapper.ProjectTo<ResumeDTO>(resume).FirstOrDefaultAsync();
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
        var query =
            _repository.Resume.GetResumeByIdIncludeAll(id) ?? throw new NotFoundException("履歷不存在");
        var resume = await query.FirstOrDefaultAsync();

        // Is Owner
        if (resume?.UserId == user.Id)
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

        return _mapper.Map<ResumeDTO>(resume);
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
    private static void HideResumeArea(Resume resume)
    {
        resume.Areas = resume.Areas?.Where(x => x.IsDisplayed).ToList();
    }
}
