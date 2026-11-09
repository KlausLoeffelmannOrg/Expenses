using Expenses.Core;
using System.ComponentModel;
using System.Globalization;

namespace Expenses.App.Views;

public sealed partial class ExpenseEditor : UserControl
{
    private int _id;
    private Guid _revision;
    public event EventHandler? CommitRequested;
    public event EventHandler? CancelRequested;
    public event EventHandler? StashRequested;
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsEditing { get; private set; }

    public ExpenseEditor()
    {
        InitializeComponent();
        // The OOP Designer constructs before Site exists; DesignMode alone is insufficient.
        if (LicenseManager.UsageMode != LicenseUsageMode.Designtime) Clear();
    }

    private void CommitClick(object? sender, EventArgs e) => CommitRequested?.Invoke(this, EventArgs.Empty);
    private void CancelClick(object? sender, EventArgs e) => CancelRequested?.Invoke(this, EventArgs.Empty);
    private void StashClick(object? sender, EventArgs e) => StashRequested?.Invoke(this, EventArgs.Empty);

    private void BrowseClick(object? sender, EventArgs e)
    {
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode) return;
        using var dialog = new OpenFileDialog { Title = "Attach supporting receipt", Filter = "Receipt files|*.pdf;*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.txt|All files|*.*" };
        if (dialog.ShowDialog(this) == DialogResult.OK) _receipt.Text = dialog.FileName;
    }

    public void ShowExpense(Expense expense) => LoadInput(new(
        expense.Id, expense.Revision, expense.PaidDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
        expense.Payee, expense.Description, expense.Category, expense.Tags, expense.PaymentReference,
        expense.Amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
        expense.BusinessUsePercent, expense.ReceiptPath), false);

    public void NewExpense(DateOnly date) => LoadInput(new(0, Guid.Empty, date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
        "", "", "Other expenses", "", "", "", 100, ""), true);

    public void LoadInput(ExpenseInput input, bool editing)
    {
        _id = input.Id;
        _revision = input.Revision;
        _date.Text = input.PaidDate;
        _payee.Text = input.Payee;
        _description.Text = input.Description;
        _category.Text = input.Category;
        _tags.Text = input.Tags;
        _payment.Text = input.PaymentReference;
        _amount.Text = input.Amount;
        _businessUse.Value = input.BusinessUsePercent;
        _receipt.Text = input.ReceiptPath;
        _fields.Visible = true;
        _heading.Text = editing ? "Editing expense - Save, Cancel, or Stash before navigating" : "Expense details (read-only)";
        SetEditing(editing);
    }

    public ExpenseInput ReadInput() => new(_id, _revision, _date.Text, _payee.Text, _description.Text,
        _category.Text, _tags.Text, _payment.Text, _amount.Text.Replace(" ", ""), (int)_businessUse.Value, _receipt.Text);

    public void SetEditing(bool editing)
    {
        IsEditing = editing;
        foreach (var control in _fields.Controls.OfType<TextBoxBase>()) control.ReadOnly = !editing;
        _businessUse.Enabled = editing;
        _browse.Enabled = editing;
        _actions.Visible = editing;
        _heading.Text = editing ? "Editing expense - Save, Cancel, or Stash before navigating" : "Expense details (read-only)";
        if (editing) _payee.Focus();
    }

    public void Clear()
    {
        IsEditing = false;
        _fields.Visible = false;
        _actions.Visible = false;
        _heading.Text = "Select an expense to view details, or choose New expense.";
    }
}
