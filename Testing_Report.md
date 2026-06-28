# 测试报告：ImageProcessingTool

## 1. TDD 开发过程

本项目遵循测试驱动开发（TDD）方法：测试在实现代码之前编写。开发周期为 红 → 绿 → 重构：

1. **红**：编写失败的测试，定义预期行为
2. **绿**：实现最少代码使测试通过
3. **重构**：在保持测试绿色的前提下优化代码

## 2. 测试用例清单

| 类别 | 测试数 | 覆盖内容 |
|------|--------|----------|
| Grayscale 灰度 | 5 | 纯红/绿/蓝/白/黑的灰度值验证，通道一致性 |
| Sepia 怀旧 | 3 | 亮色调、暗色调、颜色区分度 |
| Negative 反色 | 4 | 黑白反转、双重反色恒等、红→青 |
| BoxBlur 模糊 | 3 | 均匀图不变、单白像素扩散、边界 |
| Sharpen 锐化 | 2 | 均匀图、边缘增强 |
| Edge Detect 边缘检测 | 2 | 均匀图→全黑、垂直边缘响应 |
| Brightness 亮度 | 4 | 正负偏移、0和255钳位 |
| Contrast 对比度 | 3 | 2.0倍、0.5倍、1.0倍恒等 |
| Rotate 旋转 | 2 | 90°尺寸交换、180°对角映射 |
| Flip H 水平翻转 | 2 | 列反转、双重翻转恒等 |
| Flip V 垂直翻转 | 2 | 行反转、双重翻转恒等 |
| Resize 缩放 | 2 | 尺寸正确、内容保留 |
| 边界条件 | 1 | 1x1图像全部滤镜 |
| **合计** | **36** | |

## 3. 测试结果

```
测试运行摘要:
  通过:  36
  失败:   0
  跳过:   0
  耗时: ~236 ms
```

全部 36 个单元测试在 .NET 8.0 Windows 上通过。

## 4. 关键测试用例分析

### 灰度亮度公式验证

```csharp
[TestMethod]
public void Grayscale_SolidRed_ProducesCorrectGray()
{
    using var src = CreateSolidBitmap(10, 10, Color.Red);
    using var result = ImageProcessor.Grayscale(src);
    // 亮度公式：0.299×255 + 0.587×0 + 0.114×0 = 76
    var c = GetPixel(result, 5, 5);
    Assert.AreEqual(76, c.R, 2);
}
```

验证了 ITU-R BT.601 感知加权亮度公式的正确性。

### 双重反色恒等性

```csharp
[TestMethod]
public void Negative_DoubleNegative_ReturnsOriginal()
{
    using var src = CreateCheckerboard(8, 8, Color.Red, Color.Lime);
    using var once = ImageProcessor.Negative(src);
    using var twice = ImageProcessor.Negative(once);
    // twice 的每个像素应与 src 完全一致
    for (int y = 0; y < src.Height; y++)
        for (int x = 0; x < src.Width; x++)
            Assert.AreEqual(src.GetPixel(x, y), twice.GetPixel(x, y));
}
```

通过恒等性质验证反色算法的数学正确性。

### 均匀图像边缘检测

```csharp
[TestMethod]
public void EdgeDetectSobel_UniformImage_ProducesBlack()
{
    using var src = CreateSolidBitmap(20, 20, Color.Gray);
    using var result = ImageProcessor.EdgeDetectSobel(src);
    var c = GetPixel(result, 10, 10);
    Assert.IsTrue(c.R < 10, "均匀区域不应检测到边缘");
}
```

验证 Sobel 算子在无变化区域产生零响应——这是边缘检测的基本性质。

## 5. TDD 实践效果总结

TDD 在本项目中带来了以下收益：
- **早期缺陷发现**：Color.Green 与 Color.Lime 的混淆在第一时间被测试捕获
- **回归安全保障**：将 ApplyPixelFunc 从 managed 重构为 unsafe 时，已有测试全部通过
- **设计引导**：先写测试促使了清晰的 API 设计（纯函数、一致的接口签名）
- **可执行文档**：测试用例即为算法行为的精确文档
