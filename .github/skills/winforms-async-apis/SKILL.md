---
name: winforms-async-apis
description: >-
  USE FOR: Control.InvokeAsync, Form.ShowAsync, ShowDialogAsync, and
  TaskDialog.ShowDialogAsync, including overload choice and starting async work
  from UI code. DO NOT USE FOR: background work that never touches a control, or
  for COM apartment rules. Do not hide the design surface to start a load.
---

# WinForms async APIs

`InvokeAsync` posts to the UI message queue and returns a `Task`. `Control.Invoke` sends and blocks. Post when the UI must stay responsive. Send only when the caller truly cannot continue without the result and is not already on the UI thread holding a lock the UI needs.

## Designer baseline

- Forms and dialogs follow [winforms-designer-code](../winforms-designer-code/SKILL.md).
- The parameterless constructor does not start async work and does not hide the form.
- A kickoff handler returns immediately when `LicenseManager.UsageMode` is `Designtime`. `DesignMode` alone is not reliable in a constructor.
- One `Task`-returning method owns the work. The UI entry awaits it and observes exceptions.

## Event handlers

WinForms event handlers that await are `async void`. That is the framework shape, not a general license for `async void`. The handler awaits one `Task` method and catches failures. Do not put the whole operation inline. Do not discard the task with `_ =` when a fault must surface.

```csharp
private async void LoadButton_Click(object? sender, EventArgs e)
{
    try
    {
        await LoadAsync(CancellationToken.None);
    }
    catch (Exception ex)
    {
        MessageBox.Show(this, ex.Message, "Load failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

Do not make `OnLoad` itself `async void`. If work starts when the form is shown, subscribe to `Shown` or `Load` and use the same handler shape, with the design-time guard first. Posting through `InvokeAsync` is for a message that must finish first, such as `SelectAll` during `KeyDown`. It is not a required hop for an ordinary button click.

## Overload choice

| Callback | Overload |
|---|---|
| Sync, no result | `InvokeAsync(Action, CancellationToken)` |
| Sync, returns `T` | `InvokeAsync<T>(Func<T>, CancellationToken)` |
| Async, no result | `InvokeAsync(Func<CancellationToken, ValueTask>, CancellationToken)` |
| Async, returns `T` | `InvokeAsync<T>(Func<CancellationToken, ValueTask<T>>, CancellationToken)` |

Passing a `Task`-returning method to the synchronous overload is fire-and-forget. The analyzer reports `WFO2001`. Async callbacks take a `CancellationToken` and return `ValueTask`. Do not pass an already-created `InvokeAsync` task to `Task.Run`. CPU work belongs in `Task.Run`. UI work belongs in `InvokeAsync`. I/O usually needs neither just to hop threads.

Complex dialog validation is a `Task`-returning method. Do not assign `DialogResult.OK` until it succeeds. Keep the dialog open and show the error. `ShowDialogAsync` completes when the form closes, so a premature OK closes it. The close gate itself is [databinding](../winforms-databinding/SKILL.md).

`ShowAsync`, `ShowDialogAsync`, and `TaskDialog.ShowDialogAsync` are stable on .NET 10. On .NET 9 they were experimental (`WFO5002`). `ShowAsync` completes when the form closes. As of .NET 10 the state machine holds a weak reference, so retaining the task does not keep the form alive. Call them on the UI thread. Dispose forms on the ownership path the caller already has.

Details and the design-safe kickoff sample: [references/async-rules.md](references/async-rules.md) and [references/async-examples.md](references/async-examples.md).

## Validation

1. The design-time constructor does not start work or change chrome.
2. A faulted load surfaces in the handler. It is not only an `Application.ThreadException` hook.
3. A continuation that touches a control does not use `ConfigureAwait(false)`.
4. The signature matches the target. Do not copy a .NET 10 async dialog call onto .NET Framework.
