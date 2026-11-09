---
name: warp-text-spelling
description: Use when adding spelling checks, squiggles, overview dots, correction menus or Add to Dictionary to WARP D2DTextBox, D2DTextEditor, D2DCodeEditor, D2DMarkdownEditor, chat prompts, or hosted TextBoxBase text elements. Explains backend-neutral WarpToolkit.Text.Spelling, Windows dictionary discovery, asynchronous provider lifetime, language changes, diagnostic snapshots, failure reporting and deterministic tests.
metadata:
  source: "WARP src\\WarpToolkit.Text.Spelling; src\\WarpToolkit.WinForms.DirectX (spelling adorners)"
  versions: "../warp-api-decision-guide/references/versions.md"
---

# WARP text spelling

> Designer pair rule: every Form, modal dialog and UserControl touched here is `partial` with a `.Designer.*`,
> a parameterless ctor owning `InitializeComponent()`, and a runtime ctor `(IServiceProvider) : this()`;
> services/IO/async start in `OnLoad`, never in `InitializeComponent` (`warp-winforms-application-builder`).

## Architecture and dependency choice

| Layer | Role |
| --- | --- |
| `WarpToolkit.Text.Spelling` | Backend-neutral `ISpellChecker`, dictionaries, immutable requests/results, UTF-16 ranges and the asynchronous `SpellingEngine`. No WinForms or Windows COM requirement. |
| `WarpToolkit.Windows.Spelling` | Windows installed dictionaries and user dictionary mutation. `WindowsSpellingProvider` owns a dedicated STA COM apartment; `WindowsSpellChecker` implements `ISpellChecker`. |
| `WarpToolkit.WinForms.DirectX` | Text-surface integration: asynchronous diagnostics, squiggles, overview markers, stale-result rejection and UI correction commands. |

Current `WarpToolkit.Windows.Spelling.csproj` is **not packable**
(`IsPackable=false`). Reference its project from a matching WARP source checkout
until a package actually exists. Do not invent a NuGet package version.
Use backend-neutral contracts alone for custom or fake providers.

## Which text controls participate?

The shared spelling implementation is in
`WarpToolkit.WinForms.DirectX.Controls.TextBoxBase`, the retained text element.
It is unrelated to `System.Windows.Forms.TextBoxBase`.

`D2DTextBox` exposes forwarding properties and `TextBoxElement`.
`D2DTextEditor`, `D2DCodeEditor` and `D2DMarkdownEditor` inherit the same control
integration. Configure them through their `D2DTextBox` interface.
Other D2D surfaces that host WARP `TextBoxBase` can configure the element
directly, but do not automatically gain the WinForms wrapper's correction menu
or dictionary-command handler.

Do not add a second spelling engine or manually paint underlines on a control
that already implements this integration.

## Three things required for visible spelling

1. A checker whose dictionary language matches the named `SpellCheckCulture`.
2. `SpellCheckEnabled = true`.
3. A live, UI-hosted text control with canonical text.

Enabling the property does **not** install or create a dictionary. Missing
Windows language dictionaries are a normal discovery result, not proof that
the rendering control is broken.

## Attach Windows spelling

Create the provider in regular application code, **not**
`InitializeComponent`. Initialize after the Form is loaded and has a live
UI synchronization context. Own a cancellation token for the Form/controller
lifetime, and keep the provider alive as long as any of its checkers are used.

```csharp
using System.Globalization;
using WarpToolkit.Windows.Spelling;
using WarpToolkit.WinForms.DirectX.Controls;

private readonly CancellationTokenSource _lifetime = new();
private WindowsSpellingProvider? _provider;
private WindowsSpellChecker? _activeChecker;

private async Task InitializeSpellingAsync()
{
    _provider ??= new WindowsSpellingProvider();
    CultureInfo language = _editor.SpellCheckCulture;
    WindowsSpellCheckerCreationResult result =
        await _provider.CreateSpellCheckerAsync(language, _lifetime.Token);
    _lifetime.Token.ThrowIfCancellationRequested();

    if (result.SpellChecker is not { } checker)
    {
        FluentMessageBox.Show(
            $"Spelling unavailable for {language.Name}: {result.Status} {result.Error}",
            "Spelling", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
    }

    _activeChecker = checker;
    _editor.SpellCheckEnabled = true;
    UpdateCheckerAttachment();
    _editor.SpellingDictionaryCommandHandler = AddWordAsync;
}

private void UpdateCheckerAttachment()
{
    _editor.SpellChecker = _editor.SpellCheckEnabled && _editor.Text.Length > 0
        ? _activeChecker
        : null;
}
```

