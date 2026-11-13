using DastavezStudio.Core.Models;

namespace DastavezStudio.Core.Interfaces;

public interface IAiEnhancer
{
    bool IsOnnxModelAvailable { get; }
    IReadOnlyList<string> AvailableHardwareProviders { get; }

    Task<AiEnhanceResult> EnhanceAsync(
        string inputPath,
        string outputPath,
        AiEnhanceOptions options,
        IProgress<double>? progress = null,
        CancellationToken ct = default);
}
