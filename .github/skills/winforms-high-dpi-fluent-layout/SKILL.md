---
name: winforms-high-dpi-fluent-layout
description: >-
  Use when designing, modifying, or reviewing responsive WinForms Form and
  UserControl layouts with TableLayoutPanel, FlowLayoutPanel, nested scalable
  containers, Per Monitor V2 High-DPI support, fullscreen or presentation
  modes, and modal dialogs. For broad form-design-quality reviews, pair with
  winforms-designer-code to cover Designer serialization as well as layout and
  DPI quality.
---

# WinForms High-DPI Fluent Layout Guide

Fluent layout strategies for scalable, DPI-aware WinForms form designs using TableLayoutPanel and FlowLayoutPanel. All layout code shown here must follow the **winforms-designer-code** skill rules — control configuration belongs in `InitializeComponent` inside the `.Designer.cs` file.

## Designer Compatibility Note

**CRITICAL:** All control instantiation, property assignment, and layout wiring shown in this skill belongs inside `InitializeComponent` in the `.Designer.cs` file. Do NOT extract control configuration into helper methods like `CreateDetailsPanel()` or `SetupLayout()` — this breaks the WinForms Designer. See the **winforms-designer-code** skill for the authoritative rules.

## Core Layout Principles

### Scaling and DPI Awareness

**DPI Settings:**

- Use 96 DPI / 100% scaling as the design baseline for new Forms/UserControls
- Set `AutoScaleMode` appropriately
- For existing forms: Leave `AutoScaleMode` as-is, account for scaling
- .NET 9+: Query DarkMode status via `Application.IsDarkModeEnabled`

**Important:** Only `SystemColors` change automatically in DarkMode. Absolute colors require manual handling.

### Layout Strategy Overview

**Divide and Conquer:**

- Use multiple or nested TableLayoutPanels for logical sections
- Don't cram everything into one mega-grid
- Main form uses SplitContainer or outer TableLayoutPanel with percentage/AutoSize rows/cols
- Each UI section gets its own nested TableLayoutPanel or UserControl

**Keep It Simple:**

- Individual TableLayoutPanels: 2-4 columns maximum
- Use GroupBoxes with nested TableLayoutPanels for visual grouping
- RadioButton clusters: single-column, auto-size-cells TableLayoutPanel inside AutoSize GroupBox
- Large content scrolling: Nested panel controls with `AutoScroll` enabled

## TableLayoutPanel Fundamentals

### Cell Sizing Priority

These are design preferences, **not** the order in which WinForms allocates pixels:

**For Rows:**

```text
AutoSize > Percent > Absolute
```

**For Columns:**

```text
AutoSize > Percent > Absolute
```

### Column Sizing Rules

| Size Mode | Use Case | Anchor | Notes |
|-----------|----------|--------|-------|
| **AutoSize** | Caption columns | `Left \| Right` | For labels and prompts |
| **Percent** | Content columns | `Top \| Bottom \| Left \| Right` | Distribute by reasoning |
| **Absolute** | Fixed content only | Varies | Icons, buttons (avoid when possible) |

**Default:** Anchor labels and controls that should retain their preferred size. Docking is intentional for a value control that must fill its cell (for example, a multiline editor), an equal-size dialog button, or a nested content container/GroupBox that must fill the available width. Use `DockStyle.Fill` for those cases; do not indiscriminately dock every label or AutoSize child. Docking a TableLayoutPanel to its Form or a layout inside its GroupBox is also valid.

### Row Sizing Rules

| Size Mode | Use Case | Notes |
|-----------|----------|-------|
| **AutoSize** | Every row containing real content | Entry fields, multiline content sized to preferred height, groups and buttons |
| **Percent** | Remaining space in a height-constrained layout | A deliberately unoccupied spring row, or a resizable viewport |
| **Absolute** | Avoid | Only for truly fixed-size content |

For nested content, set both `AutoSize = true` and `AutoSizeMode = AutoSizeMode.GrowAndShrink` on the nested `TableLayoutPanel` and its containing `GroupBox` (if any). Give real content rows `AutoSize`; leave a trailing `Percent, 100F` row **unoccupied** to absorb extra height when the outer layout fills a resizable Form. A percent spring is not needed in a content-sized modal dialog: it has no spare height to absorb. Percent columns allocate available width; `AutoSize` containers still measure the preferred widths of their children. Never put buttons or labels into a spring row merely to pin them to the bottom.

### Margin and Padding

**Margins:**

