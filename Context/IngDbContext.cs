using IngBackend.Interfaces.Service;
using IngBackend.Models.DBEntity;
using IngBackend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Org.BouncyCastle.Bcpg;

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
    public DbSet<AreaType> AreaType { get; set; }
    public DbSet<Tag> Tag { get; set; }
    public DbSet<TagType> TagType { get; set; }

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

        modelBuilder.Entity<AreaType>().HasIndex(t => t.Value).IsUnique();
        modelBuilder.Entity<AreaType>().Property(t => t.Id).ValueGeneratedOnAdd();

        modelBuilder.Entity<ListLayout>()
        .HasMany(l => l.Items)
        .WithMany(t => t.ListLayouts)
        .UsingEntity(
                        "ListLayoutTag",
                        l => l.HasOne(typeof(Tag)).WithMany().HasForeignKey("TagId").HasPrincipalKey(nameof(Models.DBEntity.Tag.Id)),
                        r => r.HasOne(typeof(ListLayout)).WithMany().HasForeignKey("ListLayoutId").HasPrincipalKey(nameof(ListLayout.Id)),
                        j => j.HasKey("TagId", "ListLayoutId"));

        // Data seeding

        TagType tagType =
            new()
            {
                Id = 1,
                Name = "Custom",
                Value = "Custom",
                Color = "#000"
            };

        modelBuilder.Entity<TagType>().HasData(tagType);

        // Dummy data

        Tag tag =
            new()
            {
                Id = new Guid("1f2e6d84-7a4c-4d0b-9b8f-3e8f5a2c6d9a"),
                Name = "React",
                TypeId = 1,
                Count = 0,
            };
        modelBuilder.Entity<Tag>().HasData(tag);

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

