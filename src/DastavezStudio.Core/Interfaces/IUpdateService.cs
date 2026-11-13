using System;
using System.Threading;
using System.Threading.Tasks;
using DastavezStudio.Core.Models;

namespace DastavezStudio.Core.Interfaces;

public interface IUpdateService
{
    Task<AppUpdateInfo> CheckForUpdatesAsync(string currentVersion, CancellationToken ct = default);
    Task<string> DownloadUpdateAsync(string downloadUrl, IProgress<UpdateDownloadProgress>? progress = null, CancellationToken ct = default);
    void LaunchInstallerAndExit(string installerFilePath);
}
