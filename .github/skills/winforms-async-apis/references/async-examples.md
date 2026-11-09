# Async patterns

## Contents

- [Handler and task method](#handler-and-task-method)
- [Design-safe kickoff](#design-safe-kickoff)
- [Deferred key handling](#deferred-key-handling)

## Dialog validation task

OK must not close the dialog before the task succeeds. Leave the OK button's `DialogResult` at `None` so Enter cannot accept early. `AcceptButton` may still point at that button. `Cancel` keeps `DialogResult.Cancel`.

```csharp
private async void OkButton_Click(object? sender, EventArgs e)
{
    try
    {
        if (!await ValidateDraftAsync(_draft, CancellationToken.None))
        {
            return;
        }

        DialogResult = DialogResult.OK;
    }
    catch (Exception ex)
    {
        MessageBox.Show(this, ex.Message, "Check failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

Field-level `ErrorProvider` rules stay in [binding rules](../../winforms-databinding/references/binding-rules.md). Do not set `DialogResult.OK` on the button for this path.

## Handler and task method

```csharp
private async void LoadButton_Click(object? sender, EventArgs e)
{
    try
    {
        await LoadAsync();
    }
    catch (OperationCanceledException)
    {
    }
    catch (Exception ex)
    {
        MessageBox.Show(this, ex.Message, "Load failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

private async Task LoadAsync()
{
    await Task.Delay(1);
    if (!IsDisposed)
    {
        Text = "Ready";
    }
}
```

## Design-safe kickoff

`LoadingForm` is a designed form. The designer partial owns `InitializeComponent` and `Dispose`. The main partial does not start work in the parameterless constructor and does not hide the window.

```csharp
public partial class LoadingForm : Form
{
    private readonly CancellationTokenSource _lifetime = new();
    private Task? _work;

    public LoadingForm()
    {
        InitializeComponent();
        Shown += ShownHandler;
    }

    private async void ShownHandler(object? sender, EventArgs e)
    {
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return;
        }

        try
        {
            _work ??= RunAsync(_lifetime.Token);
            await _work;
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                MessageBox.Show(this, ex.Message, "Load failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async Task RunAsync(CancellationToken token)
    {
        await Task.Delay(1, token);
        if (!IsDisposed && !token.IsCancellationRequested)
        {
            Text = "Ready";
        }
    }

    private void LoadingForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!e.Cancel)
        {
            _lifetime.Cancel();
        }
    }
}
```

Wire `FormClosing` in the designer partial with a named handler, or hook it next to `Shown` in the parameterless constructor. Cancel the lifetime from that handler. Dispose the `CancellationTokenSource` from the single designer `Dispose` only after the observed task has finished, or from `Disposed` once close has been observed. Do not add a second `Dispose` override in this file.

## Deferred key handling

```csharp
private async void TextBox_KeyDown(object? sender, KeyEventArgs e)
{
    if (e.KeyCode != Keys.Down)
    {
        return;
    }

    try
    {
        await InvokeAsync(() => _textBox.SelectAll());
    }
    catch (Exception ex) when (!IsDisposed)
    {
        MessageBox.Show(this, ex.Message, "Edit failed");
    }
}
```

That post is specifically so the current key message can finish. Do not wrap an ordinary click in `BeginInvoke` or a discarded `InvokeAsync`.
