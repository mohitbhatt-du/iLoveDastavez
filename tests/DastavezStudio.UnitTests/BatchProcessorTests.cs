using SkiaSharp;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;
using DastavezStudio.Core.Services;
using DastavezStudio.ExamPresets;
using DastavezStudio.ImageEngine;
using DastavezStudio.PdfEngine;
using Xunit;

namespace DastavezStudio.UnitTests;

public class BatchProcessorTests : IDisposable
{
    private readonly string _testOutputDir;
    private readonly List<string> _tempFiles = new();
    private readonly IImageProcessor _imageProcessor;
    private readonly IPdfProcessor _pdfProcessor;
    private readonly IExamPresetService _presetService;

    public BatchProcessorTests()
    {
        _testOutputDir = Path.Combine(Path.GetTempPath(), $"batch_test_out_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testOutputDir);
        _imageProcessor = new ImageProcessor();
        _pdfProcessor = new PdfProcessor();
        _presetService = new ExamPresetService();
    }

    private string CreateTestImage(int width, int height, SKColor color, string? fileName = null)
    {
        var dir = Path.GetTempPath();
        var file = fileName != null
            ? Path.Combine(dir, fileName)
            : Path.Combine(dir, $"batch_img_{Guid.NewGuid():N}.jpg");

        using var bmp = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(color);

        using var image = SKImage.FromBitmap(bmp);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 95);
        using var stream = File.Open(file, FileMode.Create, FileAccess.Write);
        data.SaveTo(stream);

        _tempFiles.Add(file);
        return file;
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            try { if (File.Exists(file)) File.Delete(file); } catch { }
        }

