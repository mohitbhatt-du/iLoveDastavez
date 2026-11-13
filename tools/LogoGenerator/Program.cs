using System;
using System.IO;
using SkiaSharp;

namespace LogoGenerator;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Generating Dastavez Studio / iLoveDastavez Brand Assets...");

        var assetsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/DastavezStudio.App/Assets"));
        if (!Directory.Exists(assetsDir))
        {
            Directory.CreateDirectory(assetsDir);
        }

        // 1. Render 512x512 Master Logo
        using var master512 = RenderLogo(512);
        var logo512Path = Path.Combine(assetsDir, "AppLogo.png");
        SaveBitmapAsPng(master512, logo512Path);
        Console.WriteLine($"Saved: {logo512Path}");

        // 2. Render 64x64 Small Logo
        using var master64 = RenderLogo(64);
        var logo64Path = Path.Combine(assetsDir, "AppLogo_Small.png");
        SaveBitmapAsPng(master64, logo64Path);
        Console.WriteLine($"Saved: {logo64Path}");

        // 3. Render 128x128 Medium Logo
        using var master128 = RenderLogo(128);
        var logo128Path = Path.Combine(assetsDir, "AppLogo_128.png");
        SaveBitmapAsPng(master128, logo128Path);

        // 4. Render Multi-Resolution ICO (16, 32, 48, 64, 128, 256)
        int[] icoSizes = { 16, 32, 48, 64, 128, 256 };
        var pngStreams = new byte[icoSizes.Length][];
        for (int i = 0; i < icoSizes.Length; i++)
        {
            using var bmp = RenderLogo(icoSizes[i]);
            using var img = SKImage.FromBitmap(bmp);
            using var data = img.Encode(SKEncodedImageFormat.Png, 100);
            pngStreams[i] = data.ToArray();
        }

        var icoPath = Path.Combine(assetsDir, "AppIcon.ico");
        SaveMultiResIco(icoSizes, pngStreams, icoPath);
        Console.WriteLine($"Saved: {icoPath}");

        // 5. Render Horizontal Brand Banner (Logo + "iLoveDastavez" + "Dastavez Studio" + "हर दस्तावेज़, तैयार!")
        var logosDir = Path.Combine(assetsDir, "Logos");
        if (!Directory.Exists(logosDir)) Directory.CreateDirectory(logosDir);
        using var banner = RenderBrandBanner(600, 160);
        var bannerPath = Path.Combine(logosDir, "brand_banner.png");
        SaveBitmapAsPng(banner, bannerPath);
        Console.WriteLine($"Saved: {bannerPath}");

        Console.WriteLine("Brand assets generation complete!");
    }

    static SKBitmap RenderLogo(int size)
    {
        var bitmap = new SKBitmap(size, size, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        float s = size / 512f;

        // Background squircle with rich dark navy gradient
        var bgRect = new SKRoundRect(new SKRect(16 * s, 16 * s, 496 * s, 496 * s), 96 * s, 96 * s);
        using (var bgShader = SKShader.CreateLinearGradient(
            new SKPoint(256 * s, 16 * s),
            new SKPoint(256 * s, 496 * s),
            new[] { new SKColor(0x1B, 0x2A, 0x4A), new SKColor(0x0A, 0x11, 0x28) },
            null,
            SKShaderTileMode.Clamp))
        using (var bgPaint = new SKPaint { Shader = bgShader, IsAntialias = true })
        {
            canvas.DrawRoundRect(bgRect, bgPaint);
        }

        // Subtle glowing border around squircle
        using (var borderPaint = new SKPaint
        {
            Color = new SKColor(0x38, 0x55, 0x8A, 0x99),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 4 * s,
            IsAntialias = true
        })
        {
            canvas.DrawRoundRect(bgRect, borderPaint);
        }

        // Document Sheet geometry (Left: 110, Top: 80, Right: 402, Bottom: 432)
        // Fold at top right: corner from (320, 80) to (402, 162)
        float docL = 110 * s;
        float docT = 80 * s;
        float docR = 402 * s;
        float docB = 432 * s;
        float foldSize = 82 * s;
        float docRadius = 24 * s;

        using (var docPath = new SKPath())
        {
            docPath.MoveTo(docL + docRadius, docT);
            docPath.LineTo(docR - foldSize, docT);
            docPath.LineTo(docR, docT + foldSize);
            docPath.LineTo(docR, docB - docRadius);
            docPath.ArcTo(new SKRect(docR - 2 * docRadius, docB - 2 * docRadius, docR, docB), 0, 90, false);
            docPath.LineTo(docL + docRadius, docB);
            docPath.ArcTo(new SKRect(docL, docB - 2 * docRadius, docL + 2 * docRadius, docB), 90, 90, false);
            docPath.LineTo(docL, docT + docRadius);
            docPath.ArcTo(new SKRect(docL, docT, docL + 2 * docRadius, docT + 2 * docRadius), 180, 90, false);
            docPath.Close();

            // Document body shadow
            using (var shadowPaint = new SKPaint
            {
                Color = new SKColor(0x00, 0x00, 0x00, 0x66),
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 14 * s),
                IsAntialias = true
            })
            {
                canvas.DrawPath(docPath, shadowPaint);
            }

            // Document body fill (Crisp white / pearl)
            using (var docShader = SKShader.CreateLinearGradient(
                new SKPoint(docL, docT),
                new SKPoint(docR, docB),
                new[] { new SKColor(0xF9, 0xFB, 0xFD), new SKColor(0xEA, 0xEE, 0xF5) },
                null,
                SKShaderTileMode.Clamp))
            using (var docPaint = new SKPaint { Shader = docShader, IsAntialias = true })
            {
                canvas.DrawPath(docPath, docPaint);
            }

            // Document border
            using (var docStroke = new SKPaint
            {
                Color = new SKColor(0xD0, 0xD8, 0xE5),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3 * s,
                IsAntialias = true
            })
            {
                canvas.DrawPath(docPath, docStroke);
            }
        }

        // Folded Corner Triangle
        using (var foldPath = new SKPath())
        {
            foldPath.MoveTo(docR - foldSize, docT);
            foldPath.LineTo(docR - foldSize, docT + foldSize);
            foldPath.LineTo(docR, docT + foldSize);
            foldPath.Close();

            using (var foldPaint = new SKPaint
            {
                Color = new SKColor(0xD2, 0xDC, 0xEA),
                IsAntialias = true
            })
            {
                canvas.DrawPath(foldPath, foldPaint);
            }

            using (var foldStroke = new SKPaint
            {
                Color = new SKColor(0xB5, 0xC4, 0xD8),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2 * s,
                IsAntialias = true
            })
            {
                canvas.DrawPath(foldPath, foldStroke);
            }
        }

        // Indian Saffron Header Bar across top of document
        using (var saffronRect = new SKRoundRect(new SKRect(132 * s, 102 * s, (docR - foldSize - 12 * s), 130 * s), 6 * s, 6 * s))
        using (var saffronPaint = new SKPaint
        {
            Color = new SKColor(0xFF, 0x88, 0x00), // Vibrant Saffron
            IsAntialias = true
        })
        {
            canvas.DrawRoundRect(saffronRect, saffronPaint);
        }

        // Shield Center Emblem with Golden / Saffron Outline
        float cx = 256 * s;
        float cy = 250 * s;
        float sw = 68 * s;
        float sh = 82 * s;

        using (var shieldPath = new SKPath())
        {
            shieldPath.MoveTo(cx - sw, cy - sh * 0.7f);
            shieldPath.LineTo(cx + sw, cy - sh * 0.7f);
            shieldPath.LineTo(cx + sw, cy + sh * 0.1f);
            shieldPath.QuadTo(cx + sw, cy + sh * 0.8f, cx, cy + sh);
            shieldPath.QuadTo(cx - sw, cy + sh * 0.8f, cx - sw, cy + sh * 0.1f);
            shieldPath.Close();

            // Shield fill gradient (Vibrant Orange to Rich Saffron)
            using (var shieldShader = SKShader.CreateLinearGradient(
                new SKPoint(cx, cy - sh),
                new SKPoint(cx, cy + sh),
                new[] { new SKColor(0xFF, 0x99, 0x33), new SKColor(0xE6, 0x5C, 0x00) },
                null,
                SKShaderTileMode.Clamp))
            using (var shieldPaint = new SKPaint { Shader = shieldShader, IsAntialias = true })
            {
                canvas.DrawPath(shieldPath, shieldPaint);
            }

            // Shield stroke
            using (var shieldStroke = new SKPaint
            {
                Color = new SKColor(0xFF, 0xD5, 0x80),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 4 * s,
                IsAntialias = true
            })
            {
                canvas.DrawPath(shieldPath, shieldStroke);
            }

            // White Bold Checkmark inside Shield
            using (var checkPath = new SKPath())
            {
                checkPath.MoveTo(cx - 30 * s, cy + 2 * s);
                checkPath.LineTo(cx - 10 * s, cy + 24 * s);
                checkPath.LineTo(cx + 32 * s, cy - 22 * s);

                using (var checkPaint = new SKPaint
                {
                    Color = SKColors.White,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 10 * s,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeJoin = SKStrokeJoin.Round,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(checkPath, checkPaint);
                }
            }
        }

        // Horizontal Document Content Guide Lines
        float[] lineTops = { 348 * s, 372 * s };
        float[] lineWidths = { 210 * s, 160 * s };
        for (int i = 0; i < lineTops.Length; i++)
        {
            using var linePaint = new SKPaint
            {
                Color = new SKColor(0xCB, 0xD5, 0xE1),
                StrokeWidth = 6 * s,
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true
            };
            canvas.DrawLine(146 * s, lineTops[i], 146 * s + lineWidths[i], lineTops[i], linePaint);
        }

        // Emerald Green Offline Security Badge at bottom right of document
        float badgeX = 360 * s;
        float badgeY = 385 * s;
        float badgeR = 26 * s;

        using (var badgeShader = SKShader.CreateLinearGradient(
            new SKPoint(badgeX, badgeY - badgeR),
            new SKPoint(badgeX, badgeY + badgeR),
            new[] { new SKColor(0x13, 0x88, 0x08), new SKColor(0x0C, 0x58, 0x05) },
            null,
            SKShaderTileMode.Clamp))
        using (var badgePaint = new SKPaint { Shader = badgeShader, IsAntialias = true })
        {
            canvas.DrawCircle(badgeX, badgeY, badgeR, badgePaint);
        }

        // White Lock / Star icon inside Badge
        using (var badgeBorder = new SKPaint
        {
            Color = SKColors.White,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3 * s,
            IsAntialias = true
        })
        {
            canvas.DrawCircle(badgeX, badgeY, badgeR, badgeBorder);
        }

        // Padlock body inside badge
        using (var lockBody = new SKPaint { Color = SKColors.White, IsAntialias = true })
        {
            canvas.DrawRoundRect(new SKRect(badgeX - 10 * s, badgeY - 3 * s, badgeX + 10 * s, badgeY + 11 * s), 3 * s, 3 * s, lockBody);
        }
        using (var lockArch = new SKPaint
        {
            Color = SKColors.White,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3 * s,
            IsAntialias = true
        })
        {
            canvas.DrawArc(new SKRect(badgeX - 7 * s, badgeY - 14 * s, badgeX + 7 * s, badgeY), 180, 180, false, lockArch);
        }

        return bitmap;
    }

    static SKBitmap RenderBrandBanner(int width, int height)
    {
        var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        // Draw small logo on left
        using var logo = RenderLogo(120);
        canvas.DrawBitmap(logo, 20, 20);

        // Draw Typography
        using var fontTitle = new SKFont(SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold), 38);
        using var paintAccent = new SKPaint(fontTitle) { Color = new SKColor(0xFF, 0x88, 0x00), IsAntialias = true };
        using var paintMain = new SKPaint(fontTitle) { Color = SKColors.White, IsAntialias = true };

        canvas.DrawText("iLove", 160, 68, paintAccent);
        canvas.DrawText("Dastavez", 255, 68, paintMain);

        using var fontSub = new SKFont(SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Normal), 20);
        using var paintSub = new SKPaint(fontSub) { Color = new SKColor(0x94, 0xA3, 0xB8), IsAntialias = true };
        canvas.DrawText("Dastavez Studio", 160, 102, paintSub);

        using var fontTagline = new SKFont(SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold), 17);
        using var paintTagline = new SKPaint(fontTagline) { Color = new SKColor(0x38, 0xBD, 0xF8), IsAntialias = true };
        canvas.DrawText("हर दस्तावेज़, तैयार! — 100% Offline", 160, 134, paintTagline);

        return bitmap;
    }

    static void SaveBitmapAsPng(SKBitmap bitmap, string path)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(path);
        data.SaveTo(stream);
    }

    static void SaveMultiResIco(int[] sizes, byte[][] pngData, string outputPath)
    {
        using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(stream);

        // ICONDIR Header
        writer.Write((ushort)0); // Reserved
        writer.Write((ushort)1); // Type = 1 (Icon)
        writer.Write((ushort)sizes.Length); // Image count

        int currentOffset = 6 + (16 * sizes.Length);

        // ICONDIRENTRY entries
        for (int i = 0; i < sizes.Length; i++)
        {
            int s = sizes[i];
            writer.Write((byte)(s >= 256 ? 0 : s)); // Width
            writer.Write((byte)(s >= 256 ? 0 : s)); // Height
            writer.Write((byte)0);                   // Color count
            writer.Write((byte)0);                   // Reserved
            writer.Write((ushort)1);                 // Color planes
            writer.Write((ushort)32);                // Bits per pixel
            writer.Write((uint)pngData[i].Length);   // Bytes in resource
            writer.Write((uint)currentOffset);       // Image offset

            currentOffset += pngData[i].Length;
        }

        // Image data blocks
        for (int i = 0; i < sizes.Length; i++)
        {
            writer.Write(pngData[i]);
        }
    }
}
