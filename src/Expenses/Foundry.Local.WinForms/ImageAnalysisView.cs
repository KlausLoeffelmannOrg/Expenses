using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Foundry.Local.WinForms;

public sealed partial class ImageAnalysisView : UserControl
{
    private FoundryLocalService? _service;
    private ImageInput? _image;
    private CancellationTokenSource? _operation;
    private static readonly JsonSerializerOptions DisplayJson = new() { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };
    public event EventHandler? ReceiptChanged;
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ReceiptExtraction? Receipt { get; private set; }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string ImagePath => _path.Text;
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? SelectedModelId => (_model.SelectedItem as FoundryModel)?.Id;
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsBusy => _operation is not null;

    public ImageAnalysisView()
    {
        InitializeComponent();
        _model.SelectedIndexChanged += (_, _) => { ClearReceipt(); UpdateCommands(); };
        Disposed += (_, _) => { CancelOperation(); _preview.Image?.Dispose(); };
    }

    public void Initialize(FoundryLocalService service) => _service = service ?? throw new ArgumentNullException(nameof(service));
    public void CancelOperation() => _operation?.Cancel();

    public Task RefreshModelsAsync(string? preferredId = null) => RunAsync(async ct =>
    {
        if (_service is null) throw new InvalidOperationException("No Foundry service was provided.");
        var models = await _service.Runtime.GetModelsAsync(ct);
        if (IsDisposed) return;
        var loaded = models.Where(m => m.Loaded && m.SupportsImages).ToList();
        string? selected = preferredId ?? SelectedModelId;
        _models.DataSource = loaded;
        _model.SelectedIndex = loaded.Count == 0 ? -1 : Math.Max(0, loaded.FindIndex(m => m.Id == selected));
        _state.Text = loaded.Count == 0 ? "No loaded multimodal model. Open Model management, download a Multimodal model, and Load it."
            : "Select an image, then Describe or Extract receipt. Only local inference is used.";
    }, "Reading loaded multimodal models...");

    public Task SelectImageAsync(string path) => RunAsync(async ct =>
    {
        var input = await ImageInput.FromFileAsync(path, ct);
        using var stream = input.OpenRead();
        using var source = System.Drawing.Image.FromStream(stream);
        if ((long)source.Width * source.Height > 16_000_000 || source.Width > 8192 || source.Height > 8192)
            throw new ArgumentException("Resize the image to at most 16 megapixels and 8192 pixels per edge before analysis.");
        var preview = new Bitmap(source);
        var old = _preview.Image;
        _preview.Image = preview;
        old?.Dispose();
        _image = input;
        _path.Text = Path.GetFullPath(path);
        ClearReceipt();
        _output.Clear();
        _state.Text = "Image selected. AI output must be checked against the original.";
    }, "Reading image...");

    public Task DescribeAsync() => AnalyzeAsync(false);
    public Task ExtractReceiptAsync() => AnalyzeAsync(true);

    private Task AnalyzeAsync(bool receipt) => RunAsync(async ct =>
    {
        if (_service is null || _image is null || SelectedModelId is not { } id)
            throw new InvalidOperationException("Select an image and a loaded multimodal model first.");
        ClearReceipt();
        _output.Clear();
        if (receipt)
        {
            Receipt = await _service.ExtractReceiptAsync(id, _image, ct);
            if (IsDisposed) return;
            _output.Text = JsonSerializer.Serialize(Receipt, DisplayJson);
            _state.Text = $"Receipt: {Receipt.IsReceiptConfidence}% | Extraction: {Receipt.OverallConfidence}%. These are uncalibrated model estimates. Review every field.";
            ReceiptChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            _output.Text = await _service.DescribeImageAsync(id, _image, ct);
            _state.Text = "Description complete. Verify it against the image.";
        }
    }, receipt ? "Extracting receipt locally..." : "Describing image locally...");

    private void ClearReceipt()
    {
        Receipt = null;
        ReceiptChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task RunAsync(Func<CancellationToken, Task> action, string message)
    {
        if (IsBusy) return;
        using var operation = new CancellationTokenSource();
        _operation = operation;
        _state.Text = message;
        UpdateCommands();
        try { await action(operation.Token); }
        catch (OperationCanceledException) when (operation.IsCancellationRequested)
        {
            if (!IsDisposed) _state.Text = "Analysis cancelled. No data has been imported.";
        }
        catch (Exception ex) { if (!IsDisposed) _state.Text = $"Foundry operation failed: {ex.Message}"; }
        finally { _operation = null; if (!IsDisposed) UpdateCommands(); }
    }

    private void UpdateCommands()
    {
        _browse.Enabled = _refresh.Enabled = _model.Enabled = !IsBusy;
        _describe.Enabled = _extract.Enabled = !IsBusy && _image is not null && SelectedModelId is not null;
        _cancel.Enabled = IsBusy;
    }

    private async void BrowseClick(object? sender, EventArgs e)
    {
        using var picker = new OpenFileDialog { Filter = "Receipt images (PNG, JPEG)|*.png;*.jpg;*.jpeg", Title = "Choose image for local analysis" };
        if (picker.ShowDialog(this) == DialogResult.OK) await SelectImageAsync(picker.FileName);
    }
    private async void RefreshClick(object? sender, EventArgs e) => await RefreshModelsAsync();
    private async void DescribeClick(object? sender, EventArgs e) => await DescribeAsync();
    private async void ExtractClick(object? sender, EventArgs e) => await ExtractReceiptAsync();
    private void CancelClick(object? sender, EventArgs e) => CancelOperation();
}
