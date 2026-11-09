namespace Expenses.Tests.Scratch;

public sealed partial class HostedScalingScratchView : UserControl
{
    public HostedScalingScratchView()
    {
        InitializeComponent();
    }

    public TextBox Input => _input;
    public NumericUpDown Number => _number;
    public MaskedTextBox DateInput => _date;
    public ComboBox Choice => _choice;
    public TableLayoutPanel Fields => _fields;
}
