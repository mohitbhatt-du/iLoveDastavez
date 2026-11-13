using CommunityToolkit.Mvvm.ComponentModel;

namespace DastavezStudio.App.ViewModels;

public partial class ScannerOcrViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string? InputFilePath { get; set; }

    [ObservableProperty]
    public partial bool AutoDeskew { get; set; } = true;

    [ObservableProperty]
    public partial bool ShadowRemoval { get; set; } = true;

    [ObservableProperty]
    public partial bool DocumentCleanup { get; set; } = true;

    [ObservableProperty]
    public partial string ExtractedText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsProcessing { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Ready for document cleanup and offline OCR.";
}
