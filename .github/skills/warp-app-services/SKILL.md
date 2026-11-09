---
name: warp-app-services
description: Use this skill when bootstrapping a WinForms application with the WARP-Toolkit's hosting/DI infrastructure, registering app-level services (dialogs, exception handling, user settings, API-key retrieval), implementing IDialogService consumers in view-models, or wiring AI provider keys from environment variables. Covers WarpToolkit.WinForms.AppServices and WarpToolkit.Desktop.AppServices together because they form one logical feature.
metadata:
  source: "WARP src\\WarpToolkit.WinForms.AppServices; src\\WarpToolkit.Desktop.AppServices"
  versions: "../warp-api-decision-guide/references/versions.md"
---

# WARP App Services

## Designer invariant (applies to every WARP surface)

- Every Form, modal dialog and UserControl is `partial` with a paired `.Designer.*`.
- Parameterless ctor owns `InitializeComponent()`; the DI ctor is `(IServiceProvider sp) : this()`.
- `InitializeComponent` never touches services, settings, keys, IO or async.
- The Designer opens every surface without the host; `OnLoad` guards the null-provider path.
- Shape of record: `warp-winforms-application-builder`; serialization rules: `winforms-designer-code`.

This skill covers the two packages that together provide application-level
infrastructure for a WARP-based WinForms app:

| Package | Role |
|---------|------|
| `WarpToolkit.Desktop.AppServices` | UI-framework-agnostic **contracts** (interfaces, `ViewModelBase`). View-models depend on this only. |
| `WarpToolkit.WinForms.AppServices` | WinForms-specific **implementations** plus the `IHost`-based application builder. |

## Mental model

```
            ┌──────────────────────────────────────────┐
            │   WarpToolkit.Desktop.AppServices        │
            │   IDialogService, IModalDialogResult<T>, │
            │   ISyncContextService,                   │
            │   IWinFormsAppExceptionService,          │
            │   ViewModelBase                          │
            └────────────────┬─────────────────────────┘
                             │ implemented by
                             ▼
            ┌──────────────────────────────────────────┐
            │   WarpToolkit.WinForms.AppServices       │
            │   WinFormsApplication / Builder / Options│
            │   EnvironmentVariableKeyService          │
            │   ServiceCollection AddWinForms…()       │
            └──────────────────────────────────────────┘
```

ViewModels reference **only** `Desktop.AppServices`. Forms and `Program.cs`
reference `WinForms.AppServices`.

## Canonical bootstrap (`Program.cs`)

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
            .AddWinFormsUserSettingsService(o =>
            {
                o.SaveMode = UserSettingsSaveMode.Debounced;
            })
            .AddLocalKeyRetrievalService();

        // Register your own services next:
        builder.Services.AddSingleton<MainViewModel>();

        builder.UseStartupForm<MainForm>()               // requires a parameterless ctor
            .UseColorMode(SystemColorMode.System)        // DarkMode aware
            .UseHighDpiMode(HighDpiMode.PerMonitorV2)
            .UseTextRenderingV2()
            .UseLegacyVisualStyles();

        builder.Build().Run();
    }
}
```

Notes:

- **Do not** also call `Application.SetCompatibleTextRenderingDefault(...)` or
  `Application.SetHighDpiMode(...)` yourself — the builder owns startup options.
  `UseLegacyVisualStyles(bool)` / `UseVisualStylesMode(VisualStylesMode)` exist;
  `UseVisualStyles(bool)` does not.

- **Do not** add an `app.config`. Use `appsettings.json` (the builder
  automatically wires it; toggle JSON settings with
  `AllowWinFormsJsonAppSettings(true)`).

- **VB with the WARP host:** create `Program.vb` with `Sub Main`
  (`<StartupObject>Sub Main</StartupObject>`) and turn the VB Application
  Framework off — `WinFormsApplication.Run` owns `Application.Run`, and the
  Application Framework would start a second message loop. This matches every
  WARP VB template. Classic VB apps that do not use the WARP host keep the
  Application Framework and have no `Program.vb`.

## Using `IDialogService` from a ViewModel

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WarpToolkit.Desktop.AppServices;

public partial class CustomerListViewModel(IDialogService dialogs) : ObservableObject
{
    [ObservableProperty]
    private CustomerVm? selectedCustomer;

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync()
    {
        if (selectedCustomer is null)
        {
            return;
        }

        bool ok = await dialogs.ShowConfirmationAsync(
            message: $"Delete '{selectedCustomer.DisplayName}'?",
            title: "Delete customer");

        if (!ok)
        {
            return;
        }

        // delete...
    }

    private bool CanDelete() => selectedCustomer is not null;
}
```

