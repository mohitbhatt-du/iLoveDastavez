using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;

namespace DastavezStudio.App.ViewModels;

public partial class AiEnhanceViewModel : ObservableObject
{
    private readonly IAiEnhancer _aiEnhancer;
    private readonly ITempFileManager _tempFileManager;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasInputFile))]
    [NotifyPropertyChangedFor(nameof(InputFileName))]
    public partial string? InputFilePath { get; set; }

    public string InputFileName => string.IsNullOrEmpty(InputFilePath) ? string.Empty : Path.GetFileName(InputFilePath);
    public bool HasInputFile => !string.IsNullOrEmpty(InputFilePath) && File.Exists(InputFilePath);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEnhancedFile))]
    public partial string? EnhancedFilePath { get; set; }

    public bool HasEnhancedFile => !string.IsNullOrEmpty(EnhancedFilePath) && File.Exists(EnhancedFilePath);

    [ObservableProperty]
    public partial int SelectedUpscaleIndex { get; set; } = 0; // 0: 2x, 1: 4x

    public int UpscaleFactor => SelectedUpscaleIndex == 1 ? 4 : 2;

    [ObservableProperty]
    public partial bool Denoise { get; set; } = true;

    [ObservableProperty]
    public partial bool Sharpen { get; set; } = true;

    [ObservableProperty]
    public partial bool EnhanceContrast { get; set; } = true;

    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Ready for offline AI enhancement.";

    [ObservableProperty]
    public partial string ResultDetails { get; set; } = string.Empty;

    public bool IsOnnxAvailable => _aiEnhancer.IsOnnxModelAvailable;
    public IReadOnlyList<string> HardwareProviders => _aiEnhancer.AvailableHardwareProviders;

    [ObservableProperty]
    public partial string SelectedHardwareProvider { get; set; }

    public AiEnhanceViewModel(IAiEnhancer aiEnhancer, ITempFileManager tempFileManager)
    {
        _aiEnhancer = aiEnhancer ?? throw new ArgumentNullException(nameof(aiEnhancer));
        _tempFileManager = tempFileManager ?? throw new ArgumentNullException(nameof(tempFileManager));
        SelectedHardwareProvider = HardwareProviders.FirstOrDefault() ?? "CPU (Local Hardware)";
    }

    public void SetInputFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        InputFilePath = filePath;
        EnhancedFilePath = null;
        ResultDetails = string.Empty;
        Progress = 0;
        StatusMessage = $"Loaded: {Path.GetFileName(filePath)}. Ready to enhance.";
    }

    [RelayCommand]
    public async Task RunEnhanceAsync()
    {
        if (string.IsNullOrEmpty(InputFilePath) || !File.Exists(InputFilePath) || IsRunning)
            return;

        IsRunning = true;
        Progress = 0;
        StatusMessage = "Running offline enhancement & super-resolution...";
        _cts = new CancellationTokenSource();

        var tempOutput = _tempFileManager.GetTempFilePath(Path.GetExtension(InputFilePath));

        var options = new AiEnhanceOptions
        {
            UpscaleFactor = UpscaleFactor,
            Denoise = Denoise,
            Sharpen = Sharpen,
            EnhanceContrast = EnhanceContrast,
            UseGpuAcceleration = SelectedHardwareProvider.Contains("GPU", StringComparison.OrdinalIgnoreCase) ||
                                 SelectedHardwareProvider.Contains("DirectML", StringComparison.OrdinalIgnoreCase)
        };

        var progressReporter = new Progress<double>(val =>
        {
            Progress = val;
            StatusMessage = val switch
            {
                < 30 => "Loading image buffer...",
                < 60 => $"Applying {UpscaleFactor}× super-resolution upscaling...",
                < 80 => "Suppressing noise & artifacts...",
                < 95 => "Sharpening edges and details...",
                _ => "Finalizing enhanced image..."
            };
        });

        try
        {
            var result = await _aiEnhancer.EnhanceAsync(InputFilePath, tempOutput, options, progressReporter, _cts.Token);
            EnhancedFilePath = result.OutputPath;
            ResultDetails = $"Original: {result.OriginalWidth}×{result.OriginalHeight} px  ➔  Enhanced: {result.EnhancedWidth}×{result.EnhancedHeight} px ({result.MethodUsed}) in {result.Duration.TotalSeconds:F2}s";
            StatusMessage = "✓ Enhancement complete! Image is ready for export.";
            Progress = 100;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Enhancement cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Enhancement error: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand]
    public void CancelEnhance()
    {
        if (IsRunning && _cts != null && !_cts.IsCancellationRequested)
        {
            StatusMessage = "Cancelling enhancement...";
            _cts.Cancel();
        }
    }
}
