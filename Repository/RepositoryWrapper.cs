using AutoMapper;
using IngBackend.Context;
using IngBackend.Interfaces.Repository;
using IngBackend.Models.DBEntity;

namespace IngBackend.Repository;

public class RepositoryWrapper(IngDbContext context, IMapper mapper) : IRepositoryWrapper
{
    private readonly IngDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private IUserRepository _userRepository;
    private IAreaRepository _areaRepository;
    private IResumeRepository _resumeRepository;
    private IRecruitmentRepository _recruitmentRepository;

    private IRepository<AreaType, int> _areaTypeRepository;


    private ITagRepository _tagRepository;

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
            _areaRepository ??= new AreaRepository(_context, _mapper);
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
    public ITagRepository Tag
    {
        get
        {
            _tagRepository ??= new TagRepository(_context);
            return _tagRepository;
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