using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;
using Windows.ApplicationModel.DataTransfer;

namespace DastavezStudio.App.ViewModels;

public partial class ScannerOcrViewModel : ObservableObject
{
    private readonly IDocumentCleaner _documentCleaner;
    private readonly IOcrEngine _ocrEngine;
    private readonly ITempFileManager _tempFileManager;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasInputFile))]
    [NotifyPropertyChangedFor(nameof(InputFileName))]
    public partial string? InputFilePath { get; set; }

    public string InputFileName => string.IsNullOrEmpty(InputFilePath) ? string.Empty : Path.GetFileName(InputFilePath);
    public bool HasInputFile => !string.IsNullOrEmpty(InputFilePath) && File.Exists(InputFilePath);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCleanedFile))]
    public partial string? CleanedFilePath { get; set; }

    public bool HasCleanedFile => !string.IsNullOrEmpty(CleanedFilePath) && File.Exists(CleanedFilePath);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSearchablePdf))]
    public partial string? SearchablePdfPath { get; set; }

    public bool HasSearchablePdf => !string.IsNullOrEmpty(SearchablePdfPath) && File.Exists(SearchablePdfPath);

    [ObservableProperty]
    public partial bool AutoDeskew { get; set; } = true;

    [ObservableProperty]
    public partial bool ShadowRemoval { get; set; } = true;

    [ObservableProperty]
    public partial bool DocumentCleanup { get; set; } = true;

    [ObservableProperty]
    public partial bool EnhanceContrast { get; set; } = true;

    [ObservableProperty]
    public partial int Threshold { get; set; } = 128;

    [ObservableProperty]
    public partial string SelectedLanguage { get; set; } = "en-US";

    public ObservableCollection<string> AvailableLanguages { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasExtractedText))]
    public partial string ExtractedText { get; set; } = string.Empty;

    public bool HasExtractedText => !string.IsNullOrWhiteSpace(ExtractedText);

    [ObservableProperty]
    public partial int WordCount { get; set; }

    [ObservableProperty]
    public partial int LineCount { get; set; }

    [ObservableProperty]
    public partial double? DetectedSkewAngle { get; set; }

    [ObservableProperty]
    public partial bool IsProcessing { get; set; }

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Ready for document cleanup and offline OCR.";

    [ObservableProperty]
    public partial string ResultDetails { get; set; } = string.Empty;

    public ScannerOcrViewModel(
        IDocumentCleaner documentCleaner,
        IOcrEngine ocrEngine,
        ITempFileManager tempFileManager)
    {
        _documentCleaner = documentCleaner ?? throw new ArgumentNullException(nameof(documentCleaner));
        _ocrEngine = ocrEngine ?? throw new ArgumentNullException(nameof(ocrEngine));
        _tempFileManager = tempFileManager ?? throw new ArgumentNullException(nameof(tempFileManager));

        _ = LoadLanguagesAsync();
    }

    private async Task LoadLanguagesAsync()
    {
        try
        {
            var langs = await _ocrEngine.GetAvailableLanguagesAsync();
            AvailableLanguages.Clear();
            foreach (var lang in langs)
            {
                AvailableLanguages.Add(lang);
            }
            if (AvailableLanguages.Count > 0 && !AvailableLanguages.Contains(SelectedLanguage))
            {
                SelectedLanguage = AvailableLanguages[0];
            }
        }
        catch
        {
            if (AvailableLanguages.Count == 0)
            {
                AvailableLanguages.Add("en-US");
            }
        }
    }

    public void SetInputFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        InputFilePath = filePath;
        CleanedFilePath = null;
        SearchablePdfPath = null;
        ExtractedText = string.Empty;
        WordCount = 0;
        LineCount = 0;
        DetectedSkewAngle = null;
        ResultDetails = string.Empty;
        Progress = 0;
        StatusMessage = $"Loaded: {Path.GetFileName(filePath)}. Ready to scan or extract text.";
    }

    [RelayCommand]
    public async Task CleanDocumentAsync()
    {
        if (string.IsNullOrEmpty(InputFilePath) || !File.Exists(InputFilePath) || IsProcessing)
            return;

        IsProcessing = true;
        Progress = 0;
        StatusMessage = "Analyzing document, removing shadows, deskewing...";
        _cts = new CancellationTokenSource();

        var tempOutput = _tempFileManager.GetTempFilePath(".png");

        var options = new DocumentCleanOptions
        {
            AutoDeskew = AutoDeskew,
            RemoveShadows = ShadowRemoval,
            BinarizePureBlackAndWhite = DocumentCleanup,
            EnhanceContrast = EnhanceContrast,
            Threshold = Threshold
        };

        var progressReporter = new Progress<double>(val =>
        {
            Progress = val;
            StatusMessage = val switch
            {
                < 30 => "Detecting rotation skew...",
                < 60 => "Normalizing illumination and removing shadows...",
                < 85 => "Applying adaptive binarization for clear text...",
                _ => "Finalizing cleaned document..."
            };
        });

        try
        {
            var result = await _documentCleaner.CleanDocumentAsync(InputFilePath, tempOutput, options, progressReporter, _cts.Token);
            CleanedFilePath = result.OutputPath;
            DetectedSkewAngle = result.DetectedSkewAngleDegrees;

            string deskewInfo = Math.Abs(result.DetectedSkewAngleDegrees) > 0.1
                ? $"Straightened {result.DetectedSkewAngleDegrees:F1}° skew"
                : "Straight alignment verified";

            ResultDetails = $"Cleaned in {result.Duration.TotalSeconds:F2}s ({deskewInfo}, shadows removed, crisp black & white contrast).";
            StatusMessage = "✓ Document cleaning complete! Ready for OCR or export.";
            Progress = 100;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Document cleaning cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Document cleaning error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand]
    public async Task ExtractTextAsync()
    {
        var targetFile = CleanedFilePath ?? InputFilePath;
        if (string.IsNullOrEmpty(targetFile) || !File.Exists(targetFile) || IsProcessing)
            return;

        IsProcessing = true;
        Progress = 0;
        StatusMessage = $"Performing offline OCR text extraction ({SelectedLanguage})...";
        _cts = new CancellationTokenSource();

        try
        {
            var result = await _ocrEngine.ExtractTextAsync(targetFile, SelectedLanguage, _cts.Token);
            ExtractedText = result.FullText;
            LineCount = result.Lines.Count;
            WordCount = result.Lines.Sum(l => l.Words.Count);

            ResultDetails = $"Extracted {WordCount} words across {LineCount} lines in {result.Duration.TotalSeconds:F2}s ({result.Language}).";
            StatusMessage = WordCount > 0
                ? "✓ OCR extraction successful! Text is ready to copy or export."
                : "No text detected in this document.";
            Progress = 100;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "OCR extraction cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"OCR error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand]
    public async Task CreateSearchablePdfAsync()
    {
        var targetFile = CleanedFilePath ?? InputFilePath;
        if (string.IsNullOrEmpty(targetFile) || !File.Exists(targetFile) || IsProcessing)
            return;

        IsProcessing = true;
        Progress = 0;
        StatusMessage = "Generating searchable PDF with overlaid text layer...";
        _cts = new CancellationTokenSource();

        var tempPdf = _tempFileManager.GetTempFilePath(".pdf");

        var progressReporter = new Progress<double>(val =>
        {
            Progress = val;
            StatusMessage = val switch
            {
                < 50 => "Extracting word coordinates...",
                < 80 => "Building vector PDF and embedding text layer...",
                _ => "Finalizing searchable PDF..."
            };
        });

        try
        {
            var resultPath = await _ocrEngine.CreateSearchablePdfAsync(targetFile, tempPdf, SelectedLanguage, progressReporter, _cts.Token);
            SearchablePdfPath = resultPath;
            StatusMessage = "✓ Searchable PDF generated! Ready to save.";
            Progress = 100;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "PDF generation cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Searchable PDF error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand]
    public void CopyExtractedText()
    {
        if (string.IsNullOrEmpty(ExtractedText)) return;

        try
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(ExtractedText);
            Clipboard.SetContent(dataPackage);
            StatusMessage = "✓ Extracted text copied to clipboard!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to copy text: {ex.Message}";
        }
    }
}
