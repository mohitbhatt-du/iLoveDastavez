using SkiaSharp;
using DastavezStudio.Core.Interfaces;
using DastavezStudio.Core.Models;

namespace DastavezStudio.ImageEngine;

public class ImageProcessor : IImageProcessor
{
    public async Task<ImageDimensions> GetImageDimensionsAsync(string inputFilePath)
    {
        return await Task.Run(() =>
        {
            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException("Image file not found.", inputFilePath);

            using var stream = File.OpenRead(inputFilePath);
            using var codec = SKCodec.Create(stream);
            if (codec == null)
            {
                using var bmp = SKBitmap.Decode(inputFilePath);
                if (bmp == null)
                    throw new InvalidOperationException("Failed to decode image data.");

                return new ImageDimensions
                {
                    Width = bmp.Width,
                    Height = bmp.Height,
                    DpiX = 96,
                    DpiY = 96,
                    Format = Path.GetExtension(inputFilePath).TrimStart('.').ToUpperInvariant()
                };
            }

            var info = codec.Info;
            var ext = Path.GetExtension(inputFilePath).TrimStart('.').ToUpperInvariant();

            return new ImageDimensions
            {
                Width = info.Width,
                Height = info.Height,
                DpiX = 96,
                DpiY = 96,
                Format = ext
            };
        });
    }

