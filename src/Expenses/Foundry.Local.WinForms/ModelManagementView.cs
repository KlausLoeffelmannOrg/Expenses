using System.ComponentModel;

namespace Foundry.Local.WinForms;

public sealed partial class ModelManagementView : UserControl
{
    private IFoundryRuntime? _runtime;
    private CancellationTokenSource? _operation;
    private bool _supported;
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsBusy => _operation is not null;
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FoundryModel? SelectedModel => _models.Current as FoundryModel;

    public ModelManagementView()
    {
        InitializeComponent();
        _grid.SelectionChanged += (_, _) => UpdateCommands();
        Disposed += (_, _) => CancelOperation();
    }

    public void Initialize(IFoundryRuntime runtime) => _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    public void CancelOperation() => _operation?.Cancel();

    public Task RefreshAsync() => RunAsync(RefreshCoreAsync, "Reading Foundry installation and model catalog...");

    private async Task RefreshCoreAsync(CancellationToken cancellationToken)
    {
        var runtime = _runtime ?? throw new InvalidOperationException("The view has no Foundry runtime.");
        var installation = await runtime.DetectAsync(cancellationToken);
        _supported = installation.IsInstalled && installation.IsSupported;
        if (!_supported)
        {
            _models.DataSource = typeof(FoundryModel);
            _state.Text = installation.IsInstalled
                ? $"Foundry {installation.Version} is too old. Install CLI 0.13 or later."
                : "Foundry Local is not installed or is not on PATH. Install it, then restart this application.";
            return;
        }
        var models = await runtime.GetModelsAsync(cancellationToken);
        if (IsDisposed) return;
        string? selected = SelectedModel?.Id;
        _models.DataSource = models.ToList();
        if (selected is not null)
            foreach (DataGridViewRow row in _grid.Rows)
                if (row.DataBoundItem is FoundryModel m && m.Id == selected) _grid.CurrentCell = row.Cells[0];
        _state.Text = $"Foundry {installation.Version}: {models.Count} variants, {models.Count(m => m.Cached)} cached, {models.Count(m => m.Loaded)} loaded.";
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
            if (!IsDisposed) _state.Text = "Cancelled. Refresh to check runtime state; server-side work may still be finishing.";
        }
        catch (Exception ex) { if (!IsDisposed) _state.Text = $"Foundry operation failed: {ex.Message}"; }
        finally
        {
            _operation = null;
            if (!IsDisposed) UpdateCommands();
        }
    }

    private async Task ChangeModelAsync(Func<IFoundryRuntime, string, CancellationToken, Task> action, string verb)
    {
        if (_runtime is null || SelectedModel is not { } model) return;
        await RunAsync(async ct =>
        {
            await action(_runtime, model.Id, ct);
            await RefreshCoreAsync(ct);
        }, $"{verb}: {model.Id}. This can take several minutes...");
    }

    private void UpdateCommands()
    {
        bool ready = !IsBusy && _runtime is not null;
        _refresh.Enabled = ready;
        _start.Enabled = ready && _supported;
        _download.Enabled = ready && _supported && SelectedModel is { Cached: false };
        _load.Enabled = ready && SelectedModel is { Cached: true, Loaded: false };
        _unload.Enabled = ready && SelectedModel is { Loaded: true };
        _remove.Enabled = ready && SelectedModel is { Cached: true, Loaded: false };
        _cancel.Enabled = IsBusy;
        _grid.Enabled = !IsBusy;
    }

    private async void RefreshClick(object? sender, EventArgs e) => await RefreshAsync();
    private async void StartClick(object? sender, EventArgs e) => await RunAsync(async ct =>
    {
        await _runtime!.StartServerAsync(ct);
        await RefreshCoreAsync(ct);
    }, "Starting the shared local Foundry server...");
    private async void DownloadClick(object? sender, EventArgs e)
    {
        if (SelectedModel is not { } model) return;
        if (MessageBox.Show(this, $"Download {model.Id} ({model.FileSizeMb:N0} MB)?\nLicense: {model.License}\nReview the model license before use. This requires internet and disk space.",
            "Download model", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;
        await ChangeModelAsync((r, id, ct) => r.DownloadAsync(id, ct), "Downloading");
    }
    private async void LoadClick(object? sender, EventArgs e) =>
        await ChangeModelAsync((r, id, ct) => r.LoadAsync(id, ct), "Loading");
    private async void UnloadClick(object? sender, EventArgs e)
    {
        if (MessageBox.Show(this, "Unload the selected model from the shared daemon?\nOther local apps using it may be interrupted.",
            "Unload model", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK) return;
        await ChangeModelAsync((r, id, ct) => r.UnloadAsync(id, ct), "Unloading");
    }
    private async void RemoveClick(object? sender, EventArgs e)
    {
        if (SelectedModel is not { } model) return;
        if (MessageBox.Show(this, $"Remove {model.Id} from the shared disk cache?\nIt must be downloaded again before use.",
            "Remove cached model", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK) return;
        await ChangeModelAsync((r, id, ct) => r.RemoveFromCacheAsync(id, ct), "Removing");
    }
    private void CancelClick(object? sender, EventArgs e) => CancelOperation();
}
