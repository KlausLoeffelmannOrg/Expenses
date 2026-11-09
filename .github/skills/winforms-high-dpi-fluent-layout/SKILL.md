---
name: winforms-high-dpi-fluent-layout
description: >-
  USE FOR: responsive WinForms Form and UserControl layout, TableLayoutPanel,
  Per-Monitor V2, the nested command-button footer, first-run and persisted
  window geometry, and font-change bounds. DO NOT USE FOR: Designer
  serialization syntax, binding, or choosing a WARP tab host. FlowLayoutPanel
  is not for command buttons. Pair with winforms-designer-code.
---

# WinForms high-DPI layout

Decide the surface and its containers first. Then assign properties in `InitializeComponent`. Do not extract `CreateDetailsPanel` or `SetupLayout` helpers. Absolute `Point` leftovers are not a layout recipe.

## Designer baseline

- Every application Form, modal dialog, and UserControl is `partial` with `.Designer.cs` or `.Designer.vb`.
- The designer partial owns declarative instance `InitializeComponent` and the single generated `Dispose(bool)`.
- The parameterless constructor calls `InitializeComponent`. It may hook lightweight handlers. It must not take services, touch IO, start async work, create devices, or hide or restyle the static UI. Fullscreen and restored bounds run later, after design-time construction.
- Runtime constructors chain `: this()`.
- Serialization rules: [winforms-designer-code](../winforms-designer-code/SKILL.md).

## Layout rules that change the design

- Design new surfaces at 96 DPI. Set `AutoScaleMode = Font` on new forms. Leave an existing form's `AutoScaleMode` alone.
- Prefer nested `TableLayoutPanel` regions over one mega-grid. Two to four columns per grid.
- Size priority is a design preference, not the allocator's order: AutoSize, then Percent, then Absolute.
- Caption columns are AutoSize. Value columns are Percent. Absolute is for genuinely fixed content.
- Real content rows are AutoSize. A content-sized modal has no spring row. A resizable root has one Percent 100 row immediately before the footer. If a viewport already owns that row, do not add a second 100% row.
- Anchor preferred-size labels. Dock a value editor, an equal-size dialog button, or a nested fill container on purpose. Do not dock every child.
- A GroupBox in a cell is `Dock = Fill`, `AutoSize`, `GrowAndShrink`, and contains only an inner table with the same settings. Other nested tables use that AutoSize chain too.
- `Padding` does not space `TableLayoutPanel` cells. Set `Margin` on the children (at least 3).
- Radio groups are a single-column AutoSize grid inside an AutoSize GroupBox.
- Large pages scroll inside the content UserControl. Actions stay outside that viewport.
- Hosted tab, wizard, and complex-form UserControls use `AutoScaleMode.Inherit`. The host owns scaling. Do not set their `Font`. A top-level window or intentional scaling-root custom control is the exception.
- Empty `TextBox`, `NumericUpDown`, `ComboBox`, and `MaskedTextBox` controls keep a font-settled minimum width. The helper matches types directly. It does not use reflection.
- A runtime font change must not grow top-level outer `Bounds`. Restore `MinimumSize` and `MaximumSize` too, or the scaled minimum forces the window larger. `SuspendLayout` does not stop AutoScale. Honor the OS DPI suggested rect. Logical font points, the already-scaled observed `Font`, and coordinate DIP conversion are different. Details: [references/window-state.md](references/window-state.md#dip-schema).

## Canonical modal footer

The contract and the partial-pair sample live in [references/layout-examples.md](references/layout-examples.md#canonical-modal-footer). Rules:

- Non-resizable dialog: content-sized root, `Dock = Top` into an `AutoScroll` form. Do not `Dock = Fill` the root and do not treat `ClientSize` as a height cap.
- Command buttons are not placed in a `FlowLayoutPanel`. The footer is an outer 2x2 AutoSize table plus an inner equal-percent button table. Both are `GrowAndShrink`. The outer panel spans the top-level row. Captions longer than 30 characters get a strategic `\n`.
- `Button.AutoSizeMode` defaults to `GrowOnly`. The footer sets `GrowAndShrink`.
- Cap the outer window to the current monitor working area at runtime. Keep overflow reachable with `AutoScroll`. Do not apply the 75/80 main-window ratios to a small modal.

WARP skills that say "canonical modal footer" mean that heading. Do not copy older one-file Form samples or the retired single-grid footer.

## Where the rest lives

- Columns, GroupBox chain, spring-row exception, editor minima: [references/layout-rules.md](references/layout-rules.md)
- Footer and resizable-viewport samples: [references/layout-examples.md](references/layout-examples.md)
- First-run geometry, DIP schema, font transaction, DPI bounds: [references/window-state.md](references/window-state.md)

A non-MVVM modal may assign `Form.DataContext` as a draft on .NET 8+. That is [databinding](../winforms-databinding/SKILL.md) and [options](../winforms-options-dialog/SKILL.md), not a request for [MVVM](../winforms-mvvm/SKILL.md).

## Validation

1. New forms and dialogs are partial pairs with a parameterless constructor. Designer fields follow the methods.
2. Resize, larger font, and DPI do not clip captions, editors, or the pinned footer. A font change does not grow top-level outer bounds. Overflow scrolls.
3. Fullscreen and bounds restore are not invoked from the design-time constructor. Saved geometry is monitor-local DIPs, not absolute pixels treated as DIPs.
4. Designer save/reopen is a separate check from the build. A posted `WM_DPICHANGED` is not a per-monitor DPI test.
