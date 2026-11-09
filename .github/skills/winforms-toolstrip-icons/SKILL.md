---
name: winforms-toolstrip-icons
description: >-
  USE FOR: semantic icons on a WinForms command ToolStrip without WARP,
  including a small symbol-font image factory, mixed items, and accessible
  names. DO NOT USE FOR: a text-domain strip the user explicitly requested, or
  a WARP app. Load warp-toolstrip-icons and warp-symbols instead. Factory
  calls do not belong in InitializeComponent.
---

# Icon-first ToolStrips

## Durable rules

- **A command ToolStrip must have semantic icons** unless the user explicitly
  asked for a text-domain strip (tab headers, category labels, and similar).
  Do not decide on your own that an ordinary command bar is text-only.
  Prefer image-only buttons with tooltips and AccessibleName. Keep Text for
  command identity. ImageAndText is a discoverability choice, not a reason to
  skip the image.
- A ToolStripLabel beside its editor, status text, and a user-requested text
  header may omit an icon. Menu captions keep text and mnemonics and still
  take an icon when the command is visual.
- With WARP, load `warp-toolstrip-icons` and `warp-symbols`. Use
  `SymbolToolStrip` / `SymbolMenuStrip` / `SymbolStatusStrip` and `FontSymbol`.
  Do not build a second glyph renderer.
- Without WARP, use one small shared image factory. Do not invent a private
  codepoint and hope GDI substitutes a font.
- Factory calls stay in runtime code after designed items have owners. A
  `.resx` image is the serialization path. A factory call inside
  `InitializeComponent` disappears on the first Designer save.

## Non-WARP image factory contract

Pick the first installed face whose glyph actually exists. Prefer Segoe Fluent
Icons. It is not guaranteed on Windows 10. Then Segoe UI Symbol. Then Segoe
MDL2 Assets. Probe that face. The same private-use codepoint is not
interchangeable: on a current Windows 11 box, U+E74E is missing in Segoe UI
Symbol (`GetGlyphIndices` returns `0xFFFF`) and present in both Fluent and
MDL2 with different glyph ids. U+E733 is Fluent-only against MDL2. U+2699 is
Segoe UI Symbol and absent from the other two. A missing glyph needs a
different meaningful glyph in the face that will draw it, not the other
face's codepoint and not an invisible tofu box.

The factory accepts a named glyph, the current host color, and a pixel size from a stable 16px at 10pt at 96 DPI baseline. Do not measure the previous bitmap and scale that. Under Per-Monitor V2, after `WM_DPICHANGED`, `Control.Font` is already `ScaledControlFont` (`font.WithSize(Size * newDpi / oldDpi)`). `Font.SizeInPoints` is therefore already DPI-scaled. Do not multiply `Font.SizeInPoints / 10` by `DeviceDpi / 96`. That double-scales after a monitor move.

Use one of these:

- Keep the user's logical point size in your own field, set only when the user picks a font. Do not read it back from `Font.SizeInPoints`. An explicit `Font` assignment clears the DPI font cache. Pixel size is `16 * logicalPoints / 10 * DeviceDpi / 96`. That `DeviceDpi` term is coordinate scaling of a logical size, not a second scale of the observed font.
- Or measure the live observed font with no extra DPI factor: `Font.Height`, or `TextRenderer` with `control.Font`. Do not call `Font.GetHeight(DeviceDpi)`.

Window geometry uses monitor DPI on pixels only. It does not use this font formula. See [DIP schema](../winforms-high-dpi-fluent-layout/references/window-state.md#dip-schema).

Return an independently owned transparent bitmap. Draw with an explicit pixel-unit `Font` and centered glyph metrics. Cache only when useful. Do not add an asset framework for a handful of commands.

Images assigned to ToolStripItems need an explicit owner:

- Keep source fonts alive while rendering; dispose owned Fonts, Graphics and
  temporary drawing objects.
- Give each item its own image or a clone of a borrowed cached image.
- On regeneration replace the item image, then dispose its previous owned image.
  A strip/item should not be assumed to dispose arbitrary assigned Images.
- Clear/dispose owned images when the window/strip is disposed. Do not dispose
  shared resources from one item's cleanup.

Own the `Font` and each `Bitmap`. Dispose them when replaced and when the window is disposed. Regenerate on a real `FontChanged`, `DpiChanged`, system metric or user-preference change, and theme or High Contrast change. A posted `WM_DPICHANGED` is not that DPI change. A scalable font source is not a live bitmap. Keep `ImageScalingSize` equal to the pixels just generated. Menu, status, tree, and list images use the same rule. Do not stretch a cached bitmap on each event. GDI details belong to [rendering](../winforms-rendering/SKILL.md).

## Mixed-item strips

For a horizontal toolbar mixing ToolStripButtons with ToolStripLabels,
ToolStripTextBoxes or ToolStripComboBoxes, explicitly use:

```csharp
toolbar.AutoSize = true;
toolbar.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
```

These are declarative property assignments suitable for the Designer partial.
Use `VerticalStackWithOverflow` only for an intentionally vertical strip.
Do not force a fixed strip height or a Flow layout that clips different preferred
heights. Keep inherited/current fonts, image scaling, item margins and editor
widths coherent. Labels remain beside their editors when space permits; verify
overflow still allows meaningful command and editor access.

Set DisplayStyle deliberately: Image for ordinary icon commands; Text for
caption labels; ImageAndText where justified. Test mixed preferred heights at
large runtime fonts and DPI, not just an all-button 100% layout.
Shared host geometry belongs to [layout](../winforms-high-dpi-fluent-layout/SKILL.md).

## Keyboard and accessibility

Frequently used menu commands need `&` mnemonics and appropriate ShortcutKeys;
avoid duplicate accelerators within a menu/category. Icon buttons need concise
AccessibleName and ToolTipText explaining their action and available shortcut.
Do not rely on a private-use glyph or the tooltip as the screen-reader name.

Captioned editors need their own semantic name and predictable focus order.
Use AccessibleDescription only for context not already supplied by the name.
Do not flood screen readers with repeated labels, decorative separators or
continuously announced status/clock updates.

Options/category navigation is a text-header exception owned by
[Options dialogs](../winforms-options-dialog/SKILL.md); it is not a command-icon
factory use case. Static item/events/cleanup follow
[Designer](../winforms-designer-code/SKILL.md).

## Final checks

1. Ordinary commands show real glyph images, not fallback boxes, and have tooltips
   and accessible names; text-only exceptions are intentional.
2. Mixed controls, overflow, keyboard accelerators and focus remain usable after
   font/DPI/theme changes.
3. Each handler executes once; font/image ownership and regeneration are explicit.