**Current native-backend edge case:** Windows `ISpellChecker.Check("")`
rejects a zero-length check with `E_INVALIDARG`. Keep the enabled preference
and active dictionary separately; call `UpdateCheckerAttachment` on
`TextChanged`, after initialization and after options changes. This detaches
the checker for an empty document and reattaches it when text is entered.
Use an identity check before assigning if repeated property assignments are
expensive. Do not swallow a native exception and manufacture successful
diagnostics. This guard is needed until the Windows provider handles empty
requests itself.

Use a named culture, e.g. `CultureInfo.GetCultureInfo("en-US")`; the invariant
culture is invalid for checking. `DiscoverAsync(token)` returns installed
dictionary information. `CreateSpellCheckerAsync` reports `MissingLanguage`
or `ProviderUnavailable` when appropriate; inspect `Status` and `Error`.
Do not silently fall back to another language.

Await initialization inside an async event boundary that catches and reports
unexpected exceptions. Catch cancellation only when the lifetime token was
actually canceled.

For a language switch: detach the old checker, set `SpellCheckCulture`, create
the matching checker, then attach it. Serialize options application, or use a
generation/requested-language check so a delayed initialization cannot assign
an old-language checker after a second change.

## Presentation and correction menu

```csharp
_editor.SpellingUnderlineColor = Color.Empty; // theme red
_editor.SpellingUnderlineAmplitude = 0.5f;    // DIPs
_editor.SpellingOverviewMarkersEnabled = true;
_editor.SpellingContextMenuEnabled = true;
```

Keep overview dots independent of underline visibility. Use theme-following
colors rather than an unconditional light-mode palette.
The `D2DTextBox` wrapper supplies right-click and keyboard context menus.
Corrections depend on capabilities advertised by the dictionary, the accepted
current diagnostic snapshot and the selected canonical source range.

Add to Dictionary is a **host/provider operation**, not a backend dependency
inside the renderer. Set `SpellingDictionaryCommandHandler`:

```csharp
private async ValueTask<bool> AddWordAsync(
    D2DSpellingDictionaryCommand command,
    CancellationToken cancellationToken)
{
    if (command.Checker is not WindowsSpellChecker checker)
    {
        throw new InvalidOperationException("Expected a Windows dictionary.");
    }

    WindowsDictionaryMutationStatus status =
        await checker.AddWordAsync(command.Word, cancellationToken);
    if (status != WindowsDictionaryMutationStatus.Success)
    {
        throw new InvalidOperationException($"Adding the word failed: {status}.");
    }

    return true;
}
```

Use `command.Checker`, not whichever checker happens to be attached later:
the command captures the dictionary, source version and generation that the
menu referred to. Return true only after a successful mutation. Report
failures at the application boundary or let the wrapper's failure event reach
that boundary; never return success on an error.

`WindowsSpellChecker` also exposes `IgnoreWordAsync` and `RemoveWordAsync`.
Respect `SpellingDictionaryCapabilities` and mutation statuses instead of
assuming every dictionary supports every command. Adding a word changes the
user's Windows dictionary; deterministic tests must not mutate real dictionaries.

## Hosted TextBoxBase elements

For a non-`D2DTextBox` host, assign the already-created checker to the actual
retained WARP text element:

```csharp
WarpToolkit.WinForms.DirectX.Controls.TextBoxBase element = textElement;
element.SpellCheckCulture = checker.Dictionary.Identity.Language;
element.SpellChecker = checker;
element.SpellCheckEnabled = true;
element.SpellingOverviewMarkersEnabled = true;
```

The element needs a real D2D host and dispatcher; do not treat it as an
independent Form control. Context-menu and dictionary UI must be implemented
by that host when it does not use `D2DTextBox`.
For chat, use `ChatView.InitializePromptSpellCheckingAsync()` and observe
`PromptSpellingStatus` / `PromptSpellingStatusChanged`; an application-supplied
prompt checker is retained. See `ChatView.Spelling.cs` and the AI/chat skill.

