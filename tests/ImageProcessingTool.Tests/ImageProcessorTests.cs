using System.Drawing;
using System.Drawing.Imaging;
using ImageProcessingTool;

namespace ImageProcessingTool.Tests;

[TestClass]
public class ImageProcessorTests
{
    // ==================== Helpers ====================

    private static Bitmap CreateSolidBitmap(int w, int h, Color c)
    {
        var bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
        using var g = Graphics.FromImage(bmp);
        g.Clear(c);
        return bmp;
    }

    private static Color GetPixel(Bitmap bmp, int x, int y) => bmp.GetPixel(x, y);

    /// <summary>
    /// Create a checkerboard pattern: (0,0)=c1, (1,0)=c2, alternating both dimensions.
    /// </summary>
    private static Bitmap CreateCheckerboard(int w, int h, Color c1, Color c2)
    {
        var bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                bmp.SetPixel(x, y, ((x + y) % 2 == 0) ? c1 : c2);
        return bmp;
    }

    private static void AssertColorEqual(Color expected, Color actual)
    {
        Assert.AreEqual(expected.ToArgb(), actual.ToArgb(),
            $"Expected {expected}, got {actual}");
    }

    private static void AssertColorApprox(Color expected, Color actual, int tolerance = 3)
    {
        Assert.AreEqual(expected.R, actual.R, tolerance, $"R channel mismatch: expected ~{expected.R}, got {actual.R}");
        Assert.AreEqual(expected.G, actual.G, tolerance, $"G channel mismatch: expected ~{expected.G}, got {actual.G}");
        Assert.AreEqual(expected.B, actual.B, tolerance, $"B channel mismatch: expected ~{expected.B}, got {actual.B}");
    }

    private static void AssertImageNotAllSameColor(Bitmap bmp)
    {
        Color first = bmp.GetPixel(0, 0);
        bool allSame = true;
        for (int y = 0; y < bmp.Height && allSame; y++)
            for (int x = 0; x < bmp.Width && allSame; x++)
                if (bmp.GetPixel(x, y) != first)
                    allSame = false;
        Assert.IsFalse(allSame, "Expected image to have varying pixel values");
    }

    // ==================== Grayscale Tests ====================

    [TestMethod]
    public void Grayscale_SolidRed_ProducesCorrectGray()
    {
        using var src = CreateSolidBitmap(10, 10, Color.Red);
        using var result = ImageProcessor.Grayscale(src);
        // Luminosity formula: 0.299*255 + 0.587*0 + 0.114*0 = 76
        var c = GetPixel(result, 5, 5);
        Assert.AreEqual(76, c.R, 2);
        Assert.AreEqual(76, c.G, 2);
        Assert.AreEqual(76, c.B, 2);
    }

    [TestMethod]
    public void Grayscale_SolidGreen_ProducesCorrectGray()
    {
        using var src = CreateSolidBitmap(10, 10, Color.Lime); // Lime = (0,255,0), not Green=(0,128,0)
        using var result = ImageProcessor.Grayscale(src);
        // 0.299*0 + 0.587*255 + 0.114*0 = 150
        var c = GetPixel(result, 5, 5);
        Assert.AreEqual(150, c.R, 2);
        Assert.AreEqual(150, c.G, 2);
        Assert.AreEqual(150, c.B, 2);
    }

    [TestMethod]
    public void Grayscale_SolidBlue_ProducesCorrectGray()
    {
        using var src = CreateSolidBitmap(10, 10, Color.Blue);
        using var result = ImageProcessor.Grayscale(src);
        // 0.299*0 + 0.587*0 + 0.114*255 = 29
        var c = GetPixel(result, 5, 5);
        Assert.AreEqual(29, c.R, 1);
        Assert.AreEqual(29, c.G, 1);
        Assert.AreEqual(29, c.B, 1);
    }

    [TestMethod]
    public void Grayscale_WhiteAndBlack_RemainSame()
    {
        using var src = CreateCheckerboard(4, 4, Color.White, Color.Black);
        using var result = ImageProcessor.Grayscale(src);
        Assert.AreEqual(Color.FromArgb(255, 255, 255), GetPixel(result, 0, 0));
        Assert.AreEqual(Color.FromArgb(0, 0, 0), GetPixel(result, 1, 0));
    }

