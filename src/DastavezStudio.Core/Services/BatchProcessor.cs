using System.Diagnostics;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;

namespace DastavezStudio.Core.Services;

public class BatchProcessor : IBatchProcessor
{
    private readonly IImageProcessor _imageProcessor;
    private readonly IPdfProcessor _pdfProcessor;
    private readonly object _pathLock = new();
    private readonly HashSet<string> _reservedPaths = new(StringComparer.OrdinalIgnoreCase);

    public BatchProcessor(IImageProcessor imageProcessor, IPdfProcessor pdfProcessor)
    {
        _imageProcessor = imageProcessor ?? throw new ArgumentNullException(nameof(imageProcessor));
        _pdfProcessor = pdfProcessor ?? throw new ArgumentNullException(nameof(pdfProcessor));
    }

    public async Task<BatchSummary> ProcessBatchAsync(
        IReadOnlyList<BatchJobItem> items,
        BatchSettings settings,
        IProgress<BatchProgressUpdate>? progress = null,
        CancellationToken ct = default)
    {
        return await ExecuteBatchInternalAsync(items, settings, progress, ct);
    }

    public async Task<BatchSummary> RetryFailedAsync(
        IReadOnlyList<BatchJobItem> items,
        BatchSettings settings,
        IProgress<BatchProgressUpdate>? progress = null,
        CancellationToken ct = default)
    {
        var failedItems = items.Where(i => i.Status == BatchItemStatus.Failed).ToList();
        foreach (var item in failedItems)
        {
            item.Status = BatchItemStatus.Pending;
            item.ErrorMessage = null;
            item.Progress = 0;
            item.OutputPath = null;
            item.OutputSizeBytes = null;
            item.Duration = null;
        }

        return await ExecuteBatchInternalAsync(failedItems, settings, progress, ct);
    }

    private async Task<BatchSummary> ExecuteBatchInternalAsync(
        IReadOnlyList<BatchJobItem> items,
        BatchSettings settings,
        IProgress<BatchProgressUpdate>? progress,
        CancellationToken ct)
    {
        if (items.Count == 0)
        {
            return new BatchSummary
            {
                TotalItems = 0,
                SucceededItems = 0,
                FailedItemsCount = 0,
                SkippedItems = 0,
                TotalDuration = TimeSpan.Zero
            };
        }

        if (string.IsNullOrWhiteSpace(settings.OutputDirectory))
        {
            settings.OutputDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "DastavezStudio",
                $"Batch_{DateTime.Now:yyyyMMdd_HHmmss}");
        }

        if (!Directory.Exists(settings.OutputDirectory))
        {
            Directory.CreateDirectory(settings.OutputDirectory);
        }

        lock (_pathLock)
        {
            _reservedPaths.Clear();
        }

        int maxConcurrency = Math.Clamp(settings.MaxConcurrency, 1, 8);
        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

        int totalCount = items.Count;
        int processedCount = 0;
        int successCount = 0;
        int failureCount = 0;
        int skippedCount = 0;

        var totalStopwatch = Stopwatch.StartNew();

