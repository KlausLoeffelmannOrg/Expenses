using System.Net;
using System.Text;
using System.Text.Json;
using Foundry.Local;
using Xunit;

namespace Expenses.Tests;

public sealed class FoundryLocalTests
{
    private static ImageInput Image() => new([137, 80, 78, 71, 13, 10, 26, 10]);
    private const string ValidReceipt = """
        {"IsReceiptConfidence":98,"OverallConfidence":90,"DateOfPurchase":"2024-02-15",
        "Vendor":{"Name":"Demo Office","Address":null,"TaxId":null},
        "Items":[{"Description":"Pens","Quantity":2,"UnitPrice":5,"Amount":10,"Confidence":94}],
        "Subtotal":10,"Tax":1,"Total":11,"Currency":"USD","ReceiptNumber":"123","PaymentMethod":null,
        "RawText":"Demo Office Pens 10 Tax 1 Total 11 USD","Warnings":[]}
        """;

    [Fact]
    public async Task ReceiptExtractionSendsImageToLoopbackAndReturnsTypedFacts()
    {
        var handler = new ReplyHandler(ValidReceipt);
        using var http = new HttpClient(handler);
        using var service = new FoundryLocalService(new FakeRuntime(), http);
        var receipt = await service.ExtractReceiptAsync("vision:1", Image());
        Assert.Equal(98, receipt.IsReceiptConfidence);
        Assert.Equal(new DateTime(2024, 2, 15), receipt.DateOfPurchase);
        Assert.Equal("Demo Office", receipt.Vendor.Name);
        Assert.Equal(11, receipt.Total);
        Assert.Equal(10, Assert.Single(receipt.Items).Amount);
        using var request = JsonDocument.Parse(handler.RequestBody!);
        Assert.Equal("vision:1", request.RootElement.GetProperty("model").GetString());
        Assert.Equal("image/png", request.RootElement.GetProperty("input")[0]
            .GetProperty("content")[1].GetProperty("media_type").GetString());
        Assert.False(string.IsNullOrWhiteSpace(request.RootElement.GetProperty("input")[0]
            .GetProperty("content")[1].GetProperty("image_data").GetString()));
        Assert.Equal("/v1/responses", handler.Url!.AbsolutePath);
        Assert.Equal("127.0.0.1", handler.Url!.Host);
    }

    [Theory]
    [InlineData("Word", "[{\"Term\":\"Bank\",\"ContextSample\":\"Die Bank ist offen.\"}]", "null")]
    [InlineData("Idiom", "[]", "\"break a leg\"")]
    [InlineData("Text", "[]", "null")]
    public async Task TranslationSupportsTermsIdiomsAndPlainText(string kind, string terms, string idiom)
    {
        string json = $$"""
            {"Kind":"{{kind}}","SourceLanguage":"English","Translation":"German text","Terms":{{terms}},
            "SourceIdiom":{{idiom}},"EquivalentIdiom":null,"Explanation":null}
            """;
        var handler = new ReplyHandler(json);
        using var http = new HttpClient(handler);
        using var service = new FoundryLocalService(new FakeRuntime(), http);
        var result = await service.TranslateAsync("vision:1", "bank", TranslationLanguage.German);
        Assert.Equal(kind, result.Kind.ToString());
        Assert.Contains("German", handler.RequestBody!);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("{\"IsReceiptConfidence\":99}")]
    public async Task MalformedOrIncompleteExtractionIsNotSuccess(string reply)
    {
        using var http = new HttpClient(new ReplyHandler(reply));
        using var service = new FoundryLocalService(new FakeRuntime(), http);
        await Assert.ThrowsAsync<InvalidDataException>(() => service.ExtractReceiptAsync("vision:1", Image()));
    }

