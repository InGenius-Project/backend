using IngBackend.Interfaces.Repository;
using IngBackend.Interfaces.UnitOfWork;
using IngBackend.Models.DBEntity;

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

    public IQueryable<Resume> GetResumeByUser(Guid userid)
    {
        var resumes = _resumeRepository.GetAll()
            .Where(x => x.UserId.Equals(userid));
        return resumes;
    }

}
