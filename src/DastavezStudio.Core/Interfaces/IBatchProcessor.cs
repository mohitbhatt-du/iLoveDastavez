using DastavezStudio.Core.Models;

namespace DastavezStudio.Core.Interfaces;

public interface IBatchProcessor
{
    Task<BatchSummary> ProcessBatchAsync(
        IReadOnlyList<BatchJobItem> items,
        BatchSettings settings,
        IProgress<BatchProgressUpdate>? progress = null,
        CancellationToken ct = default);

    Task<BatchSummary> RetryFailedAsync(
        IReadOnlyList<BatchJobItem> items,
        BatchSettings settings,
        IProgress<BatchProgressUpdate>? progress = null,
        CancellationToken ct = default);

    string GenerateCollisionSafePath(string targetDirectory, string originalFileName, string targetExtension);
}
