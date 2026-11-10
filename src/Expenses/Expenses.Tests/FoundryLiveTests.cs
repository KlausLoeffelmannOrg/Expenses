using System.Drawing.Imaging;
using System.Text.Json;
using Foundry.Local;
using Xunit;

namespace Expenses.Tests;

public sealed class FoundryLiveFactAttribute : FactAttribute
{
    public FoundryLiveFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("EXPENSES_FOUNDRY_LIVE") != "1")
            Skip = "Opt in with EXPENSES_FOUNDRY_LIVE=1; requires an installed CLI and a loaded multimodal model.";
    }
}

public sealed class FoundryLiveTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [FoundryLiveFact, Trait("Category", "FoundryLive")]
    public async Task InstalledRuntimeDescribesAndExtractsSyntheticReceipt()
    {
        using var limit = new CancellationTokenSource(TimeSpan.FromMinutes(8));
        using var service = new FoundryLocalService();
        var installation = await service.Runtime.DetectAsync(limit.Token);
        Assert.True(installation.IsInstalled && installation.IsSupported);
        var models = await service.Runtime.GetModelsAsync(limit.Token);
        var model = models.First(m => m.Loaded && m.SupportsImages);
        Assert.True((await service.Runtime.GetEndpointAsync(limit.Token)).IsLoopback);
        ImageInput image = MakeReceipt();
        string description = await service.DescribeImageAsync(model.Id, image, limit.Token);
        output.WriteLine(description);
        Assert.False(string.IsNullOrWhiteSpace(description));
        var receipt = await service.ExtractReceiptAsync(model.Id, image, limit.Token);
        output.WriteLine(JsonSerializer.Serialize(receipt));
        Assert.InRange(receipt.IsReceiptConfidence, 70, 100);
        Assert.Equal(11m, receipt.Total);
        Assert.Equal("USD", receipt.Currency);
        Assert.Equal(new DateTime(2024, 2, 15), receipt.DateOfPurchase);
        Assert.NotEmpty(receipt.Items);
        var custom = await service.ExtractStructuredAsync<ReceiptTitle>(model.Id, image,
            "Read the vendor's name into Text.",
            JsonSerializer.SerializeToElement(new { type = "object", required = new[] { "Text" },
                properties = new { Text = new { type = "string" } } }), limit.Token);
        Assert.Contains("Demo", custom.Text, StringComparison.OrdinalIgnoreCase);
    }

    [FoundryLiveFact, Trait("Category", "FoundryLive")]
    public async Task InstalledRuntimeTranslatesWordsIdiomsAndTextInBothDirections()
    {
        using var limit = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        using var service = new FoundryLocalService();
        var model = (await service.Runtime.GetModelsAsync(limit.Token)).First(m => m.Loaded && m.SupportsChat);
        var word = await service.TranslateAsync(model.Id, "bank", TranslationLanguage.German, cancellationToken: limit.Token);
        output.WriteLine(JsonSerializer.Serialize(word));
        Assert.Equal(TranslationKind.Word, word.Kind);
        Assert.NotEmpty(word.Terms);
        var idiom = await service.TranslateAsync(model.Id, "It's raining cats and dogs.", TranslationLanguage.French, cancellationToken: limit.Token);
        output.WriteLine(JsonSerializer.Serialize(idiom));
        Assert.Equal(TranslationKind.Idiom, idiom.Kind);
        Assert.False(string.IsNullOrWhiteSpace(idiom.EquivalentIdiom));
        foreach (var target in new[] { TranslationLanguage.Italian, TranslationLanguage.Danish, TranslationLanguage.Spanish })
        {
            var text = await service.TranslateAsync(model.Id, "The receipt is on the table.", target, cancellationToken: limit.Token);
            Assert.Equal(TranslationKind.Text, text.Kind);
            Assert.False(string.IsNullOrWhiteSpace(text.Translation));
        }
        var reverse = await service.TranslateAsync(model.Id, "Die Quittung liegt auf dem Tisch.", TranslationLanguage.English,
            TranslationLanguage.German, limit.Token);
        Assert.Contains("receipt", reverse.Translation, StringComparison.OrdinalIgnoreCase);
    }

    private static ImageInput MakeReceipt()
    {
        using var bitmap = new Bitmap(900, 600);
        using (var graphics = Graphics.FromImage(bitmap))
        using (var font = new Font("Arial", 26, FontStyle.Regular, GraphicsUnit.Pixel))
        {
            graphics.Clear(Color.White);
            graphics.DrawString("Demo Office\nRECEIPT 123\nDate: February 15, 2024\n2 Pens @ 5.00        10.00\nSubtotal: 10.00 USD\nTax: 1.00 USD\nTOTAL: 11.00 USD\nPaid by card", font, Brushes.Black, 40, 35);
        }
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return new ImageInput(stream.ToArray());
    }

    public sealed class ReceiptTitle
    {
        public required string Text { get; init; }
    }
}
