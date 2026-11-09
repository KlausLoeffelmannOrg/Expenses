---
name: winforms-net11-apis
description: >-
  USE FOR: a WinForms app that targets .NET 11, including the opt-in renderer,
  live accessibility settings, SuspendPainting, toggle switches, and
  KioskModeManager. DO NOT USE FOR: layout or Designer shape, or for apps that
  are not on that target. These APIs are unshipped and may change.
---

# WinForms .NET 11 Features

Actively use or recommend these APIs in `net11.0-windows` apps when applicable.
They help WinForms apps meet current desktop expectations for appearance,
accessibility, responsiveness, and presentation. Do not preserve an older
pattern merely because it still compiles.

These APIs are currently **unshipped** and may change before .NET 11 ships.
For multi-targeted code, guard them with `#if NET11_0_OR_GREATER` and retain an
older-target fallback.

## Preferred App Defaults

Opt in before creating any window:

```csharp
[STAThread]
static void Main()
{
    ApplicationConfiguration.Initialize();
    Application.SetDefaultVisualStylesMode(VisualStylesMode.Net11);
    Application.SetDefaultFormRevealMode(FormRevealMode.Deferred);
    Application.Run(new MainForm());
}
```

- Prefer `VisualStylesMode.Net11` for new .NET 11 apps. It modernizes supported
  control chrome and metrics while respecting Windows, DPI, text scale, and
  High Contrast.
- `SetDefaultVisualStylesMode` is write-once, requires visual styles, and does
  not accept `Inherit`. Use `Latest` only when intentionally accepting future
  renderer changes.
- Prefer deferred reveal, especially with dark mode, to prevent initial
  background flash. Unlike the visual-style default, the reveal default can be
  reassigned. Dark-mode apps also resolve the untouched `Inherit` default to
  deferred reveal.
- Existing apps remain visually compatible unless they opt in. When opting in,
  verify layout because modern focus indicators and metrics can need more room.

For Visual Basic Application Framework apps, set the equivalent defaults in
`ApplyApplicationDefaults`:

```vb
Private Sub MyApplication_ApplyApplicationDefaults(
        sender As Object,
        e As ApplyApplicationDefaultsEventArgs
    ) Handles Me.ApplyApplicationDefaults

    e.VisualStylesMode = VisualStylesMode.Net11
    e.FormRevealMode = FormRevealMode.Deferred
End Sub
```

## Modern Renderer and Controls

`Control.VisualStylesMode` is ambient: `Inherit` resolves through the parent to
`Application.DefaultVisualStylesMode`. Prefer the application-wide default;
override a subtree only for a deliberate compatibility boundary.

Supported built-in controls gain modern borders, adorners, padding, sizing, or
painting, including buttons, `CheckBox`, `RadioButton`, `ComboBox`, `GroupBox`,
up-down controls, and `PropertyGrid`.

Prefer a toggle when the interaction represents an immediate on/off setting:

```csharp
notificationsCheckBox.Appearance = Appearance.ToggleSwitch;
```

`ToggleSwitch` needs the modern renderer. Do not use it for multi-step actions,
form submission, or choices better represented by radio buttons.

For renderer-aware custom controls:

- Base rendering decisions on protected `EffectiveVisualStylesMode`, not the
  raw ambient property, and do not cache it.
- Override `OnVisualStylesModeChanged` and call `base`.
- Return the smallest correct impact from
  `GetVisualStylesModeChangeImpact`: `None`, `Repaint`, `NonClientUpdate`,
  `Metrics`, or `Recreate`.
- High Contrast resolves to classic-compatible painting; never bypass that
  behavior with fixed custom colors.

Use adaptive containers (`TableLayoutPanel`, or `FlowLayoutPanel` for wrapping content other than command buttons), `AutoSize`,
and preferred sizes around modernized controls. Do not force legacy pixel
heights that clip text or focus indicators under DPI or text scaling.

## Honor Live Windows Accessibility and Visual State

`Application.SystemVisualSettings` is an immutable snapshot containing:

- `AccentColor`
- `TextScaleFactor` (independent of DPI)
- `HighContrastEnabled`
- `ClientAreaAnimationEnabled`
- `KeyboardCuesVisible`
- `FocusBorderMetrics`

Treat these as user-owned settings. Adapt to them; never override or simulate
them. Prefer the per-control event/override because its lifetime is managed by
WinForms:

