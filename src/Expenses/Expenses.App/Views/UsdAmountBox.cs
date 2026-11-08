using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Expenses.App.Views;

public sealed class UsdAmountBox : MaskedTextBox
{
    public UsdAmountBox()
    {
        Culture = CultureInfo.GetCultureInfo("en-US");
        Mask = "999999999.00";
        PromptChar = ' ';
        TextMaskFormat = MaskFormat.IncludeLiterals;
        ResetOnSpace = true;
    }

    [AllowNull]
    public override string Text
    {
        get => base.Text.Replace(" ", "");
        set
        {
            if (string.IsNullOrEmpty(value)) { base.Text = ""; return; }
            string[] parts = value.Split('.');
            base.Text = parts[0].PadLeft(9) + (parts.Length > 1 ? "." + parts[1] : "");
        }
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        if (e.KeyChar == '.' && !ReadOnly)
        {
            SelectionStart = 10;
            SelectionLength = 0;
            e.Handled = true;
        }
        base.OnKeyPress(e);
    }

    protected override void WndProc(ref Message m)
    {
        const int WmPaste = 0x0302;
        if (m.Msg == WmPaste && !ReadOnly)
        {
            string value = Clipboard.GetText().Trim();
            if (!decimal.TryParse(value, NumberStyles.AllowDecimalPoint, Culture, out decimal amount)
                || amount < 0 || amount > 999999999.99m || decimal.Round(amount, 2) != amount)
            {
                MessageBox.Show(this, "Paste a USD amount with at most two decimal places, using a period as the decimal separator.",
                    "Invalid amount");
                return;
            }
            Text = value;
            return;
        }
        base.WndProc(ref m);
    }
}
