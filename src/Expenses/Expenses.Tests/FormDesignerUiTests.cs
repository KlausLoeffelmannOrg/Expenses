using System.Runtime.ExceptionServices;
using Expenses.App;
using Expenses.App.Dialogs;
using Expenses.App.Views;
using Expenses.Core;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Expenses.Tests;

public sealed class FormDesignerUiTests
{
    private static void Sta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { error = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(45)), "UI check did not finish.");
        if (error is not null) ExceptionDispatchInfo.Capture(error).Throw();
    }

    private static IEnumerable<Control> Descendants(Control parent) =>
        parent.Controls.Cast<Control>().SelectMany(c => new[] { c }.Concat(Descendants(c)));

    private static Form CreateDialog(string name) => name switch
    {
        "Options" => new OptionsDialog(),
        "Period" => new PeriodDialog(),
        "TextEntry" => new TextEntryDialog(),
        "Reminder" => new ReminderDialog(),
        "ExpenseReport" => new ExpenseReportDialog(),
        "ReportYear" => new ReportYearDialog(),
        _ => throw new ArgumentOutOfRangeException(nameof(name))
    };

    [Theory]
    [InlineData("Options")]
    [InlineData("Period")]
    [InlineData("TextEntry")]
    [InlineData("Reminder")]
    [InlineData("ExpenseReport")]
    [InlineData("ReportYear")]
    public void ParameterlessDialogsExposeTheirStaticCanvasAndKeepFooterReachableWhenResized(string name) => Sta(() =>
    {
        using var dialog = CreateDialog(name);
        Assert.NotNull(dialog.AcceptButton);
        Assert.NotNull(dialog.CancelButton);
        dialog.Show();
        using var largeFont = new Font(dialog.Font.FontFamily, 18F);
        dialog.Font = largeFont;
        dialog.Size = new Size(dialog.MinimumSize.Width + 50, dialog.MinimumSize.Height + 50);
        dialog.PerformLayout();
        Application.DoEvents();

        var footer = Descendants(dialog).OfType<TableLayoutPanel>().Single(c => c.Name == "_footer");
        var footerBounds = dialog.RectangleToClient(footer.RectangleToScreen(footer.ClientRectangle));
        Assert.True(dialog.ClientRectangle.Contains(footerBounds), $"Footer outside {name} client bounds: {footerBounds}");
        var accept = Assert.IsType<Button>(dialog.AcceptButton);
        var cancel = Assert.IsType<Button>(dialog.CancelButton);
        Assert.True(accept.Visible);
        Assert.True(cancel.Visible);
        Assert.True(accept.Width > 0 && accept.Height > 0);
        Assert.True(footer.ClientRectangle.Contains(accept.Bounds));
        Assert.True(footer.ClientRectangle.Contains(cancel.Bounds));
        Assert.InRange(Math.Abs(accept.Width - cancel.Width), 0, 1);
        if (name == "Options")
        {
            Assert.Single(Descendants(dialog).OfType<StorageOptionsView>());
            Assert.Single(Descendants(dialog).OfType<ReminderOptionsView>());
        }
        if (name == "ExpenseReport")
            Assert.True(Descendants(dialog).OfType<DataGridView>().Single().Height > 0);
        dialog.Close();
    });

    [Fact]
    public void ParameterlessMainWindowKeepsAllFixedCommandsAndResultsOnItsCanvasWithoutServices() => Sta(() =>
    {
        using var form = new MainForm();
        Assert.Null(((IServiceProvider)form).GetService(typeof(BookkeepingStore)));
        Assert.Equal(new[] { "&File", "&Edit", "&View", "&Tools" },
            form.MainMenuStrip!.Items.Cast<ToolStripMenuItem>().Select(i => i.Text));
        var edit = form.MainMenuStrip.Items.Cast<ToolStripMenuItem>().Single(i => i.Text == "&Edit");
        Assert.Contains(edit.DropDownItems.OfType<ToolStripMenuItem>(), i => i.ShortcutKeys == (Keys.Control | Keys.N));
        Assert.Contains(edit.DropDownItems.OfType<ToolStripMenuItem>(), i => i.ShortcutKeys == (Keys.Control | Keys.S));
        Assert.Contains(edit.DropDownItems.OfType<ToolStripMenuItem>(), i => i.ShortcutKeys == Keys.F2);
        Assert.Equal("Esc", edit.DropDownItems.OfType<ToolStripMenuItem>().Single(i => i.Text == "&Cancel edit").ShortcutKeyDisplayString);
        Assert.Equal(7, Descendants(form).OfType<ToolStrip>().Single(s => s.Name == "_toolbar").Items.Count);
        form.Show();
        Application.DoEvents();
        var results = Descendants(form).OfType<ExpenseResultsView>().ToArray();
        Assert.Equal(3, results.Length);
        Assert.All(results, view =>
        {
            Assert.Null(view.Grid.DataSource);
        });
        var tabs = Descendants(form).OfType<TabControl>().Single();
        Assert.Equal(3, tabs.TabPages.Count);
        foreach (TabPage page in tabs.TabPages)
        {
            tabs.SelectedTab = page;
            Assert.True(page.Controls.OfType<ExpenseResultsView>().Single().Grid.Visible);
        }
        form.Close();
    });

    [Fact]
    public void MainRuntimeConstructorStoresProviderWithoutResolvingProductionServices() => Sta(() =>
    {
        var provider = new RejectingProvider();
        using var form = new MainForm(provider);
        Assert.Equal(0, provider.Requests);
        Assert.Equal(3, Descendants(form).OfType<ExpenseResultsView>().Count());
    });

    [Fact]
    public void TextEntryModesKeepDraftLimitsAndReminderScopeAndScrollableInput() => Sta(() =>
    {
        using var draft = new TextEntryDialog("Stash expense draft", "&Draft name:");
        draft.Show();
        var draftText = Descendants(draft).OfType<TextBox>().Single();
        Assert.False(draftText.Multiline);
        Assert.Equal(150, draftText.MaxLength);
        Assert.False(Descendants(draft).OfType<CheckBox>().Single().Visible);
        draftText.Text = "  Camera draft  ";
        Assert.Equal("Camera draft", draft.EnteredText);
        draft.Close();

        using var reminder = new TextEntryDialog("Add note or reminder", "&Note / reminder:", true, true);
        reminder.Show();
        var reminderText = Descendants(reminder).OfType<TextBox>().Single();
        Assert.True(reminderText.Multiline);
        Assert.True(reminderText.AcceptsReturn);
        Assert.Equal(10000, reminderText.MaxLength);
        Assert.Equal(ScrollBars.Vertical, reminderText.ScrollBars);
        var scope = Descendants(reminder).OfType<CheckBox>().Single();
        Assert.True(scope.Visible && scope.Enabled);
        scope.Checked = true;
        reminderText.Text = "Remember receipt";
        ((Button)reminder.AcceptButton!).PerformClick();
        Assert.True(reminder.PeriodOnly);
        Assert.Equal(DialogResult.OK, reminder.DialogResult);
        reminder.Close();

        using var global = new TextEntryDialog("Add note or reminder", "&Note / reminder:", true, false);
        Assert.False(Descendants(global).OfType<CheckBox>().Single().Enabled);
    });

    [Fact]
    public void OptionsRuntimeUsesOneDraftAndSelectsDesignerCategories() => Sta(() =>
    {
        var preferences = new AppPreferences { SettingsFolder = @"C:\Original", ColumnWidths = new() { ["Payee"] = 200 } };
        using var dialog = new OptionsDialog(preferences);
        var categories = Descendants(dialog).OfType<UserControl>().ToArray();
        Assert.Equal(2, categories.Length);
        var storage = categories.OfType<StorageOptionsView>().Single();
        var reminders = categories.OfType<ReminderOptionsView>().Single();
        var input = Descendants(storage).OfType<TextBox>().Single(t => t.Text == @"C:\Original");
        input.Text = @"C:\Changed";
        dialog.Draft.ColumnWidths["Payee"] = 300;
        dialog.Show();
        Application.DoEvents();
        var tabs = dialog.Controls.OfType<ToolStrip>().Single();
        Assert.Equal(2, tabs.Items.Count);
        Assert.True(storage.Visible);
        tabs.Items[1].PerformClick();
        Assert.True(reminders.Visible);
        Assert.False(storage.Visible);
        Assert.Equal(2, Descendants(dialog).OfType<UserControl>().Count());
        dialog.DialogResult = DialogResult.Cancel;
        dialog.Close();
        Assert.Equal(@"C:\Original", preferences.SettingsFolder);
        Assert.Equal(200, preferences.ColumnWidths["Payee"]);
    });

    [Fact]
    public void PeriodRuntimeModesConstrainQuarterAndYearAndSelectLoadedPeriods() => Sta(() =>
    {
        string root = Path.Combine(Environment.CurrentDirectory, $"ExpensesDesignerTests-{Guid.NewGuid():N}");
        try
        {
            var store = new BookkeepingStore(root);
            using var create = new PeriodDialog(store, true);
            create.Show();
            var kind = Descendants(create).OfType<ComboBox>().Single();
            var number = Descendants(create).OfType<NumericUpDown>().Single(n => n.Maximum == 12);
            number.Value = 12;
            kind.SelectedIndex = 1;
            Assert.Equal(4M, number.Maximum);
            Assert.Equal(4M, number.Value);
            kind.SelectedIndex = 2;
            Assert.Equal(1M, number.Maximum);
            Assert.Equal(1M, number.Value);
            Assert.False(number.Enabled);
            ((Button)create.AcceptButton!).PerformClick();
            Assert.Equal(PeriodKind.Year, create.SelectedPeriod!.Kind);
            Assert.Equal(DialogResult.OK, create.DialogResult);
            create.Close();

            using var open = new PeriodDialog(store, false);
            Assert.Null(Descendants(open).OfType<ListBox>().Single().DataSource);
            open.Show();
            var list = Descendants(open).OfType<ListBox>().Single();
            Assert.True(list.Visible);
            Assert.Single(list.Items.Cast<BookkeepingPeriod>());
            ((Button)open.AcceptButton!).PerformClick();
            Assert.Equal(create.SelectedPeriod.Id, open.SelectedPeriod!.Id);
            open.Close();
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    });

    [Fact]
    public void NamedReminderAndReportDialogsRetainContentAndDismissalSemantics() => Sta(() =>
    {
        using var reminders = new ReminderDialog("Global notes and reminders",
            new[] { new Reminder { Text = "Bring receipts" } });
        var reminderText = Descendants(reminders).OfType<TextBox>().Single();
        Assert.True(reminderText.ReadOnly);
        Assert.Contains("Bring receipts", reminderText.Text);
        Assert.Equal("Dismiss all shown", ((Button)reminders.AcceptButton!).Text);
        Assert.Equal(DialogResult.OK, ((Button)reminders.AcceptButton).DialogResult);
        Assert.Equal("Keep for next time", ((Button)reminders.CancelButton!).Text);
        Assert.Equal(DialogResult.Cancel, ((Button)reminders.CancelButton).DialogResult);

        var totals = new[] { new CategoryTotal("Fuel", 12.34M, 6.17M) };
        using var report = new ExpenseReportDialog("Annual expense report - 2024", totals, true);
        var grid = Descendants(report).OfType<DataGridView>().Single();
        Assert.Same(totals, grid.DataSource);
        Assert.False(grid.AutoGenerateColumns);
        Assert.Equal(3, grid.Columns.Count);
        Assert.Contains(Descendants(report).OfType<Label>(), l => l.Text.Contains("overlapping entries are not deduplicated"));
        Assert.Same(report.AcceptButton, report.CancelButton);
        using var year = new ReportYearDialog(2024);
        Assert.Equal(2024, year.SelectedYear);
        Assert.Equal("View report", ((Button)year.AcceptButton!).Text);
    });

    private sealed class RejectingProvider : IServiceProvider
    {
        public int Requests { get; private set; }

        public object? GetService(Type serviceType)
        {
            Requests++;
            throw new InvalidOperationException("Services must not be resolved during construction.");
        }
    }
}
