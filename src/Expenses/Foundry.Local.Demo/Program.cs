namespace Foundry.Local.Demo;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.SetColorMode(SystemColorMode.System);
        using var service = new FoundryLocalService();
        Application.Run(new DemoForm(service));
    }
}
