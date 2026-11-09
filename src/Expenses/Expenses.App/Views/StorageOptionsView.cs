using Expenses.Core;

namespace Expenses.App.Views;

public sealed partial class StorageOptionsView : UserControl
{
    private AppPreferences? _draft;

    public StorageOptionsView()
    {
        InitializeComponent();
    }

    public StorageOptionsView(AppPreferences draft) : this() => LoadPreferences(draft);

    public void LoadPreferences(AppPreferences draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        _draft = draft;
        _settings.Text = draft.SettingsFolder;
        _database.Text = draft.DatabaseFolder;
        _backup.Text = draft.BackupFolder;
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        if (_fields is null) return;

        // Percent columns can allocate less than the value editor's minimum and overlap the picker.
        int captionWidth = Math.Max(_settingsLabel.GetPreferredSize(Size.Empty).Width + _settingsLabel.Margin.Horizontal,
            Math.Max(_databaseLabel.GetPreferredSize(Size.Empty).Width + _databaseLabel.Margin.Horizontal,
                _backupLabel.GetPreferredSize(Size.Empty).Width + _backupLabel.Margin.Horizontal));
        int settingsWidth = _settings.MinimumSize.Width + _settings.Margin.Horizontal
            + _browseSettings.GetPreferredSize(Size.Empty).Width + _browseSettings.Margin.Horizontal;
        int databaseWidth = _database.MinimumSize.Width + _database.Margin.Horizontal
            + _browseDatabase.GetPreferredSize(Size.Empty).Width + _browseDatabase.Margin.Horizontal;
        int backupWidth = _backup.MinimumSize.Width + _backup.Margin.Horizontal
            + _browseBackup.GetPreferredSize(Size.Empty).Width + _browseBackup.Margin.Horizontal;
        _fields.MinimumSize = new Size(captionWidth + Math.Max(settingsWidth + _settingsPicker.Margin.Horizontal,
            Math.Max(databaseWidth + _databasePicker.Margin.Horizontal, backupWidth + _backupPicker.Margin.Horizontal)), 0);
    }

    private void BrowseSettings(object? sender, EventArgs e) => BrowseFolder(_settings);
    private void BrowseDatabase(object? sender, EventArgs e) => BrowseFolder(_database);
    private void BrowseBackup(object? sender, EventArgs e) => BrowseFolder(_backup);

    private void BrowseFolder(TextBox input)
    {
        if (_draft is null) return;
        using var dialog = new FolderBrowserDialog
        {
            InitialDirectory = input.Text,
            Description = input.AccessibleName ?? "Choose folder",
            UseDescriptionForTitle = true
        };
        if (dialog.ShowDialog(this) == DialogResult.OK) input.Text = dialog.SelectedPath;
    }

    public void Apply()
    {
        var draft = _draft ?? throw new InvalidOperationException("Load a runtime preferences draft before applying storage options.");
        static string Folder(string text, bool optional)
        {
            text = text.Trim();
            if (optional && text.Length == 0) return "";
            if (!Path.IsPathFullyQualified(text)) throw new ArgumentException("Choose an absolute folder path.");
            return Path.GetFullPath(text);
        }
        string settings = Folder(_settings.Text, false);
        string database = Folder(_database.Text, false);
        string backup = Folder(_backup.Text, true);
        draft.SettingsFolder = settings;
        draft.DatabaseFolder = database;
        draft.BackupFolder = backup;
    }
}
