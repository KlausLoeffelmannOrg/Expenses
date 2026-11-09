---
name: warp-winforms-application-builder
description: >-
  Bootstraps or retrofits a WinForms app onto WARP's IHost-based
  WinFormsApplication / WinFormsApplicationBuilder, and prepares every Form and
  UserControl as a Designer-safe IServiceProvider façade with the canonical
  two-constructor pattern. Owns the rule that service resolution is deferred
  until after InitializeComponent. Consult first whenever a WARP feature needs DI.
metadata:
  source: "WARP src\\WarpToolkit.WinForms.AppServices\\WinFormsHost; Templates\\cs\\code\\projects\\ModernWinFormsApp"
  versions: "../warp-api-decision-guide/references/versions.md"
---

# WARP WinFormsApplicationBuilder

`WinFormsApplication` + `WinFormsApplicationBuilder` (namespace
`Microsoft.Extensions.WinForms`, package `WarpToolkit.WinForms.AppServices`) host a
WinForms program on the .NET Generic Host: DI, configuration, logging, lifetime.

## Designer invariant (applies to every WARP surface)

- Every app Form, modal dialog and UserControl is `partial` with a paired `.Designer.*`.
- The parameterless constructor owns `InitializeComponent()`; the DI constructor
  chains `: this()` and never calls `InitializeComponent` itself.
- `InitializeComponent` is serialization: no services, live data, IO, async, GPU.
- The OOP Designer must open the surface with no host or DI; design time receives
  `NullServiceProvider`. `UseStartupForm<TForm>()` itself requires `new()`.
- Runtime composition happens in `OnLoad` / `ISupportInitialize.EndInit`, guarded
  for the parameterless path.
- Serializable properties live in the Designer file; `Dispose` only there.
- Rules of record: `winforms-designer-code`; layout `winforms-high-dpi-fluent-layout`.

## Mental model

```
Program.cs: WinFormsApplication.CreateBuilder(args).UseStartupForm<MainForm>() … .Build().Run()
   → Run(): startupForm = Services.GetRequiredService<MainForm>(); Application.Run(startupForm)
   → DI picks MainForm(IServiceProvider sp) : this()
        1. this()            → InitializeComponent(); Designer-emitted comp.SetServiceProvider(this)
                               only STORES `this`; _serviceProvider is still null.
        2. DI ctor body      → _serviceProvider = new DeferredServiceProvider(sp)
        3. EndInit / OnLoad  → components and the Form may now resolve services.
```

The Form itself is the `IServiceProvider` façade — no factory object, no
"set provider later" plumbing.

## Program.cs (C#)

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.WinForms;
using WarpToolkit.WinForms.AppServices.ServiceExtensions;

