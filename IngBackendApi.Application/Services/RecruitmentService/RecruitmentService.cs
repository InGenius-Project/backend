namespace IngBackendApi.Services.RecruitmentService;

using AutoMapper;
using IngBackendApi.Application.Interfaces.Service;
using IngBackendApi.Exceptions;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Interfaces.UnitOfWork;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Models.DTO;
using Microsoft.EntityFrameworkCore;

public class RecruitmentService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IRepositoryWrapper repository
) : Service<Recruitment, RecruitmentDTO, Guid>(unitOfWork, mapper), IRecruitmentService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IRepositoryWrapper _repository = repository;
    private readonly IRepository<KeywordRecord, string> _keywordRecordRepository =
        unitOfWork.Repository<KeywordRecord, string>();
    private readonly IRepository<Recruitment, Guid> _recruitmentRepository = unitOfWork.Repository<
        Recruitment,
        Guid
    >();

    public async Task<List<RecruitmentDTO>> GetPublisherRecruitmentsAsync(Guid userId)
    {
        var recruitments = await _repository
            .Recruitment.GetIncludeAll()
            .Include(r => r.Resumes)
            .Where(r => r.PublisherId == userId)
            .ToListAsync();

        return _mapper.Map<List<RecruitmentDTO>>(recruitments);
    }

    public async Task<RecruitmentDTO?> GetRecruitmentByIdIncludeAllAsync(Guid recruitmentId)
    {
        var recruitment = await _repository
            .Recruitment.GetIncludeAll()
            .Where(r => r.Id == recruitmentId)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        if (recruitment == null)
        {
            return null;
        }

        return _mapper.Map<RecruitmentDTO>(recruitment);
    }

    public async Task<RecruitmentDTO> AddOrUpdateAsync(RecruitmentDTO recruitmentDTO, Guid userId)
    {
        var recruitment = await _repository.Recruitment.GetByIdAsync(recruitmentDTO.Id);
        // Add new recruitment
        if (recruitment == null)
        {
            recruitment = _mapper.Map<Recruitment>(recruitmentDTO);
            recruitment.PublisherId = userId;
            await _repository.Recruitment.AddAsync(recruitment);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<RecruitmentDTO>(recruitment);
        }

        // Update recruitment
        _mapper.Map(recruitmentDTO, recruitment);
        recruitment.PublisherId = userId;
        await _repository.Recruitment.UpdateAsync(recruitment);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<RecruitmentDTO>(recruitment);
    }

    public async Task<RecruitmentSearchResultDTO> SearchRecruitmentsAsync(
        RecruitmentSearchPostDTO searchDTO,
        Guid? userId
    )
    {
        if (searchDTO.PageSize <= 0)
        {
            throw new BadRequestException("PageSize 必須大於 0");
        }
        else if (searchDTO.PageSize > 20)
        {
            throw new BadRequestException("PageSize 必須小於 20");
        }

        var sortBy = searchDTO.SortBy ?? "CreatedTime";
        var orderBy = searchDTO.OrderBy == "asc" ? "asc" : "desc";

        var query = _keywordRecordRepository.GetAll();

        if (searchDTO.Query != null && searchDTO.Query.Length > 0)
        {
            query = query.Where(k =>
                EF.Functions.Like(k.Id, $"%{searchDTO.Query}%") || k.Id.Contains(searchDTO.Query)
            );
        }

        var recruitmentQuery = query
            .Include(k => k.Recruitments)
            // .ThenInclude(r => r.Resumes)
            .SelectMany(k => k.Recruitments)
            .Where(r => r.Enable)
            .Distinct();

        // count total page size
        var total = await recruitmentQuery.Select(r => r.Id).CountAsync();
        var maxPage = (int)Math.Ceiling((double)total / searchDTO.PageSize);
        searchDTO.Page = int.Max(maxPage, searchDTO.Page);

        if (orderBy == "asc")
        {
            recruitmentQuery = recruitmentQuery.OrderBy(r => r.CreatedAt);
        }
        else
        {
            recruitmentQuery = recruitmentQuery.OrderByDescending(r => r.CreatedAt);
        }
        var skip = searchDTO.PageSize * (searchDTO.Page - 1);

        // get recruitment with publisher and areas
        recruitmentQuery = recruitmentQuery.Include(r => r.Publisher).Include(r => r.Areas);
        recruitmentQuery = recruitmentQuery.Skip(skip).Take(searchDTO.PageSize);

        var result = await _mapper.ProjectTo<RecruitmentDTO>(recruitmentQuery).ToListAsync();
        if (userId != null)
        {
            var favRecruitmentIds = _repository
                .User.GetAll(u => u.Id == userId)
                .Include(u => u.FavoriteRecruitments)
                .SelectMany(u => u.FavoriteRecruitments.Select(fr => fr.Id));
            result.ForEach(r => r.IsUserFav = favRecruitmentIds.Any(id => id == r.Id));
        }

        result.ForEach(r =>
        {
            r.Areas = [];
            r.Resumes = [];
            r.Keywords = r.Keywords.Take(5);
        });

        return _mapper.Map<RecruitmentSearchResultDTO>(
            new RecruitmentSearchResultDTO
            {
                Query = searchDTO.Query,
                TagIds = searchDTO.TagIds,
                Page = searchDTO.Page,
                PageSize = searchDTO.PageSize,
                MaxPage = maxPage,
                Total = total,
                result = result
            }
        );
    }

    public async Task<SafetyReport?> GetSafetyReportAsync(Guid recruitmentId)
    {
        var recruiment =
            await _repository
                .Recruitment.GetAll(r => r.Id == recruitmentId)
                .Include(r => r.SafetyReport)
                .SingleOrDefaultAsync() ?? throw new NotFoundException("Recruitment not exist.");
        return recruiment.SafetyReport;
    }

    public async Task ApplyRecruitmentAsync(Guid recruitmentId, Guid resumeId, Guid userId)
    {
        var recruitment =
            await _repository
                .Recruitment.GetAll(r => r.Resumes)
                .FirstOrDefaultAsync(r => r.Id == recruitmentId)
            ?? throw new RecruitmentNotFoundException(recruitmentId.ToString());

        if (recruitment.Resumes.Any(r => r.Id == resumeId))
        {
            throw new BadRequestException("已經申請過此職缺");
        }

        var resume =
            await _repository.Resume.GetAll().FirstOrDefaultAsync(r => r.Id == resumeId)
            ?? throw new ResumeNotFoundException(resumeId.ToString());

        // Owner Check
        if (resume.UserId != userId)
        {
            throw new ForbiddenException();
        }

        resume.Visibility = true;
        recruitment.Resumes.Add(resume);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<ResumeDTO>> SearchRelativeResumeAsync(
        Guid recruitmentId,
        bool searchAll = false
    )
    {
        var recruitment =
            await _recruitmentRepository
                .GetAll(r => r.Id == recruitmentId)
                .AsNoTracking()
                .Include(r => r.Resumes)
                .ThenInclude(r => r.User.Avatar)
                .Include(r => r.Keywords)
                .ThenInclude(k => k.Resumes)
                .ThenInclude(r => r.User.Avatar)
                .SingleOrDefaultAsync() ?? throw new NotFoundException("Recruitment not found");
        var query = recruitment.Keywords.SelectMany(k => k.Resumes).Distinct();

        if (!searchAll)
        {
            query = query.Where(r => recruitment.Resumes.Select(a => a.Id).Contains(r.Id));
        }

        var resumes = query
            .GroupBy(r => r.Id)
            .OrderByDescending(g => g.Count())
            .Select(g => g.First());

        return _mapper.Map<IEnumerable<ResumeDTO>>(resumes);
    }

    public async Task<IEnumerable<Guid>> GetNotAnalyzedApplyedResumeIdAsync(Guid recruitmentId)
    {
        var recruitment =
            await _recruitmentRepository
                .GetAll(r => r.Id == recruitmentId)
                .AsNoTracking()
                .Include(r => r.Resumes)
                .ThenInclude(r => r.Keywords)
                .SingleOrDefaultAsync() ?? throw new NotFoundException("Recruitment not found");
        return recruitment.Resumes.Where(r => r.Keywords.Count > 0).Select(r => r.Id);
    }

    public async Task<bool> CheckRecruitmentOwnershipAsync(Guid userId, Guid recruitmentId)
    {
        var recruitment =
            await _recruitmentRepository
                .GetAll(r => r.Id == recruitmentId)
                .AsNoTracking()
                .SingleOrDefaultAsync() ?? throw new NotFoundException("Recruitment not found");
        return recruitment.PublisherId == userId;
    }
}
