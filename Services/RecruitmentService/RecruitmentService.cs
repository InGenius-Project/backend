using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Services.AreaService;
using IngBackend.Models.DBEntity;
using Microsoft.EntityFrameworkCore;
using IngBackend.Models.DTO;
using AutoMapper;

namespace IngBackend.Services.RecruitmentService;

public class RecruitmentService : Service<Recruitment, RecruitmentDTO, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repository;

    public RecruitmentService(IUnitOfWork unitOfWork, IMapper mapper, IRepositoryWrapper repository) : base(unitOfWork, mapper)

    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _repository = repository;
    }
    public List<RecruitmentDTO> GetUserRecruitements(Guid userId)
    {
        var recruitments = _repository.Recruitment.GetAll()
            .Where(x => x.PublisherId.Equals(userId))
            .ToList();
        return _mapper.Map<List<RecruitmentDTO>>(recruitments);
    }

    public async Task<RecruitmentDTO?> GetRecruitmentByIdIncludeAllAsync(Guid recruitmentId)
    {
        var query = _repository.Recruitment.GetRecruitmentByIdIncludeAll(recruitmentId);
        return await _mapper.ProjectTo<RecruitmentDTO>(query).FirstOrDefaultAsync();
    }



}