using Expenses.Core;

namespace Expenses.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.SetColorMode(SystemColorMode.System);
        Application.ThreadException += (_, e) => Dialogs.DialogLayout.Error(null!, e.Exception);
        try
        {
            var preferencesStore = new PreferencesStore();
            var preferences = preferencesStore.Load();
            var store = new BookkeepingStore(preferences.DatabaseFolder);
            Application.Run(new MainForm(new AppServices(store, preferencesStore, preferences)));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Expenses could not start:\n{ex.Message}",
                "Expenses - startup failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private sealed record AppServices(BookkeepingStore Store, PreferencesStore PreferencesStore, AppPreferences Preferences) : IServiceProvider
    {
        public object? GetService(Type type) => type == typeof(BookkeepingStore) ? Store
            : type == typeof(PreferencesStore) ? PreferencesStore
            : type == typeof(AppPreferences) ? Preferences : null;
    }
}