    [Fact]
    public async Task InvalidConfidenceAndTruncatedAnswersAreRejected()
    {
        using var badHttp = new HttpClient(new ReplyHandler(ValidReceipt.Replace("\"IsReceiptConfidence\":98", "\"IsReceiptConfidence\":101")));
        using var badService = new FoundryLocalService(new FakeRuntime(), badHttp);
        await Assert.ThrowsAsync<InvalidDataException>(() => badService.ExtractReceiptAsync("vision:1", Image()));
        using var shortHttp = new HttpClient(new ReplyHandler(ValidReceipt) { FinishReason = "length" });
        using var shortService = new FoundryLocalService(new FakeRuntime(), shortHttp);
        await Assert.ThrowsAsync<InvalidDataException>(() => shortService.ExtractReceiptAsync("vision:1", Image()));
    }

    [Fact]
    public async Task TextOnlyAndUnloadedModelsFailBeforeSendingImages()
    {
        var handler = new ReplyHandler("description");
        using var http = new HttpClient(handler);
        using var textService = new FoundryLocalService(new FakeRuntime { ModelType = "Chat" }, http);
        await Assert.ThrowsAsync<NotSupportedException>(() => textService.DescribeImageAsync("vision:1", Image()));
        using var unloadedService = new FoundryLocalService(new FakeRuntime { Loaded = false }, http);
        await Assert.ThrowsAsync<InvalidOperationException>(() => unloadedService.DescribeImageAsync("vision:1", Image()));
        Assert.Null(handler.RequestBody);
    }

    [Fact]
    public async Task NonLoopbackEndpointsAreRejectedBeforeAnyDataIsSent()
    {
        var handler = new ReplyHandler("description");
        using var http = new HttpClient(handler);
        using var service = new FoundryLocalService(new FakeRuntime { Endpoint = new Uri("https://example.com") }, http);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DescribeImageAsync("vision:1", Image()));
        Assert.Null(handler.RequestBody);
    }

    [Fact]
    public async Task MissingCliAndCancelledDetectionAreReported()
    {
        var runtime = new FoundryCliRuntime(Path.Combine(Environment.CurrentDirectory, "missing-foundry-test.exe"));
        Assert.False((await runtime.DetectAsync()).IsInstalled);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => runtime.DetectAsync(cancellation.Token));
    }

    [Fact]
    public void UnsupportedImagesAndEmptyWordResultsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => new ImageInput([1, 2, 3]));
        var result = FoundryLocalService.Parse<TranslationResult>("""
            {"Kind":"Word","SourceLanguage":"English","Translation":"Bank","Terms":[],"SourceIdiom":null,"EquivalentIdiom":null,"Explanation":null}
            """);
        Assert.Throws<InvalidDataException>(result.Validate);
    }

    internal sealed class FakeRuntime : IFoundryRuntime
    {
        public string ModelType { get; init; } = "Multimodal";
        public bool Loaded { get; init; } = true;
        public Uri Endpoint { get; init; } = new("http://127.0.0.1:12345");
        public Task<FoundryInstallation> DetectAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new FoundryInstallation(true, "0.13.0", true));
        public Task<IReadOnlyList<FoundryModel>> GetModelsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<FoundryModel>>([new("vision:1", "vision", ModelType, "Gpu", 100, "MIT", true, Loaded)]);
        public Task<Uri> GetEndpointAsync(CancellationToken cancellationToken = default) => Task.FromResult(Endpoint);
        public Task StartServerAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DownloadAsync(string modelId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(string modelId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UnloadAsync(string modelId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveFromCacheAsync(string modelId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class ReplyHandler(string reply) : HttpMessageHandler
    {
        public string? RequestBody { get; private set; }
        public Uri? Url { get; private set; }
        public string FinishReason { get; init; } = "stop";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Url = request.RequestUri;
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            object body = request.RequestUri!.AbsolutePath.EndsWith("/responses")
                ? new { status = FinishReason == "stop" ? "completed" : "incomplete", output = new[]
                {
                    new { type = "message", content = new[] { new { type = "output_text", text = reply } } }
                } }
                : new { choices = new[] { new { finish_reason = FinishReason, message = new { content = reply } } } };
            return new(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
        }
    }
}
