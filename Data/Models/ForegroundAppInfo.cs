namespace ScreentimeTracker.Data.Models;

public class ForegroundAppInfo
{
    public string ProcessName { get; init; } = string.Empty;
    public string ExecutablePath { get; init; } = string.Empty;
    public string WindowTitle { get; init; } = string.Empty; // Will be empty for now
    public bool IsValid { get; init; } = false;
    public string ErrorMessage { get; init; } = string.Empty; // For debugging issues
}