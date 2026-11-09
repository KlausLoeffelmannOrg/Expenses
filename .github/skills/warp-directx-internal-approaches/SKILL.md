---
name: warp-directx-internal-approaches
description: >-
  Architectural reasoning for WARP DirectX/Direct2D/DirectComposition: choose among
  GDI+-compatible Direct2D rendering, shared or dedicated render threads, DComp
  visuals, static retained scenes, game-like loops, media surfaces and complex
  hwnd-less controls (modern list, popup). Explains intent, public building blocks,
  threading contracts and capability boundaries when source is unavailable.
metadata:
  source: "WARP src\\WarpToolkit.WinForms.DirectX (public surface only)"
  versions: "../warp-api-decision-guide/references/versions.md"
---

# WARP DirectX, Direct2D, and DirectComposition: Internal Approaches

## Designer boundary

- `D2DControl`/`D2DPanel` subclasses that only render are **render controls**: a
  single-file class with `Render +=` in the ctor is acceptable (no Designer pair required).
- `D2DForm` and any Form/UserControl that *hosts* render controls stay Designer pairs:
  `partial`, `.Designer.*`, parameterless ctor, serializable properties set in
  `InitializeComponent`, no GPU/device/thread kickoff on the design path
  (see `warp-directx-getting-started`, `warp-directx-dcomp-exclusive-tlw`).
- Render mode, render-thread and visual setup happen in `OnHandleCreated`/`OnLoad`,
  guarded for `DesignMode`.

Use this skill to reason about the architecture before writing rendering code. It is
especially important when only the public NuGet package and generated API reference
are available.

For basic API syntax, also use `warp-directx-getting-started`. For detailed retained
visual examples, use `warp-directx-using-visuals`. For a DComp-exclusive top-level
window, use `warp-directx-dcomp-exclusive-tlw`.

## Highest principle: first pixels with zero friction

WARP's primary design goal is that a WinForms developer can get the first accelerated
result without first learning COM, DXGI, swap chains, Direct2D factories, DirectWrite,
or DirectComposition.

The public drawing surface, `ID2DGraphics`, intentionally resembles roughly 90 percent
of everyday `System.Drawing.Graphics` usage:

- familiar `Color`, `Point`, `Rectangle`, `Font`, `Pen`, `Brush`, and `Image` types;
- familiar verbs such as `Clear`, `DrawLine`, `DrawRectangle`, `FillRectangle`,
  `DrawEllipse`, `DrawString`, `MeasureString`, and `DrawImage`;
- a normal WinForms `Control`/`Panel`/`Form` hosting model;
- ordinary invalidation for the default mode.

The compatibility is conceptual and source-friendly, not a promise that
`ID2DGraphics` is a drop-in implementation of every GDI+ feature. Do not port COM,
swap-chain, or DirectComposition setup into application code. Start with a WARP host
and its render callback:

```csharp
using WarpToolkit.WinForms.DirectX.Controls;
using WarpToolkit.WinForms.DirectX.D2D;

internal sealed class StatusSurface : D2DControl
{
    public StatusSurface()
    {
        Render += RenderFrame;
    }

    private void RenderFrame(object? sender, D2DRenderEventArgs e)
    {
        e.Graphics.Clear(BackColor);
        e.Graphics.FillRoundedRectangle(
            Color.DodgerBlue,
            new RectangleF(12, 12, Width - 24, Height - 24),
            new SizeF(8, 8));
        e.Graphics.DrawString(Text, Font, ForeColor, 24, 24);
    }
}
```

The framework owns `BeginDraw`, `EndDraw`, presentation, device creation, resize,
handle recreation, and device-loss recovery. The callback owns only frame content.
Do not retain `e.Graphics` after the callback returns.

## The architecture in one picture

```text
WinForms Control / Panel / Form (HWND, input, layout, accessibility)
        |
        v
WARP render host (lifetime, render policy, thread ownership)
        |
        +--> Direct3D/DXGI flip-model swap chain
        +--> Direct2D + DirectWrite drawing
        `--> DirectComposition target and visual tree
                    |
                    v
                 Windows DWM
