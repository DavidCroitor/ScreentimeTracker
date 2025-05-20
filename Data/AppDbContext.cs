using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using ScreentimeTracker.Data.Models;


namespace ScreentimeTracker.Data;

public class AppDbContext : DbContext
{
    public DbSet<AppUsageEntry> AppSessions { get; set; }
    private readonly string? _databasePath;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public AppDbContext(string databaseDirectory)
    {
        if (!Directory.Exists(databaseDirectory))
        {
            Directory.CreateDirectory(databaseDirectory);
        }
        _databasePath = Path.Combine(databaseDirectory, "screentime_tracker.sqlite");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            if (string.IsNullOrEmpty(_databasePath))
            {
                var folder = Environment.SpecialFolder.LocalApplicationData;
                var path = Environment.GetFolderPath(folder);
                var dbFolderPath = Path.Combine(path, "ScreenTimeTracker");
                if (!Directory.Exists(dbFolderPath))
                {
                    var parentDir = Path.GetDirectoryName(dbFolderPath);
                    if (parentDir != null)
                    {
                        Directory.CreateDirectory(parentDir);
                    }
                }
                var dbPath = Path.Combine(dbFolderPath, "sreetime_efcore.sqlite");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
            else
            {
                optionsBuilder.UseSqlite($"Data Source={_databasePath}");
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUsageEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.StartTimeUtc);
            entity.HasIndex(e => e.ApplicationName);
        });
    }

}