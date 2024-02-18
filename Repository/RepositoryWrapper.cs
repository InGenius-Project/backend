using IngBackend.Context;
using IngBackend.Interfaces.Repository;

namespace IngBackend.Repository;

public class RepositoryWrapper : IRepositoryWrapper
{
    private readonly IngDbContext _context;
    private IUserRepository _userRepository;

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

    public RepositoryWrapper(IngDbContext context)
    {
        _context = context;
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}