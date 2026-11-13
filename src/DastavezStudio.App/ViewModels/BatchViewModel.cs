using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;

namespace DastavezStudio.App.ViewModels;

public partial class BatchItemViewModel : ObservableObject
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string FilePath { get; set; } = string.Empty;
    public string FileName => Path.GetFileName(FilePath);
    public string DirectoryName => Path.GetDirectoryName(FilePath) ?? string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InputSizeFormatted))]
    public partial long InputSizeBytes { get; set; }

    [ObservableProperty]
    public partial string? OutputPath { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OutputSizeFormatted))]
    [NotifyPropertyChangedFor(nameof(SavingsPercentageFormatted))]
    public partial long? OutputSizeBytes { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(IsProcessing))]
    [NotifyPropertyChangedFor(nameof(IsSuccess))]
    [NotifyPropertyChangedFor(nameof(IsFailed))]
    public partial BatchItemStatus Status { get; set; } = BatchItemStatus.Pending;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial TimeSpan? Duration { get; set; }

    public bool IsProcessing => Status == BatchItemStatus.Processing;
    public bool IsSuccess => Status == BatchItemStatus.Success;
    public bool IsFailed => Status == BatchItemStatus.Failed;

    public string InputSizeFormatted => FormatBytes(InputSizeBytes);
    public string OutputSizeFormatted => OutputSizeBytes.HasValue ? FormatBytes(OutputSizeBytes.Value) : "-";

    public string SavingsPercentageFormatted
    {
        get
        {
            if (InputSizeBytes <= 0 || !OutputSizeBytes.HasValue) return string.Empty;
            long saved = InputSizeBytes - OutputSizeBytes.Value;
            double pct = ((double)saved / InputSizeBytes) * 100.0;
            return pct > 0 ? $"-{pct:F0}%" : $"+{Math.Abs(pct):F0}%";
        }
    }

    public string StatusText => Status switch
    {
        BatchItemStatus.Pending => "Pending",
        BatchItemStatus.Processing => "Processing...",
        BatchItemStatus.Success => "Completed",
        BatchItemStatus.Failed => "Failed",
        BatchItemStatus.Skipped => "Skipped",
        _ => "Unknown"
    };

    public static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{(bytes / 1024.0):F1} KB";
        return $"{(bytes / (1024.0 * 1024.0)):F2} MB";
    }
}

public partial class BatchViewModel : ObservableObject
{
    private readonly IBatchProcessor _batchProcessor;
    private readonly IExamPresetService _presetService;
    private CancellationTokenSource? _cts;

    public ObservableCollection<BatchItemViewModel> Queue { get; } = new();
    public ObservableCollection<ExamPreset> Presets { get; } = new();

    public IReadOnlyList<string> SupportedFormats { get; } = new List<string> { "JPEG", "PNG", "WEBP", "PDF" };

    [ObservableProperty]
    public partial int SelectedModeIndex { get; set; } = 0; // 0: Exam Preset, 1: Custom Sizing, 2: PDF Compression

    [ObservableProperty]
    public partial ExamPreset? SelectedPreset { get; set; }

    [ObservableProperty]
    public partial int TargetWidth { get; set; } = 200;

    [ObservableProperty]
    public partial int TargetHeight { get; set; } = 230;

    [ObservableProperty]
    public partial bool MaintainAspectRatio { get; set; } = true;

    [ObservableProperty]
    public partial int TargetMaxKb { get; set; } = 50;

    [ObservableProperty]
    public partial int TargetMinKb { get; set; } = 20;

    [ObservableProperty]
    public partial string SelectedFormat { get; set; } = "JPEG";

    [ObservableProperty]
    public partial bool ConvertToGrayscale { get; set; } = false;

    [ObservableProperty]
    public partial int MaxConcurrency { get; set; } = 4;

