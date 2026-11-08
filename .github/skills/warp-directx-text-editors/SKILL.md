---
name: warp-directx-text-editors
description: "Use when choosing, embedding, or configuring WARP Direct2D text controls: D2DTextBox for input, D2DTextEditor for plain-text documents, D2DCodeEditor for classified code, D2DMarkdownEditor for raw Markdown, and D2DMarkdownPreview for rendered read-only Markdown. Covers Designer placement, editor options, document persistence, completion, preview synchronization, and when to use the separate spelling skill."
---

# WARP Direct2D text editors

## Choose the surface first

All control types below are in `WarpToolkit.WinForms.DirectX.Controls`.

| Need | Type | Important distinction |
| --- | --- | --- |
| Single-line input or a lightweight multiline field | `D2DTextBox` | TextBox-style `Text`, selection, clipboard and history; defaults to single-line. |
| Plain-text files, notes, logs, an unclassified programming document | `D2DTextEditor` | Inherits `D2DTextEditorBase`; adds document persistence, logical-line gutters, bookmarks/comments and an edit journal, without automatic classification. Used by TurboGraphics `FrmWarpPad`. |
| Language-aware source editing | `D2DCodeEditor` | Same document base, plus asynchronous classification and edit assistance. A language service/provider must be supplied for language classification. |
| Editable Markdown source with visible delimiters and attributed formatting | `D2DMarkdownEditor` | The underlying source is unchanged by formatting. This is **not** the rendered preview. |
| Rendered Markdown, links, images and tables, read-only | `D2DMarkdownPreview` | Assign `Markdown`, not `Text`. Pair with either source editor when an editable/preview split is wanted. |
| Custom retained text surface, not a WinForms control | WARP `TextBoxBase` | A hosted D2D element. WinForms `D2DTextBox` wraps one as `TextBoxElement`; it does **not** derive from `System.Windows.Forms.TextBoxBase`. |

Do not derive a new editor just to change these options. Use the shipped
controls and a regular `ConfigureEditor` method in the owning Form.

## Dependencies and source availability

Reference `WarpToolkit.WinForms.DirectX`. Its transitive Drawing and
Text.Spelling dependencies supply the rendering and backend-neutral contracts.
Add `WarpToolkit.WinForms.Roslyn` only for C#/VB Roslyn services; add
`WarpToolkit.Windows.Spelling` only for the Windows dictionary backend.

Current source targets include `net11.0-windows10.0.22000.0`: Windows 11 build
22000 still uses the `windows10.0` TFM platform version.

**Check the consuming version before using these APIs.** The WARP source may
contain newer APIs than the published DirectX package. In particular,
`WarpToolkit.Windows.Spelling.csproj` currently has `IsPackable=false`;
use a source `ProjectReference`, not a NuGet reference to a nonexistent package.
TurboGraphics demonstrates source references for the complete matching stack.
Do not mix old DirectX binaries with new Text.Spelling source contracts.

## Designer placement versus configuration

Create the actual shipped editor in `InitializeComponent` in the Form's
`.Designer.cs`, so the Designer can display and serialize it:

```csharp
_editor = new WarpToolkit.WinForms.DirectX.Controls.D2DTextEditor();
_editor.Name = "_editor";
_editor.Dock = DockStyle.Fill;
Controls.Add(_editor);
```

Declare its backing field as `D2DTextEditor`. Keep control flow, provider
construction, service resolution and document I/O out of Designer code.
Keep the parameterless constructor safe to execute in the Designer:

```csharp
public FrmMain()
{
    InitializeComponent();
}
```

Call configuration after `InitializeComponent` and after loading settings,
normally from `OnLoad`. With WARP hosting, retain the canonical two-constructor
pattern and defer component service resolution; see
`warp-winforms-application-builder`.

```csharp
private void ConfigureEditor()
{
    _editor.AutoWrap = true;
    _editor.ShowLineNumbers = true;
    _editor.ShowIndicatorMargin = true;
    _editor.VerticalScrollBarMode = D2DTextScrollBarMode.Wide;
    _editor.AutoCompleteEnabled = true;
    _editor.MinPrecedingCharsSuggestionRequestSensitivity = 2;
    _editor.MinTimeSuggestionRequestSensitivity = 0.35f;
}
```

