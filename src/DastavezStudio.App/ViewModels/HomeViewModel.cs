using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DastavezStudio.App.ViewModels;

public class QuickActionItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Glyph { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
}

public partial class HomeViewModel : ObservableObject
{
    public List<QuickActionItem> QuickActions { get; } = new()
    {
        new QuickActionItem
        {
            Title = "Exam Document Studio",
            Description = "Prepare photos, signatures & thumb impressions to exact exam specs (IBPS, SSC, UPSC)",
            Glyph = "\uE77B", // Contact
            Tag = "ExamStudio"
        },
        new QuickActionItem
        {
            Title = "Compress PDF",
            Description = "Reduce PDF file size to target KB or MB while maintaining clarity",
            Glyph = "\uE8B7", // Document
            Tag = "PdfTools"
        },
        new QuickActionItem
        {
            Title = "Resize & Compress Image",
            Description = "Iterative target-size compression and pixel-perfect dimension adjustment",
            Glyph = "\uEB9F", // Pictures
            Tag = "ImageTools"
        },
        new QuickActionItem
        {
            Title = "Batch Processing",
            Description = "Bulk format dozens of student records and exam uploads at once",
            Glyph = "\uEA37", // List
            Tag = "Batch"
        },
        new QuickActionItem
        {
            Title = "Document Scanner & OCR",
            Description = "Perspective correction, shadow removal, and offline searchable PDF extraction",
            Glyph = "\uE892", // Scan
            Tag = "ScannerOcr"
        },
        new QuickActionItem
        {
            Title = "Offline AI Upscaling",
            Description = "2x and 4x local super-resolution without remote servers or cloud data leakage",
            Glyph = "\uE7BE", // Repair/Enhance
            Tag = "AiEnhance"
        }
    };
}
