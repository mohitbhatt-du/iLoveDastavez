using SkiaSharp;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;
using DastavezStudio.ImageEngine;
using Xunit;

namespace DastavezStudio.UnitTests;

public class ImageProcessorTests
{
    private string CreateTestImage(int width, int height, SKColor color)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_img_{Guid.NewGuid():N}.jpg");
        using var bmp = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(color);

        using var image = SKImage.FromBitmap(bmp);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 95);
        using var stream = File.OpenWrite(tempFile);
        data.SaveTo(stream);

        return tempFile;
    }

    [Fact]
    public async Task GetImageDimensionsAsync_ReturnsAccurateDimensions()
    {
        var processor = new ImageProcessor();
        var tempFile = CreateTestImage(400, 300, SKColors.CornflowerBlue);

        try
        {
            var dims = await processor.GetImageDimensionsAsync(tempFile);
            Assert.Equal(400, dims.Width);
            Assert.Equal(300, dims.Height);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ResizeImageAsync_PreservesAspectRatio()
    {
        var processor = new ImageProcessor();
        var tempInput = CreateTestImage(1000, 500, SKColors.Crimson);
        var tempOutput = Path.Combine(Path.GetTempPath(), $"resized_{Guid.NewGuid():N}.jpg");

        try
        {
            await processor.ResizeImageAsync(tempInput, tempOutput, new ImageResizeOptions
            {
                WidthPx = 500,
                HeightPx = 500,
                MaintainAspectRatio = true
            });

            var dims = await processor.GetImageDimensionsAsync(tempOutput);
            Assert.Equal(500, dims.Width);
            Assert.Equal(250, dims.Height); // 2:1 ratio preserved
        }
        finally
        {
            if (File.Exists(tempInput)) File.Delete(tempInput);
            if (File.Exists(tempOutput)) File.Delete(tempOutput);
        }
    }

    [Fact]
    public async Task CompressImageAsync_AchievesTargetFileSize()
    {
        var processor = new ImageProcessor();
        var tempInput = CreateTestImage(1200, 1200, SKColors.ForestGreen);
        var tempOutput = Path.Combine(Path.GetTempPath(), $"compressed_{Guid.NewGuid():N}.jpg");

        try
        {
            await processor.CompressImageAsync(tempInput, tempOutput, new ImageCompressionOptions
            {
                TargetMaxKb = 40,
                OutputFormat = ImageFormatType.Jpeg
            });

            var fileInfo = new FileInfo(tempOutput);
            var sizeKb = fileInfo.Length / 1024.0;
            Assert.True(sizeKb <= 45.0, $"Actual size {sizeKb:F1} KB should be close to or under 40 KB target.");
        }
        finally
        {
            if (File.Exists(tempInput)) File.Delete(tempInput);
            if (File.Exists(tempOutput)) File.Delete(tempOutput);
        }
    }

    [Fact]
    public async Task ProcessForExamPresetAsync_ConformsToIbpsPhotoSpec()
    {
        var processor = new ImageProcessor();
        var tempInput = CreateTestImage(800, 1000, SKColors.DarkSlateBlue);
        var tempOutput = Path.Combine(Path.GetTempPath(), $"ibps_{Guid.NewGuid():N}.jpg");

        var ibpsPreset = new ExamPreset
        {
            Authority = "IBPS",
            RequiredWidthPx = 200,
            RequiredHeightPx = 230,
            MinSizeKb = 20,
            MaxSizeKb = 50,
            AllowedFormats = "jpg,jpeg"
        };

        try
        {
            await processor.ProcessForExamPresetAsync(tempInput, tempOutput, ibpsPreset);

            var dims = await processor.GetImageDimensionsAsync(tempOutput);
            var fileInfo = new FileInfo(tempOutput);
            var sizeKb = fileInfo.Length / 1024.0;

            Assert.Equal(200, dims.Width);
            Assert.Equal(230, dims.Height);
            Assert.True(sizeKb <= 50.0, $"File size ({sizeKb} KB) must not exceed 50 KB.");
        }
        finally
        {
            if (File.Exists(tempInput)) File.Delete(tempInput);
            if (File.Exists(tempOutput)) File.Delete(tempOutput);
        }
    }

    [Fact]
    public void GenerateDefaultLogos_GeneratesExpectedPngFiles()
    {
        var targetDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "DastavezStudio.App", "Assets", "Logos"));
        LogoGenerator.GenerateDefaultLogos(targetDir);

        Assert.True(File.Exists(Path.Combine(targetDir, "ibps.png")));
        Assert.True(File.Exists(Path.Combine(targetDir, "sbi.png")));
        Assert.True(File.Exists(Path.Combine(targetDir, "ssc.png")));
        Assert.True(File.Exists(Path.Combine(targetDir, "upsc.png")));
        Assert.True(File.Exists(Path.Combine(targetDir, "rrb.png")));
        Assert.True(File.Exists(Path.Combine(targetDir, "nta.png")));
        Assert.True(File.Exists(Path.Combine(targetDir, "psc.png")));
    }
}
