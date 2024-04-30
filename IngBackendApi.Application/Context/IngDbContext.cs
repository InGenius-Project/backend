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
    public DbSet<BackgroundTask> BackgroundTask { get; set; }

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

        modelBuilder.Entity<Tag>().HasMany(t => t.Owners).WithMany(u => u.OwnedTags);
        // Resume Area
        modelBuilder
            .Entity<Resume>()
            .HasMany(r => r.Areas)
            .WithOne(a => a.Resume)
            .HasForeignKey(a => a.ResumeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder
            .Entity<User>()
            .HasMany(u => u.Resumes)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder
            .Entity<Resume>()
            .HasOne(u => u.User)
            .WithMany(r => r.Resumes)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder
            .Entity<Recruitment>()
            .HasMany(r => r.Areas)
            .WithOne(a => a.Recruitment)
            .HasForeignKey(a => a.RecruitmentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder
            .Entity<Recruitment>()
            .HasOne(r => r.SafetyReport)
            .WithOne(s => s.Recruitment)
            .HasForeignKey<SafetyReport>(s => s.RecruitmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<SafetyReport>()
            .HasOne(r => r.Recruitment)
            .WithOne(s => s.SafetyReport)
            .HasForeignKey<Recruitment>(s => s.SafetyReportId)
            .OnDelete(DeleteBehavior.SetNull);

        #region Area Layouts
        modelBuilder
            .Entity<Area>()
            .HasOne(a => a.KeyValueListLayout)
            .WithOne(a => a.Area)
            .HasForeignKey<Area>(a => a.KeyValueListLayoutId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<Area>()
            .HasOne(a => a.ListLayout)
            .WithOne(a => a.Area)
            .HasForeignKey<Area>(a => a.ListLayoutId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<Area>()
            .HasOne(a => a.TextLayout)
            .WithOne(a => a.Area)
            .HasForeignKey<Area>(a => a.TextLayoutId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<Area>()
            .HasOne(a => a.ImageTextLayout)
            .WithOne(a => a.Area)
            .HasForeignKey<Area>(a => a.ImageTextLayoutId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion

        // User Area Relationship
        modelBuilder.Entity<Area>().HasOne(a => a.Owner);
        modelBuilder.Entity<Area>().HasOne(a => a.User).WithMany(u => u.Areas);

        modelBuilder.Entity<TagType>().HasIndex(t => t.Value).IsUnique();
        modelBuilder.Entity<TagType>().Property(t => t.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().HasMany(u => u.Recruitments).WithOne(t => t.Publisher);
        modelBuilder.Entity<User>().HasMany(u => u.ChatRooms).WithMany(c => c.Users);
        modelBuilder.Entity<User>().HasMany(u => u.InvitedChatRooms).WithMany(c => c.InvitedUsers);
        modelBuilder.Entity<ChatGroup>().HasOne(u => u.Owner).WithMany(t => t.OwnedChatRooms);

        // delete messages when chat room is deleted
        modelBuilder
            .Entity<ChatGroup>()
            .HasMany(u => u.Messages)
            .WithOne(t => t.Group)
            .HasForeignKey(t => t.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<User>()
            .HasMany(a => a.FavoriteRecruitments)
            .WithMany(r => r.FavoriteUsers);

        modelBuilder.Entity<User>().HasMany(u => u.Recruitments).WithOne(t => t.Publisher);
        modelBuilder.Entity<User>().HasMany(u => u.ChatRooms).WithMany(c => c.Users);
        modelBuilder.Entity<User>().HasMany(u => u.InvitedChatRooms).WithMany(c => c.InvitedUsers);
        modelBuilder.Entity<ChatGroup>().HasOne(u => u.Owner).WithMany(t => t.OwnedChatRooms);

        // delete messages when chat room is deleted
        modelBuilder
            .Entity<ChatGroup>()
            .HasMany(u => u.Messages)
            .WithOne(t => t.Group)
            .HasForeignKey(t => t.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        #region Default TagType
        var customTagType = new TagType
        {
            Id = 1,
            Name = "自定義",
            Value = "custom",
            Color = "#ffffff",
        };
        var skillTagType = new TagType
        {
            Id = 2,
            Name = "技能",
            Value = "skill",
            Color = "#123"
        };
        var universityTagType = new TagType
        {
            Id = 3,
            Name = "大學",
            Value = "university",
            Color = "#ffffff"
        };
        var departmentTagType = new TagType
        {
            Id = 4,
            Name = "科系",
            Value = "department",
            Color = "#ffffff"
        };
        modelBuilder
            .Entity<TagType>()
            .HasData(customTagType, departmentTagType, universityTagType, skillTagType);
        #endregion

        modelBuilder.Entity<AreaType>().HasIndex(t => t.Value).IsUnique();
        modelBuilder.Entity<AreaType>().Property(t => t.Id).ValueGeneratedOnAdd();
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
                },
                new AreaType
                {
                    Id = 2,
                    Name = "自我介紹",
                    Value = "self-introduction",
                    Description = "自我介紹",
                    UserRole = [Enum.UserRole.Intern],
                    LayoutType = Enum.LayoutType.Text,
                }
            );

        modelBuilder
            .Entity<AreaType>()
            .HasMany(a => a.ListTagTypes)
            .WithMany(t => t.AreaTypes)
            .UsingEntity<Dictionary<string, object>>(
                "AreaTypeTagType",
                l =>
                    l.HasOne<TagType>()
                        .WithMany()
                        .OnDelete(DeleteBehavior.NoAction)
                        .HasForeignKey("TagTypeId"),
                r =>
                    r.HasOne<AreaType>()
                        .WithMany()
                        .OnDelete(DeleteBehavior.NoAction)
                        .HasForeignKey("AreaTypeId"),
                at =>
                {
                    at.HasKey("AreaTypeId", "TagTypeId");
                    at.HasData(new { AreaTypeId = 1, TagTypeId = skillTagType.Id });
                }
            );
    }

    private void SeedTestingData(
        ModelBuilder modelBuilder,
        TagType universityTagType,
        TagType departmentTagType,
        TagType skillTagType
    )
    {
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

        #region Ai generation test data
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
        var educationAreaType = new AreaType()
        {
            Id = 5,
            Name = "教育背景",
            Value = "education",
            Description = "教育背景的描述",
            UserRole = [Enum.UserRole.Intern, Enum.UserRole.Admin, Enum.UserRole.Company],
            LayoutType = Enum.LayoutType.KeyValueList,
        };

        modelBuilder
            .Entity<AreaType>()
            .HasMany(a => a.ListTagTypes)
            .WithMany(t => t.AreaTypes)
            .UsingEntity(i =>
                i.HasData(
                    new { AreaTypeId = educationAreaType.Id, TagTypeId = universityTagType.Id },
                    new { AreaTypeId = educationAreaType.Id, TagTypeId = departmentTagType.Id }
                )
            );

        modelBuilder.Entity<Tag>().HasData(collegeTag, departmentTag);
        modelBuilder.Entity<AreaType>().HasData(educationAreaType);
        educationalKeyValueListLayout.AreaId = educationArea.Id;
        educationalKeyValueItem.keyValueListLayoutId = educationalKeyValueListLayout.Id;
        educationArea.AreaTypeId = educationAreaType.Id;
        educationArea.UserId = internalUser.Id;
        educationArea.OwnerId = internalUser.Id;
        modelBuilder.Entity<Area>().HasData(educationArea);
        modelBuilder.Entity<KeyValueListLayout>().HasData(educationalKeyValueListLayout);
        modelBuilder.Entity<KeyValueItem>().HasData(educationalKeyValueItem);
        modelBuilder
            .Entity<KeyValueItem>()
            .HasMany(k => k.Key)
            .WithMany(t => t.KeyValueItems)
            .UsingEntity(i =>
                i.HasData(
                    new { KeyValueItemsId = educationalKeyValueItem.Id, KeyId = collegeTag.Id },
                    new { KeyValueItemsId = educationalKeyValueItem.Id, KeyId = departmentTag.Id }
                )
            );

        var skillTag = new Tag()
        {
            Id = Guid.NewGuid(),
            Name = "C#",
            TagTypeId = skillTagType.Id,
            Count = 0
        };

        var skillTag2 = new Tag()
        {
            Id = Guid.NewGuid(),
            Name = "Python",
            TagTypeId = skillTagType.Id,
            Count = 0
        };

        var skillArea = new Area()
        {
            Id = Guid.NewGuid(),
            Sequence = 2,
            IsDisplayed = true,
            Title = "我的技能",
            AreaTypeId = 1,
            UserId = internalUser.Id,
            OwnerId = internalUser.Id
        };

        var skillAreaListLayout = new ListLayout() { Id = Guid.NewGuid() };
        skillArea.ListLayoutId = skillAreaListLayout.Id;

        modelBuilder.Entity<Tag>().HasData(skillTag, skillTag2);
        modelBuilder.Entity<ListLayout>().HasData(skillAreaListLayout);
        modelBuilder.Entity<Area>().HasData(skillArea);
        modelBuilder
            .Entity<ListLayout>()
            .HasMany(l => l.Items)
            .WithMany(t => t.ListLayouts)
            .UsingEntity(i =>
                i.HasData(
                    new { ListLayoutsId = skillAreaListLayout.Id, ItemsId = skillTag.Id },
                    new { ListLayoutsId = skillAreaListLayout.Id, ItemsId = skillTag2.Id }
                )
            );

        var selfIntroArea = new Area()
        {
            Id = Guid.NewGuid(),
            Sequence = 3,
            IsDisplayed = true,
            Title = "自我介紹",
            AreaTypeId = 2,
            UserId = internalUser.Id,
            OwnerId = internalUser.Id
        };

        var selfIntroTextLayout = new TextLayout()
        {
            Id = Guid.NewGuid(),
            Content = "我是一個熱愛程式設計的學生，對於新技術有很大的興趣，希望能夠在這裡學到更多的東西。",
            AreaId = selfIntroArea.Id
        };

        modelBuilder.Entity<Area>().HasData(selfIntroArea);
        modelBuilder.Entity<TextLayout>().HasData(selfIntroTextLayout);
        #endregion
    }
}
