using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Core;

public sealed class BookkeepingStore
{
    public string DatabasePath { get; private set; }
    private static readonly CultureInfo UsCulture = CultureInfo.GetCultureInfo("en-US");

    public BookkeepingStore(string databaseFolder)
    {
        Directory.CreateDirectory(databaseFolder);
        DatabasePath = Path.Combine(databaseFolder, "expenses.db");
        using var db = Open();
        db.Database.Migrate();
    }

    public ExpensesDbContext Open() => new(new DbContextOptionsBuilder<ExpensesDbContext>()
        .UseSqlite(new SqliteConnectionStringBuilder { DataSource = DatabasePath, ForeignKeys = true }.ToString()).Options);

    public List<BookkeepingPeriod> GetPeriods()
    {
        using var db = Open();
        return db.Periods.AsNoTracking().OrderByDescending(p => p.StartDate).ThenBy(p => p.StateCode).ToList();
    }

    public BookkeepingPeriod AddPeriod(int year, PeriodKind kind, int number, string? state)
    {
        var period = BookkeepingPeriod.Create(year, kind, number, state);
        using var db = Open();
        if (db.Periods.Any(p => p.Year == year && p.Kind == kind && p.Number == number && p.StateCode == period.StateCode))
            throw new ArgumentException("That bookkeeping period and state jurisdiction already exist.");
        db.Periods.Add(period);
        db.SaveChanges();
        return period;
    }

    public List<Expense> GetExpenses(int periodId)
    {
        using var db = Open();
        return db.Expenses.AsNoTracking().Where(e => e.BookkeepingPeriodId == periodId)
            .OrderByDescending(e => e.PaidDate).ThenByDescending(e => e.Id).ToList();
    }

    public BookkeepingPeriod AddDemoData()
    {
        using var db = Open();
        using var transaction = db.Database.BeginTransaction();
        int year = 2024;
        while (db.Periods.Any(p => p.Year == year && p.Kind == PeriodKind.Year && p.StateCode == ""))
            if (++year > 9998) throw new InvalidOperationException("No unused annual period is available for demo data.");
        var period = BookkeepingPeriod.Create(year, PeriodKind.Year, 1, "");
        db.Periods.Add(period);
        db.SaveChanges();
        (string Payee, string Description, string Category, long Cents, int Business, string Tags)[] samples =
        [
            ("Demo Fuel Stop", "Fictional client visit", "Fuel", 6234, 75, ""),
            ("Demo Camera Shop", "Fictional camera accessories", "Supplies", 12995, 100, "Sub Camera"),
            ("Demo Office", "Fictional stationery", "Supplies", 2875, 100, ""),
            ("Demo Hosting", "Fictional website hosting", "Other expenses", 1900, 100, ""),
            ("Demo Cafe", "Fictional client meeting", "Meals", 4250, 50, ""),
            ("Demo Parking", "Fictional business parking", "Travel", 1500, 100, "")
        ];
        for (int i = 0; i < samples.Length; i++)
        {
            var sample = samples[i];
            db.Expenses.Add(new Expense
            {
                BookkeepingPeriodId = period.Id, PaidDate = new DateOnly(year, 2, i + 1),
                Payee = sample.Payee, Description = sample.Description, Category = sample.Category,
                AmountCents = sample.Cents, BusinessUsePercent = sample.Business,
                Tags = sample.Tags.Length == 0 ? "DEMO" : $"DEMO, {sample.Tags}", PaymentReference = "Demo only"
            });
        }
        db.SaveChanges();
        transaction.Commit();
        return period;
    }

