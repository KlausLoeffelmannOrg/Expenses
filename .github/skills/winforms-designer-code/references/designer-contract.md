# Designer contract

Normative companion to [../SKILL.md](../SKILL.md). Examples that construct a Form live in [serialization.md](serialization.md).

## Files

| File | Owns |
|---|---|
| `FormName.cs` / `FormName.vb` | Parameterless constructor, runtime constructors that chain `: this()` / `Me.New()`, event bodies, runtime state |
| `FormName.Designer.cs` / `FormName.Designer.vb` | `InitializeComponent`, generated `Dispose(bool)`, serialized fields |
| `FormName.resx` | Localized and binary resources the generator applies |

A non-composite custom `Control` may stay in one file when it has a parameterless constructor and hosts no designed children. The moment it hosts designed children, make it a `UserControl` partial pair.

## Generated Dispose

Keep the Visual Studio shape. Do not invent a second override.

```csharp
protected override void Dispose(bool disposing)
{
    if (disposing && (components != null))
    {
        components.Dispose();
    }
    base.Dispose(disposing);
}
```

Additional runtime cleanup subscribes to `Disposed` from the main partial, or is merged into this single override if regeneration will preserve it. Never dispose `components` from both places.

## Constructor

```csharp
public CustomerDialog()
{
    InitializeComponent();
}

public CustomerDialog(CustomerDraft draft) : this()
{
    ArgumentNullException.ThrowIfNull(draft);
    _draft = draft;
}
```

Lightweight hookup such as `Load += LoadHandler` is allowed after `InitializeComponent` when the handler is design-time safe. The design-time path must not read configuration, open files, query data, start a task, create a device or WebView, or set `FormBorderStyle`, `WindowState`, `Visible`, or fullscreen.

## InitializeComponent order

| Order | Step |
|---|---|
| 1 | `components = new Container()` when any component constructor needs it |
| 2 | Instantiate controls and components. Pass `components` into component constructors |
| 3 | `BeginInit` and `SuspendLayout` for the surface and nested containers the generator suspends |
| 4 | Assign child properties and add children to serialized collections |
| 5 | Named event hookup only |
| 6 | Configure the Form or UserControl last |
| 7 | `EndInit`, `ResumeLayout`, `PerformLayout` |
| 8 | Fields at end of file (`WFO3002`). `components` may stay at the top |

`BindingSource`, `Timer`, `ErrorProvider`, and `ImageList` that the Designer owns take the container. A `BindingSource.DataSource` in this method is `typeof(SomeType)`, not a live instance, list, or service result.

## Prohibited in InitializeComponent

Control flow: `if`, `for`, `foreach`, `while`, `switch`, `try`/`catch`, `lock`, `await`, `goto`.

Operators and functions: ternary, `??`, `?.`, `?[]`, `nameof`, lambdas, anonymous methods, local functions, collection expressions, object initializers used to hide assignments.

Calls: helper methods, validation, calculation, static image/icon factories, and anything other than `SuspendLayout`, `ResumeLayout`, `PerformLayout`, `BeginInit`, and `EndInit`.

## Prohibited in the designer file

Constructor overloads, ordinary methods, ordinary properties, and business logic. Explicit interface members may remain when the generator already emitted them. Do not add partial methods except where the project's generator already uses them.

## Events

C# uses a named method in the main partial: `_save.Click += SaveClick`. VB prefers `Handles _save.Click` in the main partial. Keep existing `AddHandler` only when the file already uses it.

## Nullable and qualification

Preserve the generator's namespace style, qualification, and nullable treatment. Do not reformat a generated file to file-scoped namespaces, target-typed `new`, or collection expressions. New hand-written designer partials may use the `#nullable disable` field style Visual Studio emits. Do not mix that modernization into an existing generated file.

## Resources

Use `ComponentResourceManager.ApplyResources` and real `.resx` entries. Do not call an icon factory from `InitializeComponent` and expect the call to survive a save.

## Round-trip

Open the surface, change one property, save, close, and reopen. If an expression disappears, it was not serializable. Move it to the main partial or add a real `TypeConverter` / `CodeDomSerializer`. Compilation does not prove either outcome.

Designer load failures usually mean the parameterless constructor threw, a designed type has no public parameterless constructor, or a static/field initializer touched IO, services, or a device. Fix that path. Do not catch and hide the exception inside `InitializeComponent`.
