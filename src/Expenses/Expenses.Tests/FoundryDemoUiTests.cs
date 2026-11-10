using System.Net;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using Foundry.Local;
using Foundry.Local.Demo;
using Foundry.Local.WinForms;
using Xunit;

namespace Expenses.Tests;

public sealed class FoundryDemoUiTests
{
    [Fact]
    public void DemoConstructsWithoutServicesAndLoadsBothFeaturesWithAnInjectedRuntime() => Sta(() =>
    {
        using var design = new DemoForm();
        Assert.Single(Descendants(design).OfType<ImageAnalysisView>());
        Assert.Single(Descendants(design).OfType<TranslationView>());
        using var http = new HttpClient(new TranslationHandler());
        using var service = new FoundryLocalService(new FoundryLocalTests.FakeRuntime(), http);
        using var form = new DemoForm(service);
        form.Show();
        Application.DoEvents();
        var tabs = Descendants(form).OfType<TabControl>().Single();
        Assert.Equal(2, tabs.TabPages.Count);
        tabs.SelectedIndex = 1;
        var translation = Descendants(form).OfType<TranslationView>().Single();
        var input = Descendants(translation).OfType<TextBox>().Single(t => !t.ReadOnly);
        input.Text = "bank";
        translation.TranslateAsync().GetAwaiter().GetResult();
        var output = Descendants(translation).OfType<TextBox>().Single(t => t.ReadOnly);
        Assert.Contains("ContextSample", output.Text);
        Assert.Contains("Bank", output.Text);
        Assert.False(translation.IsBusy);
        using var larger = new Font(form.Font.FontFamily, 18);
        form.Font = larger;
        form.ClientSize = new Size(850, 650);
        Application.DoEvents();
        Assert.True(output.Height > 0);
        Assert.All(Descendants(translation).OfType<Button>(), b => Assert.True(b.Visible));
        form.Close();
    });

    private static IEnumerable<Control> Descendants(Control parent) =>
        parent.Controls.Cast<Control>().SelectMany(c => new[] { c }.Concat(Descendants(c)));

    private static void Sta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try { Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException); action(); }
            catch (Exception ex) { error = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(45)));
        if (error is not null) ExceptionDispatchInfo.Capture(error).Throw();
    }

    private sealed class TranslationHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    choices = new[]
                    {
                        new
                        {
                            finish_reason = "stop",
                            message = new { content = """
                                {"Kind":"Word","SourceLanguage":"English","Translation":"Bank",
                                "Terms":[{"Term":"Bank","ContextSample":"Die Bank ist offen."}],
                                "SourceIdiom":null,"EquivalentIdiom":null,"Explanation":null}
                                """ }
                        }
                    }
                }), Encoding.UTF8, "application/json")
            });
    }
}
