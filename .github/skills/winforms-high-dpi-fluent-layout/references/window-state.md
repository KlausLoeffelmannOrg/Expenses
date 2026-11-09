# Normal bounds, first run, and fullscreen

Save at an accepted close: normal geometry as 96-DPI DIPs (see [DIP schema](#dip-schema)), and maximized as a separate flag. The normal rect is `Bounds` when `WindowState` is `Normal`, otherwise `RestoreBounds`. Never persist or restore minimized state. Restore the normal rect before maximized state. Persistence belongs to the application's storage policy, not mandatory `Properties.Settings`.

Do not enter fullscreen or apply saved bounds from the parameterless constructor. The Designer runs that constructor. Hiding chrome there hides the design surface.

## DIP schema

Store a schema version. Current geometry is version 2:

- Monitor identity: `Screen.DeviceName` plus a bounds signature (`Screen.Bounds` origin and size). DeviceName alone changes across dock and remote sessions.
- The `WorkingArea` rect, session `DeviceDpi`, and resolution at save time. Those are context, not the geometry unit.
- Normal geometry as 96-DPI DIPs relative to that monitor's `WorkingArea.Location`. Do not use `Screen.Bounds` as the origin and do not store absolute desktop pixels as DIPs. Offsets are monitor-local, so a negative virtual-screen origin does not have to be stored as a negative DIP. The window follows a taskbar move because the origin and the clamp target are the same rect.
- Maximized as a separate flag, as above.

```
localPixels = desktopPixels - workingArea.Location
dips = localPixels * 96 / dpiAtSave
```

`dpiAtSave` is the form's `DeviceDpi` while it was Normal on that monitor. Do not multiply absolute desktop coordinates by a DPI ratio.

These are three different operations. Do not mix them:

| Operation | Input | Do not use |
|---|---|---|
| Coordinate DIP conversion | Desktop pixels and monitor `DeviceDpi` only: subtract `WorkingArea.Location`, then `* 96 / dpi` | `Font.SizeInPoints`, font ratio, or `Font.Height` |
| Logical font preference | A point size you store yourself when the user picks a font (`_logicalFontPoints`). An explicit `Font` set clears the DPI font cache | The observed `Control.Font` after a DPI move |
| Observed font | After Per-Monitor V2 `WM_DPICHANGED`, `Control.Font` is already `ScaledControlFont` (`Size * newDpi / oldDpi`). Measure it with `Font.Height` or `TextRenderer` and no extra DPI factor | `Font.SizeInPoints * DeviceDpi / 96`, or `Font.GetHeight(DeviceDpi)` |

Icon pixels use the stored logical preference: `16 * logicalPoints / 10 * DeviceDpi / 96`. See [toolstrip icons](../../winforms-toolstrip-icons/SKILL.md).

Restore only after the final font and layout have settled. WinForms has no public per-screen DPI. Prefer `GetDpiForMonitor` (`shcore`, `MDT_EFFECTIVE_DPI`) on the monitor from `MonitorFromPoint` at the working-area center. Without that call, place, wait until `Shown`, read `DeviceDpi`, and if `WM_DPICHANGED` supplied a suggested rect, keep that rect. Then clamp the outer normal bounds into the current `WorkingArea`.

If DeviceName and the bounds signature both miss, use the primary `WorkingArea`, center a 75% by 80% Normal window, and write the new schema. Do not drop the old pixels onto the primary origin.

### Legacy pixels

A blob without this schema is not silently reread as DIPs. That includes WARP 0.9.852 `ApplyFormPlacement` / `SaveFormPlacement`, which store an absolute pixel `Rectangle` plus `FormWindowState` and only test intersection with `Screen.Bounds`. Migrate once when the saved DPI is known: subtract the matched `WorkingArea` origin, convert with that DPI, and write schema 2. If DPI is absent, re-seed with [first-run geometry](#first-run-geometry). Do not guess.

## First-run geometry

LOB main windows only. Do not apply these ratios to small modal tools or dialogs.

- Selected monitor: the owner's screen when there is an owner, otherwise the screen the product already chose, otherwise `Screen.PrimaryScreen`.
- Outer Normal size is about 75% of `WorkingArea.Width` and 80% of `WorkingArea.Height`, centered in that working area. The taskbar is excluded because the rect is `WorkingArea`, not `Bounds`.
- Convert deliberate DIP minimums at the target monitor DPI, then clamp so the window stays inside the working area. The ratio does not skip the minimum, and the minimum must not exceed the working area.

## Font-change transaction

`ContainerControl.OnFontChanged` runs `PerformAutoScale` before `base.OnFontChanged` when `AutoScaleMode` is not `None` and the handle exists. That scales the form's own size. `SuspendLayout` does not inhibit it. The scale runs inside the framework's own `SuspendAllLayout`, then `ResumeAllLayout(performLayout: false)`, so layout stays deferred when the setter returns. `FontChanged` subscribers run only after that scale. Do not snapshot from the event, and do not install a global hook.

The opt-in is an override on the top-level form, or a protected base form:

```csharp
public partial class MainWindow : Form
{
    private bool _inDpiTransition;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        _inDpiTransition = true;
        try
        {
            base.OnDpiChanged(e);
        }
        finally
        {
            _inDpiTransition = false;
        }
    }

    protected override void OnFontChanged(EventArgs e)
    {
        bool managedFont = !_inDpiTransition && IsHandleCreated;
        Rectangle normal = Bounds;
        FormWindowState state = WindowState;
        Size minimum = MinimumSize;
        Size maximum = MaximumSize;
        base.OnFontChanged(e);
        if (!managedFont || IsDisposed)
        {
            return;
        }

        MinimumSize = minimum;
        MaximumSize = maximum;
        if (state != FormWindowState.Normal)
        {
            return;
        }

        Bounds = normal;
        PerformLayout();
    }
}
```

`ScaleControl` clears and rescales `MinimumSize` and `MaximumSize` by the font factor. Put the pre-scale pixel values back. They are the current-DPI values from the DIP schema. Do not leave the font-scaled copies, and do not scale saved DIPs by the font ratio.

An explicit assignment can use the same snapshot. `SuspendLayout` does not stop AutoScale. `PerformAutoScale` still runs inside the setter. Restore, resume, and restore again if that layout grew the frame:

```csharp
Rectangle normal = Bounds;
FormWindowState state = WindowState;
Size minimum = MinimumSize;
Size maximum = MaximumSize;
SuspendLayout();
try
{
    Font = nextFont;
    MinimumSize = minimum;
    MaximumSize = maximum;
    if (state == FormWindowState.Normal)
    {
        Bounds = normal;
    }
}
finally
{
    ResumeLayout(true);
}

if (state == FormWindowState.Normal && Bounds != normal)
{
    MinimumSize = minimum;
    MaximumSize = maximum;
    Bounds = normal;
}
```

Restore minimum and maximum before bounds. A font-scaled `MinimumSize` can force a larger window even after `Bounds` is written back. Those snapshots are current-DPI pixels, not a second DIP conversion and not a font-ratio scale.

That wrapper misses inherited and system font changes. The override covers those. A later queued layout must not grow the outer bounds again.

Only `WindowState.Normal` is pixel-locked. While maximized, skip the bounds write. AutoScale also rescales `RestoreBounds`. There is no scratch-verified unscale for that rect. Do not invent one. Do not run this restore on hosted views. They use `AutoScaleMode.Inherit` and must grow with the host once. An explicit `Font` on a hosted `ContainerControl` is skipped by the parent's font scale and does not get `OnFontChanged` when only the host font changes, so it neither moves nor scales. An explicit `Font` on a non-container child can be bounds-scaled while its font stays.

## DPI suggested bounds

`WM_DPICHANGED` reaches `OnDpiChanged`, which calls `ScaleContainerForDpi` with `DpiChangedEventArgs.SuggestedRectangle` and raises `OnFontChanged` inside that call. Set `_inDpiTransition` before `base.OnDpiChanged`. Do not restore saved bounds there, do not set `e.Cancel` to keep the old physical size, and do not write persisted DIPs from that handler. Skipping the suggested rect can loop the message.

That order, and the already-scaled observed `Font`, are source findings. They are not a multi-monitor runtime proof. Posting a synthetic `WM_DPICHANGED` does not create one: `DeviceDpi` can stay at the old value, so the message is not evidence of a monitor move. Do not P/Invoke that message, fake UI DPI state, or skip a failing test. A real per-monitor transition remains unverified until the window actually moves between monitors with different DPI.

## Fullscreen

Store the previous `WindowState` and `FormBorderStyle` inside the method that enters fullscreen, before mutating them. Call it from a key handler, not from construction.

```csharp
public partial class PresentationForm : Form
{
    private bool _fullscreen;
    private Rectangle _normalBounds;
    private FormWindowState _previousState;
    private FormBorderStyle _previousBorder;

    public PresentationForm()
    {
        InitializeComponent();
    }

    public void SetFullscreen(bool enabled)
    {
        if (_fullscreen == enabled)
        {
            return;
        }

        if (enabled)
        {
            _normalBounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            _previousState = WindowState;
            _previousBorder = FormBorderStyle;
            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.None;
            Bounds = Screen.FromControl(this).Bounds;
        }
        else
        {
            FormBorderStyle = _previousBorder;
            StartPosition = FormStartPosition.Manual;
            WindowState = FormWindowState.Normal;
            Bounds = _normalBounds;
            WindowState = _previousState == FormWindowState.Minimized
                ? FormWindowState.Normal
                : _previousState;
        }

        _fullscreen = enabled;
    }
}
```

`PresentationForm.Designer.cs` owns `InitializeComponent`, serialized chrome, and the only `Dispose(bool)`. Call placement from a runtime load handler after the design-time guard in [async examples](../../winforms-async-apis/references/async-examples.md).
