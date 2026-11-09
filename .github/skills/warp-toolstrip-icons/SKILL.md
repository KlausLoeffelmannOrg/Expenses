---
name: warp-toolstrip-icons
description: >-
  Gives WARP ToolStrip/MenuStrip/StatusStrip command items crisp, font- and
  DPI-tracking glyph icons through SymbolToolStrip hosts and FontSymbol items,
  with the ConfigureItem legacy path, image ownership, regeneration triggers and
  the icon-required policy for command strips.
metadata:
  source: "WARP src\\WarpToolkit.WinForms\\ToolStrips (SymbolToolStrip*, SymbolToolStripItemState, SymbolToolStripSupport); Symbols\\FontSymbol.cs; src\\WarpToolkit.WinForms.Extensions\\UI\\ToolStripExtensions.cs"
  versions: "../warp-api-decision-guide/references/versions.md"
  verified: "installed package 0.9.852-preview (WarpToolkit.WinForms, WarpToolkit.WinForms.Extensions) and source a586853b"
---

# WARP ToolStrip icons

> Designer pair rule: strips and their items are declared in `.Designer.*`; symbol
> assignment, `ConfigureItem` and command wiring run after `InitializeComponent`
> (`OnLoad`/`SetupCommands`), never inside it (`warp-winforms-application-builder`).
> New Designer fields go **after** methods in the Designer partial (WFO3002).

## Policy: command strips carry icons

- Every command `ToolStrip`/`MenuStrip`/`ContextMenuStrip` item and command
  `StatusStrip` button gets a glyph. Text-only is an **explicit, user-stated
  text-domain exemption** (e.g. a word-list or language toolbar whose items *are*
  text); never decide on your own that an ordinary command bar is text-only.
- Glyph plus caption (`DisplayStyle = ImageAndText`) is the default for primary
  toolbars; `Image` only for dense secondary bars, with `ToolTipText` and
  `AccessibleName` set because the symbol's accessibility text is a fallback only.
- Icons are generated from fonts/SVG at run time — no `.png`/`.ico` resources,
  no manual `ImageList` plumbing. A generic, non-WARP factory belongs to
  `winforms-toolstrip-icons`; load that skill instead when the project has no
  WARP package.

## Canonical path (0.9.852+): symbol hosts and symbol items

Package `WarpToolkit.WinForms`, namespace `WarpToolkit.WinForms.ToolStrips`:

| Host (Designer-placed) | Items | Policy members |
|---|---|---|
| `SymbolToolStrip : ToolStrip` | `SymbolToolStripButton`, `SymbolToolStripMenuItem` (drop-downs) | `SymbolScalingMode`, `new ImageScalingSize`, `AppId`, merge APIs, CarMode |
| `SymbolMenuStrip : MenuStrip` | `SymbolToolStripMenuItem` | `SymbolScalingMode`, `new ImageScalingSize` |
| `SymbolStatusStrip : StatusStrip` | `SymbolStatusStripLabel`, `SymbolStatusStripCommand` | `SymbolScalingMode`, `new ImageScalingSize` |

Items implement `ISymbolToolStripItem` (`ISymbol? Symbol`, `Guid CommandId`,
`Guid ParentCommandId`) and expose `RefreshSymbol()`. Plain `ToolStripButton`s
inside a symbol host get no symbol support — use the symbol item types.

`ToolStripSymbolScalingMode` (resolved from the nearest symbol host, through
nested drop-downs):

| Mode | Bitmap size |
|---|---|
| `Default` | font symbols at their host-aware natural size; others fit a square of the item font height |
| `ImageScalingSize` | fit the host `ImageScalingSize` (validated > 0) |
| `FontSizePreserveAspect` | item font height leads (width on vertical strips), aspect preserved |
| `FontSizeSquare` | item font height × item font height |

Prefer `FontSizeSquare` or `FontSizePreserveAspect` for font-driven apps: the
glyph canvas is then recomputed from the item's effective font at the strip's
`DeviceDpi`, so one Font/DPI baseline drives captions and icons together.

```csharp
// MainForm.cs — after InitializeComponent (OnLoad). _commands is a SymbolToolStrip,
// _tsbSave a SymbolToolStripButton, both declared in MainForm.Designer.cs.
private Font? _symbolFont;   // family carrier; disposed after the items (OnHandleDestroyed/Dispose)

private void SetupCommands()
{
    _commands.SymbolScalingMode = ToolStripSymbolScalingMode.FontSizeSquare;

    _symbolFont ??= new Font("Segoe Fluent Icons", 12f, GraphicsUnit.Pixel);
    _tsbSave.Symbol = new FontSymbol((char)FluentSymbols.CommonToolStripSymbols.Save, _symbolFont)
    {
        AccessibilityName = "Save",
    };
    _tsbSave.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
    _tsbSave.ToolTipText = "Save (Ctrl+S)";
}
```

`FontSymbol` facts (verified): default `SymbolScaling = ChildScaled`,
`SymbolScalingValue = 1` → the glyph renders at the **item font size** using the
`SymbolFont` *family*; the `SymbolFont` size only matters in `Parent` mode.
`FontSymbol` is not `IDisposable`; the consumer owns `SymbolFont`
(`FromFluentSymbol(enum, pixels)` allocates one you must track). Keep it alive
while any item references the symbol; dispose it after the items are disposed.

### Regeneration and ownership (no stale raster)

The item state re-renders through the shared `WinFormsSymbolCache.Shared` and
disposes its previous generated bitmap on every refresh. Automatic triggers:

