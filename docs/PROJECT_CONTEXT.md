# MoneyTracker — Project Context

Reference document for the design. Read this before writing code; `CLAUDE.md` is the short
rulebook derived from it.

- **Course project:** console-based money tracker
- **Platform:** .NET 10 (LTS), C# 14
- **UI:** plain `System.Console`, numbered menus
- **Persistence:** JSON file, autosaved after every change
- **Currency:** SEK only, `sv-SE` formatting

---

## 1. Requirements

From the brief:

| # | Requirement | Where it is satisfied |
|---|---|---|
| 1 | Model an item with title, amount, month, distinguishing income from expense | `Transaction` abstract base with `Income` / `Expense` subclasses |
| 2 | Display a collection sortable ascending/descending by month, amount or title | `TransactionService.GetTransactions(filter, sortBy, direction)` |
| 3 | Display only expenses or only incomes | `TransactionFilter` enum, same method |
| 4 | Edit and remove items | `TransactionService.Update` / `.Remove` |
| 5 | Text-based user interface | `MoneyTracker.ConsoleApp` |
| 6 | Load and save the item list to file | `ITransactionRepository` + `JsonTransactionRepository` |

Extra features included for flair:

- Balance summary (total income, total expenses, net balance), overall and per month.
- Autosave after every add/edit/delete, not only on quit.
- Atomic file writes, so a crash mid-save cannot corrupt the data file.
- Colored success/warning/error messages.
- Amount input accepts both `1234,50` and `1234.50`.

Planned for later (see §9): unit tests, a full colorful UI overhaul, per-user login.

---

## 2. Solution layout

```
MoneyTracker/
├── MoneyTracker.slnx                     # solution file (.NET 10 XML format)
├── Directory.Build.props                 # shared MSBuild settings for all projects
├── .gitignore
├── README.md
├── docs/
│   ├── PROJECT_CONTEXT.md                # this file
│   └── class-diagram.md                  # Mermaid UML
├── src/
│   ├── MoneyTracker.Core/                # classlib — domain + business logic
│   │   ├── Models/
│   │   │   ├── Transaction.cs
│   │   │   ├── Income.cs
│   │   │   ├── Expense.cs
│   │   │   ├── YearMonth.cs
│   │   │   └── BalanceSummary.cs
│   │   ├── Enums/
│   │   │   ├── TransactionFilter.cs
│   │   │   ├── SortField.cs
│   │   │   └── SortDirection.cs
│   │   ├── Abstractions/
│   │   │   └── ITransactionRepository.cs
│   │   ├── Services/
│   │   │   └── TransactionService.cs
│   │   └── Exceptions/
│   │       ├── TransactionNotFoundException.cs
│   │       └── DataStoreException.cs
│   ├── MoneyTracker.Infrastructure/      # classlib — persistence
│   │   ├── Json/
│   │   │   ├── JsonTransactionRepository.cs
│   │   │   └── YearMonthJsonConverter.cs
│   └── MoneyTracker.ConsoleApp/          # console — UI + composition root
│       ├── Program.cs                    # composition root only
│       └── UI/
│           ├── MainMenu.cs
│           ├── ConsoleInput.cs
│           ├── TransactionTable.cs
│           └── ConsoleMessage.cs
└── tests/
    └── MoneyTracker.Core.Tests/          # xunit (added now, filled in later)
```

### Project references

```
ConsoleApp ──► Core ◄── Infrastructure
     └────────────────────►┘
Tests ──► Core
```

`Core` references nothing but the base class library. This is the dependency inversion
principle: the domain defines `ITransactionRepository`, and `Infrastructure` implements it.

**What the compiler enforces.** `Core` cannot name a type that lives in `Infrastructure` or
`ConsoleApp`. Referring to one is a build error, not a code-review note: CS0246 for a bare type
name it cannot resolve at all (`JsonTransactionRepository`), CS0234 for a qualified one where
the outer namespace resolves but the next segment does not
(`MoneyTracker.Infrastructure.JsonTransactionRepository`). The same goes for `Infrastructure`
reaching into `ConsoleApp`. That is the layering rule made structural.

**What it does not enforce.** `System.Console` and `System.IO.File` live in the shared
framework and are visible from every project, `Core` included. "No `Console.*` outside `UI/`"
and "no `File.*` outside `Infrastructure/`" stay conventions, enforced by `CLAUDE.md` and by
reading the diff — not by the build.

### `Directory.Build.props`

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <InvariantGlobalization>false</InvariantGlobalization>
  </PropertyGroup>
