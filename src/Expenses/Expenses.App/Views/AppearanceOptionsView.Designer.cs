#nullable enable

namespace Expenses.App.Views;

partial class AppearanceOptionsView
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _layout = new TableLayoutPanel();
        _fontLabel = new Label();
        _fontPicker = new TableLayoutPanel();
        _fontValue = new TextBox();
        _browseFont = new Button();
        _sizeLabel = new Label();
        _size = new ComboBox();
        _hint = new Label();
        _layout.SuspendLayout();
        _fontPicker.SuspendLayout();
        SuspendLayout();
        _layout.AutoSize = true;
        _layout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _layout.Dock = DockStyle.Top;
        _layout.Padding = new Padding(12);
        _layout.ColumnCount = 2;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layout.RowCount = 3;
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.Controls.Add(_fontLabel, 0, 0);
        _layout.Controls.Add(_fontPicker, 1, 0);
        _layout.Controls.Add(_sizeLabel, 0, 1);
        _layout.Controls.Add(_size, 1, 1);
        _layout.Controls.Add(_hint, 0, 2);
        _layout.SetColumnSpan(_hint, 2);
        _fontLabel.Text = "Interface &font:";
        _fontLabel.AutoSize = true;
        _fontLabel.Anchor = AnchorStyles.Left;
        _fontLabel.Margin = new Padding(3, 8, 8, 8);
        _fontPicker.AutoSize = true;
        _fontPicker.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _fontPicker.Dock = DockStyle.Fill;
        _fontPicker.ColumnCount = 2;
        _fontPicker.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _fontPicker.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _fontPicker.RowCount = 1;
        _fontPicker.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _fontPicker.Controls.Add(_fontValue, 0, 0);
        _fontPicker.Controls.Add(_browseFont, 1, 0);
        _fontValue.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _fontValue.ReadOnly = true;
        _fontValue.MinimumSize = new Size(160, 0);
        _fontValue.AccessibleName = "Interface font";
        _browseFont.Text = "...";
        _browseFont.AutoSize = true;
        _browseFont.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _browseFont.Dock = DockStyle.Fill;
        _browseFont.AccessibleName = "Choose interface font";
        _browseFont.Click += PickFont;
        _sizeLabel.Text = "&Icon size:";
        _sizeLabel.AutoSize = true;
        _sizeLabel.Anchor = AnchorStyles.Left;
        _sizeLabel.Margin = new Padding(3, 8, 8, 8);
        _size.Dock = DockStyle.Fill;
        _size.DropDownStyle = ComboBoxStyle.DropDownList;
        _size.AccessibleName = "Toolbar icon size";
        _size.Items.AddRange(new object[] { "Small (24 px)", "Medium (36 px)", "High (48 px)" });
        _size.MinimumSize = new Size(160, 0);
        _size.TabIndex = 1;
        _hint.Text = "Sizes are at 96 DPI with a 9 pt interface font.\nIcons scale with your selected font and monitor DPI.";
        _hint.AutoSize = true;
        _hint.Margin = new Padding(3, 12, 3, 6);
        AutoScaleMode = AutoScaleMode.Inherit;
        AutoScroll = true;
        Controls.Add(_layout);
        Name = "AppearanceOptionsView";
        Size = new Size(600, 240);
        _fontPicker.ResumeLayout(false);
        _fontPicker.PerformLayout();
        _layout.ResumeLayout(false);
        _layout.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private TableLayoutPanel _layout = null!;
    private Label _fontLabel = null!;
    private TableLayoutPanel _fontPicker = null!;
    private TextBox _fontValue = null!;
    private Button _browseFont = null!;
    private Label _sizeLabel = null!;
    private ComboBox _size = null!;
    private Label _hint = null!;
}
