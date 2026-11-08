using Expenses.Core;

namespace Expenses.App.Views;

internal sealed class ReminderOptionsView : UserControl
{
    private readonly AppPreferences _draft;
    private readonly CheckBox _show;
    public ReminderOptionsView(AppPreferences draft)
    {
        _draft = draft;
        AutoScroll = true;
        _show = new CheckBox
        {
            Text = "&Show the previous session's active notes and reminders at startup",
            Checked = draft.ShowRemindersOnStartup,
            AutoSize = true,
            Location = new Point(15, 20)
        };
        Controls.Add(_show);
        Controls.Add(new Label
        {
            Text = "Global reminders appear at startup. Period reminders appear when that period opens.\n" +
                "Dismissed reminders are retained but no longer displayed.",
            AutoSize = true,
            Location = new Point(15, 60)
        });
        AutoScrollMinSize = new Size(620, 150);
        Size = new Size(680, 220);
    }
    public void Apply() => _draft.ShowRemindersOnStartup = _show.Checked;
}
