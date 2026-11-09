# WARP package versions — single policy

All WARP skills defer to this file. No skill states a "latest" version in its body.

## Policy

1. **The project decides the version.** Use the `WarpToolkit.*` version already pinned
   in the consuming `.csproj` / `.vbproj` / `Directory.Packages.props`. Never bump a
   project to a newer preview because a skill mentions one.
2. **New projects:** take the version the user names, or the version used by the
   sibling projects in the same solution. If neither exists, ask once; do not guess.
3. **All `WarpToolkit.*` packages in one solution use the same version.** Mixed
   previews produce binding failures and Designer load errors.
4. **Verification baseline ≠ requirement.** The table below records which source
   revision/preview a skill's API statements were verified against. It is a lower
   bound for the API shapes described, not an instruction to upgrade.
5. If the project's version is *older* than a skill's baseline, verify the member
   exists (Object Browser / metadata) before using it; otherwise fall back to the
   installed API.

## Verification baselines (observed in the skill set)

| Skill | Verified against |
|---|---|
| warp-binding-autoview, warp-commands-mvvm, warp-console-diagnostics, warp-containers-layout, warp-data-grids, warp-git-and-github-api, warp-input-controls, warp-messaging, warp-symbols, warp-winforms-extensions, warp-wizard, warp-windows-maps-experimental | WARP source revision `0b5b59bca1cd1ff93c2370ba1c2c9c7f051a63a5` (preview line `0.9.324-preview.*`) |
| warp-api-decision-guide, warp-app-services, warp-winforms-application-builder, warp-fluent-tab-control, warp-directx-getting-started, warp-directx-dcomp-exclusive-tlw, warp-winforms-controls (router) | preview line `0.9.323-preview.*` / `0.9.324-preview.*` |
| warp-directx-using-visuals, warp-directx-internal-approaches, warp-directx-text-editors | preview line `0.9.219-preview.*` — DirectX surface area; verify `UseDirectCompositionExclusiveRendering` / visuals API against the installed package |
| warp-toolstrip-icons, warp-symbols (symbol ToolStrip hosts, `WinFormsSymbolCache.Shared` public, Tree/List/Icon helpers), warp-app-services (`ApplyFormPlacement`/`SaveFormPlacement` pixel schema), warp-api-decision-guide font/DPI/window contract | installed `0.9.852-preview.g888af68ad8` and WARP source `a586853b` (2026-11-06) |
| warp-winforms-ai | legacy `0.9.7-preview.*` narrative; API statements re-verified against current source (`AIChatClientComponent`) |
| warp-git-and-github-api | `WarpToolkit.WinForms.Github`; earlier narrative pinned `0.9.79-preview.*` |

## Package ↔ namespace map (stable across the above baselines)

| Package | Key namespaces |
|---|---|
| `WarpToolkit.WinForms` | `WarpToolkit.WinForms` (FluentMessageBox), `.Containers` (FluentTabControl, AutoView factories), `.Containers.Adorners`, `.Containers.Wizard`, `.Controls`, `.Grid`, `.Symbols` (FontSymbol, FluentSymbols, SymbolTreeNode, ApplySymbols/ToIcon helpers), `.ToolStrips` (SymbolToolStrip/MenuStrip/StatusStrip + Symbol* items, 0.9.852+), `.Experimental.Binding`, `WinForms.PowerTools.Controls` (TransparentPanel) |
| `WarpToolkit.WinForms.Extensions` | `WarpToolkit.WinForms.Extensions.UI` (ConfigureItem, ShowDialogAsync, UI settings helpers), `.Extensions.AutoView` |
| `WarpToolkit.WinForms.Specialized` | ConsoleControl, HexAsciiDumper, FilenameDisambiguator, WarpDataGridView |
| `WarpToolkit.WinForms.AppServices` | `Microsoft.Extensions.WinForms` (WinFormsApplication/Builder), `WarpToolkit.WinForms.AppServices` (EnvironmentVariableKeyService), `.ServiceExtensions` |
| `WarpToolkit.Desktop.AppServices` | IDialogService, IModalDialogResult<T>, ISyncContextService, IWinFormsAppExceptionService, ViewModelBase |
| `WarpToolkit.Desktop.AI` | WellKnownAIProviders, `.Parser` (PromptBuilder, ReturnTokenParser), `.ChatHistory` (ObservableChatHistory), `.ChatClients` (AddWarpChatClientFactory, AddApiProviderKeyManager) |
| `WarpToolkit.Microsoft.Extensions.AI` | `.Templates` ([AITemplate], [AITemplateSegment]), [AIGenerated], IAIModelInfo |
| `WarpToolkit.WinForms.AI` | AIChatClientComponent |
| `WarpToolkit.WinForms.Chat` | ChatView, ChatRendererControl, ChatProcessor |
| `WarpToolkit.WinForms.DirectX` | D2DForm, D2DControl, D2DPanel, visuals |
| `WarpToolkit.WinForms.Github` | `.Services` (AddGitServices, AddGitHubGcmClientServiceFactory) |
| `WarpToolkit.Windows.Spelling` | spelling providers (project reference in-repo) |
| `WarpToolkit.Desktop.Roslyn` | VB→C# conversion library (no dedicated skill in this set) |
