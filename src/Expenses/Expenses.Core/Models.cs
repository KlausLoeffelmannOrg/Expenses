namespace Expenses.Core;

public enum PeriodKind { Month, Quarter, Year }

public sealed class BookkeepingPeriod
{
    public int Id { get; set; }
    public int Year { get; set; }
    public PeriodKind Kind { get; set; }
    public int Number { get; set; }
    public string StateCode { get; set; } = "";
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public override string ToString() =>
        $"{Year} {Kind switch { PeriodKind.Month => $"Month {Number:00}", PeriodKind.Quarter => $"Q{Number}", _ => "Annual" }}" +
        (StateCode.Length == 0 ? "" : $" - {StateCode}");

    public static BookkeepingPeriod Create(int year, PeriodKind kind, int number, string? state)
    {
        if (year is < 1900 or > 9998 || !Enum.IsDefined(kind))
            throw new ArgumentException("Choose a valid year and period type.");
        int maximum = kind switch { PeriodKind.Month => 12, PeriodKind.Quarter => 4, _ => 1 };
        if (number < 1 || number > maximum)
            throw new ArgumentException($"Period number must be between 1 and {maximum}.");
        string code = (state ?? "").Trim().ToUpperInvariant();
        if (code.Length > 0 && !StateCodes.Contains(code))
            throw new ArgumentException("Enter a two-letter U.S. state or District of Columbia abbreviation, or leave it blank.");
        int month = kind switch { PeriodKind.Month => number, PeriodKind.Quarter => (number - 1) * 3 + 1, _ => 1 };
        var start = new DateOnly(year, month, 1);
        return new()
        {
            Year = year,
            Kind = kind,
            Number = number,
            StateCode = code,
            StartDate = start,
            EndDate = start.AddMonths(kind switch { PeriodKind.Month => 1, PeriodKind.Quarter => 3, _ => 12 }).AddDays(-1)
        };
    }

    private static readonly HashSet<string> StateCodes =
        "AL AK AZ AR CA CO CT DE DC FL GA HI ID IL IN IA KS KY LA ME MD MA MI MN MS MO MT NE NV NH NJ NM NY NC ND OH OK OR PA RI SC SD TN TX UT VT VA WA WV WI WY"
        .Split(' ').ToHashSet(StringComparer.Ordinal);
}

public sealed class Expense
{
    public int Id { get; set; }
    public int BookkeepingPeriodId { get; set; }
    public DateOnly PaidDate { get; set; }
    public string Payee { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "Other expenses";
    public string Tags { get; set; } = "";
    public string PaymentReference { get; set; } = "";
    public long AmountCents { get; set; }
    public int BusinessUsePercent { get; set; } = 100;
    public string ReceiptPath { get; set; } = "";
    public bool IsDeleted { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid Revision { get; set; } = Guid.NewGuid();
    public decimal Amount => AmountCents / 100m;
    public decimal BusinessAmount => decimal.Round(Amount * BusinessUsePercent / 100m, 2, MidpointRounding.AwayFromZero);
}

public sealed class ExpenseDraft
{
    public int Id { get; set; }
    public int BookkeepingPeriodId { get; set; }
    public string Name { get; set; } = "";
    public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.UtcNow;
    public string Payload { get; set; } = "";
}

public sealed class Reminder
{
    public int Id { get; set; }
    public int? BookkeepingPeriodId { get; set; }
    public string Text { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsDismissed { get; set; }
}

public sealed record ExpenseInput(
    int Id, Guid Revision, string PaidDate, string Payee, string Description, string Category,
    string Tags, string PaymentReference, string Amount, int BusinessUsePercent, string ReceiptPath);

public sealed record CategoryTotal(string Category, decimal RecordedAmount, decimal BusinessAmount);