namespace ContosoLob;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        WinFormsApplicationBuilder builder = WinFormsApplication.CreateBuilder(args);

        builder.Services
            .AddWinFormsDialogService()
            .AddWinFormsExceptionService()
            .AddWinFormsUserSettingsService()
            .AddLocalKeyRetrievalService();

        builder.Services.AddSingleton<ICustomerRepository, SqlCustomerRepository>();
        builder.Services.AddScoped<MainViewModel>();
        builder.Services.TryAddScoped<EditCustomerDialog>();   // every DI-resolved surface
        builder.Services.TryAddScoped<SettingsView>();         // tab UserControl

        builder.UseStartupForm<MainForm>()                     // TryAddScoped<MainForm>()
            .UseHighDpiMode(HighDpiMode.PerMonitorV2)
            .UseColorMode(SystemColorMode.System)
            .UseTextRenderingV2()
            .UseLegacyVisualStyles();

        builder.Build().Run();
    }
}
```

Fluent members that exist on the builder: `UseStartupForm<TForm>()` /
`UseStartupForm(Form)`, `UseHighDpiMode`, `UseColorMode`, `UseDefaultFont`,
`UseLegacyVisualStyles(bool = true)`, `UseVisualStylesMode(VisualStylesMode)`,
`UseTextRenderingV2(bool = true)`, `UseFormRevealMode`, `UseProjectSettings()`,
`AllowWinFormsJsonAppSettings(bool = true)`,
`UseWinForms{Release,Debug,ReleaseOrDebug}Logger()`. There is **no** `UseVisualStyles(bool)`.
Do not also call `Application.SetHighDpiMode`/`SetCompatibleTextRenderingDefault`.

**Font, reveal and geometry order (verified 0.9.852):**

- `UseDefaultFont(Font)` is applied in `Run()` (`Application.SetDefaultFont`)
  **before** the startup Form is resolved, so the Form's `AutoScaleMode.Font`
  scaling and every hosted `AutoScaleMode.Inherit` view settle once at
  creation. Prefer `UseStartupForm<TForm>()` over `UseStartupForm(Form)` — an
  already-constructed instance has run `InitializeComponent` against the old
  default font. A user font preference must be read from settings in
  `Program.cs` and passed here; keep the **logical point size** as app state —
  after WM_DPICHANGED the runtime `Form.Font` is already DPI-scaled and must
  not be read back as the preference.
- `UseFormRevealMode(FormRevealMode)` (.NET 11+ builds only; `Inherit` default,
  `Classic`, `Deferred`, `TemplateFormReveal`) sets
  `Application.SetDefaultFormRevealMode` before any form exists; `Deferred`
  delays the first paint so the Form can restore DIP geometry and apply input
  minima/icon sizes in `OnLoad` without a visible jump. Capture or save bounds
  only **after** actual scaling happened (end of `OnLoad`/`OnShown`), never
  from Designer `ClientSize`.
- Order in the Form: `OnLoad` → services → restore DIP geometry
  (`warp-app-services`) → compose tabs/views → `OnShown` first-visible work.
  Runtime font changes go through the Form's `OnFontChanged` transaction
  ([font-change-transaction](../winforms-high-dpi-fluent-layout/references/window-state.md#font-change-transaction)).
- Exceptions/continuations stay on the UI `SynchronizationContext`
  (`ISyncContextService`, `IWinFormsAppExceptionService`); no bare
  `Task.Run(...).Result` or fire-and-forget without a catch in startup code.

**VB:** WARP-hosted VB apps use `Program.vb` with `Sub Main` and
`<StartupObject>Sub Main</StartupObject>`; the VB Application Framework is off
because `WinFormsApplication.Run` owns `Application.Run`. (Classic VB apps without
the WARP host keep the Application Framework and have no `Program.vb`.)

## Form preparation — required shape

Two files per surface. The Designer partial holds only generated members; the
regular partial holds both constructors and the façade.

```csharp
// MainForm.Designer.cs — Designer-owned
namespace ContosoLob;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _aiChatClient = new WarpToolkit.WinForms.AI.AIChatClientComponent(components);
        SuspendLayout();
        _aiChatClient.SetServiceProvider(this);     // emitted for BindableServiceProviderComponent
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Name = "MainForm";
        ResumeLayout(false);
    }
    #endregion

    private WarpToolkit.WinForms.AI.AIChatClientComponent _aiChatClient;
}
```

```csharp
// MainForm.cs — regular partial (Designer never touches this)
namespace ContosoLob;

public partial class MainForm : Form, IServiceProvider
{
    private IServiceProvider? _serviceProvider;

    // Designer path. Also the ctor the DI ctor chains into.
    public MainForm()
    {
        InitializeComponent();
    }

