using Expenses.Core;

namespace Expenses.App.Dialogs;

public sealed partial class ReminderDialog : Form
{
    public ReminderDialog()
    {
        InitializeComponent();
    }

    public ReminderDialog(string title, IReadOnlyList<Reminder> reminders) : this()
    {
        Text = title;
        _text.Text = string.Join(Environment.NewLine + Environment.NewLine,
            reminders.Select(r => $"{r.CreatedAt.LocalDateTime:g}: {r.Text}"));
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        DialogLayout.CapToWorkingArea(this);
    }
}
