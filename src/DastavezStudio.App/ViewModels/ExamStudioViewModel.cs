using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DastavezStudio.App.Helpers;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;
using Microsoft.UI.Xaml.Media.Imaging;

namespace DastavezStudio.App.ViewModels;

public partial class ExamStudioViewModel : ObservableObject
{
    private readonly IExamPresetService _presetService;
    private readonly IImageProcessor _imageProcessor;
    private readonly ITempFileManager _tempFileManager;

    [ObservableProperty]
    public partial string SelectedAuthority { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ExamPreset? SelectedPreset { get; set; }

    [ObservableProperty]
    public partial string? InputFilePath { get; set; }

    [ObservableProperty]
    public partial string? InputFileName { get; set; }

    [ObservableProperty]
    public partial string? ProcessedFilePath { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSourceImage))]
    public partial BitmapImage? SourceImage { get; set; }

    public bool HasSourceImage => SourceImage != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConformedImage))]
    public partial BitmapImage? ConformedImage { get; set; }

    public bool HasConformedImage => ConformedImage != null;

    [ObservableProperty]
    public partial string SourceDimensions { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SourceFileSize { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConformedDimensions { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConformedFileSize { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConformedDpi { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Select an examination authority and document preset to begin.";

    [ObservableProperty]
    public partial bool IsProcessing { get; set; }

    [ObservableProperty]
    public partial ComplianceResult? Compliance { get; set; }

    public ObservableCollection<string> Authorities { get; } = new();
    public ObservableCollection<ExamPreset> FilteredPresets { get; } = new();

    public ExamStudioViewModel(
        IExamPresetService presetService,
        IImageProcessor imageProcessor,
        ITempFileManager tempFileManager)
    {
        _presetService = presetService;
        _imageProcessor = imageProcessor;
        _tempFileManager = tempFileManager;
        _ = LoadInitialDataAsync();
    }

    public async Task LoadInitialDataAsync()
    {
        var auths = await _presetService.GetSupportedAuthoritiesAsync();
        Authorities.Clear();
        foreach (var a in auths)
        {
            Authorities.Add(a);
        }

        if (Authorities.Any())
        {
            SelectedAuthority = Authorities.First();
            await UpdatePresetsForAuthorityAsync(SelectedAuthority);
        }
    }

    async partial void OnSelectedAuthorityChanged(string value)
    {
        await UpdatePresetsForAuthorityAsync(value);
    }

    async partial void OnSelectedPresetChanged(ExamPreset? value)
    {
        if (value != null && !string.IsNullOrEmpty(InputFilePath))
        {
            await ValidateCurrentFileAsync();
            await GenerateLivePreviewAsync();
        }
    }

    private async Task UpdatePresetsForAuthorityAsync(string authority)
    {
        if (string.IsNullOrEmpty(authority)) return;
        var presets = await _presetService.GetPresetsByAuthorityAsync(authority);
        FilteredPresets.Clear();
        foreach (var p in presets)
        {
            FilteredPresets.Add(p);
        }

        if (FilteredPresets.Any())
        {
            SelectedPreset = FilteredPresets.First();
        }
    }

    public async Task SetInputFileAsync(string path)
    {
        if (!File.Exists(path)) return;

        InputFilePath = path;
        InputFileName = Path.GetFileName(path);
        ProcessedFilePath = null;
        ConformedImage = null;
        ConformedDimensions = string.Empty;
        ConformedFileSize = string.Empty;

        // Load source image preview in-memory
        SourceImage = await ImagePreviewHelper.LoadBitmapFromFileAsync(path);

        try
        {
            var dims = await _imageProcessor.GetImageDimensionsAsync(path);
            SourceDimensions = $"{dims.Width} × {dims.Height} px";
            var sizeKb = new FileInfo(path).Length / 1024.0;
            SourceFileSize = sizeKb >= 1024 ? $"{sizeKb / 1024.0:F2} MB" : $"{sizeKb:F1} KB";
        }
        catch
        {
            SourceDimensions = "PDF Document";
            SourceFileSize = $"{new FileInfo(path).Length / 1024.0:F1} KB";
        }

        await ValidateCurrentFileAsync();
        await GenerateLivePreviewAsync();
    }

    [RelayCommand]
    public async Task ValidateCurrentFileAsync()
    {
        var targetFile = ProcessedFilePath ?? InputFilePath;
        if (string.IsNullOrEmpty(targetFile) || SelectedPreset == null)
            return;

        IsProcessing = true;
        StatusMessage = "Analyzing document compliance...";

        Compliance = await _presetService.ValidateComplianceAsync(targetFile, SelectedPreset);

        try
        {
            var dims = await _imageProcessor.GetImageDimensionsAsync(targetFile);
            Compliance.ActualWidthPx = dims.Width;
            Compliance.ActualHeightPx = dims.Height;
            Compliance.ActualDpi = dims.DpiX;

            if (SelectedPreset.RequiredWidthPx > 0 && 
                (dims.Width != SelectedPreset.RequiredWidthPx || dims.Height != SelectedPreset.RequiredHeightPx))
            {
                Compliance.Issues.Add(new ComplianceIssue
                {
                    Severity = ComplianceSeverity.Error,
                    Property = "Dimensions",
                    Message = $"Dimensions are {dims.Width}x{dims.Height} px, but required is {SelectedPreset.RequiredWidthPx}x{SelectedPreset.RequiredHeightPx} px."
                });
            }
        }
        catch
        {
            // PDF or non-image format
        }

        if (Compliance.IsCompliant)
        {
            StatusMessage = "✓ Document meets all technical guidelines for this exam preset.";
        }
        else
        {
            StatusMessage = "⚠ Document needs adjustments to match exam specifications.";
        }

        IsProcessing = false;
    }

    [RelayCommand]
    public async Task AutoConformAsync()
    {
        if (string.IsNullOrEmpty(InputFilePath) || SelectedPreset == null)
        {
            StatusMessage = "Please select an input image first.";
            return;
        }

        await GenerateLivePreviewAsync();

        if (ProcessedFilePath != null)
        {
            StatusMessage = $"✓ Document successfully conformed to {SelectedPreset.Authority} {SelectedPreset.ExamName} ({Compliance?.ActualWidthPx}x{Compliance?.ActualHeightPx} px, {Compliance?.ActualSizeKb:F1} KB)! Ready to export.";
        }
    }

    private async Task GenerateLivePreviewAsync()
    {
        if (string.IsNullOrEmpty(InputFilePath) || SelectedPreset == null || !File.Exists(InputFilePath))
            return;

        IsProcessing = true;
        try
        {
            var ext = Path.GetExtension(InputFilePath).ToLowerInvariant();
            if (ext == ".pdf")
            {
                // PDF handling
                ProcessedFilePath = InputFilePath;
                ConformedImage = SourceImage;
                ConformedDimensions = "PDF Document";
                ConformedFileSize = SourceFileSize;
                return;
            }

            var tempOut = _tempFileManager.GetTempFilePath(".jpg");
            await _imageProcessor.ProcessForExamPresetAsync(InputFilePath, tempOut, SelectedPreset);
            ProcessedFilePath = tempOut;

            // Load conformed image preview in-memory
            ConformedImage = await ImagePreviewHelper.LoadBitmapFromFileAsync(tempOut);

            var outDims = await _imageProcessor.GetImageDimensionsAsync(tempOut);
            ConformedDimensions = $"{outDims.Width} × {outDims.Height} px";
            var outSizeKb = new FileInfo(tempOut).Length / 1024.0;
            ConformedFileSize = $"{outSizeKb:F1} KB";
            ConformedDpi = $"{outDims.DpiX} DPI";

            await ValidateCurrentFileAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Preview error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
