namespace IngBackend.Repository;

using IngBackendApi.Repository;
using IngBackendApi.Context;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Models.DBEntity;

public class RepositoryWrapper(IngDbContext context) : IRepositoryWrapper
{
    private readonly IngDbContext _context = context;
    private IUserRepository _userRepository;
    private IAreaRepository _areaRepository;
    private IResumeRepository _resumeRepository;
    private IRecruitmentRepository _recruitmentRepository;

    private IRepository<AreaType, int> _areaTypeRepository;
    private IRepository<TagType, int> _tagTypeRepository;

    public IUserRepository User
    {
        get
        {
            _userRepository ??= new UserRepository(_context);
            return _userRepository;
        }
    }

    public IResumeRepository Resume
    {
        get
        {
            _resumeRepository ??= new ResumeRepository(_context);
            return _resumeRepository;
        }
    }

    public IRecruitmentRepository Recruitment
    {
        get
        {
            _recruitmentRepository ??= new RecruitmentRepository(_context);
            return _recruitmentRepository;
        }
    }

    public IAreaRepository Area
    {
        get
        {
            _areaRepository ??= new AreaRepository(_context);
            return _areaRepository;
        }
    }

    public IRepository<AreaType, int> AreaType
    {
        get
        {
            _areaTypeRepository ??= new Repository<AreaType, int>(_context);
            return _areaTypeRepository;
        }
    }

    public IRepository<TagType, int> TagType
    {
        get
        {
            _tagTypeRepository ??= new Repository<TagType, int>(_context);
            return _tagTypeRepository;
        }
    }

    public void Save() => _context.SaveChanges();
}