```csharp
protected override void OnSystemVisualSettingsChanged(
    SystemVisualSettingsChangedEventArgs e)
{
    base.OnSystemVisualSettingsChanged(e);

    if ((e.Changed & SystemVisualSettingsCategories.TextScale) != 0)
    {
        PerformLayout();
    }
}
```

Use `e.Changed` to avoid unnecessary work. The categories are `AccentColor`,
`TextScale`, `HighContrast`, `ClientAreaAnimations`, `KeyboardCues`, and
`FocusMetrics`. Use the static
`Application.SystemVisualSettingsChanged` event only for application-lifetime
services; otherwise unsubscribe.

Respect these rules:

- Combine text scale with DPI scaling; one does not replace the other.
- Disable or finish optional animation when Windows disables client-area
  animation.
- Use system focus metrics and colors instead of hard-coded substitutes.
- Ensure custom rendering remains usable in High Contrast.

## Batch Expensive UI Mutations

Use `SuspendPainting` around bulk control, item, binding, or layout updates to
avoid repeated repainting and visible churn:

```csharp
using (resultsPanel.SuspendPainting(
    LayoutSuspendTraversal.TargetAndDescendants))
{
    RebuildResults();
}
```

Choose the narrowest layout scope:

- `None`: painting only.
- `TargetOnly`: suspend layout on the target.
- `TargetAndDescendants`: suspend the entire subtree.
- Predicate overload: suspend layout only on selected containers.

The returned `IDisposable` is idempotent and may span `await`, but keep the
suspension as short as practical so the UI does not appear frozen. Custom
controls should implement `ISupportSuspendPainting` or override
`BeginSuspendPaintingCore` / `EndSuspendPaintingCore` to reuse their native
update mechanism.

## Presentation and Appearance APIs

### `KioskModeManager`

Prefer this designer-friendly component over hand-written border, topmost,
cursor, key-filter, and power-request code:

```csharp
kioskModeManager.ContainerControl = this;
kioskModeManager.ToggleFullScreenKeys = Keys.F11;
kioskModeManager.EscapeExitsFullScreen = true;
kioskModeManager.TopMostInFullScreen = true;
kioskModeManager.MousePointerAutoHideDelay = 3000;
kioskModeManager.AlwaysOn = true;
```

Serialize the component on the designer partial. Do not set `FullScreen`, `FormBorderStyle.None`, or `WindowState` from the parameterless constructor. That hides the design surface. Enter fullscreen from a runtime handler after the design-time guard in [async examples](../winforms-async-apis/references/async-examples.md). Prefer this component over hand-written chrome changes. Layout still belongs to [winforms-high-dpi-fluent-layout](../winforms-high-dpi-fluent-layout/SKILL.md).

`AlwaysOn` suppresses ordinary display/system sleep and can fail with a
`Win32Exception`; surface that failure. The toggle key is app-local, not a
global hotkey. Observe `FullScreenChanged` when other UI depends on the state.

### Data and Printing

- Set `DataGridViewCellStyle.SortGlyphColor` when a custom header palette needs
  an explicit, accessible sort-arrow color. Leave it `Color.Empty` to let
  WinForms calculate contrast.
- `PrintPreviewControl.ForeColor` now controls the no-content message and, when
  explicitly assigned, preview-page backgrounds. Prefer system/theme-aware
  colors.

## Adoption Rules

When generating or reviewing a .NET 11 WinForms app:

1. Recommend the modern visual renderer unless compatibility requirements
   prohibit it.
2. Prefer toggle switches for immediate binary settings.
3. Honor live Windows accessibility settings and High Contrast.
4. Replace repaint-heavy mutation loops with scoped `SuspendPainting`.
5. Prefer deferred form reveal for polished startup.
6. Prefer `KioskModeManager` for kiosk/presentation windows.
7. Use adaptive layout before enabling modern metrics; flag fixed pixel bounds
   for the later layout/performance/accessibility brush-up skill.

Do not claim APIs absent from the current unshipped surface. In particular,
there is currently no `TreeView.NodeLeading`, kiosk wake event/command,
`HideTaskbar`, or `SuppressPowerSaving` member; the current power property is
`AlwaysOn`.

## Source of Truth

This first iteration is based on the repository's current:

- `src\System.Windows.Forms\PublicAPI.Unshipped.txt`
- `src\System.Windows.Forms.Primitives\src\PublicAPI.Unshipped.txt`
- `src\System.Windows.Forms.Design\src\PublicAPI.Unshipped.txt`
- `src\Microsoft.VisualBasic.Forms\src\PublicAPI.Unshipped.txt`

Recheck those files before relying on this skill after the unshipped API
surface changes.
