using Expenses.App;
using Foundry.Local;
using Xunit;

namespace Expenses.Tests;

public sealed class ReceiptImportTests
{
    [Theory]
    [InlineData("USD", 11.25, "11.25")]
    [InlineData("usd", 11.25, "11.25")]
    [InlineData("EUR", 11.25, "")]
    [InlineData(null, 11.25, "")]
    [InlineData("USD", 0, "")]
    [InlineData("USD", -10, "")]
    [InlineData("USD", 11.255, "")]
    [InlineData("USD", 1000000000, "")]
    public void ReceiptDraftNeverLabelsUnknownOrForeignMoneyAsUsd(string? currency, double total, string expected)
    {
        var (input, message) = ReceiptExpenseDraft.Create(Sample(currency, (decimal)total), "test-receipt.png");
        Assert.Equal(expected, input.Amount);
        Assert.Equal(0, input.Id);
        Assert.Equal(0, input.BusinessUsePercent);
        Assert.Equal("02/15/2024", input.PaidDate);
        Assert.Contains("Pens", input.Description);
        Assert.Contains("Unsaved", message);
        if (expected.Length == 0) Assert.Contains("amount left blank", message);
    }

    [Fact]
    public void UnknownReceiptFactsRemainMissingRatherThanInventingDateVendorOrTotal()
    {
        var receipt = new ReceiptExtraction
        {
            IsReceiptConfidence = 30, OverallConfidence = 10, DateOfPurchase = null,
            Vendor = new VendorInfo { Name = null, Address = null, TaxId = null },
            Items = [], Subtotal = null, Tax = null, Total = null, Currency = null, ReceiptNumber = null,
            PaymentMethod = null, RawText = "", Warnings = ["Unreadable"]
        };
        var (input, message) = ReceiptExpenseDraft.Create(receipt, "unknown.png");
        Assert.Equal("", input.PaidDate);
        Assert.Equal("", input.Payee);
        Assert.Equal("", input.Amount);
        Assert.Contains("Unreadable", message);
    }

    internal static ReceiptExtraction Sample(string? currency = "USD", decimal total = 11.25m) => new()
    {
        IsReceiptConfidence = 95, OverallConfidence = 90, DateOfPurchase = new DateTime(2024, 2, 15),
        Vendor = new VendorInfo { Name = "Demo Office", Address = null, TaxId = null },
        Items = [new ReceiptItem { Description = "Pens", Amount = 10, Quantity = 2, UnitPrice = 5, Confidence = 90 }],
        Subtotal = 10, Tax = 1.25m, Total = total, Currency = currency, ReceiptNumber = "123",
        PaymentMethod = "Card", RawText = "Demo Office Pens Total 11.25 USD", Warnings = []
    };
}
