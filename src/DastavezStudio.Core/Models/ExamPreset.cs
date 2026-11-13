namespace DastavezStudio.Core.Models;

/// <summary>
/// Defines formal document requirement specifications for examination and recruitment authorities.
/// </summary>
public class ExamPreset
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Authority { get; set; } = string.Empty; // e.g. "IBPS", "SBI", "SSC", "UPSC", "RRB", "NTA", "State PSC"
    public string ExamName { get; set; } = string.Empty;  // e.g. "IBPS PO / Clerk / SO"
    public DocumentType DocumentType { get; set; }
    
    // Pixel Dimensions
    public int RequiredWidthPx { get; set; }
    public int RequiredHeightPx { get; set; }
    public int? MinWidthPx { get; set; }
    public int? MaxWidthPx { get; set; }
    public int? MinHeightPx { get; set; }
    public int? MaxHeightPx { get; set; }
    
    // File Size bounds in Kilobytes
    public int MinSizeKb { get; set; }
    public int MaxSizeKb { get; set; }
    
    // Quality & Metadata
    public string AllowedFormats { get; set; } = "jpg,jpeg"; // Comma-separated: "jpg,jpeg,png"
    public int? TargetDpi { get; set; } = 200;
    public double AspectRatio => RequiredHeightPx > 0 ? (double)RequiredWidthPx / RequiredHeightPx : 1.0;
    
    // Presets verification metadata
    public string AdditionalInstructions { get; set; } = string.Empty;
    public string SourceNotification { get; set; } = string.Empty;
    public string VerificationDate { get; set; } = string.Empty;
    public bool IsCustom { get; set; } = false;

    public override string ToString() => $"{Authority} - {ExamName} ({DocumentType})";
}
