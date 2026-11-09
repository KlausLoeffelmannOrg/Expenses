namespace Expenses.App.Dialogs;

public sealed partial class TextEntryDialog : Form
{
    public string EnteredText => _text.Text.Trim();
    public bool PeriodOnly => _periodOnly.Checked;

    public TextEntryDialog()
    {
        InitializeComponent();
    }

    public TextEntryDialog(string title, string caption, bool reminder = false, bool hasPeriod = false) : this()
    {
        Text = title;
        _caption.Text = caption;
        _text.AccessibleName = caption.Replace("&", "").TrimEnd(':');
        _text.Multiline = reminder;
        _text.AcceptsReturn = reminder;
        _text.ScrollBars = reminder ? ScrollBars.Vertical : ScrollBars.None;
        _text.MaxLength = reminder ? 10000 : 150;
        _scopeCaption.Visible = reminder;
        _periodOnly.Visible = reminder;
        _periodOnly.Enabled = hasPeriod;
        if (!reminder)
        {
            _text.Dock = DockStyle.Top;
            _text.MinimumSize = Size.Empty;
            _layout.RowStyles[1].SizeType = SizeType.AutoSize;
            ClientSize = new Size(550, 180);
            MinimumSize = new Size(380, 180);
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        DialogLayout.CapToWorkingArea(this);
    }

    private void AcceptClick(object? sender, EventArgs e)
    {
        if (EnteredText.Length == 0)
        {
            MessageBox.Show(this, "Enter a value before continuing.", Text);
            return;
        }
        DialogResult = DialogResult.OK;
    }
}
