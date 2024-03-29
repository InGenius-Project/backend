using IngBackend.Context;
using IngBackend.Interfaces.Repository;
using IngBackend.Models.DBEntity;

namespace IngBackend.Repository;

public class RepositoryWrapper : IRepositoryWrapper
{
    private readonly IngDbContext _context;
    private IUserRepository _userRepository;
    private IAreaRepository _areaRepository;
    private IResumeRepository _resumeRepository;
    private IRecruitmentRepository _recruitmentRepository;

    private IRepository<AreaType, int> _areaTypeRepository;



    public IUserRepository User
    {
        get
        {
            if (_userRepository == null)
            {
                _userRepository = new UserRepository(_context);
            }
            return _userRepository;
        }
    }

    public IResumeRepository Resume
    {
        get
        {
            if (_resumeRepository == null)
            {
                _resumeRepository = new ResumeRepository(_context);
            }
            return _resumeRepository;
        }
    }

    public IRecruitmentRepository Recruitment
    {
        get
        {
            if (_recruitmentRepository == null)
            {
                _recruitmentRepository = new RecruitmentRepository(_context);
            }
            return _recruitmentRepository;
        }
    }



    public IAreaRepository Area
    {
        get
        {
            if (_areaRepository == null)
            {
                _areaRepository = new AreaRepository(_context);
            }
            return _areaRepository;
        }
    }

    public IRepository<AreaType, int> AreaType
    {
        get
        {
            if (_areaTypeRepository == null)
            {
                _areaTypeRepository = new Repository<AreaType, int>(_context);
            }
            return _areaTypeRepository;
        }

    }


    public RepositoryWrapper(IngDbContext context)
    {
        _context = context;
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}