    public Expense SaveExpense(int periodId, ExpenseInput input, int? draftId = null)
    {
        using var db = Open();
        var period = db.Periods.Single(p => p.Id == periodId);
        if (!DateOnly.TryParseExact(input.PaidDate, "MM/dd/yyyy", UsCulture, DateTimeStyles.None, out var paidDate)
            || paidDate < period.StartDate || paidDate > period.EndDate)
            throw new ArgumentException($"Date paid must be a valid MM/DD/YYYY date within {period.StartDate.ToString("MM/dd/yyyy", UsCulture)} - {period.EndDate.ToString("MM/dd/yyyy", UsCulture)}.");
        if (!decimal.TryParse(input.Amount, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
            UsCulture, out decimal amount) || amount <= 0 || amount > 999999999.99m || decimal.Round(amount, 2) != amount)
            throw new ArgumentException("Amount must be greater than zero, at most 999,999,999.99, with no more than two decimal places (USD).");
        if (string.IsNullOrWhiteSpace(input.Payee) || string.IsNullOrWhiteSpace(input.Description) || string.IsNullOrWhiteSpace(input.Category))
            throw new ArgumentException("Payee, business purpose/description, and category are required.");
        if (input.BusinessUsePercent is < 0 or > 100)
            throw new ArgumentException("Business use must be between 0 and 100 percent.");
        Expense expense;
        if (input.Id == 0)
        {
            expense = new Expense { BookkeepingPeriodId = periodId };
            db.Expenses.Add(expense);
        }
        else
        {
            expense = db.Expenses.SingleOrDefault(e => e.Id == input.Id && e.BookkeepingPeriodId == periodId)
                ?? throw new InvalidOperationException("The expense no longer exists or has been deleted. Cancel and refresh.");
            if (expense.Revision != input.Revision)
                throw new InvalidOperationException("The expense changed in another session. Stash your draft, then reopen the current record.");
        }
        expense.PaidDate = paidDate;
        expense.AmountCents = checked((long)(amount * 100));
        expense.Payee = input.Payee.Trim();
        expense.Description = input.Description.Trim();
        expense.Category = input.Category.Trim();
        expense.Tags = input.Tags.Trim();
        expense.PaymentReference = input.PaymentReference.Trim();
        expense.BusinessUsePercent = input.BusinessUsePercent;
        expense.ReceiptPath = ImportReceipt(input.ReceiptPath);
        expense.UpdatedAt = DateTimeOffset.UtcNow;
        expense.Revision = Guid.NewGuid();
        if (draftId.HasValue)
            db.Drafts.Remove(db.Drafts.Single(d => d.Id == draftId && d.BookkeepingPeriodId == periodId));
        db.SaveChanges();
        return expense;
    }

