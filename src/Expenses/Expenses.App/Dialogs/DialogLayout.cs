namespace Expenses.App.Dialogs;

internal static class DialogLayout
{
    internal static void CapToWorkingArea(Form form)
    {
        var area = Screen.FromControl(form).WorkingArea;
        var cap = new Size((int)(area.Width * .75), (int)(area.Height * .75));
        form.MinimumSize = new Size(Math.Min(form.MinimumSize.Width, cap.Width), Math.Min(form.MinimumSize.Height, cap.Height));
        form.Size = new Size(Math.Min(form.Width, cap.Width), Math.Min(form.Height, cap.Height));
    }

    internal static void Error(IWin32Window owner, Exception error) =>
        MessageBox.Show(owner, error.Message, "Expenses - action failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
