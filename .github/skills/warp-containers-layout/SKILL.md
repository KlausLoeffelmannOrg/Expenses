---
name: warp-containers-layout
description: >-
  Configures WARP adorner borders, validation flashes and per-cell control styles,
  or adds transparent/click-through overlays. Diagnoses shadowed panel properties,
  automatic cell adjustments and window-handle recreation.
metadata:
  source: "WARP src\\WarpToolkit.WinForms\\Containers"
---

# Adorners and overlays

## Critical constraints

- **Shared field/group/modal layout belongs to `winforms-high-dpi-fluent-layout`.**
  Apply its rules by reference — the layout SKILL's TLP/label/editor rules plus
  [canonical modal footer](../winforms-high-dpi-fluent-layout/references/layout-examples.md#canonical-modal-footer)
  and [resizable viewport](../winforms-high-dpi-fluent-layout/references/layout-examples.md#resizable-viewport)
  — not its older one-file Form samples; adornment does not replace them.
- Every Form/dialog/UserControl hosting these containers is a Designer pair with
  a parameterless ctor (`warp-winforms-application-builder`).
- Designer placement belongs to `winforms-designer-code`; runtime event/style
  customization belongs outside InitializeComponent.
- AdornerPanel BorderStyle and Padding shadow Panel members; use the concrete type.
- TransparentPanel.TransparencyStyle changes recreate its handle. PassThrough must
  remain enum value zero because CreateParams can run before field initializers.

## Layout contract WARP containers must respect

Summary of the WinForms rules of record (do not re-derive; follow the anchors):

- **Command footer** — never a `FlowLayoutPanel` (FlowLayout stays for flowing
  content). Nested TLP: outer 2 cols (100% + AutoSize) × 2 AutoSize rows, outer
  and inner `AutoSize` + `GrowAndShrink`; empty spacer Label row0/col1
  (AutoSize, Dock Fill, Margin top/bottom 5); inner TLP row1/col1 Dock Fill,
  one AutoSize row, N equal-percent columns; buttons Dock Fill, AutoSize,
  GrowAndShrink; captions >30 chars wrap deliberately. Vertical command stacks
  transpose the same schema. Outer footer spans all root columns in the
  root's AutoSize footer row.
- **Root rows** — autosized dialog: all rows AutoSize. Resizable root: exactly
  one Percent spring row before the footer. If a content viewport (grid, tab
  host, AdornerTableLayoutPanel) already owns the Percent row, that row *is*
  the spring — do not add a second Percent100 beside it (it halves the
  viewport); contain extra content in a nested region or cap the spring with
  a deliberate percentage.
- **GroupBox chain** — GroupBox Dock Fill, AutoSize, GrowAndShrink with a sole
  inner TLP configured the same; `AdornerPanel`/`AdornerTableLayoutPanel`
  follow the same preferred-size chain.
- **Scaling root** — the Form owns `AutoScaleMode.Font`; adorned regions inside
  hosted UserControls are `AutoScaleMode.Inherit` with no own `Font`.
- **Adaptive input minima** — no reflection; during font-settled layout
  pattern-match `TextBox`/`NumericUpDown`/`ComboBox`/`MaskedTextBox` recursively
  and set `MinimumSize` from expected character capacity + chrome measured on
  the settled font, not from the current (growing) size
  (`warp-input-controls`).

## AdornerPanel

WarpToolkit.WinForms.Containers.Adorners:
`AdornerPanel : Panel, ILayoutDefinitionProvider`.
OptimizedDoubleBuffer/ResizeRedraw enabled.

| Member | Contract/default |
|---|---|
| LayoutDefinition | string, e.g. `"Auto, *, Auto"` |
| ControlPadding | int, 10 |
| BorderStyle | new AdornerBorderStyle, Modern |
| BorderColor / BorderThickness | Color / int, thickness 1 |
| Orientation | System.Windows.Forms.Orientation, not LayoutOrientation.Grid |
| Padding | new Padding |
| ToolStripVisibilitySize / ToolStrip | int; >15 materializes optional ToolStrip; nullable getter |
| ErrorSignalingDuration | int |
| SignalError / SignalErrorAsync / ResetSignal | flash/clear validation border |

Changed events: LayoutDefinition, ControlPadding, BorderStyle, BorderThickness,
Orientation, ErrorSignalingDuration and ToolStripVisibilitySize.
AdornerBorderStyle values None=0, Classic=1, Modern=2.

```csharp
adorner.BorderStyle = AdornerBorderStyle.Modern;
adorner.BorderThickness = 1;
adorner.ControlPadding = 8;
await adorner.SignalErrorAsync();
```

## AdornerTableLayoutPanel and styles

`AdornerTableLayoutPanel : TableLayoutPanel, ISupportInitialize` paints cell borders
and can adjust hosted controls using AutoApplyControlAdjustments.
DefaultStyle plus type defaults: DefaultTextBoxStyle, DefaultMultilineTextBoxStyle,
DefaultLabelStyle, DefaultAutoSizeLabelStyle, DefaultComboBoxStyle,
DefaultCheckBoxStyle, DefaultButtonStyle, DefaultPictureBoxStyle.
BeginInit/EndInit support initialization batching.

CreateDefaultStyleInstance receives Control and writable nullable Style. Replace
the selected style in that event for per-control adjustments, while preserving
the shared layout's anchor/dock/autosize contracts.

AdornerCellStyle is a mutable record with constructor:
`(AutoPropertyApplySettings PropertyApplySettings, AdornerBorderStyle CellBorderStyle,
AnchorStyles CellContentAnchoring, Padding? AdornerCellPadding = default,
Padding? AdornerCellMargin = default, Color CellBorderColor = default)`.
Mutable properties additionally include AdornerLineWidth (2),
AutoApplyControlProperties, CellBorderStyle/Color/ContentAnchoring, margin/padding.
Default border color is Blue in dark mode, DarkBlue in light.

AutoPropertyApplySettings flags:
None=0, Margin=1, Anchoring=2, AutoSize=4, Borderstyle=8, RenderStyle=16.
Do not let automatic adjustments silently override canonical single-line anchors.
The panel enables OptimizedDoubleBuffer/AllPaintingInWmPaint/UserPaint/ResizeRedraw;
keep heavy work out of paint callbacks.

The shared GridLayouting vocabulary also includes LayoutSizeType.Star/Absolute
and LayoutAlignment.Start/Center/End/Fill; inspect the applicable API instead of
assigning those unrelated types to AdornerPanel.Orientation.

## TransparentPanel

Package WarpToolkit.WinForms; unusual namespace **WinForms.PowerTools.Controls**.
TransparentPanel derives Control, enables Opaque style and paints nothing over its
parent. TransparencyStyle: PassThrough=0, SneezeGuard=1, Interactive=2.
CreateParams removes WS_CLIPCHILDREN/SIBLINGS, adds WS_EX_TRANSPARENT unless
Interactive and WS_DISABLED for SneezeGuard/PassThrough. Do not infer arbitrary
alpha composition or use handle recreation on an animation hot path.

## Conditional references and final checks

Read `warp-fluent-tab-control` for tabs (their API/layout is not owned here),
`winforms-rendering` for custom paint/theming and
`winforms-custom-and-usercontrols` when authoring a new container.

## Final checks

1. Automatic cell adjustment preserves the canonical layout contract (footer,
   single spring row, GroupBox chain, `Inherit` hosted views).
2. Shadowed members and transparency style are accessed through correct types.
3. Validation/overlay lifetime and runtime repaint/handle effects are explicit.

## References

- Detailed member tables and samples: [references/api.md](references/api.md)
- Package versions: [versions.md](../warp-api-decision-guide/references/versions.md) (project pin wins; never guess "latest")
- Compatibility alias: `warptoolkit-containers-layout` routes here; do not load both.
