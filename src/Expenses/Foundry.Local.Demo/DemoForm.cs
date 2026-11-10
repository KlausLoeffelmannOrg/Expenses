using Foundry.Local.WinForms;

namespace Foundry.Local.Demo;

public sealed partial class DemoForm : Form
{
    private FoundryLocalService? _service;

    public DemoForm() => InitializeComponent();
    public DemoForm(FoundryLocalService service) : this()
    {
        _service = service;
        _images.Initialize(service);
        _translation.Initialize(service);
    }

    private async void DemoShown(object? sender, EventArgs e)
    {
        if (_service is null) return;
        var area = Screen.FromControl(this).WorkingArea;
        Bounds = new Rectangle(area.X + area.Width / 8, area.Y + area.Height / 10,
            area.Width * 3 / 4, area.Height * 4 / 5);
        await RefreshModelsAsync();
    }

    private async Task RefreshModelsAsync()
    {
        await _images.RefreshModelsAsync();
        await _translation.RefreshModelsAsync();
    }

    private async void ManageClick(object? sender, EventArgs e)
    {
        if (_service is null) return;
        if (_images.IsBusy || _translation.IsBusy)
        {
            MessageBox.Show(this, "Finish or cancel the current operation before managing models.", "Foundry Local");
            return;
        }
        using var dialog = new ModelManagementDialog(_service.Runtime);
        await dialog.ShowDialogAsync(this);
        await RefreshModelsAsync();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_images.IsBusy || _translation.IsBusy)
        {
            _images.CancelOperation();
            _translation.CancelOperation();
            e.Cancel = true;
        }
        base.OnFormClosing(e);
    }
}
