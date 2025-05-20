namespace ScreentimeTracker.Data.Models;

public class DailyTotalStat
{
    public DayOfWeek Day { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan TotalScreenTime { get; set; }
}