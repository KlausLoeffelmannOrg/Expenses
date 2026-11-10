namespace Foundry.Local;

public sealed class ReceiptExtraction
{
    public required int IsReceiptConfidence { get; init; }
    public required int OverallConfidence { get; init; }
    public required DateTime? DateOfPurchase { get; init; }
    public required VendorInfo Vendor { get; init; }
    public required List<ReceiptItem> Items { get; init; }
    public required decimal? Subtotal { get; init; }
    public required decimal? Tax { get; init; }
    public required decimal? Total { get; init; }
    public required string? Currency { get; init; }
    public required string? ReceiptNumber { get; init; }
    public required string? PaymentMethod { get; init; }
    public required string RawText { get; init; }
    public required List<string> Warnings { get; init; }

    internal void Validate()
    {
        if (IsReceiptConfidence is < 0 or > 100 || OverallConfidence is < 0 or > 100
            || Vendor is null || Items is null || Warnings is null || RawText is null
            || Currency is not null && (Currency.Length != 3 || !Currency.All(char.IsAsciiLetter))
            || Items.Any(i => i is null || string.IsNullOrWhiteSpace(i.Description) || i.Confidence is < 0 or > 100))
            throw new InvalidDataException("The model returned an invalid receipt structure or confidence value.");
    }
}

public sealed class VendorInfo
{
    public required string? Name { get; init; }
    public required string? Address { get; init; }
    public required string? TaxId { get; init; }
}

public sealed class ReceiptItem
{
    public required string Description { get; init; }
    public required decimal? Quantity { get; init; }
    public required decimal? UnitPrice { get; init; }
    public required decimal? Amount { get; init; }
    public required int Confidence { get; init; }
}
