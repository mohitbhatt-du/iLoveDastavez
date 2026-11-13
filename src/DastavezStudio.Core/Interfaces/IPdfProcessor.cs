using DastavezStudio.Core.Models;

namespace DastavezStudio.Core.Interfaces;

public enum PdfCompressionLevel
{
    Low,
    Balanced,
    High,
    TargetSize
}

public class PdfCompressionOptions
{
    public PdfCompressionLevel Level { get; set; } = PdfCompressionLevel.Balanced;
    public int? TargetSizeKb { get; set; }
    public int ImageDpi { get; set; } = 150;
    public int JpegQuality { get; set; } = 75;
}

public class PdfPageInfo
{
    public int PageNumber { get; set; }
    public double WidthPt { get; set; }
    public double HeightPt { get; set; }
    public int RotationDegrees { get; set; }
}

public interface IPdfProcessor
{
    Task<IReadOnlyList<PdfPageInfo>> GetPdfInfoAsync(string pdfPath, string? password = null);
    Task<string> CompressPdfAsync(string inputPdfPath, string outputPdfPath, PdfCompressionOptions options, IProgress<double>? progress = null, CancellationToken ct = default);
    Task<string> MergePdfsAsync(IReadOnlyList<string> inputPdfPaths, string outputPdfPath, CancellationToken ct = default);
    Task<IReadOnlyList<string>> SplitPdfAsync(string inputPdfPath, string outputFolder, int? splitEveryNPages = null, CancellationToken ct = default);
    Task<string> ExtractPagesAsync(string inputPdfPath, string outputPdfPath, IReadOnlyList<int> pageNumbers, CancellationToken ct = default);
    Task<string> RotatePagesAsync(string inputPdfPath, string outputPdfPath, IReadOnlyList<int> pageNumbers, int rotationDegrees, CancellationToken ct = default);
    Task<string> ConvertImagesToPdfAsync(IReadOnlyList<string> imagePaths, string outputPdfPath, CancellationToken ct = default);
    Task<IReadOnlyList<string>> ConvertPdfToImagesAsync(string inputPdfPath, string outputFolder, ImageFormatType format, int dpi = 200, CancellationToken ct = default);
    Task<string> SetPasswordAsync(string inputPdfPath, string outputPdfPath, string userPassword, string ownerPassword, CancellationToken ct = default);
    Task<string> RemovePasswordAsync(string inputPdfPath, string outputPdfPath, string password, CancellationToken ct = default);
}
