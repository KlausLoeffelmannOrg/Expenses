---
name: winforms-rendering
description: >-
  USE FOR: WinForms GDI and GDI+ painting, text measurement, double buffering,
  flicker, DPI-aware strokes, and dark-mode colors. DO NOT USE FOR: layout
  containers, Designer serialization, or creating GPU, Direct2D, or WebView
  devices. Those stay off the design-time path.
---

# WinForms rendering

Paint in `OnPaint`. Match the measure API to the draw API. Do not create a device, swap chain, or `CreateGraphics` surface while the Designer is constructing the control.

## Designer baseline

- A designed Form or UserControl follows [winforms-designer-code](../winforms-designer-code/SKILL.md).
- A non-composite `Control` may be one file with a parameterless constructor. `SetStyle` and `DoubleBuffered` in that constructor are design-safe. File, network, and GPU work are not.
- A designed UserControl has one `Dispose(bool)`, in the designer partial. Dispose cached pens there or on `Disposed`. Do not add a second override.
- Layout of the host belongs to [winforms-high-dpi-fluent-layout](../winforms-high-dpi-fluent-layout/SKILL.md).

## Rules that change the answer

- `TextRenderer` matches system UI text. `Graphics.DrawString` is for transformed, alpha, or mixed GDI+ drawing. Do not measure with one and draw with the other.
- Precise `MeasureString` uses the same `Graphics`, the same format (`StringFormat.GenericTypographic` when padding would accumulate), and the same quality modes as the draw.
- A pen stroke is centered on the coordinates. Inset by half the width to stay inside the bounds.
- Never dispose `e.Graphics`, `SystemBrushes`, `SystemPens`, or `SystemColors`.
- Dispose pens, brushes, fonts, images, and `Graphics` objects you create. Cache stable ones as fields. Do not allocate them on every paint.
- `DoubleBuffered = true` is the default flicker fix. `OptimizedDoubleBuffer | AllPaintingInWmPaint | UserPaint` is the owner-draw set. `ResizeRedraw` prevents smears.
- Never call `Invalidate` or `Refresh` from `OnPaint`.
- Crisp 1-pixel lines use `SmoothingMode.None` and integer coordinates. Anti-aliased strokes use `AntiAlias` and half-pixel offsets.
- Small UI text prefers `TextRenderingHint.ClearTypeGridFit` or `SystemDefault`. Large display text may use `AntiAlias`.
- Scale stroke width with `DeviceDpi`. Rebuild menu, status, tree, list, and toolbar bitmaps from a stable 16px at 10pt at 96 DPI baseline, once. After a Per-Monitor V2 move, `Font.SizeInPoints` is already DPI-scaled. Do not multiply it by `DeviceDpi / 96`. Use a stored logical point size, or measure the live font with no extra DPI factor. Never `Font.GetHeight(DeviceDpi)`. Do not stretch the previous bitmap. See [toolstrip icons](../winforms-toolstrip-icons/SKILL.md).
- Dark mode: prefer `SystemColors`. Named `ControlDark` is not unconditionally dark. Fixed palettes switch on `Application.IsDarkModeEnabled` at runtime, not in `InitializeComponent`.
- High Contrast must stay usable. Do not replace system colors with a fixed palette there.

Quality-mode names, offscreen buffers, and cache disposal: [references/rendering-rules.md](references/rendering-rules.md). Matched measurement samples: [references/painting-examples.md](references/painting-examples.md).

## Validation

1. Scroll and resize without flicker or leftover pixels.
2. Measure and draw use the same API and format.
3. The Designer constructs the control without a device or file.
4. Cached GDI objects are disposed on the single cleanup path.
