using Expenses.Core;
using WarpToolkit.WinForms.Specialized;

namespace Expenses.App.Views;

public sealed class ExpenseResultsView : UserControl
{
    public WarpDataGridView Grid { get; } = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AutoGenerateColumns = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        RowHeadersVisible = false,
        AllowUserToOrderColumns = true,
        AccessibleName = "Expense results"
    };

    public ExpenseResultsView()
    {
        AddColumn(nameof(Expense.PaidDate), "Date paid", 105, "MM/dd/yyyy");
        AddColumn(nameof(Expense.Payee), "Payee", 150);
        AddColumn(nameof(Expense.Description), "Business purpose", 230);
        AddColumn(nameof(Expense.Category), "Category", 150);
        AddColumn(nameof(Expense.Tags), "Tags", 140);
        AddColumn(nameof(Expense.Amount), "Amount (USD)", 110, "N2");
        AddColumn(nameof(Expense.BusinessUsePercent), "Business use %", 115);
        AddColumn(nameof(Expense.BusinessAmount), "Business portion (USD)", 150, "N2");
        Controls.Add(Grid);
    }

    private void AddColumn(string property, string caption, int width, string format = "")
    {
        Grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = property,
            DataPropertyName = property,
            HeaderText = caption,
            Width = width,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = format,
                FormatProvider = System.Globalization.CultureInfo.GetCultureInfo("en-US")
            }
        });
    }
}