        var tasks = items.Select(async item =>
        {
            if (ct.IsCancellationRequested)
            {
                item.Status = BatchItemStatus.Skipped;
                item.ErrorMessage = "Operation cancelled before execution.";
                Interlocked.Increment(ref skippedCount);
                Interlocked.Increment(ref processedCount);
                ReportProgress(totalCount, processedCount, successCount, failureCount, item.FileName, progress);
                return;
            }

            await semaphore.WaitAsync(ct).ConfigureAwait(false);
            var itemStopwatch = Stopwatch.StartNew();

            try
            {
                if (ct.IsCancellationRequested)
                {
                    item.Status = BatchItemStatus.Skipped;
                    item.ErrorMessage = "Operation cancelled.";
                    Interlocked.Increment(ref skippedCount);
                    return;
                }

                item.Status = BatchItemStatus.Processing;
                item.Progress = 10;
                ReportProgress(totalCount, processedCount, successCount, failureCount, item.FileName, progress);

                if (!File.Exists(item.FilePath))
                {
                    throw new FileNotFoundException($"Input file does not exist: {item.FilePath}");
                }

                item.InputSizeBytes = new FileInfo(item.FilePath).Length;

                string targetExt = DetermineTargetExtension(settings);
                string baseName = Path.GetFileNameWithoutExtension(item.FilePath);

                string targetPath;
                if (settings.OverwriteExisting)
                {
                    targetPath = Path.Combine(settings.OutputDirectory, $"{baseName}{targetExt}");
                }
                else
                {
                    targetPath = GenerateCollisionSafePath(settings.OutputDirectory, baseName, targetExt);
                }

                item.Progress = 30;

                await ProcessSingleItemAsync(item.FilePath, targetPath, settings, ct).ConfigureAwait(false);

                if (!File.Exists(targetPath))
                {
                    throw new InvalidOperationException($"Expected output file was not generated: {targetPath}");
                }

                itemStopwatch.Stop();
                item.OutputPath = targetPath;
                item.OutputSizeBytes = new FileInfo(targetPath).Length;
                item.Status = BatchItemStatus.Success;
                item.Progress = 100;
                item.Duration = itemStopwatch.Elapsed;
                item.ErrorMessage = null;
                Interlocked.Increment(ref successCount);
            }
            catch (OperationCanceledException)
            {
                itemStopwatch.Stop();
                item.Status = BatchItemStatus.Skipped;
                item.ErrorMessage = "Operation cancelled.";
                item.Duration = itemStopwatch.Elapsed;
                Interlocked.Increment(ref skippedCount);
            }
            catch (Exception ex)
            {
                itemStopwatch.Stop();
                item.Status = BatchItemStatus.Failed;
                item.ErrorMessage = ex.Message;
                item.Duration = itemStopwatch.Elapsed;
                Interlocked.Increment(ref failureCount);
            }
            finally
            {
                semaphore.Release();
                Interlocked.Increment(ref processedCount);
                ReportProgress(totalCount, processedCount, successCount, failureCount, item.FileName, progress);
            }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);
        totalStopwatch.Stop();

        long totalInput = items.Sum(i => i.InputSizeBytes);
        long totalOutput = items.Where(i => i.Status == BatchItemStatus.Success).Sum(i => i.OutputSizeBytes ?? 0);

        return new BatchSummary
        {
            TotalItems = totalCount,
            SucceededItems = successCount,
            FailedItemsCount = failureCount,
            SkippedItems = skippedCount,
            TotalInputBytes = totalInput,
            TotalOutputBytes = totalOutput,
            TotalDuration = totalStopwatch.Elapsed,
            FailedItems = items.Where(i => i.Status == BatchItemStatus.Failed).ToList()
        };
    }

    private async Task ProcessSingleItemAsync(string inputPath, string outputPath, BatchSettings settings, CancellationToken ct)
    {
        switch (settings.Mode)
        {
            case BatchProcessingMode.ExamPreset:
                if (settings.ExamPreset == null)
                    throw new InvalidOperationException("An Exam Preset must be specified when using ExamPreset mode.");

                await _imageProcessor.ProcessForExamPresetAsync(inputPath, outputPath, settings.ExamPreset, ct).ConfigureAwait(false);
                break;

            case BatchProcessingMode.CustomResizeCompress:
                await ProcessCustomImageAsync(inputPath, outputPath, settings, ct).ConfigureAwait(false);
                break;

            case BatchProcessingMode.PdfCompress:
                var pdfOptions = new PdfCompressionOptions
                {
                    TargetSizeKb = settings.TargetMaxKb,
                    ImageDpi = settings.ImageDpi
                };
                await _pdfProcessor.CompressPdfAsync(inputPath, outputPath, pdfOptions, null, ct).ConfigureAwait(false);
                break;

            default:
                throw new NotSupportedException($"Unsupported batch processing mode: {settings.Mode}");
        }
    }

