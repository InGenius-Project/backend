using IngBackend.Context;
using IngBackend.Interfaces.Repository;
using IngBackend.Models.DBEntity;
using Microsoft.EntityFrameworkCore;

namespace IngBackend.Repository;

public class UserRepository : Repository<User, Guid>, IUserRepository
{
    private readonly IngDbContext _context;

    public UserRepository(IngDbContext context) : base(context)
    {
        _context = context;
    }
    public IQueryable<User> GetUserById(Guid id)
    {
        return _context.User
            .Include(u => u.Avatar)
            .Include(u => u.Areas)
                .ThenInclude(a => a.TextLayout)
            .Include(u => u.Areas)
                .ThenInclude(a => a.ImageTextLayout)
                    .ThenInclude(itl => itl.Image)
            .Include(u => u.Areas)
                .ThenInclude(a => a.ListLayout)
                    .ThenInclude(l => l.Items)
            .Include(u => u.Areas)
                .ThenInclude(a => a.KeyValueListLayout)
                    .ThenInclude(kv => kv.Items)
                    .ThenInclude(kvi => kvi.Key)
            .Include(u => u.Areas)
                .ThenInclude(a => a.AreaType)
            .Include(u => u.Recruitments)
            .Where(u => u.Id == id);
    }
}