</Project>
```

`InvariantGlobalization` must stay `false`, because the app formats currency with `sv-SE`.

---

## 3. Domain model

### `Transaction` (abstract)

| Member | Type | Notes |
|---|---|---|
| `Id` | `int` | Auto-increment, assigned by `TransactionService`, unique and stable |
| `Title` | `string` | Trimmed, non-empty, max 35 chars |
| `Amount` | `decimal` | Always **positive**, rounded to 2 decimals |
| `Month` | `YearMonth` | Year + month |
| `SignedAmount` | `decimal` | **abstract** — the polymorphic hook |

`Income.SignedAmount => Amount`, `Expense.SignedAmount => -Amount`.

This is the answer to requirement 1. The amount itself is never negative; the *subclass*
decides what the amount means. Summaries then reduce to `transactions.Sum(t => t.SignedAmount)`
with no type checks, no `if (isExpense)`, and no enum switch anywhere.

`Update(title, amount, month)` mutates an existing instance in place and re-validates. `Id` is
never changed after construction.

Both subclasses are `sealed`. The hierarchy is deliberately closed — a third kind of
transaction would be a new requirement, not an extension point.

### `YearMonth`

A `readonly record struct` with `Year` and `Month`, implementing `IComparable<YearMonth>`.

Using a purpose-built type instead of a bare `int Month` prevents January 2026 and January 2027
from collapsing into the same bucket, and makes sorting by month correct by construction.
`DateOnly` would be wrong here: a transaction belongs to a month, not a day, and storing a day
would invent precision the user never entered.

- `ToString()` → `"2026-09"`
- `TryParse` accepts `"2026-09"`, `"2026-9"`
- Validation: year 1900–2999, month 1–12

### `BalanceSummary`

A `readonly record struct` carrying `TotalIncome`, `TotalExpenses` and `Balance`. Returned by
`TransactionService.GetSummary`, so the UI never does arithmetic itself.

### Enums

`TransactionFilter { All, IncomesOnly, ExpensesOnly }`,
`SortField { Month, Amount, Title }`,
`SortDirection { Ascending, Descending }`.

These exist so the UI passes intent rather than strings or magic numbers.

---

## 4. `TransactionService`

The single entry point to all business logic. Holds the in-memory list, owns ID assignment, and
persists after every mutation.

```csharp
public IReadOnlyList<Transaction> GetTransactions(
    TransactionFilter filter = TransactionFilter.All,
    SortField sortBy = SortField.Month,
    SortDirection direction = SortDirection.Ascending);

public Transaction? FindById(int id);
public Income  AddIncome (string title, decimal amount, YearMonth month);
public Expense AddExpense(string title, decimal amount, YearMonth month);
public Transaction Update(int id, string title, decimal amount, YearMonth month);
public void Remove(int id);
public BalanceSummary GetSummary(YearMonth? month = null);
```

**Construction.** The constructor takes `ITransactionRepository`, calls `Load()` once, and sets
`_nextId = existing.Count == 0 ? 1 : existing.Max(t => t.Id) + 1`. IDs therefore stay unique
across restarts and are never reused, even after deletions.

**Sorting.** Title sorting uses `StringComparer.CurrentCultureIgnoreCase` so that å, ä and ö
sort where a Swedish user expects them. Sorting by amount uses `SignedAmount`, not `Amount`, so
the largest expense and the largest income land at opposite ends of the list rather than next to
each other.

**Returns.** Every read returns `IReadOnlyList<Transaction>`, and the internal `List<Transaction>`
is never handed out. Callers cannot mutate state behind the service's back.

**Persistence.** Every mutating method ends with a private `Persist()` call. There is no separate
"save" command in the domain — the file is always current.

---

## 5. Persistence

### Contract

```csharp
public interface ITransactionRepository
{
    IReadOnlyList<Transaction> Load();
    void Save(IEnumerable<Transaction> transactions);
}
```

Deliberately tiny. Swapping JSON for SQLite, or one file per user, means one new implementation
and one changed line in `Program.cs`.

### `JsonTransactionRepository`

Uses `System.Text.Json` with polymorphic serialization, configured on the base type:

```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Income),  "income")]
[JsonDerivedType(typeof(Expense), "expense")]
public abstract class Transaction { ... }
```

Each subclass gets a `[JsonConstructor]` whose parameter names match the property names, which is
how `System.Text.Json` populates types with non-public setters.

`YearMonthJsonConverter` lives in `Infrastructure` and is registered on the options object rather
than declared with an attribute on `YearMonth`. Serialization concerns stay out of `Core`, and the
file stays readable: `"month": "2026-09"` instead of a nested object.

Resulting file shape:

```json
[
  { "type": "income",  "id": 1, "title": "Salary",    "amount": 32000.00, "month": "2026-09" },
  { "type": "expense", "id": 2, "title": "Rent",   "amount": 9500.00,  "month": "2026-09" }
]
```

**Location.** `src/MoneyTracker.Infrastructure/data/transactions.json`. `Program.cs` resolves this
by walking up from `AppContext.BaseDirectory` (which sits under `bin/<Config>/net10.0/`) back into
the source tree, so the file is committed to the repo — including its seed data — instead of being
left behind in the gitignored build output. The directory is created on first save.

**Missing or empty file** → return an empty list. A first run is not an error.

**Atomic writes.** Serialize to `transactions.json.tmp`, then `File.Move(tmp, path, overwrite: true)`.
Because the app autosaves on every keystroke-level change, a crash mid-write is a real risk; this
makes it impossible to end up with a half-written file.

**Corrupt or unreadable file** → wrap the `JsonException` / `IOException` in `DataStoreException`
and rethrow. `Program.cs` catches it, reports it clearly, and exits rather than starting up with
silently empty data.

---

## 6. Console UI

`Program.cs` is the composition root and contains no business logic:

```csharp
var path = Path.GetFullPath(Path.Combine(
    AppContext.BaseDirectory, "..", "..", "..", "..",
    "MoneyTracker.Infrastructure", "data", "transactions.json"));
