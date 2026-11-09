---
name: warp-api-decision-guide
description: >-
  FIRST step for any task in a WinForms codebase that uses WARP-Toolkit
  (WarpToolkit.* packages). Inventories the UI surfaces (Forms, modal dialogs,
  UserControls), fixes the Designer hierarchy and runtime composition, then maps
  the task to the WARP package, entry-point types and the owning warp-* skill.
  Use before any topic-specific WARP skill; also use when asked "how do I do X
  with WARP" or "which WARP package".
metadata:
  source: "WARP src (WinFormsApplicationBuilder, Templates, Containers, AI)"
  versions: "references/versions.md"
---

# WARP-Toolkit API Decision Guide

Thin router. Steps 0–2 mirror `winforms-api-decision-guide` (which owns them);
WARP only swaps the *host* or the *API* of a surface after the surface itself is
decided.

## Designer invariant (applies to every WARP surface)

- Every app Form, modal dialog and UserControl is `partial` with a paired
  `.Designer.cs` / `.Designer.vb`.
- The parameterless constructor owns `InitializeComponent()`; a runtime/DI
  constructor chains `: this()` and never calls `InitializeComponent` itself.
- `InitializeComponent` is serialization: no services, live data binding, IO,
  async, GPU init, `AddTab`, `AddWizardPage`, `ConfigureItem`, `WireUpCommands`.
- The OOP Designer must open the surface with no host, DI container, network,
  or WARP runtime service; design-time gets `NullServiceProvider`.
- Runtime composition (tabs, wizard pages, DI-resolved child views, API keys,
  renderers) happens in `OnLoad` / `ISupportInitialize.EndInit`, guarded.
- Static controls and their serializable properties live in the Designer file,
  never in the constructor body after `InitializeComponent`.
- Decompose: one UserControl per tab, wizard page or complex region; the Form
  hosts, it does not contain everything. Only `Dispose` in the Designer partial.
- Render-only custom controls (`D2DControl`/`D2DPanel`/`Control` subclasses) may
  be plain classes; `Form`/`D2DForm`/`UserControl` surfaces may not.
