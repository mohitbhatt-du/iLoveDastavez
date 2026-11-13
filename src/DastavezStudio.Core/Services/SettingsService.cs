using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using DastavezStudio.Core.Interfaces;

namespace DastavezStudio.Core.Services;

public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    public AppPreferences Preferences { get; private set; } = new();

    public SettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dir = Path.Combine(appData, "DastavezStudio");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        _settingsFilePath = Path.Combine(dir, "settings.json");

        Preferences.DefaultExportDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = await File.ReadAllTextAsync(_settingsFilePath);
                var loaded = JsonSerializer.Deserialize<AppPreferences>(json);
                if (loaded != null)
                {
                    Preferences = loaded;
                }
            }
        }
        catch
        {
            // Fallback to default preferences
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(Preferences, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
        catch
        {
            // Suppress error
        }
    }
}
