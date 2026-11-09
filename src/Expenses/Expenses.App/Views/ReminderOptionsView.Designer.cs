#nullable enable

namespace Expenses.App.Views;

partial class ReminderOptionsView
{
    private System.ComponentModel.IContainer? components = null;

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
        _layout = new TableLayoutPanel();
        _show = new CheckBox();
        _help = new Label();
        _layout.SuspendLayout();
        SuspendLayout();
        _layout.AutoSize = true;
        _layout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _layout.ColumnCount = 1;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layout.Dock = DockStyle.Top;
        _layout.Name = "_layout";
        _layout.RowCount = 2;
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.Controls.Add(_show, 0, 0);
        _layout.Controls.Add(_help, 0, 1);
        _show.AutoSize = true;
        _show.Anchor = AnchorStyles.Left;
        _show.AccessibleName = "Show active notes and reminders at startup";
        _show.Name = "_show";
        _show.Text = "&Show the previous session's active notes\nand reminders at startup";
        _show.TabIndex = 0;
        _help.AutoSize = true;
        _help.Anchor = AnchorStyles.Left;
        _help.Margin = new Padding(3, 12, 3, 3);
        _help.Name = "_help";
        _help.Text = "Global reminders appear at startup.\nPeriod reminders appear when that period opens.\nDismissed reminders are retained but no longer displayed.";
        _help.TabIndex = 1;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Inherit;
        AutoScroll = true;
        Controls.Add(_layout);
        Name = "ReminderOptionsView";
        Padding = new Padding(12);
        Size = new Size(680, 220);
        _layout.ResumeLayout(false);
        _layout.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private TableLayoutPanel _layout = null!;
    private CheckBox _show = null!;
    private Label _help = null!;
}
