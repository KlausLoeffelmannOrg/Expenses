namespace Expenses.App.Views;

public sealed class PeriodTreeView : TreeView
{
    public PeriodTreeView()
    {
        DrawMode = TreeViewDrawMode.OwnerDrawAll;
        HideSelection = false;
        FullRowSelect = true;
        ShowLines = false;
        ShowPlusMinus = false;
        ShowRootLines = false;
        AccessibleName = "Bookkeeping periods and stashed drafts";
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnDrawNode(DrawTreeNodeEventArgs e)
    {
        if (e.Node is null) return;
        bool selected = (e.State & TreeNodeStates.Selected) != 0;
        var row = new Rectangle(0, e.Bounds.Top, ClientSize.Width, Math.Max(e.Bounds.Height, ItemHeight));
        using var background = new SolidBrush(selected ? SystemColors.Highlight : SystemColors.Window);
        e.Graphics.FillRectangle(background, row);
        Rectangle text = new(e.Bounds.Left, row.Top, Math.Max(0, row.Width - e.Bounds.Left), row.Height);
        TextRenderer.DrawText(e.Graphics, e.Node.Text, Font, text,
            selected ? SystemColors.HighlightText : SystemColors.WindowText,
            TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        if (selected && Focused) ControlPaint.DrawFocusRectangle(e.Graphics, row);
    }

    protected override void OnSystemColorsChanged(EventArgs e)
    {
        base.OnSystemColorsChanged(e);
        BackColor = SystemColors.Window;
        ForeColor = SystemColors.WindowText;
        Invalidate();
    }
}
