namespace IngBackend.Context;
using IngBackend.Models.DBEntity;
using IngBackend.Services;
using Microsoft.EntityFrameworkCore;

public class IngDbContext(DbContextOptions<IngDbContext> options) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => base.OnConfiguring(optionsBuilder);

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

        modelBuilder.Entity<TagType>().HasIndex(t => t.Value).IsUnique();
        modelBuilder.Entity<TagType>().Property(t => t.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        // TODO: Default data
        modelBuilder
            .Entity<TagType>()
            .HasData(
                new TagType
                {
                    Id = 1,
                    Name = "custom",
                    Value = "custom",
                    Color = "#ffffff"
                }
            );

        modelBuilder.Entity<AreaType>().HasIndex(t => t.Value).IsUnique();
        modelBuilder.Entity<AreaType>().Property(t => t.Id).ValueGeneratedOnAdd();

        // TODO: Default Data
        modelBuilder
            .Entity<AreaType>()
            .HasData(
                new AreaType
                {
                    Id = 1,
                    Name = "custom",
                    Value = "unique",
                    Description = "whatever",
                    UserRole =
                    [
                        Enum.UserRole.InternalUser,
                        Enum.UserRole.Admin
                    ],
                    LayoutType = Enum.LayoutType.List,
                    CreatedAt = DateTime.UtcNow
                }
            );


        var hasher = new PasswordHasher();
        var user = new User
        {
            Id = new Guid("f5a41815-6233-4a4a-9e62-108f0d09a8ce"),
            Username = "User",
            Email = "user@gmail.com",
            HashedPassword = hasher.HashPassword("testtest"),
            Role = Enum.UserRole.Intern
        };


        var internalUser = new User
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
