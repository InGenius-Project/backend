﻿using IngBackend.Models.DBEntity;
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
                    UserRole = new List<Enum.UserRole>
                    {
                        Enum.UserRole.InternalUser,
                        Enum.UserRole.Admin
                    },
                    LayoutType = Enum.LayoutType.List,
                    CreatedAt = DateTime.UtcNow
                }
            );
    }
}
