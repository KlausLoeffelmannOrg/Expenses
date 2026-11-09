# Async API notes

Companion to [../SKILL.md](../SKILL.md).

## InvokeAsync

`InvokeAsync` queues the delegate and returns. The caller is not blocked. Use it from a worker to touch controls, or from the UI thread when the current message must finish before the next UI change. `KeyDown` plus `SelectAll` is that second case. Test the actual key. It does not promise an order against paint.

Avoid `InvokeAsync(() => SomeTaskAsync())`. Overload resolution can pick `Func<Task>` and return `Task<Task>`, or drop the inner task. Wrap the work in `async (CancellationToken ct) => ...` and return `ValueTask`.

`Task.Run` takes a delegate, not an existing `Task`. `Task.Run(() => control.InvokeAsync(...))` is usually redundant because `InvokeAsync` already posts. Do not move a UI loop onto the thread pool and then marshal every line back.

## Dialog APIs

| API | Stable | Result |
|---|---|---|
| `Form.ShowAsync` | .NET 10 | `Task` completed on close or dispose |
| `Form.ShowDialogAsync` | .NET 10 | `DialogResult` |
| `TaskDialog.ShowDialogAsync` | .NET 10 | clicked `TaskDialogButton` |

## Staged dialog validation

Cross-field or async checks are one `Task` method. Leave the OK button's `DialogResult` at `None`. The handler awaits the task and sets `DialogResult.OK` only after success. On failure, show a specific error and return, leaving the dialog open. Cancel still closes without committing the draft. Do not use `async void` for the validation method itself. The click handler may be `async void` and must observe the fault.

A shared validator interface beyond `ErrorProvider`, `OnFormClosing`, and this task is not specified here. Add one only when a product already has that type.

Prefer an owner overload when an owner exists. Check the target signature. Calling `ShowAsync` twice on one instance throws. The caller still constructs and disposes the form. These calls do not belong in the parameterless constructor.

## Kickoff

`Shown` is the clear point when visibility matters. A synchronous `OnLoad` may subscribe a handler. It should not itself be `async void`. The handler:

1. Returns immediately in design mode (`LicenseManager.UsageMode == LicenseUsageMode.Designtime`).
2. Awaits one `Task` method.
3. Catches cancellation separately from faults.
4. Restores cursor and button state in `finally` when it changed them.

Keep the `CancellationTokenSource` for the form lifetime. Cancel on an accepted close. Do not dispose it while the observed task still uses the token. Observe completion before disposing shared state.

Two UI-bound activities are concurrent, not automatically parallel. `Task.WhenAny` returns the completed `Task`. Await that task before restarting it. On stop, cancel and observe both outstanding tasks so faults are not dropped.

Library I/O may use `ConfigureAwait(false)` only when its continuation does not touch a control. Marshal the UI update afterward with `InvokeAsync` if the method resumed off the UI context.

## What not to use as the design

`Application.ThreadException` and `AppDomain.UnhandledException` are last-resort logs. They are not the error path for a button. `BeginInvoke` is not required when `InvokeAsync` or a direct `await` on the UI context already posts or resumes correctly.
