# ImageProcessingTool

A Windows Forms image processing application developed for the Windows Programming course.

## Features

- **Filters**: Grayscale, Sepia, Negative, Box Blur, Sharpen, Edge Detection (Sobel)
- **Adjustments**: Brightness, Contrast
- **Transformations**: Rotate (90°/180°/270°), Flip (Horizontal/Vertical), Resize
- **Undo/Redo**: Up to 20 steps history
- **File Support**: JPG, PNG, BMP, GIF, TIFF

## Requirements

- Windows 10/11
- .NET 8 SDK

## Build & Run

```bash
dotnet build
dotnet run --project src/ImageProcessingTool
```

## Run Tests

```bash
dotnet test
```

## Project Structure

```
├── src/ImageProcessingTool/     # Main application
│   ├── Program.cs               # Entry point
│   ├── MainForm.cs              # Main window + helper dialogs
│   ├── ImageProcessor.cs        # Core image processing algorithms
│   ├── UndoRedoManager.cs       # Undo/Redo via bitmap snapshot stack
│   ├── ImageFileHandler.cs      # File I/O utilities
│   └── AboutDialog.cs           # About dialog
├── tests/ImageProcessingTool.Tests/
│   └── ImageProcessorTests.cs   # Unit tests (36 test cases)
├── Project_Proposal.md          # Project proposal
└── Testing_Report.md            # Testing report
```
