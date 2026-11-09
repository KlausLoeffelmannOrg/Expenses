using System.ComponentModel;
using WarpToolkit.WinForms.Specialized;

namespace Expenses.App.Views;

public sealed partial class ExpenseResultsView : UserControl
{
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public WarpDataGridView Grid => _grid;

    public ExpenseResultsView()
    {
        InitializeComponent();
    }
}
