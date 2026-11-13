using DastavezStudio.Core.Models;
using DastavezStudio.ExamPresets;
using Xunit;

namespace DastavezStudio.UnitTests;

public class ExamPresetServiceTests
{
    [Fact]
    public async Task GetAllPresetsAsync_ReturnsBuiltinPresets()
    {
        var service = new ExamPresetService();
        var presets = await service.GetAllPresetsAsync();

        Assert.NotNull(presets);
        Assert.NotEmpty(presets);
        Assert.Contains(presets, p => p.Authority == "IBPS");
        Assert.Contains(presets, p => p.Authority == "SBI");
        Assert.Contains(presets, p => p.Authority == "SSC");
        Assert.Contains(presets, p => p.Authority == "UPSC");
        Assert.Contains(presets, p => p.Authority == "RRB");
        Assert.Contains(presets, p => p.Authority == "NTA");
    }

    [Fact]
    public async Task GetPresetsByAuthority_FiltersCorrectly()
    {
        var service = new ExamPresetService();
        var ibpsPresets = await service.GetPresetsByAuthorityAsync("IBPS");

        Assert.NotEmpty(ibpsPresets);
        Assert.All(ibpsPresets, p => Assert.Equal("IBPS", p.Authority));
    }

    [Fact]
    public async Task ValidateCompliance_ChecksFileSizeLimits()
    {
        var service = new ExamPresetService();
        var tempFile = Path.GetTempFileName();
        try
        {
            // Write 100 KB file
            var data = new byte[100 * 1024];
            await File.WriteAllBytesAsync(tempFile, data);

            var preset = new ExamPreset
            {
                Id = "test",
                MinSizeKb = 20,
                MaxSizeKb = 50,
                AllowedFormats = "tmp"
            };

            var result = await service.ValidateComplianceAsync(tempFile, preset);
            Assert.False(result.IsCompliant);
            Assert.Contains(result.Issues, i => i.Property == "FileSize");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
