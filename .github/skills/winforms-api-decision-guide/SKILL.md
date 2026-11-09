---
name: winforms-api-decision-guide
description: >-
  USE FOR: the first step on a WinForms feature, control choice, settings
  dialog, customer entry, preferences page, resizable window, background load,
  custom control, deployment, or maps request. DO NOT USE FOR: implementing a
  topic already owned by a selected skill, or for WPF, MAUI, or Avalonia.
  Selects skills and APIs. Does not replace their contracts.
---

# WinForms API decision guide

Open this skill first. Then open only the owners the table names.

## Step 0 — surfaces, then APIs

Inventory the Forms, modal dialogs, and UserControls. Decide which surface owns navigation, which UserControl owns each region, and what stays runtime-only. Then read the WinForms owner. WARP, if the app already uses it, only swaps a host or service after that choice. It does not decide containment and it does not replace Designer rules.

Copy [assets/agents-md/winforms-app.md](assets/agents-md/winforms-app.md) into an application repository when later sessions must keep these choices. Do not paste the whole skill set into that file.

## Designer baseline

- Every application Form, modal dialog, and UserControl is `partial` with `.Designer.cs` or `.Designer.vb`.
- The designer partial owns declarative instance `InitializeComponent` and the single generated `Dispose(bool)` (`if (disposing && (components != null))`).
- The parameterless constructor is the design-time path. It calls `InitializeComponent`. Safe lightweight event hookup may follow. It must not take services, touch IO, start async work, create GPU/WebView/map devices, or hide or restyle the static UI.
- Runtime constructors chain `: this()`, then assign services and data.
- Authoritative serialization rules: [winforms-designer-code](../winforms-designer-code/SKILL.md).

## Durable rules

- Inspect target framework, language, Designer use, and existing architecture before selecting APIs. Do not silently upgrade.
- Select by the user's requirement. Load only the owners named below.
- Treat each selected owner's constraints as implementation requirements.
- Re-open the relevant `SKILL.md` after context compaction or before changing phase.
- Keep static UI Designer-compatible. Keep runtime work UI-thread-safe.
- A command ToolStrip has semantic icons unless the user explicitly asked for a text-domain strip. Non-WARP uses [toolstrip icons](../winforms-toolstrip-icons/SKILL.md). WARP uses `warp-toolstrip-icons` and `warp-symbols`, not a local factory.
- LOB main-window first run is about 75% by 80% of the selected monitor `WorkingArea`, centered, Normal. Persist monitor-local 96-DPI DIPs. Do not apply those ratios to a small modal. A font change must not grow outer bounds. Honor OS DPI suggested bounds. [Window state](../winforms-high-dpi-fluent-layout/references/window-state.md#dip-schema).
- Hosted tab, wizard, and complex-form UserControls use `AutoScaleMode.Inherit`. The host owns scaling.
- A non-MVVM modal passes a draft in `Form.DataContext` on .NET 8+. Uncomplicated OK/Cancel use `AcceptButton`, `CancelButton`, and `DialogResult`. Commit only after a successful OK. Complex async validation stays open until the task succeeds.
- Command buttons use the nested footer, not `FlowLayoutPanel`. Empty text editors keep a font-settled minimum width.

## Requirement → owner

| Request | Read |
|---|---|
| New application, target/framework, startup, C#/VB conventions | [Development](../winforms-development/SKILL.md). Classic VB stays on the Application Framework. A WARP VB host keeps `Program.vb` / `Sub Main` |
| Tools/Options, preference categories, font/file/color settings | [Options](../winforms-options-dialog/SKILL.md), then [layout](../winforms-high-dpi-fluent-layout/SKILL.md) and [Designer](../winforms-designer-code/SKILL.md). WARP `FluentTabControl` is only the navigation host: load `warp-fluent-tab-control` after the category UserControls exist |
| Command toolbar, menu, or status icons | [ToolStrip icons](../winforms-toolstrip-icons/SKILL.md) when WARP is absent. With WARP, load `warp-toolstrip-icons` and `warp-symbols`. Text-only only when the user asked for a text-domain strip |
| Customer entry, clipped labels, DPI, dialog footer, first-run size, font-change bounds | [Layout](../winforms-high-dpi-fluent-layout/SKILL.md). Footer: [canonical modal footer](../winforms-high-dpi-fluent-layout/references/layout-examples.md#canonical-modal-footer). Geometry: [DIP schema](../winforms-high-dpi-fluent-layout/references/window-state.md#dip-schema) |
| Modal OK/Cancel without MVVM | [Databinding](../winforms-databinding/SKILL.md) for the draft and simple close gate. [Async](../winforms-async-apis/SKILL.md) when validation is async or cross-field. [Options](../winforms-options-dialog/SKILL.md) when the dialog is preferences |
| Reusable editor, wizard/tab page, custom control | [Custom/UserControls](../winforms-custom-and-usercontrols/SKILL.md), plus layout and Designer |
| Save/search binding, lists, validation, ErrorProvider | [Databinding](../winforms-databinding/SKILL.md) |
| Explicit MVVM, commands, shared ViewModels | [MVVM](../winforms-mvvm/SKILL.md), plus databinding |
| Worker progress, cross-thread UI, async dialogs | [Async](../winforms-async-apis/SKILL.md) |
| Flicker, owner-draw, text measurement | [Rendering](../winforms-rendering/SKILL.md) |
| COM/camera/render wrong-thread or teardown | [COM apartments](../winforms-com-apartments/SKILL.md) |
| Which installer, `.vdproj`, WiX, or package identity as a capability gate | [Setup](../winforms-setup-and-deployment/SKILL.md) chooses the vehicle |
| Store submission, MSIX identity, signing, x64/ARM64 after the vehicle is chosen | [Store packaging](../winforms-store-packaging/SKILL.md) |
| Offline maps, geocoding, embedded online map | [Windows Maps](../winforms-windows-maps/SKILL.md) |
| Explicit .NET 11 renderer, toggle, kiosk, painting suspension | [.NET 11](../winforms-net11-apis/SKILL.md). Layout and lifetime stay with their owners |

A customer-preferences dialog needs options, layout, Designer, and binding. It does not need every skill. A worker filling a grid needs async and binding, and layout only if the viewport changes.

## Version gates

| Target | Gate |
|---|---|
| .NET Framework | Preserve legacy startup and tooling. Dark mode, `InvokeAsync`, built-in commands, and the out-of-process Designer SDK are not Framework equivalents. |
| .NET 8+ | Built-in `DataContext` and button/menu commands. Older targets can still use explicit MVVM. |
| .NET 9 | `InvokeAsync`. Dark mode and async dialogs may still carry experimental diagnostics. |
| .NET 10 | Dark mode and async dialogs are stable. Prefer typed clipboard/JSON. Do not re-enable unsafe BinaryFormatter compatibility. |
| .NET 11 prerelease | Use only when the target is requested. Inspect the reference assembly. |
| Windows identity | Check OS, package identity, capability, and architecture. A Windows TFM is not proof an API works. |

Current .NET 10 clipboard and capture-policy details stay in [development](../winforms-development/SKILL.md).

## Final checks

1. Step 0 names every Form, dialog, and UserControl before a WARP package is chosen.
2. Selected owners cover the request. Setup chooses the vehicle. Store does not invent a second one.
3. Required target, OS, and identity gates remain in the working checklist.
4. Verify compilation and the relevant runtime behavior. Report an unopened Designer as unverified.
