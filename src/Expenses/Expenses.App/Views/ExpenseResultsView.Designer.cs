#nullable enable

namespace Expenses.App.Views;

partial class ExpenseResultsView
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _grid = new DataGridView();
        _paidDateColumn = new DataGridViewTextBoxColumn();
        _payeeColumn = new DataGridViewTextBoxColumn();
        _descriptionColumn = new DataGridViewTextBoxColumn();
        _categoryColumn = new DataGridViewTextBoxColumn();
        _tagsColumn = new DataGridViewTextBoxColumn();
        _amountColumn = new DataGridViewTextBoxColumn();
        _businessUseColumn = new DataGridViewTextBoxColumn();
        _businessAmountColumn = new DataGridViewTextBoxColumn();
        _dateCellStyle = new DataGridViewCellStyle();
        _amountCellStyle = new DataGridViewCellStyle();
        _businessAmountCellStyle = new DataGridViewCellStyle();
        _gridCellStyle = new DataGridViewCellStyle();
        ((System.ComponentModel.ISupportInitialize)_grid).BeginInit();
        SuspendLayout();
        _dateCellStyle.Format = "MM/dd/yyyy";
        _dateCellStyle.FormatProvider = new System.Globalization.CultureInfo("en-US");
        _amountCellStyle.Format = "N2";
        _amountCellStyle.FormatProvider = new System.Globalization.CultureInfo("en-US");
        _businessAmountCellStyle.Format = "N2";
        _businessAmountCellStyle.FormatProvider = new System.Globalization.CultureInfo("en-US");
        _gridCellStyle.FormatProvider = new System.Globalization.CultureInfo("en-US");
        _paidDateColumn.Name = "PaidDate";
        _paidDateColumn.DataPropertyName = "PaidDate";
        _paidDateColumn.HeaderText = "Date paid";
        _paidDateColumn.Width = 105;
        _paidDateColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        _paidDateColumn.DefaultCellStyle = _dateCellStyle;
        _payeeColumn.Name = "Payee";
        _payeeColumn.DataPropertyName = "Payee";
        _payeeColumn.HeaderText = "Payee";
        _payeeColumn.Width = 150;
        _payeeColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        _descriptionColumn.Name = "Description";
        _descriptionColumn.DataPropertyName = "Description";
        _descriptionColumn.HeaderText = "Business purpose";
        _descriptionColumn.Width = 230;
        _descriptionColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        _categoryColumn.Name = "Category";
        _categoryColumn.DataPropertyName = "Category";
        _categoryColumn.HeaderText = "Category";
        _categoryColumn.Width = 150;
        _categoryColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        _tagsColumn.Name = "Tags";
        _tagsColumn.DataPropertyName = "Tags";
        _tagsColumn.HeaderText = "Tags";
        _tagsColumn.Width = 140;
        _tagsColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        _amountColumn.Name = "Amount";
        _amountColumn.DataPropertyName = "Amount";
        _amountColumn.HeaderText = "Amount (USD)";
        _amountColumn.Width = 110;
        _amountColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        _amountColumn.DefaultCellStyle = _amountCellStyle;
        _businessUseColumn.Name = "BusinessUsePercent";
        _businessUseColumn.DataPropertyName = "BusinessUsePercent";
        _businessUseColumn.HeaderText = "Business use %";
        _businessUseColumn.Width = 115;
        _businessUseColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        _businessAmountColumn.Name = "BusinessAmount";
        _businessAmountColumn.DataPropertyName = "BusinessAmount";
        _businessAmountColumn.HeaderText = "Business portion (USD)";
        _businessAmountColumn.Width = 150;
        _businessAmountColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        _businessAmountColumn.DefaultCellStyle = _businessAmountCellStyle;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToOrderColumns = true;
        _grid.AutoGenerateColumns = false;
        _grid.EnableHeadersVisualStyles = false;
        _grid.AccessibleName = "Expense results";
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _grid.DefaultCellStyle = _gridCellStyle;
        _grid.Dock = DockStyle.Fill;
        _grid.MultiSelect = false;
        _grid.Name = "_grid";
        _grid.ReadOnly = true;
        _grid.RowHeadersVisible = false;
        _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.TabIndex = 0;
        _grid.Columns.AddRange(new DataGridViewColumn[] {
            _paidDateColumn, _payeeColumn, _descriptionColumn, _categoryColumn,
            _tagsColumn, _amountColumn, _businessUseColumn, _businessAmountColumn });
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Inherit;
        Controls.Add(_grid);
        Name = "ExpenseResultsView";
        Size = new Size(700, 300);
        ((System.ComponentModel.ISupportInitialize)_grid).EndInit();
        ResumeLayout(false);
    }

    private DataGridView _grid = null!;
    private DataGridViewTextBoxColumn _paidDateColumn = null!;
    private DataGridViewTextBoxColumn _payeeColumn = null!;
    private DataGridViewTextBoxColumn _descriptionColumn = null!;
    private DataGridViewTextBoxColumn _categoryColumn = null!;
    private DataGridViewTextBoxColumn _tagsColumn = null!;
    private DataGridViewTextBoxColumn _amountColumn = null!;
    private DataGridViewTextBoxColumn _businessUseColumn = null!;
    private DataGridViewTextBoxColumn _businessAmountColumn = null!;
    private DataGridViewCellStyle _dateCellStyle = null!;
    private DataGridViewCellStyle _amountCellStyle = null!;
    private DataGridViewCellStyle _businessAmountCellStyle = null!;
    private DataGridViewCellStyle _gridCellStyle = null!;
}