- Rules of record: `winforms-designer-code` (serialization) and
  `winforms-high-dpi-fluent-layout` (layout rules; modal footer:
  [canonical modal footer](../winforms-high-dpi-fluent-layout/references/layout-examples.md#canonical-modal-footer)).
  Cite them; do not restate or copy older one-file Form samples.
- New Designer fields go **after** the methods in the Designer partial
  (analyzer WFO3002); code samples obey this too.

## Font, DPI and window contract (applies to every WARP surface)

- One scaling root per window: the Form carries `AutoScaleMode.Font` and the
  app font (`UseDefaultFont`, applied **before** geometry restore). Every hosted
  tab/wizard/region UserControl is `AutoScaleMode.Inherit` with no own `Font`
  or `AutoScaleMode.Dpi`; an explicit Font on a hosted ContainerControl is a
  missed scale (parent skips it, its own `OnFontChanged` does not run).
- Runtime font change = one transaction on the Form (`OnFontChanged` override,
  Normal state only): snapshot Bounds + Min/Max, `SuspendLayout`, base (AutoScale
  runs; `SuspendLayout` does not inhibit it), restore Min/Max then Bounds,
  `ResumeLayout(true)`, then regenerate symbol images once. Skip while
  Maximized and while `OnDpiChanged` is in progress; honor WM_DPICHANGED
  suggested bounds. Rule of record:
  [font-change-transaction](../winforms-high-dpi-fluent-layout/references/window-state.md#font-change-transaction),
  [dpi-suggested-bounds](../winforms-high-dpi-fluent-layout/references/window-state.md#dpi-suggested-bounds).
- After WM_DPICHANGED the observed `Font.SizeInPoints` is already DPI-scaled;
  size icons from a stored logical point size × `DeviceDpi/96` or from the live
  `Font.Height`, never both factors.
- Window geometry: LOB main window first run ≈75% `Screen.WorkingArea` width /
  80% height, centered on the chosen monitor; persist normal bounds as 96-DPI
  DIPs relative to that monitor's `WorkingArea.Location` plus monitor id, DPI,
  working area, schema version; maximized is a separate flag. WARP
  `ApplyFormPlacement/SaveFormPlacement` is an absolute-pixel legacy schema —
  migrate explicitly (`warp-app-services`). Rule of record:
  [dip-schema](../winforms-high-dpi-fluent-layout/references/window-state.md#dip-schema),
  [first-run-geometry](../winforms-high-dpi-fluent-layout/references/window-state.md#first-run-geometry).
- Command ToolStrips/MenuStrips carry icons (`SymbolToolStrip` + `Symbol`
  items); text-only is allowed only when the user states a text-domain intent
  (`warp-toolstrip-icons`).
- Command buttons never sit in a `FlowLayoutPanel`: nested-TLP footer
  (`canonical-modal-footer`), transposed for vertical stacks; FlowLayout stays
  for content. Modal non-MVVM dialogs: `Form.DataContext` holds the draft,
  `AcceptButton`/`CancelButton`, `DialogResult` OK/Cancel (`warp-app-services`).

## Workflow

0. **Inventory surfaces.** List every Form, modal dialog and UserControl the
   task touches or creates, including tab views and regions of a decomposed
   complex form. Name each file pair (`X.cs` + `X.Designer.cs`).
1. **Decide the Designer hierarchy.** Per surface: which static controls and
   serializable properties belong in `InitializeComponent`; which child
   UserControls are placed as Designer fields; parameterless ctor exists.
2. **Decide runtime composition.** What is added in `OnLoad`/`EndInit` (tab
   contents, wizard pages, DI-resolved views, key services, AI components'
   runtime wiring) and how it is guarded when the surface is Designer-opened.
3. **Only now choose WARP packages/APIs** from the table, and hand off to the
   owning skill. Versions: `references/versions.md` — the project decides.

## Decision table

| If the task is… | Reach for | Owner skill |
|---|---|---|
| Bootstrap/retrofit app with DI, config, logging, dark mode, fluent `Use*` | `WinFormsApplication.CreateBuilder(args)` / `WinFormsApplicationBuilder` (`WarpToolkit.WinForms.AppServices`) | `warp-winforms-application-builder` |
| Form/UserControl must be DI-resolvable (two ctors, `IServiceProvider` façade, deferral) | Canonical two-constructor shape | `warp-winforms-application-builder` |
| Dialog/confirmation/input **from a ViewModel**; modal non-MVVM `DataContext` dialog; user settings incl. window geometry (DIP schema vs legacy `ApplyFormPlacement`); exception service; API keys | `IDialogService`, `IUserSettingsService`, `IWinFormsAppExceptionService`, `EnvironmentVariableKeyService` | `warp-app-services` |
| Themed message box from UI code | `FluentMessageBox.Show(text, caption, …)` — no owner overload | `warp-messaging` |
| Tabbed UI (one `Inherit` UserControl per tab, first-run 75%/80% sizing, scrolling) | `FluentTabControl.AddTab(title, ContainerControl)` at runtime | `warp-fluent-tab-control` |
| Adorned TLP/borders/error flash; transparent overlay; nested-TLP footer, GroupBox AutoSize chain, spring-row rules | `AdornerPanel`, `AdornerTableLayoutPanel`, `TransparentPanel` | `warp-containers-layout` |
| Multi-page wizard | `WizardContainer.AddWizardPage`; `WizardDialogResult.Finished` | `warp-wizard` |
| Path picker, bindable combo, float sliders; adaptive input minima; date entry policy | `FilePathPicker`, `BindableComboBox`, `EditFloatingPointSlider` | `warp-input-controls` |
| Grids with Dark-mode theming or template rows | `WarpDataGridView`, `GridView` + `GridViewItemTemplate` | `warp-data-grids` |
| AutoView / property-to-control factories | `AutoViewBase<T>`, `TypeToControlFactoryBase` (scaffolding only) | `warp-binding-autoview` |
| Commands/MVVM glue, `WireUpCommands` | `WarpToolkit.WinForms.Extensions` command helpers | `warp-commands-mvvm` |
| Control-tree walk, converters, `ShowDialogAsync<T>` (`IModalDialogResult<T>`), DataGridView dark mode | `WarpToolkit.WinForms.Extensions` | `warp-winforms-extensions` |
| Fluent glyphs on ToolStrip/Menu/Status items, TreeView/ListView, app/tray icons | `SymbolToolStrip`/`SymbolMenuStrip`/`SymbolStatusStrip` + `Symbol*` items with `FontSymbol`; `ApplySymbols`, `ToIcon`; legacy `ConfigureItem` | `warp-symbols` (catalog), `warp-toolstrip-icons` (recipes); generic icon factory → `winforms-toolstrip-icons` |
| In-process console, hex dump, unique filenames | `ConsoleControl`, `HexAsciiDumper`, `FilenameDisambiguator` | `warp-console-diagnostics` |
| AI chat component + chat UI, prompts, streaming parser, history | `AIChatClientComponent`, `ChatView`, `[AITemplate]`, `ReturnTokenParser`, `ObservableChatHistory` | `warp-winforms-ai` |
| Hardware-accelerated 2D drawing | `D2DPanel`/`D2DControl` (render controls), `D2DForm` (surface) | `warp-directx-getting-started` |
| DirectComposition visuals, exclusive top-level rendering, DX text editors, internal render approaches | `WarpToolkit.WinForms.DirectX` visuals | `warp-directx-using-visuals`, `warp-directx-dcomp-exclusive-tlw`, `warp-directx-text-editors`, `warp-directx-internal-approaches` |
| Spell checking in editors/chat | `WarpToolkit.Windows.Spelling` provider | `warp-text-spelling` |
| GitHub/Octokit client, local git inspection, branch composition | `WarpToolkit.WinForms.Github` services | `warp-git-and-github-api` |
| Windows Maps (experimental) | project brief + reference | `warp-windows-maps-experimental` |
| Roslyn source rewriting, VB→C# conversion | `WarpToolkit.Desktop.Roslyn` (`VisualBasicConverter.ConvertText`) — no dedicated skill in this set; drive the library from a small CLI, hand-fix only `// TODO(vb-convert):` markers | — |
| Designer-aware logger, precision timer, Bluetooth, Typography, Async helpers | `WarpToolkit.Microsoft.Extensions`, `WarpToolkit.Windows.Interop`, `WarpToolkit.Windows.Bluetooth`, `WarpToolkit.WinForms.Typography`, `WarpToolkit.WinForms.Async` — verify installed API; no dedicated skill | — |

Generic WinForms concerns are owned by the WinForms set and are not restated
here: `winforms-designer-code`, `winforms-high-dpi-fluent-layout`,
`winforms-databinding`, `winforms-async`, `winforms-mvvm`,
`winforms-options-dialog`.

## Startup rules (C# and VB)

- C#: `Program.cs` with `[STAThread] Main` → `WinFormsApplication.CreateBuilder(args)…Build().Run()`.
  `UseStartupForm<TForm>()` requires `TForm : Form, new()` — the parameterless
  ctor is mandatory, not optional.
- VB, **WARP-hosted** (uses `WinFormsApplicationBuilder`): `Program.vb` with
  `Sub Main` and `<StartupObject>Sub Main</StartupObject>`; the Application
  Framework is off because `WinFormsApplication.Run` owns `Application.Run`.
  This is what every WARP VB template (`ModernWinFormsApp`, `WinFormsSysTrayApp`,
  `MvvmWinFormsApp`, DirectX apps) ships.
- VB, **classic** (no WARP host, Application Framework on): no `Program.vb`;
  `ApplicationEvents.vb` owns startup — the WinForms-set contract. Do not add a
  WARP host to such an app without switching it to the `Sub Main` model.
- Builder fluent members that exist: `UseStartupForm`, `UseHighDpiMode`,
  `UseColorMode`, `UseDefaultFont`, `UseLegacyVisualStyles(bool)`,
  `UseVisualStylesMode(VisualStylesMode)`, `UseTextRenderingV2`,
  `UseFormRevealMode`, `UseProjectSettings`, `AllowWinFormsJsonAppSettings`,
  `UseWinForms{Release,Debug,ReleaseOrDebug}Logger`. There is **no**
  `UseVisualStyles(bool)`.

## Anti-patterns

- `Application.Run(new MainForm())` next to `WinFormsApplicationBuilder`.
- DI-only constructor on a Form/UserControl (no parameterless path, or
  `InitializeComponent` called from the DI ctor).
- `MessageBox.Show` from view-models; `FluentMessageBox.Show(owner, …)` (no such overload).
- Composing tabs/wizard pages/`ConfigureItem` inside `InitializeComponent`.
- Setting `FormBorderStyle`/`WindowState`/render-mode properties in the ctor
  body instead of the Designer file.
- Hosted views with their own `Font`/`AutoScaleMode.Dpi`; restoring saved pixel
  bounds before the font settles; locking outer bounds during WM_DPICHANGED.
- Text-only command toolbars without a stated text-domain intent; OK/Cancel in
  a `FlowLayoutPanel`; `DateTimePicker`/`MonthCalendar` as default date entry.
- Sharing `D2DDeviceManager`/`IDWriteFactory` across panels on different threads.
- Hand-translating VB→C# file by file in chat.
- Inventing a packaging/setup vehicle: deployment is owned by the WinForms
  set's setup guidance (unpackaged, single-project MSIX, WiX or WAP — one choice).

## Per-project persistence

Append `assets/agents-md/warp-winforms.md` (WARP routes + version pin) below the
WinForms base fragment `winforms-api-decision-guide/assets/agents-md/winforms-app.md`
in the consuming repository's `AGENTS.md`; fill placeholders only.

## Final checks

1. Every surface in the inventory has a Designer pair and a parameterless ctor.
2. Nothing in `InitializeComponent` needs a host, service, data or GPU.
3. Exactly one owner skill was chosen per requirement; its version policy
   (`references/versions.md`) was respected — no "latest" bump.
4. One scaling root per window, hosted views `Inherit`, icons regenerated once
   per font/DPI baseline, geometry stored as monitor-relative DIPs.
