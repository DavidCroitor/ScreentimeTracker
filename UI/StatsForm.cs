using ScreentimeTracker.Data;
using ScreentimeTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ScreentimeTracker.UI;

public partial class StatsForm : Form
{
    private readonly IDataLogger _dataLogger;
    private DateTime _currentWeekStartDate;
    private DateTime _selectedDate;
    private Dictionary<string, int> _iconCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    public StatsForm(IDataLogger dataLogger)
    {
        _dataLogger = dataLogger;
        _selectedDate = DateTime.Today;
        _currentWeekStartDate = GetStartOfWeek(_selectedDate);

        InitializeComponent();

        // Configure initial UI state
        _nextWeekButton.Enabled = !IsCurrentWeek();
        UpdateWeekRangeLabel();
        // Wire up event handlers
        this.Load += StatsForm_Load;
        _prevWeekButton.Click += (s, e) => NavigateWeek(-1);
        _nextWeekButton.Click += (s, e) => NavigateWeek(1);
        _weeklyChart.MouseClick += WeeklyChart_MouseClick;

        // Set up ListView customization
        _appListView.OwnerDraw = true;
        _appListView.DrawColumnHeader += (s, e) => e.DrawDefault = true;
        _appListView.DrawItem += (s, e) => e.DrawDefault = true;
        _appListView.DrawSubItem += AppListView_DrawSubItem;
    }

