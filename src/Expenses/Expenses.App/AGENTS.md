# Expenses WinForms UI

This project targets .NET 11 on Windows without WARP. Preserve package versions,
storage behavior, and bookkeeping terminology. Use built-in WinForms APIs and
small local helpers; PDFsharp-GDI remains the PDF exporter.

## Read before UI work

The deployed skills live in `..\..\..\.github\skills`. Start with
`winforms-expert\SKILL.md`, `winforms-api-decision-guide\SKILL.md`,
`winforms-designer-code\SKILL.md`, and `warp-api-decision-guide\SKILL.md`.
Then load only the topic skills and references needed for the change.

## Designer contract

- Inventory the main Form, every modal dialog, and reusable UserControls before
  choosing WARP APIs. Give each options category and filter view its own
  designer-editable UserControl.
- Every application Form and UserControl must be partial and have a matching
  `.Designer.cs` file. This includes reminder, report, and picker dialogs; do not
  assemble them as anonymous `new Form()` instances.
- Provide a public parameterless construction path that calls
  `InitializeComponent` without resolving services, loading settings or data,
  accessing files, starting async work, or creating graphics devices.
  Preserve runtime constructor overloads and chain them through `: this()`.
- Keep static controls, labels, columns, menus, toolbars, layout, and
  designer-supported component collections in the designer partial using named
  fields and ordinary serializer-compatible statements. Use a single generated
  disposal path for owned components.
- Keep event behavior, runtime data binding, validation, service access, and
  genuinely dynamic content in the main partial. Do not hide the editor's design
  surface during parameterless construction merely to implement runtime
  no-selection behavior.
- Place fixed TabPages and their views in the Designer partial.

## Preserve UI behavior

Use native `TabControl` for result filters and `DataGridView` for tables.
Options uses text-only ToolStrip category headers and a panel of UserControls.
Retain masked Gregorian U.S. date and USD entry, draft-only options editing,
Save/Cancel/Stash navigation locking, and all existing commands and shortcuts.
Use responsive layout and allow dialogs and editor content to remain usable
when resized, scaled, or given longer text.

## Icons and scaling

- Ordinary command ToolStrips require semantic icons. A text-only strip needs
  an explicit user request or domain-specific text/header purpose. This app uses
  a shared local symbol-font image factory (no WARP). Preserve command Text,
  shortcuts, tooltips, and accessible names.
- Regenerate owned raster images from a stable logical baseline using the
  settled font and current host DPI once. Keep actual pixels and
  `ImageScalingSize` consistent; dispose replaced owned images. Apply the same
  ownership and scaling policy to menu/status images and tree/list image lists.
  Use the stored logical font preference, not observed `Font.SizeInPoints`
  multiplied by DPI: Per-Monitor V2 may already have scaled the observed Font.
  Alternatively measure the live native font without adding a second DPI factor.
- First-run LOB window normal bounds use approximately 75% of the selected
  monitor's working-area width and 80% of its height, centered. Use
  `Screen.WorkingArea`, not screen bounds including the taskbar. Modal tools
  follow their content sizing instead.
- Persist normal window geometry as 96-DPI DIPs relative to the monitor's
  working-area origin, with monitor identity, session DPI/scaling, monitor
  resolution/working area, schema version, and maximized state. Do not use
  font-relative coordinates. Migrate old pixel-only settings explicitly.
- Set the final font and settle child scaling before restoring normal bounds
  with the selected monitor's current DPI, then restore maximized state.
  Clamp to the current working area and handle missing monitors.
- Font-only changes must preserve the top-level window's outer bounds and
  state while allowing child layout to grow or scroll. `SuspendLayout` alone
  does not stop automatic scaling. Follow the tested transaction in the layout
  skill, including top-level pixel minimum/maximum constraints; do not suppress
  genuine DPI transitions or OS-suggested bounds. The scratch transaction covers
  normal windows; maximized normal-state restoration needs separate verification.
- Hosted tab, wizard, and decomposed view UserControls use
  `AutoScaleMode.Inherit`. The containing Form is the scaling root.

## Modal data and button layout

- For non-MVVM dialogs, `Form.DataContext` carries the editable input/output
  draft. Commit only after successful OK; Cancel must not mutate live data.
- Assign the OK/Cancel buttons to `AcceptButton`/`CancelButton`. Simple dialogs
  use button `DialogResult.OK`/`Cancel`; reject a simple OK close in
  `OnFormClosing` with `e.Cancel`. Complex/async validation must keep the dialog
  open until successful validation and follows the canonical validation skill.
- Do not use `FlowLayoutPanel` for precise command-button placement. Use the
  layout skill's nested outer/inner TableLayoutPanel footer: both AutoSize and
  GrowAndShrink, inner equal-percent columns with Dock.Fill autosized buttons,
  outer 100%-plus-AutoSize columns and two AutoSize rows, empty spacer Label
  above the inner panel with five logical pixels of top/bottom margin.
- Place the outer footer in an AutoSize root row and span the root columns.
  Resizable layouts need a deliberate spring region before the footer; do not
  accidentally split a viewport's allocation by adding a competing spring.
  If AutoSize content exceeds a bounded root, use one measured scrollable
  Percent viewport: an empty spring can become negative and overlap the footer.
  Transpose the nested structure for an explicitly requested vertical layout.
- Wrap button captions longer than 30 characters strategically. Validate the
  complete preferred-size chain, not only the leaf buttons.
- GroupBoxes sit in TLP cells, are Dock.Fill/AutoSize/GrowAndShrink, and contain
  a single inner TLP with the same sizing contract.
- Empty text/masked/combo/numeric editors need font-aware capacity minima that
  do not accumulate across layout passes. Traverse Controls and pattern-match
  input types; never use reflection for this layout helper. Prefer masked
  date/time/currency entry over DateTimePicker or MonthCalendar for this app.

## Validation

Run focused STA UI tests before the solution build:

```powershell
dotnet test ..\Expenses.Tests\Expenses.Tests.csproj --nologo --filter "FullyQualifiedName~FormDesignerUiTests|FullyQualifiedName~ViewDesignerUiTests|FullyQualifiedName~FontScalingScratchTests"
dotnet build ..\Expenses.slnx --nologo
```

The test-only `..\Expenses.Tests\Scratch` Forms and `FontScalingScratchTests`
exercise font transactions, inherited view scaling, nested footers, image sizing,
and DIP-based restoration. They do not replace runtime checks of the real app.

Test parameterless construction independently of runtime service injection.
Exercise the changed layout and editing behavior, not just the existence of
designer files. Compilation and structural checks do not prove a Visual Studio
OOP Designer save/reopen round trip; state explicitly when that was not exercised.
