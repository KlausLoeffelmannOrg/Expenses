---
name: warp-winforms-ai
description: >-
  Adds AI chat features to a WARP-based WinForms app: the Designer-droppable
  AIChatClientComponent (WarpToolkit.WinForms.AI), the ChatView / ChatRendererControl
  UI (WarpToolkit.WinForms.Chat), prompt templates ([AITemplate]/[AITemplateSegment]),
  streaming parsing with ReturnTokenParser and ObservableChatHistory
  (WarpToolkit.Desktop.AI, WarpToolkit.Microsoft.Extensions.AI). Use for chat UI,
  structured prompts, streaming extraction or migrating from the older
  AIChatServiceBase/OpenAIChatService stack.
metadata:
  source: "WARP src\\WarpToolkit.WinForms.AI\\AIChatClientComponent; src\\WarpToolkit.WinForms.Chat\\ChatView; src\\WarpToolkit.Desktop.AI"
  versions: "../warp-api-decision-guide/references/versions.md"
---

# WARP WinForms AI

## Designer invariant (applies to every WARP surface)

- The Form/UserControl hosting `AIChatClientComponent` or `ChatView` is `partial`
  with a paired `.Designer.*`; parameterless ctor owns `InitializeComponent()`,
  DI ctor is `(IServiceProvider sp) : this()`.
- `InitializeComponent` only instantiates the component into `components`, sets
  serializable properties (Provider, ModelName, Temperature …) and emits
  `SetServiceProvider(this)`. No keys, no network, no `IChatClient` creation.
- Key/factory services are resolved by the component in `EndInit`/`OnLoad`
  through the Form façade; the Designer gets `NullServiceProvider`.
- Shape of record: `warp-winforms-application-builder`; serialization:
  `winforms-designer-code`.

## Packages

| Package | Role |
|---|---|
| `WarpToolkit.Desktop.AI` | Provider-agnostic: `WellKnownAIProviders`, `Parser.PromptBuilder`, `Parser.ReturnTokenParser`, `ChatHistory.ObservableChatHistory`, `ChatClients.AddWarpChatClientFactory/AddApiProviderKeyManager` |
| `WarpToolkit.Microsoft.Extensions.AI` | `Templates.[AITemplate]`/`[AITemplateSegment]`, `[AIGenerated]`, `IAIModelInfo`/`IAIModels` |
| `WarpToolkit.WinForms.AI` | `AIChatClientComponent : BindableServiceProviderComponent, ISupportInitialize` — the one Designer-droppable AI component |
| `WarpToolkit.WinForms.Chat` | `ChatView : UserControl`, `ChatRendererControl`, `ChatProcessor` |

View-models reference only `Desktop.AI` + `Microsoft.Extensions.AI`, never the
WinForms packages. The older `AIChatServiceBase`/`OpenAIChatService`/`AIChatHistory`/
`KeyRetrievalService` stack is **historical**; see
[contracts](references/contracts.md) for the migration inventory.

## DI registration (Program.cs)

```csharp
builder.Services.AddLocalKeyRetrievalService();                 // EnvironmentVariableKeyService
builder.Services.AddApiProviderKeyManager(keyDefinitionsBasePath);
builder.Services.AddWarpChatClientFactory();
builder.Services.TryAddScoped<ChatView>();                      // DI-resolved UserControl
```

`AIChatClientComponent` resolves keys and the chat-client factory through the
hosting Form's `IServiceProvider` façade; there is **no** `KeyRetrievalService`
or `ApiKeyGetter` property on the component.

## Designer-time wiring

```csharp
// ChattyForm.Designer.cs — inside InitializeComponent (serialization only)
components = new System.ComponentModel.Container();
_aiChatClient = new WarpToolkit.WinForms.AI.AIChatClientComponent(components);
_aiChatClient.Provider = WarpToolkit.Desktop.AI.WellKnownAIProviders.OpenAI;
_aiChatClient.ModelName = "gpt-4o-mini";
_aiChatClient.Temperature = 0.4F;
_aiChatClient.SetServiceProvider(this);        // Designer-emitted for BindableServiceProviderComponent
```

Serializable properties: `Provider`, `ModelName`, `ChatClientName`, `Endpoint`,
`DeploymentName`, `DeveloperPrompt`, `JsonSchemaString`, `RequestCodeBlockExtractions`,
`ResponseTextFormat`, `MaxOutputTokens`, `Temperature`, `TopP`, `TopK`, `Seed`,
`FrequencyPenalty`, `PresencePenalty`, `StopSequences`, `ConversationId`.

## Host Form (regular partial)

