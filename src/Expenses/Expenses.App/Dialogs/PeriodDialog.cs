using Expenses.Core;

namespace Expenses.App.Dialogs;

public sealed partial class PeriodDialog : Form
{
    private BookkeepingStore? _store;
    private bool _create;
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public BookkeepingPeriod? SelectedPeriod { get; private set; }

    public PeriodDialog()
    {
        InitializeComponent();
    }

    public PeriodDialog(BookkeepingStore store, bool create) : this()
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _create = create;
        Text = create ? "New bookkeeping period" : "Open bookkeeping period";
        _ok.Text = create ? "Create" : "Open";
        _fields.Visible = create;
        _periodsPanel.Visible = !create;
        _year.Value = DateTime.Today.Year;
        _number.Value = DateTime.Today.Month;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        DialogLayout.CapToWorkingArea(this);
        if (_store is null || _create) return;
        try { _periods.DataSource = _store.GetPeriods(); }
        catch (Exception ex) { DialogLayout.Error(this, ex); }
    }

    private void KindChanged(object? sender, EventArgs e)
    {
        int maximum = _kind.SelectedIndex switch { 0 => 12, 1 => 4, _ => 1 };
        _number.Value = Math.Min(_number.Value, maximum);
        _number.Maximum = maximum;
        _number.Enabled = maximum > 1;
    }

    private void AcceptClick(object? sender, EventArgs e)
    {
        if (_store is null) return;
        try
        {
            if (_create)
            {
                if (_kind.SelectedIndex < 0) return;
                SelectedPeriod = _store.AddPeriod((int)_year.Value, (PeriodKind)_kind.SelectedIndex,
                    (int)_number.Value, _state.Text);
            }
            else
            {
                if (_periods.SelectedItem is not BookkeepingPeriod period) return;
                SelectedPeriod = period;
            }
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex) { DialogLayout.Error(this, ex); }
    }
}
