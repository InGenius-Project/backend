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
        modelBuilder
            .Entity<TagType>()
            .HasData(
                new TagType
                {
                    Id = 1,
                    Name = "custom",
                    Value = "custom",
                    Color = "#ffffff"
                },
                new TagType
                {
                    Id = 2,
                    Name = "fake",
                    Value = "fake",
                    Color = "#ffffff"
                },
                new TagType
                {
                    Id = 3,
                    Name = "university",
                    Value = "university",
                    Color = "#ffffff"
                },
                new TagType
                {
                    Id = 4,
                    Name = "department",
                    Value = "department",
                    Color = "#ffffff"
                }
            );

        modelBuilder.Entity<AreaType>().HasIndex(t => t.Value).IsUnique();
        modelBuilder.Entity<AreaType>().Property(t => t.Id).ValueGeneratedOnAdd();

        modelBuilder
            .Entity<AreaType>()
            .HasData(
                new AreaType
                {
                    Id = 1,
                    Name = "custom",
                    Value = "unique",
                    Description = "whatever",
                    UserRole = [Enum.UserRole.InternalUser, Enum.UserRole.Admin],
                    LayoutType = Enum.LayoutType.List,
                    CreatedAt = DateTime.UtcNow
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

        // Resume
        var collegeTag = new Tag()
        {
            Id = Guid.NewGuid(),
            Name = "中正大學",
            TagTypeId = 3,
            Count = 0
        };

        var departmentTag = new Tag()
        {
            Id = Guid.NewGuid(),
            Name = "資訊管理系",
            TagTypeId = 4,
            Count = 0
        };

        var educationalKeyValueItem = new KeyValueItem() { Id = Guid.NewGuid(), Value = "大學", };

        var educationalKeyValueListLayout = new KeyValueListLayout() { Id = Guid.NewGuid() };

        var educationArea = new Area()
        {
            Id = Guid.NewGuid(),
            Sequence = 1,
            IsDisplayed = true,
            Title = "教育背景",
            OwnerId = adminUser.Id,
        };

        modelBuilder.Entity<Tag>().HasData(collegeTag, departmentTag);
        educationalKeyValueListLayout.AreaId = educationArea.Id;
        educationalKeyValueItem.keyValueListLayoutId = educationalKeyValueListLayout.Id;
        modelBuilder.Entity<Area>().HasData(educationArea);
        modelBuilder.Entity<KeyValueListLayout>().HasData(educationalKeyValueListLayout);
        modelBuilder.Entity<KeyValueItem>().HasData(educationalKeyValueItem);
    }
}
