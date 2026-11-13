using System.Text.Json;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;

namespace DastavezStudio.ExamPresets;

public class ExamPresetService : IExamPresetService
{
    private readonly string _customPresetsPath;
    private readonly List<ExamPreset> _builtinPresets;

    public ExamPresetService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var studioFolder = Path.Combine(appData, "iLoveDastavez", "DastavezStudio");
        if (!Directory.Exists(studioFolder))
        {
            Directory.CreateDirectory(studioFolder);
        }
        _customPresetsPath = Path.Combine(studioFolder, "custom_presets.json");

        _builtinPresets = InitializeBuiltinPresets();
    }

    private static List<ExamPreset> InitializeBuiltinPresets()
    {
        return new List<ExamPreset>
        {
            // IBPS
            new()
            {
                Id = "ibps-photo",
                Authority = "IBPS",
                ExamName = "IBPS PO / Clerk / SO / RRB",
                DocumentType = DocumentType.PassportPhoto,
                RequiredWidthPx = 200,
                RequiredHeightPx = 230,
                MinWidthPx = 200,
                MaxWidthPx = 200,
                MinHeightPx = 230,
                MaxHeightPx = 230,
                MinSizeKb = 20,
                MaxSizeKb = 50,
                AllowedFormats = "jpg,jpeg",
                TargetDpi = 200,
                AdditionalInstructions = "Color photo against white/light background. 200 x 230 pixels, 20 KB to 50 KB.",
                SourceNotification = "IBPS Official Notification Guidelines",
                VerificationDate = "2026",
                IsCustom = false
            },
            new()
            {
                Id = "ibps-signature",
                Authority = "IBPS",
                ExamName = "IBPS PO / Clerk / SO / RRB",
                DocumentType = DocumentType.Signature,
                RequiredWidthPx = 140,
                RequiredHeightPx = 60,
                MinWidthPx = 140,
                MaxWidthPx = 140,
                MinHeightPx = 60,
                MaxHeightPx = 60,
                MinSizeKb = 10,
                MaxSizeKb = 20,
                AllowedFormats = "jpg,jpeg",
                TargetDpi = 200,
                AdditionalInstructions = "Signed with black ink on white paper. Do not use capital/block letters for signature.",
                SourceNotification = "IBPS Official Notification Guidelines",
                VerificationDate = "2026",
                IsCustom = false
            },
            new()
            {
                Id = "ibps-lti",
                Authority = "IBPS",
                ExamName = "IBPS PO / Clerk / SO / RRB",
                DocumentType = DocumentType.LeftThumbImpression,
                RequiredWidthPx = 240,
                RequiredHeightPx = 240,
                MinWidthPx = 240,
                MaxWidthPx = 240,
                MinHeightPx = 240,
                MaxHeightPx = 240,
                MinSizeKb = 20,
                MaxSizeKb = 50,
                AllowedFormats = "jpg,jpeg",
                TargetDpi = 200,
                AdditionalInstructions = "Clear left thumb impression using blue or black ink on white paper.",
                SourceNotification = "IBPS Official Notification Guidelines",
                VerificationDate = "2026",
                IsCustom = false
            },
            new()
            {
                Id = "ibps-declaration",
                Authority = "IBPS",
                ExamName = "IBPS PO / Clerk / SO / RRB",
                DocumentType = DocumentType.HandwrittenDeclaration,
                RequiredWidthPx = 800,
                RequiredHeightPx = 400,
                MinWidthPx = 800,
                MaxWidthPx = 800,
                MinHeightPx = 400,
                MaxHeightPx = 400,
                MinSizeKb = 50,
                MaxSizeKb = 100,
                AllowedFormats = "jpg,jpeg",
                TargetDpi = 200,
                AdditionalInstructions = "Handwritten statement in English on white paper with black ink. In user's own handwriting.",
                SourceNotification = "IBPS Official Notification Guidelines",
                VerificationDate = "2026",
                IsCustom = false
            },

            // SBI
            new()
            {
                Id = "sbi-photo",
                Authority = "SBI",
                ExamName = "SBI PO / Clerk",
                DocumentType = DocumentType.PassportPhoto,
                RequiredWidthPx = 200,
                RequiredHeightPx = 230,
                MinWidthPx = 200,
                MaxWidthPx = 200,
                MinHeightPx = 230,
                MaxHeightPx = 230,
                MinSizeKb = 20,
                MaxSizeKb = 50,
                AllowedFormats = "jpg,jpeg",
                TargetDpi = 200,
                AdditionalInstructions = "Recent passport photograph, 200 x 230 pixels, light background.",
                SourceNotification = "SBI Recruitment Notification",
                VerificationDate = "2026",
                IsCustom = false
            },
            new()
            {
                Id = "sbi-signature",
                Authority = "SBI",
                ExamName = "SBI PO / Clerk",
                DocumentType = DocumentType.Signature,
                RequiredWidthPx = 140,
                RequiredHeightPx = 60,
                MinWidthPx = 140,
                MaxWidthPx = 140,
                MinHeightPx = 60,
                MaxHeightPx = 60,
                MinSizeKb = 10,
                MaxSizeKb = 20,
                AllowedFormats = "jpg,jpeg",
                TargetDpi = 200,
                AdditionalInstructions = "Black ink on white paper, 140 x 60 pixels, 10-20 KB.",
                SourceNotification = "SBI Recruitment Notification",
                VerificationDate = "2026",
                IsCustom = false
            },

            // SSC
            new()
            {
                Id = "ssc-photo",
                Authority = "SSC",
                ExamName = "SSC CGL / CHSL / MTS / GD",
                DocumentType = DocumentType.PassportPhoto,
                RequiredWidthPx = 350,
                RequiredHeightPx = 450,
                MinWidthPx = 300,
                MaxWidthPx = 500,
                MinHeightPx = 400,
                MaxHeightPx = 600,
                MinSizeKb = 20,
                MaxSizeKb = 50,
                AllowedFormats = "jpg,jpeg",
                TargetDpi = 100,
                AdditionalInstructions = "3.5 cm x 4.5 cm (width x height), clear front facing without cap or spectacles.",
                SourceNotification = "Staff Selection Commission Guidelines",
                VerificationDate = "2026",
                IsCustom = false
            },
            new()
            {
                Id = "ssc-signature",
                Authority = "SSC",
                ExamName = "SSC CGL / CHSL / MTS / GD",
                DocumentType = DocumentType.Signature,
                RequiredWidthPx = 400,
                RequiredHeightPx = 200,
                MinWidthPx = 200,
                MaxWidthPx = 500,
                MinHeightPx = 100,
                MaxHeightPx = 300,
                MinSizeKb = 10,
                MaxSizeKb = 20,
                AllowedFormats = "jpg,jpeg",
                TargetDpi = 100,
                AdditionalInstructions = "4.0 cm x 2.0 cm, horizontal signature in black/dark ink on white background.",
                SourceNotification = "Staff Selection Commission Guidelines",
                VerificationDate = "2026",
                IsCustom = false
            },

            // UPSC
            new()
            {
                Id = "upsc-photo",
                Authority = "UPSC",
                ExamName = "UPSC Civil Services / NDA / CDS",
                DocumentType = DocumentType.PassportPhoto,
                RequiredWidthPx = 350,
                RequiredHeightPx = 350,
                MinWidthPx = 350,
                MaxWidthPx = 1000,
                MinHeightPx = 350,
                MaxHeightPx = 1000,
                MinSizeKb = 20,
                MaxSizeKb = 300,
                AllowedFormats = "jpg,jpeg",
                TargetDpi = 300,
                AdditionalInstructions = "Dimensions between 350x350 and 1000x1000 pixels. Must display candidate's name and date taken.",
                SourceNotification = "UPSC Online Application Instructions",
                VerificationDate = "2026",
                IsCustom = false
            },
            new()
            {
                Id = "upsc-signature",
                Authority = "UPSC",
                ExamName = "UPSC Civil Services / NDA / CDS",
                DocumentType = DocumentType.Signature,
                RequiredWidthPx = 350,
                RequiredHeightPx = 350,
                MinWidthPx = 350,
                MaxWidthPx = 1000,
                MinHeightPx = 350,
                MaxHeightPx = 1000,
                MinSizeKb = 20,
                MaxSizeKb = 300,
                AllowedFormats = "jpg,jpeg",
                TargetDpi = 300,
                AdditionalInstructions = "Clear scanned signature between 350x350 and 1000x1000 pixels, 20 KB to 300 KB.",
                SourceNotification = "UPSC Online Application Instructions",
                VerificationDate = "2026",
                IsCustom = false
            },

            // RRB
            new()
            {
                Id = "rrb-photo",
                Authority = "RRB",
                ExamName = "RRB NTPC / Group D / ALP",
                DocumentType = DocumentType.PassportPhoto,
                RequiredWidthPx = 320,
                RequiredHeightPx = 410,
                MinWidthPx = 300,
                MaxWidthPx = 400,
                MinHeightPx = 380,
                MaxHeightPx = 480,
                MinSizeKb = 30,
                MaxSizeKb = 70,
                AllowedFormats = "jpg,jpeg",
                TargetDpi = 100,
                AdditionalInstructions = "35 mm x 45 mm color photo with plain light background.",
                SourceNotification = "Railway Recruitment Board Notifications",
                VerificationDate = "2026",
                IsCustom = false
            },
            new()
            {
                Id = "rrb-signature",
                Authority = "RRB",
                ExamName = "RRB NTPC / Group D / ALP",
                DocumentType = DocumentType.Signature,
                RequiredWidthPx = 200,
                RequiredHeightPx = 80,
                MinWidthPx = 140,
                MaxWidthPx = 300,
                MinHeightPx = 60,
                MaxHeightPx = 120,
                MinSizeKb = 30,
                MaxSizeKb = 70,
                AllowedFormats = "jpg,jpeg",
                TargetDpi = 100,
                AdditionalInstructions = "50 mm x 20 mm running handwriting signature in black ink.",
                SourceNotification = "Railway Recruitment Board Notifications",
                VerificationDate = "2026",
                IsCustom = false
            },

            // NTA
            new()
            {
                Id = "nta-photo",
                Authority = "NTA",
                ExamName = "NTA JEE / NEET / UGC NET",
                DocumentType = DocumentType.PassportPhoto,
                RequiredWidthPx = 200,
                RequiredHeightPx = 250,
                MinWidthPx = 150,
                MaxWidthPx = 500,
                MinHeightPx = 200,
                MaxHeightPx = 600,
                MinSizeKb = 10,
                MaxSizeKb = 200,
                AllowedFormats = "jpg,jpeg",
                TargetDpi = 200,
                AdditionalInstructions = "80% face coverage, white background, clear ears visible.",
                SourceNotification = "National Testing Agency Information Bulletin",
                VerificationDate = "2026",
                IsCustom = false
            },
            new()
            {
                Id = "nta-signature",
                Authority = "NTA",
                ExamName = "NTA JEE / NEET / UGC NET",
                DocumentType = DocumentType.Signature,
                RequiredWidthPx = 140,
                RequiredHeightPx = 60,
                MinWidthPx = 100,
                MaxWidthPx = 400,
                MinHeightPx = 40,
                MaxHeightPx = 200,
                MinSizeKb = 4,
                MaxSizeKb = 30,
                AllowedFormats = "jpg,jpeg",
                TargetDpi = 200,
                AdditionalInstructions = "Signature in black ink on white paper, 4 KB to 30 KB.",
                SourceNotification = "National Testing Agency Information Bulletin",
                VerificationDate = "2026",
                IsCustom = false
            }
        };
    }

    public async Task<IReadOnlyList<ExamPreset>> GetAllPresetsAsync()
    {
        var list = new List<ExamPreset>(_builtinPresets);
        var custom = await LoadCustomPresetsAsync();
        list.AddRange(custom);
        return list;
    }

    public async Task<IReadOnlyList<ExamPreset>> GetPresetsByAuthorityAsync(string authority)
    {
        var all = await GetAllPresetsAsync();
        return all.Where(p => string.Equals(p.Authority, authority, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<IReadOnlyList<string>> GetSupportedAuthoritiesAsync()
    {
        var all = await GetAllPresetsAsync();
        return all.Select(p => p.Authority).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(a => a).ToList();
    }

    public async Task<ComplianceResult> ValidateComplianceAsync(string filePath, ExamPreset preset)
    {
        if (!File.Exists(filePath))
        {
            var missing = new ComplianceResult();
            missing.Issues.Add(new ComplianceIssue
            {
                Severity = ComplianceSeverity.Error,
                Property = "File",
                Message = "File does not exist."
            });
            return missing;
        }

        var fileInfo = new FileInfo(filePath);
        var ext = fileInfo.Extension.TrimStart('.').ToLowerInvariant();
        var sizeBytes = fileInfo.Length;
        var sizeKb = sizeBytes / 1024.0;

        var result = new ComplianceResult
        {
            ActualSizeBytes = sizeBytes,
            ActualFormat = ext
        };

        // Format check
        var allowedList = preset.AllowedFormats.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(f => f.TrimStart('.').ToLowerInvariant());

        if (!allowedList.Contains(ext))
        {
            result.Issues.Add(new ComplianceIssue
            {
                Severity = ComplianceSeverity.Error,
                Property = "Format",
                Message = $"Format is '{ext}', but allowed formats are: {preset.AllowedFormats}."
            });
        }

        // File size check
        if (sizeKb > preset.MaxSizeKb)
        {
            result.Issues.Add(new ComplianceIssue
            {
                Severity = ComplianceSeverity.Error,
                Property = "FileSize",
                Message = $"File size is {sizeKb:F1} KB, which exceeds maximum limit of {preset.MaxSizeKb} KB."
            });
        }
        else if (preset.MinSizeKb > 0 && sizeKb < preset.MinSizeKb)
        {
            result.Issues.Add(new ComplianceIssue
            {
                Severity = ComplianceSeverity.Error,
                Property = "FileSize",
                Message = $"File size is {sizeKb:F1} KB, which is below minimum required {preset.MinSizeKb} KB."
            });
        }

        await Task.CompletedTask;
        return result;
    }

    public async Task SaveCustomPresetAsync(ExamPreset preset)
    {
        preset.IsCustom = true;
        var custom = await LoadCustomPresetsAsync();
        var existingIndex = custom.FindIndex(p => p.Id == preset.Id);
        if (existingIndex >= 0)
        {
            custom[existingIndex] = preset;
        }
        else
        {
            custom.Add(preset);
        }

        var json = JsonSerializer.Serialize(custom, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_customPresetsPath, json);
    }

    public async Task DeleteCustomPresetAsync(string presetId)
    {
        var custom = await LoadCustomPresetsAsync();
        custom.RemoveAll(p => p.Id == presetId);
        var json = JsonSerializer.Serialize(custom, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_customPresetsPath, json);
    }

    private async Task<List<ExamPreset>> LoadCustomPresetsAsync()
    {
        if (!File.Exists(_customPresetsPath))
            return new List<ExamPreset>();

        try
        {
            var json = await File.ReadAllTextAsync(_customPresetsPath);
            var list = JsonSerializer.Deserialize<List<ExamPreset>>(json);
            return list ?? new List<ExamPreset>();
        }
        catch
        {
            return new List<ExamPreset>();
        }
    }
}