- Set `Margin` on controls (minimum 3px default)
- Margins create spacing between controls

**Padding:**

- `Padding` does NOT affect TableLayoutPanel cells
- Use for container controls only

## Example: Basic Form Layout (Designer-Compatible)

All controls are declared as backing fields and configured inside `InitializeComponent`:

```csharp
// In MainForm.Designer.cs
private void InitializeComponent()
{
    _tableLayoutPanel = new TableLayoutPanel();
    _lblName = new Label();
    _txtName = new TextBox();
    _lblEmail = new Label();
    _txtEmail = new TextBox();
    _buttonPanel = new TableLayoutPanel();
    _btnOK = new Button();
    _btnCancel = new Button();

    SuspendLayout();
    _tableLayoutPanel.SuspendLayout();
    _buttonPanel.SuspendLayout();

    // _tableLayoutPanel
    _tableLayoutPanel.ColumnCount = 2;
    _tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
    _tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
    _tableLayoutPanel.Controls.Add(_lblName, 0, 0);
    _tableLayoutPanel.Controls.Add(_txtName, 1, 0);
    _tableLayoutPanel.Controls.Add(_lblEmail, 0, 1);
    _tableLayoutPanel.Controls.Add(_txtEmail, 1, 1);
    _tableLayoutPanel.Controls.Add(_buttonPanel, 1, 3);
    _tableLayoutPanel.Dock = DockStyle.Fill;
    _tableLayoutPanel.Location = new Point(0, 0);
    _tableLayoutPanel.Name = "_tableLayoutPanel";
    _tableLayoutPanel.RowCount = 4;
    _tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
    _tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
    _tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
    _tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
    _tableLayoutPanel.Size = new Size(400, 200);
    _tableLayoutPanel.TabIndex = 0;

    // _lblName
    _lblName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    _lblName.AutoSize = true;
    _lblName.Location = new Point(3, 3);
    _lblName.Margin = new Padding(3);
    _lblName.Name = "_lblName";
    _lblName.Size = new Size(42, 15);
    _lblName.TabIndex = 0;
    _lblName.Text = "Name:";

    // _txtName
    _txtName.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
    _txtName.Location = new Point(51, 3);
    _txtName.Margin = new Padding(3);
    _txtName.Name = "_txtName";
    _txtName.Size = new Size(346, 23);
    _txtName.TabIndex = 1;

    // _lblEmail
    _lblEmail.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    _lblEmail.AutoSize = true;
    _lblEmail.Location = new Point(3, 32);
    _lblEmail.Margin = new Padding(3);
    _lblEmail.Name = "_lblEmail";
    _lblEmail.Size = new Size(39, 15);
    _lblEmail.TabIndex = 2;
    _lblEmail.Text = "Email:";

    // _txtEmail
    _txtEmail.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
    _txtEmail.Location = new Point(51, 32);
    _txtEmail.Margin = new Padding(3);
    _txtEmail.Name = "_txtEmail";
    _txtEmail.Size = new Size(346, 23);
    _txtEmail.TabIndex = 3;

    // _buttonPanel
    _buttonPanel.AutoSize = true;
    _buttonPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
    _buttonPanel.ColumnCount = 2;
    _buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
    _buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
    _buttonPanel.Controls.Add(_btnOK, 0, 0);
    _buttonPanel.Controls.Add(_btnCancel, 1, 0);
    _buttonPanel.Dock = DockStyle.Fill;
    _buttonPanel.Name = "_buttonPanel";
    _buttonPanel.RowCount = 1;
    _buttonPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
    _buttonPanel.TabIndex = 4;

    // _btnOK
    _btnOK.AutoSize = true;
    _btnOK.AutoSizeMode = AutoSizeMode.GrowAndShrink;
    _btnOK.Dock = DockStyle.Fill;
    _btnOK.Margin = new Padding(4);
    _btnOK.Name = "_btnOK";
    _btnOK.TabIndex = 0;
    _btnOK.Text = "OK";
    _btnOK.UseVisualStyleBackColor = true;

    // _btnCancel
    _btnCancel.AutoSize = true;
    _btnCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
    _btnCancel.Dock = DockStyle.Fill;
    _btnCancel.Margin = new Padding(4);
    _btnCancel.Name = "_btnCancel";
    _btnCancel.TabIndex = 1;
    _btnCancel.Text = "Cancel";
    _btnCancel.UseVisualStyleBackColor = true;

    // MainForm
    AcceptButton = _btnOK;
    AutoScaleDimensions = new SizeF(7F, 15F);
    AutoScaleMode = AutoScaleMode.Font;
    CancelButton = _btnCancel;
    ClientSize = new Size(400, 200);
    Controls.Add(_tableLayoutPanel);
    Name = "MainForm";
    Text = "Contact Form";

    _buttonPanel.ResumeLayout(false);
    _buttonPanel.PerformLayout();
    _tableLayoutPanel.ResumeLayout(false);
    _tableLayoutPanel.PerformLayout();
    ResumeLayout(false);
}

private TableLayoutPanel _tableLayoutPanel;
private Label _lblName;
private TextBox _txtName;
private Label _lblEmail;
private TextBox _txtEmail;
private TableLayoutPanel _buttonPanel;
private Button _btnOK;
private Button _btnCancel;
```

