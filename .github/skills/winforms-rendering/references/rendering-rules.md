# Rendering rules

Companion to [../SKILL.md](../SKILL.md). These types are custom `Control` classes, not Forms. A designed UserControl still uses the designer partial for cleanup.

## Styles

`DoubleBuffered = true` is enough for many controls. Owner-draw also sets `UserPaint`, `AllPaintingInWmPaint`, `OptimizedDoubleBuffer`, and usually `ResizeRedraw`. Call `Invalidate`, not `Refresh`, so paints coalesce.

## Disposal

| Object | Dispose? |
|---|---|
| `PaintEventArgs.Graphics` | No |
| `SystemBrushes`, `SystemPens` | No |
| `Pen`, `Brush`, `Font`, `Image`, `Region`, `GraphicsPath`, `StringFormat` you created | Yes |
| `Graphics` from `CreateGraphics`, `FromImage`, or `FromHwnd` | Yes |
| `BufferedGraphics` you allocated | Yes |

`CreateGraphics` is a last resort outside paint. It is not a design-time activity. Manual `BufferedGraphics` is for a composition you must cache. Dispose it when the size changes and in the single cleanup path. Never call `Invalidate` from `OnPaint`.

## Quality modes

Set them at the start of the paint that needs them.

| API | Typical value |
|---|---|
| `SmoothingMode` | `None` for axis-aligned 1-pixel lines. `AntiAlias` for curves. `HighQuality` only when the cost is acceptable |
| `TextRenderingHint` | `ClearTypeGridFit` under about 12 pt. `AntiAlias` for large display type. `SystemDefault` to match the shell |
| `InterpolationMode` | `NearestNeighbor` for pixel art. `HighQualityBicubic` for photo downscale. `Bilinear` otherwise |
| `CompositingMode` | `SourceOver` for alpha. `SourceCopy` when blending is unnecessary |
| `CompositingQuality` | `HighQuality` only for the blend that needs it |

## Text

`TextRenderer.MeasureText` pairs with `TextRenderer.DrawText`. `Graphics.MeasureString` pairs with `DrawString`. Generic typographic format reduces overhang padding for monospace and mixed runs. Default `MeasureString` padding is not automatically wrong, but mixing it with a tighter draw accumulates error.

`Font.GetHeight(g)` is the line box. Extra leading for large fonts is a product choice, not a framework constant. Do not invent a second spacing rule in the designer file.

## DPI and theme

Recreate cached bitmaps and wide pens when `DeviceDpi` or the font changes. Size icon, menu, status, tree, and list images from a stable logical baseline. Do not multiply `Font.SizeInPoints` by `DeviceDpi / 96`: under Per-Monitor V2 that point size is already scaled. Use a stored logical size, or `Font.Height` / `TextRenderer` with the live font. Never `Font.GetHeight(DeviceDpi)`. Do not multiply the last bitmap's size again. Do not cache a `Graphics` from a previous handle. Read `Application.IsDarkModeEnabled` when painting, or handle the theme change and invalidate. Do not store a design-time color snapshot as if it were live.

## Design-time

`CreateParams` and `OnPaint` may run in the Designer. They must tolerate an unready handle and must not open devices, files, or networks. GPU and DirectComposition belong to the COM skill and stay off this path until the control is created at runtime and not in design mode.
