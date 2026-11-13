using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;

namespace DastavezStudio.App.ViewModels;

public partial class ImageToolsViewModel : ObservableObject
{
    private readonly IImageProcessor _imageProcessor;
    private readonly ITempFileManager _tempFileManager;

    [ObservableProperty]
    public partial string? InputFilePath { get; set; }

    [ObservableProperty]
    public partial string? InputFileName { get; set; }

    [ObservableProperty]
    public partial string? ProcessedFilePath { get; set; }

    [ObservableProperty]
    public partial int TargetWidth { get; set; } = 800;

    [ObservableProperty]
    public partial int TargetHeight { get; set; } = 600;

    [ObservableProperty]
    public partial int OriginalWidth { get; set; }

    [ObservableProperty]
    public partial int OriginalHeight { get; set; }

    [ObservableProperty]
    public partial bool MaintainAspectRatio { get; set; } = true;

    [ObservableProperty]
    public partial int TargetSizeKb { get; set; } = 50;

    [ObservableProperty]
    public partial int Quality { get; set; } = 85;

    [ObservableProperty]
    public partial bool EnableTargetSizeCompression { get; set; } = true;

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

    public ImageToolsViewModel(IImageProcessor imageProcessor, ITempFileManager tempFileManager)
    {
        _imageProcessor = imageProcessor;
        _tempFileManager = tempFileManager;
    }

    public async Task SetInputFileAsync(string path)
    {
        InputFilePath = path;
        InputFileName = Path.GetFileName(path);
        ProcessedFilePath = null;

        try
        {
            var dims = await _imageProcessor.GetImageDimensionsAsync(path);
            OriginalWidth = dims.Width;
            OriginalHeight = dims.Height;
            TargetWidth = dims.Width;
            TargetHeight = dims.Height;
            TargetDpi = dims.DpiX;

            var sizeKb = new FileInfo(path).Length / 1024.0;
            StatusMessage = $"Loaded: {InputFileName} ({dims.Width}x{dims.Height} px, {sizeKb:F1} KB, {dims.Format})";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Notice: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task ProcessImageAsync()
    {
        if (string.IsNullOrEmpty(InputFilePath) || !File.Exists(InputFilePath))
        {
            StatusMessage = "Please select an image file first.";
            return;
        }

        IsProcessing = true;
        StatusMessage = "Processing image (resizing & compressing)...";

        try
        {
            var ext = SelectedFormat.ToLowerInvariant() switch
            {
                "png" => ".png",
                "webp" => ".webp",
                _ => ".jpg"
            };

            var tempResized = _tempFileManager.GetTempFilePath(ext);
            await _imageProcessor.ResizeImageAsync(InputFilePath, tempResized, new ImageResizeOptions
            {
                WidthPx = TargetWidth,
                HeightPx = TargetHeight,
                MaintainAspectRatio = MaintainAspectRatio,
                TargetDpi = TargetDpi,
                StripMetadata = StripExif
            });

            var tempCompressed = _tempFileManager.GetTempFilePath(ext);
            await _imageProcessor.CompressImageAsync(tempResized, tempCompressed, new ImageCompressionOptions
            {
                Quality = Quality,
                TargetMaxKb = EnableTargetSizeCompression ? TargetSizeKb : null,
                OutputFormat = SelectedFormat.ToLowerInvariant() switch
                {
                    "png" => ImageFormatType.Png,
                    "webp" => ImageFormatType.Webp,
                    _ => ImageFormatType.Jpeg
                },
                StripMetadata = StripExif
            });

            ProcessedFilePath = tempCompressed;
            var outDims = await _imageProcessor.GetImageDimensionsAsync(tempCompressed);
            var outSizeKb = new FileInfo(tempCompressed).Length / 1024.0;

            StatusMessage = $"✓ Processed successfully! Output: {outDims.Width}x{outDims.Height} px, {outSizeKb:F1} KB.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