The view-model has **no** reference to `System.Windows.Forms`. It can be unit
tested with a mocked `IDialogService` and even reused under a different UI
framework (e.g. a MAUI shell) by providing a different implementation.

### Dialog API at a glance

| Method | Use case |
|--------|----------|
| `ShowMessageAsync` | Informational message. |
| `ShowWarningAsync` | Warning. |
| `ShowErrorAsync` | Error report. |
| `ShowConfirmationAsync` | Yes/No question. Returns `bool`. |
| `RequestInputAsync` | Single text input. Returns the entered string. |
| `ShowDialogAsync<TViewModel>(vm)` | Show a full modal dialog given a view-model instance. Resolves the corresponding view via DI. |

`ShowDialogAsync<TViewModel>` resolves the dialog Form through DI (keyed by the
view-model type), hands the view-model over via `DataContext`, and returns an
`IModalDialogResult<TViewModel>` — **not** a WinForms `DialogResult` and not a
tuple. The dialog Form stays a Designer pair with a parameterless ctor; the
service only resolves and shows it:

```CSharp
    public async Task<IModalDialogResult<TViewModel>> ShowDialogAsync<TViewModel>(TViewModel viewModel)
        where TViewModel : class, INotifyPropertyChanged
    {
        Form formView = ServiceProvider.GetKeyedService<Form>(viewModel.GetType())
            ?? throw new InvalidOperationException($"No form found for view model of type {typeof(TViewModel).Name}.");

        // Get the form type from the view model
        return await formView.ShowDialogAsync<TViewModel>(viewModel);
    }
```

## API-key retrieval for AI providers

`EnvironmentVariableKeyService` (in `WinForms.AppServices`) implements
`IKeyRetrievalService` + `ILocalKeyRetrievalService` and is pre-populated for
all `WellKnownAIProviders`:

```csharp
using WarpToolkit.Desktop.AI;
using WarpToolkit.WinForms.AppServices;

// In Program.cs:
builder.Services.AddLocalKeyRetrievalService();

// Inside a service:
public sealed class MyAgent(EnvironmentVariableKeyService keys)
{
    public string OpenAIKey => keys.GetAiProviderKey(WellKnownAIProviders.OpenAI);

    public void Configure(string apiKey)
        => keys.SetAiProviderKey(
            WellKnownAIProviders.OpenAI,
            apiKey,
            EnvironmentVariableTarget.User);
}
```

Use `TryGetAiProviderKey` for the "may not be configured yet" path so the UI
can guide the user to set the key, rather than throwing
`EnvironmentVariableNotFoundException`.

## User settings and window geometry

Inject `IUserSettingsService` (contract in `WarpToolkit.ComponentModel`,
implementation registered via `AddWinFormsUserSettingsService(...)`): generic
`Get<T>(key, default)` / `TryGet<T>` / `Set<T>` / `Remove` / `Contains` /
`Flush`. The Form keeps its Designer pair and two constructors; the service is
resolved in `OnLoad` through the façade, never in the constructor or
`InitializeComponent`.

**Built-in placement helpers are a legacy pixel schema — not the recommended
restore policy.**
`UIServiceExtensions` (`WarpToolkit.WinForms.Extensions.UI`, verified 0.9.852)
offers `TryApplyFormBounds/SaveFormBounds` (absolute `Rectangle`, restored when
it intersects any `Screen.WorkingArea`) and `ApplyFormPlacement/
SaveFormPlacement` (absolute `Rectangle` + `FormWindowState`; invisible rects
fall back to a centered 80%/80% of the primary working area — that is the
library's own fallback, documented here only to recognize legacy behavior; it
is **not** the 75%/80% first-run policy and must not be adopted as one.
Minimized is never restored), plus `TryApply/Save DataGridViewColumnWidths`,
`ListViewColumnWidths`,
`SplitterDistance` and `Form.GetRestorableBounds()` (Normal → `Bounds`,
Maximized/Minimized → `RestoreBounds`). They store physical pixels with no DPI
or monitor identity, so a saved rect is wrong after a DPI, font or monitor
change. For a LOB main window persist the **DIP schema** through the generic
`Set<T>`/`TryGet<T>` instead and treat an existing pixel blob as an explicit
migration (versioned key, convert once with the DPI recorded at migration or
discard and re-center — never reinterpret pixels as DIPs). Column widths and
splitter distances remain fine through the helpers when re-applied after the
font settles.

