using System.ComponentModel;

namespace Foundry.Local.WinForms;

public sealed partial class ReceiptAnalysisDialog : Form
{
    private bool _runtime;
    private string? _preferredModel;
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ReceiptExtraction? Receipt => _view.Receipt;
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string ImagePath => _view.ImagePath;
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? SelectedModelId => _view.SelectedModelId;

    public ReceiptAnalysisDialog()
    {
        InitializeComponent();
        _view.ReceiptChanged += (_, _) => _useReceipt.Enabled = _view.Receipt is not null;
    }

    public ReceiptAnalysisDialog(FoundryLocalService service, string? preferredModel = null) : this()
    {
        _view.Initialize(service);
        _preferredModel = preferredModel;
        _runtime = true;
    }

    private async void DialogShown(object? sender, EventArgs e)
    {
        if (_runtime) await _view.RefreshModelsAsync(_preferredModel);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_view.IsBusy)
        {
            _view.CancelOperation();
            e.Cancel = true;
        }
        if (DialogResult == DialogResult.OK && Receipt is null) e.Cancel = true;
        base.OnFormClosing(e);
    }
}
