using SkiaSharp;

namespace DastavezStudio.ImageEngine;

public static class LogoGenerator
{
    public static void GenerateDefaultLogos(string targetDirectory)
    {
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        var logos = new[]
        {
            new { Name = "ibps.png", Text = "IBPS", Color = new SKColor(24, 76, 120), Sub = "BANKING" },
            new { Name = "sbi.png", Text = "SBI", Color = new SKColor(10, 114, 186), Sub = "RECRUIT" },
            new { Name = "ssc.png", Text = "SSC", Color = new SKColor(168, 50, 45), Sub = "COMMISSION" },
            new { Name = "upsc.png", Text = "UPSC", Color = new SKColor(30, 41, 59), Sub = "CIVIL SERVICES" },
            new { Name = "rrb.png", Text = "RRB", Color = new SKColor(139, 26, 26), Sub = "RAILWAYS" },
            new { Name = "nta.png", Text = "NTA", Color = new SKColor(22, 101, 52), Sub = "TESTING" },
            new { Name = "psc.png", Text = "PSC", Color = new SKColor(88, 28, 135), Sub = "STATE EXAMS" }
        };

        const int width = 120;
        const int height = 48;

        using var boldTypeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright) ?? SKTypeface.Default;
        using var normalTypeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright) ?? SKTypeface.Default;

        using var titleFont = new SKFont(boldTypeface, 16);
        using var subFont = new SKFont(normalTypeface, 9);

        foreach (var item in logos)
        {
            var filePath = Path.Combine(targetDirectory, item.Name);
            if (File.Exists(filePath)) continue;

            using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            // Draw rounded pill background
            using var bgPaint = new SKPaint
            {
                Color = item.Color,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRoundRect(new SKRoundRect(new SKRect(2, 2, width - 2, height - 2), 6, 6), bgPaint);

            // Draw title text
            using var textPaint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true
            };
            canvas.DrawText(item.Text, width / 2.0f, 24, SKTextAlign.Center, titleFont, textPaint);

            // Draw sub text
            using var subPaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, 200),
                IsAntialias = true
            };
            canvas.DrawText(item.Sub, width / 2.0f, 38, SKTextAlign.Center, subFont, subPaint);

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.Open(filePath, FileMode.Create, FileAccess.Write);
            data.SaveTo(stream);
        }
    }
}
