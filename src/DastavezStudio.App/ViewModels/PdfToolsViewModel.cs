using CommunityToolkit.Mvvm.ComponentModel;

namespace DastavezStudio.App.ViewModels;

public partial class PdfToolsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string ActiveTool { get; set; } = "Compress";

    [ObservableProperty]
    public partial string? InputFilePath { get; set; }

    [ObservableProperty]
    public partial int TargetSizeKb { get; set; } = 300;

    [ObservableProperty]
    public partial string CompressionQuality { get; set; } = "Balanced";

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Ready to process PDF documents.";

    [ObservableProperty]
    public partial bool IsProcessing { get; set; }
}
