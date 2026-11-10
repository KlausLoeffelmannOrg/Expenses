namespace Expenses.App.Dialogs;

partial class OptionsDialog
{
    private System.ComponentModel.IContainer components;

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
        components = new System.ComponentModel.Container();
        _tabs = new ToolStrip();
        _storageHeader = new ToolStripButton();
        _remindersHeader = new ToolStripButton();
        _appearanceHeader = new ToolStripButton();
        _preview = new Panel();
        _storage = new Views.StorageOptionsView();
        _reminders = new Views.ReminderOptionsView();
        _appearance = new Views.AppearanceOptionsView();
        _footer = new TableLayoutPanel();
        _ok = new Button();
        _cancel = new Button();
        _preview.SuspendLayout();
        _footer.SuspendLayout();
        SuspendLayout();
        _tabs.Dock = DockStyle.Top;
        _tabs.GripStyle = ToolStripGripStyle.Hidden;
        _tabs.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
        _tabs.Items.AddRange(new ToolStripItem[] { _storageHeader, _remindersHeader, _appearanceHeader });
        _tabs.Name = "_tabs";
        _storageHeader.Text = "&Storage";
        _storageHeader.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _storageHeader.Checked = true;
        _storageHeader.Click += StorageClick;
        _remindersHeader.Text = "&Notes and reminders";
        _remindersHeader.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _remindersHeader.Click += RemindersClick;
        _appearanceHeader.Text = "&Appearance";
        _appearanceHeader.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _appearanceHeader.Click += AppearanceClick;
        _preview.Dock = DockStyle.Fill;
        _preview.Controls.Add(_storage);
        _preview.Controls.Add(_reminders);
        _preview.Controls.Add(_appearance);
        _preview.Name = "_preview";
        _storage.Dock = DockStyle.Fill;
        _storage.Name = "_storage";
        _reminders.Dock = DockStyle.Fill;
        _reminders.Name = "_reminders";
        _reminders.Visible = false;
        _appearance.Dock = DockStyle.Fill;
        _appearance.Visible = false;
        _footer.Name = "_footer";
        _footer.AutoSize = true;
        _footer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _footer.Dock = DockStyle.Bottom;
        _footer.Padding = new Padding(8);
        _footer.ColumnCount = 2;
        _footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _footer.RowCount = 1;
        _footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _footer.Controls.Add(_ok, 0, 0);
        _footer.Controls.Add(_cancel, 1, 0);
        _ok.AutoSize = true;
        _ok.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _ok.Dock = DockStyle.Fill;
        _ok.Margin = new Padding(4);
        _ok.Text = "OK";
        _ok.TabIndex = 0;
        _ok.DialogResult = DialogResult.OK;
        _cancel.AutoSize = true;
        _cancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _cancel.Dock = DockStyle.Fill;
        _cancel.Margin = new Padding(4);
        _cancel.Text = "Cancel";
        _cancel.DialogResult = DialogResult.Cancel;
        _cancel.TabIndex = 1;
        AcceptButton = _ok;
        CancelButton = _cancel;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(760, 480);
        MinimumSize = new Size(440, 330);
        Controls.Add(_preview);
        Controls.Add(_tabs);
        Controls.Add(_footer);
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Name = "OptionsDialog";
        Text = "Options";
        _footer.ResumeLayout(false);
        _footer.PerformLayout();
        _preview.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    private ToolStrip _tabs;
    private ToolStripButton _storageHeader;
    private ToolStripButton _remindersHeader;
    private ToolStripButton _appearanceHeader;
    private Panel _preview;
    private Views.StorageOptionsView _storage;
    private Views.ReminderOptionsView _reminders;
    private Views.AppearanceOptionsView _appearance;
    private TableLayoutPanel _footer;
    private Button _ok;
    private Button _cancel;
}
