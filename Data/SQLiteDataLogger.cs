using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ScreentimeTracker.Data.Models;

namespace ScreentimeTracker.Data;

public class SQLiteDataLogger : IDataLogger
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public SQLiteDataLogger(DbContextOptions<AppDbContext> dbContextOptions)
    {
        _dbContextOptions = dbContextOptions;
        EnsureInitialized();
    }
    private AppDbContext CreateDbContext()
    {
        return new AppDbContext(_dbContextOptions);
    }
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void EnsureInitialized()
    {
        try
        {
            using (var context = CreateDbContext())
            {
                context.Database.EnsureCreated();
            }
            Console.WriteLine("Database initialized successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing database: {ex.Message}");
            throw;
        }

    }

    public async Task<IEnumerable<AppUsageStat>> GetUsageStatsAsync(DateTime startDateUtc, DateTime endDateUtc)
    {
        await using (var context = CreateDbContext())
        {
            return await context.AppSessions
                .Where(s => s.StartTimeUtc >= startDateUtc && s.EndTimeUtc <= endDateUtc)
                .GroupBy(s => s.ApplicationName)                .Select(g => new AppUsageStat
                {
                    ApplicationName = g.Key,
                    ExecutablePath = g.Select(s => s.ExecutablePath).FirstOrDefault() ?? string.Empty,
                    TotalDurationSeconds = g.Sum(s => s.DurationSeconds),
                    SessionCount = g.Count()
                })
                .OrderByDescending(stat => stat.TotalDurationSeconds)
                .ToListAsync();
        }
    }

    public async Task LogSessionAsync(AppUsageEntry sessionEntry)
    {
        if (sessionEntry == null)
        {
            throw new ArgumentNullException(nameof(sessionEntry));
        }

        try
        {
            await using (var context = CreateDbContext())
            {
                await context.AppSessions.AddAsync(sessionEntry);
                await context.SaveChangesAsync();
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging session for {sessionEntry.ApplicationName}: {ex.Message}");
        }
    }
}