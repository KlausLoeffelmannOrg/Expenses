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
        _tabs.Renderer = new CategoryRenderer();
    }

    public OptionsDialog(AppPreferences preferences) : this()
    {
        Draft = JsonSerializer.Deserialize<AppPreferences>(JsonSerializer.Serialize(preferences))
            ?? throw new InvalidOperationException("Could not create options draft.");
        DataContext = Draft;
        _storage.LoadPreferences(Draft);
        _reminders.LoadPreferences(Draft);
        _runtime = true;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        DialogLayout.CapToWorkingArea(this);
        SelectCategory(_storage, _storageHeader);
    }

    private void StorageClick(object? sender, EventArgs e) => SelectCategory(_storage, _storageHeader);
    private void RemindersClick(object? sender, EventArgs e) => SelectCategory(_reminders, _remindersHeader);

    private void SelectCategory(Control view, ToolStripButton header)
    {
        _storage.Visible = view == _storage;
        _reminders.Visible = view == _reminders;
        _storageHeader.Checked = header == _storageHeader;
        _remindersHeader.Checked = header == _remindersHeader;
        view.BringToFront();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_runtime && DialogResult == DialogResult.OK)
        {
            try
            {
                _storage.Apply();
                _reminders.Apply();
            }
            catch (Exception ex) { e.Cancel = true; DialogLayout.Error(this, ex); }
        }
        base.OnFormClosing(e);
    }

    private sealed class CategoryRenderer : ToolStripSystemRenderer
    {
        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            base.OnRenderButtonBackground(e);
            if (e.Item is not ToolStripButton { Checked: true }) return;
            using var brush = new SolidBrush(SystemColors.Highlight);
            e.Graphics.FillRectangle(brush, 2, e.Item.Height - 3, Math.Max(0, e.Item.Width - 4), 3);
        }
    }
}
