using System.Globalization;
using Expenses.Core;
using Foundry.Local;

namespace Expenses.App;

internal static class ReceiptExpenseDraft
{
    internal static (ExpenseInput Input, string ReviewMessage) Create(ReceiptExtraction receipt, string path)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        bool usd = string.Equals(receipt.Currency, "USD", StringComparison.OrdinalIgnoreCase);
        bool validTotal = receipt.Total is > 0 and <= 999999999.99m
            && decimal.Round(receipt.Total.Value, 2) == receipt.Total.Value;
        string amount = usd && validTotal ? receipt.Total!.Value.ToString("0.00", CultureInfo.InvariantCulture) : "";
        var itemDescriptions = receipt.Items.Select(i =>
            i.Amount is { } cost ? $"{i.Description} ({cost.ToString("0.00", CultureInfo.InvariantCulture)})" : i.Description);
        string original = receipt.Total is { } total ? total.ToString("0.00", CultureInfo.InvariantCulture) : "unknown";
        string description = $"Receipt items: {string.Join("; ", itemDescriptions)}. " +
            $"Original total: {original} {receipt.Currency ?? "(unknown currency)"}. Verify business purpose.";
        var input = new ExpenseInput(0, Guid.Empty,
            receipt.DateOfPurchase?.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) ?? "",
            receipt.Vendor.Name ?? "", description, "Other expenses", "Foundry - review required",
            string.Join(" / ", new[] { receipt.PaymentMethod, receipt.ReceiptNumber }.Where(s => !string.IsNullOrWhiteSpace(s))),
            amount, 0, Path.GetFullPath(path));
        string review = $"Unsaved AI draft. Receipt confidence: {receipt.IsReceiptConfidence}%; extraction: {receipt.OverallConfidence}%. " +
            "Verify date, vendor, business purpose, category and business-use percentage before Save.";
        if (amount.Length == 0) review += " USD amount left blank: confirm currency and enter a valid USD amount manually.";
        if (receipt.Warnings.Count > 0) review += " Model warnings: " + string.Join("; ", receipt.Warnings);
        return (input, review);
    }
}