    private string ImportReceipt(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "";
        string root = Path.GetDirectoryName(DatabasePath)!;
        string fullPath = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));
        string receiptFolder = Path.Combine(root, "Receipts");
        if (!File.Exists(fullPath)) throw new FileNotFoundException("The receipt file cannot be found.", fullPath);
        if (Path.GetDirectoryName(fullPath)!.Equals(receiptFolder, StringComparison.OrdinalIgnoreCase))
            return Path.GetRelativePath(root, fullPath);
        Directory.CreateDirectory(receiptFolder);
        string name = Guid.NewGuid().ToString("N") + Path.GetExtension(fullPath);
        File.Copy(fullPath, Path.Combine(receiptFolder, name));
        return Path.Combine("Receipts", name);
    }

    public void DeleteExpense(int id, Guid revision)
    {
        using var db = Open();
        var expense = db.Expenses.Single(e => e.Id == id);
        if (expense.Revision != revision) throw new InvalidOperationException("The expense changed. Refresh before deleting it.");
        expense.IsDeleted = true;
        expense.Revision = Guid.NewGuid();
        expense.UpdatedAt = DateTimeOffset.UtcNow;
        db.SaveChanges();
    }

    public List<ExpenseDraft> GetDrafts(int periodId)
    {
        using var db = Open();
        return db.Drafts.AsNoTracking().Where(d => d.BookkeepingPeriodId == periodId).ToList()
            .OrderByDescending(d => d.SavedAt).ToList();
    }

    public int Stash(int periodId, string name, ExpenseInput input, int? draftId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Enter a name for the stashed draft.");
        using var db = Open();
        var draft = draftId.HasValue
            ? db.Drafts.Single(d => d.Id == draftId && d.BookkeepingPeriodId == periodId)
            : new ExpenseDraft { BookkeepingPeriodId = periodId };
        if (!draftId.HasValue) db.Drafts.Add(draft);
        draft.Name = name.Trim();
        draft.SavedAt = DateTimeOffset.UtcNow;
        draft.Payload = JsonSerializer.Serialize(input);
        db.SaveChanges();
        return draft.Id;
    }

    public void AddReminder(int? periodId, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Enter a note or reminder.");
        using var db = Open();
        db.Reminders.Add(new Reminder { BookkeepingPeriodId = periodId, Text = text.Trim() });
        db.SaveChanges();
    }

    public List<Reminder> GetReminders(int? periodId)
    {
        using var db = Open();
        return db.Reminders.AsNoTracking().Where(r => !r.IsDismissed && r.BookkeepingPeriodId == periodId)
            .OrderBy(r => r.Id).ToList();
    }

    public void DismissReminders(IEnumerable<int> ids)
    {
        using var db = Open();
        foreach (var reminder in db.Reminders.Where(r => ids.Contains(r.Id))) reminder.IsDismissed = true;
        db.SaveChanges();
    }

    public List<CategoryTotal> Report(int year, int? periodId)
    {
        using var db = Open();
        var query = db.Expenses.AsNoTracking();
        query = periodId.HasValue ? query.Where(e => e.BookkeepingPeriodId == periodId.Value)
            : query.Where(e => db.Periods.Any(p => p.Id == e.BookkeepingPeriodId && p.Year == year));
        var expenses = query.ToList();
        return expenses.GroupBy(e => e.Category).OrderBy(g => g.Key)
            .Select(g => new CategoryTotal(g.Key, g.Sum(e => e.Amount), g.Sum(e => e.BusinessAmount))).ToList();
    }

    public string Backup(string folder)
    {
        Directory.CreateDirectory(folder);
        string destination = Path.Combine(folder, $"expenses-{DateTime.UtcNow:yyyyMMdd-HHmmss-fffffff}-{Guid.NewGuid():N}.db");
        using var source = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = DatabasePath }.ToString());
        using var target = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = destination, Pooling = false }.ToString());
        source.Open();
        target.Open();
        source.BackupDatabase(target);
        string receipts = Path.Combine(Path.GetDirectoryName(DatabasePath)!, "Receipts");
        if (Directory.Exists(receipts))
        {
            string receiptBackup = Path.ChangeExtension(destination, null) + "-Receipts";
            Directory.CreateDirectory(receiptBackup);
            foreach (string file in Directory.EnumerateFiles(receipts))
                File.Copy(file, Path.Combine(receiptBackup, Path.GetFileName(file)));
        }
        return destination;
    }

    public void Relocate(string folder)
    {
        string targetPath = Path.Combine(Path.GetFullPath(folder), "expenses.db");
        if (targetPath.Equals(Path.GetFullPath(DatabasePath), StringComparison.OrdinalIgnoreCase)) return;
        if (File.Exists(targetPath)) throw new IOException("The selected folder already contains an expenses database. Choose an empty folder.");
        string targetReceipts = Path.Combine(folder, "Receipts");
        if (Directory.Exists(targetReceipts)) throw new IOException("The selected folder already contains a Receipts folder.");
        Directory.CreateDirectory(folder);
        string snapshot = Backup(folder);
        File.Move(snapshot, targetPath);
        string receiptBackup = Path.ChangeExtension(snapshot, null) + "-Receipts";
        if (Directory.Exists(receiptBackup))
        {
            Directory.Move(receiptBackup, targetReceipts);
        }
        DatabasePath = targetPath;
    }

    public static string Csv(IEnumerable<Expense> expenses)
    {
        static string Cell(string value)
        {
            // Prevent spreadsheet formula evaluation without changing stored values.
            if (value.TrimStart().StartsWith('=') || value.TrimStart().StartsWith('+')
                || value.TrimStart().StartsWith('-') || value.TrimStart().StartsWith('@')
                || value.StartsWith('\t') || value.StartsWith('\r'))
                value = "'" + value;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        var csv = new StringBuilder("Date paid,Payee,Business purpose,Category,Tags,Payment reference,Amount USD,Business use percent,Business portion USD,Receipt file\r\n");
        foreach (var e in expenses)
            csv.AppendLine(string.Join(",", new[] { e.PaidDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), Cell(e.Payee), Cell(e.Description),
                Cell(e.Category), Cell(e.Tags), Cell(e.PaymentReference), e.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                e.BusinessUsePercent.ToString(CultureInfo.InvariantCulture), e.BusinessAmount.ToString("0.00", CultureInfo.InvariantCulture), Cell(e.ReceiptPath) }));
        return csv.ToString();
    }
}
