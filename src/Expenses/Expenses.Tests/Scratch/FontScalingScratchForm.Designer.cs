#nullable disable

namespace Expenses.Tests.Scratch;

partial class FontScalingScratchForm
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _root = new TableLayoutPanel();
        _toolbar = new ToolStrip();
        _acceptCommand = new ToolStripButton();
        _viewport = new Panel();
        _view = new HostedScalingScratchView();
        _outerButtons = new TableLayoutPanel();
        _innerButtons = new TableLayoutPanel();
        _spacer = new Label();
        _ok = new Button();
        _cancel = new Button();
        _root.SuspendLayout();
        _toolbar.SuspendLayout();
        _viewport.SuspendLayout();
        _outerButtons.SuspendLayout();
        _innerButtons.SuspendLayout();
        SuspendLayout();
        _root.ColumnCount = 1;
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _root.RowCount = 3;
        _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _root.Dock = DockStyle.Fill;
        _root.Padding = new Padding(12);
        _root.Name = "RootLayout";
        _root.Controls.Add(_toolbar, 0, 0);
        _root.Controls.Add(_viewport, 0, 1);
        _root.Controls.Add(_outerButtons, 0, 2);
        _root.SetColumnSpan(_outerButtons, 1);
        _toolbar.AutoSize = true;
        _toolbar.Dock = DockStyle.Fill;
        _toolbar.GripStyle = ToolStripGripStyle.Hidden;
        _toolbar.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
        _toolbar.Items.Add(_acceptCommand);
        _toolbar.Name = "CommandToolbar";
        _acceptCommand.Text = "Accept";
        _acceptCommand.AccessibleName = "Accept";
        _acceptCommand.ToolTipText = "Accept changes";
        _acceptCommand.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
        _viewport.AutoScroll = true;
        _viewport.Dock = DockStyle.Fill;
        _viewport.Name = "ContentViewport";
        _viewport.Controls.Add(_view);
        _view.AutoSize = false;
        _view.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _view.Dock = DockStyle.Fill;
        _view.Name = "HostedView";
        _outerButtons.AutoSize = true;
        _outerButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _outerButtons.Dock = DockStyle.Fill;
        _outerButtons.ColumnCount = 2;
        _outerButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _outerButtons.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _outerButtons.RowCount = 2;
        _outerButtons.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _outerButtons.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _outerButtons.Controls.Add(_spacer, 1, 0);
        _outerButtons.Controls.Add(_innerButtons, 1, 1);
        _outerButtons.Name = "OuterButtonPanel";
        _spacer.AutoSize = true;
        _spacer.Dock = DockStyle.Fill;
        _spacer.Margin = new Padding(0, 5, 0, 5);
        _spacer.Text = "";
        _spacer.Name = "ButtonSpacer";
        _innerButtons.AutoSize = true;
        _innerButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _innerButtons.Dock = DockStyle.Fill;
        _innerButtons.ColumnCount = 2;
        _innerButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _innerButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _innerButtons.RowCount = 1;
        _innerButtons.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _innerButtons.Controls.Add(_ok, 0, 0);
        _innerButtons.Controls.Add(_cancel, 1, 0);
        _innerButtons.Name = "InnerButtonPanel";
        _ok.AutoSize = true;
        _ok.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _ok.Dock = DockStyle.Fill;
        _ok.Margin = new Padding(4);
        _ok.DialogResult = DialogResult.OK;
        _ok.Text = "OK";
        _ok.Name = "OkButton";
        _cancel.AutoSize = true;
        _cancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _cancel.Dock = DockStyle.Fill;
        _cancel.Margin = new Padding(4);
        _cancel.DialogResult = DialogResult.Cancel;
        _cancel.Text = "Cancel";
        _cancel.Name = "CancelButton";
        AcceptButton = _ok;
        CancelButton = _cancel;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(760, 520);
        MinimumSize = new Size(320, 260);
        StartPosition = FormStartPosition.Manual;
        Text = "Font and DPI scaling scratch";
        Name = "FontScalingScratchForm";
        Controls.Add(_root);
        _innerButtons.ResumeLayout(false);
        _innerButtons.PerformLayout();
        _outerButtons.ResumeLayout(false);
        _outerButtons.PerformLayout();
        _toolbar.ResumeLayout(false);
        _toolbar.PerformLayout();
        _viewport.ResumeLayout(false);
        _root.ResumeLayout(false);
        _root.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private TableLayoutPanel _root;
    private ToolStrip _toolbar;
    private ToolStripButton _acceptCommand;
    private Panel _viewport;
    private HostedScalingScratchView _view;
    private TableLayoutPanel _outerButtons;
    private TableLayoutPanel _innerButtons;
    private Label _spacer;
    private Button _ok;
    private Button _cancel;
}
