using System;
using System.IO;
using System.Text.Json;


namespace ScreentimeTracker.Utils;

public class AppSettings
{
    public int IdleThreshold { get; set; } = 2;
    public bool IsBreakReminderEnabled { get; set; } = true;
    public int BreakReminderIntervalMinutes { get; set; } = 45;
    private static string GetSettingsFilePath()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var appDataPath = Environment.GetFolderPath(folder);
        var appSpecificDir = Path.Combine(appDataPath, "ScreenTimeTrackerApp"); 
        return Path.Combine(appSpecificDir, "settings.json");
    }

    public void Save()
    {
        try
        {
            string filePath = GetSettingsFilePath();
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath != null)
            {
                Directory.CreateDirectory(directoryPath);
            }
            string jsonString = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonString);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving settings: {ex.Message}");
        }
    }

    public static AppSettings Load()
    {
        try
        {
            string filePath = GetSettingsFilePath();
            if (File.Exists(filePath))
            {
                string jsonString = File.ReadAllText(filePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(jsonString);
                return settings ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings: {ex.Message}");
        }
        return new AppSettings();
    }

}
