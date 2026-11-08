---
name: warp-directx-using-visuals
description: Use this skill for WARP-DirectX apps with a render/animation loop — games, info tables, dashboards, and fullscreen kiosk scenes — that need flicker-free animation and complex scene composition via retained DirectComposition Visuals (sprites). It explains the Visuals collection (Visuals.AddNew, D2DVisual.Bounds/Visible/Transform), z-order (BringToFront/SendToBack), the two ways to draw a visual's content — the render-thread-safe D2DVisual.PaintContent callback (required for background render modes) and immediate-mode D2DGraphics.FromVisual (D2DWinFormsClassic only) — invalidating content with InvalidateContent, committing changes with CommitVisualsAsync, the PreserveLastFrame/TargetFrameRate/VSyncEnabled loop knobs, and how to drive frames from a high-precision timer.
metadata: Required nuget packages "WarpToolkit.WinForms.DirectX", "WarpToolkit.Windows.Interop"; latest version "0.9.219-preview.g61ade857e5"
---

# WARP DirectX — Using Visuals (render loop, animation, scene composition)

> Read **`warp-directx-getting-started`** first for base types, the render
> callback and `RenderMode`. This skill covers **retained Visuals** — the path
> for animated, composited scenes that stay flicker-free.

## Immediate-mode vs retained Visuals

- **Immediate mode** (the *getting-started* `RenderBackground` / `Render`
  handler) redraws the whole surface each frame. Great for simple scenes.
- **Retained Visuals** give each scene element its own DirectComposition visual
  with cached content. You move/show/hide visuals and the **compositor** redraws
  only what changed — ideal for many moving sprites, dashboards, or layered
  scenes where you don't want to re-issue every draw call every frame.

Hosts (`D2DPanel`, `D2DForm`, `D2DControl`) expose a retained `Visuals`
collection (`D2DVisualCollection`).

Current preview version: `0.9.219-preview.g61ade857e5`.
                        
## The Visual API

```csharp
using WarpToolkit.WinForms.DirectX.Controls; // D2DVisual, RenderMode
using WarpToolkit.WinForms.DirectX.D2D;      // ID2DGraphics, D2DGraphics, D2DVisualPaintEventArgs
```

- `host.Visuals.AddNew(Rectangle bounds)` → `D2DVisual` (also `AddNew(Point, Size)`).
- `D2DVisual.Bounds` — position/size in host-client coordinates (set to move/resize).
- `D2DVisual.Visible` — show/hide without redrawing content.
- `D2DVisual.Transform` / `SetRotation` / `SetTransform` — an affine transform the
  compositor applies for free (rotate/scale/skew) **without** redrawing content.
- `D2DVisual.BringToFront()` / `SendToBack()` — change z-order (see below).
- `host.Visuals.Remove(visual)` — remove a visual.
- **Drawing content:** see *Drawing a visual's content* below — use the
  `D2DVisual.PaintContent` event (render-thread-safe, works in **every** render mode)
  or, in `D2DWinFormsClassic` only, `D2DGraphics.FromVisual(visual)`.
- `D2DVisual.InvalidateContent()` — request that `PaintContent` re-run on the render
  thread (after you change data the handler reads). Safe to call from any thread.
- `await host.CommitVisualsAsync(waitForVSync, cancellationToken)` — commit all
  pending visual changes (adds, bounds, visibility, z-order) on the render thread.

> **All of the property setters above (Bounds/Visible/Transform/z-order) are
> thread-safe** — they record intent and are applied by the compositor on the render
> thread. Only *content drawing* is thread-affine (next section).

## Drawing a visual's content (two paths)

A visual's *pixel content* is cached in its own swap chain. There are two ways to
draw it, and the right choice depends on `RenderMode`:

