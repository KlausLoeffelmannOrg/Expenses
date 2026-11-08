# Expenses
Bookkeeping tool for managing receipts/expenses

Windows desktop expense recordkeeping for a self-employed individual or
single-member LLC, using .NET 11, WARP WinForms controls, and EF Core/SQLite.
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
