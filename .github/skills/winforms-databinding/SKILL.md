---
name: winforms-databinding
description: >-
  USE FOR: WinForms BindingSource, INotifyPropertyChanged, BindingList,
  update modes, validation, ErrorProvider, master-detail, and DataGridView
  binding. DO NOT USE FOR: applying MVVM unless asked, or for Designer file
  shape and layout. ObservableCollection is not a direct DataSource.
---

# WinForms data binding

Bind through a `BindingSource` the Designer owns. Discover properties with `typeof(T)` inside `InitializeComponent`. Assign the live instance in the main partial after the parameterless constructor.

## Designer baseline

- Designed forms and user controls follow [winforms-designer-code](../winforms-designer-code/SKILL.md).
- Create `components` before `new BindingSource(components)` or `new ErrorProvider(components)`.
- Do not put a live object, list, service call, or lambda `Format` handler in `InitializeComponent`.
- Do not add a second `Dispose` to release an adapter. Use `Disposed` or the single generated override.

## Rules that change the answer

| Need | Use |
|---|---|
| Property changes | `INotifyPropertyChanged`. Raise only when the value changes |
| List changes WinForms understands | `BindingList<T>` |
| WPF/MAUI-style lists | Adapter over `ObservableCollection<T>`. Never bind that collection directly |
| When edits write back | `DataSourceUpdateMode`: `OnValidation`, `OnPropertyChanged`, or `Never` |
| Master/detail | Detail `BindingSource.DataSource` is the master source. `DataMember` is the navigation property |
| Field errors | `IDataErrorInfo` plus `ErrorProvider.DataSource`. Do not set `e.Cancel = true` on every validating event |
| Non-MVVM modal draft | On .NET 8+, `Form.DataContext` holds the draft. Commit to live state only after a successful OK. Cancel does not write back |
| Conversion | `Binding.Format` and `Binding.Parse` in the main partial. WinForms has no `IValueConverter` |
| Large grids | Virtual mode and `CellValueNeeded`. Do not load the entire set onto the UI thread |
| .NET Framework typed DataSets | Leave them if the project still uses them. New .NET work uses object binding |
| .NET 8+ commands | [winforms-mvvm](../winforms-mvvm/SKILL.md), and only when MVVM was requested |

The Data Sources window is a Framework designer feature. On modern .NET, a `.datasource` file plus `typeof` is the discovery path.

Null sources: assign `null` or a placeholder deliberately. Set `Binding.DataSourceNullValue` / `NullValue` when empty display must not become a type default by accident.

Currency follows the master `BindingSource`. Do not keep a second selected-item copy unless the screen is not bound.

Tables, the adapter boundary, and virtual-mode notes: [references/binding-rules.md](references/binding-rules.md). Recipes: [references/binding-recipes.md](references/binding-recipes.md). Adapter: [references/observable-adapter.md](references/observable-adapter.md).

## Validation

1. Designer binding uses `typeof`, and the live list is assigned only at runtime.
2. Add, remove, and property edits refresh the control.
3. A validation error is visible and does not trap focus.
4. The `BindingSource` is container-owned and disposed once.
