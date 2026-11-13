using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DastavezStudio.Core.Interfaces;

namespace DastavezStudio.App.ViewModels;

public partial class PdfToolsViewModel : ObservableObject
{
    private readonly IPdfProcessor _pdfProcessor;
    private readonly ITempFileManager _tempFileManager;

    [ObservableProperty]
    public partial string ActiveTool { get; set; } = "Compress";

    [ObservableProperty]
    public partial string? InputFilePath { get; set; }

    [ObservableProperty]
    public partial string? InputFileName { get; set; }

    [ObservableProperty]
    public partial string? ProcessedFilePath { get; set; }

    [ObservableProperty]
    public partial int PageCount { get; set; }

    [ObservableProperty]
    public partial int TargetSizeKb { get; set; } = 300;

    [ObservableProperty]
    public partial string CompressionQuality { get; set; } = "Balanced";

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Ready to process PDF documents.";

    [ObservableProperty]
    public partial bool IsProcessing { get; set; }

    public ObservableCollection<string> SelectedMergeFiles { get; } = new();

    public PdfToolsViewModel(IPdfProcessor pdfProcessor, ITempFileManager tempFileManager)
    {
        _pdfProcessor = pdfProcessor;
        _tempFileManager = tempFileManager;
    }

    public async Task SetInputFileAsync(string path)
    {
        InputFilePath = path;
        InputFileName = Path.GetFileName(path);
        ProcessedFilePath = null;

        try
        {
            var info = await _pdfProcessor.GetPdfInfoAsync(path);
            PageCount = info.Count;
            var sizeKb = new FileInfo(path).Length / 1024.0;
            StatusMessage = $"Loaded PDF: {InputFileName} ({PageCount} pages, {sizeKb:F1} KB)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Notice: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task CompressPdfAsync()
    {
        if (string.IsNullOrEmpty(InputFilePath) || !File.Exists(InputFilePath))
        {
            StatusMessage = "Please select a PDF document first.";
            return;
        }

        IsProcessing = true;
        StatusMessage = "Compressing PDF streams and optimizing page resources...";

        try
        {
            var tempOut = _tempFileManager.GetTempFilePath(".pdf");
            await _pdfProcessor.CompressPdfAsync(InputFilePath, tempOut, new PdfCompressionOptions
            {
                TargetSizeKb = TargetSizeKb,
                Level = PdfCompressionLevel.Balanced
            });

            ProcessedFilePath = tempOut;
            var outSizeKb = new FileInfo(tempOut).Length / 1024.0;
            StatusMessage = $"✓ PDF compressed successfully! Size: {outSizeKb:F1} KB.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