        try
        {
            if (Directory.Exists(_testOutputDir))
            {
                Directory.Delete(_testOutputDir, true);
            }
        }
        catch { }
    }

    [Fact]
    public async Task ProcessBatchAsync_WithExamPreset_ProcessesAllConcurrently()
    {
        var processor = new BatchProcessor(_imageProcessor, _pdfProcessor);
        var allPresets = await _presetService.GetAllPresetsAsync();
        var preset = allPresets.FirstOrDefault(p => p.Id == "ibps-photo");
        Assert.NotNull(preset);

        var items = new List<BatchJobItem>();
        for (int i = 0; i < 4; i++)
        {
            var imgPath = CreateTestImage(600, 600, SKColors.Teal);
            items.Add(new BatchJobItem
            {
                FilePath = imgPath,
                InputSizeBytes = new FileInfo(imgPath).Length
            });
        }

        var settings = new BatchSettings
        {
            Mode = BatchProcessingMode.ExamPreset,
            ExamPreset = preset,
            OutputDirectory = _testOutputDir,
            MaxConcurrency = 3
        };

        var progressUpdates = new List<BatchProgressUpdate>();
        var progress = new Progress<BatchProgressUpdate>(update =>
        {
            lock (progressUpdates)
            {
                progressUpdates.Add(update);
            }
        });

        var summary = await processor.ProcessBatchAsync(items, settings, progress);

        Assert.Equal(4, summary.TotalItems);
        Assert.Equal(4, summary.SucceededItems);
        Assert.Equal(0, summary.FailedItemsCount);
        Assert.Equal(0, summary.SkippedItems);
        Assert.True(summary.TotalDuration > TimeSpan.Zero);

        foreach (var item in items)
        {
            Assert.Equal(BatchItemStatus.Success, item.Status);
            Assert.NotNull(item.OutputPath);
            Assert.True(File.Exists(item.OutputPath));

            var dims = await _imageProcessor.GetImageDimensionsAsync(item.OutputPath);
            Assert.Equal(preset.RequiredWidthPx, dims.Width);
            Assert.Equal(preset.RequiredHeightPx, dims.Height);

            long fileSizeBytes = new FileInfo(item.OutputPath).Length;
            Assert.True(fileSizeBytes <= preset.MaxSizeKb * 1024, $"File {item.OutputPath} was {fileSizeBytes} bytes");
        }
    }

    [Fact]
    public async Task ProcessBatchAsync_CollisionSafeFilenames_ResolvesDuplicateNames()
    {
        var processor = new BatchProcessor(_imageProcessor, _pdfProcessor);

        // Create two subfolders with identically named files: photo.jpg
        var dirA = Path.Combine(Path.GetTempPath(), $"subA_{Guid.NewGuid():N}");
        var dirB = Path.Combine(Path.GetTempPath(), $"subB_{Guid.NewGuid():N}");
        Directory.CreateDirectory(dirA);
        Directory.CreateDirectory(dirB);

        var fileA = Path.Combine(dirA, "applicant_doc.jpg");
        var fileB = Path.Combine(dirB, "applicant_doc.jpg");

        using (var bmp = new SKBitmap(100, 100))
        using (var canvas = new SKCanvas(bmp))
        {
            canvas.Clear(SKColors.Blue);
            using var img = SKImage.FromBitmap(bmp);
            using var data = img.Encode(SKEncodedImageFormat.Jpeg, 90);
            using (var s = File.Create(fileA)) data.SaveTo(s);
            using (var s = File.Create(fileB)) data.SaveTo(s);
        }

        _tempFiles.Add(fileA);
        _tempFiles.Add(fileB);

        var items = new List<BatchJobItem>
        {
            new() { FilePath = fileA },
            new() { FilePath = fileB }
        };

        var settings = new BatchSettings
        {
            Mode = BatchProcessingMode.CustomResizeCompress,
            OutputFormat = ImageFormatType.Jpeg,
            TargetWidth = 50,
            TargetHeight = 50,
            OutputDirectory = _testOutputDir,
            OverwriteExisting = false
        };

        var summary = await processor.ProcessBatchAsync(items, settings);

        Assert.Equal(2, summary.SucceededItems);
        Assert.NotEqual(items[0].OutputPath, items[1].OutputPath);
        Assert.True(File.Exists(items[0].OutputPath!));
        Assert.True(File.Exists(items[1].OutputPath!));

        try { Directory.Delete(dirA, true); } catch { }
        try { Directory.Delete(dirB, true); } catch { }
    }

    [Fact]
    public async Task ProcessBatchAsync_WithCancellation_HaltsGracefully()
    {
        var processor = new BatchProcessor(_imageProcessor, _pdfProcessor);

        var items = new List<BatchJobItem>();
        for (int i = 0; i < 5; i++)
        {
            var imgPath = CreateTestImage(200, 200, SKColors.Purple);
            items.Add(new BatchJobItem { FilePath = imgPath });
        }

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        var settings = new BatchSettings
        {
            Mode = BatchProcessingMode.CustomResizeCompress,
            OutputDirectory = _testOutputDir
        };

        var summary = await processor.ProcessBatchAsync(items, settings, null, cts.Token);

        Assert.Equal(5, summary.TotalItems);
        Assert.True(summary.SkippedItems > 0 || summary.FailedItemsCount > 0);
    }

    [Fact]
    public async Task RetryFailedAsync_OnlyRetriesFailedItems()
    {
        var processor = new BatchProcessor(_imageProcessor, _pdfProcessor);

        var validFile = CreateTestImage(200, 200, SKColors.Orange);
        var missingFile = Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}.jpg");

        var items = new List<BatchJobItem>
        {
            new() { FilePath = validFile },
            new() { FilePath = missingFile }
        };

        var settings = new BatchSettings
        {
            Mode = BatchProcessingMode.CustomResizeCompress,
            TargetWidth = 100,
            TargetHeight = 100,
            OutputDirectory = _testOutputDir
        };

        // First run: 1 success, 1 failure
        var summary1 = await processor.ProcessBatchAsync(items, settings);
        Assert.Equal(1, summary1.SucceededItems);
        Assert.Equal(1, summary1.FailedItemsCount);
        Assert.Equal(BatchItemStatus.Success, items[0].Status);
        Assert.Equal(BatchItemStatus.Failed, items[1].Status);

        // Now create the missing file
        CreateTestImage(200, 200, SKColors.DarkGreen, Path.GetFileName(missingFile));

        // Retry failed
        var retrySummary = await processor.RetryFailedAsync(items, settings);
        Assert.Equal(1, retrySummary.TotalItems);
        Assert.Equal(1, retrySummary.SucceededItems);
        Assert.Equal(0, retrySummary.FailedItemsCount);
        Assert.Equal(BatchItemStatus.Success, items[1].Status);
    }
}
