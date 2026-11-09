# Service deferral, component authoring and DI diagnostics

Companion to `warp-winforms-application-builder/SKILL.md`. Read when authoring a
`BindableServiceProviderComponent`, when a component resolves services "too early",
or when DI fails at startup.

## Contents

- What the Designer emits
- The "too early" trap
- Deferral pattern for components (`ISupportInitialize` + `SetServiceProvider`)
- Form-side deferral table
- Debugging checklist

## What the Designer emits

For every component whose type derives from `BindableServiceProviderComponent` (or
carries `[DesignerSerializer("WarpToolkit.WinForms.Design.ServiceProviderAssignableComponentCodeDomSerializer", …)]`)
the Designer generates, **inside** `InitializeComponent`:

```csharp
// Designer-generated:
_aiChatClient.SetServiceProvider(this);
```

- `this` is the Form/UserControl, which implements `IServiceProvider` explicitly.
- `SetServiceProvider` must only **store** the reference. At that moment the Form's
  `_serviceProvider` field is still `null` (the parameterless ctor is running).
- After the DI ctor body assigns the `DeferredServiceProvider`, the stored reference
  becomes usable from `EndInit` / `OnLoad` onward.
- At design time `BindableServiceProviderComponent.EnsurePlausibleServiceProvider`
  substitutes a `NullServiceProvider`, so the Designer surface never resolves anything.

## The "too early" trap

Even with a correctly prepared Form, a component must not assume the provider is
usable when `SetServiceProvider` runs:

1. **Component order.** Inside `InitializeComponent` components are added one by one;
   component *B* does not exist yet when *A* receives the provider.
2. **No handle yet.** `Form.Handle`, `Control.InvokeAsync` and a captured
   `WindowsFormsSynchronizationContext` are unavailable during `InitializeComponent`.
3. **`Load` has not fired.** Many WARP services and view-models key off `OnLoad`.

## Deferral pattern for components

`AIChatClientComponent` (`BindableServiceProviderComponent, ISupportInitialize`) is the
reference implementation. Follow the same shape:

```csharp
[DesignerSerializer(
    "WarpToolkit.WinForms.Design.ServiceProviderAssignableComponentCodeDomSerializer",
    "Microsoft.DotNet.DesignTools.Serialization.CodeDomSerializer")]
[ToolboxItem(true)]
public partial class MyServiceComponent
    : BindableServiceProviderComponent, ISupportInitialize
{
    private bool _isInitializing;
    private ILogger<MyServiceComponent>? _logger;

    void ISupportInitialize.BeginInit() => _isInitializing = true;

    void ISupportInitialize.EndInit()
    {
        _isInitializing = false;
        TryWireUpDependencies();
    }

    public override IServiceProvider SetServiceProvider(object serviceProvider)
    {
        IServiceProvider provider = base.SetServiceProvider(serviceProvider);

        // Never resolve here while the surrounding Form may still be in InitializeComponent.
        if (!_isInitializing)
        {
            TryWireUpDependencies();
        }

        return provider;
    }

    private void TryWireUpDependencies()
    {
        try
        {
            IServiceProvider sp = this;
            _logger = sp.GetService<ILogger<MyServiceComponent>>()
                ?? NullLogger<MyServiceComponent>.Instance;
        }
        catch (Exception ex)
        {
            // Tolerate: at design time the provider is NullServiceProvider by construction.
            Debug.WriteLine($"[MyServiceComponent] deferred wire-up failed: {ex}");
        }
    }
}
```

Rules:

- Always call `base.SetServiceProvider(...)`; bypassing it crashes the Designer.
- Do not throw from `SetServiceProvider` for non-fatal reasons; degrade instead.
- Designer-instantiated components need a parameterless ctor (plus the
  `IContainer` overload); apply parameters via properties or `EndInit`.

## Form-side deferral

| Where | What belongs there |
|---|---|
| `InitializeComponent()` | Designer-only. No service access. |
| Ctor body after `InitializeComponent` (DI ctor) | Only the `DeferredServiceProvider` assignment. |
| `OnLoad` | HWND-dependent work, DI-resolved child views (`AddTab`), restore of persisted DIP geometry (font already applied by `UseDefaultFont`), input minima/icon sizing from the settled font, AI component wiring, async kick-off via a guarded `async void` that delegates to a `Task` method. |
| `OnShown` | First-visible work (focus, banners); with `FormRevealMode.Deferred` this is the first painted frame — capture/save bounds no earlier than here. |

```csharp
protected override void OnLoad(EventArgs e)
{
    base.OnLoad(e);

    if (_serviceProvider is null)
    {
        return; // Designer / parameterless path: nothing to compose.
    }

    _chatView = _serviceProvider.GetRequiredService<ChatView>();
    _mainTabControl.AddTab("Chat", _chatView);
}
```

If `OnLoad` must await, keep the override synchronous and start a guarded task:

```csharp
protected override void OnLoad(EventArgs e)
{
    base.OnLoad(e);
    _ = InitializeAsyncGuarded();
}

private async Task InitializeAsyncGuarded()
{
    try
    {
        await LoadRecentAsync();
    }
    catch (Exception ex)
    {
        IDialogService? dialogs = _serviceProvider?.GetService<IDialogService>();
        if (dialogs is not null)
        {
            await dialogs.ShowErrorAsync(ex.Message, "Startup");
        }
    }
}
```

`async void` is acceptable for conventional UI event handlers only when the body
is a single awaited call to a `Task` method that owns the `try/catch`.

## Debugging checklist

| Symptom | Likely cause |
|---|---|
| `NullReferenceException` in component code accessing services | Component called `GetService` synchronously from `SetServiceProvider`. Defer to `EndInit`/`OnLoad`. |
| `InvalidOperationException` from the Form's `IServiceProvider.GetService` | Form constructed via `new MainForm()` at runtime, or DI ctor did not chain `: this()` / assign the `DeferredServiceProvider`. Resolve Forms through DI. |
| Component reports `NullServiceProvider` at runtime | Form does not implement `IServiceProvider`, or was `new`-ed instead of resolved. |
| Designer crash opening the form | `SetServiceProvider` override skipped `base.SetServiceProvider`. |
| `A startup form must be specified` | Missing `UseStartupForm<TForm>()` / `UseStartupForm(form)`. |
| Components double-registered / events fire twice | `InitializeComponent` ran twice — DI ctor called it directly instead of `: this()`. |
| Second `WinFormsApplication` complains | Previous instance not disposed (tests). Dispose before creating a new one. |
