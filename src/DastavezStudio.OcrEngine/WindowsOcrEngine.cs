using System.Diagnostics;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using WinOcr = Windows.Media.Ocr;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;

namespace DastavezStudio.OcrEngine;

public class WindowsOcrEngine : IOcrEngine
{
    public async Task<IReadOnlyList<string>> GetAvailableLanguagesAsync()
    {
        return await Task.Run(() =>
        {
            var langs = WinOcr.OcrEngine.AvailableRecognizerLanguages;
            return langs.Select(l => l.LanguageTag).ToList();
        });
    }

    public async Task<OcrResult> ExtractTextAsync(string imagePath, string language = "en-US", CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        ct.ThrowIfCancellationRequested();

        if (!File.Exists(imagePath))
            throw new FileNotFoundException("Image file not found for OCR.", imagePath);

        // Language resolution
        var lang = new Language(language);
        if (!WinOcr.OcrEngine.IsLanguageSupported(lang))
        {
            var available = WinOcr.OcrEngine.AvailableRecognizerLanguages;
            lang = available.FirstOrDefault() ?? new Language("en-US");
        }

        var engine = WinOcr.OcrEngine.TryCreateFromLanguage(lang);
        if (engine == null)
        {
            return new OcrResult
            {
                FullText = string.Empty,
                Lines = Array.Empty<OcrLine>(),
                Duration = sw.Elapsed,
                Language = language
            };
        }

        using var fileStream = File.OpenRead(imagePath);
        using var randomStream = fileStream.AsRandomAccessStream();
        var decoder = await BitmapDecoder.CreateAsync(randomStream);
        using var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

        var winOcrResult = await engine.RecognizeAsync(softwareBitmap);
        sw.Stop();

        var lines = new List<OcrLine>();
        var fullTextBuilder = new System.Text.StringBuilder();

        foreach (var line in winOcrResult.Lines)
        {
            ct.ThrowIfCancellationRequested();
            var words = line.Words.Select(w => new OcrWord
            {
                Text = w.Text,
                X = w.BoundingRect.X,
                Y = w.BoundingRect.Y,
                Width = w.BoundingRect.Width,
                Height = w.BoundingRect.Height,
                Confidence = 1.0
            }).ToList();

            lines.Add(new OcrLine
            {
                Text = line.Text,
                Words = words
            });

            fullTextBuilder.AppendLine(line.Text);
        }

        return new OcrResult
        {
            FullText = fullTextBuilder.ToString().TrimEnd(),
            Lines = lines,
            TextAngleDegrees = winOcrResult.TextAngle,
            Duration = sw.Elapsed,
            Language = lang.LanguageTag
        };
    }

    public async Task<string> CreateSearchablePdfAsync(
        string imagePath,
        string outputPdfPath,
        string language = "en-US",
        IProgress<double>? progress = null,
        CancellationToken ct = default)
    {
        return await Task.Run(async () =>
        {
            progress?.Report(10.0);
            ct.ThrowIfCancellationRequested();

            var ocr = await ExtractTextAsync(imagePath, language, ct);
            progress?.Report(50.0);

            using var doc = new PdfDocument();
            var page = doc.AddPage();

            using var xImage = XImage.FromFile(imagePath);
            page.Width = XUnit.FromPoint(xImage.PointWidth);
            page.Height = XUnit.FromPoint(xImage.PointHeight);

            using var gfx = XGraphics.FromPdfPage(page);

            // Step 1: Draw visual document background
            gfx.DrawImage(xImage, 0, 0, page.Width, page.Height);

            progress?.Report(75.0);
            ct.ThrowIfCancellationRequested();

            // Step 2: Overlay selectable invisible text layer
            double scaleX = page.Width.Point / xImage.PixelWidth;
            double scaleY = page.Height.Point / xImage.PixelHeight;

            var invisibleBrush = new XSolidBrush(XColor.FromArgb(0, 0, 0, 0));

            foreach (var line in ocr.Lines)
            {
                foreach (var word in line.Words)
                {
                    double ptX = word.X * scaleX;
                    double ptY = (word.Y + word.Height) * scaleY;
                    double ptFontSize = Math.Max(6, word.Height * scaleY * 0.85);

                    var font = new XFont("Arial", ptFontSize);
                    gfx.DrawString(word.Text, font, invisibleBrush, new XPoint(ptX, ptY));
                }
            }

            // Ensure destination directory exists
            var dir = Path.GetDirectoryName(outputPdfPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            doc.Save(outputPdfPath);
            progress?.Report(100.0);

            return outputPdfPath;
        }, ct);
    }
}
