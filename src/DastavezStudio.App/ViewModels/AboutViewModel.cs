using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;

namespace DastavezStudio.App.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    private readonly IUpdateService _updateService;

    public string AppName => "Dastavez Studio";
    public string Publisher => "iLoveDastavez";
    public string Tagline => "हर दस्तावेज़, तैयार!";
    public string Version => "v1.0.3 (LTS Release)";
    public string Platform => ".NET 10.0 • WinUI 3 • Windows App SDK";

    public string CreatorName => "Mohit Bhatt";
    public string CreatorGitHubUrl => "https://github.com/mohitbhatt-du";
    public string RepositoryUrl => "https://github.com/mohitbhatt-du/iLoveDastavez";
    public string ReleasesUrl => "https://github.com/mohitbhatt-du/iLoveDastavez/releases";
    public string IssuesUrl => "https://github.com/mohitbhatt-du/iLoveDastavez/issues";

    [ObservableProperty]
    public partial bool IsCheckingUpdates { get; set; }

    [ObservableProperty]
    public partial string UpdateStatus { get; set; } = "Check for updates from GitHub";

    [ObservableProperty]
    public partial bool HasUpdateResult { get; set; }

    public AboutViewModel(IUpdateService updateService)
    {
        _updateService = updateService;
    }

    [RelayCommand]
    public async Task OpenCreatorGitHubAsync()
    {
        await Windows.System.Launcher.LaunchUriAsync(new Uri(CreatorGitHubUrl));
    }

    [RelayCommand]
    public async Task OpenRepositoryAsync()
    {
        await Windows.System.Launcher.LaunchUriAsync(new Uri(RepositoryUrl));
    }

    [RelayCommand]
    public async Task OpenReleasesAsync()
    {
        await Windows.System.Launcher.LaunchUriAsync(new Uri(ReleasesUrl));
    }

    [RelayCommand]
    public async Task OpenIssuesAsync()
    {
        await Windows.System.Launcher.LaunchUriAsync(new Uri(IssuesUrl));
    }

    [RelayCommand]
    public async Task CheckForUpdatesAsync()
    {
        IsCheckingUpdates = true;
        HasUpdateResult = false;
        UpdateStatus = "Checking for updates...";

        try
        {
            var info = await _updateService.CheckForUpdatesAsync("v1.0.3");
            HasUpdateResult = true;
            if (info.IsUpdateAvailable)
            {
                UpdateStatus = $"🎉 New update {info.LatestVersion} is available!";
            }
            else
            {
                UpdateStatus = "✓ You are running the latest version (v1.0.3).";
            }
        }
        catch (Exception ex)
        {
            UpdateStatus = $"Notice: {ex.Message}";
        }
        finally
        {
            IsCheckingUpdates = false;
        }
    }

    public string PrivacyPolicySummary =>
        "Dastavez Studio runs 100% locally on your PC. No documents, photos, signatures, or personal identity files are ever uploaded or transmitted over the network. Zero analytics. Zero telemetry.";

    public string Disclaimer =>
        "Examination authority names and logos (such as IBPS, SBI, SSC, UPSC, RRB, NTA) are the property of their respective governing bodies. Dastavez Studio is an independent document toolkit and is not affiliated with, authorized, or endorsed by these organizations. Requirements are provided for convenience and must always be cross-verified against official notifications.";
}
