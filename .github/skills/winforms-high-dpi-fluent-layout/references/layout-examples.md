# Declarative adaptive layouts

## Contents

- [Canonical modal footer](#canonical-modal-footer)
- [Resizable viewport](#resizable-viewport)
- [Runtime minima](#runtime-minima)

## Canonical modal footer

Profile for a non-resizable dialog: content-sized root, not an empty percent spring. The footer is an outer 2x2 table plus an inner equal-percent button table. The root is `Dock = Top` inside an `AutoScroll` form. `ClientSize` is a designer starting value, not a height constraint. Control fields follow the methods (`WFO3002`). `components` may stay at the top.

```csharp
// InputDialog.cs
public partial class InputDialog : Form
{
    public string InputValue => _txtInput.Text;

    public InputDialog()
    {
        InitializeComponent();
    }

    public InputDialog(string prompt) : this()
    {
        _lblPrompt.Text = prompt;
    }
}
```

```csharp
// InputDialog.Designer.cs
partial class InputDialog
{
    private System.ComponentModel.IContainer components = null;

    private void InitializeComponent()
    {
        _layoutPanel = new TableLayoutPanel();
        _lblPrompt = new Label();
        _txtInput = new TextBox();
        _footerPanel = new TableLayoutPanel();
        _footerSpacer = new Label();
        _buttonPanel = new TableLayoutPanel();
        _btnOK = new Button();
        _btnCancel = new Button();
        SuspendLayout();
        _layoutPanel.SuspendLayout();
        _footerPanel.SuspendLayout();
        _buttonPanel.SuspendLayout();
        _layoutPanel.AutoSize = true;
        _layoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _layoutPanel.ColumnCount = 1;
        _layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layoutPanel.Controls.Add(_lblPrompt, 0, 0);
        _layoutPanel.Controls.Add(_txtInput, 0, 1);
        _layoutPanel.Controls.Add(_footerPanel, 0, 2);
        _layoutPanel.SetColumnSpan(_footerPanel, _layoutPanel.ColumnCount);
        _layoutPanel.Dock = DockStyle.Top;
        _layoutPanel.RowCount = 3;
        _layoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _lblPrompt.AutoSize = true;
        _lblPrompt.Text = "Prompt:";
        _txtInput.Dock = DockStyle.Fill;
        _footerPanel.AutoSize = true;
        _footerPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _footerPanel.ColumnCount = 2;
        _footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _footerPanel.Controls.Add(_footerSpacer, 1, 0);
        _footerPanel.Controls.Add(_buttonPanel, 1, 1);
        _footerPanel.Dock = DockStyle.Fill;
        _footerPanel.RowCount = 2;
        _footerPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _footerPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _footerSpacer.AutoSize = true;
        _footerSpacer.Dock = DockStyle.Fill;
        _footerSpacer.Margin = new Padding(0, 5, 0, 5);
        _footerSpacer.Text = "";
        _buttonPanel.AutoSize = true;
        _buttonPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _buttonPanel.ColumnCount = 2;
        _buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _buttonPanel.Controls.Add(_btnOK, 0, 0);
        _buttonPanel.Controls.Add(_btnCancel, 1, 0);
        _buttonPanel.Dock = DockStyle.Fill;
        _buttonPanel.RowCount = 1;
        _buttonPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _btnOK.AutoSize = true;
        _btnOK.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _btnOK.DialogResult = DialogResult.OK;
        _btnOK.Dock = DockStyle.Fill;
        _btnOK.Margin = new Padding(3, 0, 3, 0);
        _btnOK.Text = "OK";
        _btnCancel.AutoSize = true;
        _btnCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _btnCancel.DialogResult = DialogResult.Cancel;
        _btnCancel.Dock = DockStyle.Fill;
        _btnCancel.Margin = new Padding(3, 0, 3, 0);
        _btnCancel.Text = "Cancel";
        AcceptButton = _btnOK;
        AutoScaleMode = AutoScaleMode.Font;
        AutoScroll = true;
        CancelButton = _btnCancel;
        ClientSize = new Size(360, 140);
        Controls.Add(_layoutPanel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "InputDialog";
        Text = "Input";
        _buttonPanel.ResumeLayout(false);
        _footerPanel.ResumeLayout(false);
        _layoutPanel.ResumeLayout(false);
        _layoutPanel.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private TableLayoutPanel _layoutPanel;
    private Label _lblPrompt;
    private TextBox _txtInput;
    private TableLayoutPanel _footerPanel;
    private Label _footerSpacer;
    private TableLayoutPanel _buttonPanel;
    private Button _btnOK;
    private Button _btnCancel;
}
```

A caption longer than 30 characters gets a strategic `\n` in `Text`. Three buttons use `33.33333F` columns, not a flow panel. Vertical top-aligned buttons transpose the inner panel to N AutoSize rows and keep the outer nest. Working-area capping belongs in the main partial after placement, not in `InitializeComponent`. See [layout-rules.md](layout-rules.md).

Usage keeps the runtime overload: `using InputDialog dialog = new InputDialog("Enter name:");`.

## Resizable viewport

A resizable form uses `Dock = Fill` on the root. Bounded inputs use AutoSize rows, then one empty Percent 100 spring, then the AutoSize footer row. The footer is still the nested pair above, column 0, spanning every column.

A grid or tab viewport that must fill spare height is that single Percent row. Do not add another Percent 100 row in front of the footer. Two 100% rows split the client and can halve the grid. Do not set the grid row to AutoSize so an empty spring can exist. Nest the viewport in one content region if the page also needs a gap. A content-sized dialog has no spring. Do not leave `InitializeComponent` in the logic partial.

## Runtime minima

Editor minimum widths run in the main partial after font and layout have settled. Call `EditorWidths.Apply(root)`. It is not serialized. The callable API is that one method.

Do not derive chrome from `GetPreferredSize`, from `child.Text`, or from the previous `MinimumSize.Width`. `GetPreferredSize` applies `MinimumSize`, so a filled or already-minimum control undercounts chrome and the next pass can grow. Do not construct a probe control: its `DeviceDpi` can be the system DPI rather than the host. Do not clear live `Text`. Measure capacity with `TextRenderer.MeasureText` and the control's settled `Font` only. Do not multiply that font by `DeviceDpi`.

Chrome is the realized non-client difference, not the allocated width. `TextBox` and `MaskedTextBox` use `Width - ClientSize.Width`. `NumericUpDown` uses `Width -` the inner `TextBoxBase.ClientSize.Width`, which includes the buttons. Walk `Controls`; do not use reflection. `ComboBox` has no separate public button metric, so add `PreferredHeight` as the font-native chrome allowance. A 1..100 `NumericUpDown` measures `"WWWW"` because 100 has three integer digits plus one air `W`. A masked editor measures the longer of `TextCapacity` and `Mask`, so a long date or currency mask is not stuck at six `W`s. `IntegerDigits` divides the truncated value while it is outside -10..10. It does not call `decimal.Abs`, which overflows `decimal.MinValue`. A non-positive measured width throws rather than leaving the previous minimum.

Matched editors are leaves. Do not set a minimum on the up-down edit or a combo's inner child. Replace `MinimumSize.Width`. Never add to the previous width. If the client width is still 0, chrome is 0 rather than a guessed constant. Apply again after the handle and layout exist. `PreferredSize` is read-only. A specialized masked editor may override `GetPreferredSize` and `SetBoundsCore` instead of joining a new base class.

```csharp
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
```
