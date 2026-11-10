namespace Foundry.Local.WinForms;

partial class ReceiptAnalysisDialog
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
        _view = new ImageAnalysisView();
        _footer = new TableLayoutPanel();
        _buttons = new TableLayoutPanel();
        _spacer = new Label();
        _useReceipt = new Button();
        _close = new Button();
        _footer.SuspendLayout();
        _buttons.SuspendLayout();
        SuspendLayout();
        _view.Dock = DockStyle.Fill;
        _footer.Dock = DockStyle.Bottom;
        _footer.AutoSize = true;
        _footer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _footer.Padding = new Padding(8);
        _footer.ColumnCount = 2;
        _footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _footer.RowCount = 2;
        _footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _footer.Controls.Add(_spacer, 1, 0);
        _footer.Controls.Add(_buttons, 1, 1);
        _spacer.AutoSize = true;
        _spacer.Margin = new Padding(0, 5, 0, 5);
        _buttons.AutoSize = true;
        _buttons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _buttons.Dock = DockStyle.Fill;
        _buttons.ColumnCount = 2;
        _buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _buttons.RowCount = 1;
        _buttons.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _buttons.Controls.Add(_useReceipt, 0, 0);
        _buttons.Controls.Add(_close, 1, 0);
        _useReceipt.AutoSize = true;
        _useReceipt.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _useReceipt.Dock = DockStyle.Fill;
        _useReceipt.Text = "&Use as expense draft";
        _useReceipt.Enabled = false;
        _useReceipt.DialogResult = DialogResult.OK;
        _close.AutoSize = true;
        _close.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _close.Dock = DockStyle.Fill;
        _close.Text = "&Close";
        _close.DialogResult = DialogResult.Cancel;
        AcceptButton = _useReceipt;
        CancelButton = _close;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 750);
        MinimumSize = new Size(600, 450);
        Controls.Add(_view);
        Controls.Add(_footer);
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        Text = "Foundry Local - Review receipt (nothing is saved automatically)";
        Shown += DialogShown;
        _buttons.ResumeLayout(false);
        _buttons.PerformLayout();
        _footer.ResumeLayout(false);
        _footer.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private ImageAnalysisView _view;
    private TableLayoutPanel _footer;
    private TableLayoutPanel _buttons;
    private Label _spacer;
    private Button _useReceipt;
    private Button _close;
}
