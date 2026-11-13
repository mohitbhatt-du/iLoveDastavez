using DastavezStudio.Core.Models;

namespace DastavezStudio.Core.Interfaces;

public interface IExamPresetService
{
    Task<IReadOnlyList<ExamPreset>> GetAllPresetsAsync();
    Task<IReadOnlyList<ExamPreset>> GetPresetsByAuthorityAsync(string authority);
    Task<IReadOnlyList<string>> GetSupportedAuthoritiesAsync();
    Task<ComplianceResult> ValidateComplianceAsync(string filePath, ExamPreset preset);
    Task SaveCustomPresetAsync(ExamPreset preset);
    Task DeleteCustomPresetAsync(string presetId);
}
