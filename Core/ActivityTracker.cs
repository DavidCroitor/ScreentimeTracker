using System;
using System.Diagnostics;
using ScreentimeTracker.Data;
using ScreentimeTracker.Data.Models;

namespace ScreentimeTracker.Core;


public class ActivityTracker
{
    private readonly UserActivityMonitor _userActivityMonitor;
    private readonly IDataLogger _dataLogger;
    private readonly TimeSpan _idleThreshold;

    // Session state tracking
    private string _currentTrackedAppName = string.Empty;
    private string _currentTrackedExecutablePath = string.Empty;
    private DateTime _currentSessionStartTimeUtc;
    private bool _isCurrentlyTrackingActiveSession;
    private bool _wasUserPreviouslyIdle = true; // Start assuming idle until first activity
    
    public ActivityTracker(
        IDataLogger dataLogger,
        UserActivityMonitor userActivityMonitor,
        TimeSpan? idleThreshold = null)
    {
        _dataLogger = dataLogger ?? throw new ArgumentNullException(nameof(dataLogger));
        _userActivityMonitor = userActivityMonitor ?? throw new ArgumentNullException(nameof(userActivityMonitor));
        _idleThreshold = idleThreshold ?? TimeSpan.FromMilliseconds(120 * 1000); // Default 2 minutes
    }

    public async Task ProcessTickAsync()
    {
        ForegroundAppInfo appInfo = _userActivityMonitor.GetCurrentForegroundAppInfo();
        bool isUserConsideredActiveNow = _userActivityMonitor.IsUserConsideredActive(_idleThreshold);

        if (!isUserConsideredActiveNow)
        {
            await HandleUserIdleAsync();
            return;
        }
        if (_wasUserPreviouslyIdle)
        {
            Debug.WriteLine($"User became active. Previous App:{_currentTrackedAppName}");
        }

        _wasUserPreviouslyIdle = false;

        if (!appInfo.IsValid || string.IsNullOrEmpty(appInfo.ProcessName))
        {
            await HandleInvalidOrNoForegroundAppAsync();
            return;
        }

        await HandleApplicationChangeAsync(appInfo);
    }

    private async Task HandleUserIdleAsync()
    {
        if (_isCurrentlyTrackingActiveSession)
        {
            Debug.WriteLine($"User idle. Ending session for: {_currentTrackedAppName}");
            await LogEndOfSessionAsync();

            _isCurrentlyTrackingActiveSession = false;
            _currentTrackedAppName = "IDLE";
            _currentTrackedExecutablePath = string.Empty;
        }

        _wasUserPreviouslyIdle = true;
    }

    private async Task HandleInvalidOrNoForegroundAppAsync()
    {
        if (_isCurrentlyTrackingActiveSession)
        {
            Debug.WriteLine($"No valid foreground app. Ending session for: {_currentTrackedAppName}");
            await LogEndOfSessionAsync();
        }
            _isCurrentlyTrackingActiveSession = false;
            _currentTrackedAppName = "DESKTOP/UNKNOWN";
            _currentTrackedExecutablePath = string.Empty;
    }

    private async Task HandleApplicationChangeAsync(ForegroundAppInfo appInfo)
    {
        string newAppName = appInfo.ProcessName;
        string newExecutablePath = appInfo.ExecutablePath;
        bool appIdentifierChanged = newExecutablePath != _currentTrackedExecutablePath || newAppName != _currentTrackedAppName;

        // App changed or user just became active
        if (appIdentifierChanged || _wasUserPreviouslyIdle)
        {
            if (_isCurrentlyTrackingActiveSession)
            {
                // End the previous session if it was a valid tracked app
                if (_currentTrackedAppName != "IDLE" && _currentTrackedAppName != "DESKTOP/UNKNOWN")
                {
                    Debug.WriteLine($"App changed from {_currentTrackedAppName} to {newAppName}. Ending old session.");
                    await LogEndOfSessionAsync(); // Changed to async
                }
            }

            // Start a new session
            _currentTrackedAppName = newAppName;
            _currentTrackedExecutablePath = newExecutablePath;
            _currentSessionStartTimeUtc = DateTime.Now;
            _isCurrentlyTrackingActiveSession = true;

            Debug.WriteLine($"New session: App: {_currentTrackedAppName}, Path: {_currentTrackedExecutablePath}, Start: {_currentSessionStartTimeUtc}");
        }
    }

    private async Task LogEndOfSessionAsync()
    {
        if (!_isCurrentlyTrackingActiveSession ||
               string.IsNullOrEmpty(_currentTrackedAppName) ||
               _currentTrackedAppName == "IDLE" ||
               _currentTrackedAppName == "DESKTOP/UNKNOWN")
        {
            _isCurrentlyTrackingActiveSession = false;
            return;
        }

        DateTime endTimeUtc = DateTime.Now;
        TimeSpan duration = endTimeUtc - _currentSessionStartTimeUtc;

        // Log to debug output for now (will be replaced with proper logging later)
        Debug.WriteLine($"Logging session: App: {_currentTrackedAppName}, Path: {_currentTrackedExecutablePath}, Duration: {duration.TotalSeconds:F0}s, Start: {_currentSessionStartTimeUtc}, End: {endTimeUtc}");

        if (duration.TotalSeconds < 1)
        {
            Debug.WriteLine($"Skipping log for {_currentTrackedAppName} due to short duration: {duration.TotalSeconds:F1}s");
            _isCurrentlyTrackingActiveSession = false; // Still reset session state
            return;
        }

        var entry = new AppUsageEntry
        {
            ApplicationName = _currentTrackedAppName,
            ExecutablePath = _currentTrackedExecutablePath,
            StartTimeUtc = _currentSessionStartTimeUtc,
            EndTimeUtc = endTimeUtc,
            Duration = duration
        };

        await _dataLogger.LogSessionAsync(entry);
        _isCurrentlyTrackingActiveSession = false;

    }

    public bool IsUserActive()
    {
        return _userActivityMonitor.IsUserConsideredActive(_idleThreshold);
    }

    public async Task FinalizeAndLogCurrentSessionAsync()
    {
        if (_isCurrentlyTrackingActiveSession && !string.IsNullOrEmpty(_currentTrackedAppName))
        {
            Debug.WriteLine($"Finalizing session on exit for: {_currentTrackedAppName}");
            await LogEndOfSessionAsync();
        }
    }
}