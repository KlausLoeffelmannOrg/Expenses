# Serialization examples

Normative partial pair. Layout recipes are not defined here. Use [layout examples](../../winforms-high-dpi-fluent-layout/references/layout-examples.md#canonical-modal-footer) when adding a footer.

## C# partial pair

```csharp
// CustomerDialog.cs
public partial class CustomerDialog : Form
{
    private CustomerDraft? _draft;

    public CustomerDialog()
    {
        InitializeComponent();
    }

    public CustomerDialog(CustomerDraft draft) : this()
    {
        ArgumentNullException.ThrowIfNull(draft);
        _draft = draft;
    }

    private void InspectClick(object? sender, EventArgs e)
    {
        Text = "Inspected";
    }
}
```

```csharp
// CustomerDialog.Designer.cs
partial class CustomerDialog
{
    private System.ComponentModel.IContainer components = null;

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _toolbar = new ToolStrip();
        _inspect = new ToolStripButton();
        _preview = new PictureBox();
        ((System.ComponentModel.ISupportInitialize)_preview).BeginInit();
        SuspendLayout();
        _toolbar.SuspendLayout();
        _inspect.Text = "Inspect";
        _inspect.Click += InspectClick;
        _toolbar.Items.Add(_inspect);
        _preview.Dock = DockStyle.Fill;
        _toolbar.Dock = DockStyle.Top;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(640, 480);
        Controls.Add(_preview);
        Controls.Add(_toolbar);
        Name = "CustomerDialog";
        Text = "Preview";
        _toolbar.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_preview).EndInit();
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

    private ToolStrip _toolbar;
    private ToolStripButton _inspect;
    private PictureBox _preview;
}
```

The `Point` values a generator emits for absolute leftovers are serialization residue. Do not copy them as the way to design a new form.

## VB differences

Generated fields are `Friend WithEvents`. The main partial is `Partial Class`. A named handler uses `Handles _inspect.Click`. Do not add or remove `Program.vb` as part of making a form designer-compatible. Classic Application Framework apps have none. WARP-hosted VB apps already have `Program.vb` and must keep it. Event bodies and async exception handling stay out of `InitializeComponent`.

## Round-trip checklist

Open in a supported Visual Studio, change a property, save, close, reopen, and inspect the serialized source. If a factory call disappears, move that work to the main partial.
