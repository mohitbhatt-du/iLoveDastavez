using SkiaSharp;
using PdfSharpCore.Pdf.IO;
using DastavezStudio.AiEngine;
using DastavezStudio.Core.Models;
using DastavezStudio.OcrEngine;
using Xunit;

namespace DastavezStudio.UnitTests;

public class AiEnhanceAndOcrTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    private string CreateTestBitmap(int width, int height, SKColor bg, Action<SKCanvas>? drawExtra = null)
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_sample_{Guid.NewGuid():N}.png");
        using var bmp = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(bg);

        drawExtra?.Invoke(canvas);

        using var img = SKImage.FromBitmap(bmp);
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        using var s = File.Create(path);
        data.SaveTo(s);

        _tempFiles.Add(path);
        return path;
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            try { if (File.Exists(file)) File.Delete(file); } catch { }
        }
    }

    [Fact]
    public async Task AiEnhance_2xUpscaling_DoublesDimensions()
    {
        var enhancer = new AiEnhancer();
        var inputPath = CreateTestBitmap(120, 160, SKColors.LightSkyBlue);
        var outputPath = Path.Combine(Path.GetTempPath(), $"enhanced_2x_{Guid.NewGuid():N}.jpg");
        _tempFiles.Add(outputPath);

        var options = new AiEnhanceOptions
        {
            UpscaleFactor = 2,
            Denoise = true,
            Sharpen = true,
            EnhanceContrast = true
        };

        var progressList = new List<double>();
        var progress = new Progress<double>(val => progressList.Add(val));

        var result = await enhancer.EnhanceAsync(inputPath, outputPath, options, progress);

        Assert.True(File.Exists(outputPath));
        Assert.Equal(120, result.OriginalWidth);
        Assert.Equal(160, result.OriginalHeight);
        Assert.Equal(240, result.EnhancedWidth);
        Assert.Equal(320, result.EnhancedHeight);
        Assert.NotEmpty(progressList);

        using var resultBmp = SKBitmap.Decode(outputPath);
        Assert.Equal(240, resultBmp.Width);
        Assert.Equal(320, resultBmp.Height);
    }

    [Fact]
    public async Task AiEnhance_4xUpscaling_QuadruplesDimensions()
    {
        var enhancer = new AiEnhancer();
        var inputPath = CreateTestBitmap(50, 50, SKColors.Coral);
        var outputPath = Path.Combine(Path.GetTempPath(), $"enhanced_4x_{Guid.NewGuid():N}.png");
        _tempFiles.Add(outputPath);

        var options = new AiEnhanceOptions
        {
            UpscaleFactor = 4,
            Denoise = false,
            Sharpen = true,
            EnhanceContrast = false
        };

        var result = await enhancer.EnhanceAsync(inputPath, outputPath, options);

        Assert.True(File.Exists(outputPath));
        Assert.Equal(200, result.EnhancedWidth);
        Assert.Equal(200, result.EnhancedHeight);

        using var resultBmp = SKBitmap.Decode(outputPath);
        Assert.Equal(200, resultBmp.Width);
        Assert.Equal(200, resultBmp.Height);
    }

    [Fact]
    public async Task DocumentCleaner_RemovesShadowsAndBinarizes()
    {
        var cleaner = new DocumentCleaner();
        // Create a document with a simulated dark gradient shadow and dark text
        var inputPath = CreateTestBitmap(300, 200, SKColors.White, canvas =>
        {
            using var shadowPaint = new SKPaint
            {
                Color = new SKColor(180, 180, 180, 200)
            };
            canvas.DrawRect(0, 0, 150, 200, shadowPaint);

            using var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            };
            canvas.DrawRect(40, 50, 100, 15, textPaint);
            canvas.DrawRect(40, 80, 160, 15, textPaint);
        });

        var outputPath = Path.Combine(Path.GetTempPath(), $"cleaned_{Guid.NewGuid():N}.png");
        _tempFiles.Add(outputPath);

        var options = new DocumentCleanOptions
        {
            AutoDeskew = false,
            RemoveShadows = true,
            BinarizePureBlackAndWhite = true,
            Threshold = 140
        };

        var result = await cleaner.CleanDocumentAsync(inputPath, outputPath, options);

        Assert.True(File.Exists(outputPath));
        Assert.True(result.ShadowsRemoved);
        Assert.True(result.Binarized);

        using var cleanedBmp = SKBitmap.Decode(outputPath);
        Assert.Equal(300, cleanedBmp.Width);
        Assert.Equal(200, cleanedBmp.Height);
    }

    [Fact]
    public async Task DocumentCleaner_DetectsRotationSkew()
    {
        var cleaner = new DocumentCleaner();
        // Create an image with horizontal text lines rotated by 3 degrees
        var inputPath = CreateTestBitmap(400, 400, SKColors.White, canvas =>
        {
            canvas.Translate(200, 200);
            canvas.RotateDegrees(3.0f);
            canvas.Translate(-200, -200);

            using var paint = new SKPaint { Color = SKColors.Black };
            for (int y = 50; y < 350; y += 30)
            {
                canvas.DrawRect(40, y, 320, 10, paint);
            }
        });

        double skew = await cleaner.DetectSkewAngleAsync(inputPath);
        // The detected angle should be close to 3°
        Assert.True(Math.Abs(skew) > 0.5, $"Detected skew was {skew}");
    }

    [Fact]
    public async Task WindowsOcrEngine_ReturnsAvailableLanguages()
    {
        var ocr = new WindowsOcrEngine();
        var langs = await ocr.GetAvailableLanguagesAsync();

        Assert.NotNull(langs);
        // On Windows 10/11 there is at least one installed display language
        Assert.NotEmpty(langs);
    }

    [Fact]
    public async Task WindowsOcrEngine_CreatesSearchablePdf()
    {
        var ocr = new WindowsOcrEngine();
        var inputPath = CreateTestBitmap(400, 300, SKColors.White, canvas =>
        {
            using var font = new SKFont(SKTypeface.Default, 24);
            using var paint = new SKPaint { Color = SKColors.Black };
            canvas.DrawText("CERTIFICATE OF MERIT", 50, 100, SKTextAlign.Left, font, paint);
            canvas.DrawText("GOVERNMENT OF INDIA", 50, 150, SKTextAlign.Left, font, paint);
        });

        var outputPdf = Path.Combine(Path.GetTempPath(), $"searchable_{Guid.NewGuid():N}.pdf");
        _tempFiles.Add(outputPdf);

        var resultPdf = await ocr.CreateSearchablePdfAsync(inputPath, outputPdf, "en-US");

        Assert.True(File.Exists(resultPdf));
        using var pdfDoc = PdfReader.Open(resultPdf, PdfDocumentOpenMode.Import);
        Assert.Equal(1, pdfDoc.PageCount);
    }
}
