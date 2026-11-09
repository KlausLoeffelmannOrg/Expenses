namespace Expenses.App.Dialogs;

public sealed partial class ReportYearDialog : Form
{
    public int SelectedYear => (int)_year.Value;

    public ReportYearDialog()
    {
        InitializeComponent();
    }

    public ReportYearDialog(int year) : this()
    {
        _year.Value = year;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        DialogLayout.CapToWorkingArea(this);
    }
}
