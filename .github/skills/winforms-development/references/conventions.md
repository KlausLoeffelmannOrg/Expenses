# Regular-code conventions and migration

## C# and file ownership

Use the project's language version. Modern C# in the main partial may use file-scoped namespaces, nullable references, pattern matching, records, target-typed `new`, and raw strings when the compiler supports them. Do not move that syntax into Designer serialization.

Prefer primitive aliases. Use `var` when the type is obvious. Nullable sender parameters match the framework. Use throw helpers when the target has them.

Property lifetime matters: `Items => new List<T>()` returns a new collection on each get; `Items { get; } = new()` keeps one instance; `Items => _items ??= new()` initializes lazily. Changing identity breaks bindings.

Document exposed APIs to the repository's standard. Avoid comments that repeat the code.

Forms and UserControls use a main partial and a designer partial. See [winforms-designer-code](../../winforms-designer-code/SKILL.md).

## VB

Application Framework is the default for a classic WinForms VB app. The startup fragment in [project-setup.md](project-setup.md) belongs in the existing `MyApplication` partial. Do not add `Program.vb` to that app. A WARP-hosted VB app is not this default: keep its `Program.vb` / `Sub Main` and leave the Application Framework off. See [project-setup.md](project-setup.md).

Use `Friend WithEvents` and `Handles` for designed control events. Existing `AddHandler` may stay. Event bodies live in the main partial.

The main form class is `Partial`. A non-partial `Public Class MainForm` cannot merge with `MainForm.Designer.vb`.

## Migration sequence

1. Inventory project format, framework, Windows version, architecture, packaging, resources, manifests, and Designer tooling.
2. Keep a baseline build. Scope framework and package upgrades explicitly.
3. Move to SDK startup only when requested. Translate legacy DPI settings into `ApplicationHighDpiMode` or the VB defaults event. Do not leave both a manifest DPI setting and a second `SetHighDpiMode` call.
4. Introduce nullable and language conventions in regular code without rewriting generated serialization.
5. Modernize async, binding, and theme behavior through their owners. Do not replace a working event model with MVVM unless asked.
6. Rebuild and round-trip Forms and UserControls. Test font, DPI, keyboard, native, and deployment paths.

Renaming a generated file is not a VB6 migration. Map events, validation, data access, resources, and control lifetime explicitly.

## Version-sensitive features

Use the target's reference assembly for overloads and obsoletions. .NET 10 typed clipboard and capture policy need an explicit product decision. .NET 11 renderer and kiosk APIs need that target. Neither is a reason to retarget every project.
