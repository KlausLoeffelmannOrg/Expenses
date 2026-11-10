using Expenses.App.Dialogs;
using Foundry.Local;
using Foundry.Local.WinForms;

namespace Expenses.App;

public partial class MainForm
{
    private FoundryLocalService? _foundry;
    private FoundryLocalService Foundry => _foundry ??= new FoundryLocalService();

    private void ShowFoundryModels()
    {
        using var dialog = new ModelManagementDialog(Foundry.Runtime) { Font = Font };
        DialogLayout.CapToWorkingArea(dialog);
        dialog.ShowDialog(this);
    }

    private void ImportReceipt()
    {
        if (_period is null || _editor.IsEditing) return;
        using var dialog = new ReceiptAnalysisDialog(Foundry, _preferences.FoundryModelId) { Font = Font };
        DialogLayout.CapToWorkingArea(dialog);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Receipt is not { } receipt) return;
        _preferences.FoundryModelId = dialog.SelectedModelId;
        QueuePreferencesSave();
        StageReceipt(receipt, dialog.ImagePath);
    }

    internal void StageReceipt(ReceiptExtraction receipt, string path)
    {
        if (_period is null || _editor.IsEditing)
            throw new InvalidOperationException("Open a bookkeeping period and finish the current edit before importing a receipt.");
        var (input, review) = ReceiptExpenseDraft.Create(receipt, path);
        _activeDraft = null;
        _editor.LoadInput(input, true);
        UpdateCommands();
        _status.Text = review;
        if (receipt.DateOfPurchase is { } date &&
            (DateOnly.FromDateTime(date) < _period.StartDate || DateOnly.FromDateTime(date) > _period.EndDate))
            _status.Text += " Date is outside the open period; Cancel or Stash and open the correct period.";
        _status.ToolTipText = _status.Text;
    }
}
