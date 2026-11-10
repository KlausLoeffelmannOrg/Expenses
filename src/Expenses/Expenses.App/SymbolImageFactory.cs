using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace Expenses.App;

internal enum CommandSymbol { Add, Open, Note, Edit, Delete, Save, Cancel, Stash, Report, Export, Options, Exit, Demo, Foundry, Image }

internal static class SymbolImageFactory
{
    internal static int PixelSize(int logicalPixels, float logicalFontPoints, int dpi)
    {
        if (logicalPixels is not (24 or 36 or 48)) throw new ArgumentOutOfRangeException(nameof(logicalPixels));
        if (!float.IsFinite(logicalFontPoints) || logicalFontPoints is < 6 or > 32)
            throw new ArgumentOutOfRangeException(nameof(logicalFontPoints));
        if (dpi <= 0) throw new ArgumentOutOfRangeException(nameof(dpi));
        return checked((int)Math.Round(logicalPixels * logicalFontPoints / 9d * dpi / 96d));
    }

    internal static Bitmap Create(CommandSymbol symbol, Color color, int pixels)
    {
        if (pixels <= 0) throw new ArgumentOutOfRangeException(nameof(pixels));
        var (fluent, unicode) = symbol switch
        {
            CommandSymbol.Add => ('\uE710', '\u002B'),
            CommandSymbol.Open => ('\uE8B7', '\u25A1'),
            CommandSymbol.Note => ('\uE70B', '\u270E'),
            CommandSymbol.Edit => ('\uE70F', '\u270E'),
            CommandSymbol.Delete => ('\uE74D', '\u232B'),
            CommandSymbol.Save => ('\uE74E', '\u2713'),
            CommandSymbol.Cancel => ('\uE711', '\u2715'),
            CommandSymbol.Stash => ('\uE7B8', '\u25A3'),
            CommandSymbol.Report => ('\uE9D9', '\u25A4'),
            CommandSymbol.Export => ('\uEDE1', '\u2197'),
            CommandSymbol.Options => ('\uE713', '\u2699'),
            CommandSymbol.Exit => ('\uE8BB', '\u2192'),
            CommandSymbol.Demo => ('\uE8F1', '\u2605'),
            CommandSymbol.Foundry => ('\uE950', '\u2605'),
            CommandSymbol.Image => ('\uEB9F', '\u25A7'),
            _ => throw new ArgumentOutOfRangeException(nameof(symbol))
        };
        using var installed = new InstalledFontCollection();
        foreach (var (face, glyph) in new[] { ("Segoe Fluent Icons", fluent), ("Segoe UI Symbol", unicode), ("Segoe MDL2 Assets", fluent) })
        {
            if (!installed.Families.Any(f => f.Name.Equals(face, StringComparison.OrdinalIgnoreCase))) continue;
            using var font = new Font(face, pixels * .78f, FontStyle.Regular, GraphicsUnit.Pixel);
            if (!HasGlyph(font, glyph)) continue;
            var bitmap = new Bitmap(pixels, pixels, PixelFormat.Format32bppArgb);
            try
            {
                using var graphics = Graphics.FromImage(bitmap);
                using var brush = new SolidBrush(color);
                using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                graphics.Clear(Color.Transparent);
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                graphics.DrawString(glyph.ToString(), font, brush, new RectangleF(0, 0, pixels, pixels), format);
                return bitmap;
            }
            catch { bitmap.Dispose(); throw; }
        }
        throw new InvalidOperationException($"No installed Windows symbol font contains the icon for {symbol}.");
    }

    private static bool HasGlyph(Font font, char glyph)
    {
        nint dc = CreateCompatibleDC(0);
        if (dc == 0) throw new System.ComponentModel.Win32Exception();
        nint handle = font.ToHfont();
        nint previous = SelectObject(dc, handle);
        try
        {
            ushort[] indices = new ushort[1];
            uint result = GetGlyphIndices(dc, glyph.ToString(), 1, indices, 1);
            if (result == uint.MaxValue) throw new System.ComponentModel.Win32Exception();
            return indices[0] != ushort.MaxValue && indices[0] != 0;
        }
        finally
        {
            SelectObject(dc, previous);
            DeleteObject(handle);
            DeleteDC(dc);
        }
    }

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern nint CreateCompatibleDC(nint dc);
    [DllImport("gdi32.dll")]
    private static extern nint SelectObject(nint dc, nint value);
    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(nint value);
    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteDC(nint dc);
    [DllImport("gdi32.dll", EntryPoint = "GetGlyphIndicesW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetGlyphIndices(nint dc, string text, int length, [Out] ushort[] glyphs, uint flags);
}