## Complex Nested Layout Pattern (Designer-Compatible)

For complex layouts with toolbar, content area, and status bar, all controls are still declared as backing fields and configured inside `InitializeComponent`. The key is nesting containers:

```csharp
// In MainForm.Designer.cs
private void InitializeComponent()
{
    _outerLayout = new TableLayoutPanel();
    _toolStrip = new ToolStrip();
    _splitContainer = new SplitContainer();
    _treeView = new TreeView();
    _detailsPanel = new TableLayoutPanel();
    _lblDetail1 = new Label();
    _txtDetail1 = new TextBox();
    _statusStrip = new StatusStrip();
    _statusLabel = new ToolStripStatusLabel();

    SuspendLayout();
    _outerLayout.SuspendLayout();
    ((ISupportInitialize)_splitContainer).BeginInit();
    _splitContainer.Panel1.SuspendLayout();
    _splitContainer.Panel2.SuspendLayout();
    _splitContainer.SuspendLayout();
    _detailsPanel.SuspendLayout();

    // _outerLayout
    _outerLayout.ColumnCount = 1;
    _outerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
    _outerLayout.Controls.Add(_toolStrip, 0, 0);
    _outerLayout.Controls.Add(_splitContainer, 0, 1);
    _outerLayout.Controls.Add(_statusStrip, 0, 2);
    _outerLayout.Dock = DockStyle.Fill;
    _outerLayout.Location = new Point(0, 0);
    _outerLayout.Name = "_outerLayout";
    _outerLayout.RowCount = 3;
    _outerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
    _outerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
    _outerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
    _outerLayout.Size = new Size(800, 600);
    _outerLayout.TabIndex = 0;

    // _toolStrip
    _toolStrip.Location = new Point(0, 0);
    _toolStrip.Name = "_toolStrip";
    _toolStrip.Size = new Size(800, 25);
    _toolStrip.TabIndex = 0;

    // _splitContainer
    _splitContainer.Dock = DockStyle.Fill;
    _splitContainer.Location = new Point(3, 28);
    _splitContainer.Name = "_splitContainer";
    _splitContainer.Size = new Size(794, 544);
    _splitContainer.SplitterDistance = 250;
    _splitContainer.TabIndex = 1;

    // _treeView (in Panel1)
    _treeView.Dock = DockStyle.Fill;
    _treeView.Location = new Point(0, 0);
    _treeView.Name = "_treeView";
    _treeView.Size = new Size(250, 544);
    _treeView.TabIndex = 0;

    // _detailsPanel (in Panel2)
    _detailsPanel.ColumnCount = 2;
    _detailsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
    _detailsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
    _detailsPanel.Controls.Add(_lblDetail1, 0, 0);
    _detailsPanel.Controls.Add(_txtDetail1, 1, 0);
    _detailsPanel.Dock = DockStyle.Fill;
    _detailsPanel.Location = new Point(0, 0);
    _detailsPanel.Name = "_detailsPanel";
    _detailsPanel.RowCount = 2;
    _detailsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
    _detailsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
    _detailsPanel.Size = new Size(540, 544);
    _detailsPanel.TabIndex = 0;

    // _lblDetail1
    _lblDetail1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    _lblDetail1.AutoSize = true;
    _lblDetail1.Location = new Point(3, 5);
    _lblDetail1.Name = "_lblDetail1";
    _lblDetail1.Size = new Size(42, 15);
    _lblDetail1.TabIndex = 0;
    _lblDetail1.Text = "Name:";

    // _txtDetail1
    _txtDetail1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
    _txtDetail1.Location = new Point(51, 3);
    _txtDetail1.Name = "_txtDetail1";
    _txtDetail1.Size = new Size(486, 23);
    _txtDetail1.TabIndex = 1;

    // _statusStrip
    _statusStrip.Location = new Point(0, 575);
    _statusStrip.Name = "_statusStrip";
    _statusStrip.Size = new Size(800, 25);
    _statusStrip.TabIndex = 2;

    // _statusLabel
    _statusLabel.Name = "_statusLabel";
    _statusLabel.Size = new Size(39, 20);
    _statusLabel.Text = "Ready";

    // Wire up panels
    _splitContainer.Panel1.Controls.Add(_treeView);
    _splitContainer.Panel2.Controls.Add(_detailsPanel);
    _statusStrip.Items.Add(_statusLabel);

    // MainForm
    AutoScaleDimensions = new SizeF(7F, 15F);
    AutoScaleMode = AutoScaleMode.Font;
    ClientSize = new Size(800, 600);
    Controls.Add(_outerLayout);
    Name = "MainForm";
    Text = "Application";

    _detailsPanel.ResumeLayout(false);
    _detailsPanel.PerformLayout();
    _splitContainer.Panel1.ResumeLayout(false);
    _splitContainer.Panel2.ResumeLayout(false);
    ((ISupportInitialize)_splitContainer).EndInit();
    _splitContainer.ResumeLayout(false);
    _outerLayout.ResumeLayout(false);
    _outerLayout.PerformLayout();
    ResumeLayout(false);
    PerformLayout();
}

private TableLayoutPanel _outerLayout;
private ToolStrip _toolStrip;
private SplitContainer _splitContainer;
private TreeView _treeView;
private TableLayoutPanel _detailsPanel;
private Label _lblDetail1;
private TextBox _txtDetail1;
private StatusStrip _statusStrip;
private ToolStripStatusLabel _statusLabel;
```

