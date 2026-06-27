# Project Proposal: ImageProcessingTool

## 1. Project Background

With the ubiquity of digital images in modern applications, image processing has become a fundamental skill in software development. This project aims to build a desktop image processing tool using C# WinForms, demonstrating proficiency in Windows GUI programming while delivering a practical, user-friendly application.

## 2. Functional Requirements

### Core Features

| Category | Feature | Description |
|----------|---------|-------------|
| File I/O | Open | Support JPG, PNG, BMP, GIF, TIFF |
| File I/O | Save As | Export processed images in common formats |
| Filters | Grayscale | Luminosity-based grayscale conversion |
| Filters | Sepia | Warm-toned vintage photo effect |
| Filters | Negative | Color inversion |
| Filters | Box Blur | Convolution-based image smoothing |
| Filters | Sharpen | Laplacian sharpening kernel |
| Filters | Edge Detection | Sobel operator for edge extraction |
| Adjustments | Brightness | Linear brightness offset (-100 to +100) |
| Adjustments | Contrast | Contrast factor adjustment (0x to 3x) |
| Transform | Rotate | 90°/180°/270° rotation |
| Transform | Flip | Horizontal and vertical mirror |
| Transform | Resize | Arbitrary dimensions with aspect ratio lock |
| Edit | Undo/Redo | Up to 20-step history via bitmap snapshot stack |

### Non-Functional Requirements
- Responsive UI: large images processed asynchronously with progress bar
- Memory safety: proper IDisposable patterns for native GDI+ resources
- Testability: pure-function image processing core with 36 unit tests

## 3. Technology Selection

| Technology | Rationale |
|------------|-----------|
| **.NET 8** | LTS release, mature WinForms support, `System.Drawing.Common` for GDI+ |
| **WinForms** | Course requirement; native Windows GUI with rich control set |
| **C#** | Course language; modern syntax (unsafe, async/await, pattern matching) |
| **LockBits + unsafe** | 5-10x performance improvement over GetPixel/SetPixel |
| **MSTest** | Built-in .NET testing framework, minimal configuration |
| **TDD** | Tests written before implementation; 36 tests covering all algorithms |

## 4. System Architecture

```
┌─────────────────────────────────────┐
│            MainForm (UI)             │
│  MenuStrip / ToolStrip / StatusStrip │
└──────────┬──────────────────────────┘
           │ calls
    ┌──────┼──────┬──────────────┐
    ▼      ▼      ▼              ▼
┌──────┐ ┌─────┐ ┌────────┐ ┌──────────┐
│Image │ │Undo │ │Image   │ │About     │
│Proces│ │Redo │ │File    │ │Dialog    │
│sor   │ │Mgr  │ │Handler │ │          │
└──────┘ └─────┘ └────────┘ └──────────┘
   ↑                   ↑
   │ unit tests        │ static utility
   ▼                   ▼
 ImageProcessorTests   System.Drawing
```

**ImageProcessor**: All methods are static pure functions (input Bitmap → new Bitmap). Uses `LockBits` + unsafe pointer arithmetic for performance. Supports `IProgress<int>` for progress reporting.

**UndoRedoManager**: Bitmap snapshot stack with configurable depth (default 20). Pushes current state before each operation; Undo/Redo swap between two stacks.

**ImageFileHandler**: Uses `MemoryStream` to avoid file locking. Provides format detection, size formatting, and file dialog filter generation.

## 5. Development Environment

- **OS**: Windows 11
- **IDE**: Visual Studio Code / CLI
- **SDK**: .NET 8.0
- **Language**: C# 12
- **Testing**: MSTest
- **AI Assistant**: Claude Code (used throughout development)
- **Version Control**: Git

## 6. Clone Address

```bash
git clone https://github.com/Xentire/Windows-.git
cd ImageProcessingTool
dotnet build
dotnet test
dotnet run --project src/ImageProcessingTool
```
