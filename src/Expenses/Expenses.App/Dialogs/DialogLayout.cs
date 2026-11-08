namespace Expenses.App.Dialogs;

internal static class DialogLayout
{
    internal static TableLayoutPanel Fields() => new()
    {
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        Dock = DockStyle.Top,
        ColumnCount = 2,
        Padding = new Padding(12),
        ColumnStyles = { new ColumnStyle(SizeType.AutoSize), new ColumnStyle(SizeType.Percent, 100) }
    };

    internal static void Add(TableLayoutPanel table, string caption, Control input)
    {
        int row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.Controls.Add(new Label { Text = caption, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 12, 8) }, 0, row);
        input.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        input.Margin = new Padding(3, 6, 3, 6);
        input.AccessibleName = caption.Replace("&", "").TrimEnd(':');
        input.TabIndex = row;
        table.Controls.Add(input, 1, row);
    }

    internal static Button Footer(Form form, Action accept, string text = "OK", string cancelText = "Cancel")
    {
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(8), FlowDirection = FlowDirection.RightToLeft };
        var cancel = new Button { Text = cancelText, DialogResult = DialogResult.Cancel, AutoSize = true };
        var ok = new Button { Text = text, AutoSize = true };
        ok.Click += (_, _) => accept();
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(ok);
        form.Controls.Add(buttons);
        form.AcceptButton = ok;
        form.CancelButton = cancel;
        return ok;
    }

    internal static void Configure(Form form, string title, Size desired)
    {
        form.Text = title;
        form.StartPosition = FormStartPosition.CenterParent;
        form.MinimizeBox = false;
        form.MaximizeBox = false;
        form.ShowInTaskbar = false;
        form.AutoScaleMode = AutoScaleMode.Font;
        var area = Screen.FromControl(form).WorkingArea;
        form.ClientSize = new Size(Math.Min(desired.Width, (int)(area.Width * .75)), Math.Min(desired.Height, (int)(area.Height * .75)));
    }

    internal static void Error(IWin32Window owner, Exception error) =>
        MessageBox.Show(owner, error.Message, "Expenses - action failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
