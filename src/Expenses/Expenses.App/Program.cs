using Microsoft.Extensions.DependencyInjection;
using Expenses.Core;
using Microsoft.Extensions.WinForms;
using WarpToolkit.Microsoft.Extensions.Logging;
using WarpToolkit.WinForms.AppServices.ServiceExtensions;

namespace Expenses.App
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var preferencesStore = new PreferencesStore();
            AppPreferences preferences;
            BookkeepingStore store;
            try
            {
                preferences = preferencesStore.Load();
                store = new BookkeepingStore(preferences.DatabaseFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Expenses could not open its settings or database:\n{ex.Message}",
                    "Expenses - startup failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            WinFormsApplicationBuilder builder = WinFormsApplication.CreateBuilder();

            builder.Services.AddSingleton(preferencesStore);
            builder.Services.AddSingleton(preferences);
            builder.Services.AddSingleton(store);
            builder.Services.AddWinFormsUserSettingsService();
            builder.Services.AddWinFormsExceptionService();
            builder.Logging.UseWinFormsLogging();

            builder.UseStartupForm<MainForm>()
                .UseHighDpiMode(HighDpiMode.PerMonitorV2)
                .UseColorMode(SystemColorMode.System)
                .UseTextRenderingV2()
                .UseLegacyVisualStyles()
                .UseFormRevealMode(FormRevealMode.Deferred)
                .UseVisualStylesMode(VisualStylesMode.Net11)
                ;

            WinFormsApplication app = builder.Build();
            app.Run();
        }
    }
}
