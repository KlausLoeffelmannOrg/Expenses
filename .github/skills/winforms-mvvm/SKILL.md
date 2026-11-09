---
name: winforms-mvvm
description: >-
  USE FOR: an explicit request to apply MVVM on WinForms .NET 8+, including
  Command, DataContext, CommunityToolkit view models, and designer typeof
  binding. DO NOT USE FOR: ordinary event-handler forms, or for layout and
  Designer file shape. Those stay with their owners.
---

# WinForms MVVM

Use this skill only when the user asked for MVVM. Do not convert a working event-handler form on your own. A non-MVVM modal may still put its draft in `Form.DataContext` on .NET 8+. That assignment is not this skill. Accept, cancel, and validation stay with databinding, options, and async.

## Designer baseline

- Every application Form, modal dialog, and UserControl is `partial` with a designer file.
- The designer partial owns `InitializeComponent` and the only generated `Dispose(bool)`.
- The parameterless constructor is the design-time path. It must not construct a view model that needs services, and it must not assign a live data source.
- The runtime constructor chains `: this()`, then assigns the view model.
- Designer `BindingSource.DataSource` is `typeof(TheViewModel)`, not an instance.

## Rules that change the answer

- .NET 8+ adds `Control.DataContext`, `ButtonBase.Command`, `ToolStripItem.Command`, and `CommandParameter`. `ToolStripItem` derives from `BindableComponent`. Commands enable and disable from `CanExecute`.
- Below .NET 8, implement the same separation explicitly. Do not pretend `Command` exists.
- Put CommunityToolkit view models in their own project. The generator does not cascade into the WinForms project.
- View models do not reference `System.Windows.Forms`. Dialogs and `MessageBox` stay in the main partial.
- `ObservableCollection<T>` is not a WinForms `DataSource`. Adapt it. See [winforms-databinding](../winforms-databinding/SKILL.md).
- Do not add a second `Dispose` to dispose an adapter. Subscribe to `Disposed` from the runtime constructor, or merge into the single designer override.
- A `.datasource` file helps the Designer discover properties. It does not remove the need for `typeof(...)` in `InitializeComponent`.

API tables, the toolkit shape, and the partial-pair sample: [references/mvvm-rules.md](references/mvvm-rules.md). A coherent toolkit example: [references/mvvm-example.md](references/mvvm-example.md).

## Validation

1. The Designer opens the form without the DI constructor.
2. `typeof` binding still resolves the generated property names.
3. `CanExecute` changes update the bound control.
4. View-model tests do not construct a Form.
