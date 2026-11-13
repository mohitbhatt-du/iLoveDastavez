namespace DastavezStudio.Core.Models;

public enum ComplianceSeverity
{
    Success,
    Warning,
    Error
}

public class ComplianceIssue
{
    public ComplianceSeverity Severity { get; set; }
    public string Property { get; set; } = string.Empty; // e.g. "Dimensions", "FileSize", "Format", "DPI"
    public string Message { get; set; } = string.Empty;
}

public class ComplianceResult
{
    public bool IsCompliant => !Issues.Any(i => i.Severity == ComplianceSeverity.Error);
    public bool HasWarnings => Issues.Any(i => i.Severity == ComplianceSeverity.Warning);
    
    public int ActualWidthPx { get; set; }
    public int ActualHeightPx { get; set; }
    public long ActualSizeBytes { get; set; }
    public double ActualSizeKb => Math.Round(ActualSizeBytes / 1024.0, 2);
    public string ActualFormat { get; set; } = string.Empty;
    public int? ActualDpi { get; set; }
    
    public List<ComplianceIssue> Issues { get; set; } = new();

    public static ComplianceResult Valid(int width, int height, long sizeBytes, string format, int? dpi = null)
    {
        return new ComplianceResult
        {
            ActualWidthPx = width,
            ActualHeightPx = height,
            ActualSizeBytes = sizeBytes,
            ActualFormat = format,
            ActualDpi = dpi
        };
    }
}