| Path | Runs on | Use in |
|------|---------|--------|
| **`D2DVisual.PaintContent`** event | the host **render thread** | **every** render mode — the portable choice, and the **only** correct one for `D2DSharedRenderThread` / `D2DDedicatedRenderThread` |
| `D2DGraphics.FromVisual(visual)` (immediate `BeginDraw`/`EndDraw`) | the **caller's** thread | `D2DWinFormsClassic` only (its render thread *is* the UI thread) |

> ⚠️ `D2DGraphics.FromVisual` / `visual.GetGraphics()` is **render-thread-affine**: it
> throws if called from any thread that doesn't own the host's Direct2D device. In a
> background render mode that means calling it from the UI thread **throws**. Use
> `PaintContent` instead — then the same code works in every mode.

### `PaintContent` (recommended)

Subscribe a handler; the host raises it **on the render thread**, with an
`ID2DGraphics` already bound to the visual inside a `BeginDraw`/`EndDraw` bracket and
the surface pre-cleared to transparent. Draw in content-local coordinates (origin =
visual top-left). Presentation of the visual's swap chain is automatic.

```csharp
D2DVisual visual = host.Visuals.AddNew(bounds);
visual.PaintContent += (s, e) =>
{
    ID2DGraphics g = e.Graphics;                  // already begun + cleared transparent
    Size size = e.ContentSize;                    // the visual's content size in pixels
    g.FillRectangle(Color.FromArgb(200, 60, 160, 220), new RectangleF(0, 0, size.Width, size.Height));
    using Font font = new("Segoe UI", 10f, FontStyle.Bold);
    g.DrawString(index.ToString(), font, Color.White, 6f, 5f);
};
```

The host re-raises `PaintContent` automatically when the visual is **created,
resized, or after device-loss / a render-mode switch**, so retained content is
restored without you re-issuing draws. To force a repaint yourself (e.g. after
changing data the handler reads), call `visual.InvalidateContent()`.

Because `PaintContent` runs off the UI thread in background modes, **snapshot any
WinForms state** (control values, fonts, colors) into thread-safe fields on the UI
thread and read those snapshots in the handler — never touch a `Control` from it.

## Loop knobs for flicker-free animation

Set these on the host (and on any visual-hosting child surface):

```csharp
RenderMode        = RenderMode.D2DWinFormsClassic; // or a background-thread mode
PreserveLastFrame = true;   // keep last frame when a tick issues no draws
VSyncEnabled      = false;  // pair with TargetFrameRate for a fixed cadence
TargetFrameRate   = 60d;    // Hz, used when VSyncEnabled is false
```

Flip-model presentation + DirectComposition means there is no GDI "erase then
paint" step, so retained visuals are inherently flicker-free.

## Setting up a visual collection

```csharp
private readonly List<D2DVisual> _visuals = new();

private void BuildScene(Rectangle sceneBounds)
{
    for (int i = 0; i < 200; i++)
    {
        Rectangle bounds = CreateRandomBounds(sceneBounds);
        D2DVisual visual = Visuals.AddNew(bounds);

        int index = i; // capture per-visual state for the handler
        visual.PaintContent += (s, e) => DrawVisual(e, index);

        _visuals.Add(visual);
    }

    CommitVisuals();
}

// Runs on the render thread; e.Graphics is begun + cleared transparent for you.
private void DrawVisual(D2DVisualPaintEventArgs e, int index)
{
    ID2DGraphics g = e.Graphics;
    Size size = e.ContentSize;          // visual content size; origin (0,0) = top-left

    g.FillRectangle(Color.FromArgb(200, 60, 160, 220), new RectangleF(0, 0, size.Width, size.Height));
    using Font font = new("Segoe UI", 10f, FontStyle.Bold);
    g.DrawString(index.ToString(), font, Color.White, 6f, 5f);
}

private void CommitVisuals()
    => CommitVisualsAsync(VSyncEnabled).GetAwaiter().GetResult();
```

Redraw a visual's content only when its **appearance** changes — call
`visual.InvalidateContent()` (or, in `D2DWinFormsClassic`, `D2DGraphics.FromVisual` +
`BeginDraw`/`EndDraw`). To merely **move/rotate** it, set `visual.Bounds` /
`visual.Transform` and commit — no content redraw needed.

