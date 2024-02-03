﻿using IngBackend.Models.DBEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

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

    public DbSet<User> User { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resume>()
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
        
            
    }
}
