namespace Expenses.App;

partial class MainForm
{
    private System.ComponentModel.IContainer components;
    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _menu = new MenuStrip();
        _toolbar = new ToolStrip();
        _navigation = new SplitContainer();
        _details = new SplitContainer();
        _tree = new Views.PeriodTreeView();
        _tabs = new WarpToolkit.WinForms.Containers.FluentTabControl();
        _editor = new Views.ExpenseEditor();
        _statusStrip = new StatusStrip();
        _status = new ToolStripStatusLabel();
        _layoutTimer = new System.Windows.Forms.Timer(components);
        ((System.ComponentModel.ISupportInitialize)_navigation).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_details).BeginInit();
        _navigation.SuspendLayout();
        _details.SuspendLayout();
        SuspendLayout();
        _menu.Dock = DockStyle.Top;
        _toolbar.Dock = DockStyle.Top;
        _toolbar.GripStyle = ToolStripGripStyle.Hidden;
        _toolbar.AccessibleName = "Expense commands";
        _navigation.Dock = DockStyle.Fill;
        _navigation.Size = new Size(1200, 750);
        _navigation.SplitterDistance = 240;
        _navigation.Panel1MinSize = 140;
        _navigation.Panel2MinSize = 350;
        _navigation.Panel1.Controls.Add(_tree);
        _navigation.Panel2.Controls.Add(_details);
        _tree.Dock = DockStyle.Fill;
        _details.Dock = DockStyle.Fill;
        _details.Size = new Size(950, 750);
        _details.Orientation = Orientation.Horizontal;
        _details.SplitterDistance = 300;
        _details.Panel1MinSize = 100;
        _details.Panel2MinSize = 100;
        _details.Panel1.Controls.Add(_tabs);
        _details.Panel2.Controls.Add(_editor);
        _tabs.Dock = DockStyle.Fill;
        _editor.Dock = DockStyle.Fill;
        _statusStrip.Items.Add(_status);
        _status.Text = "Create or open a bookkeeping period to begin.";
        _layoutTimer.Interval = 400;
        Controls.Add(_navigation);
        Controls.Add(_toolbar);
        Controls.Add(_menu);
        Controls.Add(_statusStrip);
        MainMenuStrip = _menu;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 850);
        MinimumSize = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Name = "MainForm";
        Text = "Expenses - Bookkeeping";
        _navigation.ResumeLayout(false);
        _details.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_navigation).EndInit();
        ((System.ComponentModel.ISupportInitialize)_details).EndInit();
        ResumeLayout(true);
    }

    private MenuStrip _menu;
    private ToolStrip _toolbar;
    private SplitContainer _navigation;
    private SplitContainer _details;
    private Views.PeriodTreeView _tree;
    private WarpToolkit.WinForms.Containers.FluentTabControl _tabs;
    private Views.ExpenseEditor _editor;
    private StatusStrip _statusStrip;
    private ToolStripStatusLabel _status;
    private System.Windows.Forms.Timer _layoutTimer;
}
