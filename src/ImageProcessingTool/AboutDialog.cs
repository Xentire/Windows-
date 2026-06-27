namespace ImageProcessingTool;

public class AboutDialog : Form
{
    public AboutDialog()
    {
        Text = "About ImageProcessingTool";
        Size = new Size(400, 280);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(20)
        };

        table.Controls.Add(new Label
        {
            Text = "ImageProcessingTool",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        }, 0, 0);

        table.Controls.Add(new Label
        {
            Text = "A Windows Forms image processing application\n" +
                   "developed as a course project for Windows Programming.\n\n" +
                   "Features: Grayscale, Sepia, Negative, Blur, Sharpen,\n" +
                   "Edge Detection, Brightness/Contrast, Rotate, Flip, Resize",
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        }, 0, 1);

        table.Controls.Add(new Label
        {
            Text = ".NET 8 | WinForms | C#",
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        }, 0, 2);

        var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 80 };
        var btnPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Height = 40, Padding = new Padding(0, 0, 0, 0) };
        btnOk.Margin = new Padding((400 - 80 - 40) / 2, 0, 0, 0);
        btnPanel.Controls.Add(btnOk);
        table.Controls.Add(btnPanel, 0, 3);

        Controls.Add(table);
        AcceptButton = btnOk;
    }
}
