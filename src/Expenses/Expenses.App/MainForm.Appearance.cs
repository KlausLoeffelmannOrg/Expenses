using Expenses.App.Views;
using Microsoft.Win32;

namespace Expenses.App;

public partial class MainForm
{
    private readonly Dictionary<ToolStripItem, Image> _ownedImages = [];
    private Font? _ownedFont;
    private bool _changingDpi;
    private bool _applyingAppearance;
    private bool _imagesReady;

    private IEnumerable<(ToolStripItem Item, CommandSymbol Symbol)> IconCommands =>
    [
        (_fileMenu, CommandSymbol.Open), (_editMenu, CommandSymbol.Edit), (_viewMenu, CommandSymbol.Report),
        (_toolsMenu, CommandSymbol.Options), (_newPeriod, CommandSymbol.Add), (_openPeriod, CommandSymbol.Open),
        (_exportCsv, CommandSymbol.Export), (_exportPdf, CommandSymbol.Export), (_exit, CommandSymbol.Exit),
        (_newExpense, CommandSymbol.Add), (_newButton, CommandSymbol.Add),
        (_addReminder, CommandSymbol.Note), (_noteButton, CommandSymbol.Note),
        (_editExpense, CommandSymbol.Edit), (_editButton, CommandSymbol.Edit),
        (_deleteExpense, CommandSymbol.Delete), (_deleteButton, CommandSymbol.Delete),
        (_saveExpense, CommandSymbol.Save), (_saveButton, CommandSymbol.Save),
        (_cancelEdit, CommandSymbol.Cancel), (_cancelButton, CommandSymbol.Cancel),
        (_stashDraft, CommandSymbol.Stash), (_stashButton, CommandSymbol.Stash),
        (_periodReport, CommandSymbol.Report), (_annualReport, CommandSymbol.Report),
        (_allReminders, CommandSymbol.Note), (_options, CommandSymbol.Options), (_demoData, CommandSymbol.Demo),
        (_foundryMenu, CommandSymbol.Foundry), (_manageFoundryModels, CommandSymbol.Options), (_importReceipt, CommandSymbol.Image)
    ];

    private void ApplyAppearance()
    {
        Font replacement = AppearanceOptionsView.CreateFont(_preferences);
        var old = _ownedFont;
        var bounds = Bounds;
        var minimum = MinimumSize;
        var maximum = MaximumSize;
        bool preserve = _loaded && WindowState == FormWindowState.Normal;
        _applyingAppearance = true;
        SuspendLayout();
        try
        {
            Font = replacement;
            _ownedFont = replacement;
            PerformAutoScale();
            MinimumSize = minimum;
            MaximumSize = maximum;
            if (preserve) Bounds = bounds;
        }
        finally
        {
            ResumeLayout(true);
            _applyingAppearance = false;
            MinimumSize = minimum;
            MaximumSize = maximum;
            if (preserve) Bounds = bounds;
            old?.Dispose();
        }
        if (_imagesReady) RefreshCommandImages();
    }

    protected override void OnFontChanged(EventArgs e)
    {
        var bounds = Bounds;
        var minimum = MinimumSize;
        var maximum = MaximumSize;
        bool preserve = _imagesReady && !_changingDpi && WindowState == FormWindowState.Normal;
        if (preserve) SuspendLayout();
        try { base.OnFontChanged(e); }
        finally
        {
            if (preserve)
            {
                MinimumSize = minimum;
                MaximumSize = maximum;
                Bounds = bounds;
                ResumeLayout(true);
                MinimumSize = minimum;
                MaximumSize = maximum;
                Bounds = bounds;
            }
        }
        if (_imagesReady && !_changingDpi && !_applyingAppearance)
        {
            _preferences.InterfaceFontFamily = Font.FontFamily.Name;
            _preferences.InterfaceFontPoints = Font.SizeInPoints;
            _preferences.InterfaceFontStyle = (int)Font.Style;
            RefreshCommandImages();
        }
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        _changingDpi = true;
        try { base.OnDpiChanged(e); }
        finally { _changingDpi = false; }
        if (_imagesReady) RefreshCommandImages();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _imagesReady = true;
        Execute(RefreshCommandImages);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (!DesignMode) SystemEvents.UserPreferenceChanged += SystemPreferenceChanged;
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        SystemEvents.UserPreferenceChanged -= SystemPreferenceChanged;
        base.OnHandleDestroyed(e);
    }

    private void SystemPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (!IsHandleCreated || IsDisposed) return;
        _ = InvokeAsync(() => { if (!IsDisposed && _imagesReady) Execute(RefreshCommandImages); });
    }

    private void RefreshCommandImages()
    {
        int pixels = SymbolImageFactory.PixelSize(_preferences.ToolbarIconSize, _preferences.InterfaceFontPoints, DeviceDpi);
        _toolbar.ImageScalingSize = _menu.ImageScalingSize = new Size(pixels, pixels);
        foreach (var (item, symbol) in IconCommands)
        {
            var image = SymbolImageFactory.Create(symbol, item.ForeColor, pixels);
            _ownedImages.Remove(item, out var previous);
            item.ImageScaling = ToolStripItemImageScaling.None;
            item.Image = image;
            _ownedImages.Add(item, image);
            item.AccessibleName = item.Text?.Replace("&", "").Replace("...", "");
            if (item is ToolStripButton)
                item.DisplayStyle = ToolStripItemDisplayStyle.Image;
            if (item is ToolStripMenuItem menu) menu.DropDown.ImageScalingSize = new Size(pixels, pixels);
            previous?.Dispose();
        }
    }

    private void DisposeAppearance()
    {
        foreach (var (item, image) in _ownedImages)
        {
            item.Image = null;
            image.Dispose();
        }
        _ownedImages.Clear();
        _ownedFont?.Dispose();
    }
}
