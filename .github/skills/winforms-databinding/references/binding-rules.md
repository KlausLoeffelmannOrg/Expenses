# Binding rules

Companion to [../SKILL.md](../SKILL.md). Declarative fragments assume fields the designer partial already declared. They are not a second Form class.

## Designer fragment

```csharp
components = new System.ComponentModel.Container();
personBindingSource = new BindingSource(components);
errorProvider = new ErrorProvider(components);
personBindingSource.DataSource = typeof(Person);
errorProvider.DataSource = personBindingSource;
_txtName.DataBindings.Add(new Binding("Text", personBindingSource, "Name", true, DataSourceUpdateMode.OnPropertyChanged));
```

## Runtime fragment

```csharp
personBindingSource.DataSource = people; // BindingList<Person>, after InitializeComponent
```

`new Person { Name = "..." }` and service results belong in this runtime assignment, never in `InitializeComponent`.

## Update mode

| Mode | When |
|---|---|
| `OnValidation` | Default. Writes when the control validates |
| `OnPropertyChanged` | Each change. Search-as-you-type and immediate commands |
| `Never` | Source to control only |

Call the binding owner's focused-field writeback before save if the mode has not pushed the current edit.

## Master/detail

Master source points at the customer list. Detail source uses `DataSource = master` and `DataMember = "Orders"` when that property exists. Each grid binds to its own source. Detail follows master currency. The child collection must notify.

## Validation

`IDataErrorInfo.this[string]` returns a field message. `ErrorProvider` bound to the same `BindingSource` displays it. A `Validating` handler may set an error. It must not set `e.Cancel = true` as the normal way to block a bad field. That traps keyboard users.

## Dialog accept

A non-MVVM modal edits a draft. On .NET 8+ that draft is `Form.DataContext`. Below .NET 8 it is one field on the form. Do not open [MVVM](../../winforms-mvvm/SKILL.md) just to pass the draft.

Uncomplicated OK/Cancel assign `AcceptButton` and `CancelButton`, and the buttons' `DialogResult` is `OK` and `Cancel`. If a synchronous check fails, `OnFormClosing` sets `e.Cancel` when `DialogResult` is `OK`, shows the error, and leaves the dialog open. Do not cancel unrelated focus changes.

Copy the draft onto live state only after that close is allowed. Cancel, Escape, and the title-bar close leave the live object unchanged.

Cross-field or async validation leaves the OK button's `DialogResult` at `None` and must not accept before the task succeeds. That workflow is [async](../../winforms-async-apis/references/async-examples.md#dialog-validation-task). A reusable multi-step validator type is not required by this contract.

`Format` and `Parse` convert display text such as currency. Attach them in the main partial. A failed parse leaves the binding exception visible. Do not silently substitute a default.

## Lists and grids

`BindingList<T>` raises list-changed events. `ObservableCollection<T>` does not, until the adapter in [observable-adapter.md](observable-adapter.md) is used. Bind `adapter.List` through a `BindingSource`. Command-owned lists disable `AllowUserToAddRows` and `AllowUserToDeleteRows`.

Virtual mode: `VirtualMode = true`, set `RowCount`, handle `CellValueNeeded` and, if editable, `CellValuePushed`. Do not also assign a full-list `DataSource` for those cells.

Combo boxes use `DisplayMember`, `ValueMember`, and a `SelectedValue` binding. The list instance is still a runtime assignment.

## Framework versus modern .NET

| Feature | .NET Framework | .NET 8+ |
|---|---|---|
| Typed DataSet designer | Supported | Legacy, code-only |
| Object binding | Supported | Preferred |
| Data Sources window | Present | Not the same tool |
| `Command` / `DataContext` | Absent | Present |

## Leaks and nulls

A `BindingSource` left outside `components` must be disposed on the single cleanup path. Dropping a form without disposing a subscribed adapter leaks the source. Setting `DataSource = null` is a valid empty state. Do not replace it with `new Person()` unless an empty object is the product rule.

## Performance

Reset a `BindingList` in one notification when replacing the whole list. Do not add thousands of rows on the UI thread. Virtual mode or a paged `BindingList` is the large-list path.
