using System.ComponentModel.DataAnnotations.Schema;

namespace ScreentimeTracker.Data.Models;

public class AppUsageStat
{
    public required string ApplicationName { get; set; }
    public required string ExecutablePath { get; set; }
    public long TotalDurationSeconds { get; set; }
    public int SessionCount { get; set; }
    public double PercentageOfDay { get; set; }

    [NotMapped]
    public TimeSpan TotalDuration => TimeSpan.FromSeconds(TotalDurationSeconds);
    
}