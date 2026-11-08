using System.Text.Json;
using Expenses.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Expenses.Tests;

public sealed class BookkeepingTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "ExpensesTests-" + Guid.NewGuid().ToString("N"));
    private readonly BookkeepingStore _store;
    public BookkeepingTests() => _store = new BookkeepingStore(Path.Combine(_root, "Data"));

    private static ExpenseInput Input(string amount = "12.34", string date = "02/15/2024") =>
        new(0, Guid.Empty, date, "Station", "Client visit", "Fuel", "Sub Camera", "Business card", amount, 50, "");

    [Fact]
    public void MigrationsCreateSchemaAndStatePeriodsRemainDistinctAfterReopen()
    {
        var ca = _store.AddPeriod(2024, PeriodKind.Month, 2, "ca");
        _store.AddPeriod(2024, PeriodKind.Month, 2, "NY");
        Assert.Equal(new DateOnly(2024, 2, 29), ca.EndDate);
        Assert.Throws<ArgumentException>(() => _store.AddPeriod(2024, PeriodKind.Month, 2, "CA"));
        Assert.Equal(2, new BookkeepingStore(Path.Combine(_root, "Data")).GetPeriods().Count);
        using var db = _store.Open();
        Assert.Single(db.Database.GetAppliedMigrations());
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [Theory]
    [InlineData(PeriodKind.Month, 12, 12, 31)]
    [InlineData(PeriodKind.Quarter, 1, 3, 31)]
    [InlineData(PeriodKind.Quarter, 4, 12, 31)]
    [InlineData(PeriodKind.Year, 1, 12, 31)]
    public void PeriodBoundaries(PeriodKind kind, int number, int month, int day)
    {
        var period = BookkeepingPeriod.Create(2024, kind, number, "DC");
        Assert.Equal(new DateOnly(2024, month, day), period.EndDate);
    }

    [Fact]
    public void RejectsInvalidPeriodIdentities()
    {
        Assert.Throws<ArgumentException>(() => BookkeepingPeriod.Create(2024, PeriodKind.Quarter, 5, ""));
        Assert.Throws<ArgumentException>(() => BookkeepingPeriod.Create(2024, PeriodKind.Month, 2, "ZZ"));
        Assert.Throws<ArgumentException>(() => BookkeepingPeriod.Create(2024, (PeriodKind)99, 1, ""));
    }

    [Fact]
    public void ExpensesUseExactCentsAndDetectStaleEditsAndSoftDeletion()
    {
        var period = _store.AddPeriod(2024, PeriodKind.Month, 2, "");
        var saved = _store.SaveExpense(period.Id, Input("0.29"));
        Assert.Equal(29, saved.AmountCents);
        Assert.Equal(.15m, saved.BusinessAmount);
        var edit = Input("25.10") with { Id = saved.Id, Revision = saved.Revision };
        var updated = _store.SaveExpense(period.Id, edit);
        Assert.Throws<InvalidOperationException>(() => _store.SaveExpense(period.Id, edit));
        _store.DeleteExpense(updated.Id, updated.Revision);
        Assert.Empty(_store.GetExpenses(period.Id));
        Assert.Empty(_store.Report(2024, null));
        using var db = _store.Open();
        Assert.True(db.Expenses.IgnoreQueryFilters().Single().IsDeleted);
        Assert.Throws<InvalidOperationException>(() => _store.SaveExpense(period.Id, edit));
    }

    [Theory]
    [InlineData("0", "02/15/2024")]
    [InlineData("-1.00", "02/15/2024")]
    [InlineData("1.234", "02/15/2024")]
    [InlineData("1000000000.00", "02/15/2024")]
    [InlineData("1,23", "02/15/2024")]
    [InlineData("2.00", "02/30/2024")]
    [InlineData("2.00", "03/01/2024")]
    public void InvalidMoneyAndDatesDoNotWriteAnExpense(string amount, string date)
    {
        var period = _store.AddPeriod(2024, PeriodKind.Month, 2, "");
        Assert.Throws<ArgumentException>(() => _store.SaveExpense(period.Id, Input(amount, date)));
        Assert.Empty(_store.GetExpenses(period.Id));
    }

    [Fact]
    public void RequiredFieldsAndBusinessAllocationAreValidated()
    {
        var period = _store.AddPeriod(2024, PeriodKind.Month, 2, "");
        Assert.Throws<ArgumentException>(() => _store.SaveExpense(period.Id, Input() with { Payee = " " }));
        Assert.Throws<ArgumentException>(() => _store.SaveExpense(period.Id, Input() with { Description = "" }));
        Assert.Throws<ArgumentException>(() => _store.SaveExpense(period.Id, Input() with { BusinessUsePercent = 101 }));
    }

    [Fact]
    public void NamedIncompleteDraftSurvivesRestartAndCommitConsumesItAtomically()
    {
        var period = _store.AddPeriod(2024, PeriodKind.Month, 2, "");
        int id = _store.Stash(period.Id, "Waiting for receipt", Input() with { Amount = "", PaidDate = "02/  /2024" }, null);
        var reopened = new BookkeepingStore(Path.Combine(_root, "Data"));
        var draft = Assert.Single(reopened.GetDrafts(period.Id));
        Assert.Equal("Waiting for receipt", draft.Name);
        Assert.Equal("", JsonSerializer.Deserialize<ExpenseInput>(draft.Payload)!.Amount);
        Assert.Empty(reopened.GetExpenses(period.Id));
        Assert.Throws<ArgumentException>(() => reopened.SaveExpense(period.Id, Input("0"), id));
        Assert.Single(reopened.GetDrafts(period.Id));
        reopened.SaveExpense(period.Id, Input(), id);
        Assert.Empty(reopened.GetDrafts(period.Id));
        Assert.Single(reopened.GetExpenses(period.Id));
    }

    [Fact]
    public void ReminderScopeAndDismissalPersist()
    {
        var period = _store.AddPeriod(2024, PeriodKind.Month, 2, "");
        _store.AddReminder(null, "Global");
        _store.AddReminder(period.Id, "Period");
        var global = Assert.Single(_store.GetReminders(null));
        Assert.Equal("Period", Assert.Single(_store.GetReminders(period.Id)).Text);
        _store.DismissReminders([global.Id]);
        Assert.Empty(_store.GetReminders(null));
        using var db = _store.Open();
        Assert.Equal(2, db.Reminders.Count());
    }

    [Fact]
    public void ReportsAreScopedAndAnnualReportAggregatesJurisdictions()
    {
        var ca = _store.AddPeriod(2024, PeriodKind.Month, 2, "CA");
        var ny = _store.AddPeriod(2024, PeriodKind.Month, 2, "NY");
        var later = _store.AddPeriod(2025, PeriodKind.Year, 1, "");
        _store.SaveExpense(ca.Id, Input("10.00"));
        _store.SaveExpense(ny.Id, Input("20.00"));
        _store.SaveExpense(later.Id, Input("99.00", "02/15/2025"));
        Assert.Equal(10m, Assert.Single(_store.Report(2024, ca.Id)).RecordedAmount);
        Assert.Equal(30m, Assert.Single(_store.Report(2024, null)).RecordedAmount);
        Assert.Equal(15m, Assert.Single(_store.Report(2024, null)).BusinessAmount);
    }

    [Fact]
    public void ReceiptCopiesAndOnlineBackupsRemainReadableAndRelocationKeepsData()
    {
        var period = _store.AddPeriod(2024, PeriodKind.Month, 2, "");
        string original = Path.Combine(_root, "original.txt");
        File.WriteAllText(original, "Receipt evidence");
        var saved = _store.SaveExpense(period.Id, Input() with { ReceiptPath = original });
        File.Delete(original);
        Assert.Equal("Receipt evidence", File.ReadAllText(Path.Combine(Path.GetDirectoryName(_store.DatabasePath)!, saved.ReceiptPath)));
        string backup = _store.Backup(Path.Combine(_root, "Backup"));
        using (var db = new ExpensesDbContext(new DbContextOptionsBuilder<ExpensesDbContext>().UseSqlite($"Data Source={backup}").Options))
            Assert.Single(db.Expenses);
        Assert.Single(Directory.GetFiles(Path.ChangeExtension(backup, null) + "-Receipts"));
        string moved = Path.Combine(_root, "Moved");
        _store.Relocate(moved);
        Assert.Single(new BookkeepingStore(moved).GetExpenses(period.Id));
        Assert.True(File.Exists(Path.Combine(moved, saved.ReceiptPath)));
        Assert.Throws<IOException>(() => _store.Relocate(Path.Combine(_root, "Data")));
    }

    [Fact]
    public void CsvEscapesQuotesNewlinesAndSpreadsheetFormulas()
    {
        var expense = new Expense
        {
            PaidDate = new DateOnly(2024, 2, 15),
            Payee = "=HYPERLINK(\"bad\")",
            Description = "A,\r\nB",
            AmountCents = 1234,
            BusinessUsePercent = 50
        };
        string csv = BookkeepingStore.Csv([expense]);
        Assert.Contains("\"'=HYPERLINK(\"\"bad\"\")\"", csv);
        Assert.Contains("\"A,\r\nB\"", csv);
        Assert.Contains(",12.34,50,6.17,", csv);
    }

    [Fact]
    public void CustomSettingsFolderIsFoundFromStablePointer()
    {
        var preferencesStore = new PreferencesStore(Path.Combine(_root, "Bootstrap"));
        var preferences = new AppPreferences
        {
            SettingsFolder = Path.Combine(_root, "CustomSettings"),
            DatabaseFolder = Path.Combine(_root, "Data"),
            BackupFolder = "",
            ShowRemindersOnStartup = false,
            ColumnWidths = new() { ["Payee"] = 235 }
        };
        preferencesStore.Save(preferences);
        var loaded = preferencesStore.Load();
        Assert.Equal(preferences.SettingsFolder, loaded.SettingsFolder);
        Assert.Equal(235, loaded.ColumnWidths["Payee"]);
        Assert.False(loaded.ShowRemindersOnStartup);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_root, true);
    }
}
