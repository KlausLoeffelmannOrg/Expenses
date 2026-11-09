<!-- Reference of warp-directx-internal-approaches. Render controls (D2DControl/D2DPanel subclasses) are render-only surfaces and are the one exception to the Designer-pair rule; D2DForm hosts are not. -->

# Static scenes and animated loops

## Static scene that must not be wiped away

For a static panel background, use `D2DPanel.RenderBackground`. The panel redraws it
only when dirty, and DirectComposition keeps presenting the last result:

```csharp
internal sealed class StaticScenePanel : D2DPanel
{
    private Color _accent = Color.CornflowerBlue;

    public StaticScenePanel()
    {
        RenderMode = RenderMode.D2DWinFormsClassic;
        PreserveLastFrame = true;
        RenderBackground += DrawStaticScene;
    }

    public Color Accent
    {
        get => _accent;
        set
        {
            if (_accent == value)
            {
                return;
            }

            _accent = value;
            InvalidateBackground();
        }
    }

    private void DrawStaticScene(object? sender, D2DRenderEventArgs e)
    {
        e.Graphics.FillRectangle(BackColor, ClientRectangle);
        e.Graphics.FillEllipse(_accent, new RectangleF(24, 24, 96, 96));
    }
}
```

Use `InvalidateBackground()` only when the static content changes. Keep dynamic
foreground content in a child D2D control or separate retained visual. Redrawing a
foreground into the panel's immediate swap chain every frame requires repainting the
whole frame because flip-discard back-buffer contents are not a general scene store.

For a static layer that must remain while other layers animate, create a
`D2DVisual`, draw its content in `PaintContent`, and invalidate that content only
when its appearance changes:

```csharp
D2DVisual background = Visuals.AddNew(ClientRectangle);
background.PaintContent += (_, e) =>
{
    e.Graphics.FillRectangle(Color.FromArgb(24, 24, 28),
        new RectangleF(PointF.Empty, e.ContentSize));
};
```

`PaintContent` is raised on the correct render thread and is the portable path for all
render modes. The framework re-requests it after creation, resize, mode changes, and
device loss. Call `background.InvalidateContent()` when the data it draws changes.

## Game-like or continuously animated loop

Use a background render mode and draw a complete frame from an immutable snapshot:

```csharp
internal sealed class GameSurface : D2DControl
{
    private SceneSnapshot _scene = SceneSnapshot.Empty;

    public GameSurface()
    {
        RenderMode = RenderMode.D2DSharedRenderThread;
        RenderBackgroundFramesOnDemand = false;
        PreserveLastFrame = false;
        VSyncEnabled = true;
        Render += DrawFrame;
    }

    public void Publish(SceneSnapshot scene)
        => Volatile.Write(ref _scene, scene);

    private void DrawFrame(object? sender, D2DRenderEventArgs e)
    {
        SceneSnapshot scene = Volatile.Read(ref _scene);
        e.Graphics.Clear(Color.Black);
        scene.Draw(e.Graphics, e.FrameDelta);
    }
}
```

Use `e.FrameDelta` or `D2DSharedRenderClock.Frame` for time-based simulation. The
shared clock event runs once before its hosts render, which is useful when several
surfaces must observe one consistent simulation step.

Choose loop settings deliberately:

- `PreserveLastFrame = false`: every tick begins from `BackColor`; appropriate for a
  full-frame game renderer.
- `PreserveLastFrame = true`: a frame with no drawing leaves the prior presentation
  visible.
- `RenderBackgroundFramesOnDemand = true`: background loops sleep until invalidated;
  ideal for snapshot-driven controls, not continuous games.
- `VSyncEnabled = true`: present at vblank.
- `VSyncEnabled = false` plus `TargetFrameRate`: timer-paced rendering.

For retained animation, update `D2DVisual.Bounds`, `Visible`, or `Transform`, then
batch with one `CommitVisualsAsync` per logical frame. Do not redraw content merely to
move or rotate it.