    // Runtime path. Wrapped provider is assigned AFTER InitializeComponent ran.
    public MainForm(IServiceProvider serviceProvider) : this()
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _serviceProvider = new DeferredServiceProvider(serviceProvider);
    }

    object IServiceProvider.GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        if (_serviceProvider is null)
        {
            throw new InvalidOperationException(
                "MainForm was constructed without a service provider. Resolve it from WinFormsApplication.");
        }

        return _serviceProvider.GetService(serviceType)
            ?? throw new InvalidOperationException($"Service '{serviceType.Name}' is not registered.");
    }

    private sealed class DeferredServiceProvider : IServiceProvider
    {
        private readonly Func<IServiceProvider> _factory;

        public DeferredServiceProvider(IServiceProvider serviceProvider)
        {
            _factory = () => serviceProvider;
        }

        public object GetService(Type serviceType)
            => _factory().GetService(serviceType)
               ?? throw new InvalidOperationException($"Service '{serviceType.Name}' is not registered.");
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (_serviceProvider is null)
        {
            return;   // Designer / parameterless path: nothing to compose.
        }

        // First safe point for service-touching work (see references).
    }
}
```

Rules baked into the shape:

- `_serviceProvider` is nullable and assigned only in the DI ctor; the Designer
  path leaves it `null` and `OnLoad` guards on it.
- `: this()` guarantees `InitializeComponent` runs exactly once.
- Explicit `IServiceProvider.GetService` throws a clear exception if a component
  resolved too early — that is a component bug, not a Form bug.
- The WARP project templates place the explicit `GetService` and the nested
  `DeferredServiceProvider` in the Designer partial under
  `#pragma warning disable WFO3001`. Either location compiles; keep one, never both.
  Prefer the regular partial so the Designer file stays pure serialization.
- UserControls that host `BindableServiceProviderComponent`s follow the identical
  shape (`UserControl, IServiceProvider`, two ctors).

## Retrofitting `Application.Run(new Form1())`

1. Add `WarpToolkit.WinForms.AppServices` at the project's pinned WARP version.
2. Replace `Main` with the builder pattern above; delete manual `Application.Set*` calls.
3. For each surface the host resolves: keep the parameterless ctor, add
   `(IServiceProvider sp) : this()`, implement `IServiceProvider` explicitly, add
   `DeferredServiceProvider`, register with `TryAddScoped<T>()` (or `UseStartupForm<T>()`).
4. Move static singletons (logging, settings, dialogs) into DI.
5. Replace `MessageBox.Show` in view-models with `IDialogService`.
6. Run. The classic failure is a component calling `GetService` inside its own
   `SetServiceProvider` — defer to `EndInit`/`OnLoad`
   ([deferral and diagnostics](references/deferral-and-diagnostics.md)).

## Anti-patterns

- Calling `InitializeComponent()` from the DI ctor (runs twice, double-registers).
- Touching `_serviceProvider` in a field initializer or before `: this()` completes.
- Assigning the raw provider instead of `DeferredServiceProvider`.
- Resolving services from `SetServiceProvider` while the Form is in `InitializeComponent`.
- `Application.Run` next to `WinFormsApplication.Run`; static `IServiceProvider` caches.
- Designer components with parameterized ctors; passing `this` manually to a
  child `BindableServiceProviderComponent` (Designer already emits it).
- `async void OnLoad` with awaits in the override body; use a sync override that
  starts a guarded `Task` method (pattern in references).

## References

- [Deferral, component authoring, debugging checklist](references/deferral-and-diagnostics.md)
- Versions: [`warp-api-decision-guide/references/versions.md`](../warp-api-decision-guide/references/versions.md)

## Hand-offs

- Dialog/settings/exception/key services: `warp-app-services`.
- `AIChatClientComponent` / `ChatView`: `warp-winforms-ai`.
- Tabs whose pages are DI-resolved UserControls: `warp-fluent-tab-control`.
- Designer serialization rules: `winforms-designer-code`.

## Final checks

1. Each DI-resolved surface has both ctors; `InitializeComponent` appears only in the parameterless one.
2. `OnLoad`/`EndInit` guard the null-provider (Designer) path.
3. Builder calls use members that exist on the installed version; no `Application.Set*` duplicates.
