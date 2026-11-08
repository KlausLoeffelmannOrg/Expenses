# Expenses - U.S. Expense Recordkeeping Work Order

## Purpose and boundaries

Build a Windows desktop expense-recordkeeping tool for a self-employed individual
or single-member limited liability company (LLC). Organize expense records and
supporting receipts into monthly, quarterly, or annual **bookkeeping periods**.

These are organizational periods, not separate federal income tax years, tax
returns, or quarterly estimated tax payments. A single-member LLC's federal tax
treatment depends on its elections and circumstances; do not assume every LLC
files Schedule C or that creating a state group establishes state filing obligations.

The initial release records expenses paid in U.S. dollars using a date-paid
workflow. It is not a general ledger, reconciliation system, income tracker,
depreciation engine, sales-tax calculator, or tax-return preparation product.

## Terminology and record content

| Original wording | Application wording |
|---|---|
| Account/accounting period | Bookkeeping period |
| Booking item / item / record | Expense |
| Receipt | Supporting receipt/document, distinct from its expense record |
| State | State jurisdiction (optional two-letter abbreviation, including DC) |
| Commit | Save expense |
| Clipboard ring | Stashed expense drafts |
| Year report | Annual expense report |

Record date paid, payee, business purpose/description, expense category, project
or equipment tags, payment reference, amount in USD, business-use percentage,
and an optional supporting receipt file. Copy selected receipt files into managed
storage so that moving or deleting the source does not break saved evidence.
Payment references and receipts are supporting records, not automatic proof that
an expense meets every substantiation requirement.

Store money as exact integer cents; calculate each expense's business portion
using decimal arithmetic and round to cents. A business-use allocation is **not**
a determination of deductible expense. For example, vehicle fuel costs do not
establish eligibility for an actual-expense deduction or support taking both
actual costs and the standard mileage deduction. Equipment purchases may require
capitalization, depreciation, or other tax treatment. Have a qualified tax
professional determine deductibility and applicable substantiation requirements.

## Data and persistence

- Use EF Core entities as the source model, SQLite for storage, and checked-in
  EF Core migrations, a model snapshot, and a generated SQL migration script.
  EF Core calls this code-first/model-driven migration development; the legacy
  EF Designer/EDMX "Model First" workflow is not part of EF Core.
- Identify each period by calendar year, period type, period number, and optional
  state jurisdiction. Allow identical dates for different states; reject duplicate
  identities within the same jurisdiction.
- Limit the date paid to the open bookkeeping period.
- Soft-delete expenses. Exclude deleted expenses from ordinary grids, reports,
  and exports while preserving the original row.
- Preserve named, timestamped expense drafts, including incomplete edits, between
  sessions. Detect edits against a record changed or deleted since it was opened.
- Preserve notes/reminders and their scope and dismissal state.

## Main window

- MenuStrip and command ToolStrip.
- Left pane: an owner-drawn, system-color-aware period/draft TreeView.
- Right upper pane: WARP FluentTabControl with WarpDataGridView results.
  Initial filters: All expenses, Fuel (expense category), and Sub Camera
  (comma-separated tag). Do not use a native TabControl.
- Right lower pane: a scrollable expense-details editor.
- Use text labels and ordinary text entry for textual values, masked date and
  USD amount entry, and NumericUpDown for numeric percentages. Do not use a
  month calendar or DateTimePicker.

### Editing contract

1. Selecting a grid row displays read-only details. An empty selection shows no
   expense entry form.
2. New expense / Ctrl+N starts a new record in the open period. Edit expense / F2
   or double-clicking a row edits the selected existing record. Ctrl+N deliberately
   follows the standard Windows "New" convention, resolving the original ambiguity.
3. During editing, block period, filter, selection, report, export, and options
   navigation. Require Save, Cancel, or Stash before another activity or exit.
4. Save validates typed values and persists the expense. Cancel discards the
   active working copy without changing the saved expense.
5. Stash prompts for a name and persists a timestamped draft without changing
   the original expense. The TreeView restores it for further editing.
   Canceling a restored draft leaves the persisted draft available; saving it
   removes the draft in the same database transaction.
