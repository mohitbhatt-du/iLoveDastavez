using System;

namespace DastavezStudio.Core.Models;

public record AppUpdateInfo(
    bool IsUpdateAvailable,
    string CurrentVersion,
    string LatestVersion,
    string ReleaseTitle,
    string ReleaseNotes,
    string HtmlUrl,
    string? InstallerUrl,
    long InstallerSizeBytes,
    string? PortableZipUrl,
    DateTime PublishedAt
);

public record UpdateDownloadProgress(
    long BytesReceived,
    long TotalBytes,
    double PercentComplete
);
