using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessingTool;

public class MainForm : Form
{
    private Bitmap? _currentBitmap;
    private string? _currentFilePath;
    private readonly UndoRedoManager _undoRedo = new(20);

    // --- Controls ---
    private MenuStrip _menuStrip = null!;
    private ToolStrip _toolStrip = null!;
    private Panel _imagePanel = null!;
    private PictureBox _pictureBox = null!;
    private StatusStrip _statusStrip = null!;
    private ToolStripStatusLabel _infoLabel = null!;
    private ToolStripProgressBar _progressBar = null!;
    private ToolStripButton _btnUndo = null!;
    private ToolStripButton _btnRedo = null!;

    public MainForm()
    {
        InitializeComponent();
        _undoRedo.StateChanged += (_, _) => UpdateUndoRedoButtons();
        UpdateUndoRedoButtons();
    }

    // ==================== Initialization ====================

    private void InitializeComponent()
    {
        Text = "ImageProcessingTool - Image Processing Tool";
        Size = new Size(1280, 800);
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;

        // --- MenuStrip ---
        _menuStrip = new MenuStrip();

        var fileMenu = new ToolStripMenuItem("&File");
        var miOpen = new ToolStripMenuItem("&Open...", null, OnFileOpen, Keys.Control | Keys.O);
        var miSaveAs = new ToolStripMenuItem("Save &As...", null, OnFileSaveAs, Keys.Control | Keys.S);
        var miExit = new ToolStripMenuItem("E&xit", null, (_, _) => Close(), Keys.Alt | Keys.F4);
        fileMenu.DropDownItems.AddRange([miOpen, miSaveAs, new ToolStripSeparator(), miExit]);

        var editMenu = new ToolStripMenuItem("&Edit");
        var miUndo = new ToolStripMenuItem("&Undo", null, (_, _) => Undo(), Keys.Control | Keys.Z);
        var miRedo = new ToolStripMenuItem("&Redo", null, (_, _) => Redo(), Keys.Control | Keys.Y);
        var miReset = new ToolStripMenuItem("&Reset to Original", null, (_, _) => ResetToOriginal());
        editMenu.DropDownItems.AddRange([miUndo, miRedo, new ToolStripSeparator(), miReset]);

        var filterMenu = new ToolStripMenuItem("Fi&lters");
        filterMenu.DropDownItems.AddRange([
            new ToolStripMenuItem("&Grayscale", null, (_, _) => ApplyFilter(ImageProcessor.Grayscale, "Grayscale")),
            new ToolStripMenuItem("&Sepia", null, (_, _) => ApplyFilter(ImageProcessor.Sepia, "Sepia")),
            new ToolStripMenuItem("&Negative", null, (_, _) => ApplyFilter(ImageProcessor.Negative, "Negative")),
            new ToolStripSeparator(),
            new ToolStripMenuItem("Box &Blur (3px)", null, (_, _) => ApplyFilter((s, p) => ImageProcessor.BoxBlur(s, 1, p), "Box Blur")),
            new ToolStripMenuItem("&Sharpen", null, (_, _) => ApplyFilter(ImageProcessor.Sharpen, "Sharpen")),
            new ToolStripMenuItem("Edge Detection (&Sobel)", null, (_, _) => ApplyFilter(ImageProcessor.EdgeDetectSobel, "Edge Detect"))
        ]);

        var adjustMenu = new ToolStripMenuItem("&Adjustments");
        var miBrightness = new ToolStripMenuItem("&Brightness...", null, (_, _) => OpenBrightnessDialog());
        var miContrast = new ToolStripMenuItem("&Contrast...", null, (_, _) => OpenContrastDialog());
        adjustMenu.DropDownItems.AddRange([miBrightness, miContrast]);

        var transformMenu = new ToolStripMenuItem("&Transform");
        transformMenu.DropDownItems.AddRange([
            new ToolStripMenuItem("Rotate 90° &CW", null, (_, _) => ApplyTransform(ImageProcessor.Rotate, RotateFlipType.Rotate90FlipNone)),
            new ToolStripMenuItem("Rotate &180°", null, (_, _) => ApplyTransform(ImageProcessor.Rotate, RotateFlipType.Rotate180FlipNone)),
            new ToolStripMenuItem("Rotate 90° C&CW", null, (_, _) => ApplyTransform(ImageProcessor.Rotate, RotateFlipType.Rotate270FlipNone)),
            new ToolStripSeparator(),
            new ToolStripMenuItem("Flip &Horizontal", null, (_, _) => ApplySimpleTransform(ImageProcessor.FlipHorizontal)),
            new ToolStripMenuItem("Flip &Vertical", null, (_, _) => ApplySimpleTransform(ImageProcessor.FlipVertical)),
            new ToolStripSeparator(),
            new ToolStripMenuItem("&Resize...", null, (_, _) => OpenResizeDialog())
        ]);

        var helpMenu = new ToolStripMenuItem("&Help");
        helpMenu.DropDownItems.AddRange([
            new ToolStripMenuItem("&About", null, (_, _) => new AboutDialog().ShowDialog(this))
        ]);

        _menuStrip.Items.AddRange([fileMenu, editMenu, filterMenu, adjustMenu, transformMenu, helpMenu]);

        // --- ToolStrip ---
        _toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
        _btnUndo = new ToolStripButton("↩ Undo", null, (_, _) => Undo()) { DisplayStyle = ToolStripItemDisplayStyle.Text };
        _btnRedo = new ToolStripButton("↪ Redo", null, (_, _) => Redo()) { DisplayStyle = ToolStripItemDisplayStyle.Text };
        var btnGrayscale = new ToolStripButton("Grayscale", null, (_, _) => ApplyFilter(ImageProcessor.Grayscale, "Grayscale")) { DisplayStyle = ToolStripItemDisplayStyle.Text };
        var btnSepia = new ToolStripButton("Sepia", null, (_, _) => ApplyFilter(ImageProcessor.Sepia, "Sepia")) { DisplayStyle = ToolStripItemDisplayStyle.Text };
        var btnBlur = new ToolStripButton("Blur", null, (_, _) => ApplyFilter((s, p) => ImageProcessor.BoxBlur(s, 1, p), "Box Blur")) { DisplayStyle = ToolStripItemDisplayStyle.Text };
        var btnRotate = new ToolStripButton("Rotate 90°", null, (_, _) => ApplyTransform(ImageProcessor.Rotate, RotateFlipType.Rotate90FlipNone)) { DisplayStyle = ToolStripItemDisplayStyle.Text };
        var btnFlipH = new ToolStripButton("Flip H", null, (_, _) => ApplySimpleTransform(ImageProcessor.FlipHorizontal)) { DisplayStyle = ToolStripItemDisplayStyle.Text };

        _toolStrip.Items.AddRange([
            new ToolStripButton("Open", null, OnFileOpen) { DisplayStyle = ToolStripItemDisplayStyle.Text },
            new ToolStripButton("Save", null, OnFileSaveAs) { DisplayStyle = ToolStripItemDisplayStyle.Text },
            new ToolStripSeparator(),
            _btnUndo, _btnRedo,
            new ToolStripSeparator(),
            btnGrayscale, btnSepia, btnBlur,
            new ToolStripSeparator(),
            btnRotate, btnFlipH
        ]);

        // --- Image Panel + PictureBox ---
        _imagePanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.FromArgb(40, 40, 40)
        };
        _pictureBox = new PictureBox
        {
            SizeMode = PictureBoxSizeMode.AutoSize,
            Location = new Point(0, 0)
        };
        _imagePanel.Controls.Add(_pictureBox);

