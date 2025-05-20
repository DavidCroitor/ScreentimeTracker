using System;
using System.Windows.Forms;
using ScreentimeTracker.Interop;
using System.Runtime.InteropServices;
using System.Reflection;
using ScreentimeTracker.Core;
using System.Diagnostics;
using System.Data;
using ScreentimeTracker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.ApplicationServices;
using System.Threading.Tasks;
using ScreentimeTracker.UI;
using ScreentimeTracker.Utils;

namespace ScreentimeTracker.Core;

public class MyApplicationContext : ApplicationContext
{
    private NotifyIcon? _trayIcon;
    private System.Windows.Forms.Timer? _trackingTimer;
    private ActivityTracker _activityTracker;
    private IDataLogger _dataLogger;
    private UserActivityMonitor _userActivityMonitor;
    private AppSettings _appSettings;


    private const int TIMER_INTERVAL_MS = 2000;
    private const string ICON_RESOURCE_NAME = "ScreenTimeTracker.appicon.ico"; // Adjust the resource name as needed

    public MyApplicationContext()
    {
        _appSettings = AppSettings.Load();

        var folder = Environment.SpecialFolder.LocalApplicationData;
        var appDataPath = Environment.GetFolderPath(folder);
        var appSpecificDirectory = Path.Combine(appDataPath, "ScreenTimeTrackerApp");
        if (!Directory.Exists(appSpecificDirectory))
        {
            Directory.CreateDirectory(appSpecificDirectory);
        }
        var databasePath = Path.Combine(appSpecificDirectory, "screentime_efcore.sqlite");

        var dbContextOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        dbContextOptionsBuilder.UseSqlite($"Data Source={databasePath}");

        _dataLogger = new SQLiteDataLogger(dbContextOptionsBuilder.Options);

        _userActivityMonitor = new UserActivityMonitor();
        TimeSpan idleThreshold = TimeSpan.FromMinutes(_appSettings.IdleThreshold);

        _activityTracker = new ActivityTracker(
            _dataLogger,
            _userActivityMonitor,
            idleThreshold);

        InitializeTrayIcon();
        InitializeTimer();

        _trackingTimer?.Start();

        Debug.WriteLine("ScreenTimeTracker started.");
    }

    private void InitializeTrayIcon()
    {
        _trayIcon = new NotifyIcon();

        try
        {
            // Get the current assembly
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            using (Stream? iconStream = currentAssembly.GetManifestResourceStream(ICON_RESOURCE_NAME))
            {
                if (iconStream != null)
                {
                    _trayIcon.Icon = new Icon(iconStream);
                }
                else
                {
                    Debug.WriteLine($"Error: Resource '{ICON_RESOURCE_NAME}' not found. Using default icon.");
                    _trayIcon.Icon = SystemIcons.Application;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading icon from embedded resource: {ex.Message}. Using default icon.");
            _trayIcon.Icon = SystemIcons.Application;
        }

        _trayIcon.Text = "Screen Time Tracker";

        // 4. Create and Assign the Context Menu
        ContextMenuStrip contextMenu = new ContextMenuStrip();

        ToolStripMenuItem showStatsItem = new ToolStripMenuItem("Show Stats");
        showStatsItem.Click += OnShowStats;
        contextMenu.Items.Add(showStatsItem);

        ToolStripMenuItem showSettingsItem = new ToolStripMenuItem("Settings");
        showSettingsItem.Click += OnSettingsClick;
        contextMenu.Items.Add(showSettingsItem);

        ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += OnExit;
        contextMenu.Items.Add(exitItem);

        _trayIcon.ContextMenuStrip = contextMenu;

        _trayIcon.Visible = true;
    }

    private void OnSettingsClick(object? sender, EventArgs e)
    {
        _activityTracker.FinalizeAndLogCurrentSessionAsync().Wait();
        using (var settingsForm = new SettingsForm(_appSettings))
        {
            settingsForm.ShowDialog();
        }
    }

    private void InitializeTimer()
    {
        _trackingTimer = new System.Windows.Forms.Timer();
        _trackingTimer.Interval = TIMER_INTERVAL_MS;
        _trackingTimer.Tick += TrackingTimer_Tick;
    }

    private async void TrackingTimer_Tick(Object? sender, EventArgs e)
    {
        await _activityTracker.ProcessTickAsync();   
    }

    private void OnShowStats(object? sender, EventArgs e)
    {
        _activityTracker.FinalizeAndLogCurrentSessionAsync().Wait();
        using (var statsForm = new StatsForm(_dataLogger))
        {
            statsForm.ShowDialog();
        }
    }

    private void OnExit(Object? sender, EventArgs e)
    {
        _trackingTimer?.Stop();

        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
        Application.Exit();
    }
}