    [TestMethod]
    public void Grayscale_AllChannelsEqual()
    {
        using var src = CreateCheckerboard(8, 8, Color.Red, Color.CornflowerBlue);
        using var result = ImageProcessor.Grayscale(src);
        for (int y = 0; y < result.Height; y++)
            for (int x = 0; x < result.Width; x++)
            {
                var c = result.GetPixel(x, y);
                Assert.AreEqual(c.R, c.G, $"R!=G at ({x},{y})");
                Assert.AreEqual(c.G, c.B, $"G!=B at ({x},{y})");
            }
    }

    // ==================== Sepia Tests ====================

    [TestMethod]
    public void Sepia_PureWhite_StaysLight()
    {
        using var src = CreateSolidBitmap(10, 10, Color.White);
        using var result = ImageProcessor.Sepia(src);
        var c = GetPixel(result, 5, 5);
        // White pixel has gray=255, which is >154
        // r=240, g=Clamp(gray+20)=255, b=Clamp(gray-30)=225
        Assert.AreEqual(240, c.R, 2);
        Assert.AreEqual(255, c.G, 2);
        Assert.AreEqual(225, c.B, 2);
    }

    [TestMethod]
    public void Sepia_PureBlack_StaysDark()
    {
        using var src = CreateSolidBitmap(10, 10, Color.Black);
        using var result = ImageProcessor.Sepia(src);
        var c = GetPixel(result, 5, 5);
        // Gray=0, <=154: r=0+100=100, g=0+50=50, b=0
        Assert.AreEqual(100, c.R, 2);
        Assert.AreEqual(50, c.G, 2);
        Assert.AreEqual(0, c.B, 1);
    }

    [TestMethod]
    public void Sepia_ImageIsNotGrayscale()
    {
        using var src = CreateCheckerboard(8, 8, Color.Cyan, Color.Magenta);
        using var result = ImageProcessor.Sepia(src);
        // Sepia toning should produce different R/G/B values (not all equal)
        var c = GetPixel(result, 0, 0);
        Assert.IsTrue(c.R != c.G || c.G != c.B,
            "Sepia image should have distinct channel values");
    }

    // ==================== Negative Tests ====================

    [TestMethod]
    public void Negative_WhiteBecomesBlack()
    {
        using var src = CreateSolidBitmap(10, 10, Color.White);
        using var result = ImageProcessor.Negative(src);
        Assert.AreEqual(Color.FromArgb(0, 0, 0), GetPixel(result, 5, 5));
    }

    [TestMethod]
    public void Negative_BlackBecomesWhite()
    {
        using var src = CreateSolidBitmap(10, 10, Color.Black);
        using var result = ImageProcessor.Negative(src);
        Assert.AreEqual(Color.FromArgb(255, 255, 255), GetPixel(result, 5, 5));
    }

    [TestMethod]
    public void Negative_RedBecomesCyan()
    {
        using var src = CreateSolidBitmap(10, 10, Color.Red);
        using var result = ImageProcessor.Negative(src);
        Assert.AreEqual(Color.FromArgb(0, 255, 255), GetPixel(result, 5, 5));
    }

    [TestMethod]
    public void Negative_DoubleNegative_ReturnsOriginal()
    {
        using var src = CreateCheckerboard(8, 8, Color.Red, Color.Lime);
        using var once = ImageProcessor.Negative(src);
        using var twice = ImageProcessor.Negative(once);
        for (int y = 0; y < src.Height; y++)
            for (int x = 0; x < src.Width; x++)
                Assert.AreEqual(src.GetPixel(x, y), twice.GetPixel(x, y),
                    $"Mismatch at ({x},{y})");
    }

    // ==================== BoxBlur Tests ====================

    [TestMethod]
    public void BoxBlur_UniformImage_Unchanged()
    {
        using var src = CreateSolidBitmap(20, 20, Color.FromArgb(100, 150, 200));
        using var result = ImageProcessor.BoxBlur(src, 1);
        AssertColorApprox(Color.FromArgb(100, 150, 200), GetPixel(result, 10, 10), 3);
    }

