using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DastavezStudio.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string SelectedTheme { get; set; } = "System Default";

    [ObservableProperty]
    public partial string DefaultExportDirectory { get; set; } = 
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

    [ObservableProperty]
    public partial int DefaultDpi { get; set; } = 200;

    [ObservableProperty]
    public partial int DefaultJpegQuality { get; set; } = 85;

    [ObservableProperty]
    public partial bool AutoCleanTempOnExit { get; set; } = true;

    [ObservableProperty]
    public partial string CleanupStatus { get; set; } = "Temporary session files are clean.";

    [RelayCommand]
    public void CleanTemporaryFiles()
    {
        try
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "DastavezStudio");
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
                Directory.CreateDirectory(tempDir);
            }
            CleanupStatus = "✓ All temporary workspace cache files have been purged.";
        }
        catch (Exception ex)
        {
            CleanupStatus = $"Notice: {ex.Message}";
        }
    }
}
