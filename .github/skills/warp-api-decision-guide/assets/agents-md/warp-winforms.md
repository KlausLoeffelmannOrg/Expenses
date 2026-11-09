# WARP-Toolkit addendum (AGENTS.md fragment)

<!-- Append below the WinForms base fragment (winforms-api-decision-guide/assets/agents-md/winforms-app.md),
     which already carries the surface inventory and the Designer invariant. Replace <placeholders> only. -->

## WARP packages

- Pinned `WarpToolkit.*` version: `<version from <App>.csproj / Directory.Packages.props>` —
  all WARP packages share it; never bump to a skill-mentioned "latest".
- Packages in use: `<WarpToolkit.WinForms, WarpToolkit.WinForms.AppServices, …>`.
- Startup: `<Program.cs | Program.vb (Sub Main, Application Framework off)>` using
  `WinFormsApplication.CreateBuilder(args)`; startup form `<MainForm>` has a parameterless ctor
  and a `(IServiceProvider) : this()` ctor.

## WARP runtime composition (per surface)

| Surface | Designer-placed WARP controls | Composed in OnLoad/EndInit |
|---|---|---|
| `<MainForm>` | `<FluentTabControl _tabs>` | `<_tabs.AddTab("Settings", _settingsView)>` |
| `<…>` | `<…>` | `<…>` |

## WARP font / DPI / window policy

- Scaling root: `<MainForm>` (`AutoScaleMode.Font`, app font via `UseDefaultFont` before geometry restore);
  hosted views are `AutoScaleMode.Inherit` without own Font.
- Runtime font change: Form `OnFontChanged` transaction (Normal only; skipped during `OnDpiChanged`);
  symbol images regenerated once afterwards from `<logical point size field>`, never `Font.SizeInPoints × DeviceDpi/96`.
- Window geometry: `<settings key>` DIP schema v`<n>` relative to monitor `WorkingArea.Location`; first run ≈75%/80% of
  the chosen monitor's working area; legacy `ApplyFormPlacement` pixels migrated at `<version>`.
- Command strips: `<SymbolToolStrip/SymbolMenuStrip>` with icons; text-only exemption: `<none | stated domain>`.

## WARP skill routes

- First: `warp-api-decision-guide` (mirrors `winforms-api-decision-guide` Step 0–2, then API).
- Hosting/DI two-ctor shape: `warp-winforms-application-builder`; services: `warp-app-services`.
- `<topic → warp-* skill, e.g. tabs → warp-fluent-tab-control>`

## WARP validation

- `dotnet build <App.csproj> --nologo`; open each changed surface in the Designer without the host running.
- `<project-specific validator>`