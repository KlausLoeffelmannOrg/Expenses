<!-- Reference of warp-directx-internal-approaches. Render controls (D2DControl/D2DPanel subclasses) are render-only surfaces and are the one exception to the Designer-pair rule; D2DForm hosts are not. -->

# Complex controls: media surfaces, ModernListView, popups

## Example 1: Approach to implement Camera/Playback controls: reuse the host, supply the media pipeline

You could implement camera preview or media playback without rebuilding the WinForms,
Direct2D, swap-chain, DComp, render-loop, resize, and device-loss infrastructure.
Derive from `D2DControl` and render decoded frames through `ID2DGraphics.DrawImage`.

The DirectX package does **not** currently provide a public camera-capture service,
media decoder, transport clock, audio pipeline, or ready-made camera/video control.
Those remain application or media-package responsibilities.

Recommended boundary:

1. Capture/decode frames outside the render callback.
2. Publish an immutable/latest-frame snapshot; drop obsolete frames rather than
   queueing unbounded latency.
3. Request a frame (`Invalidate` in classic mode, or use a background on-demand loop).
4. Draw only from the render callback.
5. Dispose frame/image resources deterministically.

`DrawImage(Image, ...)` is the zero-friction path but converts the GDI+ image for the
draw. For repeated frames, `e.Graphics.FromImage(image)` creates a reusable
`ID2DImage`, and `ID2DImage.BitmapBytes` plus `ApplyBitmapBytes()` can update pixel
data. These objects are tied to the active D2D resource domain: create, update, draw,
and dispose them on the owning render thread, and be prepared to recreate them after
a render-mode change or device loss. If the decoder produces native DXGI textures and
zero-copy interop is required, do not invent access to WARP internals; verify that the
installed public package exposes an interop API. Otherwise record that as a required
extension point.

## Example 2: Complex controls: ModernListView

There is no public `ModernListView` in this package. If you're tasks result into building
one. then do so from the existing control and element infrastructure rather than treating 
every item as a child HWND.

Recommended shape:

- derive the public control from `D2DControl`;
- assign a custom `D2DElementContainer` or `DocumentControlVisual` to `RootElement`;
- use `D2DVScrollbar` / `D2DHScrollbar` as composed hwnd-less elements;
- virtualize rows and columns: materialize and draw only the viewport plus a small
  overscan range;
- use `D2DElement` hit testing and input events for row/header interactions;
- keep selection, focus, keyboard navigation, edit state, and data binding in the
  control/model layer;
- publish an immutable render snapshot before using a background render mode;
- prefer `D2DSharedRenderThread` with
  `RenderBackgroundFramesOnDemand = true` for a large snapshot-driven list;
- use a retained `D2DVisual` only for coarse layers that move independently, such as
  a drag image or animated overlay—not for every cell;
- implement UI Automation/accessibility, DPI scaling, theming, keyboard conventions,
  and Designer behavior explicitly. Hwnd-less elements do not automatically become
  full WinForms accessible child controls.

`DocumentControlVisual` already supplies virtual surface extents, viewport transforms,
zoom, scroll offsets, and composed scrollbars. Reuse it when that document model fits;
do not copy its scrolling mechanics.

## Complex controls: PopupForm or PopupWindow

There is no public `D2DPopup`/`PopupWindow` abstraction in the package today. Choose
one of these supported compositions:

| Requirement | Approach |
|---|---|
| D2D popup with its own HWND | Owned, borderless `D2DForm` |
| Popup containing ordinary WinForms controls | Normal owned `Form` containing a `D2DPanel`/`D2DControl` where acceleration is needed |
| Overlay constrained to an existing D2D host | `D2DVisual` or `D2DElement` layer |
| All-DComp kiosk/fullscreen top-level | `D2DForm` with `UseDirectCompositionExclusiveRendering = true` |

Do not turn on DComp-exclusive form rendering when the popup contains classic child
HWNDs. They require a top-level redirection surface.

A production popup abstraction still owns concerns that rendering does not solve:
owner/activation behavior, focus restoration, keyboard dismissal, outside-click
dismissal, modality, monitor working-area placement, per-monitor DPI, shadows and
corners, topmost semantics, accessibility, and deterministic disposal. Keep these in
the window/control layer; keep drawing in the D2D render layer.

