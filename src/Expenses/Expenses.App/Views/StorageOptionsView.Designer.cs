#nullable enable

namespace Expenses.App.Views;

partial class StorageOptionsView
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
        _fields = new TableLayoutPanel();
        _settingsLabel = new Label();
        _databaseLabel = new Label();
        _backupLabel = new Label();
        _settingsPicker = new TableLayoutPanel();
        _databasePicker = new TableLayoutPanel();
        _backupPicker = new TableLayoutPanel();
        _settings = new TextBox();
        _database = new TextBox();
        _backup = new TextBox();
        _browseSettings = new Button();
        _browseDatabase = new Button();
        _browseBackup = new Button();
        _help = new Label();
        _fields.SuspendLayout();
        _settingsPicker.SuspendLayout();
        _databasePicker.SuspendLayout();
        _backupPicker.SuspendLayout();
        SuspendLayout();
        _fields.AutoSize = true;
        _fields.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _fields.ColumnCount = 2;
        _fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _fields.Dock = DockStyle.Top;
        _fields.Name = "_fields";
        _fields.RowCount = 4;
        _fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _fields.Controls.Add(_settingsLabel, 0, 0);
        _fields.Controls.Add(_settingsPicker, 1, 0);
        _fields.Controls.Add(_databaseLabel, 0, 1);
        _fields.Controls.Add(_databasePicker, 1, 1);
        _fields.Controls.Add(_backupLabel, 0, 2);
        _fields.Controls.Add(_backupPicker, 1, 2);
        _fields.Controls.Add(_help, 0, 3);
        _fields.SetColumnSpan(_help, 2);
        _settingsLabel.Anchor = AnchorStyles.Left;
        _settingsLabel.AutoSize = true;
        _settingsLabel.Name = "_settingsLabel";
        _settingsLabel.Text = "App &settings folder:";
        _settingsLabel.TabIndex = 0;
        _databaseLabel.Anchor = AnchorStyles.Left;
        _databaseLabel.AutoSize = true;
        _databaseLabel.Name = "_databaseLabel";
        _databaseLabel.Text = "&Database folder:";
        _databaseLabel.TabIndex = 2;
        _backupLabel.Anchor = AnchorStyles.Left;
        _backupLabel.AutoSize = true;
        _backupLabel.Name = "_backupLabel";
        _backupLabel.Text = "Additional &backup\nfolder:";
        _backupLabel.TabIndex = 4;
        _settingsPicker.AutoSize = true;
        _settingsPicker.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _settingsPicker.ColumnCount = 2;
        _settingsPicker.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _settingsPicker.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _settingsPicker.Dock = DockStyle.Fill;
        _settingsPicker.Name = "_settingsPicker";
        _settingsPicker.RowCount = 1;
        _settingsPicker.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _settingsPicker.TabIndex = 1;
        _settingsPicker.Controls.Add(_settings, 0, 0);
        _settingsPicker.Controls.Add(_browseSettings, 1, 0);
        _databasePicker.AutoSize = true;
        _databasePicker.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _databasePicker.ColumnCount = 2;
        _databasePicker.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _databasePicker.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _databasePicker.Dock = DockStyle.Fill;
        _databasePicker.Name = "_databasePicker";
        _databasePicker.RowCount = 1;
        _databasePicker.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _databasePicker.TabIndex = 3;
        _databasePicker.Controls.Add(_database, 0, 0);
        _databasePicker.Controls.Add(_browseDatabase, 1, 0);
        _backupPicker.AutoSize = true;
        _backupPicker.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _backupPicker.ColumnCount = 2;
        _backupPicker.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _backupPicker.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _backupPicker.Dock = DockStyle.Fill;
        _backupPicker.Name = "_backupPicker";
        _backupPicker.RowCount = 1;
        _backupPicker.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _backupPicker.TabIndex = 5;
        _backupPicker.Controls.Add(_backup, 0, 0);
        _backupPicker.Controls.Add(_browseBackup, 1, 0);
        _settings.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _settings.AccessibleName = "App settings folder";
        _settings.MinimumSize = new Size(120, 0);
        _settings.Name = "_settings";
        _settings.TabIndex = 0;
        _database.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _database.AccessibleName = "Database folder";
        _database.MinimumSize = new Size(120, 0);
        _database.Name = "_database";
        _database.TabIndex = 0;
        _backup.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _backup.AccessibleName = "Additional backup folder";
        _backup.MinimumSize = new Size(120, 0);
        _backup.Name = "_backup";
        _backup.TabIndex = 0;
        _browseSettings.AccessibleName = "Choose app settings folder";
        _browseSettings.AutoSize = true;
        _browseSettings.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _browseSettings.Dock = DockStyle.Fill;
        _browseSettings.Name = "_browseSettings";
        _browseSettings.Text = "...";
        _browseSettings.TabIndex = 1;
        _browseSettings.Click += BrowseSettings;
        _browseDatabase.AccessibleName = "Choose database folder";
        _browseDatabase.AutoSize = true;
        _browseDatabase.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _browseDatabase.Dock = DockStyle.Fill;
        _browseDatabase.Name = "_browseDatabase";
        _browseDatabase.Text = "...";
        _browseDatabase.TabIndex = 1;
        _browseDatabase.Click += BrowseDatabase;
        _browseBackup.AccessibleName = "Choose additional backup folder";
        _browseBackup.AutoSize = true;
        _browseBackup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _browseBackup.Dock = DockStyle.Fill;
        _browseBackup.Name = "_browseBackup";
        _browseBackup.Text = "...";
        _browseBackup.TabIndex = 1;
        _browseBackup.Click += BrowseBackup;
        _help.AutoSize = true;
        _help.Anchor = AnchorStyles.Left;
        _help.Margin = new Padding(3, 12, 3, 3);
        _help.Name = "_help";
        _help.Text = "A local database and receipt backup is always made at session end.\nThe additional folder is optional; choose OneDrive for a synced copy.\nChanging the database folder copies the current database and receipts.\nChoose a new folder; an existing database will never be overwritten.";
        _help.TabIndex = 6;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Inherit;
        AutoScroll = true;
        Controls.Add(_fields);
        Name = "StorageOptionsView";
        Padding = new Padding(12);
        Size = new Size(680, 300);
        _backupPicker.ResumeLayout(false);
        _backupPicker.PerformLayout();
        _databasePicker.ResumeLayout(false);
        _databasePicker.PerformLayout();
        _settingsPicker.ResumeLayout(false);
        _settingsPicker.PerformLayout();
        _fields.ResumeLayout(false);
        _fields.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private TableLayoutPanel _fields = null!;
    private Label _settingsLabel = null!;
    private Label _databaseLabel = null!;
    private Label _backupLabel = null!;
    private TableLayoutPanel _settingsPicker = null!;
    private TableLayoutPanel _databasePicker = null!;
    private TableLayoutPanel _backupPicker = null!;
    private TextBox _settings = null!;
    private TextBox _database = null!;
    private TextBox _backup = null!;
    private Button _browseSettings = null!;
    private Button _browseDatabase = null!;
    private Button _browseBackup = null!;
    private Label _help = null!;
}