**Key points:**

- All controls are backing fields, not local variables
- No helper methods — everything is in `InitializeComponent`
- SuspendLayout/ResumeLayout called for each container
- ISupportInitialize used for SplitContainer
- Form configuration is LAST

## RadioButton Cluster Pattern (Designer-Compatible)

RadioButton groups use a GroupBox with a nested single-column TableLayoutPanel. All configured in `InitializeComponent`:

```csharp
// Inside InitializeComponent (showing relevant section)

// Instantiation
_grpOptions = new GroupBox();
_radioLayout = new TableLayoutPanel();
_rbOption1 = new RadioButton();
_rbOption2 = new RadioButton();
_rbOption3 = new RadioButton();

// ...after SuspendLayout()...

// _grpOptions
_grpOptions.AutoSize = true;
_grpOptions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
_grpOptions.Controls.Add(_radioLayout);
_grpOptions.Location = new Point(3, 3);
_grpOptions.Name = "_grpOptions";
_grpOptions.Size = new Size(150, 100);
_grpOptions.TabIndex = 0;
_grpOptions.TabStop = false;
_grpOptions.Text = "Select Option";

// _radioLayout
_radioLayout.AutoSize = true;
_radioLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
_radioLayout.ColumnCount = 1;
_radioLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
_radioLayout.Controls.Add(_rbOption1, 0, 0);
_radioLayout.Controls.Add(_rbOption2, 0, 1);
_radioLayout.Controls.Add(_rbOption3, 0, 2);
_radioLayout.Dock = DockStyle.Fill;
_radioLayout.Location = new Point(3, 19);
_radioLayout.Name = "_radioLayout";
_radioLayout.RowCount = 3;
_radioLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
_radioLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
_radioLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
_radioLayout.Size = new Size(144, 78);
_radioLayout.TabIndex = 0;

// _rbOption1
_rbOption1.AutoSize = true;
_rbOption1.Location = new Point(3, 3);
_rbOption1.Name = "_rbOption1";
_rbOption1.Size = new Size(72, 19);
_rbOption1.TabIndex = 0;
_rbOption1.Text = "Option 1";
_rbOption1.UseVisualStyleBackColor = true;

// _rbOption2
_rbOption2.AutoSize = true;
_rbOption2.Location = new Point(3, 28);
_rbOption2.Name = "_rbOption2";
_rbOption2.Size = new Size(72, 19);
_rbOption2.TabIndex = 1;
_rbOption2.Text = "Option 2";
_rbOption2.UseVisualStyleBackColor = true;

// _rbOption3
_rbOption3.AutoSize = true;
_rbOption3.Location = new Point(3, 53);
_rbOption3.Name = "_rbOption3";
_rbOption3.Size = new Size(72, 19);
_rbOption3.TabIndex = 2;
_rbOption3.Text = "Option 3";
_rbOption3.UseVisualStyleBackColor = true;

// Backing fields at EOF:
private GroupBox _grpOptions;
private TableLayoutPanel _radioLayout;
private RadioButton _rbOption1;
private RadioButton _rbOption2;
private RadioButton _rbOption3;
```

