using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using MemeApi.library;

namespace MemeApi.Models.Context;

public class MemeContext : IdentityDbContext<User, IdentityRole<string>, string>
{
    private readonly MemeApiSettings _settings;
    public MemeContext(DbContextOptions<MemeContext> options, MemeApiSettings settings) : base(options)
    {
        _settings = settings;
    }
    public DbSet<Meme> Memes { get; set; }
    public DbSet<MemeVisual> Visuals { get; set; }
    public DbSet<MemeSound> Sounds { get; set; }
    public DbSet<MemeText> Texts { get; set; }
    public DbSet<Vote> Votes { get; set; }
    public DbSet<Votable> Votables { get; set; }
    public DbSet<Topic> Topics { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();

        modelBuilder.Entity<User>()
            .HasMany(m => m.Votes)
            .WithOne(v => v.User)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Meme>()
            .HasOne(m => m.MemeVisual)
            .WithMany()
            .HasForeignKey("MemeVisualId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Meme>()
            .HasOne(m => m.Toptext);

        modelBuilder.Entity<Meme>()
            .HasOne(m => m.BottomText);

        modelBuilder.Entity<Vote>()
            .HasOne(v => v.User)
            .WithMany(u => u.Votes)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        modelBuilder.Entity<Vote>()
            .HasOne(v => v.Element)
            .WithMany(u => u.Votes)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        modelBuilder.Entity<Topic>()
            .HasOne(t => t.Owner)
            .WithMany(u => u.Topics)
            .HasForeignKey(t => t.OwnerId);
        
        modelBuilder.Entity<Topic>()
            .HasMany(t => t.Moderators);

        modelBuilder.Entity<Topic>()
            .HasIndex(t => t.Name)
            .IsUnique();

        modelBuilder.Entity<Votable>()
            .HasMany(v => v.Topics)
            .WithMany(t => t.Votables);

        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = _settings.GetAdminUsername() ,
            Email = _settings.GetAdminPassword(),
            SecurityStamp = DateTime.UtcNow.ToString(),
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
        };

        var defaultTopic = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            OwnerId = admin.Id,
            Name = "Swu-legacy",
            Description = "Memes created 2020-2023",
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

        var defaultTopic2 = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            OwnerId = admin.Id,
            Name = "Rotte-Grotte",
            Description = "Memes are back baby!",
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

        var memeOfTheDayTopic = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            OwnerId = admin.Id,
            Name = "MemeOfTheDay",
            Description = "Memes of the day",
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

        modelBuilder.Entity<Topic>().HasData(defaultTopic, defaultTopic2, memeOfTheDayTopic);
        modelBuilder.Entity<User>().HasData(admin);
    }
}
