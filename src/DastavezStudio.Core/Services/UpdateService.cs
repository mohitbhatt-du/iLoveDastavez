using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;

namespace DastavezStudio.Core.Services;

public class UpdateService : IUpdateService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    private const string GitHubRepoApi = "https://api.github.com/repos/mohitbhatt-du/iLoveDastavez/releases/latest";

    static UpdateService()
    {
        HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DastavezStudio-App", "1.0"));
        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
    }

    public async Task<AppUpdateInfo> CheckForUpdatesAsync(string currentVersion, CancellationToken ct = default)
    {
        try
        {
            using var response = await HttpClient.GetAsync(GitHubRepoApi, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
            {
                return new AppUpdateInfo(
                    IsUpdateAvailable: false,
                    CurrentVersion: currentVersion,
                    LatestVersion: currentVersion,
                    ReleaseTitle: string.Empty,
                    ReleaseNotes: $"Unable to fetch release details (HTTP {response.StatusCode}).",
                    HtmlUrl: "https://github.com/mohitbhatt-du/iLoveDastavez/releases",
                    InstallerUrl: null,
                    InstallerSizeBytes: 0,
                    PortableZipUrl: null,
                    PublishedAt: DateTime.UtcNow);
            }

            var jsonStream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(jsonStream, cancellationToken: ct);
            var root = doc.RootElement;

            var tagName = root.TryGetProperty("tag_name", out var tagElem) ? tagElem.GetString() ?? "" : "";
            var releaseName = root.TryGetProperty("name", out var nameElem) ? nameElem.GetString() ?? tagName : tagName;
            var body = root.TryGetProperty("body", out var bodyElem) ? bodyElem.GetString() ?? "" : "";
            var htmlUrl = root.TryGetProperty("html_url", out var htmlElem) ? htmlElem.GetString() ?? "" : "";
            var publishedAt = root.TryGetProperty("published_at", out var pubElem) && pubElem.TryGetDateTime(out var dt) ? dt : DateTime.UtcNow;

            string? installerUrl = null;
            long installerSize = 0;
            string? zipUrl = null;

            if (root.TryGetProperty("assets", out var assetsElem) && assetsElem.ValueKind == JsonValueKind.Array)
            {
                foreach (var asset in assetsElem.EnumerateArray())
                {
                    var aName = asset.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                    var aUrl = asset.TryGetProperty("browser_download_url", out var u) ? u.GetString() ?? "" : "";
                    var aSize = asset.TryGetProperty("size", out var s) && s.TryGetInt64(out var sz) ? sz : 0;

                    if (aName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        installerUrl = aUrl;
                        installerSize = aSize;
                    }
                    else if (aName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        zipUrl = aUrl;
                    }
                }
            }

            var isNewer = IsVersionNewer(currentVersion, tagName);

            return new AppUpdateInfo(
                IsUpdateAvailable: isNewer,
                CurrentVersion: currentVersion,
                LatestVersion: tagName,
                ReleaseTitle: releaseName,
                ReleaseNotes: body,
                HtmlUrl: htmlUrl,
                InstallerUrl: installerUrl,
                InstallerSizeBytes: installerSize,
                PortableZipUrl: zipUrl,
                PublishedAt: publishedAt);
        }
        catch (Exception ex)
        {
            return new AppUpdateInfo(
                IsUpdateAvailable: false,
                CurrentVersion: currentVersion,
                LatestVersion: currentVersion,
                ReleaseTitle: string.Empty,
                ReleaseNotes: $"Error checking updates: {ex.Message}",
                HtmlUrl: "https://github.com/mohitbhatt-du/iLoveDastavez/releases",
                InstallerUrl: null,
                InstallerSizeBytes: 0,
                PortableZipUrl: null,
                PublishedAt: DateTime.UtcNow);
        }
    }

    public async Task<string> DownloadUpdateAsync(string downloadUrl, IProgress<UpdateDownloadProgress>? progress = null, CancellationToken ct = default)
    {
        var tempFolder = Path.Combine(Path.GetTempPath(), "DastavezStudio_Update");
        if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);

        var fileName = Path.GetFileName(new Uri(downloadUrl).LocalPath);
        if (string.IsNullOrEmpty(fileName)) fileName = "DastavezStudio-Setup.exe";

        var destPath = Path.Combine(tempFolder, fileName);
        if (File.Exists(destPath)) File.Delete(destPath);

        using var response = await HttpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;

        using var contentStream = await response.Content.ReadAsStreamAsync(ct);
        using var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[16384];
        long totalRead = 0;
        int read;

        while ((read = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, read), ct);
            totalRead += read;

            if (totalBytes > 0 && progress != null)
            {
                var pct = ((double)totalRead / totalBytes) * 100.0;
                progress.Report(new UpdateDownloadProgress(totalRead, totalBytes, pct));
            }
        }

        progress?.Report(new UpdateDownloadProgress(totalRead, totalBytes > 0 ? totalBytes : totalRead, 100.0));
        return destPath;
    }

    public void LaunchInstallerAndExit(string installerFilePath)
    {
        if (!File.Exists(installerFilePath)) throw new FileNotFoundException("Installer not found", installerFilePath);

        var psi = new ProcessStartInfo
        {
            FileName = installerFilePath,
            UseShellExecute = true
        };
        Process.Start(psi);
        Environment.Exit(0);
    }

    private static bool IsVersionNewer(string current, string latest)
    {
        var cleanCurrent = current.Trim().TrimStart('v').Split('-')[0].Split(' ')[0];
        var cleanLatest = latest.Trim().TrimStart('v').Split('-')[0].Split(' ')[0];

        if (Version.TryParse(cleanCurrent, out var curVer) && Version.TryParse(cleanLatest, out var latVer))
        {
            return latVer > curVer;
        }

        return !string.Equals(cleanCurrent, cleanLatest, StringComparison.OrdinalIgnoreCase);
    }
}
