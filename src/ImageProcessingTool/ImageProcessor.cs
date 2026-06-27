using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessingTool;

public static class ImageProcessor
{
    // ==================== FILTERS ====================

    public static Bitmap Grayscale(Bitmap source, IProgress<int>? progress = null)
    {
        return ApplyPixelFunc(source, (r, g, b) =>
        {
            byte gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
            return (gray, gray, gray);
        }, progress);
    }

    public static Bitmap Sepia(Bitmap source, IProgress<int>? progress = null)
    {
        return ApplyPixelFunc(source, (r, g, b) =>
        {
            int gray = (int)(0.299 * r + 0.587 * g + 0.114 * b);
            byte sr, sg, sb;
            if (gray > 154)
            {
                sr = 240;
                sg = ClampToByte(gray + 20);
                sb = ClampToByte(gray - 30);
            }
            else
            {
                sr = ClampToByte(gray + 100);
                sg = ClampToByte(gray + 50);
                sb = (byte)gray;
            }
            return (sr, sg, sb);
        }, progress);
    }

    public static Bitmap Negative(Bitmap source, IProgress<int>? progress = null)
    {
        return ApplyPixelFunc(source, (r, g, b) =>
            ((byte)(255 - r), (byte)(255 - g), (byte)(255 - b)), progress);
    }

    public static Bitmap BoxBlur(Bitmap source, int radius = 1, IProgress<int>? progress = null)
    {
        int kernelSize = 2 * radius + 1;
        double[,] kernel = new double[kernelSize, kernelSize];
        for (int i = 0; i < kernelSize; i++)
            for (int j = 0; j < kernelSize; j++)
                kernel[i, j] = 1.0;
        return ApplyConvolution(source, kernel, 1.0 / (kernelSize * kernelSize), 0.0, progress);
    }

    public static Bitmap Sharpen(Bitmap source, IProgress<int>? progress = null)
    {
        double[,] kernel = {
            { 0, -1,  0 },
            { -1, 5, -1 },
            { 0, -1,  0 }
        };
        return ApplyConvolution(source, kernel, 1.0, 0.0, progress);
    }

    public static Bitmap EdgeDetectSobel(Bitmap source, IProgress<int>? progress = null)
    {
        double[,] gxKernel = {
            { -1, 0, 1 },
            { -2, 0, 2 },
            { -1, 0, 1 }
        };
        double[,] gyKernel = {
            { -1, -2, -1 },
            {  0,  0,  0 },
            {  1,  2,  1 }
        };

        var rect = new Rectangle(0, 0, source.Width, source.Height);
        var srcData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        var result = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
        var dstData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

        int stride = srcData.Stride;
        int bytes = Math.Abs(stride) * source.Height;
        byte[] srcBuf = new byte[bytes];
        System.Runtime.InteropServices.Marshal.Copy(srcData.Scan0, srcBuf, 0, bytes);

        byte[] dstBuf = new byte[bytes];
        int w = source.Width, h = source.Height;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                double gxR = 0, gxG = 0, gxB = 0;
                double gyR = 0, gyG = 0, gyB = 0;

                for (int ky = -1; ky <= 1; ky++)
                {
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        int sx = Math.Clamp(x + kx, 0, w - 1);
                        int sy = Math.Clamp(y + ky, 0, h - 1);
                        int idx = sy * stride + sx * 3;

                        double gx = gxKernel[ky + 1, kx + 1];
                        double gy = gyKernel[ky + 1, kx + 1];

                        gxB += srcBuf[idx] * gx;
                        gyB += srcBuf[idx] * gy;
                        gxG += srcBuf[idx + 1] * gx;
                        gyG += srcBuf[idx + 1] * gy;
                        gxR += srcBuf[idx + 2] * gx;
                        gyR += srcBuf[idx + 2] * gy;
                    }
                }