The radio layout fills the GroupBox interior while both nested containers measure their children. If the GroupBox is in a wider parent cell, dock it to fill that cell only when the section should stretch horizontally; retain `AutoSize`/`GrowAndShrink` so its content determines its required height.

## Form/Control Initialization Order

### Constructor Execution Chain

**CRITICAL:** WinForms uses OOP inheritance and virtual methods heavily. For inherited controls, the inherited constructor does NOT run first!

**Order of Execution:**

1. Base class field initializers
2. Base class constructor
3. (If overridden) `DefaultSize` of youngest descendant
4. (If overridden) `SetStyle` of youngest descendant
5. Likely all `SetStyle` calls in inheritance chain up to base
6. (If overridden) `CreateParams` of youngest descendant
7. Likely all `CreateParams` calls in inheritance chain up to base
8. All constructor bodies, oldest to youngest descendant
9. Field initializers of youngest descendant
10. Constructor body of youngest descendant

### Why This Matters

**When overriding `CreateParams` or `SetStyle`:**

- Class constructor has NOT run yet
- Accessing backing fields may get `null` or uninitialized values
- Return const values for `DefaultSize` — don't depend on constructor state

**Example:**

```csharp
public class CustomControl : Control
{
    private int _borderWidth;

    public CustomControl()
    {
        _borderWidth = 2;  // This runs AFTER CreateParams
    }

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            // ❌ WRONG - _borderWidth is still 0 here!
            // cp.ExStyle |= _borderWidth;

            // ✅ CORRECT - Use const or literal
            cp.ExStyle |= 2;
            return cp;
        }
    }

    protected override Size DefaultSize
    {
        get
        {
            // ✅ CORRECT - Return const values
            return new Size(200, 100);

            // ❌ WRONG - Don't depend on fields
            // return new Size(_width, _height);
        }
    }
}
```

## Fullscreen Pattern (Presentations, Kiosk Mode)

### CRITICAL: State Initialization

**Initialize state variables BEFORE entering fullscreen!**

```csharp
public class PresentationForm : Form
{
    private bool _isFullscreen = false;
    private FormWindowState _previousWindowState;
    private FormBorderStyle _previousBorderStyle;

    public PresentationForm()
    {
        InitializeComponent();

        // CRITICAL: Store state BEFORE first fullscreen call
        _previousWindowState = WindowState;
        _previousBorderStyle = FormBorderStyle;

        // Now safe to enter fullscreen
        EnterFullscreen();
    }

    private void EnterFullscreen()
    {
        if (_isFullscreen) return;

        _previousWindowState = WindowState;
        _previousBorderStyle = FormBorderStyle;

        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        _isFullscreen = true;
    }

    private void ExitFullscreen()
    {
        if (!_isFullscreen) return;

        FormBorderStyle = _previousBorderStyle;
        WindowState = _previousWindowState;
        _isFullscreen = false;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F11)
        {
            if (_isFullscreen)
                ExitFullscreen();
            else
                EnterFullscreen();
            return true;
        }

        if (keyData == Keys.Escape && _isFullscreen)
        {
            ExitFullscreen();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }
}
```

**Without pre-initialization:** Form appears offset (approximately 1/8th screen) on first fullscreen.

## Modal Dialog Patterns

### Dialog Button Configuration

| Aspect | Rule |
|--------|------|
| Primary button (OK) | `AcceptButton` property, `DialogResult = OK` |
| Secondary button (Cancel) | `CancelButton` property, `DialogResult = Cancel` |
| Close strategy | `DialogResult` applied automatically, no additional code needed |
| Validation | Perform at Form level, never block focus change with `Cancel = true` |

### Example: Modal Dialog (Designer-Compatible)

For a **non-resizable** dialog, size the outer container to its content: use `AutoSize`/`GrowAndShrink` on the layout and Form, `AutoSize` rows for prompt, input, and button panel, and dock the layout `Top` into a scrollable Form. Do **not** use a percent-height spring or `DockStyle.Fill` on the root: the content, not an arbitrary initial `ClientSize`, determines required height. The `ClientSize` below is a Designer starting value, not a height constraint. At runtime, limit the window to the current monitor's working area and keep overflow reachable through `AutoScroll` (example after the Designer code).

