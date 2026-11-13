using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;

namespace DastavezStudio.App.Helpers;

/// <summary>
/// Thread-safe, ephemeral memory image and PDF preview helper.
/// Loads images and renders PDF page previews in-memory with zero disk file retention or locking.
/// </summary>
public static class ImagePreviewHelper
{
    public static async Task<BitmapImage?> LoadBitmapFromFileAsync(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return null;

        var ext = Path.GetExtension(filePath).ToLowerInvariant();

        try
        {
            if (ext == ".pdf")
            {
                return await RenderPdfPageAsync(filePath, 0);
            }

            // Read file bytes into memory and transfer to RandomAccessStream to avoid file locking
            var bytes = await File.ReadAllBytesAsync(filePath);
            return await LoadBitmapFromBytesAsync(bytes);
        }
        catch
        {
            return null;
        }
    }

    public static async Task<BitmapImage?> LoadBitmapFromBytesAsync(byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return null;

        try
        {
            var stream = new InMemoryRandomAccessStream();
            using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
            {
                writer.WriteBytes(bytes);
                await writer.StoreAsync();
            }

            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);
            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    public static async Task<BitmapImage?> RenderPdfPageAsync(string pdfPath, uint pageIndex = 0)
    {
        if (string.IsNullOrEmpty(pdfPath) || !File.Exists(pdfPath))
            return null;

        try
        {
            var storageFile = await StorageFile.GetFileFromPathAsync(pdfPath);
            var pdfDoc = await PdfDocument.LoadFromFileAsync(storageFile);
            if (pdfDoc.PageCount == 0) return null;

            if (pageIndex >= pdfDoc.PageCount) pageIndex = 0;

            using var page = pdfDoc.GetPage(pageIndex);
            var stream = new InMemoryRandomAccessStream();
            await page.RenderToStreamAsync(stream);

            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);
            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    public static async Task<uint> GetPdfPageCountAsync(string pdfPath)
    {
        if (string.IsNullOrEmpty(pdfPath) || !File.Exists(pdfPath))
            return 0;

        try
        {
            var storageFile = await StorageFile.GetFileFromPathAsync(pdfPath);
            var pdfDoc = await PdfDocument.LoadFromFileAsync(storageFile);
            return pdfDoc.PageCount;
        }
        catch
        {
            return 0;
        }
    }
}
