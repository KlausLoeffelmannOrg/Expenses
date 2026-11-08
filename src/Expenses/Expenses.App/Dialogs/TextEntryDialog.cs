namespace Expenses.App.Dialogs;

internal sealed class TextEntryDialog : Form
{
    private readonly TextBox _text;
    public string EnteredText => _text.Text.Trim();
    public bool PeriodOnly => _periodOnly.Checked;
    private readonly CheckBox _periodOnly;

    public TextEntryDialog(string title, string caption, bool reminder = false, bool hasPeriod = false)
    {
        DialogLayout.Configure(this, title, new Size(550, reminder ? 300 : 180));
        var fields = DialogLayout.Fields();
        _text = new TextBox { Multiline = reminder, Height = reminder ? 110 : 25, MaxLength = reminder ? 10000 : 150 };
        _periodOnly = new CheckBox { Text = "Only for the current bookkeeping period", AutoSize = true, Enabled = hasPeriod };
        DialogLayout.Add(fields, caption, _text);
        if (reminder) DialogLayout.Add(fields, "&Scope:", _periodOnly);
        Controls.Add(fields);
        DialogLayout.Footer(this, () =>
        {
            if (EnteredText.Length == 0)
            {
                MessageBox.Show(this, "Enter a value before continuing.", Text);
                return;
            }
            DialogResult = DialogResult.OK;
        });
    }
}
