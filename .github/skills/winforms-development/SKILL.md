---
name: winforms-development
description: >-
  USE FOR: creating or maintaining a .NET WinForms application or control
  library, including project setup, startup, Designer-compatible organization,
  and C#/VB conventions. DO NOT USE FOR: Designer serialization details,
  binding, MVVM, rendering, high-DPI layout, or packaging. Those have owners.
---

# WinForms development

Modern WinForms keeps Designer serialization and application code apart. Inspect the existing target, language, and startup before changing them.

## Designer baseline

- Every application Form, modal dialog, and UserControl is `partial` with `.Designer.cs` or `.Designer.vb`.
- The designer partial owns declarative instance `InitializeComponent` and the single generated `Dispose(bool)`.
- The parameterless constructor is the design-time path. Safe lightweight event hookup may follow `InitializeComponent`. No services, IO, async, GPU/WebView/map devices, or hidden chrome on that path.
- Runtime constructors chain `: this()`.
- Serialization rules: [winforms-designer-code](../winforms-designer-code/SKILL.md).

## Startup

New C# SDK apps call `ApplicationConfiguration.Initialize()` once. That already applies `ApplicationHighDpiMode`. Do not call `SetHighDpiMode` afterward. New projects set `PerMonitorV2` in the project file unless the project already chose another mode. `Application.SetColorMode` is a .NET 9+ call before `Application.Run`, not a second DPI call.

Application Framework VB apps set `e.HighDpiMode` and `e.ColorMode` in the existing `ApplyApplicationDefaults` handler and do not get `Program.vb`. A WARP-hosted VB app (`WinFormsApplicationBuilder`) is the exception: it ships `Program.vb` with `<StartupObject>Sub Main</StartupObject>` and the Application Framework off, because `WinFormsApplication.Run` calls `Application.Run`. Do not delete that file to match the classic contract, and do not add `Program.vb` to a classic Application Framework app.

Exact project and startup fragments: [references/project-setup.md](references/project-setup.md). Language and migration notes: [references/conventions.md](references/conventions.md).

## What stays here

- Enable nullable and implicit usings for new SDK projects. Do not push those features into generated designer files.
- Document public, internal, and protected APIs the repository already documents. Do not add comments that repeat the code.
- `Items => new List<T>()` allocates on every get. `Items { get; } = new()` keeps one instance. Do not swap those shapes blindly.
- Async UI work belongs to [winforms-async-apis](../winforms-async-apis/SKILL.md). An `async void` event handler only awaits a `Task` method and observes exceptions. Do not treat `Application.ThreadException` as the normal error path.
- Dark mode uses `SystemColors` first. Absolute colors need an explicit theme branch. `SystemColors.ControlDark` is not "always dark" under dark mode.
- .NET 10 typed clipboard and `Form.ScreenCaptureMode` are explicit feature choices, not defaults to sprinkle into every app.

## Related owners

- Designer files: [winforms-designer-code](../winforms-designer-code/SKILL.md)
- Layout: [winforms-high-dpi-fluent-layout](../winforms-high-dpi-fluent-layout/SKILL.md)
- Binding: [winforms-databinding](../winforms-databinding/SKILL.md)
- MVVM, only when requested: [winforms-mvvm](../winforms-mvvm/SKILL.md)
- Rendering: [winforms-rendering](../winforms-rendering/SKILL.md)

## Validation

1. Startup sets DPI once, through the project property or the VB defaults event, not through a second `SetHighDpiMode`.
2. New Forms follow the designer baseline.
3. Framework and package upgrades are explicit. Report the build command separately from a Designer round-trip.
