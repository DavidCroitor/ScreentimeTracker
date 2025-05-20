using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScreentimeTracker.Data.Models;

public class AppUsageEntry
{
    [Key]
    public int Id { get; set; }
    [Required]
    public required string ApplicationName { get; set; }
    public string? ExecutablePath { get; set; }

    [Required]
    public DateTime StartTimeUtc { get; set; }
    [Required]
    public DateTime EndTimeUtc { get; set; }
    [Required]
    public long DurationSeconds { get; set; }

    [NotMapped]
    public TimeSpan Duration
    {
        get => TimeSpan.FromSeconds(DurationSeconds);
        set => DurationSeconds = (long)value.TotalSeconds;
    }

    // EF Core parameterless constructor
    public AppUsageEntry() { }

    public AppUsageEntry(
        string applicationName,
        string? executablePath,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        TimeSpan duration
    )
    {
        ApplicationName = applicationName;
        ExecutablePath = executablePath;
        StartTimeUtc = startTimeUtc;
        EndTimeUtc = endTimeUtc;
        Duration = duration;    
    }
    
    
}
