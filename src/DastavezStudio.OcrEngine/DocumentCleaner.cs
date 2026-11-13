using System.Diagnostics;
using SkiaSharp;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;

namespace DastavezStudio.OcrEngine;

public class DocumentCleaner : IDocumentCleaner
{
    public async Task<DocumentCleanResult> CleanDocumentAsync(
        string inputPath,
        string outputPath,
        DocumentCleanOptions options,
        IProgress<double>? progress = null,
        CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();
            ct.ThrowIfCancellationRequested();

            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Scanned document file not found.", inputPath);

            progress?.Report(10.0);

            using var inputStream = File.OpenRead(inputPath);
            using var original = SKBitmap.Decode(inputStream);
            if (original == null)
                throw new InvalidOperationException("Failed to decode document image.");

            ct.ThrowIfCancellationRequested();
            progress?.Report(25.0);

            SKBitmap working = original.Copy();
            double detectedAngle = 0.0;
            bool shadowsRemoved = false;
            bool binarized = false;

            try
            {
                // Step 1: Auto-Deskew
                if (options.AutoDeskew)
                {
                    detectedAngle = DetectSkewAngleInternal(working, ct);
                    if (Math.Abs(detectedAngle) > 0.4)
                    {
                        var deskewed = RotateWithWhiteBackground(working, (float)-detectedAngle);
                        working.Dispose();
                        working = deskewed;
                    }
                }

                ct.ThrowIfCancellationRequested();
                progress?.Report(50.0);

                // Step 2: Shadow & Uneven Lighting Removal
                if (options.RemoveShadows)
                {
                    var shadowCleaned = RemoveShadowsInternal(working, ct);
                    working.Dispose();
                    working = shadowCleaned;
                    shadowsRemoved = true;
                }

                ct.ThrowIfCancellationRequested();
                progress?.Report(75.0);

                // Step 3: Pure Black & White Clean Document Thresholding
                if (options.BinarizePureBlackAndWhite)
                {
                    var binarizedBmp = ApplyAdaptiveThreshold(working, options.Threshold, ct);
                    working.Dispose();
                    working = binarizedBmp;
                    binarized = true;
                }
                else if (options.EnhanceContrast)
                {
                    var contrasted = ApplyDocumentContrast(working, ct);
                    working.Dispose();
                    working = contrasted;
                }

                ct.ThrowIfCancellationRequested();
                progress?.Report(90.0);

                // Ensure target directory exists
                var dir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using var image = SKImage.FromBitmap(working);
                var format = outputPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                    ? SKEncodedImageFormat.Png
                    : SKEncodedImageFormat.Jpeg;

                using var data = image.Encode(format, 95);
                using (var outStream = File.Open(outputPath, FileMode.Create, FileAccess.Write))
                {
                    data.SaveTo(outStream);
                }

                working.Dispose();
                sw.Stop();
                progress?.Report(100.0);

                return new DocumentCleanResult
                {
                    OutputPath = outputPath,
                    DetectedSkewAngleDegrees = detectedAngle,
                    ShadowsRemoved = shadowsRemoved,
                    Binarized = binarized,
                    Duration = sw.Elapsed
                };
            }
            catch
            {
                working.Dispose();
                throw;
            }
        }, ct);
    }

    public async Task<double> DetectSkewAngleAsync(string inputPath, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            using var stream = File.OpenRead(inputPath);
            using var bmp = SKBitmap.Decode(stream);
            if (bmp == null) return 0.0;
            return DetectSkewAngleInternal(bmp, ct);
        }, ct);
    }

    private static double DetectSkewAngleInternal(SKBitmap bmp, CancellationToken ct)
    {
        // Downsample to max 500px width for fast projection profiling
        int sampleW = Math.Min(500, bmp.Width);
        int sampleH = (int)((double)sampleW / bmp.Width * bmp.Height);

        using var smallBmp = new SKBitmap(sampleW, sampleH, SKColorType.Gray8, SKAlphaType.Opaque);
        using (var canvas = new SKCanvas(smallBmp))
        {
            canvas.Clear(SKColors.White);
            canvas.DrawBitmap(bmp, new SKRect(0, 0, sampleW, sampleH), new SKSamplingOptions(SKFilterMode.Linear));
        }

        double bestAngle = 0.0;
        double maxVariance = -1.0;

        // Search from -10° to +10° in 0.5° steps
        for (double angle = -10.0; angle <= 10.0; angle += 0.5)
        {
            ct.ThrowIfCancellationRequested();

            using var rotated = RotateWithWhiteBackground(smallBmp, (float)angle);
            double variance = CalculateHorizontalProjectionVariance(rotated);

            if (variance > maxVariance)
            {
                maxVariance = variance;
                bestAngle = angle;
            }
        }

        return bestAngle;
    }

    private static double CalculateHorizontalProjectionVariance(SKBitmap bmp)
    {
        int w = bmp.Width;
        int h = bmp.Height;
        var pixels = bmp.GetPixels();
        var rowSums = new double[h];

        unsafe
        {
            byte* ptr = (byte*)pixels.ToPointer();
            int bpp = bmp.BytesPerPixel;
            int rowBytes = bmp.RowBytes;

            for (int y = 0; y < h; y++)
            {
                byte* row = ptr + (y * rowBytes);
                double sum = 0;
                for (int x = 0; x < w; x++)
                {
                    sum += row[x * bpp];
                }
                rowSums[y] = sum;
            }
        }

        double mean = rowSums.Average();
        double variance = rowSums.Sum(v => (v - mean) * (v - mean)) / h;
        return variance;
    }

    private static SKBitmap RotateWithWhiteBackground(SKBitmap src, float angleDegrees)
    {
        int w = src.Width;
        int h = src.Height;
        var dst = new SKBitmap(w, h, src.ColorType, src.AlphaType);

        using (var canvas = new SKCanvas(dst))
        {
            canvas.Clear(SKColors.White);
            canvas.Translate(w / 2.0f, h / 2.0f);
            canvas.RotateDegrees(angleDegrees);
            canvas.Translate(-w / 2.0f, -h / 2.0f);
            canvas.DrawBitmap(src, 0, 0, new SKSamplingOptions(SKFilterMode.Linear));
        }

        return dst;
    }

    private static SKBitmap RemoveShadowsInternal(SKBitmap src, CancellationToken ct)
    {
        int w = src.Width;
        int h = src.Height;
        var dst = new SKBitmap(w, h, src.ColorType, src.AlphaType);

        // Background estimate: strong blur captures ambient lighting & shadow gradients
        using var background = new SKBitmap(w, h);
        using (var canvas = new SKCanvas(background))
        {
            using var blurPaint = new SKPaint
            {
                ImageFilter = SKImageFilter.CreateBlur(24.0f, 24.0f)
            };
            canvas.DrawBitmap(src, 0, 0, new SKSamplingOptions(SKFilterMode.Linear), blurPaint);
        }

        var srcPixels = src.GetPixels();
        var bgPixels = background.GetPixels();
        var dstPixels = dst.GetPixels();

        unsafe
        {
            byte* sPtr = (byte*)srcPixels.ToPointer();
            byte* bPtr = (byte*)bgPixels.ToPointer();
            byte* dPtr = (byte*)dstPixels.ToPointer();
            int totalBytes = w * h * 4;

            for (int i = 0; i < totalBytes; i += 4)
            {
                for (int c = 0; c < 3; c++)
                {
                    int bgVal = Math.Max(1, (int)bPtr[i + c]);
                    int normalized = (sPtr[i + c] * 245) / bgVal;
                    dPtr[i + c] = (byte)Math.Clamp(normalized, 0, 255);
                }
                dPtr[i + 3] = sPtr[i + 3];
            }
        }

        return dst;
    }

    private static SKBitmap ApplyAdaptiveThreshold(SKBitmap src, int baseThreshold, CancellationToken ct)
    {
        int w = src.Width;
        int h = src.Height;
        var dst = new SKBitmap(w, h, src.ColorType, src.AlphaType);

        // Local background average for Sauvola / Bradley adaptive binarization
        using var blurred = new SKBitmap(w, h);
        using (var canvas = new SKCanvas(blurred))
        {
            using var blurPaint = new SKPaint
            {
                ImageFilter = SKImageFilter.CreateBlur(8.0f, 8.0f)
            };
            canvas.DrawBitmap(src, 0, 0, new SKSamplingOptions(SKFilterMode.Linear), blurPaint);
        }

        var srcPixels = src.GetPixels();
        var blurPixels = blurred.GetPixels();
        var dstPixels = dst.GetPixels();

        unsafe
        {
            byte* sPtr = (byte*)srcPixels.ToPointer();
            byte* bPtr = (byte*)blurPixels.ToPointer();
            byte* dPtr = (byte*)dstPixels.ToPointer();
            int totalBytes = w * h * 4;

            for (int i = 0; i < totalBytes; i += 4)
            {
                int srcLum = (sPtr[i] * 11 + sPtr[i + 1] * 59 + sPtr[i + 2] * 30) / 100;
                int bgLum = (bPtr[i] * 11 + bPtr[i + 1] * 59 + bPtr[i + 2] * 30) / 100;

                // If pixel is darker than 88% of local surrounding background, it is text
                bool isText = srcLum < (bgLum * 0.88) && srcLum < baseThreshold;
                byte val = isText ? (byte)0 : (byte)255;

                dPtr[i] = val;
                dPtr[i + 1] = val;
                dPtr[i + 2] = val;
                dPtr[i + 3] = 255;
            }
        }

        return dst;
    }

    private static SKBitmap ApplyDocumentContrast(SKBitmap src, CancellationToken ct)
    {
        int w = src.Width;
        int h = src.Height;
        var dst = new SKBitmap(w, h, src.ColorType, src.AlphaType);

        var srcPixels = src.GetPixels();
        var dstPixels = dst.GetPixels();

        unsafe
        {
            byte* sPtr = (byte*)srcPixels.ToPointer();
            byte* dPtr = (byte*)dstPixels.ToPointer();
            int totalBytes = w * h * 4;

            for (int i = 0; i < totalBytes; i += 4)
            {
                int lum = (sPtr[i] * 11 + sPtr[i + 1] * 59 + sPtr[i + 2] * 30) / 100;
                // S-curve contrast enhancement
                int boosted = (int)(255.0 / (1.0 + Math.Exp(-0.04 * (lum - 128))));
                byte b = (byte)Math.Clamp(boosted, 0, 255);

                dPtr[i] = b;
                dPtr[i + 1] = b;
                dPtr[i + 2] = b;
                dPtr[i + 3] = sPtr[i + 3];
            }
        }

        return dst;
    }
}
