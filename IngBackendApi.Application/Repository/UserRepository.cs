using IngBackend.Repository;
using IngBackendApi.Context;
using IngBackendApi.Interfaces.Repository;
using IngBackendApi.Models.DBEntity;
using Microsoft.EntityFrameworkCore;

namespace IngBackendApi.Repository;

public class UserRepository : Repository<User, Guid>, IUserRepository
{
    private readonly IngDbContext _context;

    public UserRepository(IngDbContext context)
        : base(context)
    {
        _context = context;
    }

    public IQueryable<User> GetUserByIdIncludeAll(Guid id)
    {
        return _context
            .User.Where(u => u.Id == id)
            .Include(u => u.FavoriteRecruitments)
            .Include(u => u.Avatar)
            .Include(u => u.Recruitments)
            .ThenInclude(r => r.Areas)
            .Include(u => u.Recruitments)
            .ThenInclude(r => r.Publisher)
            .Include(u => u.Areas)
            .ThenInclude(a => a.TextLayout)
            .Include(u => u.Areas)
            .ThenInclude(a => a.ImageTextLayout.Image)
            .Include(u => u.Areas)
            .ThenInclude(a => a.ListLayout.Items)
            .ThenInclude(i => i.Type)
            .Include(u => u.Areas)
            .ThenInclude(a => a.KeyValueListLayout.Items)
            .ThenInclude(i => i.Key)
            .ThenInclude(t => t.Type)
            .Include(u => u.Areas)
            .ThenInclude(a => a.AreaType);
    }

    public IQueryable<Resume> GetResumesByUserId(Guid id)
    {
        var resumes = _context
            .User.Include(u => u.Resumes)
            .Where(u => u.Id == id)
            .SelectMany(u => u.Resumes);
        return resumes;
    }

    public IQueryable<User> GetUserByEmail(string email)
    {
        return _context.User.Where(u => u.Email == email);
    }
}
