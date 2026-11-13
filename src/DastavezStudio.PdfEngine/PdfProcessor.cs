using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf.Security;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;

namespace DastavezStudio.PdfEngine;

public class PdfProcessor : IPdfProcessor
{
    public async Task<IReadOnlyList<PdfPageInfo>> GetPdfInfoAsync(string pdfPath, string? password = null)
    {
        return await Task.Run(() =>
        {
            if (!File.Exists(pdfPath))
                throw new FileNotFoundException("PDF file not found.", pdfPath);

            using var doc = string.IsNullOrEmpty(password) 
                ? PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import)
                : PdfReader.Open(pdfPath, password, PdfDocumentOpenMode.Import);

            var list = new List<PdfPageInfo>();
            for (int i = 0; i < doc.PageCount; i++)
            {
                var page = doc.Pages[i];
                list.Add(new PdfPageInfo
                {
                    PageNumber = i + 1,
                    WidthPt = page.Width.Point,
                    HeightPt = page.Height.Point,
                    RotationDegrees = page.Rotate
                });
            }

            return (IReadOnlyList<PdfPageInfo>)list;
        });
    }

    public async Task<string> CompressPdfAsync(string inputPdfPath, string outputPdfPath, PdfCompressionOptions options, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            using var doc = PdfReader.Open(inputPdfPath, PdfDocumentOpenMode.Modify);

            // Configure stream compression
            doc.Options.UseFlateDecoderForJpegImages = PdfUseFlateDecoderForJpegImages.Automatic;
            doc.Options.CompressContentStreams = true;

            var dir = Path.GetDirectoryName(outputPdfPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            doc.Save(outputPdfPath);
            progress?.Report(100);

            return outputPdfPath;
        }, ct);
    }

    public async Task<string> MergePdfsAsync(IReadOnlyList<string> inputPdfPaths, string outputPdfPath, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            using var outputDoc = new PdfDocument();

            foreach (var inputPath in inputPdfPaths)
            {
                ct.ThrowIfCancellationRequested();
                using var inputDoc = PdfReader.Open(inputPath, PdfDocumentOpenMode.Import);
                int count = inputDoc.PageCount;
                for (int idx = 0; idx < count; idx++)
                {
                    outputDoc.AddPage(inputDoc.Pages[idx]);
                }
            }

            var dir = Path.GetDirectoryName(outputPdfPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            outputDoc.Save(outputPdfPath);
            return outputPdfPath;
        }, ct);
    }

    public async Task<IReadOnlyList<string>> SplitPdfAsync(string inputPdfPath, string outputFolder, int? splitEveryNPages = null, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            using var inputDoc = PdfReader.Open(inputPdfPath, PdfDocumentOpenMode.Import);
            int step = splitEveryNPages.GetValueOrDefault(1);
            if (step < 1) step = 1;

            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            var createdFiles = new List<string>();
            var baseName = Path.GetFileNameWithoutExtension(inputPdfPath);

            int fileIndex = 1;
            for (int i = 0; i < inputDoc.PageCount; i += step)
            {
                ct.ThrowIfCancellationRequested();
                using var partDoc = new PdfDocument();
                int upper = Math.Min(i + step, inputDoc.PageCount);
                for (int pageIdx = i; pageIdx < upper; pageIdx++)
                {
                    partDoc.AddPage(inputDoc.Pages[pageIdx]);
                }

                var outPath = Path.Combine(outputFolder, $"{baseName}_part_{fileIndex:D3}.pdf");
                partDoc.Save(outPath);
                createdFiles.Add(outPath);
                fileIndex++;
            }

            return (IReadOnlyList<string>)createdFiles;
        }, ct);
    }

    public async Task<string> ExtractPagesAsync(string inputPdfPath, string outputPdfPath, IReadOnlyList<int> pageNumbers, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            using var inputDoc = PdfReader.Open(inputPdfPath, PdfDocumentOpenMode.Import);
            using var outputDoc = new PdfDocument();

            foreach (var pageNum in pageNumbers)
            {
                int index = pageNum - 1;
                if (index >= 0 && index < inputDoc.PageCount)
                {
                    outputDoc.AddPage(inputDoc.Pages[index]);
                }
            }

            var dir = Path.GetDirectoryName(outputPdfPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            outputDoc.Save(outputPdfPath);
            return outputPdfPath;
        }, ct);
    }

    public async Task<string> RotatePagesAsync(string inputPdfPath, string outputPdfPath, IReadOnlyList<int> pageNumbers, int rotationDegrees, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            using var doc = PdfReader.Open(inputPdfPath, PdfDocumentOpenMode.Modify);

            foreach (var pageNum in pageNumbers)
            {
                int index = pageNum - 1;
                if (index >= 0 && index < doc.PageCount)
                {
                    var page = doc.Pages[index];
                    page.Rotate = (page.Rotate + rotationDegrees) % 360;
                }
            }

            var dir = Path.GetDirectoryName(outputPdfPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            doc.Save(outputPdfPath);
            return outputPdfPath;
        }, ct);
    }

    public async Task<string> ConvertImagesToPdfAsync(IReadOnlyList<string> imagePaths, string outputPdfPath, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            using var doc = new PdfDocument();

            foreach (var imgPath in imagePaths)
            {
                ct.ThrowIfCancellationRequested();
                if (!File.Exists(imgPath)) continue;

                using var xImage = XImage.FromFile(imgPath);
                var page = doc.AddPage();
                page.Width = XUnit.FromPoint(xImage.PointWidth);
                page.Height = XUnit.FromPoint(xImage.PointHeight);

                using var gfx = XGraphics.FromPdfPage(page);
                gfx.DrawImage(xImage, 0, 0, page.Width.Point, page.Height.Point);
            }

            var dir = Path.GetDirectoryName(outputPdfPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            doc.Save(outputPdfPath);
            return outputPdfPath;
        }, ct);
    }

    public async Task<IReadOnlyList<string>> ConvertPdfToImagesAsync(string inputPdfPath, string outputFolder, ImageFormatType format, int dpi = 200, CancellationToken ct = default)
    {
        // PDFium or render-based extraction
        return await Task.FromResult<IReadOnlyList<string>>(new List<string>());
    }

    public async Task<string> SetPasswordAsync(string inputPdfPath, string outputPdfPath, string userPassword, string ownerPassword, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            using var doc = PdfReader.Open(inputPdfPath, PdfDocumentOpenMode.Modify);
            var sec = doc.SecuritySettings;
            sec.UserPassword = userPassword;
            sec.OwnerPassword = ownerPassword;

            var dir = Path.GetDirectoryName(outputPdfPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            doc.Save(outputPdfPath);
            return outputPdfPath;
        }, ct);
    }

    public async Task<string> RemovePasswordAsync(string inputPdfPath, string outputPdfPath, string password, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            using var inputDoc = PdfReader.Open(inputPdfPath, password, PdfDocumentOpenMode.Import);
            using var outputDoc = new PdfDocument();

            for (int i = 0; i < inputDoc.PageCount; i++)
            {
                outputDoc.AddPage(inputDoc.Pages[i]);
            }

            var dir = Path.GetDirectoryName(outputPdfPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            outputDoc.Save(outputPdfPath);
            return outputPdfPath;
        }, ct);
    }
}