6. Delete requires confirmation and only marks the expense as deleted.

## Commands

### File

- New bookkeeping period...
- Open bookkeeping period...
- Separator.
- Export as CSV...
- Export as PDF...
- Separator.
- Exit.

The period dialog selects a calendar year, Month/Quarter/Year, month or quarter
number where applicable, and optional state jurisdiction. Export all active
expenses in the open period, regardless of the currently selected filter.
CSV must escape fields and protect spreadsheet users against formula evaluation.
PDF is an actual multipage document containing expense detail and totals.

### Edit

- New expense (Ctrl+N), enabled only with an open period and visible details pane.
- Add note or reminder...
- Edit expense (F2), enabled only with a selected expense.
- Delete expense..., enabled only with a selected expense.
- Save expense (Ctrl+S), Cancel edit (Escape), Stash draft (Ctrl+Shift+S).

Global reminders appear at startup when enabled. Period reminders appear when
opening their period. Provide a View command to revisit active reminders, and
allow explicit dismissal without deleting their history.

### View

- Period expense report...
- Annual expense report... with a year selector.
- Active notes and reminders...

Reports group recorded amounts and business portions by category and show totals.
Annual reports include all bookkeeping periods and jurisdictions for the selected
calendar year. Warn that overlapping entries are not deduplicated; users must
not record the same expense in multiple groups and assume the annual total is
an unduplicated federal deduction. No report is an IRS form.

### Tools > Options

Use FluentTabControl and one UserControl per options category. Edit a draft;
only OK persists changes. Cancel, Escape, window close, or canceling a folder
picker must not change live settings.

- Storage: application settings folder, database folder, optional additional
  backup folder (OneDrive suggested when configured).
- Notes and reminders: show the previous session's active global reminders on
  next startup.
- Default settings to `%LOCALAPPDATA%\Expenses`, the database to its `Data`
  subfolder, and optional synced backups to `%OneDrive%\Expenses\Backups` when
  OneDrive is available.
- Keep a stable local settings-location pointer so a custom settings folder can
  be discovered at the next startup.
- Changing the database folder copies the current database and receipt documents
  to the new location without overwriting an existing database; retain the old
  database as a safety copy.

## UI state and session backups

Persist window bounds/state, splitter positions, last-open period, and grid
column widths after changes (short debounce allowed). Restore valid positions
within an available monitor's working area.

At normal session exit, create a timestamped local SQLite online-backup snapshot
and copy supporting receipts. If an additional backup folder is configured, make
another backup there. Do not copy a live SQLite database file directly or rely
on cloud synchronization as a database consistency mechanism. Surface backup
failures and require an explicit decision before exiting without a successful
session save. Abrupt process termination and power loss cannot run an exit backup.

## Acceptance criteria

- Periods and jurisdiction distinctions survive restart.
- Expenses, validation, guarded editing, soft deletion, and named drafts work.
- Fuel and Sub Camera tabs filter the same open period correctly.
- Global/period reminders respect their scope and dismissal state.
- Options cancellation leaves saved settings unchanged; accepted paths survive restart.
- CSV and multipage PDF files contain actual persisted period data and totals.
- Period/annual reports exclude deleted expenses and disclose aggregation limits.
- UI layout changes persist, and normal exit produces readable database backups
  with supporting receipt copies.
- Automated tests exercise persistence, migrations, monetary/date validation,
  draft handling, reports, exports, backups, and representative Windows UI flows.

## IRS terminology references

- [What kind of records should I keep?](https://www.irs.gov/businesses/small-businesses-self-employed/what-kind-of-records-should-i-keep)
  identifies payee, amount paid, proof of payment, date, and description/business
  expense purpose as important supporting information.
- [Publication 583: Starting a Business and Keeping Records](https://www.irs.gov/forms-pubs/about-publication-583)
- [Publication 463: Travel, Gift, and Car Expenses](https://www.irs.gov/forms-pubs/about-publication-463)

These references guide terminology and scope; they do not represent a claim of
IRS certification or tax-compliance automation.
