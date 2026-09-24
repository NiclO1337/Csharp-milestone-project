# CLAUDE.md

Console money tracker. .NET 10 (LTS), C# 14. School project.
Full design: `docs/PROJECT_CONTEXT.md` — read it before making design decisions.

## Commands

```bash
dotnet build                                   # build all
dotnet run --project src/MoneyTracker.ConsoleApp
dotnet test                                    # run tests
dotnet format                                  # fix style before committing
```

Warnings are errors. A build with warnings is a failed build.

## Architecture

```
ConsoleApp ──► Core ◄── Infrastructure
     └────────────────────►┘
Tests ──► Core
```

- `MoneyTracker.Core` — models, enums, `ITransactionRepository`, `TransactionService`, exceptions.
  **References nothing.**
- `MoneyTracker.Infrastructure` — `JsonTransactionRepository`, `YearMonthJsonConverter`. References `Core`.
- `MoneyTracker.ConsoleApp` — `Program.cs` (composition root only) and `UI/`. References both.
- `MoneyTracker.Core.Tests` — xunit. References `Core`.

**Never add a project reference that points the other way.** If a layer seems to need one, the
design is wrong — stop and say so.

## Hard rules

- **No `Console.*` outside `src/MoneyTracker.ConsoleApp/UI/`.**
- **No `File.*` / `Directory.*` outside `src/MoneyTracker.Infrastructure/`.**
- **No business logic in `UI/`** — no sorting, filtering, or arithmetic. Ask `TransactionService`.
- **No business logic in `Program.cs`** — it wires up dependencies and runs `MainMenu`, nothing else.
- **`decimal` for money.** Never `double` or `float`.
- **`Amount` is always positive.** Sign comes from `Income.SignedAmount` / `Expense.SignedAmount`.
- **Never branch on transaction type.** No `is Expense`, no `GetType()`, no type enum. If you
  need different behaviour per type, add an abstract member to `Transaction` and override it.
- Never hand out the internal `List<Transaction>`. Return `IReadOnlyList<Transaction>`.
- Never catch `Exception` broadly. Catch the specific type.
- Don't hand-edit `data/transactions.json` — it is generated state.

## Domain

- `Transaction` (abstract): `Id` (int, auto-increment), `Title`, `Amount` (positive `decimal`),
  `Month` (`YearMonth`), abstract `SignedAmount`.
- `Income` / `Expense`: `sealed`, override `SignedAmount` as `+Amount` / `-Amount`.
- `YearMonth`: `readonly record struct` (Year + Month), `IComparable<YearMonth>`, `"2026-09"`.
- `TransactionService`: the only place business logic lives. Owns ID assignment and calls
  `Persist()` at the end of every mutating method.

`_nextId` is `Max(existing Id) + 1` on load. IDs are never reused.

## Persistence

- JSON via `System.Text.Json`, polymorphic through `[JsonPolymorphic]` / `[JsonDerivedType]`
  with discriminator `"type"` (`"income"` / `"expense"`).
- `YearMonth` converter is registered on `JsonSerializerOptions` in `Infrastructure` — **not**
  as an attribute on the type in `Core`.
- **Autosave after every add/edit/delete.** There is no manual save command.
- Write atomically: serialize to `.tmp`, then `File.Move(tmp, path, overwrite: true)`.
- Missing or empty file → empty list, not an error.
- Corrupt/unreadable file → throw `DataStoreException`. Never start with silently empty data.

## Validation

Validate twice: `ConsoleInput` re-prompts so bad input never leaves the UI, and the domain
guards its own invariants anyway.

| Rule | Throw |
|---|---|
| Title non-blank, ≤ 35 chars | `ArgumentException` |
| Amount > 0, ≤ 1 000 000 000 | `ArgumentOutOfRangeException` |
| Year 1900–2999, month 1–12 | `ArgumentOutOfRangeException` |
| Unknown ID | `TransactionNotFoundException` |

Invalid console input is a normal event — re-prompt, don't throw.

## UI

- Plain `System.Console`. **No Spectre.Console yet** — it's a planned future feature.
- Numbered menus: `Select option (0 - 6): `.
- Edit prompts show the current value; empty input keeps it.
- Deleting asks for confirmation and shows the item first.
- All colour goes through `ConsoleMessage`, which always calls `Console.ResetColor()`.
- Currency: `CultureInfo.GetCultureInfo("sv-SE")`, SEK only. Accept `,` and `.` on input.

## Style

- File-scoped namespaces, one type per file, filename matches type.
- `sealed` by default. `virtual`/`abstract` only where polymorphism is actually used.
- Nullable enabled. No `!` without a comment justifying it.
- `_camelCase` private fields, `s_camelCase` static readonly.
- XML doc comments on all public members of `Core`.
- LINQ over manual loops.

## Scope

Build what `docs/PROJECT_CONTEXT.md` describes. Don't add async, DI containers, logging
frameworks, databases, or NuGet packages beyond xunit — this is a school project graded on
clean OOP, and unasked-for machinery counts against it.

These are planned but **not yet in scope** — do not build them until asked:
unit test bodies, Spectre.Console UI overhaul, per-user login.
