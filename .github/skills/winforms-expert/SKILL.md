---
name: winforms-expert
description: >-
  USE FOR: WinForms, Windows Forms, System.Windows.Forms, Form or UserControl
  designer files (*.Designer.cs or *.Designer.vb), the Visual Studio WinForms
  Designer, TableLayoutPanel, BindingSource, DataGridView, Control.InvokeAsync,
  or custom-control serialization. DO NOT USE FOR: WPF, .NET MAUI, Avalonia, or
  any request with no Windows Forms marker. Local replacement for the upstream
  winforms-expert skill. Do not implement from this file.
---

# WinForms expert (local router)

This file replaces the upstream `dotnet/skills` `winforms-expert` monolith when
this folder is loaded instead of that plugin. Do not modify the upstream plugin.
Do not implement UI from this file.

1. Open [winforms-api-decision-guide](../winforms-api-decision-guide/SKILL.md) and complete Step 0 before choosing a WARP package.
2. Obey [winforms-designer-code](../winforms-designer-code/SKILL.md) for every Form, modal dialog, and UserControl.
3. After the surface choice, copy [assets/agents-md/winforms-app.md](../winforms-api-decision-guide/assets/agents-md/winforms-app.md) into the app repository only when that repository should persist the invariants.
4. Icons, first-run geometry, font bounds, hosted-view scaling, modal accept, and the nested footer are mandatory in the decision guide. Do not implement them from this file.

## Designer baseline

- Every application Form, modal dialog, and UserControl is `partial` with `.Designer.cs` or `.Designer.vb`.
- The designer partial owns declarative instance `InitializeComponent` and the single generated `Dispose(bool)` (`if (disposing && (components != null))`).
- The parameterless constructor is the design-time path. It calls `InitializeComponent`. Safe lightweight event hookup may follow. It must not take services, touch IO, start async work, create GPU/WebView/map devices, or hide or restyle the static UI.
- Runtime constructors chain `: this()`, then assign services and data.

## Do not

- Treat this file as the implementation guide.
- Convert the app to another UI stack unless asked.
- Change the target framework, DPI mode, or startup model unless the request requires it.

## Validation

The decision guide's selected owners were opened. Every new Form, dialog, and UserControl meets the baseline. A successful build is not a Designer round-trip.