    private Icon? GetIconForFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return null;
        }
        try
        {
            Icon? appIcon = Icon.ExtractAssociatedIcon(filePath);
            return appIcon;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error extracting icon for {filePath}: {ex.Message}");
            return null;
        }
    }
    private int GetOrAddIconIndex(string executablePath)
    {
        if (string.IsNullOrEmpty(executablePath))
        {
            return -1;
        }

        if (_iconCache.TryGetValue(executablePath, out int existingIndex))
        {
            return existingIndex;
        }

        Icon? icon = GetIconForFile(executablePath);
        if (icon != null)
        {
            _appIconList.Images.Add(icon);
            int newIndex = _appIconList.Images.Count - 1;
            _iconCache[executablePath] = newIndex;
            icon.Dispose(); // ImageList makes its own copy
            return newIndex;
        }
        return -1; // No icon found or error
    }
    private async void WeeklyChart_MouseClick(object? sender, MouseEventArgs e)
    {
        HitTestResult result = _weeklyChart.HitTest(e.X, e.Y);
        if (result.ChartElementType == ChartElementType.DataPoint)
        {
            DataPoint point = _weeklyChart.Series[0].Points[result.PointIndex];
            if (point.Tag is DateTime clickedDate)
            {
                // Update UI for the selected day
                _selectedDate = clickedDate;

                // Reset colors and highlight selected day
                for (int i = 0; i < _weeklyChart.Series[0].Points.Count; i++)
                {
                    _weeklyChart.Series[0].Points[i].Color = i == result.PointIndex ?
                        Color.DarkOrange : Color.SteelBlue;
                }

                await LoadDayStats(_selectedDate);
            }
        }
    }

    private async void StatsForm_Load(object? sender, EventArgs e)
    {
        this.Load -= StatsForm_Load; // Unsubscribe to prevent multiple calls if shown/hidden

        try
        {
            await LoadWeekStats(); // Await the actual async method
            await LoadDayStats(_selectedDate); // Await the actual async method
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading initial statistics: {ex.Message}", "Initialization Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            Debug.WriteLine($"Exception in StatsForm_Load: {ex}");
        }
    }
    
    private DateTime GetStartOfWeek(
        DateTime date,
        DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-1 * diff).Date;
    }
    
    private bool IsCurrentWeek()
    {
        DateTime currentWeekStart = GetStartOfWeek(DateTime.Today);
        return _currentWeekStartDate >= currentWeekStart;
    }

    public static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalMinutes < 1)
        {
            return "<1 min";
        }
        if (duration.TotalHours < 1)
        {
            return $"{duration.Minutes}m";
        }
        return $"{(int)duration.TotalHours}h {duration.Minutes % 60}m";
    }
    
    private void UpdateWeekRangeLabel()
    {
        DateTime weekEnd = _currentWeekStartDate.AddDays(6);
        _weekRangeLabel.Text = $"{_currentWeekStartDate:MMM d} - {weekEnd:MMM d, yyyy}";
        _nextWeekButton.Enabled = !IsCurrentWeek();
    }
    
    private async void NavigateWeek(int direction)
    {
        _currentWeekStartDate = _currentWeekStartDate.AddDays(7 * direction);
        UpdateWeekRangeLabel();
        await LoadWeekStats();
        
        // Update the selected date to be within the new week
        if (_selectedDate < _currentWeekStartDate || _selectedDate >= _currentWeekStartDate.AddDays(7))
        {
            _selectedDate = _currentWeekStartDate;
            await LoadDayStats(_selectedDate);
        }
    }

    private async Task LoadWeekStats()
    {
        try
        {
            var currentWeekEndDate = _currentWeekStartDate.AddDays(7);

            // Clear existing data
            _weeklyChart.Series[0].Points.Clear();
            _weeklyChart.Series[0].IsValueShownAsLabel = true;
            _weeklyChart.Series[0].LabelFormat = "";
            _weeklyChart.Series[0].Font = new Font("Segoe UI", 7.0f); // Smaller font for labels
            _weeklyChart.Series[0].LabelForeColor = Color.Black; // Or SystemColors.ControlText

            _weeklyChart.Series[0].SmartLabelStyle.Enabled = true; // Disable 
            _weeklyChart.Series[0].SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.No; // Keep it inside
            _weeklyChart.Series[0].SmartLabelStyle.IsOverlappedHidden = true; // Important: hides labels that would overlap severely
            _weeklyChart.Series[0].SmartLabelStyle.MovingDirection = LabelAlignmentStyles.Top;
            _weeklyChart.Series[0].SmartLabelStyle.MinMovingDistance = 0; // Allow small adjustments
            _weeklyChart.Series[0].SmartLabelStyle.MaxMovingDistance = 50; // Limit how far it tries to move
            
            // Calculate daily totals for the week
            List<DailyTotalStat> dailyStats = new List<DailyTotalStat>();

            for (int i = 0; i < 7; i++)
            {
                DateTime localDay = _currentWeekStartDate.AddDays(i);
                DateTime dayStartUtc = localDay.Date.ToUniversalTime();
                DateTime dayEndUtc = localDay.AddDays(1);

                var appStats = await _dataLogger.GetUsageStatsAsync(dayStartUtc, dayEndUtc);
                TimeSpan totalTime = TimeSpan.FromSeconds(appStats.Sum(a => a.TotalDurationSeconds));


                dailyStats.Add(new DailyTotalStat
                {
                    Day = localDay.DayOfWeek,
                    Date = localDay,
                    TotalScreenTime = totalTime
                });

                // Add to chart
                DataPoint point = new DataPoint();
                point.SetValueXY(localDay.DayOfWeek.ToString().Substring(0, 3), totalTime.TotalHours);
                point.ToolTip = $"{localDay:ddd, MMM d}: {FormatDuration(totalTime)}";
                point.Tag = localDay;

                if (totalTime.TotalSeconds > 0) 
                {
                    if (totalTime.TotalMinutes < 1)
                    {
                        point.Label = $"{(int)totalTime.TotalSeconds}s";
                    }
                    else
                    {
                        point.Label = FormatDuration(totalTime);
                    }
                }
                else
                {
                    point.Label = "0m";
                }

                point.Color = localDay.Date == _selectedDate.Date ? Color.DarkOrange : Color.SteelBlue;

                _weeklyChart.Series[0].Points.Add(point);

                // Make chart bars clickable
                _weeklyChart.Series[0].Points[i].Tag = localDay;
            }

            double maxHours = 0.5;
            if (_weeklyChart.Series[0].Points.Any())
            {
                maxHours = _weeklyChart.Series[0].Points.Cast<DataPoint>().Max(dp => dp.YValues[0]);
            }

            double labelHeadroom = Math.Max(0.1, maxHours * 0.20);
            _weeklyChart.ChartAreas[0].AxisY.Maximum = maxHours + labelHeadroom;
            _weeklyChart.ChartAreas[0].AxisY.Minimum = 0;


            _weeklyChart.Invalidate();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading weekly statistics: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async Task LoadDayStats(DateTime date)
    {
        try
        {
            _selectedDayLabel.Text = $"App Usage for {date:dddd, MMMM d, yyyy}";
            
            DateTime dayEnd = date.AddDays(1);
            var appStats = await _dataLogger.GetUsageStatsAsync(date, dayEnd);
            
            Debug.WriteLine($"Retrieved {appStats.Count()} apps for {date:yyyy-MM-dd}");
            
            _appListView.Visible = true;

            _appListView.Items.Clear();
            _appListView.BeginUpdate();
            
            // Calculate total screen time for the day
            long totalSeconds = appStats.Sum(a => a.TotalDurationSeconds);
            TimeSpan totalTime = TimeSpan.FromSeconds(totalSeconds);
            
            Debug.WriteLine($"Total time: {totalTime}");
            
            // Add a total row at the top
            ListViewItem totalItem = new ListViewItem("App");
            totalItem.SubItems.Add(FormatDuration(totalTime));
            totalItem.SubItems.Add("100%");
            
            totalItem.Font = new Font(_appListView.Font, FontStyle.Bold);
            _appListView.Items.Add(totalItem);
            
            int appCount = 0;
            // Add all apps with their usage
            foreach (var app in appStats.OrderByDescending(a => a.TotalDurationSeconds))
            {
                
                double percentage = totalSeconds > 0 ? (double)app.TotalDurationSeconds / totalSeconds * 100 : 0;
                
                ListViewItem item = new ListViewItem(app.ApplicationName);

                if (!string.IsNullOrEmpty(app.ExecutablePath))
                {
                    item.ImageIndex = GetOrAddIconIndex(app.ExecutablePath);
                }
                else
                {
                    item.ImageIndex = -1;
                }

                item.SubItems.Add(FormatDuration(app.TotalDuration));
                item.SubItems.Add($"{percentage:0.0}%");
                
                item.Tag = percentage; 
                
                _appListView.Items.Add(item);
                appCount++;
                
                Debug.WriteLine($"Added app: {app.ApplicationName}, Time: {FormatDuration(app.TotalDuration)}");
            }
            
            Debug.WriteLine($"Added {appCount} apps to the list view");
            
            _appListView.EndUpdate();
            _appListView.Refresh();
            
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading day statistics: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            Debug.WriteLine($"Exception in LoadDayStats: {ex}");
        }
    }

    private void AppListView_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
    {
        if (e.Item == null)
        {
            return;
        }

        e.DrawBackground();

        string textToDraw;
        Font textFont = e.Item.Font;

        if (e.ItemIndex == 0 && e.Item.Text == "Total Screen Time")
        {
            textFont = new Font(e.Item.Font, FontStyle.Bold); 
            textToDraw = e.SubItem?.Text ?? string.Empty;
            if (e.ColumnIndex == 0) textToDraw = e.Item.Text;
        }
        else
        {
            textToDraw = e.SubItem?.Text ?? string.Empty;
            if (e.ColumnIndex == 0) textToDraw = e.Item.Text;
        }

        TextFormatFlags flags = TextFormatFlags.HorizontalCenter |
                                TextFormatFlags.VerticalCenter |
                                TextFormatFlags.SingleLine | 
                                TextFormatFlags.WordEllipsis;


        Color textColor = e.Item.ForeColor; 

        if (e.Item.Selected)
        {
            textColor = _appListView.Focused ? SystemColors.HighlightText : e.Item.ForeColor;
        }


        Rectangle textBounds = e.Bounds;

        TextRenderer.DrawText(e.Graphics, textToDraw, textFont, textBounds, textColor, flags);


        if (e.Item.Focused && e.Item.Selected && e.ColumnIndex == 0)
        {
            ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds, textColor, e.Item.BackColor);
        }
    }
}