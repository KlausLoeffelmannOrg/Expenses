using System.Runtime.ExceptionServices;
using Expenses.App;
using Expenses.App.Dialogs;
using Expenses.Core;
using Xunit;

namespace Expenses.Tests;

public sealed class AppearanceTests
{
    [Theory]
    [InlineData(24, 9, 96, 24)]
    [InlineData(36, 9, 144, 54)]
    [InlineData(48, 18, 192, 192)]
    public void IconDemandScalesTheLogicalSizeWithFontAndDpiOnce(int size, float points, int dpi, int expected) =>
        Assert.Equal(expected, SymbolImageFactory.PixelSize(size, points, dpi));

    [Fact]
    public void AllCommandsRenderVisibleIndependentImages()
    {
        foreach (var symbol in Enum.GetValues<CommandSymbol>())
        {
            using var image = SymbolImageFactory.Create(symbol, Color.Black, 36);
            using var another = SymbolImageFactory.Create(symbol, Color.White, 36);
            Assert.NotSame(image, another);
            Assert.Contains(Enumerable.Range(0, image.Width).SelectMany(x =>
                Enumerable.Range(0, image.Height).Select(y => image.GetPixel(x, y))), c => c.A > 0);
            Assert.Equal(0, image.GetPixel(0, 0).A);
        }
    }

    [Fact]
    public void AppearanceIsADraftAndIconsRegenerateWhenTheFontChanges() => Sta(() =>
    {
        var preferences = new AppPreferences();
        using var options = new OptionsDialog(preferences);
        options.Draft.ToolbarIconSize = 48;
        options.Draft.InterfaceFontPoints = 18;
        options.DialogResult = DialogResult.Cancel;
        Assert.Equal(24, preferences.ToolbarIconSize);
        Assert.Equal(9, preferences.InterfaceFontPoints);
        using var form = new MainForm();
        form.Show();
        Application.DoEvents();
        var toolbar = form.Controls.OfType<ToolStrip>().Single(t => t.Name == "_toolbar");
        var previous = toolbar.Items[0].Image;
        Assert.NotNull(previous);
        Rectangle bounds = form.Bounds;
        using var larger = new Font(form.Font.FontFamily, 18);
        form.Font = larger;
        Assert.NotSame(previous, toolbar.Items[0].Image);
        Assert.Equal(bounds, form.Bounds);
        Assert.Equal(SymbolImageFactory.PixelSize(24, 18, form.DeviceDpi), toolbar.ImageScalingSize.Width);
        Assert.All(toolbar.Items.OfType<ToolStripButton>(), item =>
        {
            Assert.NotNull(item.Image);
            Assert.Equal(toolbar.ImageScalingSize, item.Image.Size);
            Assert.False(string.IsNullOrWhiteSpace(item.AccessibleName));
        });
        form.Close();
    });

    private static void Sta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() => { try { action(); } catch (Exception ex) { error = ex; } });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(45)));
        if (error is not null) ExceptionDispatchInfo.Capture(error).Throw();
    }
}
