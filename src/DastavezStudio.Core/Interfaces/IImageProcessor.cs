using DastavezStudio.Core.Models;

namespace DastavezStudio.Core.Interfaces;

public class ImageResizeOptions
{
    public int? WidthPx { get; set; }
    public int? HeightPx { get; set; }
    public bool MaintainAspectRatio { get; set; } = true;
    public int? TargetDpi { get; set; }
    public bool StripMetadata { get; set; } = true;
}

public class ImageCompressionOptions
{
    public int Quality { get; set; } = 85; // 1 to 100
    public int? TargetMaxKb { get; set; }
    public int? TargetMinKb { get; set; }
    public ImageFormatType OutputFormat { get; set; } = ImageFormatType.Jpeg;
    public bool StripMetadata { get; set; } = true;
}

public class ImageDimensions
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int DpiX { get; set; }
    public int DpiY { get; set; }
    public string Format { get; set; } = string.Empty;
}

public interface IImageProcessor
{
    Task<ImageDimensions> GetImageDimensionsAsync(string inputFilePath);
    Task<string> ResizeImageAsync(string inputFilePath, string outputFilePath, ImageResizeOptions options, CancellationToken ct = default);
    Task<string> CompressImageAsync(string inputFilePath, string outputFilePath, ImageCompressionOptions options, CancellationToken ct = default);
    Task<string> ProcessForExamPresetAsync(string inputFilePath, string outputFilePath, ExamPreset preset, CancellationToken ct = default);
    Task<string> CropImageAsync(string inputFilePath, string outputFilePath, int x, int y, int width, int height, CancellationToken ct = default);
    Task<string> RotateFlipAsync(string inputFilePath, string outputFilePath, int rotateDegrees, bool flipHorizontal, bool flipVertical, CancellationToken ct = default);
    Task<string> AdjustColorsAsync(string inputFilePath, string outputFilePath, float brightness, float contrast, bool grayscale, CancellationToken ct = default);
}
