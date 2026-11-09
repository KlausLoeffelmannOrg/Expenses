using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Expenses.Tests.Scratch;

public sealed partial class FontScalingScratchForm : Form
{
    private Bitmap? _ownedIcon;
    private bool _changingFont;
    private float _logicalFontPoints = 10;
    private bool _updatingViewport;

    public FontScalingScratchForm()
    {
        InitializeComponent();
        Disposed += DisposeIcon;
    }

    public HostedScalingScratchView View => _view;
    public ToolStrip Toolbar => _toolbar;
    public TableLayoutPanel Root => _root;
    public Panel Viewport => _viewport;
    public TableLayoutPanel OuterButtons => _outerButtons;
    public TableLayoutPanel InnerButtons => _innerButtons;
    public Button OkButton => _ok;
    public Button ExitButton => _cancel;

    public void SetInitialUiFont(Font font)
    {
        ArgumentNullException.ThrowIfNull(font);
        if (IsHandleCreated)
            throw new InvalidOperationException("Set the startup font before creating the window handle.");
        _logicalFontPoints = font.SizeInPoints;
        Font = font;
    }

    public void ChangeUiFont(Font font)
    {
        ArgumentNullException.ThrowIfNull(font);
        if (!IsHandleCreated || WindowState != FormWindowState.Normal)
            throw new InvalidOperationException("The scratch transaction requires a shown normal window.");

        Rectangle bounds = Bounds;
        Size minimum = MinimumSize;
        Size maximum = MaximumSize;
        _changingFont = true;
        SuspendLayout();
        try
        {
            _logicalFontPoints = font.SizeInPoints;
            Font = font;
            PerformAutoScale();
            MinimumSize = minimum;
            MaximumSize = maximum;
            Bounds = bounds;
        }
        finally
        {
            try
            {
                ResumeLayout(true);
            }
            finally
            {
                _changingFont = false;
                MinimumSize = minimum;
                MaximumSize = maximum;
                Bounds = bounds;
            }
        }
        RefreshIcon();
        UpdateViewport();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        RefreshIcon();
        UpdateViewport();
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        if (IsHandleCreated && !_changingFont) UpdateViewport();
    }

    private void UpdateViewport()
    {
        if (_updatingViewport) return;
        _updatingViewport = true;
        try
        {
            Size preferred = _view.GetPreferredSize(new Size(_viewport.ClientSize.Width, 0));
            if (_viewport.AutoScrollMinSize != preferred) _viewport.AutoScrollMinSize = preferred;
        }
        finally
        {
            _updatingViewport = false;
        }
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        if (IsHandleCreated && !_changingFont) RefreshIcon();
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);
        RefreshIcon();
    }

    private void RefreshIcon()
    {
        // Per-Monitor V2 may already DPI-scale Font.SizeInPoints.
        Size pixels = ImagePixels(_logicalFontPoints, DeviceDpi);
        Bitmap replacement = CreateCheckmark(pixels, _toolbar.ForeColor);
        Bitmap? previous = _ownedIcon;
        _ownedIcon = replacement;
        _acceptCommand.ImageScaling = ToolStripItemImageScaling.None;
        _toolbar.ImageScalingSize = pixels;
        _acceptCommand.Image = replacement;
        previous?.Dispose();
    }

    private void DisposeIcon(object? sender, EventArgs e)
    {
        _acceptCommand.Image = null;
        _ownedIcon?.Dispose();
        _ownedIcon = null;
    }

    internal static Size ImagePixels(float logicalFontPoints, int dpi)
    {
        if (!float.IsFinite(logicalFontPoints) || logicalFontPoints <= 0)
            throw new ArgumentOutOfRangeException(nameof(logicalFontPoints));
        if (dpi <= 0) throw new ArgumentOutOfRangeException(nameof(dpi));
        int pixels = checked((int)Math.Round(16d * logicalFontPoints / 10d * dpi / 96d));
        return new Size(Math.Max(1, pixels), Math.Max(1, pixels));
    }

    private static Bitmap CreateCheckmark(Size size, Color color)
    {
        using var installed = new InstalledFontCollection();
        if (!installed.Families.Any(f => f.Name == "Segoe UI Symbol"))
            throw new InvalidOperationException("The scratch icon needs the Windows Segoe UI Symbol font.");

        var bitmap = new Bitmap(size.Width, size.Height);
        try
        {
            using Graphics graphics = Graphics.FromImage(bitmap);
            using var font = new Font("Segoe UI Symbol", size.Height * .8f, FontStyle.Regular, GraphicsUnit.Pixel);
            using var brush = new SolidBrush(color);
            using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            graphics.Clear(Color.Transparent);
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.DrawString("\u2713", font, brush, new RectangleF(0, 0, size.Width, size.Height), format);
            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }
}

internal sealed record ScratchWindowPlacement(
    int SchemaVersion,
    string MonitorId,
    int SessionDpi,
    Rectangle SessionMonitorBounds,
    Rectangle SessionWorkingArea,
    RectangleF NormalBoundsDip,
    bool Maximized)
{
    internal static ScratchWindowPlacement Capture(
        Rectangle normalBounds, string monitorId, int dpi, Rectangle monitorBounds,
        Rectangle workingArea, bool maximized)
    {
        Validate(dpi, workingArea);
        if (normalBounds.Width <= 0 || normalBounds.Height <= 0 || string.IsNullOrWhiteSpace(monitorId))
            throw new ArgumentException("A placement needs positive normal bounds and a monitor identity.");
        float factor = 96f / dpi;
        return new(1, monitorId, dpi, monitorBounds, workingArea,
            new RectangleF((normalBounds.Left - workingArea.Left) * factor,
                (normalBounds.Top - workingArea.Top) * factor,
                normalBounds.Width * factor, normalBounds.Height * factor), maximized);
    }

    internal Rectangle Restore(Rectangle workingArea, int targetDpi)
    {
        Validate(targetDpi, workingArea);
        if (SchemaVersion != 1)
            throw new NotSupportedException("Legacy pixel placement needs an explicit migration.");
        double factor = targetDpi / 96d;
        int width = Math.Clamp((int)Math.Round(NormalBoundsDip.Width * factor), 1, workingArea.Width);
        int height = Math.Clamp((int)Math.Round(NormalBoundsDip.Height * factor), 1, workingArea.Height);
        int x = workingArea.Left + (int)Math.Round(NormalBoundsDip.X * factor);
        int y = workingArea.Top + (int)Math.Round(NormalBoundsDip.Y * factor);
        return new Rectangle(Math.Clamp(x, workingArea.Left, workingArea.Right - width),
            Math.Clamp(y, workingArea.Top, workingArea.Bottom - height), width, height);
    }

    internal static Rectangle FirstRun(Rectangle workingArea)
    {
        Validate(96, workingArea);
        int width = Math.Max(1, (int)Math.Round(workingArea.Width * .75));
        int height = Math.Max(1, (int)Math.Round(workingArea.Height * .80));
        return new Rectangle(workingArea.Left + (workingArea.Width - width) / 2,
            workingArea.Top + (workingArea.Height - height) / 2, width, height);
    }

    private static void Validate(int dpi, Rectangle workingArea)
    {
        if (dpi <= 0 || workingArea.Width <= 0 || workingArea.Height <= 0)
            throw new ArgumentOutOfRangeException(nameof(dpi));
    }
}
