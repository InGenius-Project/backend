using IngBackend.Interfaces.Service;
using IngBackend.Models.DBEntity;
using IngBackend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace IngBackend.Context;

public class IngDbContext : DbContext
{

    public IngDbContext(DbContextOptions<IngDbContext> options)
        : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    public DbSet<User> User { get; set; }
    public DbSet<Resume> Resume { get; set; }
    public DbSet<Recruitment> Recruitment { get; set; }
    public DbSet<Area> Area { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Resume>()
            .HasMany(e => e.Recruitments)
            .WithMany(e => e.Resumes)
            .UsingEntity<Dictionary<string, object>>(
                "ResumeRecruitment",
                l => l.HasOne<Recruitment>().WithMany().OnDelete(DeleteBehavior.NoAction),
                r => r.HasOne<Resume>().WithMany().OnDelete(DeleteBehavior.NoAction)
            );

        modelBuilder.Entity<TagType>()
            .HasIndex(t => t.Value).IsUnique();
        modelBuilder.Entity<TagType>()
            .Property(t => t.Id).ValueGeneratedOnAdd();

        modelBuilder.Entity<AreaType>()
            .HasIndex(t => t.Value).IsUnique();
        modelBuilder.Entity<AreaType>()
            .Property(t => t.Id).ValueGeneratedOnAdd();


        PasswordHasher hasher = new PasswordHasher();
        User user = new User
        {
            Id = new Guid("f5a41815-6233-4a4a-9e62-108f0d09a8ce"),
            Username = "User",
            Email = "user@gmail.com",
            HashedPassword = hasher.HashPassword("testtest"),
            Role = Enum.UserRole.Intern
        };


        User internalUser = new User
        {
            Id = new Guid("d6e2c7c3-89a5-4d8e-b74b-7af6f79e7348"),
            Username = "Internal",
            Email = "i@gmail.com",
            HashedPassword = hasher.HashPassword("testtest"),
            Role = Enum.UserRole.InternalUser
        };
        modelBuilder.Entity<User>().HasData(user, internalUser);
    }
}