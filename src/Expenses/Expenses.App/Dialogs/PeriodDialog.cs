using Expenses.Core;

namespace Expenses.App.Dialogs;

internal sealed class PeriodDialog : Form
{
    public BookkeepingPeriod? SelectedPeriod { get; private set; }

    public PeriodDialog(BookkeepingStore store, bool create)
    {
        DialogLayout.Configure(this, create ? "New bookkeeping period" : "Open bookkeeping period", new Size(570, 360));
        var fields = DialogLayout.Fields();
        fields.AutoScroll = true;
        fields.Dock = DockStyle.Fill;
        Controls.Add(fields);
        if (create)
        {
            var year = new NumericUpDown { Minimum = 1900, Maximum = 9998, Value = DateTime.Today.Year };
            var kind = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, DataSource = Enum.GetValues<PeriodKind>() };
            var number = new NumericUpDown { Minimum = 1, Maximum = 12, Value = DateTime.Today.Month };
            var state = new MaskedTextBox { Mask = ">LL", TextMaskFormat = MaskFormat.ExcludePromptAndLiterals };
            kind.SelectedIndexChanged += (_, _) =>
            {
                int maximum = kind.SelectedItem is PeriodKind.Month ? 12 : kind.SelectedItem is PeriodKind.Quarter ? 4 : 1;
                number.Value = Math.Min(number.Value, maximum);
                number.Maximum = maximum;
                number.Enabled = maximum > 1;
            };
            DialogLayout.Add(fields, "&Calendar year:", year);
            DialogLayout.Add(fields, "Period &type:", kind);
            DialogLayout.Add(fields, "&Month / quarter number:", number);
            DialogLayout.Add(fields, "&State (optional):", state);
            DialogLayout.Add(fields, "", new Label { Text = "Use a two-letter U.S. state abbreviation.\nMonthly/quarterly groups are not separate federal tax years.", AutoSize = true });
            DialogLayout.Footer(this, () =>
            {
                try
                {
                    if (kind.SelectedItem is not PeriodKind selectedKind) return;
                    SelectedPeriod = store.AddPeriod((int)year.Value, selectedKind, (int)number.Value, state.Text);
                    DialogResult = DialogResult.OK;
                }
                catch (Exception ex) { DialogLayout.Error(this, ex); }
            }, "Create");
        }
        else
        {
            var periods = new ListBox { DataSource = store.GetPeriods(), Height = 220 };
            DialogLayout.Add(fields, "&Bookkeeping period:", periods);
            DialogLayout.Footer(this, () =>
            {
                if (periods.SelectedItem is not BookkeepingPeriod period) return;
                SelectedPeriod = period;
                DialogResult = DialogResult.OK;
            }, "Open");
        }
    }
}
