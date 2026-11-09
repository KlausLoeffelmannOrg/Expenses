using System.Runtime.ExceptionServices;
using System.Text.Json;
using Expenses.Tests.Scratch;
using Xunit;

namespace Expenses.Tests;

public sealed class FontScalingScratchTests
{
    private static void Sta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { error = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(45)), "Scratch layout check did not finish.");
        if (error is not null) ExceptionDispatchInfo.Capture(error).Throw();
    }

    [Fact]
    public void FontTransactionsScaleHostedContentButPreserveOuterBoundsAndRebuildImages() => Sta(() =>
    {
        using var initialFont = new Font("Segoe UI", 10);
        using var largerFont = new Font("Segoe UI", 12);
        using var largestFont = new Font("Segoe UI", 18);
        using var form = new FontScalingScratchForm();
        form.SetInitialUiFont(initialFont);
        form.Show();
        form.Bounds = new Rectangle(80, 80, 820, 620);
        Application.DoEvents();
        Rectangle bounds = form.Bounds;
        int initialHeight = form.View.Input.Height;
        Image original = Assert.IsAssignableFrom<Image>(form.Toolbar.Items[0].Image);

        form.ChangeUiFont(largerFont);
        Application.DoEvents();
        Assert.Equal(bounds, form.Bounds);
        Assert.True(form.View.Input.Height > initialHeight);
        Assert.Equal(largerFont.SizeInPoints, form.View.Input.Font.SizeInPoints);
        Assert.InRange(form.View.Input.Height, form.View.Input.PreferredHeight, form.View.Input.PreferredHeight + 1);
        Assert.NotSame(original, form.Toolbar.Items[0].Image);
        Image largerImage = Assert.IsAssignableFrom<Image>(form.Toolbar.Items[0].Image);
        Assert.Equal(FontScalingScratchForm.ImagePixels(12, form.DeviceDpi), largerImage.Size);
        Bitmap bitmap = Assert.IsType<Bitmap>(largerImage);
        Assert.Contains(Enumerable.Range(0, bitmap.Width).SelectMany(x =>
            Enumerable.Range(0, bitmap.Height).Select(y => bitmap.GetPixel(x, y))), color => color.A > 0);

        form.ChangeUiFont(largestFont);
        Application.DoEvents();
        Assert.Equal(bounds, form.Bounds);
        Assert.Equal(FontScalingScratchForm.ImagePixels(18, form.DeviceDpi), form.Toolbar.ImageScalingSize);
        Assert.Equal(AutoScaleMode.Inherit, form.View.AutoScaleMode);
        int settledHeight = form.View.Input.Height;
        for (int i = 0; i < 5; i++) form.PerformLayout();
        Application.DoEvents();
        Assert.Equal(bounds, form.Bounds);
        Assert.Equal(settledHeight, form.View.Input.Height);
        form.Close();
    });

    [Fact]
    public void StartupRestoresScreenGeometryAfterTheNewFontHasSettled() => Sta(() =>
    {
        using var oldFont = new Font("Segoe UI", 10);
        using var newFont = new Font("Segoe UI", 12);
        using var previous = new FontScalingScratchForm();
        previous.SetInitialUiFont(oldFont);
        previous.Show();
        previous.Bounds = new Rectangle(80, 80, 820, 620);
        Application.DoEvents();
        Screen monitor = Screen.FromControl(previous);
        var placement = ScratchWindowPlacement.Capture(previous.Bounds, monitor.DeviceName,
            previous.DeviceDpi, monitor.Bounds, monitor.WorkingArea, false);
        previous.Close();

        using var next = new FontScalingScratchForm();
        next.SetInitialUiFont(newFont);
        next.Show();
        Rectangle expected = placement.Restore(monitor.WorkingArea, next.DeviceDpi);
        next.Bounds = expected;
        for (int i = 0; i < 5; i++) next.PerformLayout();
        Application.DoEvents();
        Assert.Equal(expected, next.Bounds);
        Assert.Equal(newFont.SizeInPoints, next.View.Input.Font.SizeInPoints);
        Assert.InRange(next.View.Input.Height, next.View.Input.PreferredHeight, next.View.Input.PreferredHeight + 1);
        next.Close();
    });

    [Fact]
    public void FontTransactionDoesNotScaleTopLevelPixelConstraints() => Sta(() =>
    {
        using var oldFont = new Font("Segoe UI", 10);
        using var newFont = new Font("Segoe UI", 12);
        using var form = new FontScalingScratchForm();
        form.SetInitialUiFont(oldFont);
        form.Show();
        form.MinimumSize = new Size(760, 560);
        form.MaximumSize = new Size(1600, 1000);
        form.Bounds = new Rectangle(80, 80, 820, 620);
        Application.DoEvents();
        Rectangle bounds = form.Bounds;
        form.ChangeUiFont(newFont);
        Application.DoEvents();
        Assert.Equal(bounds, form.Bounds);
        Assert.Equal(new Size(760, 560), form.MinimumSize);
        Assert.Equal(new Size(1600, 1000), form.MaximumSize);
        form.Close();
    });

    [Fact]
    public void NestedButtonFooterKeepsEqualWidthsAndReachableActionsAcrossFontAndResize() => Sta(() =>
    {
        using var font = new Font("Segoe UI", 16);
        using var form = new FontScalingScratchForm();
        form.Show();
        form.Bounds = new Rectangle(80, 80, 850, 620);
        form.ChangeUiFont(font);
        form.ExitButton.Text = "Cancel and return\nwithout saving changes";
        form.PerformLayout();
        Application.DoEvents();
        foreach (Size size in new[] { new Size(850, 620), new Size(700, 520) })
        {
            form.Size = size;
            form.PerformLayout();
            Application.DoEvents();
            Assert.InRange(Math.Abs(form.OkButton.Width - form.ExitButton.Width), 0, 1);
            Assert.True(form.OkButton.Height >= form.OkButton.GetPreferredSize(Size.Empty).Height);
            Assert.True(form.ExitButton.Height >= form.ExitButton.GetPreferredSize(Size.Empty).Height);
            Assert.True(form.InnerButtons.Bounds.Bottom <= form.OuterButtons.ClientSize.Height);
            Rectangle footer = form.Root.GetCellPosition(form.OuterButtons).Row == 2
                ? form.OuterButtons.Bounds : Rectangle.Empty;
            Assert.True(footer.Bottom <= form.Root.ClientSize.Height);
            Assert.True(form.Viewport.Bounds.Bottom <= footer.Top,
                $"Viewport {form.Viewport.Bounds}, preferred {form.View.GetPreferredSize(Size.Empty)}, " +
                $"footer {footer}, rows {string.Join(",", form.Root.GetRowHeights())}, window {form.Size}.");
            Assert.All(form.Root.GetRowHeights(), height => Assert.True(height >= 0));
            Assert.Equal(SizeType.Percent, form.Root.RowStyles[1].SizeType);
            form.Viewport.ScrollControlIntoView(form.View.Choice);
            Rectangle choice = form.Viewport.RectangleToClient(
                form.View.Choice.RectangleToScreen(form.View.Choice.ClientRectangle));
            Assert.True(choice.Top >= 0 && choice.Bottom <= form.Viewport.ClientSize.Height);
        }
        form.Close();
    });

    [Theory]
    [InlineData(96, 144)]
    [InlineData(144, 192)]
    [InlineData(192, 96)]
    public void MonitorLocalDipsSurviveJsonAndRestoreWithoutAnyFontFactor(int savedDpi, int targetDpi)
    {
        var oldArea = new Rectangle(-2560, 40, 2560, 1400);
        var normal = new Rectangle(-2260, 140, 1200, 900);
        var saved = ScratchWindowPlacement.Capture(normal, "DISPLAY-LEFT", savedDpi,
            new Rectangle(-2560, 0, 2560, 1440), oldArea, true);
        var restored = Assert.IsType<ScratchWindowPlacement>(
            JsonSerializer.Deserialize<ScratchWindowPlacement>(JsonSerializer.Serialize(saved)));
        var targetArea = new Rectangle(1920, 0, 3840, 2100);
        Rectangle bounds = restored.Restore(targetArea, targetDpi);
        double factor = targetDpi / (double)savedDpi;
        Assert.InRange(Math.Abs(bounds.Left - (targetArea.Left + 300 * factor)), 0, 1);
        Assert.InRange(Math.Abs(bounds.Top - (targetArea.Top + 100 * factor)), 0, 1);
        Assert.InRange(Math.Abs(bounds.Width - 1200 * factor), 0, 1);
        Assert.InRange(Math.Abs(bounds.Height - 900 * factor), 0, 1);
        Assert.True(restored.Maximized);
        Assert.Equal(saved.SessionWorkingArea, restored.SessionWorkingArea);
    }

    [Fact]
    public void FirstRunExcludesTaskbarAndRestoredBoundsFitReplacementMonitor()
    {
        var area = new Rectangle(-1920, 0, 1920, 1040);
        Rectangle first = ScratchWindowPlacement.FirstRun(area);
        Assert.Equal(1440, first.Width);
        Assert.Equal(832, first.Height);
        Assert.Equal(new Point(-1680, 104), first.Location);
        var saved = ScratchWindowPlacement.Capture(new Rectangle(-3000, -400, 2400, 1800),
            "MISSING-MONITOR", 96, new Rectangle(-3200, 0, 3200, 1800),
            new Rectangle(-3200, 0, 3200, 1760), false);
        var replacement = new Rectangle(0, 48, 1280, 720);
        Rectangle bounds = saved.Restore(replacement, 144);
        Assert.Equal(replacement, bounds);
        Assert.Throws<NotSupportedException>(() => (saved with { SchemaVersion = 0 }).Restore(replacement, 96));
    }

    [Fact]
    public void InputMinimaIgnoreCurrentTextAndDoNotAccumulateAcrossLayoutPasses() => Sta(() =>
    {
        using var font = new Font("Segoe UI", 10);
        using var form = new FontScalingScratchForm();
        form.SetInitialUiFont(font);
        form.Show();
        Control[] inputs = [form.View.Input, form.View.Number, form.View.DateInput, form.View.Choice];
        var nativeEdit = Assert.Single(form.View.Number.Controls.OfType<TextBoxBase>());
        Size nativeMinimum = nativeEdit.MinimumSize;
        EditorWidths.Apply(form.View);
        int[] baseline = inputs.Select(input => input.MinimumSize.Width).ToArray();

        form.View.Input.Text = new string('W', 200);
        form.View.Number.Value = 100;
        form.View.DateInput.Text = "12/31/2026";
        form.View.Choice.SelectedIndex = 1;
        for (int i = 0; i < 8; i++)
        {
            form.PerformLayout();
            EditorWidths.Apply(form.View);
        }
        Assert.Equal(baseline, inputs.Select(input => input.MinimumSize.Width).ToArray());
        Assert.Equal(nativeMinimum, nativeEdit.MinimumSize);

        form.View.Input.Clear();
        form.View.DateInput.Clear();
        form.View.Number.Value = 1;
        form.View.Choice.SelectedIndex = -1;
        EditorWidths.Apply(form.View);
        Assert.Equal(baseline, inputs.Select(input => input.MinimumSize.Width).ToArray());
        form.Close();
    });

    [Fact]
    public void InputMinimaRecalculateForSettledFontWithoutGrowingTheWindow() => Sta(() =>
    {
        using var oldFont = new Font("Segoe UI", 10);
        using var newFont = new Font("Segoe UI", 18);
        using var form = new FontScalingScratchForm();
        form.SetInitialUiFont(oldFont);
        form.Show();
        form.Bounds = new Rectangle(80, 80, 900, 680);
        Control[] inputs = [form.View.Input, form.View.Number, form.View.DateInput, form.View.Choice];
        EditorWidths.Apply(form.View);
        int[] baseline = inputs.Select(input => input.MinimumSize.Width).ToArray();
        Rectangle bounds = form.Bounds;

        form.ChangeUiFont(newFont);
        EditorWidths.Apply(form.View);
        form.PerformLayout();
        Application.DoEvents();
        int[] larger = inputs.Select(input => input.MinimumSize.Width).ToArray();
        for (int i = 0; i < inputs.Length; i++) Assert.True(larger[i] > baseline[i]);
        int capacity = TextRenderer.MeasureText("WWWW", form.View.Number.Font,
            Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width;
        Assert.True(form.View.Number.MinimumSize.Width >= capacity);
        for (int i = 0; i < 8; i++)
        {
            EditorWidths.Apply(form.View);
            form.PerformLayout();
        }
        Application.DoEvents();
        Assert.Equal(larger, inputs.Select(input => input.MinimumSize.Width).ToArray());
        Assert.Equal(bounds, form.Bounds);
        form.Close();
    });

    [Fact]
    public void InputMinimaReserveTheWholeMaskAndAcceptFullDecimalRange() => Sta(() =>
    {
        using var font = new Font("Segoe UI", 10);
        using var form = new FontScalingScratchForm();
        form.SetInitialUiFont(font);
        form.Show();
        form.View.DateInput.Mask = "00000000000000000000.00";
        form.View.Number.Minimum = decimal.MinValue;
        form.View.Number.Maximum = decimal.MaxValue;
        EditorWidths.Apply(form.View);
        int maskCapacity = TextRenderer.MeasureText(
            new string('W', form.View.DateInput.Mask.Length), form.View.DateInput.Font,
            Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width;
        Assert.True(form.View.DateInput.MinimumSize.Width >= maskCapacity);
        Assert.True(form.View.Number.MinimumSize.Width > 0);
        form.Close();
    });

    [Theory]
    [InlineData(10, 96, 16)]
    [InlineData(12, 144, 29)]
    [InlineData(18, 192, 58)]
    public void IconPixelDemandUsesStableFontAndDpiBaselinesExactlyOnce(float points, int dpi, int pixels)
    {
        Assert.Equal(new Size(pixels, pixels), FontScalingScratchForm.ImagePixels(points, dpi));
    }
}
