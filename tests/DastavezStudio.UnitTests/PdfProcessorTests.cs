using PdfSharpCore.Pdf;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.PdfEngine;
using Xunit;

namespace DastavezStudio.UnitTests;

public class PdfProcessorTests
{
    private string CreateSamplePdf(int pageCount, string filenamePrefix = "test_pdf")
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{filenamePrefix}_{Guid.NewGuid():N}.pdf");
        using var doc = new PdfDocument();
        for (int i = 0; i < pageCount; i++)
        {
            doc.AddPage();
        }
        doc.Save(tempFile);
        return tempFile;
    }

    [Fact]
    public async Task GetPdfInfoAsync_ReturnsCorrectPageCount()
    {
        var processor = new PdfProcessor();
        var tempPdf = CreateSamplePdf(3);

        try
        {
            var info = await processor.GetPdfInfoAsync(tempPdf);
            Assert.Equal(3, info.Count);
            Assert.Equal(1, info[0].PageNumber);
            Assert.Equal(3, info[2].PageNumber);
        }
        finally
        {
            if (File.Exists(tempPdf)) File.Delete(tempPdf);
        }
    }

    [Fact]
    public async Task MergePdfsAsync_CombinesPagesAccurately()
    {
        var processor = new PdfProcessor();
        var pdf1 = CreateSamplePdf(2, "merge_1");
        var pdf2 = CreateSamplePdf(3, "merge_2");
        var outputPdf = Path.Combine(Path.GetTempPath(), $"merged_{Guid.NewGuid():N}.pdf");

        try
        {
            await processor.MergePdfsAsync(new[] { pdf1, pdf2 }, outputPdf);

            var info = await processor.GetPdfInfoAsync(outputPdf);
            Assert.Equal(5, info.Count);
        }
        finally
        {
            if (File.Exists(pdf1)) File.Delete(pdf1);
            if (File.Exists(pdf2)) File.Delete(pdf2);
            if (File.Exists(outputPdf)) File.Delete(outputPdf);
        }
    }

    [Fact]
    public async Task SplitPdfAsync_ProducesIndividualFiles()
    {
        var processor = new PdfProcessor();
        var pdf = CreateSamplePdf(4, "split");
        var outputFolder = Path.Combine(Path.GetTempPath(), $"split_out_{Guid.NewGuid():N}");

        try
        {
            var parts = await processor.SplitPdfAsync(pdf, outputFolder, splitEveryNPages: 1);
            Assert.Equal(4, parts.Count);
            foreach (var part in parts)
            {
                Assert.True(File.Exists(part));
                var partInfo = await processor.GetPdfInfoAsync(part);
                Assert.Equal(1, partInfo.Count);
            }
        }
        finally
        {
            if (File.Exists(pdf)) File.Delete(pdf);
            if (Directory.Exists(outputFolder)) Directory.Delete(outputFolder, recursive: true);
        }
    }

    [Fact]
    public async Task ExtractPagesAsync_ExtractsRequestedPages()
    {
        var processor = new PdfProcessor();
        var pdf = CreateSamplePdf(5, "extract");
        var outputPdf = Path.Combine(Path.GetTempPath(), $"extracted_{Guid.NewGuid():N}.pdf");

        try
        {
            await processor.ExtractPagesAsync(pdf, outputPdf, new[] { 2, 4 });
            var info = await processor.GetPdfInfoAsync(outputPdf);
            Assert.Equal(2, info.Count);
        }
        finally
        {
            if (File.Exists(pdf)) File.Delete(pdf);
            if (File.Exists(outputPdf)) File.Delete(outputPdf);
        }
    }

    [Fact]
    public async Task PasswordProtection_EncryptsAndDecrypts()
    {
        var processor = new PdfProcessor();
        var pdf = CreateSamplePdf(2, "secure");
        var securedPdf = Path.Combine(Path.GetTempPath(), $"locked_{Guid.NewGuid():N}.pdf");
        var unlockedPdf = Path.Combine(Path.GetTempPath(), $"unlocked_{Guid.NewGuid():N}.pdf");

        try
        {
            // Protect with user password "secret123"
            await processor.SetPasswordAsync(pdf, securedPdf, "secret123", "owner123");
            Assert.True(File.Exists(securedPdf));

            // Verify opening with password works
            var info = await processor.GetPdfInfoAsync(securedPdf, "secret123");
            Assert.Equal(2, info.Count);

            // Remove password
            await processor.RemovePasswordAsync(securedPdf, unlockedPdf, "secret123");
            var unlockedInfo = await processor.GetPdfInfoAsync(unlockedPdf);
            Assert.Equal(2, unlockedInfo.Count);
        }
        finally
        {
            if (File.Exists(pdf)) File.Delete(pdf);
            if (File.Exists(securedPdf)) File.Delete(securedPdf);
            if (File.Exists(unlockedPdf)) File.Delete(unlockedPdf);
        }
    }
}
