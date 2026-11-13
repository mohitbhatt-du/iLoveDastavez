using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DastavezStudio.App.Helpers;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;
using Microsoft.UI.Xaml.Media.Imaging;

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
    [NotifyPropertyChangedFor(nameof(HasSourceImage))]
    public partial BitmapImage? SourceImage { get; set; }

    public bool HasSourceImage => SourceImage != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasProcessedImage))]
    public partial BitmapImage? ProcessedImage { get; set; }

    public bool HasProcessedImage => ProcessedImage != null;

    [ObservableProperty]
    public partial string OriginalDimensions { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string OriginalFileSize { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProcessedDimensions { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProcessedFileSize { get; set; } = string.Empty;

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
        if (!File.Exists(path)) return;

        InputFilePath = path;
        InputFileName = Path.GetFileName(path);
        ProcessedFilePath = null;
        ProcessedImage = null;
        ProcessedDimensions = string.Empty;
        ProcessedFileSize = string.Empty;

        // Load source image in-memory
        SourceImage = await ImagePreviewHelper.LoadBitmapFromFileAsync(path);

        try
        {
            var dims = await _imageProcessor.GetImageDimensionsAsync(path);
            OriginalWidth = dims.Width;
            OriginalHeight = dims.Height;
            TargetWidth = dims.Width;
            TargetHeight = dims.Height;
            TargetDpi = dims.DpiX;

            OriginalDimensions = $"{dims.Width} × {dims.Height} px";
            var sizeKb = new FileInfo(path).Length / 1024.0;
            OriginalFileSize = sizeKb >= 1024 ? $"{sizeKb / 1024.0:F2} MB" : $"{sizeKb:F1} KB";

            StatusMessage = $"Loaded: {InputFileName} ({dims.Width}x{dims.Height} px, {OriginalFileSize}, {dims.Format})";

            // Automatically generate processed preview
            await ProcessImageAsync();
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

            // Load processed image preview in-memory
            ProcessedImage = await ImagePreviewHelper.LoadBitmapFromFileAsync(tempCompressed);

            var outDims = await _imageProcessor.GetImageDimensionsAsync(tempCompressed);
            ProcessedDimensions = $"{outDims.Width} × {outDims.Height} px";
            var outSizeKb = new FileInfo(tempCompressed).Length / 1024.0;
            ProcessedFileSize = $"{outSizeKb:F1} KB";

            StatusMessage = $"✓ Processed successfully! Output: {outDims.Width}x{outDims.Height} px, {ProcessedFileSize}.";
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
