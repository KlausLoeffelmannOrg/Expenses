<!-- Moved from the former warptoolkit-data-grids skill; verified against WARP source. Versions: ../../warp-api-decision-guide/references/versions.md -->


# warp-data-grids — detailed API reference

Scope: `GridView` (`WarpToolkit.WinForms`, namespace `WarpToolkit.WinForms.Grid`, single template-painted column) and `WarpDataGridView` (`WarpToolkit.WinForms.Specialized`, classic columns + SystemColors theming); both derive from `DataGridView`.

## WarpDataGridView

`WarpDataGridView : DataGridView` — namespace `WarpToolkit.WinForms.Specialized`.
A `DataGridView` that derives all cell/header colors from `SystemColors` so it
follows the OS Light/Dark theme. The constructor sets `DoubleBuffered = true`,
`EnableHeadersVisualStyles = false`, and calls `ApplySystemColors()`. It hooks
`SystemEvents.UserPreferenceChanged` (on handle create) to re-theme live.

| Member | Type / default |
|--------|----------------|
| `AlternatingRowColorMode` | `AlternatingRowColorMode` (default `Auto`) |

`AlternatingRowColorMode` enum:
- `Auto` — derive the alternating-row color from `SystemColors.Window` with a
  lightness shift; re-applied on theme change.
- `Off` — leave alternating rows untouched (no banding).
- `Custom` — leave `AlternatingRowsDefaultCellStyle` untouched so you can set it.

```csharp
// Designer file — flat configuration:
_grid = new WarpDataGridView();
_grid.Dock = DockStyle.Fill;
_grid.AlternatingRowColorMode = AlternatingRowColorMode.Auto;
Controls.Add(_grid);

// Regular code — standard DataGridView binding:
_grid.AutoGenerateColumns = true;
_grid.DataSource = _bindingSource;
```

A common pattern (see GitScanner) is to derive a strongly-typed grid:

```csharp
public abstract class WarpRowGridView<TRow> : WarpDataGridView
    where TRow : class
{
    // wraps BindingList<TRow> + BindingSource, defines columns, etc.
}
```

## GridView

`GridView : DataGridView` — namespace `WarpToolkit.WinForms.Grid`. A virtual-mode
grid with a **single** template-painted column: each row is drawn by a
`GridViewItemTemplate`. The constructor sets `VirtualMode = true` and creates one
internal `GridViewCell` column.

| Member | Signature |
|--------|-----------|
| `DataContext` | `new object? { get; set; }` (the bound list) |
| `SelectedItem` | `object? { get; set; }` |
| `GridViewItemTemplate` | `GridViewItemTemplate? { get; set; }` |
| `GridViewItemTemplateChanged` | `event EventHandler?` |
| `SelectedItemChanged` | `event EventHandler?` |

When you assign a template, the grid sets
`GridViewItemTemplate.IsDarkMode = Application.IsDarkModeEnabled`. `GridView` also
exposes `new`-shadowed `DataGridView` styling properties (e.g. `Columns`,
`DataSource`, `DefaultCellStyle`, `ColumnHeaders*`, `RowHeaders*`) via the
`GridView_ShadowingBaseProperties` partial.

### GridViewItemTemplate

`public abstract partial class GridViewItemTemplate : INotifyPropertyChanged`.
Subclass it to define how a single item paints. Public surface:

| Member | Signature |
|--------|-----------|
| `Padding` | `virtual Padding { get; set; }` |
| `ContentPadding` | `virtual Padding { get; set; }` |
| `LineSpacing` | `virtual int { get; set; }` |
| `ItemBackgroundColor` / `ItemForegroundColor` | `Color { get; }` (DarkMode-aware) |
| `HighlightFontColor` / `StandardFontColor` | `Color { get; }` |
| `HighlightedBackgroundColor` / `SelectedBackgroundColor` | `Color { get; }` |
| `*Brush` variants | `Brush { get; }` for the colors above |
| `SetProperty<T>` | `bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")` |
| `PropertyChanged` | `event PropertyChangedEventHandler?` |

The color/brush members switch on the protected `IsDarkMode` flag (set by the
host `GridView`), so a single template renders correctly in both themes.
Override the protected paint/measure members (`OnPaintContent`,
`GetPreferredSize`, `PaintBorder`, `PaintErrorIcon`) in your subclass.

Supporting public types:
- `GridViewItemTemplate.GridViewItemTemplateConverter : TypeConverter` — design-time conversion/standard-values for picking a template type.
- `GridViewItemTemplate.GridViewItemTemplateWrapper(Type itemTemplate)` — `Type ItemTemplate { get; }`, used by the converter's drop-down.
- `GridViewExtension` (static) — `Rectangle Pad(this Rectangle rec, Padding padding)` helper for paint code.

```csharp
public sealed class CustomerCardTemplate : GridViewItemTemplate
{
    protected override void OnPaintContent(/* paint args from base */)
    {
        // use ItemForegroundColorBrush / HighlightedBackgroundColorBrush etc.
    }
}

// Regular code:
_gridView.GridViewItemTemplate = new CustomerCardTemplate();
_gridView.DataContext = _customers;   // the bound list
_gridView.SelectedItemChanged += (s, e) => ShowDetails(_gridView.SelectedItem);
```

## DarkMode & High-DPI notes

- `WarpDataGridView` requires `EnableHeadersVisualStyles = false` (set in its
  ctor) so header colors come from `SystemColors`; it re-themes on
  `UserPreferenceChanged`. Don't re-enable visual styles or you lose dark headers.
- `GridView` propagates `Application.IsDarkModeEnabled` into the template's
  `IsDarkMode`; template colors follow automatically.
- Neither grid overrides `AutoScaleMode`; the Form is the scaling root
  (`AutoScaleMode.Font`), a hosted grid view is `AutoScaleMode.Inherit`. The
  grid row is the single Percent spring of its root TLP — do not add a second
  Percent row beside it ([resizable viewport](../../winforms-high-dpi-fluent-layout/references/layout-examples.md#resizable-viewport));
  re-apply saved column widths after the font settles.

## Common gotchas

- `GridView` is **virtual-mode, single-column, template-painted** — it is *not*
  a drop-in for arbitrary multi-column `DataGridView` layouts. For classic
  columns use `WarpDataGridView`.
- For banded rows on `WarpDataGridView`, leave `AlternatingRowColorMode = Auto`
  (theme-derived) or set it to `Custom` and assign
  `AlternatingRowsDefaultCellStyle` yourself; `Off` disables banding.
- `GridView.DataContext` is a **`new`** member (shadows the base); set the bound
  list there, not on a base reference.
