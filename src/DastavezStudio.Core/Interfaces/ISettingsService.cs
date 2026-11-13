using System.Threading.Tasks;

namespace DastavezStudio.Core.Interfaces;

public class AppPreferences
{
    public int ThemeIndex { get; set; } = 0; // 0 = System, 1 = Light, 2 = Dark
    public bool AutoCleanTempOnExit { get; set; } = true;
    public string DefaultExportDirectory { get; set; } = string.Empty;
    public int DefaultDpi { get; set; } = 200;
    public int DefaultJpegQuality { get; set; } = 85;
}

public interface ISettingsService
{
    AppPreferences Preferences { get; }
    Task SaveAsync();
    Task LoadAsync();
}