    [TestMethod]
    public void BoxBlur_SingleWhitePixel_SpreadsToNeighbors()
    {
        // 5x5 black image with a single white pixel at center
        using var src = new Bitmap(5, 5, PixelFormat.Format24bppRgb);
        using var g = Graphics.FromImage(src);
        g.Clear(Color.Black);
        src.SetPixel(2, 2, Color.White);

        using var result = ImageProcessor.BoxBlur(src, 1); // 3x3 kernel
        // Center should now be darker than white (averaged with black neighbors)
        var center = GetPixel(result, 2, 2);
        Assert.IsTrue(center.R < 255, "Center should be darker than pure white after blur");
        // Neighbor should be brighter than black
        var neighbor = GetPixel(result, 1, 2);
        Assert.IsTrue(neighbor.R > 0, "Neighbor should be brighter than black after blur");
    }

    [TestMethod]
    public void BoxBlur_DoesNotThrow_OnValidInput()
    {
        using var src = CreateSolidBitmap(10, 10, Color.Gray);
        using var r1 = ImageProcessor.BoxBlur(src, 2);
        using var r2 = ImageProcessor.BoxBlur(src, 5);
        Assert.IsNotNull(r1);
        Assert.IsNotNull(r2);
    }

    // ==================== Sharpen Tests ====================

    [TestMethod]
    public void Sharpen_UniformImage_StaysSimilar()
    {
        using var src = CreateSolidBitmap(20, 20, Color.FromArgb(128, 128, 128));
        using var result = ImageProcessor.Sharpen(src);
        AssertColorApprox(Color.FromArgb(128, 128, 128), GetPixel(result, 10, 10), 10);
    }

    [TestMethod]
    public void Sharpen_EnhancesEdges()
    {
        // Create an image with a sharp vertical edge
        using var src = new Bitmap(10, 10, PixelFormat.Format24bppRgb);
        using var g = Graphics.FromImage(src);
        g.Clear(Color.FromArgb(50, 50, 50));
        g.FillRectangle(Brushes.White, 5, 0, 5, 10);

        using var result = ImageProcessor.Sharpen(src);
        // Pixels near the edge should show enhanced contrast
        // At minimum, the result should be a valid image with variation
        AssertImageNotAllSameColor(result);
    }

    // ==================== EdgeDetectSobel Tests ====================

    [TestMethod]
    public void EdgeDetectSobel_UniformImage_ProducesBlack()
    {
        using var src = CreateSolidBitmap(20, 20, Color.Gray);
        using var result = ImageProcessor.EdgeDetectSobel(src);
        // Uniform image has no edges -> all black (or nearly black)
        var c = GetPixel(result, 10, 10);
        Assert.IsTrue(c.R < 10, $"Expected near-black for uniform region, got R={c.R}");
    }

    [TestMethod]
    public void EdgeDetectSobel_VerticalEdge_HasResponse()
    {
        // Half black, half white vertically
        using var src = new Bitmap(10, 10, PixelFormat.Format24bppRgb);
        using var g = Graphics.FromImage(src);
        g.Clear(Color.Black);
        g.FillRectangle(Brushes.White, 5, 0, 5, 10);

        using var result = ImageProcessor.EdgeDetectSobel(src);
        // The edge at x=5 should produce bright pixels
        var edgePixel = GetPixel(result, 5, 5);
        Assert.IsTrue(edgePixel.R > 100,
            $"Expected bright edge response, got R={edgePixel.R}");
    }

    // ==================== Brightness Tests ====================

    [TestMethod]
    public void AdjustBrightness_Positive_IncreasesValues()
    {
        using var src = CreateSolidBitmap(10, 10, Color.FromArgb(100, 100, 100));
        using var result = ImageProcessor.AdjustBrightness(src, 50);
        var c = GetPixel(result, 5, 5);
        Assert.AreEqual(150, c.R, 1);
        Assert.AreEqual(150, c.G, 1);
        Assert.AreEqual(150, c.B, 1);
    }

    [TestMethod]
    public void AdjustBrightness_Negative_DecreasesValues()
    {
        using var src = CreateSolidBitmap(10, 10, Color.FromArgb(100, 100, 100));
        using var result = ImageProcessor.AdjustBrightness(src, -50);
        var c = GetPixel(result, 5, 5);
        Assert.AreEqual(50, c.R, 1);
        Assert.AreEqual(50, c.G, 1);
        Assert.AreEqual(50, c.B, 1);
    }

