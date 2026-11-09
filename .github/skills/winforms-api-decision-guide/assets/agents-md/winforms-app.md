# WinForms application invariants

Permanent rules for this repository. Replace placeholders. Do not delete the invariants.

## Designer baseline

- Every application Form, modal dialog, and UserControl is a partial class with a matching `.Designer.cs` or `.Designer.vb`.
- The designer partial owns declarative `InitializeComponent` and the only generated `Dispose(bool)`.
- The parameterless constructor is the design-time path. It may hook lightweight event handlers after `InitializeComponent`. It must not take services, touch IO, start async work, create GPU/WebView/map devices, or hide or restyle the static UI.
- Runtime constructors chain `: this()`, then assign services, data, and runtime-only state.
- Decide surfaces and containment before choosing WARP or other host APIs. WARP swaps the host; it does not replace layout or Designer rules.

## Project choices

- Language: `{C# or VB}`
- Target framework: `{net8.0-windows | net9.0-windows | net10.0-windows | net11.0-windows | net48}`
- DPI mode: `{PerMonitorV2 unless this project already chose otherwise}`
- Startup: `{C#: ApplicationConfiguration.Initialize, no second SetHighDpiMode | classic VB Application Framework, no Program.vb | WARP VB: Program.vb Sub Main, Application Framework off}`
- UI additions: `{none | WARP packages actually referenced}`
- Packaging vehicle: `{none | single-project MSIX | WiX MSI | WAP}` — the setup skill chooses the vehicle. Store identity, architectures, and submission stay with the store skill.

## Surfaces

- Main window: `{Form name}`
- Modal dialogs: `{names}`
- UserControls and their hosts: `{name -> container}`

## Interaction

- Command ToolStrips have semantic icons unless the user explicitly asked for a text-domain strip. Non-WARP uses the symbol-font factory. WARP uses `warp-toolstrip-icons` and `warp-symbols`.
- LOB main-window first run is about 75% by 80% of the selected monitor `WorkingArea`, centered, Normal. Persist monitor-local 96-DPI DIPs. Do not apply those ratios to small modals.
- A runtime font change must not grow top-level outer bounds. `SuspendLayout` does not stop AutoScale. Honor OS DPI suggested bounds.
- Hosted tab, wizard, and view UserControls use `AutoScaleMode.Inherit`. The host owns scaling.
- Non-MVVM modals pass a draft via `DataContext` on .NET 8+. Commit only after a successful OK.
- Command buttons use the nested footer, not `FlowLayoutPanel`.

## Validation

- The Designer opens each surface without runtime services, data, IO, async, or GPU.
- Build success is not a Designer save/reopen. Report which of those checks actually ran.
