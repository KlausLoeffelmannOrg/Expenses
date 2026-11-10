# Expenses
Bookkeeping tool for managing receipts/expenses

Windows desktop expense recordkeeping for a self-employed individual or
single-member LLC, using .NET 11, built-in WinForms controls, and EF Core/SQLite.
This is not tax-return preparation software; business-use amounts are not
automatically deductible amounts.

## Run and develop

Install the .NET 11 SDK on Windows, then:

```powershell
dotnet run --project src\Expenses\Expenses.App
dotnet test src\Expenses\Expenses.slnx
```

Create a bookkeeping period from File, then use New expense (Ctrl+N). F2 or
double-click edits a selected row. Save (Ctrl+S), Cancel (Escape), or Stash
(Ctrl+Shift+S) must finish an edit before navigating. Restore named drafts from
the left tree. The Fuel tab matches the Fuel category; Sub Camera matches that
comma-separated tag. Exports include all active expenses in the open period,
not just its selected tab.

Tools > Add demo data creates six fictional expenses in a separate unused annual
period. Tools > Options > Appearance sets the interface font and Small (24 px),
Medium (36 px), or High (48 px) command icons. Pixel sizes are relative to 9 pt at
96 DPI and scale once with the selected logical font size and monitor DPI.

## Foundry Local API

`Foundry.Local` is a reusable, UI-independent assembly with no NuGet dependencies.
`Foundry.Local.WinForms` supplies Designer-editable model-management and image
analysis views/dialogs. They use the installed **Foundry Local CLI 0.13 or later**
(on PATH), its structured JSON output, and its dynamically discovered loopback
HTTP endpoint. No Azure account or cloud inference is used; initial catalog,
model, or runtime downloads may need internet. The daemon/cache are shared with
other local apps, and this library never stops the daemon on disposal.

In model management, Download adds model files to the disk cache, Load loads them
into memory, Unload releases memory but retains files, and Remove from cache
deletes downloaded files. Review each model's license and hardware requirements.
Image operations require a **loaded Multimodal** model; text-only Chat models can
translate. No large model is downloaded implicitly.

```csharp
using Foundry.Local;

using var foundry = new FoundryLocalService();
var installation = await foundry.Runtime.DetectAsync();
var models = await foundry.Runtime.GetModelsAsync();
var model = models.First(m => m.Loaded && m.SupportsImages);
var image = await ImageInput.FromFileAsync(imagePath); // PNG/JPEG, <= 20 MiB
string description = await foundry.DescribeImageAsync(model.Id, image);
ReceiptExtraction receipt = await foundry.ExtractReceiptAsync(model.Id, image);
TranslationResult translation = await foundry.TranslateAsync(
    model.Id, "break a leg", TranslationLanguage.German);
```

`ExtractStructuredAsync<T>` accepts an image, extraction instructions, and a
`JsonElement` JSON schema for another app's OCR/qualification structure. All
operations accept cancellation tokens. Receipt results include vendor, purchase
date, line-item quantity/unit price/amount/confidence, subtotal, tax, total,
currency, original OCR text, and warnings. Unknown facts stay null. Confidence
values are **uncalibrated model estimates**, not probabilities or proof of
authenticity. Malformed, incomplete, or truncated structured responses fail
explicitly. Always review AI output before saving financial data.

Translation supports English, German, French, Italian, Danish, and Spanish in
either direction, with optional automatic source-language detection. Single
words return alternatives with example sentences. Idioms identify the source
and equivalent idiom (or explain when no equivalent is available). Other input
returns a plain translation.

For CLI 0.13, image inference deliberately uses `/v1/responses` with
`input_image.image_data` and `media_type`: its Chat Completions path does not
reliably forward images. Text inference uses `/v1/chat/completions`.
See Microsoft's [getting started guide](https://learn.microsoft.com/en-us/azure/foundry-local/get-started?pivots=programming-language-csharp)
and [Responses vision sample](https://github.com/microsoft/Foundry-Local/tree/main/sdk/cpp/sample/web-server-responses-vision).

Deterministic tests do not require Foundry. To opt in to live verification using
an already-loaded multimodal model and an in-memory synthetic receipt:

```powershell
$env:EXPENSES_FOUNDRY_LIVE = "1"
dotnet test src\Expenses\Expenses.Tests --filter "Category=FoundryLive"
```

Settings default to `%LOCALAPPDATA%\Expenses`, with the SQLite database and
managed receipts in `Data`. Tools > Options can relocate storage and configure
an additional backup folder. Keep the active database on a local disk; use
OneDrive for backup snapshots rather than live database synchronization.
UI settings are saved after layout changes; normal exit creates a database and
receipt backup in `Data\Backups` and the optional additional folder.

To restore a backup, close the application, keep a safety copy of the current
data, and copy the selected timestamped `.db` snapshot to the configured data
folder as `expenses.db`. Copy its matching `-Receipts` directory as `Receipts`.
Do not replace a database while the application is open.

## Database migrations

Entities define the EF model. The checked-in migration, snapshot, and initial
SQL script are in `src\Expenses\Expenses.Core\Migrations`. Startup applies
pending migrations.

```powershell
dotnet tool restore
dotnet ef migrations add MigrationName --project src\Expenses\Expenses.Core
dotnet ef migrations script --project src\Expenses\Expenses.Core
```

See the [corrected original work order](Copilot/wordorder/original-corrected.md)
and [U.S. bookkeeping work order](Copilot/wordorder/us-bookkeeping-work-order.md)
for scope, terminology, tax caveats, and acceptance criteria.
