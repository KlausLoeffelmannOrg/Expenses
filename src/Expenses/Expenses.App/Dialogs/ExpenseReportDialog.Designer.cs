namespace Expenses.App.Dialogs;

partial class ExpenseReportDialog
{
    private System.ComponentModel.IContainer components;

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
        components = new System.ComponentModel.Container();
        _layout = new TableLayoutPanel();
        _grid = new DataGridView();
        _category = new DataGridViewTextBoxColumn();
        _recorded = new DataGridViewTextBoxColumn();
        _business = new DataGridViewTextBoxColumn();
        _recordedStyle = new DataGridViewCellStyle();
        _businessStyle = new DataGridViewCellStyle();
        _summary = new Panel();
        _summaryLayout = new TableLayoutPanel();
        _totals = new Label();
        _advice = new Label();
        _scope = new Label();
        _footer = new TableLayoutPanel();
        _close = new Button();
        ((System.ComponentModel.ISupportInitialize)_grid).BeginInit();
        _layout.SuspendLayout();
        _summary.SuspendLayout();
        _summaryLayout.SuspendLayout();
        _footer.SuspendLayout();
        SuspendLayout();
        _layout.Dock = DockStyle.Fill;
        _layout.Padding = new Padding(12);
        _layout.ColumnCount = 1;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layout.RowCount = 3;
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 65F));
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.Controls.Add(_grid, 0, 0);
        _layout.Controls.Add(_summary, 0, 1);
        _layout.Controls.Add(_footer, 0, 2);
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.EnableHeadersVisualStyles = false;
        _grid.RowHeadersVisible = false;
        _grid.AccessibleName = "Expense totals by category";
        _grid.TabIndex = 0;
        _grid.Columns.AddRange(new DataGridViewColumn[] { _category, _recorded, _business });
        _category.Name = "Category";
        _category.DataPropertyName = "Category";
        _category.HeaderText = "Category";
        _category.MinimumWidth = 100;
        _recorded.Name = "RecordedAmount";
        _recorded.DataPropertyName = "RecordedAmount";
        _recorded.HeaderText = "Recorded amount (USD)";
        _recorded.MinimumWidth = 100;
        _recordedStyle.Format = "N2";
        _recorded.DefaultCellStyle = _recordedStyle;
        _business.Name = "BusinessAmount";
        _business.DataPropertyName = "BusinessAmount";
        _business.HeaderText = "Business portion (USD)";
        _business.MinimumWidth = 100;
        _businessStyle.Format = "N2";
        _business.DefaultCellStyle = _businessStyle;
        _summary.Dock = DockStyle.Fill;
        _summary.AutoScroll = true;
        _summary.Controls.Add(_summaryLayout);
        _summaryLayout.Dock = DockStyle.Top;
        _summaryLayout.AutoSize = true;
        _summaryLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _summaryLayout.ColumnCount = 1;
        _summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _summaryLayout.RowCount = 3;
        _summaryLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _summaryLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _summaryLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _summaryLayout.Controls.Add(_totals, 0, 0);
        _summaryLayout.Controls.Add(_advice, 0, 1);
        _summaryLayout.Controls.Add(_scope, 0, 2);
        _totals.AutoSize = true;
        _totals.Text = "Recorded: 0.00 USD | Business portion: 0.00 USD";
        _totals.Margin = new Padding(3, 6, 3, 6);
        _advice.AutoSize = true;
        _advice.Text = "Not a tax deduction calculation.\nReview vehicle costs, meals, and capital assets with your tax adviser.";
        _advice.Margin = new Padding(3, 6, 3, 6);
        _scope.AutoSize = true;
        _scope.Text = "Includes all active expenses in this period,\nregardless of filter tab.";
        _scope.Margin = new Padding(3, 6, 3, 6);
        _footer.Name = "_footer";
        _footer.AutoSize = true;
        _footer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _footer.Dock = DockStyle.Fill;
        _footer.ColumnCount = 1;
        _footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _footer.RowCount = 1;
        _footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _footer.Controls.Add(_close, 0, 0);
        _close.AutoSize = true;
        _close.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _close.Dock = DockStyle.Fill;
        _close.Margin = new Padding(4);
        _close.Text = "Close";
        _close.DialogResult = DialogResult.Cancel;
        _close.TabIndex = 1;
        AcceptButton = _close;
        CancelButton = _close;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(850, 500);
        MinimumSize = new Size(430, 350);
        Controls.Add(_layout);
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Name = "ExpenseReportDialog";
        Text = "Expense report";
        ((System.ComponentModel.ISupportInitialize)_grid).EndInit();
        _footer.ResumeLayout(false);
        _footer.PerformLayout();
        _summaryLayout.ResumeLayout(false);
        _summaryLayout.PerformLayout();
        _summary.ResumeLayout(false);
        _summary.PerformLayout();
        _layout.ResumeLayout(false);
        _layout.PerformLayout();
        ResumeLayout(false);
    }

    private TableLayoutPanel _layout;
    private DataGridView _grid;
    private DataGridViewTextBoxColumn _category;
    private DataGridViewTextBoxColumn _recorded;
    private DataGridViewTextBoxColumn _business;
    private DataGridViewCellStyle _recordedStyle;
    private DataGridViewCellStyle _businessStyle;
    private Panel _summary;
    private TableLayoutPanel _summaryLayout;
    private Label _totals;
    private Label _advice;
    private Label _scope;
    private TableLayoutPanel _footer;
    private Button _close;
}