        // --- StatusStrip ---
        _statusStrip = new StatusStrip();
        _infoLabel = new ToolStripStatusLabel("No image loaded") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
        _progressBar = new ToolStripProgressBar { Visible = false, Width = 150 };
        _statusStrip.Items.AddRange([_infoLabel, _progressBar]);

        // --- Layout ---
        Controls.Add(_imagePanel);
        Controls.Add(_toolStrip);
        Controls.Add(_menuStrip);
        Controls.Add(_statusStrip);
        MainMenuStrip = _menuStrip;

        // --- Keyboard ---
        KeyDown += (_, e) =>
        {
            if (e.Control && e.KeyCode == Keys.Z) Undo();
            if (e.Control && e.KeyCode == Keys.Y) Redo();
        };
    }

    // ==================== File Operations ====================

    private void OnFileOpen(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Title = "Open Image",
            Filter = ImageFileHandler.BuildFilter()
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            var (bmp, _, _, _, _) = ImageFileHandler.OpenImage(dlg.FileName);
            _currentBitmap?.Dispose();
            _currentBitmap = bmp;
            _currentFilePath = dlg.FileName;
            _undoRedo.Clear();
            UpdatePictureBox();
            UpdateStatusBar();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to open image:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnFileSaveAs(object? sender, EventArgs e)
    {
        if (_currentBitmap == null)
        {
            MessageBox.Show("No image to save.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dlg = new SaveFileDialog
        {
            Title = "Save Image As",
            Filter = ImageFileHandler.BuildFilter(),
            FileName = _currentFilePath != null
                ? Path.GetFileNameWithoutExtension(_currentFilePath) + ".png"
                : "output.png"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            ImageFileHandler.SaveImage(_currentBitmap, dlg.FileName);
            _currentFilePath = dlg.FileName;
            UpdateStatusBar();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save image:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ==================== Edit Operations ====================

    private void Undo()
    {
        if (_currentBitmap == null) return;
        var prev = _undoRedo.Undo(_currentBitmap);
        if (prev != null)
        {
            _currentBitmap.Dispose();
            _currentBitmap = prev;
            UpdatePictureBox();
        }
    }

    private void Redo()
    {
        if (_currentBitmap == null) return;
        var next = _undoRedo.Redo(_currentBitmap);
        if (next != null)
        {
            _currentBitmap.Dispose();
            _currentBitmap = next;
            UpdatePictureBox();
        }
    }

    private void ResetToOriginal()
    {
        if (_currentFilePath == null || !File.Exists(_currentFilePath))
        {
            MessageBox.Show("Original file no longer available.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        try
        {
            var (bmp, _, _, _, _) = ImageFileHandler.OpenImage(_currentFilePath);
            _currentBitmap?.Dispose();
            _currentBitmap = bmp;
            _undoRedo.Clear();
            UpdatePictureBox();
            UpdateStatusBar();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to reset:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ==================== Filter / Transform Operations ====================

    private async void ApplyFilter(Func<Bitmap, IProgress<int>, Bitmap> filter, string name)
    {
        if (_currentBitmap == null) return;
        _undoRedo.PushState(_currentBitmap);
        await RunAsync(filter, name);
    }

    private async void ApplySimpleTransform(Func<Bitmap, Bitmap> transform)
    {
        if (_currentBitmap == null) return;
        _undoRedo.PushState(_currentBitmap);
        await RunAsync(transform);
    }

    private async void ApplyTransform(Func<Bitmap, RotateFlipType, Bitmap> transform, RotateFlipType type)
    {
        if (_currentBitmap == null) return;
        _undoRedo.PushState(_currentBitmap);
        await RunAsync(transform, type);
    }

    private async Task RunAsync(Func<Bitmap, IProgress<int>, Bitmap> filter, string name)
    {
        SetProcessingState(true);
        try
        {
            var source = _currentBitmap!;
            var progress = new Progress<int>(p =>
            {
                _progressBar.Value = p;
                _infoLabel.Text = $"Processing {name}... {p}%";
            });

            _currentBitmap = await Task.Run(() => filter(source, progress));
            UpdatePictureBox();
            UpdateStatusBar();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Processing failed:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetProcessingState(false);
        }
    }

    private async Task RunAsync(Func<Bitmap, Bitmap> transform)
    {
        SetProcessingState(true);
        try
        {
            var source = _currentBitmap!;
            _currentBitmap = await Task.Run(() => transform(source));
            UpdatePictureBox();
            UpdateStatusBar();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Processing failed:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetProcessingState(false);
        }
    }

    private async Task RunAsync(Func<Bitmap, RotateFlipType, Bitmap> transform, RotateFlipType type)
    {
        SetProcessingState(true);
        try
        {
            var source = _currentBitmap!;
            _currentBitmap = await Task.Run(() => transform(source, type));
            UpdatePictureBox();
            UpdateStatusBar();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Processing failed:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetProcessingState(false);
        }
    }

    // ==================== Dialogs ====================

    private void OpenBrightnessDialog()
    {
        if (_currentBitmap == null) return;
        using var dlg = new AdjustmentDialog("Brightness", -100, 100, 0);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        _undoRedo.PushState(_currentBitmap);
        var source = _currentBitmap!;
        _currentBitmap = ImageProcessor.AdjustBrightness(source, dlg.Value);
        UpdatePictureBox();
        UpdateStatusBar();
    }

    private void OpenContrastDialog()
    {
        if (_currentBitmap == null) return;
        using var dlg = new AdjustmentDialog("Contrast", 0, 300, 100,
            v => $"{(v / 100.0):F2}x");
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        _undoRedo.PushState(_currentBitmap);
        var source = _currentBitmap!;
        _currentBitmap = ImageProcessor.AdjustContrast(source, dlg.Value / 100.0);
        UpdatePictureBox();
        UpdateStatusBar();
    }

    private void OpenResizeDialog()
    {
        if (_currentBitmap == null) return;
        using var dlg = new ResizeDialog(_currentBitmap.Width, _currentBitmap.Height);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        _undoRedo.PushState(_currentBitmap);
        var source = _currentBitmap!;
        _currentBitmap = ImageProcessor.Resize(source, dlg.NewWidth, dlg.NewHeight);
        UpdatePictureBox();
        UpdateStatusBar();
    }

    // ==================== UI Helpers ====================

    private void UpdatePictureBox()
    {
        _pictureBox.Image?.Dispose();
        if (_currentBitmap != null)
        {
            _pictureBox.Image = new Bitmap(_currentBitmap); // Clone for display
        }
    }

    private void UpdateStatusBar()
    {
        if (_currentBitmap == null)
        {
            _infoLabel.Text = "No image loaded";
            return;
        }
        _infoLabel.Text = ImageFileHandler.GetImageInfoString(_currentBitmap, _currentFilePath);
    }

    private void UpdateUndoRedoButtons()
    {
        _btnUndo.Enabled = _undoRedo.CanUndo;
        _btnRedo.Enabled = _undoRedo.CanRedo;
    }

    private void SetProcessingState(bool processing)
    {
        _menuStrip.Enabled = !processing;
        _toolStrip.Enabled = !processing;
        _progressBar.Visible = processing;
        _progressBar.Value = 0;
        if (!processing)
        {
            _progressBar.Visible = false;
            UpdateStatusBar();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _currentBitmap?.Dispose();
        _undoRedo.Dispose();
        base.OnFormClosing(e);
    }
}

// ==================== Helper Dialogs ====================

/// <summary>
/// Generic adjustment dialog with a TrackBar for brightness/contrast.
/// </summary>
public class AdjustmentDialog : Form
{
    private readonly TrackBar _trackBar;
    private readonly Label _valueLabel;
    private readonly Func<int, string> _formatValue;
    public int Value => _trackBar.Value;

    public AdjustmentDialog(string title, int min, int max, int initial,
        Func<int, string>? formatValue = null)
    {
        _formatValue = formatValue ?? (v => v.ToString());
        Text = $"Adjust {title}";
        Size = new Size(400, 200);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        _valueLabel = new Label
        {
            Text = _formatValue(initial),
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 50
        };

        _trackBar = new TrackBar
        {
            Minimum = min,
            Maximum = max,
            Value = initial,
            TickFrequency = (max - min) / 10,
            Dock = DockStyle.Top,
            Height = 50
        };
        _trackBar.ValueChanged += (_, _) => _valueLabel.Text = _formatValue(_trackBar.Value);

        var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 80 };
        var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 80 };
        var btnPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Bottom,
            Height = 40,
            Padding = new Padding(10)
        };
        btnPanel.Controls.Add(btnCancel);
        btnPanel.Controls.Add(btnOk);

        Controls.Add(btnPanel);
        Controls.Add(_trackBar);
        Controls.Add(_valueLabel);
        AcceptButton = btnOk;
        CancelButton = btnCancel;
    }
}

/// <summary>
/// Resize dialog with width/height input and aspect ratio lock.
/// </summary>
public class ResizeDialog : Form
{
    private readonly NumericUpDown _numWidth;
    private readonly NumericUpDown _numHeight;
    private readonly CheckBox _chkAspectRatio;
    private readonly int _origWidth;
    private readonly int _origHeight;
    private bool _updating;

    public int NewWidth => (int)_numWidth.Value;
    public int NewHeight => (int)_numHeight.Value;

    public ResizeDialog(int currentWidth, int currentHeight)
    {
        _origWidth = currentWidth;
        _origHeight = currentHeight;
        Text = "Resize Image";
        Size = new Size(320, 200);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(15, 15, 15, 15)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

        table.Controls.Add(new Label { Text = "Width:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);
        _numWidth = new NumericUpDown { Minimum = 1, Maximum = 10000, Value = currentWidth, Dock = DockStyle.Fill };
        table.Controls.Add(_numWidth, 1, 0);

        table.Controls.Add(new Label { Text = "Height:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);
        _numHeight = new NumericUpDown { Minimum = 1, Maximum = 10000, Value = currentHeight, Dock = DockStyle.Fill };
        table.Controls.Add(_numHeight, 1, 1);

        _chkAspectRatio = new CheckBox { Text = "Maintain aspect ratio", Checked = true, Dock = DockStyle.Fill };
        table.SetColumnSpan(_chkAspectRatio, 2);
        table.Controls.Add(_chkAspectRatio, 0, 2);

        var btnPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Height = 40
        };
        var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 80 };
        var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 80 };
        btnPanel.Controls.Add(btnCancel);
        btnPanel.Controls.Add(btnOk);
        table.Controls.Add(btnPanel, 0, 3);
        table.SetColumnSpan(btnPanel, 2);

        Controls.Add(table);
        AcceptButton = btnOk;
        CancelButton = btnCancel;

        _numWidth.ValueChanged += OnWidthChanged;
        _numHeight.ValueChanged += OnHeightChanged;
    }

    private void OnWidthChanged(object? sender, EventArgs e)
    {
        if (_updating || !_chkAspectRatio.Checked) return;
        _updating = true;
        _numHeight.Value = (decimal)((double)_numWidth.Value * _origHeight / _origWidth);
        _updating = false;
    }

    private void OnHeightChanged(object? sender, EventArgs e)
    {
        if (_updating || !_chkAspectRatio.Checked) return;
        _updating = true;
        _numWidth.Value = (decimal)((double)_numHeight.Value * _origWidth / _origHeight);
        _updating = false;
    }
}