    [ObservableProperty]
    public partial string OutputDirectory { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool OverwriteExisting { get; set; } = false;

    [ObservableProperty]
    public partial double OverallProgress { get; set; } = 0;

    [ObservableProperty]
    public partial bool IsRunning { get; set; } = false;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Add images or documents to begin batch processing.";

    [ObservableProperty]
    public partial string ProgressSubtext { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int TotalCount { get; set; } = 0;

    [ObservableProperty]
    public partial int SuccessCount { get; set; } = 0;

    [ObservableProperty]
    public partial int FailedCount { get; set; } = 0;

    [ObservableProperty]
    public partial bool HasQueue { get; set; } = false;

    [ObservableProperty]
    public partial bool HasFailures { get; set; } = false;

    [ObservableProperty]
    public partial bool IsCompleted { get; set; } = false;

    [ObservableProperty]
    public partial string SummaryText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BytesSavedText { get; set; } = string.Empty;

    public BatchViewModel(IBatchProcessor batchProcessor, IExamPresetService presetService)
    {
        _batchProcessor = batchProcessor ?? throw new ArgumentNullException(nameof(batchProcessor));
        _presetService = presetService ?? throw new ArgumentNullException(nameof(presetService));

        OutputDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            "DastavezStudio",
            $"Batch_{DateTime.Now:yyyyMMdd_HHmmss}");

        _ = LoadPresetsAsync();
    }

    public async Task LoadPresetsAsync()
    {
        try
        {
            var presets = await _presetService.GetAllPresetsAsync();
            Presets.Clear();
            foreach (var preset in presets)
            {
                Presets.Add(preset);
            }

            if (Presets.Count > 0 && SelectedPreset == null)
            {
                SelectedPreset = Presets.FirstOrDefault(p => p.Id == "ibps-photo") ?? Presets[0];
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load presets: {ex.Message}";
        }
    }

    public void AddFiles(IEnumerable<string> filePaths)
    {
        foreach (var path in filePaths)
        {
            if (!File.Exists(path)) continue;
            if (Queue.Any(q => q.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase))) continue;

            var fi = new FileInfo(path);
            var item = new BatchItemViewModel
            {
                FilePath = path,
                InputSizeBytes = fi.Length,
                Status = BatchItemStatus.Pending
            };
            Queue.Add(item);
        }

        UpdateQueueMetrics();
        IsCompleted = false;
        StatusMessage = $"{Queue.Count} item(s) in queue. Ready to start.";
    }

    [RelayCommand]
    public void RemoveItem(BatchItemViewModel item)
    {
        if (item != null && Queue.Contains(item))
        {
            Queue.Remove(item);
            UpdateQueueMetrics();
        }
    }

    [RelayCommand]
    public void ClearQueue()
    {
        Queue.Clear();
        UpdateQueueMetrics();
        OverallProgress = 0;
        ProgressSubtext = string.Empty;
        IsCompleted = false;
        SummaryText = string.Empty;
        BytesSavedText = string.Empty;
        StatusMessage = "Add images or documents to begin batch processing.";
    }

    [RelayCommand]
    public async Task StartBatchAsync()
    {
        if (Queue.Count == 0 || IsRunning) return;

        IsRunning = true;
        IsCompleted = false;
        OverallProgress = 0;
        SummaryText = string.Empty;
        BytesSavedText = string.Empty;
        StatusMessage = "Preparing batch processing...";

        // Reset queue statuses
        foreach (var item in Queue)
        {
            item.Status = BatchItemStatus.Pending;
            item.Progress = 0;
            item.ErrorMessage = null;
            item.OutputPath = null;
            item.OutputSizeBytes = null;
        }

        _cts = new CancellationTokenSource();

        var settings = BuildBatchSettings();

        // Convert VM queue to Core BatchJobItem
        var jobItems = Queue.Select(vm => new BatchJobItem
        {
            FilePath = vm.FilePath,
            InputSizeBytes = vm.InputSizeBytes
        }).ToList();

        // Mapping map by FilePath
        var itemMap = Queue.ToDictionary(k => k.FilePath, StringComparer.OrdinalIgnoreCase);

        var progress = new Progress<BatchProgressUpdate>(update =>
        {
            OverallProgress = update.OverallPercent;
            ProgressSubtext = $"Processed {update.ProcessedItems} of {update.TotalItems} ({update.OverallPercent:F0}%)";
            StatusMessage = $"Processing: {update.CurrentItemName}...";
            SuccessCount = update.SuccessCount;
            FailedCount = update.FailureCount;
            HasFailures = update.FailureCount > 0;
        });

        try
        {
            var summary = await _batchProcessor.ProcessBatchAsync(jobItems, settings, progress, _cts.Token);

            // Sync statuses back to VM
            foreach (var job in jobItems)
            {
                if (itemMap.TryGetValue(job.FilePath, out var vm))
                {
                    vm.Status = job.Status;
                    vm.OutputPath = job.OutputPath;
                    vm.OutputSizeBytes = job.OutputSizeBytes;
                    vm.ErrorMessage = job.ErrorMessage;
                    vm.Progress = job.Progress;
                    vm.Duration = job.Duration;
                }
            }

            OverallProgress = 100;
            IsCompleted = true;
            UpdateQueueMetrics();

            long savedBytes = summary.BytesSaved;
            double savedPct = summary.PercentSaved;

            SummaryText = $"Completed: {summary.SucceededItems} of {summary.TotalItems} succeeded ({summary.FailedItemsCount} failed, {summary.SkippedItems} skipped) in {summary.TotalDuration.TotalSeconds:F1}s.";
            BytesSavedText = savedBytes > 0
                ? $"Storage Saved: {BatchItemViewModel.FormatBytes(savedBytes)} ({savedPct:F1}%)"
                : "Storage Saved: 0 B";

            StatusMessage = summary.FailedItemsCount > 0
                ? $"Batch completed with {summary.FailedItemsCount} failure(s)."
                : "Batch completed successfully!";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Batch processing cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error during batch processing: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand]
    public void CancelBatch()
    {
        if (IsRunning && _cts != null && !_cts.IsCancellationRequested)
        {
            StatusMessage = "Cancelling batch operation...";
            _cts.Cancel();
        }
    }

    [RelayCommand]
    public async Task RetryFailedAsync()
    {
        var failedVMs = Queue.Where(q => q.Status == BatchItemStatus.Failed).ToList();
        if (failedVMs.Count == 0 || IsRunning) return;

        IsRunning = true;
        IsCompleted = false;
        StatusMessage = $"Retrying {failedVMs.Count} failed item(s)...";

        foreach (var item in failedVMs)
        {
            item.Status = BatchItemStatus.Pending;
            item.Progress = 0;
            item.ErrorMessage = null;
        }

        _cts = new CancellationTokenSource();
        var settings = BuildBatchSettings();

        var failedJobItems = failedVMs.Select(vm => new BatchJobItem
        {
            FilePath = vm.FilePath,
            InputSizeBytes = vm.InputSizeBytes
        }).ToList();

        var itemMap = failedVMs.ToDictionary(k => k.FilePath, StringComparer.OrdinalIgnoreCase);

        var progress = new Progress<BatchProgressUpdate>(update =>
        {
            OverallProgress = update.OverallPercent;
            ProgressSubtext = $"Retried {update.ProcessedItems} of {update.TotalItems} ({update.OverallPercent:F0}%)";
            StatusMessage = $"Processing: {update.CurrentItemName}...";
            SuccessCount = Queue.Count(q => q.Status == BatchItemStatus.Success) + update.SuccessCount;
            FailedCount = update.FailureCount;
            HasFailures = update.FailureCount > 0;
        });

        try
        {
            var summary = await _batchProcessor.ProcessBatchAsync(failedJobItems, settings, progress, _cts.Token);

            foreach (var job in failedJobItems)
            {
                if (itemMap.TryGetValue(job.FilePath, out var vm))
                {
                    vm.Status = job.Status;
                    vm.OutputPath = job.OutputPath;
                    vm.OutputSizeBytes = job.OutputSizeBytes;
                    vm.ErrorMessage = job.ErrorMessage;
                    vm.Progress = job.Progress;
                    vm.Duration = job.Duration;
                }
            }

            IsCompleted = true;
            UpdateQueueMetrics();
            StatusMessage = $"Retry complete. {summary.SucceededItems} recovered.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Retry operation cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error retrying batch items: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand]
    public void OpenOutputFolder()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(OutputDirectory) && Directory.Exists(OutputDirectory))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = OutputDirectory,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Cannot open output folder: {ex.Message}";
        }
    }

    private BatchSettings BuildBatchSettings()
    {
        var mode = SelectedModeIndex switch
        {
            0 => BatchProcessingMode.ExamPreset,
            1 => BatchProcessingMode.CustomResizeCompress,
            2 => BatchProcessingMode.PdfCompress,
            _ => BatchProcessingMode.ExamPreset
        };

        var format = SelectedFormat.ToUpperInvariant() switch
        {
            "PNG" => ImageFormatType.Png,
            "WEBP" => ImageFormatType.Webp,
            "PDF" => ImageFormatType.Pdf,
            _ => ImageFormatType.Jpeg
        };

        return new BatchSettings
        {
            Mode = mode,
            ExamPreset = SelectedPreset,
            TargetWidth = TargetWidth > 0 ? TargetWidth : null,
            TargetHeight = TargetHeight > 0 ? TargetHeight : null,
            MaintainAspectRatio = MaintainAspectRatio,
            TargetMaxKb = TargetMaxKb > 0 ? TargetMaxKb : null,
            TargetMinKb = TargetMinKb > 0 ? TargetMinKb : null,
            OutputFormat = format,
            ConvertToGrayscale = ConvertToGrayscale,
            MaxConcurrency = Math.Clamp(MaxConcurrency, 1, 8),
            OutputDirectory = OutputDirectory,
            OverwriteExisting = OverwriteExisting
        };
    }

    private void UpdateQueueMetrics()
    {
        TotalCount = Queue.Count;
        SuccessCount = Queue.Count(q => q.Status == BatchItemStatus.Success);
        FailedCount = Queue.Count(q => q.Status == BatchItemStatus.Failed);
        HasQueue = Queue.Count > 0;
        HasFailures = FailedCount > 0;
    }
}
