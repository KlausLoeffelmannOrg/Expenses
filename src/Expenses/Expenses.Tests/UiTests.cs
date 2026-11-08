using System.Runtime.ExceptionServices;
using System.Globalization;
using Expenses.App;
using Expenses.App.Dialogs;
using Expenses.App.Views;
using Expenses.Core;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using PdfSharp.Pdf.IO;
using WarpToolkit.WinForms.Containers;
using Xunit;

namespace Expenses.Tests;

public sealed class UiTests
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

    [Theory]
    [InlineData("0.29")]
    [InlineData("12.34")]
    [InlineData("123456789.99")]
    public void MaskedEditorRoundTripsMoneyAndDatesAndKeepsDetailsHiddenWithoutSelection(string amount) => Sta(() =>
    {
        using var editor = new ExpenseEditor();
        Assert.False(editor.IsEditing);
        Assert.False(Descendants(editor).OfType<MaskedTextBox>().First().Visible);
        var input = new ExpenseInput(0, Guid.Empty, "02/29/2024", "Payee", "Purpose", "Fuel", "", "", amount, 100, "");
        editor.LoadInput(input, true);
        Assert.Equal(amount, editor.ReadInput().Amount);
        Assert.Equal(input.PaidDate, editor.ReadInput().PaidDate);
        Assert.True(editor.IsEditing);
        editor.SetEditing(false);
        Assert.All(Descendants(editor).OfType<TextBoxBase>(), box => Assert.True(box.ReadOnly || !box.Enabled));
    });

    [Fact]
    public void OptionsUseFluentTabsAndCancelDoesNotMutateLiveSettings() => Sta(() =>
    {
        var preferences = new AppPreferences { SettingsFolder = @"C:\OriginalSettings", BackupFolder = "" };
        using var dialog = new OptionsDialog(preferences);
        Assert.Single(dialog.Controls.OfType<FluentTabControl>());
        var editor = Descendants(dialog).OfType<TextBox>().First(b => b.Text == preferences.SettingsFolder);
        editor.Text = @"C:\ChangedSettings";
        dialog.DialogResult = DialogResult.Cancel;
        Assert.Equal(@"C:\OriginalSettings", preferences.SettingsFolder);
        Assert.DoesNotContain(Descendants(dialog), c => c is TabControl);
    });

    [Fact]
    public void EditorAndCsvUseGregorianUsDatesRegardlessOfWindowsCulture() => Sta(() =>
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("th-TH");
        using var editor = new ExpenseEditor();
        var expense = new Expense { PaidDate = new DateOnly(2024, 2, 29), AmountCents = 1234 };
        editor.ShowExpense(expense);
        Assert.Equal("02/29/2024", editor.ReadInput().PaidDate);
        Assert.StartsWith("2024-02-29,", BookkeepingStore.Csv([expense]).Split('\n')[1]);
    });

    [Fact]
    public void MainWindowFiltersLocksNavigationAndRestoresLayoutAndCreatesExitBackup() => Sta(() =>
    {
        string root = Path.Combine(Path.GetTempPath(), "ExpensesUiTests-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new BookkeepingStore(Path.Combine(root, "Data"));
            var period = store.AddPeriod(2024, PeriodKind.Month, 2, "");
            store.SaveExpense(period.Id, new(0, Guid.Empty, "02/15/2024", "Station", "Work travel", "Fuel", "", "", "12.34", 100, ""));
            store.SaveExpense(period.Id, new(0, Guid.Empty, "02/16/2024", "Shop", "Camera part", "Supplies", "Sub Camera", "", "42.00", 100, ""));
            var preferences = new AppPreferences
            {
                SettingsFolder = Path.Combine(root, "Settings"),
                DatabaseFolder = Path.Combine(root, "Data"),
                BackupFolder = "",
                ShowRemindersOnStartup = false,
                LastPeriodId = period.Id,
                ColumnWidths = new() { [nameof(Expense.Payee)] = 235 }
            };
            var preferencesStore = new PreferencesStore(Path.Combine(root, "Bootstrap"));
            using var provider = new ServiceCollection().AddSingleton(store).AddSingleton(preferences)
                .AddSingleton(preferencesStore).BuildServiceProvider();
            using var form = new MainForm(provider);
            form.Show();
            Application.DoEvents();
            var results = Descendants(form).OfType<ExpenseResultsView>().ToList();
            Assert.Equal(3, results.Count);
            Assert.Equal(2, ((List<Expense>)results[0].Grid.DataSource!).Count);
            Assert.Single((List<Expense>)results[1].Grid.DataSource!);
            Assert.Single((List<Expense>)results[2].Grid.DataSource!);
            Assert.Equal(235, results[0].Grid.Columns[nameof(Expense.Payee)]!.Width);
            var editor = Descendants(form).OfType<ExpenseEditor>().Single();
            var tree = Descendants(form).OfType<PeriodTreeView>().Single();
            Assert.Equal(TreeViewDrawMode.OwnerDrawAll, tree.DrawMode);
            var tabs = Descendants(form).OfType<FluentTabControl>().Single();
            tabs.CurrentTabIndex = 1;
            results[1].Grid.CurrentCell = results[1].Grid.Rows[0].Cells[0];
            Assert.Equal("Station", editor.ReadInput().Payee);
            tabs.CurrentTabIndex = 2;
            results[2].Grid.CurrentCell = results[2].Grid.Rows[0].Cells[0];
            Assert.Equal("Shop", editor.ReadInput().Payee);
            Assert.False(editor.IsEditing);
            var editMenu = form.MainMenuStrip!.Items.OfType<ToolStripMenuItem>().Single(i => i.Text == "&Edit");
            var commands = editMenu.DropDownItems.OfType<ToolStripMenuItem>().ToList();
            commands.Single(c => c.ShortcutKeys == Keys.F2).PerformClick();
            Assert.True(editor.IsEditing);
            Assert.False(tree.Enabled);
            commands.Single(c => c.Text == "&Cancel edit").PerformClick();
            var newCommand = commands.Single(c => c.ShortcutKeys == (Keys.Control | Keys.N));
            Assert.True(newCommand.Enabled);
            newCommand.PerformClick();
            Assert.True(editor.IsEditing);
            Assert.False(tree.Enabled);
            Assert.False(newCommand.Enabled);
            Assert.False(tabs.Enabled);
            commands.Single(c => c.Text == "&Cancel edit").PerformClick();
            Assert.False(editor.IsEditing);
            Assert.True(tree.Enabled);
            newCommand.PerformClick();
            editor.LoadInput(new(0, Guid.Empty, "02/17/2024", "New vendor", "Office supplies",
                "Supplies", "", "", "15.25", 100, ""), true);
            commands.Single(c => c.ShortcutKeys == (Keys.Control | Keys.S)).PerformClick();
            Assert.False(editor.IsEditing);
            Assert.Equal(3, store.GetExpenses(period.Id).Count);
            Assert.Equal(1525, store.GetExpenses(period.Id).Single(e => e.Payee == "New vendor").AmountCents);
            results[0].Grid.Columns[nameof(Expense.Payee)]!.Width = 250;
            form.Close();
            Assert.Equal(250, preferencesStore.Load().ColumnWidths[nameof(Expense.Payee)]);
            Assert.Single(Directory.GetFiles(Path.Combine(root, "Data", "Backups"), "*.db"));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    });

    [Fact]
    public void PdfExportProducesReadableMultipageDocument() => Sta(() =>
    {
        string path = Path.Combine(Path.GetTempPath(), $"ExpensesPdfTests-{Guid.NewGuid():N}.pdf");
        try
        {
            var expenses = Enumerable.Range(1, 80).Select(i => new Expense
            {
                PaidDate = new DateOnly(2024, 2, 15),
                Payee = $"Vendor {i}",
                Description = new string('W', 250),
                Category = "Supplies",
                AmountCents = 1234,
                BusinessUsePercent = 75
            }).ToList();
            PdfExport.Write(path, "Expense summary", expenses);
            using var document = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            Assert.True(document.PageCount > 1);
            Assert.Equal("Expense summary", document.Info.Title);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    });
}