    public async Task<string> ResizeImageAsync(string inputFilePath, string outputFilePath, ImageResizeOptions options, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            using var inputStream = File.OpenRead(inputFilePath);
            using var originalBitmap = SKBitmap.Decode(inputStream);
            if (originalBitmap == null)
                throw new InvalidOperationException("Unable to decode source image.");

            int targetW = options.WidthPx ?? originalBitmap.Width;
            int targetH = options.HeightPx ?? originalBitmap.Height;

            if (options.MaintainAspectRatio)
            {
                double ratioX = (double)targetW / originalBitmap.Width;
                double ratioY = (double)targetH / originalBitmap.Height;
                double ratio = Math.Min(ratioX, ratioY);

                if (options.WidthPx.HasValue && !options.HeightPx.HasValue)
                {
                    ratio = (double)options.WidthPx.Value / originalBitmap.Width;
                }
                else if (!options.WidthPx.HasValue && options.HeightPx.HasValue)
                {
                    ratio = (double)options.HeightPx.Value / originalBitmap.Height;
                }

                targetW = Math.Max(1, (int)Math.Round(originalBitmap.Width * ratio));
                targetH = Math.Max(1, (int)Math.Round(originalBitmap.Height * ratio));
            }

            var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
            using var resizedBitmap = originalBitmap.Resize(new SKImageInfo(targetW, targetH), sampling);
            if (resizedBitmap == null)
                throw new InvalidOperationException("Failed to resize bitmap.");

            using var image = SKImage.FromBitmap(resizedBitmap);
            var format = GetSkEncodedFormat(outputFilePath);
            using var data = image.Encode(format, 90);

            var dir = Path.GetDirectoryName(outputFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using var outputStream = File.Open(outputFilePath, FileMode.Create, FileAccess.Write);
            data.SaveTo(outputStream);

            return outputFilePath;
        }, ct);
    }

    public async Task<string> CompressImageAsync(string inputFilePath, string outputFilePath, ImageCompressionOptions options, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            using var originalBitmap = SKBitmap.Decode(inputFilePath);
            if (originalBitmap == null)
                throw new InvalidOperationException("Unable to decode image.");

            var format = options.OutputFormat switch
            {
                ImageFormatType.Png => SKEncodedImageFormat.Png,
                ImageFormatType.Webp => SKEncodedImageFormat.Webp,
                _ => SKEncodedImageFormat.Jpeg
            };

            var dir = Path.GetDirectoryName(outputFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!options.TargetMaxKb.HasValue || format == SKEncodedImageFormat.Png)
            {
                using var img = SKImage.FromBitmap(originalBitmap);
                using var data = img.Encode(format, options.Quality);
                using var outStream = File.Open(outputFilePath, FileMode.Create, FileAccess.Write);
                data.SaveTo(outStream);
                return outputFilePath;
            }

            // Target Size Compression via iterative binary search & adaptive downscaling
            long targetBytes = (long)options.TargetMaxKb.Value * 1024;
            SKBitmap workingBitmap = originalBitmap;
            bool ownsWorkingBitmap = false;

            try
            {
                int minQ = 5;
                int maxQ = 98;
                int bestQ = minQ;
                SKData? bestData = null;

                for (int attempt = 0; attempt < 3; attempt++)
                {
                    minQ = 5;
                    maxQ = 98;
                    bestQ = minQ;

                    while (minQ <= maxQ)
                    {
                        ct.ThrowIfCancellationRequested();
                        int midQ = (minQ + maxQ) / 2;
                        using var testImg = SKImage.FromBitmap(workingBitmap);
                        using var testData = testImg.Encode(format, midQ);

                        if (testData.Size <= targetBytes)
                        {
                            bestQ = midQ;
                            bestData?.Dispose();
                            bestData = SKData.CreateCopy(testData.Data, (nuint)testData.Size);
                            minQ = midQ + 1; // Try higher quality
                        }
                        else
                        {
                            maxQ = midQ - 1; // Reduce quality
                        }
                    }

                    if (bestData != null && bestData.Size <= targetBytes)
                    {
                        break;
                    }

                    // If even quality=5 exceeds target size, downscale bitmap resolution by 15%
                    int newW = (int)(workingBitmap.Width * 0.85);
                    int newH = (int)(workingBitmap.Height * 0.85);
                    if (newW < 50 || newH < 50) break;

                    var scaled = workingBitmap.Resize(new SKImageInfo(newW, newH), new SKSamplingOptions(SKFilterMode.Linear));
                    if (scaled == null) break;

                    if (ownsWorkingBitmap) workingBitmap.Dispose();
                    workingBitmap = scaled;
                    ownsWorkingBitmap = true;
                }

                if (bestData == null)
                {
                    using var fallbackImg = SKImage.FromBitmap(workingBitmap);
                    bestData = fallbackImg.Encode(format, 10);
                }

                using var finalStream = File.Open(outputFilePath, FileMode.Create, FileAccess.Write);
                bestData.SaveTo(finalStream);
                bestData.Dispose();

                return outputFilePath;
            }
            finally
            {
                if (ownsWorkingBitmap)
                {
                    workingBitmap.Dispose();
                }
            }
        }, ct);
    }

    public async Task<string> ProcessForExamPresetAsync(string inputFilePath, string outputFilePath, ExamPreset preset, CancellationToken ct = default)
    {
        return await Task.Run(async () =>
        {
            ct.ThrowIfCancellationRequested();
            // Step 1: Resize to exact required preset dimensions
            var tempResized = Path.Combine(Path.GetTempPath(), $"preset_resize_{Guid.NewGuid():N}.jpg");
            try
            {
                await ResizeImageAsync(inputFilePath, tempResized, new ImageResizeOptions
                {
                    WidthPx = preset.RequiredWidthPx,
                    HeightPx = preset.RequiredHeightPx,
                    MaintainAspectRatio = false,
                    TargetDpi = preset.TargetDpi
                }, ct);

                // Step 2: Compress to target file size bounds
                await CompressImageAsync(tempResized, outputFilePath, new ImageCompressionOptions
                {
                    TargetMaxKb = preset.MaxSizeKb,
                    TargetMinKb = preset.MinSizeKb,
                    OutputFormat = ImageFormatType.Jpeg,
                    StripMetadata = true
                }, ct);

                return outputFilePath;
            }
            finally
            {
                if (File.Exists(tempResized))
                {
                    try { File.Delete(tempResized); } catch { }
                }
            }
        }, ct);
    }

    public async Task<string> CropImageAsync(string inputFilePath, string outputFilePath, int x, int y, int width, int height, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            using var original = SKBitmap.Decode(inputFilePath);
            if (original == null)
                throw new InvalidOperationException("Failed to decode image for cropping.");

            int safeX = Math.Max(0, Math.Min(x, original.Width - 1));
            int safeY = Math.Max(0, Math.Min(y, original.Height - 1));
            int safeW = Math.Max(1, Math.Min(width, original.Width - safeX));
            int safeH = Math.Max(1, Math.Min(height, original.Height - safeY));

            var cropRect = new SKRectI(safeX, safeY, safeX + safeW, safeY + safeH);
            using var cropped = new SKBitmap();
            original.ExtractSubset(cropped, cropRect);

            using var image = SKImage.FromBitmap(cropped);
            var format = GetSkEncodedFormat(outputFilePath);
            using var data = image.Encode(format, 90);

            var dir = Path.GetDirectoryName(outputFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using var outStream = File.Open(outputFilePath, FileMode.Create, FileAccess.Write);
            data.SaveTo(outStream);

            return outputFilePath;
        }, ct);
    }

    public async Task<string> RotateFlipAsync(string inputFilePath, string outputFilePath, int rotateDegrees, bool flipHorizontal, bool flipVertical, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            using var original = SKBitmap.Decode(inputFilePath);
            if (original == null)
                throw new InvalidOperationException("Failed to decode image.");

            int targetW = original.Width;
            int targetH = original.Height;
            if (rotateDegrees == 90 || rotateDegrees == 270)
            {
                targetW = original.Height;
                targetH = original.Width;
            }

            using var rotatedBmp = new SKBitmap(targetW, targetH);
            using (var canvas = new SKCanvas(rotatedBmp))
            {
                canvas.Clear(SKColors.Transparent);
                canvas.Translate(targetW / 2.0f, targetH / 2.0f);
                if (rotateDegrees != 0)
                {
                    canvas.RotateDegrees(rotateDegrees);
                }
                if (flipHorizontal || flipVertical)
                {
                    canvas.Scale(flipHorizontal ? -1.0f : 1.0f, flipVertical ? -1.0f : 1.0f);
                }
                canvas.DrawBitmap(original, -original.Width / 2.0f, -original.Height / 2.0f, SKSamplingOptions.Default, null);
            }

            using var image = SKImage.FromBitmap(rotatedBmp);
            var format = GetSkEncodedFormat(outputFilePath);
            using var data = image.Encode(format, 90);

            var dir = Path.GetDirectoryName(outputFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using var outStream = File.Open(outputFilePath, FileMode.Create, FileAccess.Write);
            data.SaveTo(outStream);

            return outputFilePath;
        }, ct);
    }

    public async Task<string> AdjustColorsAsync(string inputFilePath, string outputFilePath, float brightness, float contrast, bool grayscale, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            using var original = SKBitmap.Decode(inputFilePath);
            if (original == null)
                throw new InvalidOperationException("Failed to decode image.");

            using var surface = SKSurface.Create(new SKImageInfo(original.Width, original.Height));
            var canvas = surface.Canvas;

            using var paint = new SKPaint();
            if (grayscale)
            {
                paint.ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
                {
                    0.21f, 0.72f, 0.07f, 0, 0,
                    0.21f, 0.72f, 0.07f, 0, 0,
                    0.21f, 0.72f, 0.07f, 0, 0,
                    0,     0,     0,     1, 0
                });
            }

            canvas.DrawBitmap(original, 0, 0, SKSamplingOptions.Default, paint);

            using var image = surface.Snapshot();
            var format = GetSkEncodedFormat(outputFilePath);
            using var data = image.Encode(format, 90);

            var dir = Path.GetDirectoryName(outputFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using var outStream = File.Open(outputFilePath, FileMode.Create, FileAccess.Write);
            data.SaveTo(outStream);

            return outputFilePath;
        }, ct);
    }

    private static SKEncodedImageFormat GetSkEncodedFormat(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".png" => SKEncodedImageFormat.Png,
            ".webp" => SKEncodedImageFormat.Webp,
            ".bmp" => SKEncodedImageFormat.Bmp,
            _ => SKEncodedImageFormat.Jpeg
        };
    }
}
