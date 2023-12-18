using IngBackend.Models.DBEntity;
using Microsoft.EntityFrameworkCore;

namespace IngBackend.Context;

public class IngDbContext : DbContext
{
    public IngDbContext(DbContextOptions<IngDbContext> options) : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        base.OnConfiguring(optionsBuilder);
    }

    public DbSet<InternUser> InternUser { get; set; }
    public DbSet<CompanyUser> CompanyUser { get; set; }
}
