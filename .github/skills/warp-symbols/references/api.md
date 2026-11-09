<!-- Moved from the former warptoolkit-symbols skill; verified against WARP source. Versions: ../../warp-api-decision-guide/references/versions.md -->


# warp-symbols — detailed API reference

Scope: `public static partial class FluentSymbols` (namespace `WarpToolkit.WinForms.Symbols`, packages `WarpToolkit.WinForms` + `WarpToolkit.WinForms.Extensions`); nested glyph enums decorated with `[SourceFontName("Segoe Fluent Icons")]`. Usage-focused companion: `warp-toolstrip-icons`.

## The glyph enums

All are nested under `FluentSymbols` (reference them as `FluentSymbols.<Enum>`):

| Enum | Purpose | Sample members (value = Unicode code point) |
|------|---------|---------------------------------------------|
| `FluentSymbols.AllSymbols` | The full Segoe Fluent Icons set | `GlobalNavButton = 0xE700`, `Wifi = 0xE701`, `Copy`, `SaveAs`, `Stopwatch` |
| `FluentSymbols.CommonToolStripSymbols` | Curated command-bar icons | `Send = 0xE724`, `Accept = 0xE8FB`, `Cancel = 0xE711`, `New = AddBold`, `Open = OpenFile`, `Edit = 0xE70F` |
| `FluentSymbols.DevelopmentSymbols` | Dev-tool icons | `Code = 0xE943`, `Component = 0xE950`, `DeveloperTools = 0xEC7A`, `CommandPrompt = 0xE756` |
| `FluentSymbols.TreeViewSymbols` | Tree/expander/folder icons | `ChevronDown = 0xE70D`, `ChevronRight = 0xE76C`, `Folder = 0xE8B7`, `FolderOpen = 0xE838` |

Each enum value is the glyph's Unicode code point in the source font. Some
members alias others (e.g. `New = AddBold`, `Open = OpenFile`).

## Rendering glyphs onto ToolStrip items

Canonical (0.9.852+): symbol hosts and items in `WarpToolkit.WinForms.ToolStrips`
(`SymbolToolStrip`/`SymbolMenuStrip`/`SymbolStatusStrip`; `SymbolToolStripButton`,
`SymbolToolStripMenuItem`, `SymbolStatusStripLabel` with `ISymbol? Symbol`) —
items re-render on font/DPI/theme/owner changes and own their generated bitmap.
Recipes: `warp-toolstrip-icons`.

Legacy one-off helper: the **`ConfigureItem` extension** in
`WarpToolkit.WinForms.Extensions` (namespace
`WarpToolkit.WinForms.Extensions.UI`) — not in this package. Signature:

```csharp
public static void ConfigureItem(
    this ToolStripItem toolStripItem,
    Enum symbol,
    (EventHandler clickHandler, bool removeBeforeAdd)? eventHandler = null,
    string? tooltipText = default,
    int size = 0,
    Color? foreColor = null,
    Color? backColor = null);
// 0.9.852 adds an overload with an explicit `ToolStrip owner` second parameter.
```

It accepts **any** of the glyph enums (parameter type is `Enum`). The item's
`Owner` must be set first (or use the owner overload), so call it **after** the
item is added to a `ToolStrip`/menu.

```csharp
using WarpToolkit.WinForms.Extensions.UI;
using WarpToolkit.WinForms.Symbols;

// 1) Add the item to its owner first.
_strip.Items.Add(_copyButton);

// 2) Then configure the glyph (Owner is now set).
_copyButton.ConfigureItem(
    symbol: FluentSymbols.AllSymbols.Copy,
    tooltipText: "Copy selection");

_saveButton.ConfigureItem(
    symbol: FluentSymbols.CommonToolStripSymbols.Save,
    eventHandler: (OnSaveClick, removeBeforeAdd: true),
    tooltipText: "Save (Ctrl+S)",
    size: 20);
```

To show both glyph and label, set the item's `DisplayStyle` to
`ImageAndText` and `TextImageRelation` (e.g. `ImageAboveText`) after configuring,
as the GitScanner console tab does.

## DarkMode & High-DPI notes

- Font glyphs re-render at any size without bitmap blur — ideal for Per-Monitor-V2
  High-DPI. With symbol hosts, pick `SymbolScalingMode` (`FontSizeSquare`,
  `FontSizePreserveAspect`, `ImageScalingSize`); with `ConfigureItem` the bitmap is
  rendered once at `size`/`ImageScalingSize.Width` and must be re-run after a
  font or DPI change (nothing refreshes it).
- Glyph color follows the ToolStrip's `ForeColor` (or the `foreColor` argument),
  so icons track Light/Dark theme automatically. Don't bake a fixed color unless
  you need a specific accent.
- Size icons from the settled font/DPI baseline once (measured text height of the
  effective font, or logical point preference × DeviceDpi/96) — never from
  `Font.SizeInPoints × DeviceDpi/96` after a monitor change, because WinForms
  Per-Monitor-V2 already returns a DPI-scaled `Font` object.

## Common gotchas

- **The enums live here, the renderer lives in
  `WarpToolkit.WinForms.Extensions`.** Reference both packages: `FluentSymbols.*`
  from `WarpToolkit.WinForms.Symbols`, `ConfigureItem` from
  `WarpToolkit.WinForms.Extensions.UI`.
- `ConfigureItem` **throws if `toolStripItem.Owner` is null** — add the item to
  its strip/menu before configuring it.
- These are vector glyphs from "Segoe Fluent Icons"; it ships with Windows 11
  and is not guaranteed on Windows 10. "Segoe MDL2 Assets"/"Segoe UI Symbol" are
  different PUA tables: use a separate `[SourceFontName]` enum per font, never
  the same enum with a swapped font name.
- `CommonToolStripSymbols` contains aliases (`New`, `Open`, …) that point at
  other members — that's intentional, not a duplicate.
