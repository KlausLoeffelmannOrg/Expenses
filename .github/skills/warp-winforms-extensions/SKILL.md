---
name: warp-winforms-extensions
description: >-
  Traverses WARP WinForms control trees, attaches converters to existing bindings,
  calculates Form geometry or font metrics and consumes typed modal results.
  Addresses missing bindings, incorrect traversal predicates and helpers mistaken
  for void operations or DialogResult-returning APIs.
metadata:
  source: "WARP src\\WarpToolkit.WinForms.Extensions\\UI; ComponentExtensions; FontExtensions"
  revision: "0b5b59bca1cd1ff93c2370ba1c2c9c7f051a63a5"
---

# Focused WinForms extensions

> Designer pair rule: every Form, modal dialog and UserControl touched here is `partial` with a `.Designer.*`,
> a parameterless ctor owning `InitializeComponent()`, and a runtime ctor `(IServiceProvider) : this()`;
> services/IO/async start in `OnLoad`, never in `InitializeComponent` (`warp-winforms-application-builder`).

## Critical constraints

- Use the actual helper namespace/signature; generic predicate overloads differ.
- `AddBindingConverter` modifies an **existing** binding; it does not create one.
- The WARP `CenterToScreen` extension returns a rectangle (from `Screen.Bounds`),
  not a mutation; it is not WinForms' `Form.CenterToScreen()`. Typed ShowDialog returns
  `IModalDialogResult<T>`, not WinForms DialogResult.
- Control-tree operations run on the owning UI thread. Read `winforms-async-apis`
  for marshaling; `winforms-designer-code` for declarative initialization.
- Shared layout/DPI belongs to `winforms-high-dpi-fluent-layout` (rules, not its
  older one-file Form samples). This is not a
  broad control catalog or a package-selection router.

## Workflow and traversal

Reference `WarpToolkit.WinForms.Extensions`; tree/Form helpers use
`WarpToolkit.WinForms.Extensions.UI`.

| Family | Actual operations |
|---|---|
| Immediate children | `ChildControls()` / `<T>()`, `FirstChild` / `<T>`, `FirstChildOrDefault<T>` |
| Ancestors | `AscendantControls()` / `<T>`, `FirstAscendant` / `<T>`, `FirstAscendantOrDefault` / `<T>` |
| Descendants | `DescendantControls()` / `<T>`, `FirstDescendant` / `<T>`, `FirstDescendantOrDefault` / `<T>` |
| Stateful descendant predicate | `DescendantControls<CArg>` / `<T,CArg>` with predicate and argument |
| Tree actions | `Root()`, `TraverseAction(Action<Control>)` |
| Null/ambient font | `EnsureNotNull<T>`, `HasAmbientFont`, `ApplyAmbientFont` |

First methods throw if no match; OrDefault methods return null.
`AscendantControls<T>` takes `Func<Control,bool>?`, whereas
`DescendantControls<T>` takes `Func<T,bool>?`: do not assume all four families
have identical overload inventories.

```csharp
using WarpToolkit.WinForms.Extensions.UI;

foreach (Button button in this.DescendantControls<Button>(b => b.Enabled))
{
    button.Visible = false;
}
TextBox? primary = this.FirstDescendantOrDefault<TextBox>(t => t.Tag is "Primary");
Control root = this.Root();
this.TraverseAction(c => c.UseWaitCursor = true);
```

## Existing-binding converters

Namespace: `WarpToolkit.WinForms.Extensions.ComponentExtensions`.
`AddBindingConverter(this IBindableComponent, string propertyName,
WarpToolkit.ComponentModel.IValueConverter)` returns the Binding.
It throws for null arguments or no binding on that property, hooks Format/Parse
using current culture and unhooks on component disposal.

```csharp
using WarpToolkit.WinForms.Extensions.ComponentExtensions;

textBox.DataBindings.Add(nameof(TextBox.Text), viewModel, "Amount",
    formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
Binding binding = textBox.AddBindingConverter(nameof(TextBox.Text), converter);
```

For a one-off transformation, ordinary Binding.Format/Parse is also appropriate.
`RemoveBinding` removes the property's binding and throws if missing; do not
assume it selects by source identity. Read `winforms-databinding` for update
modes, converter lifetime and validation.

## Form geometry and typed modal results

The WARP extension `CenterToScreen(this Form form, Screen? screen = null,
int horizontalFillGrade = 50, int verticalFillGrade = 50)`
(`WarpToolkit.WinForms.Extensions.UI.FormExtensions`, verified 0.9.852) returns
a rectangle computed from **`Screen.Bounds`** (taskbar included) — unlike the
WinForms `Form.CenterToScreen()` method, which centers on `Screen.WorkingArea`
but does not size or choose a monitor. Assign the extension's result only when
full-screen-based bounds are intended;
LOB first-run geometry is ≈75%/80% of `Screen.WorkingArea`, centered, and
persisted window geometry is a monitor-relative DIP schema (`warp-app-services`;
[dip-schema](../winforms-high-dpi-fluent-layout/references/window-state.md#dip-schema)).

`GetRestorableBounds()` returns Bounds when Normal and RestoreBounds when
Minimized/Maximized; unknown state throws. `RestoreBounds` is physical pixels
of the last Normal state and is not rescaled by a later font change — snapshot
the Normal bounds yourself before maximizing instead of unscaling pixels.
`TryApplyFormBounds`/`ApplyFormPlacement` and their `Save*` pairs persist
absolute pixels + `FormWindowState` (legacy schema; migrate explicitly).

```csharp
using WarpToolkit.Desktop.AppServices;
using WarpToolkit.WinForms.Extensions.UI;

IModalDialogResult<EditCustomerVm> result =
    await dialog.ShowDialogAsync<EditCustomerVm>(viewModel);
if (result.DialogCloseReason == DialogCloseReason.OK
    && result.ReturnValue is { } accepted)
{
    SaveAcceptedCustomer(accepted);
}
```

This is a **host-method fragment**: dialog/viewModel and SaveAcceptedCustomer
belong to the application. The direct helper requires a class VM; the service
overload additionally requires INotifyPropertyChanged. The helper sets DataContext,
awaits the framework modal API and validates children when closing with OK.
Do not conflate it with native Form.ShowDialogAsync, or claim it takes the VM by ref.
The current helper's event-unsubscription is not unconditional; prefer a
single-use/disposed dialog instead of repeatedly invoking it on the same instance.
Dialog service registration belongs to `warp-app-services`.

## Font metrics

Import `WarpToolkit.WinForms.Extensions.FontExtensions`.
`GetAscent`, `GetDescent`, `GetLeading`, `GetBaseLine`, `GetLineHeight` return float
pixel metrics; baseline converts font-family design units using line spacing and
font height, and line height is the font's Height. They are not DirectWrite
metrics. Read `winforms-rendering` for font/resource ownership.

## Final checks

1. Traversal predicate and null/throw behavior fit the caller.
2. Binding exists before converter attachment; repeated attachment is intentional.
3. Returned geometry/result is consumed correctly, with UI-thread ownership.
