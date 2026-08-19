# Contract: the SQLite persistence contract, and the timestamp type change

Frozen at Gate 2. This is the boundary between **T1**, which changes the CLR types, and **T2**, which
writes the migration that rewrites the stored values and the document that records why. Neither task
can verify its own half alone, which is exactly why this is a contract and not two sets of notes.

Roots, in full:

- **Worktree root** —
  `C:\Users\dmarq\Development\Repos\dungeon-master\.claude-worktrees\2026-08-18-settle-the-data-access-lifetime\`
- **Session directory** —
  `C:\Users\dmarq\Development\Repos\dungeon-master\.dir-sdlc\sessions\2026-08-18-settle-the-data-access-lifetime\`

## 1. The decision, and what it is not

E3 in
`C:\Users\dmarq\Development\Repos\dungeon-master\.dir-sdlc\sessions\2026-08-18-settle-the-data-access-lifetime\state.json`:
**the Domain changes to `DateTime` (UTC).** Not an Infrastructure `HasConversion<DateTime>()`, which
was INFRA-003's first suggested direction and was rejected; not a deferral with the contract written
and the conversion postponed, which was XCUT-007's cheapest option and was also rejected.

The reason `MASTER-029` was rated `L` / `high` is XCUT-007's: "the fix is not in the layer that found
it." This contract is where the two layers meet.

## 2. Correction to the audit record — read this before scoping

The audit and the escalation both describe the change as "`CreatedAt` and `LastModified` on all four
entity types". **In the code they are declared exactly once**, on the shared base class
`src/DungeonMaster.Domain/Entities/Entity.cs`, lines 16 and 21, with `Touch()` at line 27.
`Dungeon`, `Floor`, `Area` and `PlacedObject` all derive from it —
`grep -rn ": Entity$" --include=*.cs src/DungeonMaster.Domain` returns those four and nothing else.

The Domain blast radius of `MASTER-029` is therefore **three lines in one file**, not eight properties
across four files. The audit is describing the model, not the source. Scope accordingly, and do not go
looking for per-entity declarations that do not exist.

## 3. Every site that changes, measured

```
grep -rn "DateTimeOffset" --include=*.cs src/ | grep -v "/Migrations/"
```

returns **10 lines across 5 files** on the base commit `9903f405dc919f782ba9048cae9623bcf1d79297`,
run in the worktree. That is the complete list, and after T1 the same command must return **nothing**.

| File | Lines | Owner |
|---|---|---|
| `src/DungeonMaster.Domain/Entities/Entity.cs` | 16, 21, 27 | T1 |
| `src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeons/DungeonSummaryDto.cs` | 29, 30 | T1 |
| `src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/DungeonDetailDto.cs` | 36, 37 | T1 |
| `src/DungeonMaster.Tests/Application/Features/Dungeons/Queries/GetDungeonsQueryHandlerTests.cs` | 34, 35 | T1 |
| `src/DungeonMaster.Tests/Application/Features/Dungeons/Queries/GetDungeonDetailQueryHandlerTests.cs` | 24 | T1 |

The same grep **without** the `-v` filter returns 32 further lines, all under
`src/DungeonMaster.Infrastructure/Migrations/`. Those are EF-generated and are **T2's**, not T1's;
they change when T2 regenerates the snapshot, not by hand-editing.

### 3.1 The DTOs must change with the Domain, in one task

`DungeonReadRepository.GetSummariesAsync` and `GetDetailAsync` project `dungeon.CreatedAt` straight
into the DTO constructors inside an EF expression tree
(`src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonReadRepository.cs:25-26` and
`:44-45`). If the entity property became `DateTime` while the DTO parameter stayed `DateTimeOffset`,
the projection would need a conversion inside the expression tree. **Do not leave that mismatch in
place even transiently.** The entity, both DTOs and the three test literals change together, in T1,
in one commit.

There is no implicit conversion from `DateTimeOffset` to `DateTime`, so
`DateTimeOffset.UtcNow` in the three test literals becomes `DateTime.UtcNow`.

### 3.2 The four EF configurations need no edit for this

`AreaConfiguration.cs:51-57`, `DungeonConfiguration.cs:43-49`, `FloorConfiguration.cs:50-56` and
`PlacedObjectConfiguration.cs:38-42` configure the timestamps as `builder.Property(x => x.CreatedAt).IsRequired()`
with no type or converter mentioned. `IsRequired()` is type-agnostic. **T2 does not edit those eight
`Property` calls**, and a diff that does is a review finding — it would be churn dressed as a fix.

INFRA-003's "add `.HasConversion<DateTime>()` on the eight timestamp properties" describes the option
E3 rejected. Do not implement it.

## 4. `Entity.cs` after the change

```csharp
public Guid Id { get; private set; } = Guid.NewGuid();