```csharp
// ChattyForm.cs — ChattyForm.Designer.cs holds InitializeComponent, Dispose, fields.
public partial class ChattyForm : Form, IServiceProvider
{
    private IServiceProvider? _serviceProvider;
    private ChatView? _chatView;

    public ChattyForm()
    {
        InitializeComponent();
    }

    public ChattyForm(IServiceProvider serviceProvider) : this()
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _serviceProvider = new DeferredServiceProvider(serviceProvider);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (_serviceProvider is null)
        {
            return;   // Designer path: nothing to compose.
        }

        _chatView = _serviceProvider.GetRequiredService<ChatView>();
        _chatView.AIUserChatService = _aiChatClient;
        _chatView.AIMetaDataService = _aiChatClient;
        _chatView.AutoInvokeMetaDataAgent = true;
        _chatView.DeveloperPrompt = "You are a senior WinForms developer. Use only WARP-Toolkit APIs.";
        _chatView.ExceptionLevel = ChatExceptionLevel.User;
        _chatView.Dock = DockStyle.Fill;
        _chatPanel.Controls.Add(_chatView);   // _chatPanel is a Designer-placed Panel
    }

    // IServiceProvider.GetService + DeferredServiceProvider: warp-winforms-application-builder.
}
```

`ChatView` is itself a DI-aware UserControl with a parameterless Designer ctor and a
`(IServiceProvider, ILocalKeyRetrievalService)` runtime ctor — resolve it, do not
`new` it. It exposes `AIUserChatService`/`AIMetaDataService` (`AIChatClientComponent?`),
`DeveloperPrompt`, `ModelName`, `ResponseTextFormat`, `RequestCodeBlockExtractions`,
`ChatHistory` (`ObservableChatHistory`), `ChatRenderer`, `PromptControl`,
`SendCommandAsync(...)`, and async events (`AsyncPromptInvokedAsync`,
`AsyncBeforeSaveChat`, `ChatItemAdded`, `InitializationCompleted`, `OperationFailed`).

## Calling the component directly

```csharp
// Regular async method on the Form or a service; never in a constructor.
private async Task AskAsync(string userText, CancellationToken ct)
{
    ObservableChatHistory history = new(developerPrompt: "Answer tersely.");
    history.AddUserMessage(userText);

    ObservableChatHistory result = await _aiChatClient.RequestResponseAsync(history, ct);
    // or stream:
    await foreach (string chunk in _aiChatClient.RequestStreamingResponseAsync(history, ct))
    {
        await AppendAsync(chunk);
    }
}
```

Events on the component: `ReceivedMetaData`, `ReceivedNextParagraph`,
`CodeBlockInfoProvided` (`AsyncEventHandler<…>`), `RequestDeveloperPrompt`.
`GetChatOptions()` builds `ChatOptions` from the properties;
`GetModelsAsync(ModelFeatures? filter, CancellationToken)` returns `IAIModels`
(OpenAI catalog only; filter not applied; do not pick a "best vetted model" from
metadata the provider does not supply). `InvalidateResolvedChatClient()` forces
re-resolution after configuration changes.

## Prompt templates, streaming parser, history

Details and verified signatures: [contracts](references/contracts.md).

- `[AITemplate(Prompt = …, ProvideDate = true)]` on the request type;
  `[AITemplateSegment(Purpose = SegmentPurpose.Request, Prompt = …)]` on members —
  `Purpose` is the `SegmentPurpose` enum, not a caption string.
- `PromptBuilder.GetTypePrompt<T>()`, `GetRequestParameters(request, indentLevel, indentation)`,
  `GetTypePromptInfo`, `GetJSonSchema` (actual casing). `GetPreamblePrompt` is private.
- `ReturnTokenParser.ProcessTokens(stream, onReceivedMetaDataAsyncFunc, onReceivedNextParagraphAsyncFunc,
  onCodeBlockInfoProvidedAsyncFunc)` returns an async enumerable — enumerate it or nothing runs.
  `ChatProcessor` already does this inside `ChatView`; do not run a second parser.
- `ObservableChatHistory : ObservableCollection<ChatMessage>`: `AddUserMessage`, `AddMessage`,
  `SyncFrom(ChatResponse)` (prefix-merge), `Transcript`. Mutate only on the UI context.
- `[AIGenerated(guid, timeStamp, confidence)]` must survive Roslyn rewrites.

## Rules

- One `AIChatClientComponent` per logical client; configure it in the Designer, not
  by `new`-ing `IChatClient`s in the Form.
- Never hard-code keys; `EnvironmentVariableKeyService.TryGetAiProviderKey` for the
  "not configured yet" path (`warp-app-services`).
- `async void` only for conventional UI event handlers whose body awaits one
  `Task` method that owns the `try/catch`; route errors via `IDialogService`.
- Do not reuse `ChatOptions` across providers without re-reading provider limits.
- Do not mutate `ChatHistory` from background threads.

## Hand-offs

- Hosting, two-ctor Form shape, deferral: `warp-winforms-application-builder`.
- Keys, dialogs, settings: `warp-app-services`.
- Tabs hosting `ChatView`: `warp-fluent-tab-control`; console pane: `warp-console-diagnostics`.
- Spell checking in the prompt editor: `warp-text-spelling`.

## Final checks

1. Host surface is a Designer pair with both ctors; component is in `components` with `SetServiceProvider(this)` emitted.
2. No key/network/IChatClient work in constructors or `InitializeComponent`; `OnLoad` guards the null provider.
3. Member names match the current component (`Provider`, `ModelName`, `RequestResponseAsync`, `IAIModels`), not the historical stack.