Use theme-following defaults instead of assigning a fixed white background.
If creating fonts dynamically, keep ownership and dispose superseded fonts
only after all controls have stopped using them.

`D2DWinFormsClassic` is the normal rendering starting point. Background render
modes are not a prerequisite for accelerated text. Do not change render modes
merely because an editor has a typing/animation loop; consult the DirectX
rendering and COM-apartment skills before moving native work between threads.

## Configure the editor's options

The canonical comprehensive example is
`samples\cs\TurboGraphics\TurboGraphics\Options\EditorOptionsSnapshot.cs`.
Capture a detached options snapshot, edit a copy, validate it and apply
**only differences** on OK. Cancel must not mutate the live editor or document.

| Group | Relevant properties |
| --- | --- |
| Appearance/input | `Font`, `AutoWrap`, `ReadOnly`, `EmojiAutoDisplay`, `BorderStyle`, `ScrollBars`, `VerticalScrollBarMode` |
| Cursor/alignment | `SmoothSnapStrategy`, `SmoothSnapSpeed`, `CaretTypingPause`, `CaretBlinkHalfPeriod`, `CursorMovementStyle`, `CursorHighlightWhenUnfocused`, `CursorHighlightOnFocus`, `CursorHighlightLevel` |
| Completion | `AutoCompleteEnabled`, `MinPrecedingCharsSuggestionRequestSensitivity`, `MinTimeSuggestionRequestSensitivity`, `SuggestionFontStyle`, `SuggestionOpacity`, `SuggestionColor` |
| Spelling | `SpellCheckEnabled`, `SpellCheckCulture`, `SpellChecker`, `SpellingOverviewMarkersEnabled`, `SpellingContextMenuEnabled`, `SpellingUnderlineAmplitude`, `SpellingUnderlineColor` |
| Document editors only | `ShowLineNumbers`, `ShowIndicatorMargin`, `GutterPreviewDelay`, `DocumentEncodingKind`, `DocumentNewLineConvention`, `AutoFormatJson` |
| Diagnostics | `Diagnostics.Enabled`, `CopyReportToClipboard`, `InputLogLevel`, `IncludeTextInput` |

Do not persist consent to log typed text. The sample excludes `IncludeTextInput`
from JSON. Validate finite numeric values, valid enums, positive timing/font
sizes and a named culture before changing controls.

**Turning on completion or spelling alone is not enough:** attach completion
handlers or a spelling checker. See the completion recipe below and
`warp-text-spelling` for backend setup and lifetime handling.

## Document loading and saving

Use `D2DTextEditorBase` document APIs, not `File.ReadAllText` plus `Text`, when
encoding, newline conventions, clean state and metadata must survive:

```csharp
using WarpToolkit.WinForms.DirectX.TextEditing.Persistence;

string path = Path.GetFullPath(fileName);
var metadata = new ApplicationMetadataSidecarStore(path, "MyApplication");
await _editor.LoadDocumentAsync(path, metadata, cancellationToken);
await _editor.SaveDocumentAsync(
    path, metadata, cancellationToken: cancellationToken);
```

The metadata store keeps bookmarks, comments and session state separate from
the original text file. Choose a stable application name, not a new name on
each open.

For a fresh document, load a UTF-8 stream to reset history and document state:

```csharp
using MemoryStream content = new(Encoding.UTF8.GetBytes(initialText));
await _editor.LoadDocumentAsync(content, cancellationToken: cancellationToken);
```

Before prompting to discard/close, call
`_editor.TextBoxElement.FlushPendingTyping()` and inspect
`_editor.IsDocumentModified`. Before a new/open operation, confirm unsaved
changes. On save, update the current path/history only after success, and
check whether the document remained modified because edits occurred during I/O.
Do not indiscriminately force `Modified=false` after an asynchronous save.

Selection operations such as `SelectedText`, `Cut`, `Paste`, `Undo` and `Redo`
are the editing path; replacing a selection keeps an operation undoable.
Use `_editor.TextBoxElement.GoToLine(oneBasedLine)` for navigation.

## Markdown source and preview

Put a source editor and `D2DMarkdownPreview` into two Designer-created
`SplitContainer` panels. The source can be `D2DTextEditor` for literal text or
`D2DMarkdownEditor` for attributed raw Markdown. Do **not** replace editable
source with the preview's generated plain text.