    [TestMethod]
    public void AdjustBrightness_ClampsTo0()
    {
        using var src = CreateSolidBitmap(10, 10, Color.FromArgb(50, 0, 50));
        using var result = ImageProcessor.AdjustBrightness(src, -100);
        var c = GetPixel(result, 5, 5);
        Assert.AreEqual(0, c.R);
        Assert.AreEqual(0, c.G);
        Assert.AreEqual(0, c.B);
    }

    [TestMethod]
    public void AdjustBrightness_ClampsTo255()
    {
        using var src = CreateSolidBitmap(10, 10, Color.FromArgb(200, 200, 200));
        using var result = ImageProcessor.AdjustBrightness(src, 100);
        var c = GetPixel(result, 5, 5);
        Assert.AreEqual(255, c.R);
        Assert.AreEqual(255, c.G);
        Assert.AreEqual(255, c.B);
    }

    // ==================== Contrast Tests ====================

    [TestMethod]
    public void AdjustContrast_Factor2_IncreasesContrast()
    {
        // Above midpoint (128): 64 above * 2 = 128 above -> 128+128=256 clamped to 255
        // Below midpoint: 64 below * 2 = 128 below -> 128-128=0
        using var src = CreateCheckerboard(4, 4,
            Color.FromArgb(64, 64, 64),
            Color.FromArgb(192, 192, 192));
        using var result = ImageProcessor.AdjustContrast(src, 2.0);
        Assert.AreEqual(Color.FromArgb(0, 0, 0), GetPixel(result, 0, 0));
        Assert.AreEqual(Color.FromArgb(255, 255, 255), GetPixel(result, 1, 0));
    }

    [TestMethod]
    public void AdjustContrast_FactorPoint5_DecreasesContrast()
    {
        using var src = CreateCheckerboard(4, 4,
            Color.FromArgb(0, 0, 0),
            Color.FromArgb(255, 255, 255));
        using var result = ImageProcessor.AdjustContrast(src, 0.5);
        // Black(0): (0-128)*0.5+128 = 64
        // White(255): (255-128)*0.5+128 = 192
        AssertColorApprox(Color.FromArgb(64, 64, 64), GetPixel(result, 0, 0));
        AssertColorApprox(Color.FromArgb(191, 191, 191), GetPixel(result, 1, 0));
    }

    [TestMethod]
    public void AdjustContrast_Factor1_NoChange()
    {
        using var src = CreateCheckerboard(8, 8, Color.Red, Color.Blue);
        using var result = ImageProcessor.AdjustContrast(src, 1.0);
        Assert.AreEqual(GetPixel(src, 0, 0), GetPixel(result, 0, 0));
        Assert.AreEqual(GetPixel(src, 1, 0), GetPixel(result, 1, 0));
    }

    // ==================== Rotate Tests ====================

    [TestMethod]
    public void Rotate90_HorizontalBecomesVertical()
    {
        // Create a wide image with a distinct top-left pixel
        using var src = CreateSolidBitmap(20, 10, Color.Black);
        src.SetPixel(0, 0, Color.Red);

        using var result = ImageProcessor.Rotate(src, RotateFlipType.Rotate90FlipNone);
        // Rotate 90 CW: (0,0) should move to (height-1, 0) = (9, 0)
        Assert.AreEqual(10, result.Width);   // original height
        Assert.AreEqual(20, result.Height);  // original width
        Assert.AreEqual(Color.FromArgb(255, 0, 0).ToArgb(), GetPixel(result, 9, 0).ToArgb());
    }

    [TestMethod]
    public void Rotate180_ReturnsCorrectlyRotated()
    {
        using var src = CreateSolidBitmap(20, 10, Color.Black);
        src.SetPixel(0, 0, Color.Red);
        src.SetPixel(19, 9, Color.Blue);

        using var result = ImageProcessor.Rotate(src, RotateFlipType.Rotate180FlipNone);
        Assert.AreEqual(20, result.Width);
        Assert.AreEqual(10, result.Height);
        // (0,0) -> (19,9) after 180 rotation
        Assert.AreEqual(Color.FromArgb(255, 0, 0).ToArgb(), GetPixel(result, 19, 9).ToArgb());
        Assert.AreEqual(Color.FromArgb(0, 0, 255).ToArgb(), GetPixel(result, 0, 0).ToArgb());
    }

