using System.Globalization;
using Expenses.Core;

namespace Expenses.App.Views;

public sealed partial class AppearanceOptionsView : UserControl
{
    private AppPreferences? _draft;

    public AppearanceOptionsView() => InitializeComponent();

    public void LoadPreferences(AppPreferences draft)
    {
        _draft = draft;
        _size.SelectedIndex = draft.ToolbarIconSize switch
        {
            24 => 0, 36 => 1, 48 => 2,
            _ => throw new InvalidDataException("Icon size must be 24, 36, or 48 pixels.")
        };
        using var font = CreateFont(draft);
        ShowFont(font);
    }

    internal static Font CreateFont(AppPreferences preferences)
    {
        if (string.IsNullOrWhiteSpace(preferences.InterfaceFontFamily)
            || !float.IsFinite(preferences.InterfaceFontPoints) || preferences.InterfaceFontPoints is < 6 or > 32
            || preferences.InterfaceFontStyle is < 0 or > 3)
            throw new InvalidDataException("Choose a valid interface font between 6 and 32 points.");
        return new Font(preferences.InterfaceFontFamily, preferences.InterfaceFontPoints, (FontStyle)preferences.InterfaceFontStyle);
    }

    private void PickFont(object? sender, EventArgs e)
    {
        if (_draft is null) return;
        using var current = CreateFont(_draft);
        using var picker = new FontDialog { Font = current, MinSize = 6, MaxSize = 32, ShowEffects = false, FontMustExist = true };
        if (picker.ShowDialog(this) != DialogResult.OK) return;
        _draft.InterfaceFontFamily = picker.Font.FontFamily.Name;
        _draft.InterfaceFontPoints = picker.Font.SizeInPoints;
        _draft.InterfaceFontStyle = (int)picker.Font.Style;
        ShowFont(picker.Font);
    }

    private void ShowFont(Font font) =>
        _fontValue.Text = $"{font.FontFamily.Name}, {font.SizeInPoints.ToString("0.#", CultureInfo.CurrentCulture)} pt, {font.Style}";

    public void Apply()
    {
        if (_draft is null) return;
        _draft.ToolbarIconSize = _size.SelectedIndex switch
        {
            0 => 24, 1 => 36, 2 => 48, _ => throw new ArgumentException("Choose an icon size.")
        };
    }
}
