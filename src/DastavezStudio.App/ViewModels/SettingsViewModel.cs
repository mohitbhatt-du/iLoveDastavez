using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;

namespace DastavezStudio.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IUpdateService _updateService;

    public string CurrentAppVersion => "v1.0.3";

    [ObservableProperty]
    public partial int ThemeIndex { get; set; } = 0;

    [ObservableProperty]
    public partial string ThemeStatusMessage { get; set; } = "Theme preference applies across all tools.";

    [ObservableProperty]
    public partial string DefaultExportDirectory { get; set; } = 
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

    [ObservableProperty]
    public partial int DefaultDpi { get; set; } = 200;

    [ObservableProperty]
    public partial int DefaultJpegQuality { get; set; } = 85;

    [ObservableProperty]
    public partial bool AutoCleanTempOnExit { get; set; } = true;

    [ObservableProperty]
    public partial string CleanupStatus { get; set; } = "Temporary session files are clean.";

    // Update Management
    [ObservableProperty]
    public partial bool IsCheckingUpdates { get; set; }

    [ObservableProperty]
    public partial bool IsDownloadingUpdate { get; set; }

    [ObservableProperty]
    public partial bool IsUpdateAvailable { get; set; }

    [ObservableProperty]
    public partial bool IsUpToDate { get; set; }

    [ObservableProperty]
    public partial string UpdateStatusMessage { get; set; } = "Click 'Check for Updates' to verify the latest GitHub release.";

    [ObservableProperty]
    public partial string LatestVersion { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReleaseNotes { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double DownloadPercent { get; set; }

    [ObservableProperty]
    public partial string DownloadProgressText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool CanInstallUpdate { get; set; }

    [ObservableProperty]
    public partial string? DownloadedInstallerPath { get; set; }

    private AppUpdateInfo? _cachedUpdateInfo;

    public SettingsViewModel(ISettingsService settingsService, IUpdateService updateService)
    {
        _settingsService = settingsService;
        _updateService = updateService;

        ThemeIndex = _settingsService.Preferences.ThemeIndex;
        AutoCleanTempOnExit = _settingsService.Preferences.AutoCleanTempOnExit;
        DefaultExportDirectory = _settingsService.Preferences.DefaultExportDirectory;
        DefaultDpi = _settingsService.Preferences.DefaultDpi;
        DefaultJpegQuality = _settingsService.Preferences.DefaultJpegQuality;
    }

    public async void ApplyTheme(int index)
    {
        ThemeIndex = index;
        _settingsService.Preferences.ThemeIndex = index;
        await _settingsService.SaveAsync();

        var elementTheme = index switch
        {
            1 => ElementTheme.Light,
            2 => ElementTheme.Dark,
            _ => ElementTheme.Default
        };

        App.SetTheme(elementTheme);

        var name = index switch
        {
            1 => "Light Theme",
            2 => "Dark Theme",
            _ => "Follow Windows System Theme"
        };
        ThemeStatusMessage = $"✓ Successfully switched to {name}.";
    }

    [RelayCommand]
    public async Task CheckForUpdatesAsync()
    {
        IsCheckingUpdates = true;
        IsUpdateAvailable = false;
        IsUpToDate = false;
        CanInstallUpdate = false;
        UpdateStatusMessage = "Checking GitHub releases for iLoveDastavez / Dastavez Studio...";

        try
        {
            var updateInfo = await _updateService.CheckForUpdatesAsync(CurrentAppVersion);
            _cachedUpdateInfo = updateInfo;

            if (updateInfo.IsUpdateAvailable)
            {
                IsUpdateAvailable = true;
                LatestVersion = updateInfo.LatestVersion;
                ReleaseNotes = string.IsNullOrWhiteSpace(updateInfo.ReleaseNotes) ? "Latest security and performance enhancements." : updateInfo.ReleaseNotes;
                var sizeMb = updateInfo.InstallerSizeBytes > 0 ? $" ({updateInfo.InstallerSizeBytes / (1024.0 * 1024.0):F1} MB)" : "";
                UpdateStatusMessage = $"🎉 New update {updateInfo.LatestVersion} is available!{sizeMb}";
            }
            else
            {
                IsUpToDate = true;
                UpdateStatusMessage = $"✓ You are running the latest version of Dastavez Studio ({CurrentAppVersion}). Everything is up to date!";
            }
        }
        catch (Exception ex)
        {
            UpdateStatusMessage = $"Notice: Unable to check updates ({ex.Message}).";
        }
        finally
        {
            IsCheckingUpdates = false;
        }
    }

    [RelayCommand]
    public async Task DownloadAndInstallUpdateAsync()
    {
        if (_cachedUpdateInfo?.InstallerUrl == null)
        {
            UpdateStatusMessage = "Direct installer download link not available. Opening GitHub releases in browser...";
            if (!string.IsNullOrEmpty(_cachedUpdateInfo?.HtmlUrl))
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri(_cachedUpdateInfo.HtmlUrl));
            }
            return;
        }

        IsDownloadingUpdate = true;
        CanInstallUpdate = false;
        DownloadPercent = 0;
        DownloadProgressText = "Preparing download...";

        var progress = new Progress<UpdateDownloadProgress>(p =>
        {
            DownloadPercent = Math.Clamp(p.PercentComplete, 0.0, 100.0);
            var mbReceived = p.BytesReceived / (1024.0 * 1024.0);
            var mbTotal = p.TotalBytes / (1024.0 * 1024.0);
            DownloadProgressText = $"{p.PercentComplete:F1}% ({mbReceived:F1} MB / {mbTotal:F1} MB)";
        });

        try
        {
            var installerPath = await _updateService.DownloadUpdateAsync(_cachedUpdateInfo.InstallerUrl, progress);
            DownloadedInstallerPath = installerPath;
            CanInstallUpdate = true;
            UpdateStatusMessage = "✓ Download complete! Click 'Restart & Install Now' to complete upgrade.";
        }
        catch (Exception ex)
        {
            UpdateStatusMessage = $"Download failed: {ex.Message}";
        }
        finally
        {
            IsDownloadingUpdate = false;
        }
    }

    [RelayCommand]
    public void LaunchDownloadedInstaller()
    {
        if (!string.IsNullOrEmpty(DownloadedInstallerPath) && File.Exists(DownloadedInstallerPath))
        {
            _updateService.LaunchInstallerAndExit(DownloadedInstallerPath);
        }
    }

    [RelayCommand]
    public void CleanTemporaryFiles()
    {
        try
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "DastavezStudio");
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
                Directory.CreateDirectory(tempDir);
            }
            CleanupStatus = "✓ All temporary workspace cache files have been purged.";
        }
        catch (Exception ex)
        {
            CleanupStatus = $"Notice: {ex.Message}";
        }
    }
}
