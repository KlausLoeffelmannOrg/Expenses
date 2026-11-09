# MVVM rules and partial pair

Companion to [../SKILL.md](../SKILL.md).

## APIs (.NET 8+)

| API | Role |
|---|---|
| `Control.DataContext` | Ambient. Cascades to children |
| `ButtonBase.Command` / `ToolStripItem.Command` | `ICommand` binding |
| `CommandParameter` | Passed to the command |
| `BindingSource.DataSource = typeof(T)` | Designer property discovery |
| Live instance assignment | Main partial, after `: this()` |

Bind `Command` with `new Binding("Command", bindingSource, "SaveCommand", true)`. Set `CommandParameter` as a simple assignment. Property bindings use the generated public names, not the toolkit field names.

## Project split

`CommunityToolkit.Mvvm` source generators do not cascade. The view-model project is a class library targeting the app's TFM or a compatible `net8.0`. The WinForms project references it. Do not put `[ObservableProperty]` types only in the WinForms project and expect the generator to see them through a reference.

A `.datasource` file under `Properties/DataSources` names the view-model assembly-qualified type. It is an aid. The designer file still assigns `typeof`.

## Partial pair

```csharp
// PersonEditForm.cs
public partial class PersonEditForm : Form
{
    private PersonViewModel? _viewModel;

    public PersonEditForm()
    {
        InitializeComponent();
    }

    public PersonEditForm(PersonViewModel viewModel) : this()
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        _viewModel = viewModel;
        personViewModelBindingSource.DataSource = viewModel;
    }
}
```

```csharp
// PersonEditForm.Designer.cs
partial class PersonEditForm
{
    private System.ComponentModel.IContainer components = null;

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        personViewModelBindingSource = new BindingSource(components);
        _txtFirstName = new TextBox();
        _btnSave = new Button();
        SuspendLayout();
        personViewModelBindingSource.DataSource = typeof(PersonViewModel);
        _txtFirstName.DataBindings.Add(new Binding("Text", personViewModelBindingSource, "FirstName", true, DataSourceUpdateMode.OnPropertyChanged));
        _btnSave.DataBindings.Add(new Binding("Command", personViewModelBindingSource, "SaveCommand", true));
        AcceptButton = _btnSave;
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(_txtFirstName);
        Controls.Add(_btnSave);
        Name = "PersonEditForm";
        ResumeLayout(false);
        PerformLayout();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private BindingSource personViewModelBindingSource;
    private TextBox _txtFirstName;
    private Button _btnSave;
}
```

List adapters created in the runtime constructor subscribe to `Disposed` there. Do not override `Dispose` again.

`DataContext` on a parent may be set in the runtime constructor or in a design-time-guarded load handler. Child UserControls react in `OnDataContextChanged`. They still have their own parameterless constructor and designer partial. Do not `new BindingSource()` in that override if the designer already owns one.

## View-model boundary

Commands call services. They do not show dialogs. The main partial observes command completion or a status property and then shows UI. Async command exceptions need one application policy. Do not assume the toolkit swallows them.

`ObservableCollection<T>` needs the adapter in [observable-adapter.md](../../winforms-databinding/references/observable-adapter.md). Bind the adapter list through a `BindingSource`. Disable user add/delete on the grid when the collection is command-owned.

## Tests

Construct the view model with fakes. Do not spin a message loop to prove `CanExecute`.
