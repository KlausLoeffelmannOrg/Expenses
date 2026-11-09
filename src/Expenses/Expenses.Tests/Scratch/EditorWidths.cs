namespace Expenses.Tests.Scratch;

internal static class EditorWidths
{
    public const string TextCapacity = "WWWWWW";

    public static void Apply(Control root)
    {
        ArgumentNullException.ThrowIfNull(root);
        Visit(root);
    }

    private static void Visit(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            switch (child)
            {
                case NumericUpDown numeric:
                    AssignWidth(numeric, NumericWidth(numeric));
                    break;
                case ComboBox combo:
                    AssignWidth(combo, Measure(combo, TextCapacity) + combo.PreferredHeight);
                    break;
                case MaskedTextBox masked:
                    int capacity = Math.Max(TextCapacity.Length, masked.Mask.Length);
                    AssignWidth(masked, Measure(masked, new string('W', capacity)) + HorizontalChrome(masked));
                    break;
                case TextBox box:
                    AssignWidth(box, Measure(box, TextCapacity) + HorizontalChrome(box));
                    break;
                default:
                    Visit(child);
                    break;
            }
        }
    }

    private static void AssignWidth(Control child, int width)
    {
        if (width <= 0)
        {
            throw new InvalidOperationException("The input width measurement must be positive.");
        }
        child.MinimumSize = new Size(width, child.MinimumSize.Height);
    }

    private static int Measure(Control child, string sample)
        => TextRenderer.MeasureText(sample, child.Font).Width;

    private static int HorizontalChrome(Control child)
    {
        if (child.ClientSize.Width <= 0)
        {
            return 0;
        }
        return Math.Max(0, child.Width - child.ClientSize.Width);
    }

    private static int NumericWidth(NumericUpDown numeric)
    {
        int digits = Math.Max(IntegerDigits(numeric.Minimum), IntegerDigits(numeric.Maximum));
        int count = digits + 1;
        if (numeric.Minimum < 0)
        {
            count++;
        }
        string sample = new string('W', count);
        if (numeric.DecimalPlaces > 0)
        {
            sample = sample + "." + new string('W', numeric.DecimalPlaces);
        }
        return Measure(numeric, sample) + NumericChrome(numeric);
    }

    private static int NumericChrome(NumericUpDown numeric)
    {
        foreach (Control child in numeric.Controls)
        {
            if (child is TextBoxBase edit && edit.ClientSize.Width > 0)
            {
                return Math.Max(0, numeric.Width - edit.ClientSize.Width);
            }
        }
        return 0;
    }

    private static int IntegerDigits(decimal value)
    {
        decimal whole = decimal.Truncate(value);
        int digits = 1;
        while (whole >= 10m || whole <= -10m)
        {
            whole /= 10m;
            digits++;
        }
        return digits;
    }
}