var repository = new JsonTransactionRepository(path);
var service = new TransactionService(repository);
new MainMenu(service).Run();
```

Main menu:

```
=== MoneyTracker ===
Balance: 22 500,00 kr   (income 32 000,00 kr · expenses 9 500,00 kr)

1. Show transactions
2. Add income
3. Add expense
4. Edit transaction
5. Remove transaction
6. Monthly summary
0. Quit

Select option (0 - 6):
```

Listing screen asks for filter, then sort field, then direction, then renders:

```
  ID  Type      Title                    Month      Amount
  ──  ────────  ───────────────────────  ───────  ──────────
   1  Income    Lön                      2026-09   32 000,00
   2  Expense   Hyra                     2026-09   -9 500,00
```

### UI rules

- **`ConsoleInput` never returns invalid data.** It re-prompts in a loop until the input parses
  and is in range. Invalid input is a normal event, not an exception.
- **Edit prompts show the current value** and keep it when the user presses Enter on an empty line.
- **Delete asks for confirmation** and shows the transaction before removing it.
- **`ConsoleMessage`** owns every colour change and always restores `Console.ResetColor()`.
- **No business logic in `UI/`** — no sorting, no filtering, no arithmetic. The UI asks the
  service and renders the answer.
- Amount parsing accepts `,` and `.` as the decimal separator.
- Currency rendering uses `CultureInfo.GetCultureInfo("sv-SE")`.

---

## 7. Validation and error handling

Validation happens twice, on purpose:

1. **`ConsoleInput`** re-prompts, so bad input never leaves the UI layer.
2. **The domain** guards its own invariants, because `Core` must stay correct when called by the
   test project or a future web UI that has no such prompts.

| Rule | Violation |
|---|---|
| Title not null/whitespace, ≤ 60 chars | `ArgumentException` |
| Amount > 0 and ≤ 1 000 000 000 | `ArgumentOutOfRangeException` |
| Month valid (1900–2999, 1–12) | `ArgumentOutOfRangeException` |
| ID exists | `TransactionNotFoundException` |
| File unreadable/corrupt | `DataStoreException` |

`MainMenu` catches `TransactionNotFoundException` and shows a friendly message. `Program.cs`
catches `DataStoreException` at the top level. Nothing catches `Exception` broadly.

---

## 8. Conventions

- File-scoped namespaces, one type per file, filename matches the type.
- `sealed` by default on concrete classes; `virtual`/`abstract` only where polymorphism is used.
- Nullable reference types enabled, warnings as errors; no `!` null-forgiving operator without a
  comment explaining why it is safe.
- `decimal` for money, never `double`.
- Private fields `_camelCase`; static readonly `s_camelCase`.
- Expression-bodied members for one-liners.
- XML doc comments on every public member of `Core`.
- LINQ for querying; no manual sort loops.
- Prefer `IReadOnlyList<T>` / `IEnumerable<T>` in public signatures over `List<T>`.
- No `Console` calls outside `ConsoleApp/UI/`. No `File` calls outside `Infrastructure/`.

---

## 9. Planned future features

Designed for now, built later.

**Unit testing.** `TransactionService` takes `ITransactionRepository`, so tests use an in-memory
fake and touch no files. First tests to write: `SignedAmount` sign per subclass; each sort field
in both directions; each filter; `Update` on a missing ID throws; `Remove` renumbers nothing;
`_nextId` survives a reload; `GetSummary` arithmetic; `YearMonth.TryParse` rejects month 13.

**Colorful UI overhaul.** All rendering already sits behind `TransactionTable` and
`ConsoleMessage`. Introducing Spectre.Console means rewriting those two classes and adding a
package reference to `ConsoleApp` only — `Core` and `Infrastructure` do not change.

**User login (insecure, by design).** One JSON file per user: `data/{username}.json`.
`JsonTransactionRepository` already takes a path, so the change is confined to `Program.cs`:
prompt for a username, sanitize it against path traversal, build the path, and construct the
repository. No domain change at all.

---

## 10. Build order

1. `Directory.Build.props`, solution, four projects, project references.
2. `Core`: `YearMonth` → `Transaction`/`Income`/`Expense` → enums → `ITransactionRepository`
   → exceptions → `TransactionService`.
3. `Infrastructure`: `YearMonthJsonConverter` → `JsonTransactionRepository`.
4. `ConsoleApp`: `ConsoleMessage` → `ConsoleInput` → `TransactionTable` → `MainMenu` → `Program`.
5. Manual end-to-end check: add, list, sort, filter, edit, remove, quit, restart, verify restored.
6. `Tests`: fill in from the list in §9.