- item `OnFontChanged`, `OnForeColorChanged`, `OnOwnerChanged`;
- owner `DpiChangedAfterParent`, owner `Layout` (when `ImageScalingSize` or
  orientation changed under the respective modes), `LayoutStyleChanged`;
- `SystemEvents.UserPreferenceChanged` (theme/high contrast; not in the Designer);
- host `SymbolScalingMode`/`ImageScalingSize` setters (`RefreshSymbols`).

Consequences: when the host Form runs a Font-change transaction
(`winforms-high-dpi-fluent-layout` → `references/window-state.md#font-change-transaction`),
strip items regenerate by themselves because the strip inherits the ambient font;
do not re-run `ConfigureItem` or rebuild `ImageList`s. If an item has an explicit
`Font`, set it inside the same transaction and call `RefreshSymbol()`.
`item.Image` assignments are preserved (never disposed) and restored when the
symbol loses content; `ImageScaling` is forced to `None` while a symbol renders.
Symbol accessibility name/description apply only where the host left
`AccessibleName`/`AccessibleDescription` unset.

Fonts: `Segoe Fluent Icons` ships with Windows 11 and is installed on Windows 10
only with optional updates/apps — it is **not guaranteed**. `Segoe MDL2 Assets`
(Windows 10+) shares many but not all PUA code points; `Segoe UI Symbol` is a
different set. Never flip the font name and keep the enum: probe the family with
`InstalledFontCollection` once at startup and pick a per-font enum map
(`[SourceFontName]` type per family), or ship a documented text fallback.
Unknown glyphs render tofu silently (`HasGlyph` only checks `'\0'`).

## TreeView / ListView / window icons

Namespace `WarpToolkit.WinForms.Symbols`: `SymbolTreeNode.Symbol` +
`treeView.ApplySymbols(iconSize, color)`, `listView.ApplySymbols(iconSize, color)`,
`fontSymbol.ToIcon(size, color, font)` (caller disposes), `form.SetApplicationIcon(...)`,
`notifyIcon.SetTrayIcon(...)`. These render **once** into an `ImageList`/`Icon` at
the pixel size you pass — nothing refreshes them. Recompute `iconSize` from the
settled font/DPI baseline — either a stored **logical** point preference ×
`DeviceDpi/96`, or the live font with no DPI factor
(`TextRenderer.MeasureText("Wg", font).Height`); never
`Font.SizeInPoints × DeviceDpi/96`, because after WM_DPICHANGED WinForms already
returns a DPI-scaled `Font` — and
call `ApplySymbols` again after the Font transaction and in `OnDpiChangedAfterParent`;
dispose the previous `Icon`. Do not scale an already-rendered bitmap a second time.

## Legacy path: `ConfigureItem` (plain `ToolStripItem`)

`WarpToolkit.WinForms.Extensions`, namespace `WarpToolkit.WinForms.Extensions.UI`:

```csharp
public static void ConfigureItem(this ToolStripItem item, Enum symbol,
    (EventHandler clickHandler, bool removeBeforeAdd)? eventHandler = null,
    string? tooltipText = default, int size = 0, Color? foreColor = null, Color? backColor = null);
// overload: ConfigureItem(this ToolStripItem item, ToolStrip owner, Enum symbol, …)
public static Image GetSymbolImage(this ToolStrip strip, Enum symbol, int size = 0, Color? foreColor = null, Color? backColor = null);
public static Icon  GetSymbolIcon(this ToolStrip strip, Enum symbol, int size = 0);
```

- Renders a **one-off bitmap** at `size` (0 → owner `ImageScalingSize.Width`,
  glyph font = 80 % of that) with the owner's DPI; nothing re-renders it on
  font/DPI/theme change. You own the re-run: call `ConfigureItem` again (with
  `removeBeforeAdd: true`) after the Font transaction and after
  `DpiChangedAfterParent`, and dispose the replaced `Image` yourself.
- Requires `Owner` (add to the strip first) unless you pass the explicit
  `owner` overload. Throws `InvalidOperationException` when the enum type lacks
  `[SourceFontName]`.
- `foreColor` defaults to the owner's `ForeColor`; `backColor` defaults to
  transparent in `ConfigureItem` (owner `BackColor` in `GetSymbolImage`).

Use it only for existing code or hosts that cannot be swapped to the symbol
strips; new command bars use the canonical path above.

## Anti-patterns

- `ConfigureItem`/`Symbol =` inside `InitializeComponent`.
- Rebuilding icons on every `FontChanged`/`Layout` event by hand while symbol
  items already refresh — double work and flicker.
- Scaling a cached raster for a new DPI instead of re-rendering from the font.
- Treating `Segoe Fluent Icons`/`MDL2`/`UI Symbol` as interchangeable PUA tables.
- Deciding "text-only toolbar" without an explicit user text-domain intent.
- Disposing `SymbolFont` while items still reference the `FontSymbol`.

## Hand-off

- Symbol model, caching, renderer registration: `warp-symbols`.
- Non-WARP glyph factory, ImageList-based strips: `winforms-toolstrip-icons`.
- Font/DPI transaction and baseline sizing: `winforms-high-dpi-fluent-layout`.

## Final checks

1. Each command item has a symbol (or a recorded user text-domain exemption).
2. Symbol host modes are font-based or `ImageScalingSize` is set deliberately; no second scaling of rendered bitmaps.
3. Symbol fonts outlive their items; one-off `ConfigureItem`/`ApplySymbols` renders have an explicit re-run point.