```csharp
// InputDialog.cs (main file)
public partial class InputDialog : Form
{
    public string InputValue => _txtInput.Text;

    public InputDialog(string prompt)
    {
        InitializeComponent();
        _lblPrompt.Text = prompt;
    }
}

// InputDialog.Designer.cs
partial class InputDialog
{
    private void InitializeComponent()
    {
        _layoutPanel = new TableLayoutPanel();
        _lblPrompt = new Label();
        _txtInput = new TextBox();
        _buttonPanel = new TableLayoutPanel();
        _btnOK = new Button();
        _btnCancel = new Button();

        SuspendLayout();
        _layoutPanel.SuspendLayout();
        _buttonPanel.SuspendLayout();

        // _layoutPanel
        _layoutPanel.AutoSize = true;
        _layoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _layoutPanel.ColumnCount = 1;
        _layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layoutPanel.Controls.Add(_lblPrompt, 0, 0);
        _layoutPanel.Controls.Add(_txtInput, 0, 1);
        _layoutPanel.Controls.Add(_buttonPanel, 0, 2);
        _layoutPanel.Dock = DockStyle.Top;
        _layoutPanel.Location = new Point(0, 0);
        _layoutPanel.Name = "_layoutPanel";
        _layoutPanel.RowCount = 3;
        _layoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layoutPanel.Size = new Size(350, 150);
        _layoutPanel.TabIndex = 0;

        // _lblPrompt
        _lblPrompt.AutoSize = true;
        _lblPrompt.Location = new Point(3, 3);
        _lblPrompt.Margin = new Padding(3);
        _lblPrompt.Name = "_lblPrompt";
        _lblPrompt.Size = new Size(47, 15);
        _lblPrompt.TabIndex = 0;
        _lblPrompt.Text = "Prompt:";

        // _txtInput
        _txtInput.Dock = DockStyle.Fill;
        _txtInput.Location = new Point(3, 24);
        _txtInput.Margin = new Padding(3);
        _txtInput.Name = "_txtInput";
        _txtInput.Size = new Size(344, 23);
        _txtInput.TabIndex = 1;

        // _buttonPanel
        _buttonPanel.AutoSize = true;
        _buttonPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _buttonPanel.ColumnCount = 2;
        _buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _buttonPanel.Controls.Add(_btnOK, 0, 0);
        _buttonPanel.Controls.Add(_btnCancel, 1, 0);
        _buttonPanel.Dock = DockStyle.Fill;
        _buttonPanel.Name = "_buttonPanel";
        _buttonPanel.RowCount = 1;
        _buttonPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _buttonPanel.TabIndex = 2;

        // _btnOK
        _btnOK.AutoSize = true;
        _btnOK.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _btnOK.DialogResult = DialogResult.OK;
        _btnOK.Dock = DockStyle.Fill;
        _btnOK.Margin = new Padding(4);
        _btnOK.Name = "_btnOK";
        _btnOK.TabIndex = 0;
        _btnOK.Text = "OK";
        _btnOK.UseVisualStyleBackColor = true;

        // _btnCancel
        _btnCancel.AutoSize = true;
        _btnCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _btnCancel.DialogResult = DialogResult.Cancel;
        _btnCancel.Dock = DockStyle.Fill;
        _btnCancel.Margin = new Padding(4);
        _btnCancel.Name = "_btnCancel";
        _btnCancel.TabIndex = 1;
        _btnCancel.Text = "Cancel";
        _btnCancel.UseVisualStyleBackColor = true;

        // InputDialog
        AcceptButton = _btnOK;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoScroll = true;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        CancelButton = _btnCancel;
        ClientSize = new Size(350, 150);
        Controls.Add(_layoutPanel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "InputDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Input";

        _buttonPanel.ResumeLayout(false);
        _buttonPanel.PerformLayout();
        _layoutPanel.ResumeLayout(false);
        _layoutPanel.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private TableLayoutPanel _layoutPanel;
    private Label _lblPrompt;
    private TextBox _txtInput;
    private TableLayoutPanel _buttonPanel;
    private Button _btnOK;
    private Button _btnCancel;
}
```

The Form's working-area cap belongs in `InputDialog.cs`, **not** in `InitializeComponent`. After placement on the owner's monitor, cap the *outer* window size and re-layout; when content exceeds it, `AutoScroll` keeps the prompt, input and buttons reachable. For example, in the main partial class (alongside the constructor above):

