---
name: warp-directx-dcomp-exclusive-tlw
description: Use this skill when building a WinForms app whose top-level window is rendered purely by WARP-DirectX (a D2DForm) using DirectComposition-exclusive rendering — fullscreen, kiosk, or game-style windows that have NO GDI parent redirection surface. It explains the prerequisites, the benefits, the UseDirectCompositionExclusiveRendering switch (WS_EX_NOREDIRECTIONBITMAP), the critical "every child must also be Direct2D/DComp content" constraint, and shows a minimal working DComp-exclusive form.
metadata:
  source: "WARP src\\WarpToolkit.WinForms.DirectX\\Controls\\D2DForm"
  versions: "../warp-api-decision-guide/references/versions.md"
---

# WARP DirectX — DirectComposition-Exclusive Top-Level Window

> Read **`warp-directx-getting-started`** first for the base types, the render
> callback, and `RenderMode`. This skill is the deep dive on top-level windows
> that are owned **exclusively** by DirectComposition.

A `D2DForm` renders its client area through a flip-model DXGI swap chain hosted
by DirectComposition. By going one step further and making the form's HWND
**DComp-exclusive**, Windows never allocates a GDI **redirection bitmap** for
that window — the window is pure composited content.

Package version: project pin / [versions.md](../warp-api-decision-guide/references/versions.md).

**Designer boundary:** a `D2DForm` is still a Designer pair. `FormBorderStyle`,
`WindowState` and `UseDirectCompositionExclusiveRendering` are serialized in
`InitializeComponent`; the ctor calls only `InitializeComponent()`. Keep
`WindowState = Maximized` out of the design-time path if it disturbs the
Designer (set it in `OnLoad` guarded by `DesignMode`).

## What "exclusive" means

`UseDirectCompositionExclusiveRendering` is a **`D2DForm`-only** property. It controls
one thing: whether the top-level HWND requests the `WS_EX_NOREDIRECTIONBITMAP`
extended style.

- **Enabled** → no GDI redirection bitmap is allocated; the OS treats the HWND
  as DirectComposition-only content.
- `D2DForm` default is **`false`**, because the top-level redirection bitmap is the
  fallback surface that **classic child HWNDs** paint onto. You opt in to make a
  form DComp-*exclusive*.

The decision is genuinely top-level-only, so it does **not** exist on child
controls:

- A leaf **`D2DControl`** (e.g. `D2DButton`, `D2DLabel`, `D2DPictureBox`) is
  *always* DComp-exclusive — its pixels come purely from its own swap chain and it
  hosts no children, so it always carries `WS_EX_NOREDIRECTIONBITMAP`. There is
  nothing to configure.
- A **`D2DPanel`** is *always* a non-exclusive **container** — it keeps a
  redirection bitmap (and adds `WS_CLIPCHILDREN` + a `WM_ERASEBKGND` fill) so it can
  host moving child HWNDs without flicker. Tune that machinery via
  `D2DCompositionTuning` (compile-time switches), not a per-instance flag.

