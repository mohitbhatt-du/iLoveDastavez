using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DastavezStudio.App.Helpers;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;
using Microsoft.UI.Xaml.Media.Imaging;

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
    [NotifyPropertyChangedFor(nameof(HasSourcePdfPreview))]
    public partial BitmapImage? SourcePdfPreview { get; set; }

    public bool HasSourcePdfPreview => SourcePdfPreview != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasProcessedPdfPreview))]
    public partial BitmapImage? ProcessedPdfPreview { get; set; }

    public bool HasProcessedPdfPreview => ProcessedPdfPreview != null;

    [ObservableProperty]
    public partial uint CurrentPageIndex { get; set; } = 0;

    [ObservableProperty]
    public partial uint TotalPages { get; set; } = 0;

    [ObservableProperty]
    public partial string CurrentPageDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string OriginalFileSize { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProcessedFileSize { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CompressionRatio { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int TargetSizeKb { get; set; } = 300;

    [ObservableProperty]
    public partial string CompressionQuality { get; set; } = "Balanced";

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Ready to inspect and process PDF documents.";

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
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

        InputFilePath = path;
        InputFileName = Path.GetFileName(path);
        ProcessedFilePath = null;
        ProcessedPdfPreview = null;
        ProcessedFileSize = string.Empty;
        CompressionRatio = string.Empty;
        CurrentPageIndex = 0;

        try
        {
            var sizeBytes = new FileInfo(path).Length;
            var sizeKb = sizeBytes / 1024.0;
            OriginalFileSize = sizeKb >= 1024 ? $"{sizeKb / 1024.0:F2} MB" : $"{sizeKb:F1} KB";

            TotalPages = await ImagePreviewHelper.GetPdfPageCountAsync(path);
            if (TotalPages == 0)
            {
                var info = await _pdfProcessor.GetPdfInfoAsync(path);
                TotalPages = (uint)info.Count;
            }

            CurrentPageDisplay = TotalPages > 0 ? $"Page 1 of {TotalPages}" : "Page 1";

            // Render live preview of page 0
            SourcePdfPreview = await ImagePreviewHelper.RenderPdfPageAsync(path, 0);

            StatusMessage = $"Loaded PDF: {InputFileName} ({TotalPages} pages, {OriginalFileSize})";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Notice: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task NextPageAsync()
    {
        if (TotalPages <= 1 || CurrentPageIndex >= TotalPages - 1) return;

        CurrentPageIndex++;
        await RefreshCurrentPagePreviewAsync();
    }

    [RelayCommand]
    public async Task PreviousPageAsync()
    {
        if (CurrentPageIndex == 0) return;

        CurrentPageIndex--;
        await RefreshCurrentPagePreviewAsync();
    }

    private async Task RefreshCurrentPagePreviewAsync()
    {
        CurrentPageDisplay = $"Page {CurrentPageIndex + 1} of {TotalPages}";

        if (!string.IsNullOrEmpty(InputFilePath) && File.Exists(InputFilePath))
        {
            SourcePdfPreview = await ImagePreviewHelper.RenderPdfPageAsync(InputFilePath, CurrentPageIndex);
        }

        if (!string.IsNullOrEmpty(ProcessedFilePath) && File.Exists(ProcessedFilePath))
        {
            ProcessedPdfPreview = await ImagePreviewHelper.RenderPdfPageAsync(ProcessedFilePath, CurrentPageIndex);
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
            var originalBytes = new FileInfo(InputFilePath).Length;
            var outBytes = new FileInfo(tempOut).Length;

            var outSizeKb = outBytes / 1024.0;
            ProcessedFileSize = outSizeKb >= 1024 ? $"{outSizeKb / 1024.0:F2} MB" : $"{outSizeKb:F1} KB";

            if (originalBytes > 0 && outBytes < originalBytes)
            {
                var savedPercent = ((double)(originalBytes - outBytes) / originalBytes) * 100.0;
                CompressionRatio = $"Saved {savedPercent:F0}%";
            }
            else
            {
                CompressionRatio = "Optimized";
            }

            // Render live preview of processed PDF
            ProcessedPdfPreview = await ImagePreviewHelper.RenderPdfPageAsync(tempOut, CurrentPageIndex);

            StatusMessage = $"✓ PDF compressed successfully! Output: {ProcessedFileSize} ({CompressionRatio}).";
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
