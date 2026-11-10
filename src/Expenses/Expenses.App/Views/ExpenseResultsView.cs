using System.ComponentModel;

namespace Expenses.App.Views;

public sealed partial class ExpenseResultsView : UserControl
{
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DataGridView Grid => _grid;

    public ExpenseResultsView()
    {
        InitializeComponent();
    }
}
