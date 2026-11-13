using System;
using System.IO;
using SkiaSharp;

namespace LogoGenerator;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Rendering New Ultra-Aesthetic Dastavez Studio Brand Suite...");

        var assetsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/DastavezStudio.App/Assets"));
        if (!Directory.Exists(assetsDir)) Directory.CreateDirectory(assetsDir);

        // 1. Render 512x512 Master Aesthetic Logo
        using var master512 = RenderAestheticLogo(512);
        var logo512Path = Path.Combine(assetsDir, "AppLogo.png");
        SaveBitmapAsPng(master512, logo512Path);
        Console.WriteLine($"Saved: {logo512Path}");

        // 2. Render 128x128
        using var master128 = RenderAestheticLogo(128);
        var logo128Path = Path.Combine(assetsDir, "AppLogo_128.png");
        SaveBitmapAsPng(master128, logo128Path);

        // 3. Render 64x64
        using var master64 = RenderAestheticLogo(64);
        var logo64Path = Path.Combine(assetsDir, "AppLogo_Small.png");
        SaveBitmapAsPng(master64, logo64Path);

        // 4. Multi-Resolution ICO (16, 32, 48, 64, 128, 256)
        int[] icoSizes = { 16, 32, 48, 64, 128, 256 };
        var pngStreams = new byte[icoSizes.Length][];
        for (int i = 0; i < icoSizes.Length; i++)
        {
            using var bmp = RenderAestheticLogo(icoSizes[i]);
            using var img = SKImage.FromBitmap(bmp);
            using var data = img.Encode(SKEncodedImageFormat.Png, 100);
            pngStreams[i] = data.ToArray();
        }

        var icoPath = Path.Combine(assetsDir, "AppIcon.ico");
        SaveMultiResIco(icoSizes, pngStreams, icoPath);
        Console.WriteLine($"Saved: {icoPath}");

        // 5. Horizontal Banner
        var logosDir = Path.Combine(assetsDir, "Logos");
        if (!Directory.Exists(logosDir)) Directory.CreateDirectory(logosDir);
        using var banner = RenderBrandBanner(600, 160);
        var bannerPath = Path.Combine(logosDir, "brand_banner.png");
        SaveBitmapAsPng(banner, bannerPath);
        Console.WriteLine($"Saved: {bannerPath}");

        Console.WriteLine("Aesthetic logo suite successfully rendered!");
    }

    static SKBitmap RenderAestheticLogo(int size)
    {
        var bitmap = new SKBitmap(size, size, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        float s = size / 512f;

        // 1. Outer Soft Glow
        using (var glowPaint = new SKPaint
        {
            Color = new SKColor(0x63, 0x66, 0xF1, 0x40), // Indigo glow
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 28 * s),
            IsAntialias = true
        })
        {
            canvas.DrawRoundRect(new SKRect(32 * s, 32 * s, 480 * s, 480 * s), 110 * s, 110 * s, glowPaint);
        }

        // 2. Base Squircle - Ultra-deep Obsidian / Midnight Slate gradient
        var squircleRect = new SKRoundRect(new SKRect(24 * s, 24 * s, 488 * s, 488 * s), 108 * s, 108 * s);
        using (var bgShader = SKShader.CreateLinearGradient(
            new SKPoint(24 * s, 24 * s),
            new SKPoint(488 * s, 488 * s),
            new[] { new SKColor(0x13, 0x1A, 0x2A), new SKColor(0x0A, 0x0E, 0x17) },
            null,
            SKShaderTileMode.Clamp))
        using (var bgPaint = new SKPaint { Shader = bgShader, IsAntialias = true })
        {
            canvas.DrawRoundRect(squircleRect, bgPaint);
        }

        // 3. Luxurious Beveled Border (Translucent Metallic Gradient)
        using (var strokeShader = SKShader.CreateLinearGradient(
            new SKPoint(24 * s, 24 * s),
            new SKPoint(488 * s, 488 * s),
            new[] { new SKColor(0x81, 0x8C, 0xF8, 0xAA), new SKColor(0x33, 0x41, 0x55, 0x44) },
            null,
            SKShaderTileMode.Clamp))
        using (var strokePaint = new SKPaint
        {
            Shader = strokeShader,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3.5f * s,
            IsAntialias = true
        })
        {
            canvas.DrawRoundRect(squircleRect, strokePaint);
        }

        // 4. Stylized Fluent Document Origami / "D" Monogram
        // Document Card Base (Left: 120, Top: 95, Right: 392, Bottom: 417)
        float left = 120 * s;
        float top = 95 * s;
        float right = 392 * s;
        float bottom = 417 * s;
        float cornerRadius = 32 * s;
        float fold = 84 * s;

        // Document Path with folded top-right corner
        using (var docPath = new SKPath())
        {
            docPath.MoveTo(left + cornerRadius, top);
            docPath.LineTo(right - fold, top);
            docPath.LineTo(right, top + fold);
            docPath.LineTo(right, bottom - cornerRadius);
            docPath.ArcTo(new SKRect(right - 2 * cornerRadius, bottom - 2 * cornerRadius, right, bottom), 0, 90, false);
            docPath.LineTo(left + cornerRadius, bottom);
            docPath.ArcTo(new SKRect(left, bottom - 2 * cornerRadius, left + 2 * cornerRadius, bottom), 90, 90, false);
            docPath.LineTo(left, top + cornerRadius);
            docPath.ArcTo(new SKRect(left, top, left + 2 * cornerRadius, top + 2 * cornerRadius), 180, 90, false);
            docPath.Close();

            // Card Drop Shadow
            using (var shadowPaint = new SKPaint
            {
                Color = new SKColor(0x00, 0x00, 0x00, 0x90),
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 20 * s),
                IsAntialias = true
            })
            {
                canvas.DrawPath(docPath, shadowPaint);
            }

            // Glassmorphic / frosted white gradient for document body
            using (var docShader = SKShader.CreateLinearGradient(
                new SKPoint(left, top),
                new SKPoint(right, bottom),
                new[] { new SKColor(0xF8, 0xFA, 0xFC), new SKColor(0xE2, 0xE8, 0xF0) },
                null,
                SKShaderTileMode.Clamp))
            using (var docPaint = new SKPaint { Shader = docShader, IsAntialias = true })
            {
                canvas.DrawPath(docPath, docPaint);
            }

            // Inner subtle border
            using (var docBorder = new SKPaint
            {
                Color = new SKColor(0xCB, 0xD5, 0xE1),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2.5f * s,
                IsAntialias = true
            })
            {
                canvas.DrawPath(docPath, docBorder);
            }
        }

        // 5. Elegant Fold Corner (Fold flap at top right)
        using (var foldPath = new SKPath())
        {
            foldPath.MoveTo(right - fold, top);
            foldPath.LineTo(right - fold, top + fold - 12 * s);
            foldPath.ArcTo(new SKRect(right - fold, top + fold - 24 * s, right - fold + 24 * s, top + fold), 180, -90, false);
            foldPath.LineTo(right, top + fold);
            foldPath.Close();

            using (var foldShader = SKShader.CreateLinearGradient(
                new SKPoint(right - fold, top),
                new SKPoint(right, top + fold),
                new[] { new SKColor(0xD1, 0xD5, 0xDB), new SKColor(0x9C, 0xA3, 0xAF) },
                null,
                SKShaderTileMode.Clamp))
            using (var foldPaint = new SKPaint { Shader = foldShader, IsAntialias = true })
            {
                canvas.DrawPath(foldPath, foldPaint);
            }

            using (var foldBorder = new SKPaint
            {
                Color = new SKColor(0x94, 0xA3, 0xB8),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2 * s,
                IsAntialias = true
            })
            {
                canvas.DrawPath(foldPath, foldBorder);
            }
        }

        // 6. Central Emblem: Modern Gradient Ribbon "D" + Precision Checkmark
        // Beautiful sunset saffron-to-coral-to-indigo gradient
        float cx = 256 * s;
        float cy = 256 * s;

        // Vibrant Gradient Ribbon Badge (Rounded Shield/Emblem)
        float rw = 72 * s;
        float rh = 84 * s;
        using (var ribbonPath = new SKPath())
        {
            ribbonPath.MoveTo(cx - rw, cy - rh * 0.75f);
            ribbonPath.LineTo(cx + rw - 20 * s, cy - rh * 0.75f);
            ribbonPath.QuadTo(cx + rw + 8 * s, cy - rh * 0.75f, cx + rw + 8 * s, cy - rh * 0.3f);
            ribbonPath.QuadTo(cx + rw + 8 * s, cy + rh * 0.2f, cx + rw - 14 * s, cy + rh * 0.5f);
            ribbonPath.QuadTo(cx + rw * 0.4f, cy + rh, cx, cy + rh);
            ribbonPath.LineTo(cx - rw, cy + rh);
            ribbonPath.Close();

            // Radiant Indian Saffron to Warm Amber Gradient
            using (var ribbonShader = SKShader.CreateLinearGradient(
                new SKPoint(cx - rw, cy - rh),
                new SKPoint(cx + rw, cy + rh),
                new[] { new SKColor(0xFF, 0x6B, 0x00), new SKColor(0xFF, 0x9F, 0x1C), new SKColor(0xF5, 0x9E, 0x0B) },
                null,
                SKShaderTileMode.Clamp))
            using (var ribbonPaint = new SKPaint { Shader = ribbonShader, IsAntialias = true })
            {
                canvas.DrawPath(ribbonPath, ribbonPaint);
            }

            // Subtle inner glow on badge
            using (var ribbonStroke = new SKPaint
            {
                Color = new SKColor(0xFF, 0xEB, 0xC4, 0xCC),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3 * s,
                IsAntialias = true
            })
            {
                canvas.DrawPath(ribbonPath, ribbonStroke);
            }
        }

        // 7. Crisp High-Tech Checkmark inside Badge (Solid White, perfectly balanced)
        using (var checkPath = new SKPath())
        {
            checkPath.MoveTo(cx - 38 * s, cy + 4 * s);
            checkPath.LineTo(cx - 12 * s, cy + 30 * s);
            checkPath.LineTo(cx + 40 * s, cy - 24 * s);

            using (var checkShadow = new SKPaint
            {
                Color = new SKColor(0x00, 0x00, 0x00, 0x40),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 14 * s,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4 * s),
                IsAntialias = true
            })
            {
                canvas.DrawPath(checkPath, checkShadow);
            }

            using (var checkPaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 12 * s,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                IsAntialias = true
            })
            {
                canvas.DrawPath(checkPath, checkPaint);
            }
        }

        // 8. Horizontal Modern Minimal Document Accents
        float[] lineY = { 354 * s, 376 * s };
        float[] lineW = { 130 * s, 85 * s };
        for (int i = 0; i < lineY.Length; i++)
        {
            using var lp = new SKPaint
            {
                Color = new SKColor(0x94, 0xA3, 0xB8),
                StrokeWidth = 5 * s,
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true
            };
            canvas.DrawLine(150 * s, lineY[i], 150 * s + lineW[i], lineY[i], lp);
        }

        // 9. Tiny Emerald Green Offline Verified Dot (Bottom right of document)
        float dotX = 356 * s;
        float dotY = 376 * s;
        float dotR = 14 * s;
        using (var dotShader = SKShader.CreateLinearGradient(
            new SKPoint(dotX - dotR, dotY - dotR),
            new SKPoint(dotX + dotR, dotY + dotR),
            new[] { new SKColor(0x10, 0xB9, 0x81), new SKColor(0x05, 0x96, 0x69) },
            null,
            SKShaderTileMode.Clamp))
        using (var dotPaint = new SKPaint { Shader = dotShader, IsAntialias = true })
        {
            canvas.DrawCircle(dotX, dotY, dotR, dotPaint);
        }
        using (var dotStroke = new SKPaint
        {
            Color = SKColors.White,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3 * s,
            IsAntialias = true
        })
        {
            canvas.DrawCircle(dotX, dotY, dotR, dotStroke);
        }

        return bitmap;
    }

    static SKBitmap RenderBrandBanner(int width, int height)
    {
        var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        // Draw modern aesthetic logo
        using var logo = RenderAestheticLogo(120);
        canvas.DrawBitmap(logo, 20, 20);

        // Typography
        using var fontTitle = new SKFont(SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold), 38);
        using var paintAccent = new SKPaint { Color = new SKColor(0xFF, 0x6B, 0x00), IsAntialias = true };
        using var paintMain = new SKPaint { Color = SKColors.White, IsAntialias = true };

        canvas.DrawText("iLove", 160, 68, SKTextAlign.Left, fontTitle, paintAccent);
        canvas.DrawText("Dastavez", 255, 68, SKTextAlign.Left, fontTitle, paintMain);

        using var fontSub = new SKFont(SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Normal), 20);
        using var paintSub = new SKPaint { Color = new SKColor(0x94, 0xA3, 0xB8), IsAntialias = true };
        canvas.DrawText("Dastavez Studio", 160, 102, SKTextAlign.Left, fontSub, paintSub);

        using var fontTagline = new SKFont(SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold), 17);
        using var paintTagline = new SKPaint { Color = new SKColor(0x38, 0xBD, 0xF8), IsAntialias = true };
        canvas.DrawText("हर दस्तावेज़, तैयार! — 100% Offline", 160, 134, SKTextAlign.Left, fontTagline, paintTagline);

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
        writer.Write((ushort)0);
        writer.Write((ushort)1);
        writer.Write((ushort)sizes.Length);

        int currentOffset = 6 + (16 * sizes.Length);

        for (int i = 0; i < sizes.Length; i++)
        {
            int s = sizes[i];
            writer.Write((byte)(s >= 256 ? 0 : s));
            writer.Write((byte)(s >= 256 ? 0 : s));
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((ushort)1);
            writer.Write((ushort)32);
            writer.Write((uint)pngData[i].Length);
            writer.Write((uint)currentOffset);

            currentOffset += pngData[i].Length;
        }

        for (int i = 0; i < sizes.Length; i++)
        {
            writer.Write(pngData[i]);
        }
    }
}
