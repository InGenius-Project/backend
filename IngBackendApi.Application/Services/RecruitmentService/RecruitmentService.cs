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
        var keywords = searchDTO.Query?.Split(" ").ToArray() ?? [];

        var query = _keywordRecordRepository
            .GetAll(k => keywords.Contains(k.Id))
            .Include(k => k.Recruitments)
            .SelectMany(k => k.Recruitments)
            .Where(r => r.Enable)
            .Distinct();

        // count total page size
        var total = await query.Select(r => r.Id).CountAsync();
        var maxPage = (int)Math.Ceiling((double)total / searchDTO.PageSize);
        searchDTO.Page = int.Max(maxPage, searchDTO.Page);

        if (orderBy == "asc")
        {
            query = query.OrderBy(r => r.CreatedAt);
        }
        else
        {
            query = query.OrderByDescending(r => r.CreatedAt);
        }
        var skip = searchDTO.PageSize * (searchDTO.Page - 1);

        // get recruitment with publisher and areas
        query = query.Include(r => r.Publisher).Include(r => r.Areas);
        query = query.Skip(skip).Take(searchDTO.PageSize);

        var result = await _mapper.ProjectTo<RecruitmentDTO>(query).ToListAsync();
        if (userId != null)
        {
            var favRecruitmentIds = _repository
                .User.GetAll(u => u.Id == userId)
                .Include(u => u.FavoriteRecruitments)
                .SelectMany(u => u.FavoriteRecruitments.Select(fr => fr.Id));
            result.ForEach(r => r.IsUserFav = favRecruitmentIds.Any(id => id == r.Id));
        }

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

    public async Task ApplyRecruitmentAsync(Guid recruitmentId, Guid resumeId, Guid userId)
    {
        var recruitment =
            await _repository
                .Recruitment.GetAll(r => r.Resumes)
                .FirstOrDefaultAsync(r => r.Id == recruitmentId)
            ?? throw new RecruitmentNotFoundException(recruitmentId.ToString());

        // if (recruitment.Resumes.Any(r => r.Id == resumeId))
        // {
        //     throw new BadRequestException("已經申請過此職缺");
        // }

        var resume =
            await _repository.Resume.GetAll().FirstOrDefaultAsync(r => r.Id == resumeId)
            ?? throw new ResumeNotFoundException(resumeId.ToString());

        // Owner Check
        if (resume.UserId != userId)
        {
            throw new ForbiddenException();
        }

        // WARNING: Remove This for testing
        recruitment.Resumes = [];

        recruitment.Resumes.Add(resume);

        await _unitOfWork.SaveChangesAsync();
    }
}
