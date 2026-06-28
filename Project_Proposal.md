# 项目方案：ImageProcessingTool

## 1. 项目背景

数字图像处理已成为软件开发中的基础能力。本项目旨在使用 C# WinForms 构建一款桌面图像处理工具，在实践 Windows GUI 编程的同时交付一个实用、易用的应用程序。

## 2. 功能需求

### 核心功能

| 类别 | 功能 | 说明 |
|------|------|------|
| 文件 | 打开 | 支持 JPG、PNG、BMP、GIF、TIFF |
| 文件 | 另存为 | 导出处理后的图像 |
| 滤镜 | 灰度 | 基于亮度权重的灰度转换 |
| 滤镜 | 怀旧 | 暖色调复古照片效果 |
| 滤镜 | 反色 | 颜色反转 |
| 滤镜 | 盒式模糊 | 基于卷积的图像平滑 |
| 滤镜 | 锐化 | 拉普拉斯锐化核 |
| 滤镜 | 边缘检测 | Sobel算子边缘提取 |
| 调整 | 亮度 | 线性亮度偏移（-100 到 +100） |
| 调整 | 对比度 | 对比度因子调整（0x 到 3x） |
| 变换 | 旋转 | 90°/180°/270° 旋转 |
| 变换 | 翻转 | 水平和垂直镜像 |
| 变换 | 缩放 | 任意尺寸，支持纵横比锁定 |
| 编辑 | 撤销/重做 | 位图快照栈，最多20步 |

### 非功能需求
- 响应式UI：大图异步处理，带进度条
- 内存安全：规范的 IDisposable 模式管理 GDI+ 资源
- 可测试性：纯函数的图像处理核心，36个单元测试

## 3. 技术选型

| 技术 | 理由 |
|------|------|
| **.NET 8** | LTS 版本，成熟的 WinForms 支持 |
| **WinForms** | 课程要求，原生 Windows GUI，控件丰富 |
| **C#** | 课程语言，现代语法（unsafe、async/await） |
| **LockBits + unsafe** | 比 GetPixel/SetPixel 快 5-10 倍 |
| **MSTest** | .NET 内置测试框架，零配置 |
| **TDD** | 先写测试后写实现，36个测试覆盖全部算法 |

## 4. 系统架构

```
┌─────────────────────────────────────┐
│            MainForm (UI)             │
│  MenuStrip / ToolStrip / StatusStrip │
└──────────┬──────────────────────────┘
           │ 调用
    ┌──────┼──────┬──────────────┐
    ▼      ▼      ▼              ▼
┌──────┐ ┌─────┐ ┌────────┐ ┌──────────┐
│Image │ │Undo │ │Image   │ │About     │
│Proces│ │Redo │ │File    │ │Dialog    │
│sor   │ │Mgr  │ │Handler │ │          │
└──────┘ └─────┘ └────────┘ └──────────┘
```

**ImageProcessor**：所有方法为静态纯函数（输入 Bitmap → 新 Bitmap），使用 LockBits + unsafe 指针实现高性能处理，支持 IProgress<int> 进度报告。

**UndoRedoManager**：位图快照栈，可配置深度（默认20）。每次操作前保存当前状态；双栈实现撤销/重做切换。

**ImageFileHandler**：使用 MemoryStream 中转避免文件锁定。提供格式检测、文件大小格式化、对话框过滤器生成。

## 5. 开发环境

- **操作系统**：Windows 11
- **IDE**：Visual Studio Code / 命令行
- **SDK**：.NET 8.0
- **语言**：C# 12
- **测试**：MSTest
- **AI辅助**：Claude Code（全程使用）
- **版本管理**：Git

## 6. 仓库地址

```bash
git clone https://github.com/Xentire/Windows-.git
cd ImageProcessingTool
dotnet build
dotnet test
dotnet run --project src/ImageProcessingTool
```