DIP schema (rule of record:
[dip-schema](../winforms-high-dpi-fluent-layout/references/window-state.md#dip-schema),
[first-run-geometry](../winforms-high-dpi-fluent-layout/references/window-state.md#first-run-geometry)):

- Saved while `WindowState == Normal` (or from the last Normal snapshot):
  outer `Bounds` converted to 96-DPI DIPs (`px * 96 / DeviceDpi`) **relative to
  the monitor's `Screen.WorkingArea.Location`** (monitor-local, so negative
  desktop origins survive), plus `Screen.DeviceName`, `Screen.Bounds`,
  `WorkingArea`, the DPI at save, `IsMaximized` as a separate flag and a schema
  version. Do not font-normalize coordinates.
- Restore in `OnLoad` **after** `UseDefaultFont`/AutoScale settled: resolve the
  monitor by `DeviceName` (fallback: bounds signature, then primary); convert
  DIPs with that monitor's *current* DPI; clamp to its current `WorkingArea`;
  apply Normal bounds first, then `WindowState = Maximized` if flagged. Missing
  monitor → first-run geometry (≈75% width / 80% height of the primary working
  area, centered). The form's `DeviceDpi` only reflects the target monitor
  after it is placed there; read it after placement (or `Shown`) and accept the
  OS-suggested rect from WM_DPICHANGED rather than re-applying saved DIPs
  ([dpi-suggested-bounds](../winforms-high-dpi-fluent-layout/references/window-state.md#dpi-suggested-bounds)).
- Runtime font changes keep the outer bounds through the Form's
  `OnFontChanged` transaction
  ([font-change-transaction](../winforms-high-dpi-fluent-layout/references/window-state.md#font-change-transaction));
  save geometry from the Normal bounds afterwards, not during the transaction.

```csharp
// MainForm.cs — regular partial; MainForm.Designer.cs holds InitializeComponent/Dispose.
public partial class MainForm : Form, IServiceProvider
{
    private const string GeometryKey = "MainForm.Geometry.v2";   // DIP schema; v1 = legacy pixels
    private IServiceProvider? _serviceProvider;
    private IUserSettingsService? _settings;

    public MainForm()
    {
        InitializeComponent();
    }

    public MainForm(IServiceProvider serviceProvider) : this()
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _serviceProvider = new DeferredServiceProvider(serviceProvider);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);                       // app font already applied by the builder; AutoScale settled
        _settings = _serviceProvider?.GetService<IUserSettingsService>();

        if (_settings is not null && _settings.TryGet(GeometryKey, out WindowGeometry? dips) && dips is not null)
        {
            WindowGeometryRestorer.Apply(this, dips);          // app-owned: resolve monitor, DIP→px, clamp, Normal then Maximized
        }
        else
        {
            WindowGeometryRestorer.ApplyFirstRun(this, Screen.FromControl(this)); // 75% / 80% of WorkingArea, centered
        }

        _settings?.TryApplyDataGridViewColumnWidths(_gridCustomers, key: "Grid.Customers");
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_settings is not null && WindowState != FormWindowState.Minimized)
        {
            _settings.Set(GeometryKey, WindowGeometry.Capture(this));   // Normal bounds → DIPs + monitor + DPI + IsMaximized
        }

        _settings?.SaveDataGridViewColumnWidths(_gridCustomers, key: "Grid.Customers");
        base.OnFormClosing(e);
    }

    // IServiceProvider.GetService + DeferredServiceProvider: see warp-winforms-application-builder.
}
```

`WindowGeometry`/`WindowGeometryRestorer` are app code following the WinForms
rule of record; keep them `T`-serializable for the settings service. Verified
on scratch tests at 96/144/192 DPI with negative origins and a missing
monitor; a physical multi-monitor WM_DPICHANGED move is **not** yet covered —
test it on real hardware before claiming it.

## Modal dialogs without a ViewModel

Small input dialogs that do not justify MVVM still get a contract:

- The dialog is a Designer pair; `Form.DataContext` holds a **draft** object
  (inputs and outputs) created by the caller; the dialog edits the draft and
  never reaches back into caller state.
- `AcceptButton`/`CancelButton` are set in the Designer file; OK sets
  `DialogResult.OK`, Cancel sets `DialogResult.Cancel`; the caller reads the
  draft only on `OK`.
- Simple synchronous validation lives in `OnFormClosing`: when
  `DialogResult == DialogResult.OK` and the draft is invalid, show the error,
  set `e.Cancel = true` and keep the dialog open.
- Complex/async validation (server round-trip, file probe) keeps the dialog
  open: OK disables the footer, shows a busy state, awaits with a
  `CancellationToken` tied to Cancel/closing, reports errors inline and only
  then sets `DialogResult.OK`. Canonical lifecycle (busy/cancel/error, no
  `async void` beyond the event handler boundary): `winforms-async` and
  `winforms-options-dialog`; the footer is the nested-TLP
  [canonical modal footer](../winforms-high-dpi-fluent-layout/references/layout-examples.md#canonical-modal-footer).
- From a ViewModel use `IDialogService.ShowDialogAsync<TViewModel>` above
  instead; do not mix both styles in one dialog.

## Exception handling

```csharp
public sealed class FeatureModule(IWinFormsAppExceptionService exceptions)
{
    public void Initialize()
    {
        exceptions.RegisterExceptionHandler(OnUiException);
    }

    private void OnUiException(object? sender, ThreadExceptionEventArgs e)
    {
        // log, telemetry, recovery…
    }
}
```

`IWinFormsAppExceptionService` is the single hook for
`Application.ThreadException`. Avoid attaching to `Application.ThreadException`
directly from feature code — go through the service so multiple modules can
coexist.

## Rules and anti-patterns

- **Reference the right package:** ViewModels and shared class libraries must
  reference **only** `WarpToolkit.Desktop.AppServices`, never the WinForms
  variant. The WinForms host application references both.

- **Prepare every DI-resolved Form / UserControl correctly.** The host hands
  the service provider to the Form through a parameterized constructor, but
  that constructor must chain `: this()` to the parameter-less Designer
  constructor and wrap the provider in a private `DeferredServiceProvider`
  before assigning the backing field. Calling `InitializeComponent` from
  the parameterized ctor directly (instead of via `: this()`) will break the
  Designer round-trip and run `InitializeComponent` twice when both
  constructors exist. See the `warp-winforms-application-builder` skill for
  the canonical shape of the code-behind file and the deferral contract that
  components rely on.

- **Do not** access `Application.OpenForms` from a view-model. Use
  `IDialogService` or `ISyncContextService` for cross-thread coordination.

- **Do not** mark dialog-service consumer methods as `async void`. The
  service's `Show*Async` methods already return `Task` and bubble correctly
  via `RelayCommand` async commands.

- **Do not** instantiate `WinFormsApplication` more than once — the builder
  enforces `CheckNoExistingInstance()` for a reason.

- **Do not** store API keys in source. Use
  `EnvironmentVariableKeyService.SetAiProviderKey(...)` or an OS credential
  store accessed through `IRemoteKeyRetrievalService`.

- **Do not** dispose `WinFormsApplication` manually inside `Main` — `Run()`
  controls its lifetime.

## Where to look next

- Canonical two-constructor Form shape and deferral contract:
  `warp-winforms-application-builder`.

- AI components that consume registered keys and chat services: switch to the
  `warp-winforms-ai` skill.

- Modern controls: `warp-fluent-tab-control`, `warp-messaging`, `warp-wizard`,
  `warp-input-controls`, `warp-containers-layout` (`warp-winforms-controls` is
  only a router).

## Final checks

1. Every DI-resolved Form/UserControl has a parameterless ctor plus `(IServiceProvider) : this()`.
2. Services are resolved in `OnLoad`/`EndInit` with a null guard; never in `InitializeComponent`.
3. Builder calls use existing members; VB startup follows the WARP-host rule above.
