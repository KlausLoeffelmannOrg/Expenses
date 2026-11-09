using System.Text.Json;
using Expenses.Core;

namespace Expenses.App.Dialogs;

public sealed partial class OptionsDialog : Form
{
    private bool _runtime;
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public AppPreferences Draft { get; private set; } = new();

    public OptionsDialog()
    {
        InitializeComponent();
    }

    public OptionsDialog(AppPreferences preferences) : this()
    {
        Draft = JsonSerializer.Deserialize<AppPreferences>(JsonSerializer.Serialize(preferences))
            ?? throw new InvalidOperationException("Could not create options draft.");
        _storage.LoadPreferences(Draft);
        _reminders.LoadPreferences(Draft);
        _runtime = true;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        DialogLayout.CapToWorkingArea(this);
        if (!_runtime || _tabs.Tabs.Any()) return;
        // AddTab is not a serializable collection in the pinned WARP package.
        _tabs.AddTab("Storage", _storage);
        _tabs.AddTab("Notes and reminders", _reminders);
        _preview.Visible = false;
        _tabs.Dock = DockStyle.Fill;
        _tabs.BringToFront();
    }

    private void AcceptClick(object? sender, EventArgs e)
    {
        if (!_runtime) return;
        try
        {
            _storage.Apply();
            _reminders.Apply();
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex) { DialogLayout.Error(this, ex); }
    }
}
