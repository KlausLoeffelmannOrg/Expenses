---
name: warp-winforms-controls
description: >-
  Thin router (compatibility) for the WarpToolkit.WinForms modern-controls family.
  Use only to find the owning skill for FluentTabControl, FluentMessageBox,
  wizards, input controls, containers/layout, data grids, symbols or console panes;
  the detailed API lives in the routed skills.
metadata:
  source: "WARP src\\WarpToolkit.WinForms"
  versions: "../warp-api-decision-guide/references/versions.md"
---

# WARP WinForms Controls (router)

This skill carries no API samples. Earlier versions documented members that do
not exist (`WizardPage.Completed`, `GenerationStrategy.TimestampSuffix`,
`LayoutOrientation.Grid`, `FluentMessageBox` owner overloads, `addHandler:`
tuples); treat any copy of them as stale.

Decide surfaces first (`warp-api-decision-guide`, Workflow 0): every Form, modal
dialog and UserControl is a Designer pair with a parameterless ctor. Only then
pick the host control below.

| Need | Load |
|---|---|
| Tabbed navigation, tab views as UserControls | `warp-fluent-tab-control` |
| Message boxes, toasts, notifications | `warp-messaging` |
| Multi-step wizard, page containers | `warp-wizard` |
| Masked / typed / spin inputs, pickers | `warp-input-controls` |
| Panels, adornments, grouping, layout rules | `warp-containers-layout` |
| Grids, cell templates, column binding | `warp-data-grids` |
| Glyphs, Segoe Fluent symbols, symbol ToolStrips (icons required on command strips), Tree/List/app icons | `warp-symbols`, `warp-toolstrip-icons` |
| In-app console / diagnostics pane | `warp-console-diagnostics` |
| Auto-generated edit views from models | `warp-binding-autoview` |
| Form shape, DI, startup | `warp-winforms-application-builder` |

Shared layout rules come from `winforms-high-dpi-fluent-layout` (rules and the
`#canonical-modal-footer` reference, not its older one-file Form samples).

## Final checks

1. Loaded exactly one owning skill from the table; did not use this file as an API source.
2. Package versions come from the project / `versions.md`, never guessed.