## Failures and lifetime

Subscribe to `SpellingProviderFailed` and log/report `e.Exception`.
`SpellingDiagnostics` contains the immutable snapshot accepted for the current
canonical text, not a mutable collection to edit.
`await _editor.WaitForSpellingIdleAsync()` awaits pending checks **and**
UI publication; do not synchronously block the UI waiting for it.

Before final Form disposal:

```csharp
_editor.SpellChecker = null;
_activeChecker = null;
_editor.SpellingDictionaryCommandHandler = null;
_lifetime.Cancel();
if (_provider is not null)
{
    await _provider.DisposeAsync();
    _provider = null;
}
```

Also await outstanding initialization tasks. Do this in an asynchronous,
guarded close sequence: cancel the first `FormClosing`, confirm/save changes,
detach spelling, await cleanup, then close again under a completion flag.
Only dispose the lifetime token source after its users have finished.
`Dispose()` is synchronous and can block native COM cleanup: prefer
`DisposeAsync()` on UI threads. A native call already in progress cannot be
interrupted; a timeout is something to report, not permission to abandon
cleanup and claim success. Do not move native spelling interfaces into arbitrary
`Task.Run` calls; the provider already owns the correct apartment.

## Backend-neutral checks and custom providers

`ISpellChecker` requires a `Dictionary` and:

```csharp
ValueTask<SpellingCheckResult> CheckAsync(
    SpellingCheckRequest request,
    CancellationToken cancellationToken = default);
```

For an explicit headless check:

```csharp
var request = new SpellingCheckRequest(
    text, checker.Dictionary.Identity.Language,
    sourceDocumentVersion: 1, generation: 1);
SpellingCheckResult result = await checker.CheckAsync(request, token);
```

Ranges are half-open **UTF-16 source offsets**, not visual columns, grapheme
counts or Markdown-rendered offsets. Results must cover only the requested
range, be non-overlapping, and preserve request identity. Return
`new SpellingCheckResult(request, issues)` so validation enforces the contract.
Windows range boundaries must not split a surrogate pair.

For a custom host without the integrated D2D adapter, `SpellingEngine` supplies
debounce, cancellation and generation/source-version checks:
`RequestCheck(checker, immutableText, documentVersion, culture)`,
`SnapshotChanged`, `ProviderFailed`, and `CurrentSnapshot`.
Dispatch UI changes from engine events through the host's UI dispatcher.
The engine defaults to a 250 ms debounce; do not create another engine for an
already-integrated editor.

## Focused validation

- Fake `ISpellChecker` providers should return deterministic, validated results.
- Verify stale checks cannot replace diagnostics after text/language changes.
- Test UTF-16 source ranges, cancellation, provider failure and disposal.
- Use `WaitForSpellingIdleAsync` with a pumping UI, not sleeps to guess completion.
- Discovery/native smoke checks may inspect installed dictionaries but must
  explicitly handle missing languages and never require dictionary mutation.

## Verified source map

- `src\WarpToolkit.Text.Spelling\ISpellChecker.cs`
- `src\WarpToolkit.Text.Spelling\SpellingCheckRequest.cs`
- `src\WarpToolkit.Text.Spelling\SpellingCheckResult.cs`
- `src\WarpToolkit.Text.Spelling\SpellingEngine.cs`
- `src\WarpToolkit.Windows.Spelling\WindowsSpellingProvider.cs`
- `src\WarpToolkit.Windows.Spelling\WindowsSpellChecker.cs`
- `src\WarpToolkit.WinForms.DirectX\D2DElements\TextBoxBase.Spelling.cs`
- `src\WarpToolkit.WinForms.DirectX\Controls\D2DTextBox.cs`
- `src\WarpToolkit.WinForms.DirectX\Controls\D2DSpellingDictionaryCommand.cs`
- `src\WarpToolkit.WinForms.Chat\ChatView\ChatView.Spelling.cs`
- `samples\cs\TurboGraphics\TurboGraphics\Prototyping\Prototypes\TextEditor\FrmWarpPad.cs`