                int di = y * stride + x * 3;
                dstBuf[di]     = ClampToByte((int)Math.Sqrt(gxB * gxB + gyB * gyB));
                dstBuf[di + 1] = ClampToByte((int)Math.Sqrt(gxG * gxG + gyG * gyG));
                dstBuf[di + 2] = ClampToByte((int)Math.Sqrt(gxR * gxR + gyR * gyR));
            }
            progress?.Report((y + 1) * 100 / h);
        }

        System.Runtime.InteropServices.Marshal.Copy(dstBuf, 0, dstData.Scan0, bytes);
        source.UnlockBits(srcData);
        result.UnlockBits(dstData);
        return result;
    }

    // ==================== ADJUSTMENTS ====================

    public static Bitmap AdjustBrightness(Bitmap source, int amount, IProgress<int>? progress = null)
    {
        return ApplyPixelFunc(source, (r, g, b) =>
        {
            byte nr = ClampToByte(r + amount);
            byte ng = ClampToByte(g + amount);
            byte nb = ClampToByte(b + amount);
            return (nr, ng, nb);
        }, progress);
    }

    public static Bitmap AdjustContrast(Bitmap source, double factor, IProgress<int>? progress = null)
    {
        return ApplyPixelFunc(source, (r, g, b) =>
        {
            byte nr = ClampToByte((int)((r - 128) * factor + 128));
            byte ng = ClampToByte((int)((g - 128) * factor + 128));
            byte nb = ClampToByte((int)((b - 128) * factor + 128));
            return (nr, ng, nb);
        }, progress);
    }

    // ==================== TRANSFORMATIONS ====================

    public static Bitmap Rotate(Bitmap source, RotateFlipType type)
    {
        var result = new Bitmap(source);
        result.RotateFlip(type);
        return result;
    }

    public static Bitmap FlipHorizontal(Bitmap source)
    {
        var result = new Bitmap(source);
        result.RotateFlip(RotateFlipType.RotateNoneFlipX);
        return result;
    }

    public static Bitmap FlipVertical(Bitmap source)
    {
        var result = new Bitmap(source);
        result.RotateFlip(RotateFlipType.RotateNoneFlipY);
        return result;
    }

    public static Bitmap Resize(Bitmap source, int newWidth, int newHeight)
    {
        var result = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);
        using var g = Graphics.FromImage(result);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.DrawImage(source, 0, 0, newWidth, newHeight);
        return result;
    }

    // ==================== PRIVATE HELPERS ====================

    private static byte ClampToByte(int value)
    {
        if (value < 0) return 0;
        if (value > 255) return 255;
        return (byte)value;
    }

    private static unsafe Bitmap ApplyPixelFunc(Bitmap source,
        Func<byte, byte, byte, (byte r, byte g, byte b)> pixelFunc,
        IProgress<int>? progress = null)
    {
        var rect = new Rectangle(0, 0, source.Width, source.Height);
        var srcData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        var result = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
        var dstData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

        int stride = srcData.Stride;
        int h = source.Height;
        int w = source.Width;

        byte* srcPtr = (byte*)srcData.Scan0;
        byte* dstPtr = (byte*)dstData.Scan0;

        for (int y = 0; y < h; y++)
        {
            byte* srcRow = srcPtr + y * stride;
            byte* dstRow = dstPtr + y * dstData.Stride;

            for (int x = 0; x < w; x++)
            {
                int si = x * 3;
                byte b = srcRow[si];
                byte g = srcRow[si + 1];
                byte r = srcRow[si + 2];

                var (nr, ng, nb) = pixelFunc(r, g, b);

                int di = x * 3;
                dstRow[di] = nb;
                dstRow[di + 1] = ng;
                dstRow[di + 2] = nr;
            }

            progress?.Report((y + 1) * 100 / h);
        }

        source.UnlockBits(srcData);
        result.UnlockBits(dstData);
        return result;
    }

    private static unsafe Bitmap ApplyConvolution(Bitmap source, double[,] kernel,
        double factor, double bias, IProgress<int>? progress = null)
    {
        int kernelHeight = kernel.GetLength(0);
        int kernelWidth = kernel.GetLength(1);
        int kCenterY = kernelHeight / 2;
        int kCenterX = kernelWidth / 2;

        var rect = new Rectangle(0, 0, source.Width, source.Height);
        var srcData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        var result = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
        var dstData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

        int stride = srcData.Stride;
        int w = source.Width;
        int h = source.Height;

        byte* srcPtr = (byte*)srcData.Scan0;
        byte* dstPtr = (byte*)dstData.Scan0;
        int dstStride = dstData.Stride;

        for (int y = 0; y < h; y++)
        {
            byte* dstRow = dstPtr + y * dstStride;

            for (int x = 0; x < w; x++)
            {
                double sumR = 0, sumG = 0, sumB = 0;

                for (int ky = 0; ky < kernelHeight; ky++)
                {
                    int sy = Math.Clamp(y + ky - kCenterY, 0, h - 1);
                    byte* srcRow = srcPtr + sy * stride;

                    for (int kx = 0; kx < kernelWidth; kx++)
                    {
                        int sx = Math.Clamp(x + kx - kCenterX, 0, w - 1);
                        int si = sx * 3;
                        double k = kernel[ky, kx];

                        sumB += srcRow[si] * k;
                        sumG += srcRow[si + 1] * k;
                        sumR += srcRow[si + 2] * k;
                    }
                }

                int di = x * 3;
                dstRow[di]     = ClampToByte((int)(sumB * factor + bias));
                dstRow[di + 1] = ClampToByte((int)(sumG * factor + bias));
                dstRow[di + 2] = ClampToByte((int)(sumR * factor + bias));
            }

            progress?.Report((y + 1) * 100 / h);
        }

        source.UnlockBits(srcData);
        result.UnlockBits(dstData);
        return result;
    }
}
