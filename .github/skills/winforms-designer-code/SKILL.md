---
name: winforms-designer-code
description: >-
  USE FOR: generating, modifying, reviewing, or migrating Designer-compatible
  WinForms Forms and UserControls, including .Designer.cs/.Designer.vb,
  InitializeComponent, and Designer serialization. DO NOT USE FOR: ordinary
  event-handler or service code, or for choosing layout, binding, or WARP
  packages. Pair with winforms-high-dpi-fluent-layout for layout quality.
---

# WinForms Designer code

`InitializeComponent` is a serialization format, not application code. The Designer parses it. It does not compile modern C# into a round-trip.

Other skills describe what to build. This skill defines how the designer partial must look. Property assignments belong in `InitializeComponent`.

## Designer baseline

- Every application Form, modal dialog, and UserControl is `partial` with `.Designer.cs` or `.Designer.vb`.
- The designer partial owns declarative instance `InitializeComponent` and the single generated `Dispose(bool)` (`if (disposing && (components != null))`).
- The parameterless constructor is the design-time path. It calls `InitializeComponent`. Safe lightweight event hookup may follow. It must not take services, touch IO, start async work, create GPU/WebView/map devices, or hide or restyle the static UI.
- Runtime constructors chain `: this()`, then assign services and data.
- Do not add a second `Dispose` override in the main partial. Extra cleanup uses the `Disposed` event or the existing designer override, never both.

Priority: Designer compatibility, then code quality, then DRY, then length. A 5,000–12,000 line `InitializeComponent` is normal. Split into UserControls only when a surface is too large to edit. Do not "clean up" generated code.

## Two contexts

| Context | Where | Rule |
|---|---|---|
| Designer | `*.Designer.cs` / `*.Designer.vb`, `InitializeComponent` | Declarative assignments the serializer can rewrite. C# 2.0 shape on .NET 10 and earlier. Do not modernize it. |
| Application | Main partial, services, view models | Language features the project actually enables. |

## Required shape

1. Create `components` before any `new X(components)`.
2. Instantiate the remaining controls and components.
3. `BeginInit` / `SuspendLayout`, including nested containers that the generator suspends.
4. Assign child properties. Wire named handlers only (`Click += SaveClick` or VB `Handles`). No lambdas, anonymous methods, or local functions.
5. Configure the Form or UserControl last, including `AutoScaleMode`.
6. Matching `EndInit` / `ResumeLayout` / `PerformLayout`.
7. Backing fields at the end of the designer partial, after the methods (`WFO3002`). The `components` field may stay with the infrastructure at the top. VB fields are `Friend WithEvents`.

Forbidden inside `InitializeComponent`: `if`, loops, `switch`, `try`, `lock`, `await`, ternaries, `??`, `?.`, `nameof`, collection expressions, helper calls, and static resource factories. Factories survive a compile and disappear on the first Designer save.

The designer file may contain `InitializeComponent`, the generated `Dispose(bool)`, and explicit interface members the generator already owns. It must not contain constructor overloads, helper methods, or properties. A parameterless constructor lives in the main partial.

Absolute `Point` assignments are serialization residue. They are not a layout recipe. New layout uses [winforms-high-dpi-fluent-layout](../winforms-high-dpi-fluent-layout/SKILL.md).

## Where the rest lives

- Full contract, nullable style, resources, and round-trip checks: [references/designer-contract.md](references/designer-contract.md)
- Partial-pair skeleton: [references/serialization.md](references/serialization.md)
- Property attributes and design-time extensions: [references/property-metadata.md](references/property-metadata.md)

## Validation

1. Every new Form, dialog, and UserControl has both partials and a parameterless constructor.
2. The design-time path constructs with no services, IO, async, GPU, or hidden chrome.
3. One `Dispose(bool)` disposes `components` with the generated null check.
4. Save, close, and reopen in the Designer when it is available. Compilation does not prove a round-trip.
