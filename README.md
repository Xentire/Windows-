# ImageProcessingTool - 图像处理工具

基于 C# WinForms 开发的 Windows 图像处理应用程序，Windows程序设计课程期末项目。

## 功能

- **滤镜**：灰度、怀旧、反色、盒式模糊、锐化、Sobel边缘检测
- **调整**：亮度、对比度
- **变换**：旋转（90°/180°/270°）、翻转（水平/垂直）、缩放
- **撤销/重做**：最多20步历史记录
- **文件支持**：JPG、PNG、BMP、GIF、TIFF

## 环境要求

- Windows 10/11
- .NET 8 SDK

## 构建与运行

```bash
dotnet build
dotnet run --project src/ImageProcessingTool
```

## 运行测试

```bash
dotnet test
```

## 项目结构

```
├── src/ImageProcessingTool/     # 主应用程序
│   ├── Program.cs               # 入口
│   ├── MainForm.cs              # 主窗口 + 辅助对话框
│   ├── ImageProcessor.cs        # 核心图像处理算法
│   ├── UndoRedoManager.cs       # 撤销/重做（位图快照栈）
│   ├── ImageFileHandler.cs      # 文件读写工具
│   └── AboutDialog.cs           # 关于对话框
├── tests/ImageProcessingTool.Tests/
│   └── ImageProcessorTests.cs   # 单元测试（36个用例）
├── Project_Proposal.md          # 项目方案
└── Testing_Report.md            # 测试报告
```
