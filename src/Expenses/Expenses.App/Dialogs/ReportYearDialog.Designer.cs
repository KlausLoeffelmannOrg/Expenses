namespace Expenses.App.Dialogs;

partial class ReportYearDialog
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
        _year = new NumericUpDown();
        _footer = new TableLayoutPanel();
        _view = new Button();
        _cancel = new Button();
        ((System.ComponentModel.ISupportInitialize)_year).BeginInit();
        _layout.SuspendLayout();
        _footer.SuspendLayout();
        SuspendLayout();
        _layout.AutoSize = true;
        _layout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _layout.Dock = DockStyle.Top;
        _layout.Padding = new Padding(12);
        _layout.ColumnCount = 2;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layout.RowCount = 2;
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.Controls.Add(_caption, 0, 0);
        _layout.Controls.Add(_year, 1, 0);
        _layout.Controls.Add(_footer, 0, 1);
        _layout.SetColumnSpan(_footer, 2);
        _caption.AutoSize = true;
        _caption.Anchor = AnchorStyles.Left;
        _caption.Margin = new Padding(3, 8, 12, 8);
        _caption.Text = "&Calendar year:";
        _year.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _year.Minimum = 1900;
        _year.Maximum = 9998;
        _year.Value = 2024;
        _year.Margin = new Padding(3, 6, 3, 6);
        _year.AccessibleName = "Calendar year";
        _year.TabIndex = 0;
        _footer.Name = "_footer";
        _footer.AutoSize = true;
        _footer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _footer.Dock = DockStyle.Fill;
        _footer.ColumnCount = 2;
        _footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _footer.RowCount = 1;
        _footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _footer.Controls.Add(_view, 0, 0);
        _footer.Controls.Add(_cancel, 1, 0);
        _view.AutoSize = true;
        _view.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _view.Dock = DockStyle.Fill;
        _view.Margin = new Padding(4);
        _view.Text = "View report";
        _view.DialogResult = DialogResult.OK;
        _view.TabIndex = 1;
        _cancel.AutoSize = true;
        _cancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _cancel.Dock = DockStyle.Fill;
        _cancel.Margin = new Padding(4);
        _cancel.Text = "Cancel";
        _cancel.DialogResult = DialogResult.Cancel;
        _cancel.TabIndex = 2;
        AcceptButton = _view;
        CancelButton = _cancel;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoScroll = true;
        ClientSize = new Size(380, 160);
        MinimumSize = new Size(330, 160);
        Controls.Add(_layout);
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Name = "ReportYearDialog";
        Text = "Annual expense report";
        ((System.ComponentModel.ISupportInitialize)_year).EndInit();
        _footer.ResumeLayout(false);
        _footer.PerformLayout();
        _layout.ResumeLayout(false);
        _layout.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private TableLayoutPanel _layout;
    private Label _caption;
    private NumericUpDown _year;
    private TableLayoutPanel _footer;
    private Button _view;
    private Button _cancel;
}
