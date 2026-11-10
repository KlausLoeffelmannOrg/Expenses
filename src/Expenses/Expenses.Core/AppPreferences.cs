using System.Text.Json;

namespace Expenses.Core;

public sealed class AppPreferences
{
    public string SettingsFolder { get; set; } = DefaultRoot;
    public string DatabaseFolder { get; set; } = Path.Combine(DefaultRoot, "Data");
    public string BackupFolder { get; set; } = DefaultBackupFolder;
    public bool ShowRemindersOnStartup { get; set; } = true;
    public WindowLayout Window { get; set; } = new();
    public Dictionary<string, int> ColumnWidths { get; set; } = [];
    public int NavigationWidth { get; set; } = 240;
    public int ResultsHeight { get; set; } = 320;
    public int? LastPeriodId { get; set; }
    public int ToolbarIconSize { get; set; } = 24;
    public string InterfaceFontFamily { get; set; } = "Segoe UI";
    public float InterfaceFontPoints { get; set; } = 9;
    public int InterfaceFontStyle { get; set; }
    public string? FoundryModelId { get; set; }
    public static string DefaultRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Expenses");
    public static string DefaultBackupFolder => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OneDrive"))
        ? "" : Path.Combine(Environment.GetEnvironmentVariable("OneDrive")!, "Expenses", "Backups");
}

public sealed class WindowLayout
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 1200;
    public int Height { get; set; } = 850;
    public bool Maximized { get; set; }
    public bool HasPosition { get; set; }
}

public sealed class PreferencesStore(string bootstrapFolder)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    public PreferencesStore() : this(AppPreferences.DefaultRoot) { }

    public AppPreferences Load()
    {
        string pointer = Path.Combine(bootstrapFolder, "settings-location.json");
        string folder = File.Exists(pointer)
            ? JsonSerializer.Deserialize<string>(File.ReadAllText(pointer)) ?? throw new InvalidDataException("Invalid settings location.")
            : bootstrapFolder;
        string path = Path.Combine(folder, "settings.json");
        var settings = File.Exists(path)
            ? JsonSerializer.Deserialize<AppPreferences>(File.ReadAllText(path)) ?? throw new InvalidDataException("Invalid application settings.")
            : new AppPreferences { SettingsFolder = folder };
        settings.SettingsFolder = folder;
        return settings;
    }

    public void Save(AppPreferences preferences)
    {
        AtomicWrite(Path.Combine(preferences.SettingsFolder, "settings.json"), JsonSerializer.Serialize(preferences, JsonOptions));
        AtomicWrite(Path.Combine(bootstrapFolder, "settings-location.json"), JsonSerializer.Serialize(preferences.SettingsFolder));
    }

    private static void AtomicWrite(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, content);
            File.Move(temporary, path, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