```csharp
protected override void OnShown(EventArgs e)
{
    base.OnShown(e);
    MaximumSize = Screen.FromControl(this).WorkingArea.Size;
    PerformLayout();
}
```

Re-evaluate when the monitor/DPI changes; do not set an arbitrary fixed height or rely on clipping at the display edge. At 10, 18, 24 and 36 pt, test with a long prompt and keyboard access to the buttons.

**Usage:**

```csharp
using InputDialog dialog = new InputDialog("Enter name:");
if (dialog.ShowDialog() == DialogResult.OK)
{
    string value = dialog.InputValue;
}
```

### .NET 8+ DataContext Pattern

```csharp
public partial class PersonEditDialog : Form
{
    private PersonViewModel _viewModel;

    public PersonEditDialog(PersonViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        _viewModel = viewModel;

        InitializeComponent();
        DataContext = _viewModel;
    }
}
```

## Button Layout Pattern (High-DPI Aware)

### Goal

Ensure buttons in a logical group (OK/Cancel or Yes/No/Cancel) have:

- Same size
- Never clip text
- Scale properly with DPI

### Strict equal-size dialog buttons

Put the dialog buttons in their **own nested `TableLayoutPanel`**, docked to fill an **AutoSize** row of the parent. On the button panel set `AutoSize = true`, `AutoSizeMode = AutoSizeMode.GrowAndShrink`, one `AutoSize` row, and **N `Percent` columns each at `100F / N`** (for two buttons, two `50F` columns; for three, use `33.33333F` each). Each button uses `AutoSize = true`, `AutoSizeMode = AutoSizeMode.GrowAndShrink`, `Margin = new Padding(4)`, and `Dock = DockStyle.Fill` to fill its own cell; the modal example above shows the two-button configuration in `InitializeComponent`. Never put the buttons in a percent-height spring row. A `FlowLayoutPanel` lays out buttons by preferred width and does **not** guarantee equal widths, even if you set equal `MinimumSize`; reserve it for groups where unequal widths are acceptable.

For captions over 20 characters, choose a readable breakpoint and explicitly insert `"\n"` in the button's Designer `Text` assignment (for example, `_btnApply.Text = "Apply selected\nsettings";`). Do not rely on automatic wrapping. Keep all instantiation, row/column styles, margins, docking, and literal text assignments declarative in `InitializeComponent`.

**Pixel-equality caveat (verified in the current app at 10/18/24/36 pt):** Equal percentage columns alone can leave neighboring `AutoSize` buttons **2–3 px different** because of WinForms measurement/rounding. When *strict measured pixel equality* is required, the parent Form applies **both** runtime steps to its visible buttons: (1) clear any previous per-button width caps, measure each button's natural preferred width, and set every visible `Button.MinimumSize.Width` to the largest measured width; (2) let the `TableLayoutPanel` lay out, find the **minimum displayed `Button.Width`**, and set every visible `Button.MaximumSize.Width` to that value. Re-layout and check that every visible button has the same width without clipping its caption. The **button panel remains `AutoSize`/`GrowAndShrink` and `DockStyle.Fill` in the parent's `AutoSize` row**; do **not** cap the panel's `MaximumSize.Width`. Repeat the measurement and cap after font, DPI, visibility, or caption changes, clearing stale per-button caps first and guarding against recursive layout updates. This per-button procedure produced pixel-equal buttons at 10, 18, 24 and 36 pt in the current app. Measurement and equalization belong in regular Form code after `InitializeComponent`, **never in `InitializeComponent` or `.Designer.cs`**.

## Window Position Management

### Saving and Restoring

When saving a resizable window, use `Bounds` in normal state and `RestoreBounds` while maximized or minimized. `RestoreBounds` is the last normal rectangle, **not** the current on-screen position of a maximized window. On load, restore the saved normal bounds through `Bounds`/`Location`/`Size` *before* restoring `WindowState`; check that the rectangle is reachable on a current monitor.

```csharp
public partial class MainForm : Form
{
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        Rectangle normalBounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
        Properties.Settings.Default.WindowX = normalBounds.X;
        Properties.Settings.Default.WindowY = normalBounds.Y;
        Properties.Settings.Default.WindowWidth = normalBounds.Width;
        Properties.Settings.Default.WindowHeight = normalBounds.Height;
        Properties.Settings.Default.WindowState = WindowState == FormWindowState.Minimized
            ? FormWindowState.Normal : WindowState;
        Properties.Settings.Default.Save();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (Properties.Settings.Default.WindowWidth > 0)
        {
            Rectangle savedBounds = new Rectangle(
                Properties.Settings.Default.WindowX,
                Properties.Settings.Default.WindowY,
                Properties.Settings.Default.WindowWidth,
                Properties.Settings.Default.WindowHeight);
            if (Screen.FromRectangle(savedBounds).WorkingArea.IntersectsWith(savedBounds))
            {
                StartPosition = FormStartPosition.Manual;
                Bounds = savedBounds;
            }
            WindowState = Properties.Settings.Default.WindowState;
        }
    }
}
```

