using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;

namespace DastavezStudio.App.ViewModels;

public partial class ExamStudioViewModel : ObservableObject
{
    private readonly IExamPresetService _presetService;

    [ObservableProperty]
    public partial string SelectedAuthority { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ExamPreset? SelectedPreset { get; set; }

    [ObservableProperty]
    public partial string? InputFilePath { get; set; }

    [ObservableProperty]
    public partial string? InputFileName { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Select an examination authority and document preset to begin.";

    [ObservableProperty]
    public partial bool IsProcessing { get; set; }

    [ObservableProperty]
    public partial ComplianceResult? Compliance { get; set; }

    public ObservableCollection<string> Authorities { get; } = new();
    public ObservableCollection<ExamPreset> FilteredPresets { get; } = new();

    public ExamStudioViewModel(IExamPresetService presetService)
    {
        _presetService = presetService;
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
        await ValidateCurrentFileAsync();
    }

    [RelayCommand]
    public async Task ValidateCurrentFileAsync()
    {
        if (string.IsNullOrEmpty(InputFilePath) || SelectedPreset == null)
            return;

        IsProcessing = true;
        StatusMessage = "Analyzing document compliance...";

        Compliance = await _presetService.ValidateComplianceAsync(InputFilePath, SelectedPreset);

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
}
