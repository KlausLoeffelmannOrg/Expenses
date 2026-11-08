using System.Text.Json;
using Expenses.App.Views;
using Expenses.Core;
using WarpToolkit.WinForms.Containers;

namespace Expenses.App.Dialogs;

internal sealed class OptionsDialog : Form
{
    public AppPreferences Draft { get; }
    public OptionsDialog(AppPreferences preferences)
    {
        Draft = JsonSerializer.Deserialize<AppPreferences>(JsonSerializer.Serialize(preferences))
            ?? throw new InvalidOperationException("Could not create options draft.");
        DialogLayout.Configure(this, "Options", new Size(760, 420));
        var tabs = new FluentTabControl { Dock = DockStyle.Fill };
        var storage = new StorageOptionsView(Draft);
        var reminders = new ReminderOptionsView(Draft);
        tabs.AddTab("Storage", storage);
        tabs.AddTab("Notes and reminders", reminders);
        Controls.Add(tabs);
        DialogLayout.Footer(this, () =>
        {
            try
            {
                storage.Apply();
                reminders.Apply();
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { DialogLayout.Error(this, ex); }
        });
    }
}
