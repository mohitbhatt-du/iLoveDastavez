using DastavezStudio.Core.Models;

namespace DastavezStudio.Core.Interfaces;

public interface IOcrEngine
{
    Task<OcrResult> ExtractTextAsync(string imagePath, string language = "en-US", CancellationToken ct = default);

    Task<string> CreateSearchablePdfAsync(
        string imagePath,
        string outputPdfPath,
        string language = "en-US",
        IProgress<double>? progress = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetAvailableLanguagesAsync();
}
