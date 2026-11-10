using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Foundry.Local.Demo;

public sealed partial class TranslationView : UserControl
{
    private FoundryLocalService? _service;
    private CancellationTokenSource? _operation;
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsBusy => _operation is not null;

    public TranslationView()
    {
        InitializeComponent();
        Disposed += (_, _) => CancelOperation();
    }

    public void Initialize(FoundryLocalService service) => _service = service ?? throw new ArgumentNullException(nameof(service));
    public void CancelOperation() => _operation?.Cancel();

    public Task RefreshModelsAsync() => RunAsync(async ct =>
    {
        if (_service is null) throw new InvalidOperationException("No Foundry service was provided.");
        var models = await _service.Runtime.GetModelsAsync(ct);
        if (IsDisposed) return;
        _models.DataSource = models.Where(m => m.Loaded && m.SupportsChat).ToList();
        _state.Text = _model.Items.Count == 0 ? "Load a Chat or Multimodal model in Model management."
            : "Enter a word, idiom, or sentence. Translation results appear below.";
    }, "Reading loaded chat models...");

    public Task TranslateAsync() => RunAsync(async ct =>
    {
        if (_service is null || _model.SelectedItem is not FoundryModel model)
            throw new InvalidOperationException("Select a loaded model first.");
        var target = Enum.Parse<TranslationLanguage>((string)_target.SelectedItem!);
        TranslationLanguage? source = _source.SelectedIndex == 0 ? null
            : Enum.Parse<TranslationLanguage>((string)_source.SelectedItem!);
        var result = await _service.TranslateAsync(model.Id, _input.Text, target, source, ct);
        if (IsDisposed) return;
        _output.Text = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true, Converters = { new JsonStringEnumConverter() }
        });
        _state.Text = $"{result.Kind} translation complete. AI can make mistakes; check nuances and context.";
    }, "Translating locally...");

    private async Task RunAsync(Func<CancellationToken, Task> action, string message)
    {
        if (IsBusy) return;
        using var operation = new CancellationTokenSource();
        _operation = operation;
        _state.Text = message;
        SetBusy(true);
        try { await action(operation.Token); }
        catch (OperationCanceledException) when (operation.IsCancellationRequested)
        {
            if (!IsDisposed) _state.Text = "Translation cancelled.";
        }
        catch (Exception ex) { if (!IsDisposed) _state.Text = $"Foundry operation failed: {ex.Message}"; }
        finally { _operation = null; if (!IsDisposed) SetBusy(false); }
    }

    private void SetBusy(bool busy)
    {
        _model.Enabled = _source.Enabled = _target.Enabled = _input.Enabled = _translate.Enabled = !busy;
        _cancel.Enabled = busy;
    }

    private async void TranslateClick(object? sender, EventArgs e) => await TranslateAsync();
    private void CancelClick(object? sender, EventArgs e) => CancelOperation();
}