## Z-order

The host's **immediate-mode / default surface is anchored at the BOTTOM** of the
DirectComposition z-order, so an opaque background drawn via the host `Render` /
`RenderBackground` handler composites **behind** all retained visuals. Retained
visuals stack above it in collection order: **the last visual added is front-most.**

- `visual.BringToFront()` — moves it to the front (top) of the retained visuals.
- `visual.SendToBack()` — moves it to the back, but still **above** the default
  surface (it never goes behind the immediate-mode background).

So the layering, bottom → top, is: *default/background surface → first retained
visual → … → last retained visual*. Transparent areas of a visual reveal whatever is
behind it, down to the background surface.

## Driving the animation loop

Use a high-precision timer (`WarpToolkit.Windows.Interop.PrecisionTimer`) to tick
the simulation, then advance bounds and commit. Compute motion from elapsed time
so speed is frame-rate independent.

```csharp
using System.Diagnostics;
using WarpToolkit.Windows.Interop.PrecisionTimer;

private readonly HighPrecisionTimer _timer = new();
private object? _registration;
private long _lastTimestamp;

private void StartLoop()
{
    _lastTimestamp = Stopwatch.GetTimestamp();
    _registration = _timer.Register(
        this,
        static form => form.Tick(),
        new PeriodicTimerOptions
        {
            Interval       = TimeSpan.FromMilliseconds(1000d / 60d),
            MaxConcurrency = 1,
            OverloadPolicy = OverloadPolicy.SkipAndLog,
        });
}

private void Tick()
{
    float seconds = (float)Stopwatch.GetElapsedTime(_lastTimestamp).TotalSeconds;
    _lastTimestamp = Stopwatch.GetTimestamp();

    Rectangle scene = ClientRectangle;
    foreach (D2DVisual v in _visuals)
    {
        // advance position using your own velocity state…
        v.Bounds = StepBounds(v.Bounds, scene, seconds);
    }

    CommitVisuals();   // one commit per frame
    Invalidate();      // request presentation
}

private void StopLoop()
{
    if (_registration is not null)
    {
        _timer.Unregister(_registration);
        _registration = null;
    }
}
```

For a continuously running scene you can also choose a background render mode
(`D2DSharedRenderThread` / `D2DDedicatedRenderThread`); those run off the UI thread,
so draw visual content via `PaintContent` (never `FromVisual` from the UI thread) and
snapshot any WinForms state on the UI thread first.

## Recommended frame shape

1. **Tick** — update simulation state from elapsed time.
2. **Apply** — set `visual.Bounds` / `visual.Visible` / `visual.Transform`; for
   appearance changes call `visual.InvalidateContent()` (the render thread repaints
   via `PaintContent`).
3. **Commit** — exactly **one** `CommitVisualsAsync` per frame.
4. **Present** — `Invalidate()`.

## Anti-patterns

- **Do not** draw a visual's content with `D2DGraphics.FromVisual` /
  `visual.GetGraphics()` from the UI thread in a **background** render mode — it is
  render-thread-affine and **throws**. Use the `PaintContent` event instead, which
  works in every mode.
- **Do not** touch WinForms `Control` state from a `PaintContent` handler (or any
  background-mode render handler) — snapshot it on the UI thread first.
- **Do not** call `CommitVisualsAsync` per-visual — batch all changes and commit
  once per frame.
- **Do not** recreate visuals every frame — create once, then mutate
  `Bounds`/`Visible`/`Transform`. Recreate only when the scene's element set changes.
- **Do not** redraw a visual's cached content every frame just to move or rotate it —
  set `Bounds`/`Transform` instead; redraw (`InvalidateContent`) only on appearance
  changes.
- **Do not** use wall-clock deltas from a low-resolution timer for motion — use a
  high-precision timer / `Stopwatch.GetElapsedTime` so animation is smooth and
  frame-rate independent.
