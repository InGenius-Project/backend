namespace IngBackendApi.Services.RecruitmentService;

using AutoMapper;
using IngBackendApi.Application.Interfaces.Service;
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
            .Recruitment.GetAll()
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

        var publisher = await _repository.User.GetByIdAsync(recruitment.PublisherId);
        recruitment.Publisher = publisher;
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
}
