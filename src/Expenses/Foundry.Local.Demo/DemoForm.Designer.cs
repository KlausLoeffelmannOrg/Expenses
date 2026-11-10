namespace Foundry.Local.Demo;

partial class DemoForm
{
    private System.ComponentModel.IContainer components;
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _header = new TableLayoutPanel();
        _manage = new Button();
        _hint = new Label();
        _tabs = new TabControl();
        _imagePage = new TabPage();
        _translationPage = new TabPage();
        _images = new WinForms.ImageAnalysisView();
        _translation = new TranslationView();
        _header.SuspendLayout();
        _tabs.SuspendLayout();
        SuspendLayout();
        _header.Dock = DockStyle.Top;
        _header.Padding = new Padding(8);
        _header.AutoSize = true;
        _header.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _header.ColumnCount = 2;
        _header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _header.RowCount = 1;
        _header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _header.Controls.Add(_hint, 0, 0);
        _header.Controls.Add(_manage, 1, 0);
        _hint.AutoSize = true;
        _hint.Anchor = AnchorStyles.Left;
        _hint.Text = "Foundry Local demo - local image analysis, receipt OCR and contextual translation.\nLoad a suitable model in Model management before running an operation.";
        _manage.Text = "&Model management...";
        _manage.AutoSize = true;
        _manage.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _manage.Dock = DockStyle.Fill;
        _manage.Click += ManageClick;
        _tabs.Dock = DockStyle.Fill;
        _tabs.Controls.Add(_imagePage);
        _tabs.Controls.Add(_translationPage);
        _imagePage.Text = "Images and receipt OCR";
        _imagePage.Controls.Add(_images);
        _images.Dock = DockStyle.Fill;
        _translationPage.Text = "Translation";
        _translationPage.Controls.Add(_translation);
        _translation.Dock = DockStyle.Fill;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 800);
        MinimumSize = new Size(650, 500);
        Controls.Add(_tabs);
        Controls.Add(_header);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Foundry Local demo";
        Shown += DemoShown;
        _header.ResumeLayout(false);
        _header.PerformLayout();
        _tabs.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    private TableLayoutPanel _header;
    private Button _manage;
    private Label _hint;
    private TabControl _tabs;
    private TabPage _imagePage;
    private TabPage _translationPage;
    private WinForms.ImageAnalysisView _images;
    private TranslationView _translation;
}