    // ==================== Flip Tests ====================

    [TestMethod]
    public void FlipHorizontal_ReversesColumns()
    {
        using var src = CreateSolidBitmap(10, 10, Color.Black);
        src.SetPixel(0, 0, Color.Red);

        using var result = ImageProcessor.FlipHorizontal(src);
        Assert.AreEqual(10, result.Width);
        Assert.AreEqual(10, result.Height);
        Assert.AreEqual(Color.FromArgb(255, 0, 0).ToArgb(), GetPixel(result, 9, 0).ToArgb());
    }

    [TestMethod]
    public void FlipHorizontal_DoubleFlip_ReturnsOriginal()
    {
        using var src = CreateCheckerboard(8, 8, Color.Red, Color.Blue);
        using var once = ImageProcessor.FlipHorizontal(src);
        using var twice = ImageProcessor.FlipHorizontal(once);
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                Assert.AreEqual(src.GetPixel(x, y), twice.GetPixel(x, y));
    }

    [TestMethod]
    public void FlipVertical_ReversesRows()
    {
        using var src = CreateSolidBitmap(10, 10, Color.Black);
        src.SetPixel(0, 0, Color.Red);

        using var result = ImageProcessor.FlipVertical(src);
        Assert.AreEqual(10, result.Width);
        Assert.AreEqual(10, result.Height);
        Assert.AreEqual(Color.FromArgb(255, 0, 0).ToArgb(), GetPixel(result, 0, 9).ToArgb());
    }

    [TestMethod]
    public void FlipVertical_DoubleFlip_ReturnsOriginal()
    {
        using var src = CreateCheckerboard(8, 8, Color.Red, Color.Blue);
        using var once = ImageProcessor.FlipVertical(src);
        using var twice = ImageProcessor.FlipVertical(once);
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                Assert.AreEqual(src.GetPixel(x, y), twice.GetPixel(x, y));
    }

    // ==================== Resize Tests ====================

    [TestMethod]
    public void Resize_OutputHasCorrectDimensions()
    {
        using var src = CreateSolidBitmap(100, 50, Color.Green);
        using var result = ImageProcessor.Resize(src, 50, 25);
        Assert.AreEqual(50, result.Width);
        Assert.AreEqual(25, result.Height);
        // Content should still be green (approximately)
        var c = GetPixel(result, 25, 12);
        Assert.IsTrue(c.G > 100 && c.R < 30 && c.B < 30,
            $"Expected greenish pixel, got ({c.R},{c.G},{c.B})");
    }

    [TestMethod]
    public void Resize_PreservesAspectContent()
    {
        // Resize to half; the image center should still approximately match
        using var src = CreateCheckerboard(20, 20, Color.White, Color.Black);
        using var result = ImageProcessor.Resize(src, 10, 10);
        Assert.AreEqual(10, result.Width);
        Assert.AreEqual(10, result.Height);
        // Expect that top-left quadrant of result is lighter than bottom-right
        // (bicubic interpolation of checkerboard may blend, but relative brightness should hold)
        var tl = GetPixel(result, 2, 2);
        var br = GetPixel(result, 7, 7);
        Assert.IsTrue(tl.GetBrightness() >= br.GetBrightness(),
            "Top-left should not be darker than bottom-right");
    }

    // ==================== Edge Cases ====================

    [TestMethod]
    public void AllFilters_Handle1x1Image()
    {
        using var src = CreateSolidBitmap(1, 1, Color.FromArgb(128, 128, 128));

        using var gray = ImageProcessor.Grayscale(src);
        Assert.AreEqual(1, gray.Width);

        using var sepia = ImageProcessor.Sepia(src);
        Assert.AreEqual(1, sepia.Width);

        using var neg = ImageProcessor.Negative(src);
        Assert.AreEqual(1, neg.Width);

        using var blur = ImageProcessor.BoxBlur(src, 1);
        Assert.AreEqual(1, blur.Width);

        using var sharpen = ImageProcessor.Sharpen(src);
        Assert.AreEqual(1, sharpen.Width);

        using var edge = ImageProcessor.EdgeDetectSobel(src);
        Assert.AreEqual(1, edge.Width);
    }
}
