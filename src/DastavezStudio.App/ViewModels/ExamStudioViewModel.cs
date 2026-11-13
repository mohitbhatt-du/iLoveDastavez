using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;

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
        InputFilePath = path;
        InputFileName = Path.GetFileName(path);
        ProcessedFilePath = null;
        await ValidateCurrentFileAsync();
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
            // If PDF or non-image format
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

        IsProcessing = true;
        StatusMessage = $"Processing document to match {SelectedPreset.Authority} ({SelectedPreset.DocumentType}) specs...";

        try
        {
            var tempOut = _tempFileManager.GetTempFilePath(".jpg");
            await _imageProcessor.ProcessForExamPresetAsync(InputFilePath, tempOut, SelectedPreset);
            ProcessedFilePath = tempOut;

            await ValidateCurrentFileAsync();

            StatusMessage = $"✓ Document successfully conformed to {SelectedPreset.Authority} {SelectedPreset.ExamName} ({Compliance?.ActualWidthPx}x{Compliance?.ActualHeightPx} px, {Compliance?.ActualSizeKb:F1} KB)!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error processing document: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
