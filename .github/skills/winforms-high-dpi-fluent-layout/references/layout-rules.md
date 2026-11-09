# Layout rules

Companion to [../SKILL.md](../SKILL.md). Samples live in [layout-examples.md](layout-examples.md).

## Column and row modes

| Mode | Use |
|---|---|
| AutoSize | Captions, prompts, single-line content, the footer row |
| Percent | One spare-height spring, or spare width |
| Absolute | Fixed glyphs or a measured control that must not grow. Avoid for text |

Anchor labels that should keep preferred size. Dock Fill is for a multiline value, an equal-size dialog button, or a nested container that must use the cell. Docking the root grid to a resizable form is valid. Docking the root grid of a content-sized dialog is not. Use `Dock = Top` there.

`Padding` on a `TableLayoutPanel` does not create cell gaps. Child `Margin` does (at least 3). Nested tables that are not the footer schema still use `AutoSize` and `GrowAndShrink`, or preferred size never reaches the root.

## GroupBox

In a table cell the GroupBox is `Dock = Fill`, `AutoSize`, `GrowAndShrink`, and has one child: an inner `TableLayoutPanel` with those same settings, also `Dock = Fill`. Controls live in that inner table, not in the GroupBox.

## Command buttons

Do not place command buttons in a `FlowLayoutPanel`. Flow layout may wrap ordinary content. It does not make equal button widths.

Canonical horizontal footer. Both panels are `AutoSize` and `GrowAndShrink`:

- Inner: one AutoSize row, N Percent columns at `100F / N`. Each button is `Dock = Fill`, `AutoSize = true`, `AutoSizeMode = GrowAndShrink`. `Button.AutoSizeMode` defaults to `GrowOnly`. Set `GrowAndShrink`.
- Outer: two AutoSize rows, columns Percent 100 and AutoSize. An empty AutoSize `Label`, `Dock = Fill`, `Margin = new Padding(0, 5, 0, 5)`, is at row 0 column 1. The inner panel is `Dock = Fill` at row 1 column 1.
- The outer panel sits in a new AutoSize row of the top-level table, column 0, spanning every column.

Captions longer than 30 characters get a strategic `\n` in the designer `Text`. Do not rely on wrapping. The old threshold of about 20 characters is retired.

Vertical top-aligned buttons keep the nesting. Transpose the inner panel to one Percent column and N AutoSize rows. Put that stack in the outer AutoSize cell at the top. The outer Percent row or column takes spare space below or beside the stack. Move the spacer's 5px margin to the outer edge.

`AcceptButton`, `CancelButton`, and `DialogResult` are declarative designer properties for an uncomplicated accept. Do not cancel every focus change. Validation ownership is [databinding](../../winforms-databinding/SKILL.md).

Equal percent columns can differ by a couple of pixels. That is acceptable. Do not set per-button `MaximumSize` on this footer. It fights `GrowAndShrink` and wrapped captions.

## Resizable spring

A resizable root has one Percent 100 row immediately before the AutoSize footer.

- Bounded inputs: AutoSize content rows, then an empty Percent 100 spring, then the footer. Actions stay reachable and captions do not stretch.
- A grid, tab host, or other viewport that must consume spare height is that single Percent row. Do not add a second Percent 100 spring beside it. Two 100% rows split the area and can halve the viewport. Do not change the viewport row to AutoSize to make room for an empty spring. An AutoSize grid collapses to preferred height and the spring takes the rest.
- If a design truly needs both a gap and a viewport, nest the viewport in one content region, or assign intentional unequal percentages. Do not silently insert another 100% row.

An empty Percent spring does not protect overflowing AutoSize content in a constrained root. Extra input rows can give that spring a negative height and overlap the footer. Do not raise the saved top-level bounds to hide that, and do not add a second Percent row. Put the overflowing inputs in one measured `AutoScroll` panel. That panel is the sole Percent 100 content row. Header and footer rows stay AutoSize. The nested footer is unchanged.

A content-sized modal has no spring. Every content row and the footer row are AutoSize.

## Empty editors

Empty `TextBox`, `NumericUpDown`, `ComboBox`, and `MaskedTextBox` must not collapse when the row is AutoSize. After the font settles, one shared helper walks `Controls` and matches those types with patterns. Never use reflection.

The canonical helper is [EditorWidths.Apply](layout-examples.md#runtime-minima). It pattern-matches `TextBox`, `MaskedTextBox`, `ComboBox`, and `NumericUpDown`, and does not recurse into those leaves. Capacity is measured with `TextRenderer` and the settled `Font` only. Chrome is `Width - ClientSize.Width` for a text box, `Width -` the inner edit's `ClientSize.Width` for `NumericUpDown`, and `PreferredHeight` for `ComboBox`. A 1..100 numeric uses `"WWWW"`. A masked editor uses the longer of that six-`W` capacity and `Mask`. Digit count divides the truncated value and does not call `decimal.Abs`. Do not call `GetPreferredSize` for this, do not subtract `Text`, do not construct a probe, and do not add to the previous `MinimumSize.Width`.

`PreferredSize` is read-only. A specialized masked date, time, currency, or measurement control may override `GetPreferredSize` and `SetBoundsCore` when that is justified. Do not add a library base class only to hold a minimum width.

Avoid `DateTimePicker` and `MonthCalendar` by default. That is an accessibility and dark-mode preference, not a claim that the platform lacks those controls. See [options](../../winforms-options-dialog/SKILL.md).

## Hosted views

A UserControl that is a tab, wizard, or complex-form view part uses `AutoScaleMode.Inherit`. The host owns scaling. Do not set `Font` on that view or make it a second DPI root. An explicit `Font` on a hosted `ContainerControl` is skipped by the parent's font scale and does not receive `OnFontChanged` when only the host font changes. An explicit `Font` on a non-container child can be bounds-scaled while its font stays. A top-level window or an intentional scaling-root custom control may set its own `AutoScaleMode`.

## Working-area cap

After a dialog is placed, cap the outer window to `Screen.FromControl(this).WorkingArea` and call `PerformLayout`. Re-evaluate on DPI or monitor change. Do not invent a fixed height, and do not apply the main-window 75/80 ratios to a small modal. `AutoScroll` keeps the footer reachable. This runs from the main partial, not from `InitializeComponent`.

## Accessibility, construction, fullscreen

Set `AccessibleName` when the caption is not enough. Set `TabIndex` in the designer. Test Tab, Shift+Tab, Enter, and Escape. Prefer AutoSize and percent columns so longer translations fit.

For inherited controls, return constants from `CreateParams` and `DefaultSize`. Do not read instance fields the constructor has not assigned. Do not enter fullscreen, hide the form, or create a device there. Fullscreen and persisted bounds: [window-state.md](window-state.md).

Only `SystemColors` track dark mode automatically. Absolute colors need a runtime branch, not `InitializeComponent`.

Menu, status, tree, list, and toolbar images regenerate from a stable logical baseline. Do not multiply `Font.SizeInPoints` by `DeviceDpi / 96`. See [toolstrip icons](../../winforms-toolstrip-icons/SKILL.md).
