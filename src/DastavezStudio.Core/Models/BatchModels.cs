namespace DastavezStudio.Core.Models;

public enum BatchItemStatus
{
    Pending,
    Processing,
    Success,
    Failed,
    Skipped
}

public enum BatchProcessingMode
{
    ExamPreset,
    CustomResizeCompress,
    PdfCompress
}

public class BatchJobItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string FilePath { get; set; } = string.Empty;
    public string FileName => Path.GetFileName(FilePath);
    public long InputSizeBytes { get; set; }
    public string? OutputPath { get; set; }
    public long? OutputSizeBytes { get; set; }
    public BatchItemStatus Status { get; set; } = BatchItemStatus.Pending;
    public string? ErrorMessage { get; set; }
    public double Progress { get; set; }
    public TimeSpan? Duration { get; set; }
}

public class BatchSettings
{
    public BatchProcessingMode Mode { get; set; } = BatchProcessingMode.ExamPreset;
    public ExamPreset? ExamPreset { get; set; }
    public int? TargetWidth { get; set; }
    public int? TargetHeight { get; set; }
    public bool MaintainAspectRatio { get; set; } = true;
    public int? TargetMaxKb { get; set; }
    public int? TargetMinKb { get; set; }
    public ImageFormatType OutputFormat { get; set; } = ImageFormatType.Jpeg;
    public bool ConvertToGrayscale { get; set; }
    public int Quality { get; set; } = 85;

    public int ImageDpi { get; set; } = 200;
    public string OutputDirectory { get; set; } = string.Empty;
    public int MaxConcurrency { get; set; } = 4;
    public bool OverwriteExisting { get; set; }
}

public class BatchProgressUpdate
{
    public int TotalItems { get; init; }
    public int ProcessedItems { get; init; }
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public string? CurrentItemName { get; init; }
    public double OverallPercent { get; init; }
}

public class BatchSummary
{
    public int TotalItems { get; init; }
    public int SucceededItems { get; init; }
    public int FailedItemsCount { get; init; }
    public int SkippedItems { get; init; }
    public long TotalInputBytes { get; init; }
    public long TotalOutputBytes { get; init; }
    public TimeSpan TotalDuration { get; init; }
    public IReadOnlyList<BatchJobItem> FailedItems { get; init; } = Array.Empty<BatchJobItem>();

    public long BytesSaved => Math.Max(0, TotalInputBytes - TotalOutputBytes);
    public double PercentSaved => TotalInputBytes > 0 ? ((double)BytesSaved / TotalInputBytes) * 100.0 : 0.0;
}
