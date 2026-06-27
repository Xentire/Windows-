# Testing Report: ImageProcessingTool

## 1. TDD Approach

This project follows Test-Driven Development (TDD): tests were written **before** the implementation code. The development cycle followed Red → Green → Refactor:

1. **Red**: Write a failing test that defines the expected behavior
2. **Green**: Implement the minimum code to make the test pass
3. **Refactor**: Clean up code while keeping tests green

## 2. Test Suite Overview

| Category | Test Count | Coverage |
|----------|-----------|----------|
| Grayscale | 5 | All channel combinations, boundary values |
| Sepia | 3 | Light/dark tones, color distinctness |
| Negative | 4 | Black/white inversion, double-negative identity |
| BoxBlur | 3 | Uniform image, single pixel spread, boundary |
| Sharpen | 2 | Uniform image, edge enhancement |
| Edge Detection | 2 | Uniform (no edges) → black, vertical edge response |
| Brightness | 4 | Positive/negative offset, 0 and 255 clamping |
| Contrast | 3 | Factor 2.0, 0.5, 1.0 identity |
| Rotate | 2 | 90° dimension swap, 180° corner mapping |
| Flip Horizontal | 2 | Column reversal, double-flip identity |
| Flip Vertical | 2 | Row reversal, double-flip identity |
| Resize | 2 | Dimension correctness, content preservation |
| Edge Cases | 1 | 1x1 image for all filters |
| **Total** | **36** | |

## 3. Test Results

```
Test Run Summary:
  Passed:  36
  Failed:   0
  Skipped:  0
  Duration: ~236 ms
```

All 36 unit tests pass successfully on .NET 8.0 Windows.

## 4. Key Test Cases

### Grayscale Luminosity Verification

```csharp
[TestMethod]
public void Grayscale_SolidRed_ProducesCorrectGray()
{
    using var src = CreateSolidBitmap(10, 10, Color.Red);
    using var result = ImageProcessor.Grayscale(src);
    // Luminosity: 0.299*255 + 0.587*0 + 0.114*0 = 76
    var c = GetPixel(result, 5, 5);
    Assert.AreEqual(76, c.R, 2);
}
```

This verifies the perceptually-weighted luminosity formula (ITU-R BT.601), confirming correct channel weighting.

### Double-Negative Identity

```csharp
[TestMethod]
public void Negative_DoubleNegative_ReturnsOriginal()
{
    using var src = CreateCheckerboard(8, 8, Color.Red, Color.Lime);
    using var once = ImageProcessor.Negative(src);
    using var twice = ImageProcessor.Negative(once);
    // Every pixel of twice should equal src
    for (int y = 0; y < src.Height; y++)
        for (int x = 0; x < src.Width; x++)
            Assert.AreEqual(src.GetPixel(x, y), twice.GetPixel(x, y));
}
```

This test verifies mathematical correctness of the invert operation via the identity property.

### Edge Detection on Uniform Images

```csharp
[TestMethod]
public void EdgeDetectSobel_UniformImage_ProducesBlack()
{
    using var src = CreateSolidBitmap(20, 20, Color.Gray);
    using var result = ImageProcessor.EdgeDetectSobel(src);
    var c = GetPixel(result, 10, 10);
    Assert.IsTrue(c.R < 10, "Uniform image should produce no edges");
}
```

Verifies the Sobel operator produces zero gradient response on regions with constant intensity — a fundamental property of edge detection.

## 5. TDD Effectiveness Summary

TDD provided several benefits during this project:
- **Early bug detection**: The `Color.Green` vs `Color.Lime` confusion was caught immediately
- **Regression safety**: Refactoring `ApplyPixelFunc` from managed to unsafe code was validated by existing tests
- **Design guidance**: Writing tests first forced clear API design (pure functions, consistent signatures)
- **Documentation**: Tests serve as executable documentation of algorithm behavior
