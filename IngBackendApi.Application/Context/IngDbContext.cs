namespace IngBackendApi.Context;

using IngBackendApi.Models.DBEntity;
using IngBackendApi.Services;
using Microsoft.EntityFrameworkCore;

public class IngDbContext(DbContextOptions<IngDbContext> options) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        base.OnConfiguring(optionsBuilder);

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

        // User Area Relationship
        modelBuilder.Entity<Area>().HasOne(a => a.Owner);
        modelBuilder.Entity<Area>().HasOne(a => a.User).WithMany(u => u.Areas);

        modelBuilder.Entity<TagType>().HasIndex(t => t.Value).IsUnique();
        modelBuilder.Entity<TagType>().Property(t => t.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<User>().HasMany(u => u.Recruitments).WithOne(t => t.Publisher);

        modelBuilder
            .Entity<User>()
            .HasMany(a => a.FavoriteRecruitments)
            .WithMany(r => r.FavoriteUsers);

        // TODO: Default data
        var skillTagType = new TagType
        {
            Id = 2,
            Name = "技能",
            Value = "skill",
            Color = "#123"
        };

        modelBuilder
            .Entity<TagType>()
            .HasData(
                new TagType
                {
                    Id = 1,
                    Name = "自定義",
                    Value = "custom",
                    Color = "#ffffff",
                },
                skillTagType
            );

        modelBuilder.Entity<AreaType>().HasIndex(t => t.Value).IsUnique();
        modelBuilder.Entity<AreaType>().Property(t => t.Id).ValueGeneratedOnAdd();

        // TODO: Default Data
        modelBuilder
            .Entity<Tag>()
            .HasData(
                new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "React",
                    TagTypeId = skillTagType.Id,
                }
            );
        modelBuilder
            .Entity<Tag>()
            .HasData(
                new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "Web Design",
                    TagTypeId = skillTagType.Id,
                }
            );


        modelBuilder
            .Entity<AreaType>()
            .HasData(
                new AreaType
                {
                    Id = 1,
                    Name = "技能",
                    Value = "skill",
                    Description = "編輯技能",
                    UserRole = [Enum.UserRole.Intern],
                    LayoutType = Enum.LayoutType.List,
                    CreatedAt = DateTime.UtcNow,
                }
            );

        modelBuilder
            .Entity<AreaType>()
            .HasMany(a => a.ListTagTypes)
            .WithMany(t => t.AreaTypes)
            .UsingEntity<Dictionary<string, object>>(
                "AreaTypeTagType",
                l => l.HasOne<TagType>().WithMany().OnDelete(DeleteBehavior.NoAction).HasForeignKey("TagTypeId"),
                r => r.HasOne<AreaType>().WithMany().OnDelete(DeleteBehavior.NoAction).HasForeignKey("AreaTypeId"),
                at =>
                {
                    at.HasKey("AreaTypeId", "TagTypeId");
                    at.HasData(
                        new { AreaTypeId = 1, TagTypeId = skillTagType.Id }
                    );
                }
            );


        var hasher = new PasswordHasher();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "User",
            Email = "user@gmail.com",
            HashedPassword = hasher.HashPassword("testtest"),
            Role = Enum.UserRole.Intern
        };

        var companyUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "Company",
            Email = "c@gmail.com",
            HashedPassword = hasher.HashPassword("testtest"),
            Role = Enum.UserRole.Company
        };

        var internalUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "Internal",
            Email = "i@gmail.com",
            HashedPassword = hasher.HashPassword("testtest"),
            Role = Enum.UserRole.InternalUser
        };

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "Admin",
            Email = "a@gmail.com",
            HashedPassword = hasher.HashPassword("testtest"),
            Role = Enum.UserRole.Admin
        };
        modelBuilder.Entity<User>().HasData(user, internalUser, companyUser, adminUser);
    }
}
