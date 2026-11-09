namespace Expenses.App.Dialogs;

partial class TextEntryDialog
{
    private System.ComponentModel.IContainer components;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _layout = new TableLayoutPanel();
        _caption = new Label();
        _text = new TextBox();
        _scope = new TableLayoutPanel();
        _scopeCaption = new Label();
        _periodOnly = new CheckBox();
        _footer = new TableLayoutPanel();
        _ok = new Button();
        _cancel = new Button();
        _layout.SuspendLayout();
        _scope.SuspendLayout();
        _footer.SuspendLayout();
        SuspendLayout();
        _layout.Dock = DockStyle.Fill;
        _layout.Padding = new Padding(12);
        _layout.ColumnCount = 1;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layout.RowCount = 4;
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.Controls.Add(_caption, 0, 0);
        _layout.Controls.Add(_text, 0, 1);
        _layout.Controls.Add(_scope, 0, 2);
        _layout.Controls.Add(_footer, 0, 3);
        _caption.AutoSize = true;
        _caption.Text = "&Note / reminder:";
        _caption.Margin = new Padding(3, 3, 3, 8);
        _text.Dock = DockStyle.Fill;
        _text.Multiline = true;
        _text.AcceptsReturn = true;
        _text.ScrollBars = ScrollBars.Vertical;
        _text.MinimumSize = new Size(0, 25);
        _text.MaxLength = 10000;
        _text.AccessibleName = "Note / reminder";
        _text.TabIndex = 0;
        _scope.AutoSize = true;
        _scope.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _scope.Dock = DockStyle.Fill;
        _scope.ColumnCount = 2;
        _scope.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _scope.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _scope.RowCount = 1;
        _scope.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _scope.Controls.Add(_scopeCaption, 0, 0);
        _scope.Controls.Add(_periodOnly, 1, 0);
        _scopeCaption.AutoSize = true;
        _scopeCaption.Text = "&Scope:";
        _scopeCaption.Anchor = AnchorStyles.Left;
        _periodOnly.AutoSize = true;
        _periodOnly.Text = "Only for the current\nbookkeeping period";
        _periodOnly.TabIndex = 1;
        _footer.Name = "_footer";
        _footer.AutoSize = true;
        _footer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _footer.Dock = DockStyle.Fill;
        _footer.ColumnCount = 2;
        _footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _footer.RowCount = 1;
        _footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _footer.Controls.Add(_ok, 0, 0);
        _footer.Controls.Add(_cancel, 1, 0);
        _ok.AutoSize = true;
        _ok.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _ok.Dock = DockStyle.Fill;
        _ok.Margin = new Padding(4);
        _ok.Text = "OK";
        _ok.TabIndex = 2;
        _ok.Click += AcceptClick;
        _cancel.AutoSize = true;
        _cancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _cancel.Dock = DockStyle.Fill;
        _cancel.Margin = new Padding(4);
        _cancel.Text = "Cancel";
        _cancel.DialogResult = DialogResult.Cancel;
        _cancel.TabIndex = 3;
        AcceptButton = _ok;
        CancelButton = _cancel;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(550, 300);
        MinimumSize = new Size(380, 280);
        Controls.Add(_layout);
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Name = "TextEntryDialog";
        Text = "Add note or reminder";
        _footer.ResumeLayout(false);
        _footer.PerformLayout();
        _scope.ResumeLayout(false);
        _scope.PerformLayout();
        _layout.ResumeLayout(false);
        _layout.PerformLayout();
        ResumeLayout(false);
    }

    private TableLayoutPanel _layout;
    private Label _caption;
    private TextBox _text;
    private TableLayoutPanel _scope;
    private Label _scopeCaption;
    private CheckBox _periodOnly;
    private TableLayoutPanel _footer;
    private Button _ok;
    private Button _cancel;
}
