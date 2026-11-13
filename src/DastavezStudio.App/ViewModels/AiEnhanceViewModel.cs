using CommunityToolkit.Mvvm.ComponentModel;

namespace DastavezStudio.App.ViewModels;

public partial class AiEnhanceViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string? InputFilePath { get; set; }

    [ObservableProperty]
    public partial int UpscaleFactor { get; set; } = 2; // 2x or 4x

    [ObservableProperty]
    public partial bool Denoise { get; set; } = true;

    [ObservableProperty]
    public partial bool Sharpen { get; set; } = true;

    [ObservableProperty]
    public partial bool IsProcessing { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Offline AI enhancement ready (ONNX local inference).";
}
