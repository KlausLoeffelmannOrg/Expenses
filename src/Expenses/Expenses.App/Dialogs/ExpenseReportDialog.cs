using Expenses.Core;

namespace Expenses.App.Dialogs;

public sealed partial class ExpenseReportDialog : Form
{
    public ExpenseReportDialog()
    {
        InitializeComponent();
    }

    public ExpenseReportDialog(string title, IReadOnlyList<CategoryTotal> totals, bool annual) : this()
    {
        Text = title;
        _grid.DataSource = totals;
        _totals.Text = $"Recorded: {totals.Sum(t => t.RecordedAmount):N2} USD | Business portion: {totals.Sum(t => t.BusinessAmount):N2} USD";
        _scope.Text = annual
            ? "Includes all state jurisdictions and period types;\noverlapping entries are not deduplicated."
            : "Includes all active expenses in this period,\nregardless of filter tab.";
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        DialogLayout.CapToWorkingArea(this);
    }
}
