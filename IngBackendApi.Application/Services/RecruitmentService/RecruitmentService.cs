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

    public async Task<List<RecruitmentDTO>> GetUserRecruitementsAsync(Guid userId)
    {
        var recruitments = await _repository
            .Recruitment.GetIncludeAll()
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
        // count total page size
        var sortBy = searchDTO.SortBy ?? "CreatedTime";
        var orderBy = searchDTO.OrderBy == "asc" ? "asc" : "desc";
        var keywords = searchDTO.Query?.Split(" ").ToArray() ?? [];
        var query = _repository
            .Recruitment.GetAll()
            .Where(r => r.Enable)
            .Include(r => r.Publisher)
            .Include(r => r.Areas)
            .ThenInclude(a => a.TextLayout)
            .AsQueryable();

        if (keywords != null && keywords.Length > 0)
        {
            query = query.Where(r =>
                keywords.All(keyword =>
                    r.Areas.Select(a => a.TextLayout == null ? "" : a.TextLayout.Content)
                        .Any(content => content.Contains(keyword))
                )
            );
        }
        if (orderBy == "asc")
        {
            query = query.OrderBy(r => r.CreatedAt);
        }
        else
        {
            query = query.OrderByDescending(r => r.CreatedAt);
        }

        var total = await query.Select(r => r.Id).CountAsync();
        var maxPage = (int)Math.Ceiling((double)total / searchDTO.PageSize);
        searchDTO.Page = int.Max(maxPage, searchDTO.Page);
        var skip = searchDTO.PageSize * (searchDTO.Page - 1);
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


    public async Task ApplyRecruitmentAsync(Guid recruitmentId, ResumeDTO resumeDTO)
    {
        var recruitment = await _repository.Recruitment.GetByIdAsync(recruitmentId) ?? throw new NotFoundException("職缺");

        recruitment.Resumes.Add(_mapper.Map<Resume>(resumeDTO));

        await _unitOfWork.SaveChangesAsync();
    }
}
