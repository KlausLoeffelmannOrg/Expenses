using Expenses.Core;

namespace Expenses.App.Views;

public sealed partial class ReminderOptionsView : UserControl
{
    private AppPreferences? _draft;

    public ReminderOptionsView()
    {
        InitializeComponent();
    }

    public ReminderOptionsView(AppPreferences draft) : this() => LoadPreferences(draft);

    public void LoadPreferences(AppPreferences draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        _draft = draft;
        _show.Checked = draft.ShowRemindersOnStartup;
    }

    public void Apply()
    {
        var draft = _draft ?? throw new InvalidOperationException("Load a runtime preferences draft before applying reminder options.");
        draft.ShowRemindersOnStartup = _show.Checked;
    }
}
