using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessingTool;

public static class ImageFileHandler
{
    private static readonly Dictionary<string, ImageFormat> FormatMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg", ImageFormat.Jpeg },
        { ".jpeg", ImageFormat.Jpeg },
        { ".png", ImageFormat.Png },
        { ".bmp", ImageFormat.Bmp },
        { ".gif", ImageFormat.Gif },
        { ".tiff", ImageFormat.Tiff },
        { ".tif", ImageFormat.Tiff },
    };

    public static (Bitmap image, string format, long fileSize, int width, int height) OpenImage(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var ms = new MemoryStream();
        fs.CopyTo(ms);
        ms.Position = 0;
        var bmp = new Bitmap(ms);

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        var info = new FileInfo(filePath);

        return (bmp, ext, info.Length, bmp.Width, bmp.Height);
    }

    public static void SaveImage(Bitmap image, string filePath)
    {
        var ext = Path.GetExtension(filePath);
        var format = GetImageFormat(ext);
        image.Save(filePath, format);
    }

    public static string GetImageInfoString(Bitmap image, string? filePath = null)
    {
        string sizeStr = filePath != null && File.Exists(filePath)
            ? FormatFileSize(new FileInfo(filePath).Length)
            : "";
        string dims = $"{image.Width}x{image.Height}";
        string fmt = filePath != null
            ? Path.GetExtension(filePath).TrimStart('.').ToUpperInvariant()
            : "Untitled";

        return sizeStr.Length > 0
            ? $"{dims} | {fmt} | {sizeStr}"
            : $"{dims} | {fmt}";
    }

    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.#} {sizes[order]}";
    }

    public static ImageFormat GetImageFormat(string extension)
    {
        return FormatMap.TryGetValue(extension, out var fmt)
            ? fmt
            : ImageFormat.Png;
    }

    public static string BuildFilter()
    {
        return "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff|JPEG|*.jpg;*.jpeg|PNG|*.png|BMP|*.bmp|GIF|*.gif|TIFF|*.tiff|All Files|*.*";
    }
}
