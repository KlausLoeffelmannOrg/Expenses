namespace Foundry.Local.WinForms;

public sealed partial class ModelManagementDialog : Form
{
    private bool _runtime;
    public ModelManagementDialog() => InitializeComponent();
    public ModelManagementDialog(IFoundryRuntime runtime) : this()
    {
        _view.Initialize(runtime);
        _runtime = true;
    }

    private async void DialogShown(object? sender, EventArgs e)
    {
        if (_runtime) await _view.RefreshAsync();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_view.IsBusy)
        {
            _view.CancelOperation();
            e.Cancel = true;
        }
        base.OnFormClosing(e);
    }
}
