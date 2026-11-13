using DastavezStudio.Core.Models;

namespace DastavezStudio.Core.Interfaces;

public interface IDocumentCleaner
{
    Task<DocumentCleanResult> CleanDocumentAsync(
        string inputPath,
        string outputPath,
        DocumentCleanOptions options,
        IProgress<double>? progress = null,
        CancellationToken ct = default);

    Task<double> DetectSkewAngleAsync(string inputPath, CancellationToken ct = default);
}
