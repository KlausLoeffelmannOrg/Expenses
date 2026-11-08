using Expenses.App.Dialogs;
using Expenses.Core;

namespace Expenses.App.Views;

internal sealed class StorageOptionsView : UserControl
{
    private readonly AppPreferences _draft;
    private readonly TextBox _settings = new();
    private readonly TextBox _database = new();
    private readonly TextBox _backup = new();

    public StorageOptionsView(AppPreferences draft)
    {
        _draft = draft;
        AutoScroll = true;
        AutoScrollMinSize = new Size(500, 280);
        var fields = DialogLayout.Fields();
        AddFolder(fields, "App &settings folder:", _settings, draft.SettingsFolder);
        AddFolder(fields, "&Database folder:", _database, draft.DatabaseFolder);
        AddFolder(fields, "Additional &backup folder:", _backup, draft.BackupFolder);
        DialogLayout.Add(fields, "", new Label
        {
            Text = "A local database and receipt backup is always made at session end.\n" +
                "The additional folder is optional; choose OneDrive for a synced copy.\n" +
                "Changing the database folder copies the current database and receipts.\n" +
                "Choose a new folder; an existing database will never be overwritten.",
            AutoSize = true
        });
        Controls.Add(fields);
        Size = new Size(680, 300);
    }

    private void AddFolder(TableLayoutPanel fields, string caption, TextBox input, string value)
    {
        input.Text = value;
        input.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        input.AccessibleName = caption.Replace("&", "");
        var picker = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            ColumnStyles = { new ColumnStyle(SizeType.Percent, 100), new ColumnStyle(SizeType.AutoSize) }
        };
        var browse = new Button { Text = "...", AutoSize = true, Dock = DockStyle.Fill, AccessibleName = "Choose " + input.AccessibleName };
        browse.Click += (_, _) =>
        {
            using var dialog = new FolderBrowserDialog { InitialDirectory = input.Text, Description = input.AccessibleName, UseDescriptionForTitle = true };
            if (dialog.ShowDialog(this) == DialogResult.OK) input.Text = dialog.SelectedPath;
        };
        picker.Controls.Add(input, 0, 0);
        picker.Controls.Add(browse, 1, 0);
        DialogLayout.Add(fields, caption, picker);
    }

    public void Apply()
    {
        static string Folder(string text, bool optional)
        {
            text = text.Trim();
            if (optional && text.Length == 0) return "";
            if (!Path.IsPathFullyQualified(text)) throw new ArgumentException("Choose an absolute folder path.");
            return Path.GetFullPath(text);
        }
        _draft.SettingsFolder = Folder(_settings.Text, false);
        _draft.DatabaseFolder = Folder(_database.Text, false);
        _draft.BackupFolder = Folder(_backup.Text, true);
    }
}
