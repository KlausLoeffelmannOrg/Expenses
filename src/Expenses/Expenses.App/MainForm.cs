using System.Text;
using System.Text.Json;
using Expenses.App.Dialogs;
using Expenses.App.Views;
using Expenses.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Expenses.App;

public partial class MainForm : Form, IServiceProvider
{
    private IServiceProvider? _serviceProvider;
    private BookkeepingStore? _store;
    private PreferencesStore? _preferencesStore;
    private AppPreferences _preferences = new();
    private BookkeepingPeriod? _period;
    private int? _activeDraft;
    private bool _refreshing;
    private bool _loaded;
    private readonly List<ExpenseResultsView> _results = [];
    private readonly List<(ToolStripItem Item, Func<bool> Enabled)> _commands = [];

    public MainForm()
    {
        InitializeComponent();
        BuildCommands();
        foreach (string title in new[] { "All expenses", "Fuel", "Sub Camera" })
        {
            var view = new ExpenseResultsView();
            _results.Add(view);
            _tabs.AddTab(title, view);
            view.Grid.SelectionChanged += (_, _) => SelectionChanged();
            view.Grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) Execute(EditExpense); };
            view.Grid.ColumnWidthChanged += (_, e) =>
            {
                if (!_loaded || e.Column is null) return;
                _preferences.ColumnWidths[e.Column.Name] = e.Column.Width;
                QueuePreferencesSave();
            };
        }
        _tabs.TabChanged += (_, _) => SelectionChanged();
        _tree.BeforeSelect += (_, e) => e.Cancel = _editor.IsEditing;
        _tree.AfterSelect += (_, e) =>
        {
            if (_refreshing) return;
            Execute(() =>
            {
                if (e.Node?.Tag is BookkeepingPeriod period) OpenPeriod(period);
                else if (e.Node?.Tag is ExpenseDraft draft) RestoreDraft(draft);
            });
        };
        _editor.CommitRequested += (_, _) => Execute(CommitExpense);
        _editor.CancelRequested += (_, _) => Execute(CancelEdit);
        _editor.StashRequested += (_, _) => Execute(StashExpense);
        _layoutTimer.Tick += (_, _) => { _layoutTimer.Stop(); Execute(SavePreferences); };
        LocationChanged += (_, _) => QueuePreferencesSave();
        SizeChanged += (_, _) => QueuePreferencesSave();
        _navigation.SplitterMoved += (_, _) => QueuePreferencesSave();
        _details.SplitterMoved += (_, _) => QueuePreferencesSave();
        UpdateCommands();
    }

    public MainForm(IServiceProvider services) : this()
    {
        _serviceProvider = services;
        _store = services.GetRequiredService<BookkeepingStore>();
        _preferencesStore = services.GetRequiredService<PreferencesStore>();
        _preferences = services.GetRequiredService<AppPreferences>();
    }

    object? IServiceProvider.GetService(Type serviceType) => _serviceProvider?.GetService(serviceType);
    private BookkeepingStore Store => _store ?? throw new InvalidOperationException("Bookkeeping storage is not initialized.");
    private Expense? SelectedExpense => _results.Count == 0 ? null
        : _results[Math.Clamp(_tabs.CurrentTabIndex, 0, _results.Count - 1)].Grid.CurrentRow?.DataBoundItem as Expense;

    private void BuildCommands()
    {
        var file = new ToolStripMenuItem("&File");
        var edit = new ToolStripMenuItem("&Edit");
        var view = new ToolStripMenuItem("&View");
        var tools = new ToolStripMenuItem("&Tools");
        _menu.Items.AddRange([file, edit, view, tools]);
        AddCommand(file, "&New bookkeeping period...", Keys.None, () => SelectPeriod(true), () => !_editor.IsEditing);
        AddCommand(file, "&Open bookkeeping period...", Keys.Control | Keys.O, () => SelectPeriod(false), () => !_editor.IsEditing);
        file.DropDownItems.Add(new ToolStripSeparator());
        AddCommand(file, "Export as &CSV...", Keys.None, () => Export(false), () => _period is not null && !_editor.IsEditing);
        AddCommand(file, "Export as &PDF...", Keys.None, () => Export(true), () => _period is not null && !_editor.IsEditing);
        file.DropDownItems.Add(new ToolStripSeparator());
        AddCommand(file, "E&xit", Keys.Alt | Keys.F4, Close, () => true);
        AddCommand(edit, "&New expense", Keys.Control | Keys.N, NewExpense,
            () => _period is not null && _editor.Visible && !_editor.IsEditing, "New expense");
        AddCommand(edit, "&Add note or reminder...", Keys.None, AddReminder, () => !_editor.IsEditing, "Add note");
        AddCommand(edit, "&Edit expense", Keys.F2, EditExpense,
            () => SelectedExpense is not null && !_editor.IsEditing, "Edit expense");
        AddCommand(edit, "&Delete expense...", Keys.Delete, DeleteExpense,
            () => SelectedExpense is not null && !_editor.IsEditing, "Delete expense");
        edit.DropDownItems.Add(new ToolStripSeparator());
        AddCommand(edit, "&Save expense", Keys.Control | Keys.S, CommitExpense, () => _editor.IsEditing, "Save");
        AddCommand(edit, "&Cancel edit", Keys.None, CancelEdit, () => _editor.IsEditing, "Cancel");
        edit.DropDownItems.OfType<ToolStripMenuItem>().Last().ShortcutKeyDisplayString = "Esc";
        AddCommand(edit, "Stas&h draft...", Keys.Control | Keys.Shift | Keys.S, StashExpense, () => _editor.IsEditing, "Stash draft");
        AddCommand(view, "&Period expense report...", Keys.None, () => ShowReport(false), () => _period is not null && !_editor.IsEditing);
        AddCommand(view, "&Annual expense report...", Keys.None, () => ShowReport(true), () => !_editor.IsEditing);
        AddCommand(view, "Active &notes and reminders...", Keys.None, ShowAllReminders, () => !_editor.IsEditing);
        AddCommand(tools, "&Options...", Keys.None, ShowOptions, () => !_editor.IsEditing);
    }

    private void AddCommand(ToolStripMenuItem parent, string text, Keys shortcut, Action action, Func<bool> enabled, string? toolbarText = null)
    {
        var item = new ToolStripMenuItem(text) { ShortcutKeys = shortcut };
        item.Click += (_, _) => Execute(action);
        parent.DropDownItems.Add(item);
        _commands.Add((item, enabled));
        if (toolbarText is null) return;
        var button = new ToolStripButton(toolbarText) { DisplayStyle = ToolStripItemDisplayStyle.Text, ToolTipText = text.Replace("&", "") };
        button.Click += (_, _) => Execute(action);
        _toolbar.Items.Add(button);
        _commands.Add((button, enabled));
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (_store is null) return;
        Execute(() =>
        {
            var saved = _preferences.Window;
            var bounds = new Rectangle(saved.X, saved.Y, Math.Max(MinimumSize.Width, saved.Width), Math.Max(MinimumSize.Height, saved.Height));
            if (saved.HasPosition && Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(bounds)))
            {
                var area = Screen.FromRectangle(bounds).WorkingArea;
                Size = new Size(Math.Min(bounds.Width, area.Width), Math.Min(bounds.Height, area.Height));
                Location = new Point(Math.Clamp(bounds.X, area.Left, Math.Max(area.Left, area.Right - Width)),
                    Math.Clamp(bounds.Y, area.Top, Math.Max(area.Top, area.Bottom - Height)));
            }
            else CenterToScreen();
            if (saved.Maximized) WindowState = FormWindowState.Maximized;
            _navigation.SplitterDistance = Math.Clamp(_preferences.NavigationWidth, _navigation.Panel1MinSize,
                Math.Max(_navigation.Panel1MinSize, _navigation.Width - _navigation.Panel2MinSize - _navigation.SplitterWidth));
            _details.SplitterDistance = Math.Clamp(_preferences.ResultsHeight, _details.Panel1MinSize,
                Math.Max(_details.Panel1MinSize, _details.Height - _details.Panel2MinSize - _details.SplitterWidth));
            foreach (var result in _results)
                foreach (DataGridViewColumn column in result.Grid.Columns)
                    if (_preferences.ColumnWidths.TryGetValue(column.Name, out int width)) column.Width = Math.Clamp(width, 40, 2000);
            ReloadTree();
            _loaded = true;
            if (_preferences.ShowRemindersOnStartup) ShowReminders(null, "Global notes and reminders");
            var last = Store.GetPeriods().FirstOrDefault(p => p.Id == _preferences.LastPeriodId);
            if (last is not null) OpenPeriod(last);
            UpdateCommands();
        });
    }

    private void Execute(Action action)
    {
        try { action(); }
        catch (Exception ex) { DialogLayout.Error(this, ex); }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape && _editor.IsEditing)
        {
            Execute(CancelEdit);
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void CancelEdit()
    {
        _activeDraft = null;
        ReloadExpenses();
    }

    private void SelectPeriod(bool create)
    {
        if (_editor.IsEditing) return;
        using var dialog = new PeriodDialog(Store, create);
        if (dialog.ShowDialog(this) == DialogResult.OK && dialog.SelectedPeriod is { } period)
        {
            ReloadTree();
            OpenPeriod(period);
        }
    }

    private void OpenPeriod(BookkeepingPeriod period)
    {
        if (_editor.IsEditing) return;
        _period = period;
        _activeDraft = null;
        _preferences.LastPeriodId = period.Id;
        Text = $"Expenses - {period}";
        ReloadExpenses();
        QueuePreferencesSave();
        ShowReminders(period.Id, $"Notes and reminders - {period}");
    }

    private void ReloadTree()
    {
        _refreshing = true;
        _tree.BeginUpdate();
        try
        {
            _tree.Nodes.Clear();
            foreach (var group in Store.GetPeriods().GroupBy(p => p.Year))
            {
                var year = _tree.Nodes.Add(group.Key.ToString());
                foreach (var period in group)
                {
                    var node = year.Nodes.Add(period.ToString());
                    node.Tag = period;
                    foreach (var draft in Store.GetDrafts(period.Id))
                        node.Nodes.Add(new TreeNode($"Draft: {draft.Name} ({draft.SavedAt.LocalDateTime:g})") { Tag = draft });
                }
            }
            _tree.ExpandAll();
        }
        finally { _tree.EndUpdate(); _refreshing = false; }
    }

    private void ReloadExpenses()
    {
        _refreshing = true;
        try
        {
            var expenses = _period is null ? [] : Store.GetExpenses(_period.Id);
            _results[0].Grid.DataSource = expenses;
            _results[1].Grid.DataSource = expenses.Where(e => e.Category.Equals("Fuel", StringComparison.OrdinalIgnoreCase)).ToList();
            _results[2].Grid.DataSource = expenses.Where(e => e.Tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Contains("Sub Camera", StringComparer.OrdinalIgnoreCase)).ToList();
            foreach (var result in _results)
            {
                result.Grid.ClearSelection();
                result.Grid.CurrentCell = null;
            }
            _editor.Clear();
            _status.Text = _period is null ? "Create or open a bookkeeping period to begin."
                : $"{_period} | {expenses.Count} expenses | Recorded: {expenses.Sum(x => x.Amount):N2} USD | Business portion: {expenses.Sum(x => x.BusinessAmount):N2} USD";
        }
        finally { _refreshing = false; }
        UpdateCommands();
    }

    private void SelectionChanged()
    {
        if (_refreshing || _editor.IsEditing) return;
        if (SelectedExpense is { } expense) _editor.ShowExpense(expense);
        else _editor.Clear();
        UpdateCommands();
    }

    private void NewExpense()
    {
        if (_period is null || _editor.IsEditing) return;
        _activeDraft = null;
        var today = DateOnly.FromDateTime(DateTime.Today);
        _editor.NewExpense(today >= _period.StartDate && today <= _period.EndDate ? today : _period.StartDate);
        UpdateCommands();
    }

    private void EditExpense()
    {
        if (SelectedExpense is null || _editor.IsEditing) return;
        _activeDraft = null;
        _editor.SetEditing(true);
        UpdateCommands();
    }

    private void CommitExpense()
    {
        if (!_editor.IsEditing || _period is null) return;
        Store.SaveExpense(_period.Id, _editor.ReadInput(), _activeDraft);
        _activeDraft = null;
        ReloadTree();
        ReloadExpenses();
    }

    private void StashExpense()
    {
        if (!_editor.IsEditing || _period is null) return;
        using var dialog = new TextEntryDialog("Stash expense draft", "&Draft name:");
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        Store.Stash(_period.Id, dialog.EnteredText, _editor.ReadInput(), _activeDraft);
        _activeDraft = null;
        ReloadTree();
        ReloadExpenses();
    }

    private void RestoreDraft(ExpenseDraft draft)
    {
        if (_editor.IsEditing) return;
        bool changingPeriod = _period?.Id != draft.BookkeepingPeriodId;
        _period = Store.GetPeriods().Single(p => p.Id == draft.BookkeepingPeriodId);
        _preferences.LastPeriodId = _period.Id;
        Text = $"Expenses - {_period}";
        ReloadExpenses();
        if (changingPeriod) ShowReminders(_period.Id, $"Notes and reminders - {_period}");
        var input = JsonSerializer.Deserialize<ExpenseInput>(draft.Payload) ?? throw new InvalidDataException("This draft cannot be read.");
        _activeDraft = draft.Id;
        _editor.LoadInput(input, true);
        UpdateCommands();
        QueuePreferencesSave();
    }

    private void DeleteExpense()
    {
        if (_editor.IsEditing || SelectedExpense is not { } expense) return;
        if (MessageBox.Show(this, $"Mark the expense from {expense.Payee} for {expense.Amount:N2} USD as deleted?\nThe original record is retained.",
            "Delete expense", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        Store.DeleteExpense(expense.Id, expense.Revision);
        ReloadExpenses();
    }

    private void UpdateCommands()
    {
        foreach (var (item, enabled) in _commands) item.Enabled = enabled();
        _tree.Enabled = !_editor.IsEditing;
        _tabs.Enabled = !_editor.IsEditing;
    }

    private void AddReminder()
    {
        using var dialog = new TextEntryDialog("Add note or reminder", "&Note / reminder:", true, _period is not null);
        if (dialog.ShowDialog(this) == DialogResult.OK)
            Store.AddReminder(dialog.PeriodOnly ? _period?.Id : null, dialog.EnteredText);
    }

    private void ShowReminders(int? periodId, string title)
    {
        var reminders = Store.GetReminders(periodId);
        if (reminders.Count == 0) return;
        using var dialog = new Form();
        DialogLayout.Configure(dialog, title, new Size(650, 400));
        var text = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            Text = string.Join(Environment.NewLine + Environment.NewLine, reminders.Select(r => $"{r.CreatedAt.LocalDateTime:g}: {r.Text}")),
            AccessibleName = "Active notes and reminders"
        };
        dialog.Controls.Add(text);
        DialogLayout.Footer(dialog, () => dialog.DialogResult = DialogResult.OK, "Dismiss all shown", "Keep for next time");
        if (dialog.ShowDialog(this) == DialogResult.OK) Store.DismissReminders(reminders.Select(r => r.Id));
    }

    private void ShowAllReminders()
    {
        ShowReminders(null, "Global notes and reminders");
        if (_period is not null) ShowReminders(_period.Id, $"Notes and reminders - {_period}");
    }

    private void ShowReport(bool annual)
    {
        int year = _period?.Year ?? DateTime.Today.Year;
        if (annual)
        {
            using var picker = new Form();
            DialogLayout.Configure(picker, "Annual expense report", new Size(380, 160));
            var years = new NumericUpDown { Minimum = 1900, Maximum = 9998, Value = year };
            var fields = DialogLayout.Fields();
            DialogLayout.Add(fields, "&Calendar year:", years);
            picker.Controls.Add(fields);
            DialogLayout.Footer(picker, () => picker.DialogResult = DialogResult.OK, "View report");
            if (picker.ShowDialog(this) != DialogResult.OK) return;
            year = (int)years.Value;
        }
        var totals = Store.Report(year, annual ? null : _period?.Id);
        using var dialog = new Form();
        DialogLayout.Configure(dialog, annual ? $"Annual expense report - {year}" : $"Period expense report - {_period}", new Size(850, 500));
        var grid = new WarpToolkit.WinForms.Specialized.WarpDataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AutoGenerateColumns = false,
            AccessibleName = "Expense totals by category"
        };
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CategoryTotal.Category), HeaderText = "Category" });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(CategoryTotal.RecordedAmount),
            HeaderText = "Recorded amount (USD)",
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(CategoryTotal.BusinessAmount),
            HeaderText = "Business portion (USD)",
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
        grid.DataSource = totals;
        var caption = new Label
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            Padding = new Padding(12),
            Text = $"Recorded: {totals.Sum(t => t.RecordedAmount):N2} USD | Business portion: {totals.Sum(t => t.BusinessAmount):N2} USD\n" +
                "Not a tax deduction calculation. Review vehicle costs, meals, and capital assets with your tax adviser.\n" +
                (annual ? "Includes all state jurisdictions and period types; overlapping entries are not deduplicated." : "Includes all active expenses in this period, regardless of filter tab.")
        };
        dialog.Controls.Add(grid);
        dialog.Controls.Add(caption);
        dialog.ShowDialog(this);
    }

    private void Export(bool pdf)
    {
        if (_period is null || _editor.IsEditing) return;
        using var dialog = new SaveFileDialog
        {
            Filter = pdf ? "PDF document|*.pdf" : "CSV file|*.csv",
            FileName = $"expenses-{_period.Year}-{_period.Kind}-{_period.Number}-{(_period.StateCode.Length == 0 ? "all" : _period.StateCode)}",
            DefaultExt = pdf ? "pdf" : "csv",
            AddExtension = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var expenses = Store.GetExpenses(_period.Id);
        if (pdf) PdfExport.Write(dialog.FileName, $"Expense summary - {_period}", expenses);
        else File.WriteAllText(dialog.FileName, BookkeepingStore.Csv(expenses), new UTF8Encoding(true));
        _status.Text = $"Exported {expenses.Count} expenses (all tabs) to {dialog.FileName}";
    }

    private void ShowOptions()
    {
        using var dialog = new OptionsDialog(_preferences);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        string originalFolder = Path.GetDirectoryName(Store.DatabasePath)!;
        Store.Relocate(dialog.Draft.DatabaseFolder);
        try
        {
            _preferencesStore!.Save(dialog.Draft);
        }
        catch
        {
            // The old database remains intact if saving the new settings fails.
            _store = new BookkeepingStore(originalFolder);
            throw;
        }
        _preferences = dialog.Draft;
    }

    private void QueuePreferencesSave()
    {
        if (!_loaded) return;
        _layoutTimer.Stop();
        _layoutTimer.Start();
    }

    private void SavePreferences()
    {
        if (!_loaded || _preferencesStore is null) return;
        var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
        _preferences.Window = new WindowLayout
        {
            X = bounds.X,
            Y = bounds.Y,
            Width = bounds.Width,
            Height = bounds.Height,
            HasPosition = true,
            Maximized = WindowState == FormWindowState.Maximized
                || (WindowState == FormWindowState.Minimized && _preferences.Window.Maximized)
        };
        _preferences.NavigationWidth = _navigation.SplitterDistance;
        _preferences.ResultsHeight = _details.SplitterDistance;
        _preferencesStore.Save(_preferences);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_editor.IsEditing)
        {
            MessageBox.Show(this, "Save, Cancel, or Stash the expense before exiting.", "Expense edit in progress");
            e.Cancel = true;
            base.OnFormClosing(e);
            return;
        }
        base.OnFormClosing(e);
        if (e.Cancel || _store is null) return;
        _layoutTimer.Stop();
        try
        {
            SavePreferences();
            Store.Backup(Path.Combine(Path.GetDirectoryName(Store.DatabasePath)!, "Backups"));
            if (!string.IsNullOrWhiteSpace(_preferences.BackupFolder)) Store.Backup(_preferences.BackupFolder);
        }
        catch (Exception ex)
        {
            e.Cancel = MessageBox.Show(this, $"The session backup or settings save failed:\n{ex.Message}\n\nExit anyway?",
                "Session could not be saved completely", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes;
        }
    }
}
