using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using DastavezStudio.Core.Services;

namespace DastavezStudio.UnitTests;

public class UpdateAndSettingsTests
{
    [Fact]
    public async Task SettingsService_CanSaveAndReloadPreferences()
    {
        var service = new SettingsService();
        service.Preferences.ThemeIndex = 2; // Dark theme
        service.Preferences.AutoCleanTempOnExit = true;
        service.Preferences.DefaultDpi = 300;

        await service.SaveAsync();

        var reloaded = new SettingsService();
        await reloaded.LoadAsync();

        Assert.Equal(2, reloaded.Preferences.ThemeIndex);
        Assert.True(reloaded.Preferences.AutoCleanTempOnExit);
        Assert.Equal(300, reloaded.Preferences.DefaultDpi);
    }

    [Theory]
    [InlineData("v1.0.2", "v1.0.3", true)]
    [InlineData("v1.0.2", "v2.0.0", true)]
    [InlineData("v1.0.2", "v1.0.2", false)]
    [InlineData("1.0.2", "1.0.2", false)]
    [InlineData("v1.0.2", "v1.0.1", false)]
    public void UpdateService_VersionComparisonLogic_IsCorrect(string current, string latest, bool expectedNewer)
    {
        var cleanCurrent = current.Trim().TrimStart('v').Split('-')[0].Split(' ')[0];
        var cleanLatest = latest.Trim().TrimStart('v').Split('-')[0].Split(' ')[0];

        bool isNewer;
        if (Version.TryParse(cleanCurrent, out var curVer) && Version.TryParse(cleanLatest, out var latVer))
        {
            isNewer = latVer > curVer;
        }
        else
        {
            isNewer = !string.Equals(cleanCurrent, cleanLatest, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Equal(expectedNewer, isNewer);
    }
}
