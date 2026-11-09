# Project setup and startup

Companion to [../SKILL.md](../SKILL.md). Do not copy these fragments over a project that already chose a different TFM, DPI mode, or startup.

## New SDK project

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <ApplicationHighDpiMode>PerMonitorV2</ApplicationHighDpiMode>
  </PropertyGroup>
</Project>
```

Use a Windows-versioned TFM only when the app calls Windows APIs that need it. Do not put that TFM on a class library just to look modern. Package references use a stable major range the repository already accepts.

`ApplicationConfiguration.Initialize()` reads `ApplicationHighDpiMode`, enables visual styles, and sets the text-rendering default. Calling `SetHighDpiMode` after it throws or is a no-op depending on host, and it is not the way to "also" set DPI.

```csharp
[STAThread]
static void Main()
{
    ApplicationConfiguration.Initialize();
    Application.SetColorMode(SystemColorMode.System); // .NET 9+; before any window
    Application.Run(new MainForm());
}
```

Omit `SetColorMode` below .NET 9. Do not also call `EnableVisualStyles` or `SetHighDpiMode` beside `ApplicationConfiguration.Initialize()`.

Legacy startup that does not use `ApplicationConfiguration` calls `Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)` once, before `EnableVisualStyles` and before any window. Do not mix that legacy sequence into an SDK project that already calls `Initialize`.

Do not use `app.config` or a manifest as the DPI switch for a new SDK app.

## VB Application Framework

Classic VB WinForms apps use the Application Framework and do not have `Program.vb`. Put defaults in the existing `MyApplication` partial:

```vb
Private Sub ApplyDefaults(
        sender As Object,
        e As Microsoft.VisualBasic.ApplicationServices.ApplyApplicationDefaultsEventArgs
    ) Handles Me.ApplyApplicationDefaults

    e.HighDpiMode = HighDpiMode.PerMonitorV2
    e.ColorMode = SystemColorMode.System
End Sub
```

Set `e.Font` only when the application font is an intentional product choice. Do not stamp that font onto every designed child. .NET 11 visual-style and reveal defaults, when the target is actually .NET 11, go in this same handler. See [winforms-net11-apis](../../winforms-net11-apis/SKILL.md).

If the project already disabled the Application Framework, keep its explicit startup. Do not add `Program.vb` to an Application Framework app, and do not delete an existing explicit startup just to match the default.

WARP-hosted VB is a third shape, not a classic startup to invent or delete. `WinFormsApplicationBuilder` apps ship `Program.vb`, `<StartupObject>Sub Main</StartupObject>`, and the Application Framework off, because `WinFormsApplication.Run` calls `Application.Run`. Only the classic no-WARP template uses `ApplicationEvents.vb`. Host setup stays in `warp-winforms-application-builder`. Do not call a `UseVisualStyles(bool)` method the builder does not have. Visual styles go through `UseLegacyVisualStyles(bool)` or `UseVisualStylesMode(VisualStylesMode)`. `UseStartupForm(Of TForm)()` requires `New()`.

## Exception hooks

`Application.ThreadException` can log a UI-thread exception and keep the message loop alive. `AppDomain.UnhandledException` cannot stop termination. Neither hook replaces `try`/`catch` in an `async void` event handler.

## Solution shape

Keep Forms beside their designer partials. View models and services may live in other projects. A separate view-model project is a choice, not a requirement, unless [MVVM](../../winforms-mvvm/SKILL.md) source generators are in use.
