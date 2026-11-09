using System.ComponentModel;
using System.Globalization;
using System.Runtime.ExceptionServices;
using Expenses.App.Views;
using Expenses.Core;
using Xunit;

namespace Expenses.Tests;

[CollectionDefinition("View designer context", DisableParallelization = true)]
public sealed class ViewDesignerContextCollection;

[Collection("View designer context")]
public sealed class ViewDesignerUiTests
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
        Assert.True(thread.Join(TimeSpan.FromSeconds(45)), "View UI check did not finish.");
        if (error is not null) ExceptionDispatchInfo.Capture(error).Throw();
    }

    private static IEnumerable<Control> Descendants(Control parent) =>
        parent.Controls.Cast<Control>().SelectMany(c => new[] { c }.Concat(Descendants(c)));

    private sealed class DesignLicenseContext : LicenseContext
    {
        public override LicenseUsageMode UsageMode => LicenseUsageMode.Designtime;
    }

    [Fact]
    public void ParameterlessOptionsConstructWithoutDraftAndRequireExplicitRuntimeBinding() => Sta(() =>
    {
        using var storage = new StorageOptionsView();
        using var reminders = new ReminderOptionsView();
        Assert.All(Descendants(storage).OfType<TextBox>(), box => Assert.Empty(box.Text));
        Assert.Throws<InvalidOperationException>(storage.Apply);
        Assert.Throws<InvalidOperationException>(reminders.Apply);
        var draft = new AppPreferences
        {
            SettingsFolder = @"C:\DraftSettings",
            DatabaseFolder = @"C:\DraftData",
            BackupFolder = "",
            ShowRemindersOnStartup = true
        };
        storage.LoadPreferences(draft);
        reminders.LoadPreferences(draft);
        Assert.Contains(Descendants(storage).OfType<TextBox>(), b => b.Text == draft.DatabaseFolder);
        Assert.True(Descendants(reminders).OfType<CheckBox>().Single().Checked);
    });

    [Fact]
    public void OptionsEditingIsDraftOnlyAndInvalidApplyDoesNotPartiallyMutateDraft() => Sta(() =>
    {
        var live = new AppPreferences
        {
            SettingsFolder = @"C:\OriginalSettings",
            DatabaseFolder = @"C:\OriginalData",
            BackupFolder = "",
            ShowRemindersOnStartup = true
        };
        var draft = new AppPreferences
        {
            SettingsFolder = live.SettingsFolder,
            DatabaseFolder = live.DatabaseFolder,
            BackupFolder = live.BackupFolder,
            ShowRemindersOnStartup = live.ShowRemindersOnStartup
        };
        using var storage = new StorageOptionsView(draft);
        using var reminders = new ReminderOptionsView(draft);
        var settings = Descendants(storage).OfType<TextBox>().Single(b => b.AccessibleName == "App settings folder");
        var database = Descendants(storage).OfType<TextBox>().Single(b => b.AccessibleName == "Database folder");
        settings.Text = @"C:\ChangedSettings";
        database.Text = "relative";
        Descendants(reminders).OfType<CheckBox>().Single().Checked = false;
        Assert.Equal(live.SettingsFolder, draft.SettingsFolder);
        Assert.True(draft.ShowRemindersOnStartup);
        Assert.Throws<ArgumentException>(storage.Apply);
        Assert.Equal(live.SettingsFolder, draft.SettingsFolder);
        database.Text = @"C:\ChangedData";
        storage.Apply();
        reminders.Apply();
        Assert.Equal(@"C:\ChangedSettings", draft.SettingsFolder);
        Assert.Equal(@"C:\ChangedData", draft.DatabaseFolder);
        Assert.False(draft.ShowRemindersOnStartup);
        Assert.Equal(@"C:\OriginalSettings", live.SettingsFolder);
        Assert.True(live.ShowRemindersOnStartup);
    });

    [Fact]
    public void DesignerConstructionExposesNamedEditorCanvasAndOptionsPickerHierarchy() => Sta(() =>
    {
        var previous = LicenseManager.CurrentContext;
        try
        {
            LicenseManager.CurrentContext = new DesignLicenseContext();
            using var editor = new ExpenseEditor();
            using var storage = new StorageOptionsView();
            using var reminders = new ReminderOptionsView();
            Assert.All(Descendants(editor).OfType<MaskedTextBox>(), box => Assert.True(box.Visible));
            Assert.All(Descendants(editor).OfType<Button>(), button => Assert.True(button.Visible));
            Assert.All(Descendants(editor).OfType<Label>(), label => Assert.False(string.IsNullOrWhiteSpace(label.Name)));
            Assert.All(Descendants(storage).OfType<TextBox>(), input =>
            {
                var picker = Assert.IsType<TableLayoutPanel>(input.Parent);
                Assert.Equal(2, picker.ColumnCount);
                Assert.Single(picker.Controls.OfType<Button>());
                Assert.Equal(DockStyle.Fill, picker.Dock);
                Assert.True(picker.AutoSize);
            });
            Assert.IsType<TableLayoutPanel>(Descendants(reminders).OfType<CheckBox>().Single().Parent);
        }
        finally { LicenseManager.CurrentContext = previous; }
    });

    [Fact]
    public void ResultsGridPreservesClassicColumnsFormattingAndExpandsWithViewport() => Sta(() =>
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("th-TH");
        using var view = new ExpenseResultsView();
        using var host = new Form { ClientSize = new Size(900, 350) };
        view.Dock = DockStyle.Fill;
        host.Controls.Add(view);
        host.Show();
        view.Grid.DataSource = new List<Expense>
        {
            new() { PaidDate = new DateOnly(2024, 2, 29), Payee = "Vendor", AmountCents = 1234, BusinessUsePercent = 75 }
        };
        Application.DoEvents();
        Assert.Equal(new[] { "PaidDate", "Payee", "Description", "Category", "Tags", "Amount", "BusinessUsePercent", "BusinessAmount" },
            view.Grid.Columns.Cast<DataGridViewColumn>().Select(c => c.DataPropertyName));
        Assert.Equal("02/29/2024", view.Grid.Rows[0].Cells["PaidDate"].FormattedValue);
        Assert.Equal("12.34", view.Grid.Rows[0].Cells["Amount"].FormattedValue);
        Assert.Equal("9.26", view.Grid.Rows[0].Cells["BusinessAmount"].FormattedValue);
        Assert.False(view.Grid.AutoGenerateColumns);
        Assert.False(view.Grid.EnableHeadersVisualStyles);
        view.Grid.Columns["Payee"]!.Width = 235;
        host.ClientSize = new Size(1100, 450);
        Application.DoEvents();
        Assert.Equal(view.ClientSize, view.Grid.Size);
        Assert.Equal(235, view.Grid.Columns["Payee"]!.Width);
    });

    [Fact]
    public void OptionsRowsReflowAtLargeFontsWithoutOverlappingInstructions() => Sta(() =>
    {
        using var host = new Form { ClientSize = new Size(420, 220) };
        using var font = new Font(SystemFonts.MessageBoxFont?.FontFamily ?? FontFamily.GenericSansSerif, 18F);
        using var reminders = new ReminderOptionsView { Dock = DockStyle.Fill };
        host.Controls.Add(reminders);
        host.Show();
        reminders.Font = font;
        var show = Descendants(reminders).OfType<CheckBox>().Single();
        show.Text += "\nLonger localized reminder setting";
        Application.DoEvents();
        var help = Descendants(reminders).OfType<Label>().Single();
        Assert.True(help.Top >= show.Bottom + show.Margin.Bottom);
        Assert.True(show.Height >= show.GetPreferredSize(Size.Empty).Height);
        Assert.True(help.Height >= help.GetPreferredSize(new Size(help.Width, 0)).Height);
        Assert.True(reminders.AutoScroll);
        var reminderLayout = Assert.IsType<TableLayoutPanel>(help.Parent);
        reminders.ScrollControlIntoView(help);
        Application.DoEvents();
        Assert.True(reminderLayout.PointToScreen(help.Location).Y >= reminders.PointToScreen(Point.Empty).Y);
        Assert.True(reminderLayout.PointToScreen(new Point(help.Left, help.Bottom)).Y <= reminders.PointToScreen(new Point(0, reminders.Height)).Y);
        using var storage = new StorageOptionsView { Dock = DockStyle.Fill };
        host.Controls.Clear();
        host.Controls.Add(storage);
        storage.Font = font;
        Application.DoEvents();
        Assert.All(Descendants(storage).OfType<TextBox>(), input =>
        {
            var picker = Assert.IsType<TableLayoutPanel>(input.Parent);
            var browse = picker.Controls.OfType<Button>().Single();
            Assert.True(browse.Left >= input.Right + input.Margin.Right,
                $"Picker overlaps value: button {browse.Bounds}, input {input.Bounds}, picker {picker.Bounds}.");
            Assert.True(input.Height >= input.PreferredHeight,
                $"Value height {input.Height} is below preferred {input.PreferredHeight}.");
            Assert.True(browse.Height >= browse.GetPreferredSize(Size.Empty).Height,
                $"Picker height {browse.Height} is below preferred {browse.GetPreferredSize(Size.Empty).Height}.");
        });
    });

    [Fact]
    public void EditorKeepsRuntimeNoSelectionAndPinsActionsOutsideScrollableFields() => Sta(() =>
    {
        using var editor = new ExpenseEditor();
        Assert.False(Descendants(editor).OfType<MaskedTextBox>().First().Visible);
        using var host = new Form { ClientSize = new Size(700, 240) };
        editor.Dock = DockStyle.Fill;
        host.Controls.Add(editor);
        host.Show();
        editor.NewExpense(new DateOnly(2024, 2, 29));
        using var font = new Font(SystemFonts.MessageBoxFont?.FontFamily ?? FontFamily.GenericSansSerif, 16F);
        editor.Font = font;
        Application.DoEvents();
        var actions = Descendants(editor).OfType<FlowLayoutPanel>().Single();
        var viewport = Descendants(editor).OfType<Panel>().Single(p => p is not TableLayoutPanel && p is not FlowLayoutPanel);
        Assert.Equal(DockStyle.Bottom, actions.Dock);
        Assert.Same(editor, actions.Parent);
        Assert.True(actions.Visible);
        Assert.True(actions.Bottom <= editor.ClientSize.Height);
        Assert.True(viewport.Bottom <= actions.Top);
        Assert.True(viewport.AutoScroll);
        Assert.True(viewport.DisplayRectangle.Height > viewport.ClientSize.Height);
        var receipt = Descendants(editor).OfType<TextBox>().Single(b => b.AccessibleName == "Receipt file");
        viewport.ScrollControlIntoView(receipt);
        Application.DoEvents();
        var fields = Assert.IsType<TableLayoutPanel>(receipt.Parent);
        Assert.True(fields.PointToScreen(receipt.Location).Y < viewport.PointToScreen(new Point(0, viewport.Height)).Y);
        var businessUse = Descendants(editor).OfType<NumericUpDown>().Single();
        Assert.True(businessUse.Enabled);
        int saved = 0;
        editor.CommitRequested += (_, _) => saved++;
        Descendants(actions).OfType<Button>().Single(b => b.AccessibleName == "Save expense").PerformClick();
        Assert.Equal(1, saved);
        editor.SetEditing(false);
        Assert.False(actions.Visible);
        Assert.All(fields.Controls.OfType<TextBoxBase>(), input => Assert.True(input.ReadOnly));
        Assert.False(businessUse.Enabled);
        Assert.All(Descendants(businessUse), input => Assert.False(input.Enabled));
        Assert.False(fields.Controls.OfType<Button>().Single().Enabled);
        editor.Clear();
        Assert.False(editor.IsEditing);
        Assert.False(Descendants(editor).OfType<MaskedTextBox>().First().Visible);
    });
}
