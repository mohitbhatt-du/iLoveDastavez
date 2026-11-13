using CommunityToolkit.Mvvm.ComponentModel;

namespace DastavezStudio.App.ViewModels;

public partial class ImageToolsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string? InputFilePath { get; set; }

    [ObservableProperty]
    public partial int TargetWidth { get; set; } = 800;

    [ObservableProperty]
    public partial int TargetHeight { get; set; } = 600;

    [ObservableProperty]
    public partial bool MaintainAspectRatio { get; set; } = true;

    [ObservableProperty]
    public partial int TargetSizeKb { get; set; } = 50;

    [ObservableProperty]
    public partial int Quality { get; set; } = 85;

    [ObservableProperty]
    public partial string SelectedFormat { get; set; } = "JPEG";

    [ObservableProperty]
    public partial bool StripExif { get; set; } = true;

    [ObservableProperty]
    public partial int TargetDpi { get; set; } = 200;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Drop an image or select a file to begin.";

    [ObservableProperty]
    public partial bool IsProcessing { get; set; }
}