    private async Task ProcessCustomImageAsync(string inputPath, string outputPath, BatchSettings settings, CancellationToken ct)
    {
        string currentSource = inputPath;
        var tempFiles = new List<string>();

        try
        {
            // Step 1: Grayscale conversion if requested
            if (settings.ConvertToGrayscale)
            {
                string tempGray = Path.Combine(Path.GetTempPath(), $"batch_gray_{Guid.NewGuid():N}.png");
                tempFiles.Add(tempGray);
                await _imageProcessor.AdjustColorsAsync(currentSource, tempGray, 1.0f, 1.0f, true, ct).ConfigureAwait(false);
                currentSource = tempGray;
            }

            // Step 2: Dimensions resize if specified
            if (settings.TargetWidth.HasValue || settings.TargetHeight.HasValue)
            {
                string tempResize = Path.Combine(Path.GetTempPath(), $"batch_res_{Guid.NewGuid():N}.png");
                tempFiles.Add(tempResize);
                await _imageProcessor.ResizeImageAsync(currentSource, tempResize, new ImageResizeOptions
                {
                    WidthPx = settings.TargetWidth,
                    HeightPx = settings.TargetHeight,
                    MaintainAspectRatio = settings.MaintainAspectRatio,
                    TargetDpi = settings.ImageDpi
                }, ct).ConfigureAwait(false);
                currentSource = tempResize;
            }

            // Step 3: Format / Compress to final output
            if (settings.OutputFormat == ImageFormatType.Pdf)
            {
                await _pdfProcessor.ConvertImagesToPdfAsync(new[] { currentSource }, outputPath, ct).ConfigureAwait(false);
            }
            else
            {
                await _imageProcessor.CompressImageAsync(currentSource, outputPath, new ImageCompressionOptions
                {
                    Quality = settings.Quality,
                    TargetMaxKb = settings.TargetMaxKb,
                    TargetMinKb = settings.TargetMinKb,
                    OutputFormat = settings.OutputFormat,
                    StripMetadata = true
                }, ct).ConfigureAwait(false);
            }
        }
        finally
        {
            foreach (var temp in tempFiles)
            {
                try
                {
                    if (File.Exists(temp)) File.Delete(temp);
                }
                catch
                {
                    // Ephemeral cleanup best-effort
                }
            }
        }
    }

    private static string DetermineTargetExtension(BatchSettings settings)
    {
        return settings.Mode switch
        {
            BatchProcessingMode.ExamPreset => ".jpg",
            BatchProcessingMode.PdfCompress => ".pdf",
            BatchProcessingMode.CustomResizeCompress => settings.OutputFormat switch
            {
                ImageFormatType.Png => ".png",
                ImageFormatType.Webp => ".webp",
                ImageFormatType.Pdf => ".pdf",
                _ => ".jpg"
            },
            _ => ".jpg"
        };
    }

    public string GenerateCollisionSafePath(string targetDirectory, string originalFileName, string targetExtension)
    {
        if (!targetExtension.StartsWith('.'))
        {
            targetExtension = "." + targetExtension;
        }

        lock (_pathLock)
        {
            string candidate = Path.Combine(targetDirectory, $"{originalFileName}{targetExtension}");
            if (!File.Exists(candidate) && !_reservedPaths.Contains(candidate))
            {
                _reservedPaths.Add(candidate);
                return candidate;
            }

            int index = 1;
            while (true)
            {
                candidate = Path.Combine(targetDirectory, $"{originalFileName}_{index:D3}{targetExtension}");
                if (!File.Exists(candidate) && !_reservedPaths.Contains(candidate))
                {
                    _reservedPaths.Add(candidate);
                    return candidate;
                }
                index++;
            }
        }
    }

    private static void ReportProgress(
        int total,
        int processed,
        int success,
        int failed,
        string? currentItem,
        IProgress<BatchProgressUpdate>? progress)
    {
        if (progress == null) return;

        double percent = total > 0 ? ((double)processed / total) * 100.0 : 0.0;
        progress.Report(new BatchProgressUpdate
        {
            TotalItems = total,
            ProcessedItems = processed,
            SuccessCount = success,
            FailureCount = failed,
            CurrentItemName = currentItem,
            OverallPercent = Math.Clamp(percent, 0.0, 100.0)
        });
    }
}