> A future `D2DPopup` — a parented control that composites like a top-level window —
> will reintroduce this opt-in at the popup level (see GitHub issue #106).

## The one hard prerequisite: no classic child HWNDs

Classic WinForms child controls (`Button`, `TextBox`, `Panel`, a stock
`DataGridView`, …) render onto the **parent** window's redirection bitmap.

A DComp-exclusive top-level window **has no parent redirection surface** — there
is nothing for classic child content to render onto. Therefore:

> **Every child of a DComp-exclusive `D2DForm` must itself be Direct2D /
> DirectComposition content** (e.g. `D2DPanel`, `D2DLabel`, other `D2DControl`s,
> or visuals you draw yourself). Do not place classic GDI child controls on it.

If you need ordinary WinForms controls (menus, toolbars, dialogs), keep the form
**non-exclusive** (the default `D2DForm`) — you still get an accelerated client
area, just with the redirection bitmap available for classic children.

## Prerequisites checklist

- `net10.0-windows10.0.22000.0`+ (DirectComposition is a Windows-only target).
- A form deriving from `D2DForm`.
- Children, if any, are Direct2D/DComp content — **not** classic GDI controls.
- For animated content, pick a render mode and/or drive frames (see
  *Using-Visuals*).

## Benefits

- **No redirection bitmap** → lower memory and no GDI copy-back step; the
  window is composited directly by the DWM.
- **Tear-free, flip-model presentation** for fullscreen / kiosk / game UIs.
- **Clean compositor ownership** — ideal when the whole window is your scene and
  there are no stock controls to host.

## Minimal working DComp-exclusive form

```csharp
// KioskForm.Designer.cs (Designer-generated; fragment of InitializeComponent)
this.UseDirectCompositionExclusiveRendering = true;   // before the handle exists
this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
this.BackColor = System.Drawing.Color.Black;
this.ClientSize = new System.Drawing.Size(1920, 1080);
this.Render += new System.EventHandler<WarpToolkit.WinForms.DirectX.D2D.D2DRenderEventArgs>(this.OnFrameRender);
```

```csharp
// KioskForm.cs
using WarpToolkit.WinForms.DirectX.Controls;
using WarpToolkit.WinForms.DirectX.D2D;

internal sealed partial class KioskForm : D2DForm
{
    public KioskForm()
    {
        InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (!DesignMode)
        {
            WindowState = FormWindowState.Maximized; // fullscreen kiosk, runtime only
        }
    }

    private void OnFrameRender(object? sender, D2DRenderEventArgs e)
    {
        ID2DGraphics g = e.Graphics;
        g.Clear(Color.Black);

        using Font font = new("Segoe UI", 48f, FontStyle.Bold);
        g.DrawString("KIOSK READY", font, Color.White, 80f, 80f);
    }
}
```

Notes:
- Set `UseDirectCompositionExclusiveRendering` early (in `InitializeComponent`,
  before the handle is created). Use `ResetUseDirectCompositionExclusiveRendering()` to
  return to the form default. It throws if changed after the handle exists.
- `DefaultUseDirectCompositionExclusiveRendering` (on `D2DForm`) reports the
  default (`false`). It is a top-level concept only — child `D2DControl`s are always
  exclusive and `D2DPanel`s are always non-exclusive containers.
- The OS still draws the **title bar / borders** if you keep them; only the
  client area is Direct2D. For a true fullscreen kiosk, drop the chrome as above.

## Render modes & frame loop

All `RenderMode` options and the `PreserveLastFrame` / `TargetFrameRate` /
`VSyncEnabled` knobs from *getting-started* apply unchanged. For a continuously
animating kiosk/game, select a background render mode and/or drive frames from a
timer and use retained visuals — see **`warp-directx-using-visuals`**.

When you layer a scene, the form's own `Render` output (the immediate-mode surface)
composites at the **bottom** of the z-order, so a full-window background you draw in
`Render` sits **behind** every retained visual; the visuals stack on top in
collection order. In a background render mode, draw each visual's content via the
render-thread-safe `D2DVisual.PaintContent` event (not `D2DGraphics.FromVisual`).

## Anti-patterns

- **Do not** put classic GDI child controls on a DComp-exclusive `D2DForm` —
  they have no redirection surface to paint onto and will not render correctly.
  Either make the children Direct2D content, or keep the form non-exclusive.
- **Do not** toggle `UseDirectCompositionExclusiveRendering` after the window is
  shown and expecting child-hosting behavior to change retroactively — decide at
  construction time.
- **Do not** assume exclusivity is required for acceleration — a plain
  (non-exclusive) `D2DForm` already renders its client area with Direct2D.

## Final checks

1. `KioskForm`/`D2DForm` is a Designer pair; exclusivity, border and size are serialized in `InitializeComponent`.
2. No classic child HWNDs on an exclusive form; fullscreen state applied at runtime only.
3. Render mode/thread selection follows `warp-directx-internal-approaches`; version from the project pin.