**Which bounds to save:**

- `Bounds`/`Location`/`Size` represent the current window when normal; when maximized or minimized, use `RestoreBounds` to retain its normal position and dimensions instead.
- Read normal/restore bounds when closing, not at startup before the form has been positioned. Validate saved bounds against the current monitor working areas before applying them on load.
- Do not restore a minimized state on startup; this position-saving pattern is for resizable windows, not content-sized modal dialogs.

## Accessibility

### Essential Accessibility Features

Set these in `InitializeComponent` (Designer-compatible):

```csharp
// Inside InitializeComponent
_txtName.AccessibleName = "Name";
_txtName.AccessibleDescription = "Enter the person's full name";

// Maintain logical TabIndex
_txtName.TabIndex = 0;
_txtEmail.TabIndex = 1;
_btnOK.TabIndex = 2;
_btnCancel.TabIndex = 3;
```

Verify keyboard-only navigation works: Tab, Shift+Tab, Enter, Escape.

## Resources and Localization

### Layout Considerations for Localization

- Account for longer translations (German, Finnish typically longest)
- Use AutoSize where possible
- Test with longest expected translations
- Use TableLayoutPanel percentage columns for flexibility
- Avoid fixed-width columns for text content

## Best Practices Summary

### DO

✅ Use TableLayoutPanel for structured layouts
✅ Prefer AutoSize > Percent > Absolute sizing
✅ Use AutoSize/GrowAndShrink nested layouts and GroupBoxes; AutoSize real content rows
✅ Leave a percent spring row empty in a height-constrained parent
✅ Anchor preferred-size controls; dock value/fill controls and equal-size buttons where needed
✅ Use a nested AutoSize TableLayoutPanel with equal percent columns for dialog buttons
✅ Set Margin on controls (min 3px)
✅ Save normal Bounds or maximized RestoreBounds for resizable windows
✅ Initialize fullscreen state before first call
✅ Use GroupBoxes for visual grouping
✅ Implement proper accessibility features
✅ Keep all layout configuration in `InitializeComponent`
✅ Declare controls as backing fields, not local variables

### DON'T

❌ Dock every TableLayoutPanel child indiscriminately
❌ Put real controls into an unoccupied percent spring row
❌ Use FlowLayoutPanel for strictly equal-size modal buttons
❌ Create one mega-grid for entire form
❌ Use Absolute sizing unless necessary
❌ Forget Padding doesn't work in TableLayoutPanel
❌ Save maximized Location/Size as normal window bounds
❌ Skip accessibility properties
❌ Design layouts that don't scale
❌ Extract control setup into helper methods (breaks Designer)
❌ Use object initializers for control properties
❌ Declare layout controls as local variables

## Troubleshooting

**Controls not sizing correctly:**

- Check `AutoSize` and `AutoSizeMode.GrowAndShrink` on nested layouts and GroupBoxes
- Verify Anchor versus intentional Dock on value/fill controls
- Ensure real content rows are AutoSize and any percent spring is empty
- Check Margin values

**Layout not responsive:**

- Use Percent sizing for spare space rather than real content
- Verify Anchor or Dock includes all needed sides
- Check that content-sized containers allow growth and that fixed windows cap overflow to the working area with scrolling

**DPI scaling issues:**

- Verify AutoScaleMode is set correctly
- Use percentage-based layouts
- Test on different DPI settings
- Avoid hard-coded pixel values

**Fullscreen appears offset:**

- Initialize state variables in constructor
- Store WindowState and FormBorderStyle before first fullscreen

## Summary

Effective WinForms High-DPI fluent layout requires:

- Strategic use of TableLayoutPanel and nesting
- AutoSize/GrowAndShrink content containers, AutoSize rows and unoccupied percent springs where appropriate
- Correct Anchor versus intentional Dock on controls
- DPI awareness and testing
- All layout code inside `InitializeComponent` (Designer-compatible)
- Accessibility consideration

Follow these patterns for layouts that scale properly across DPI settings, support accessibility, and remain Designer-compatible.
