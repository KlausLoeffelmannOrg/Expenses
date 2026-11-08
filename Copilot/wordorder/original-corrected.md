# Expenses Application - Original Work Order (Corrected)

This version preserves the requested scope and original terminology, correcting
only grammar, spelling, and formatting.

## Basic application scope

- The application collects expense receipts for a freelancer or single-member LLC
  for monthly or quarterly bookkeeping.
- The client can create multiple accounting periods covering the same time span
  within a year for different states.
- Store data in a SQLite database using Entity Framework Core.
- Use a model-first approach to create entity objects and database migration scripts.
- Use appropriate features from the WARP library.

## Tools > Options

Use FluentTabControl for these settings, with reasonable default folders:

- Application settings storage folder.
- Database storage folder.
- Optional database backup folder in OneDrive.

Include a "Show the last session's notes at the next startup" setting.

## Main interface

- MenuStrip.
- ToolStrip.
- SplitContainer with a TreeView on the left. Owner-draw the TreeView so that dark
  mode is properly supported.
- A second SplitContainer on the right:
  - Upper pane: WarpDataGridView with FluentTabControl filter tabs. The displayed
    records change with the selected tab, such as all fuel receipts or all receipts
    related to "Sub Camera."
  - Lower pane: a details view for editing.

### Details and editing

- Display a record in the entry form only after a grid row is selected.
- Double-clicking a grid row, clicking the appropriate toolbar or menu command,
  or pressing Ctrl+N enters edit mode.
- Once editing begins, the user must commit, cancel, or stash the edited record
  in a clipboard-style ring. Each stashed record has a name and timestamp so that
  it can be found again.
- The New toolbar command creates a new booking item.
- Delete marks an item as deleted; it does not physically remove it.
- Use labels, masked-edit controls for money and date/time values, and
  NumericUpDown controls. Do not use CalendarControl or DateTimePicker.
- Avoid the native Win32 tab control; use FluentTabControl instead.
- Save UI settings, including column widths and window size and position, when
  they change, and restore them at the next startup.
- Back up the database at the end of every session.

## Menus

### File

- Create a new accounting period... Open a dialog to select a month, quarter, or
  year; the specific period; and, optionally, a state.
- Open accounting period... Open an existing period for editing.
- Separator.
- Export as CSV.
- Export as PDF.
- Separator.
- Quit.

### Edit

- New item/record: enabled when the details pane is visible; otherwise disabled.
- Add note/reminder:
  - Global scope: show the reminder when the application starts.
  - Period scope: show the reminder when the period opens for editing.
- Edit item: enabled when an item is selected.
- Delete item.

### View

- Period report...
- Year report...

## Work-order documentation

Save this corrected original work order and a separately rephrased work order
using appropriate U.S. bookkeeping and IRS terminology under `Copilot\wordorder`.
Adjust the application's UI terminology accordingly.