```

Direct2D is the drawing API. DXGI supplies the presentation buffers.
DirectComposition attaches those buffers to HWNDs and composes retained visual layers.
The three technologies solve different parts of the problem; "just use DirectX" is
not an equivalent hosting architecture.

## Why DirectComposition, not just Direct2D/DirectX?

DirectComposition is what lets WARP integrate accelerated pixels into normal WinForms
windowing without asking customers to manage a custom presentation engine.

It provides:

- **HWND integration:** a flip-model swap chain becomes content of a WinForms window.
- **DWM composition:** presentation participates in the Windows compositor rather
  than copying a GDI bitmap into the window.
- **Retained layers:** independent visual surfaces can remain visible when no new
  drawing is issued.
- **Cheap scene changes:** bounds, visibility, z-order, and transforms can change
  without redrawing cached visual content.
- **Flicker resistance:** there is no GDI erase-then-paint cycle for D2D content.
- **Separation of concerns:** Direct2D draws pixels; DComp positions and combines
  surfaces; WinForms continues to own input, layout, focus, handles, and accessibility.

This distinction matters for static scenes and animation. A redraw is not required
merely because a retained visual moved or rotated; DComp can recompose its existing
content.

## The execution model: three Direct2D modes, plus fallback

WARP makes no compromise by forcing all workloads through one threading model.
Choose among three Direct2D execution modes. `RenderMode.GDIPlus` is an additional
non-Direct2D fallback and comparison mode.

| Mode | Thread and frame driver | Best fit | Non-negotiable constraint |
|---|---|---|---|
| `GDIPlus` | Normal WinForms/GDI+ painting; no D2D resources | Fallback, compatibility testing, A/B comparison | `ID2DGraphics` and retained D2D visuals are unavailable |
| `D2DWinFormsClassic` | UI thread, on demand through `WM_PAINT` | Default choice, static controls, straightforward GDI+ ports | Keep rendering short enough for UI responsiveness |
| `D2DSharedRenderThread` | One process-wide background render clock | Many lightweight animated or snapshot-driven surfaces | Render callbacks must not read live WinForms state |
| `D2DDedicatedRenderThread` | One background render thread per host | A small number of heavy, independently paced surfaces | Dedicated threads are process-budgeted; assignment can fail |

### Selection rule

1. Start with `D2DWinFormsClassic`.
2. Move to `D2DSharedRenderThread` when several controls need smooth background
   rendering or when snapshot-driven work should not occupy the UI thread.
3. Use `D2DDedicatedRenderThread` only for a demonstrably heavy surface that needs
   independent pacing. Treat it as scarce capacity, not the default for every control.

Setting dedicated mode is a hard request and may throw `InvalidOperationException`
when the process-wide budget is exhausted. Application code should be ready to use
shared mode instead. Do not create a farm of dedicated controls.

For any background mode:

- publish immutable render snapshots from the UI/model side;
- never read `Control.Handle`, `ClientSize`, `Text`, `Font`, child controls, or other
  live WinForms state from the render callback;
- create and use thread-affine drawing resources only inside the owning render path;
- marshal UI changes back to the UI thread.

## Hosts and which one to choose

| Public type | Purpose |
|---|---|
| `D2DControl` | Leaf accelerated custom control and common base |
| `D2DPanel` | Accelerated container that can host child WinForms HWNDs |
| `D2DForm` | Accelerated top-level window |
| `D2DButton`, `D2DLabel` | Ready-made simple D2D controls |
| `D2DTextBox`, `D2DCodeEditor`, `D2DConsole`, `D2DMarkdownPreview` | Ready-made snapshot-driven text/document controls |

`D2DPanel` deliberately keeps the redirection support needed by classic child HWNDs.
Its static background is cached. `D2DForm` can optionally become
DirectComposition-exclusive, but then every child must also be D2D/DComp content.

Do not override `OnPaint` to put GDI+ drawing on a `D2DControl` or `D2DPanel`. Use the
D2D render callback. `CreateGraphics()` for direct GDI drawing is intentionally not
the model.

## Why visuals?

There are three useful composition levels. Pick the least complex one that fits:

| Level | API | Use when |
|---|---|---|
| Immediate frame | `Render` / `RenderBackground` + `ID2DGraphics` | The whole small surface can be redrawn together |
| Hwnd-less interaction tree | `RootElement`, `D2DElement`, `D2DElementContainer` | Many interactive parts need hit testing, focus, keyboard, mouse, layout, and invalidation |
| Retained DComp layer | `Visuals`, `D2DVisual` | A layer must retain pixels and move, hide, reorder, scale, or rotate independently |

A `D2DVisual` is not merely a drawing helper. It owns retained pixel content and a
place in the DComp tree. Use visuals for independently composited layers: sprites,
overlays, a fixed background behind animation, selection adorners, or panels that
move without repainting their contents.

Do not create one DComp visual for every trivial row, glyph, or cell. Every retained
visual has presentation resources. For dense controls, render many logical items into
one host surface and reserve retained visuals for coarse independently moving layers.


## Static scenes, animated loops, complex controls

Worked samples moved to references:

- [Static scene / game loop / loop settings](references/scenes-and-loops.md)
- [Media surfaces, ModernListView, PopupForm/PopupWindow](references/complex-controls.md)

## Threading and resource rules

- A render callback is the only universally safe place to use its `ID2DGraphics`.
- Never cache a frame-scoped `ID2DGraphics`.
- `D2DVisual.PaintContent` is the correct content-drawing API in every mode.
- `D2DGraphics.FromVisual(visual)` / `visual.GetGraphics()` is render-thread-affine;
  use it only when already on the owning render thread. UI-thread calls are only
  naturally valid in `D2DWinFormsClassic`.
- Visual state (`Bounds`, `Visible`, `Transform`, z-order) may be changed independently;
  commit related changes once with `CommitVisualsAsync`.
- Do not read live WinForms state in a background render callback. Snapshot first.
- Do not share or manufacture D3D/D2D/DComp/DirectWrite COM objects across WARP hosts
  or render modes.
- A render-mode transition changes resource ownership. Assume application-created
  device resources must be rebuilt.

## Source-unavailable workflow

When WARP source code is unavailable:

1. Treat the installed package's public API and generated reference documentation as
   authoritative.
2. Use the public hosts, callbacks, elements, visuals, and image abstractions first.
3. Do not use reflection to reach internal render hosts, device managers, swap chains,
   DComp visuals, resource caches, or dedicated-thread budget controls.
4. Do not copy inferred internal COM setup into customer code.
5. If a required capability is absent—native video texture import, popup lifecycle,
   custom effects, or another interop seam—state the gap and design a narrow public
   extension request instead of depending on implementation details.
6. Keep fallback behavior explicit. A rejected dedicated-thread request should lead
   to shared rendering, not a silently broken control.

## Architecture guardrails

- Optimize first-use ergonomics, but do not erase threading distinctions.
- Default to classic UI-thread rendering; escalate only for a measured reason.
- Keep dedicated render threads few.
- Keep static content retained and invalidation-driven.
- Keep continuously changing scenes snapshot-driven and frame-rate independent.
- Use DComp visuals for independent composition, not as a replacement for every
  logical UI element.
- Keep code conflicts between WinForms state and render-thread state visible through
  explicit snapshots; never hide them behind unsafe cross-thread reads.
- Preserve the boundary: WinForms owns window semantics and input, Direct2D draws,
  DXGI presents, and DirectComposition composes.

## Final checks

1. Render controls are the only non-Designer classes; every `D2DForm`/host Form is a Designer pair.
2. Render mode chosen by the selection rule; dedicated threads are few and have a shared fallback.
3. No reflection into internals; gaps recorded as public extension requests.
4. Versions come from the project / `versions.md`.