public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

public DateTime LastModified { get; private set; } = DateTime.UtcNow;

protected void Touch() =>
	LastModified = DateTime.UtcNow;
```

`DateTime.UtcNow`, never `DateTime.Now`. The existing XML summaries already say "UTC timestamp"; they
stay true and are not reworded.

**T1 additionally adds a `<remarks>` block** to both timestamp properties pointing at the document T2
writes, by its path: `docs/persistence-contract.md`. This is XCUT-007's actual request — that the
persistence consequence be recorded "next to the entities rather than in the configurations" — and
`Entity.cs` is the one place in the Domain where the decision is made. Two `<remarks>` blocks, one
sentence each, naming the file. That is T1's whole share of `MASTER-103`.

The existing Domain test `src/DungeonMaster.Tests/Domain/Entities/Floor/FloorMethodsTests.cs:16,27`
asserts `second.LastModified.Should().BeAfter(before)`. FluentAssertions supports `BeAfter` on
`DateTime` as well as `DateTimeOffset`, so that test is expected to compile and pass unchanged; T1
confirms it did rather than assuming it.

## 5. The migration — T2's, and there is exactly one

T2 authors **one** migration under `src/DungeonMaster.Infrastructure/Migrations/` covering every model
change this session makes:

1. The eight timestamp columns' **stored value** rewrite (`MASTER-029`).
2. Dropping the `Scale` store default (`MASTER-075`, specified in T2's acceptance criteria).

Any column-type change the scaffolder emits for the timestamps is incidental — both `DateTimeOffset`
and `DateTime` land in SQLite as `TEXT`. **The value rewrite is the substance**, and the scaffolder
will not write it: `dotnet ef migrations add` produces schema operations, not data ones, so the value
rewrite is hand-written `migrationBuilder.Sql(...)` inside the generated `Up`.

### 5.1 What the value rewrite must achieve, stated as an outcome not a format string

The EF Core SQLite provider stores the two CLR types as TEXT in different shapes: a `DateTimeOffset`
carries a trailing UTC offset that a `DateTime` does not. A row written before this migration
therefore has an offset suffix that the `DateTime` reader must not be handed.

**The criterion is behavioural, and T2 must establish the actual stored formats by observation rather
than from this contract.** Concretely: create a database with the pre-migration model, write a row,
read the raw TEXT with `SELECT CreatedAt FROM Dungeons`, quote it; apply the migration; read it again,
quote it; then read the row back through `AppDbContext` and assert the `DateTime` round-trips to the
same instant with `Kind` handled consistently.

I have **not** run that experiment. Do not treat any format string as given, and do not write an
acceptance criterion that names one.

All eight columns are affected: `CreatedAt` and `LastModified` on each of `Dungeons`, `Floors`,
`Areas` and `PlacedObjects`.

### 5.2 `Down`

`Down` reverses the value rewrite. If it cannot faithfully do so — the offset information is not
recoverable from a bare `DateTime`, and every value was written as UTC — `Down` reconstructs the `+00:00`
offset and the migration's XML documentation says that it does, and why that is lossless **for this
data set specifically** (every value originated from `DateTimeOffset.UtcNow`). State the reasoning in
the migration file, not only in the task report.

## 6. `docs/persistence-contract.md` — `MASTER-103`'s deliverable

T2 creates `docs/persistence-contract.md` at the repository root's `docs/` directory. It is the
"documented persistence contract" XCUT-007 says is missing. It must contain, at minimum:

1. **The target.** SQLite via `Microsoft.EntityFrameworkCore.Sqlite`, one local file per user under
   `{LocalApplicationData}/DungeonMaster/dungeon-master.db`, single-writer, no server.
2. **Which CLR types are cheap and which are not**, as a table, each row citing why. It must cover at
   least the four cases the audit found: `DateTime` (UTC) yes / `DateTimeOffset` no —
   [SQLite limitations](https://learn.microsoft.com/ef/core/providers/sqlite/limitations) lists it as
   not natively supported and says operations other than equality "will require evaluation on the
   client"; `float`/`REAL`; `string`/`TEXT` with the note that **`HasMaxLength` is model metadata only
   and SQLite enforces none of it** (INFRA-002, tracked as `MASTER-152`, **not in this session** —
   recorded in the document as context, not fixed); and `Guid` keys as `ValueGeneratedNever`.
3. **The store-default hazard**, generalised from INFRA-008: a store default makes the CLR default
   value unsendable, so a domain range that ever admits the CLR default silently loses data. Name the
   `Scale` case as the worked example and say what T2 did about it.
4. **The rule the next type choice should follow.** One paragraph: a Domain type choice is a
   persistence choice on this project because there is no mapping layer between them, so a new
   timestamp, decimal or collection property gets checked against this table before it is added.
5. **A pointer back**, so the document is discoverable from where the decision is made: it names
   `src/DungeonMaster.Domain/Entities/Entity.cs` as the file whose `<remarks>` point here.

This document is prose, not code. It is not required to be tab-indented and it is not required to be
CRLF — `.editorconfig`'s `[*]` section applies, so match whatever the existing files under `docs/`
already do and say in the report which that was.

## 7. `MASTER-093` — the migration history note

INFRA-020's suggested direction has three parts. Two are in scope and one is not.

**In scope, T2:**

- Put the pre-squash decision where a reader will find it. Commit `7a4bb2d`'s message says "Existing
  local databases are not compatible and must be deleted; nothing shipped on the old history." That
  sentence, and the fact that the baseline identifier `20260805014736` never existed before that
  commit, go into a note a reader of the repository can find. Put it in
  `docs/persistence-contract.md` as a "Migration history" section rather than creating a second
  document — one file to find, and it is already the file about how this database works.
- Update the stale comment at
  `src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs:36`,
  which currently reads `// DB default backs existing rows; domain guard prevents 0.0f from ever being
  written`. It describes a world where `Scale` arrived as an added column, which no migration in the
  tree does any more. `MASTER-075` removes the default the comment is about, so the comment is deleted
  outright rather than reworded.

**Not in scope, and not to be worked around:**

- "Add `dotnet ef migrations has-pending-model-changes` to CI." **There is no
  `.github/workflows` directory in this repository** — `ls -d .github/workflows` fails. No acceptance
  criterion in this session may depend on CI existing, and no task may create a CI pipeline to give
  this suggestion somewhere to land. It is carried to the backlog.
- "Have startup detect an unrecognised `__EFMigrationsHistory` and refuse with a clear message." This
  is a new startup failure mode with a user-visible message, which is the same view work
  `contracts/persistence-ports-and-unit-of-work.md` §6 defers for UI-002. Carried to the backlog with
  it. `MASTER-093` is closed as *the decision is written down where a reader will find it and the
  stale comment is gone*, and the T2 report says so explicitly rather than letting a reviewer find the
  gap.