```csharp
private void OnSourceChanged(object? sender, EventArgs e)
{
    _preview.Markdown = _editor.Text;
}

private void SetDocumentPath(string? fileName)
{
    _preview.DocumentBaseUri = fileName is null
        ? null
        : new Uri(Path.GetFullPath(fileName));
    _preview.Markdown = _editor.Text;
}
```

Wire the named `TextChanged` handler, perform an initial synchronization, and
refresh `DocumentBaseUri` after Open/Save As. Use the **file URI**, not an
arbitrary working directory, so relative images resolve beside the document.
Collapse the preview panel for non-Markdown documents or when disabled.
For very large documents, debounce host preview updates rather than compiling
the entire source on every keystroke.

The preview supports `MarkdownTemplate`, `LinkOpening`, `LinkOpenFailed`,
`ImageLoadFailed`, semantic links/anchors/images/tables and read-only navigation.
Surface image/link failures rather than suppressing them. Decide the external
link policy in the host.

For the raw Markdown editor:

```csharp
_markdown.WrapColumn = 80; // valid range: 30..200
_markdown.AutoWrap = true;
_markdown.ImageDocumentBaseUri = new Uri(Path.GetFullPath(fileName));
_markdown.ProjectImages = true;
```

`D2DMarkdownEditor` hides the base `AutoWrap` property to refresh paragraph
layout. Assign through the **Markdown-typed** reference when updating it.
`ProjectImages` projects images within source; it is not a separate preview.
`RevealImageSource()` and `RestoreImageProjectionAsync()` expose/restore image
syntax without changing the canonical Markdown source.

## Code classification

For C#/VB, with `WarpToolkit.WinForms.Roslyn`:

```csharp
using WarpToolkit.WinForms.Roslyn;

_codeEditor.LanguageId = "csharp"; // use "vb" for Visual Basic
_codeEditor.LanguageService = new RoslynCodeEditorService();
```

`LanguageService` takes precedence over `RunProvider`; do not assign both
expecting both to classify. XML-family tag completion uses the default
`XmlTagCompletionService`; select an appropriate `LanguageId`.
Do not promise Rust/Python/etc. classification merely because those file
extensions can be opened as plain text.

For C#/VB fenced code in Markdown, assign a
`RoslynClassificationRunProvider` to `FencedCodeRunProvider`, using a
theme-appropriate `ClassificationStyleMap`. See `FrmProtoMarkdownEditor.cs`.

## Inline completion

Subscribe to `AsyncRequestAutoCompleteSuggestion`. The handler returns a
`Task`; set `e.SuggestionText` to only the **untyped suffix**. Honor
`e.CancellationToken`, and report `AutoCompleteSuggestionFailed`.
Never perform blocking provider/network calls in the UI handler.

`CurrentParagraph` is the text preceding the caret in that paragraph; a
completion provider should inspect its trailing word-boundary prefix, not
require the entire paragraph to equal the start of a term.
TurboGraphics `WarpPadVocabulary.GetSuggestion` is a deterministic example.

## Verified source map

- `src\WarpToolkit.WinForms.DirectX\Controls\D2DTextBox.cs`
- `src\WarpToolkit.WinForms.DirectX\Controls\D2DTextEditor.cs`
- `src\WarpToolkit.WinForms.DirectX\Controls\D2DTextEditorBase.Persistence.cs`
- `src\WarpToolkit.WinForms.DirectX\Controls\D2DCodeEditor.cs`
- `src\WarpToolkit.WinForms.DirectX\Controls\D2DMarkdownEditor.cs`
- `src\WarpToolkit.WinForms.DirectX\Controls\D2DMarkdownEditor.Images.cs`
- `src\WarpToolkit.WinForms.DirectX\Controls\D2DMarkdownPreview.cs`
- `src\WarpToolkit.WinForms.Roslyn\Roslyn\RoslynCodeEditorService.cs`
- `samples\cs\TurboGraphics\TurboGraphics\Prototyping\Prototypes\TextEditor\FrmWarpPad.cs`
- `samples\cs\TurboGraphics\TurboGraphics\Options\EditorOptionsSnapshot.cs`
