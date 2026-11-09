namespace Expenses.App.Dialogs;

partial class ReminderDialog
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
        _text = new TextBox();
        _footer = new TableLayoutPanel();
        _dismiss = new Button();
        _keep = new Button();
        _footer.SuspendLayout();
        SuspendLayout();
        _text.Dock = DockStyle.Fill;
        _text.Multiline = true;
        _text.ReadOnly = true;
        _text.ScrollBars = ScrollBars.Vertical;
        _text.AccessibleName = "Active notes and reminders";
        _text.TabIndex = 0;
        _footer.Name = "_footer";
        _footer.AutoSize = true;
        _footer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _footer.Dock = DockStyle.Bottom;
        _footer.Padding = new Padding(8);
        _footer.ColumnCount = 2;
        _footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _footer.RowCount = 1;
        _footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _footer.Controls.Add(_dismiss, 0, 0);
        _footer.Controls.Add(_keep, 1, 0);
        _dismiss.AutoSize = true;
        _dismiss.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _dismiss.Dock = DockStyle.Fill;
        _dismiss.Margin = new Padding(4);
        _dismiss.Text = "Dismiss all shown";
        _dismiss.DialogResult = DialogResult.OK;
        _dismiss.TabIndex = 1;
        _keep.AutoSize = true;
        _keep.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _keep.Dock = DockStyle.Fill;
        _keep.Margin = new Padding(4);
        _keep.Text = "Keep for next time";
        _keep.DialogResult = DialogResult.Cancel;
        _keep.TabIndex = 2;
        AcceptButton = _dismiss;
        CancelButton = _keep;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(650, 400);
        MinimumSize = new Size(410, 240);
        Padding = new Padding(12);
        Controls.Add(_text);
        Controls.Add(_footer);
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Name = "ReminderDialog";
        Text = "Notes and reminders";
        _footer.ResumeLayout(false);
        _footer.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private TextBox _text;
    private TableLayoutPanel _footer;
    private Button _dismiss;
    private Button _keep;
}
