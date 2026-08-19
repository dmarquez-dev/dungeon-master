# Settle the data-access lifetime and the persistence contract

Give `AppDbContext` a lifetime the host actually provides, name the unit of work that follows from it,
and make the EF model fit the provider it runs on.

## Context

Session 3 of seven audit fix sessions, carrying sixteen entries and rated the highest risk of the
seven. `docs/audit/00-master-index.md` calls it one blast radius: `Persistence/`, the two
repositories, and the two composition roots.

The defect at the centre is `MASTER-023`. `Infrastructure/DependencyInjection.cs` registers
`AddDbContext<AppDbContext>` plus two `AddScoped` repositories; `MauiProgram.cs` registers every view
model `AddTransient` and never opens a scope, and `MigrateDatabase` holds the only `CreateScope` in
the solution. Scoped services resolved outside a scope come from the root, so one `AppDbContext`
lives for the whole process: the change tracker grows without bound, and `DetectChanges` — which
compares every tracked entity at every `SaveChanges` — gets slower for the life of the session.

That is the gate. It decides what `MASTER-015` and `MASTER-092` mean, so it is settled first and
everything else follows behind it.

Session 2 merged after this audit was written, so four things in the audit's record are stale and the
contracts are written against the code instead: `Application/DependencyInjection.cs` is now an
assembly scan, `UpdateAsync` calls `Update` rather than `Attach`, `DeleteAsync` returns `Task<bool>`,
and the no-regression floor is **434 tests**, not 292 — `dotnet test src/DungeonMaster.sln --nologo`
on `9903f405`.

## Definition of done

**The lifetime**

- [ ] `Infrastructure/DependencyInjection.cs` calls `AddDbContextFactory<AppDbContext>`, and
      `grep -c 'AddScoped' src/DungeonMaster.Infrastructure/DependencyInjection.cs` returns `0`
      (it returns `2` on the base commit, lines 33–34).
- [ ] `grep -rn 'CreateScope\|CreateAsyncScope' --include=*.cs src/DungeonMaster.Infrastructure
      src/DungeonMaster.App src/DungeonMaster.Application` returns nothing. It returns exactly one hit
      on the base commit, `DependencyInjection.cs:47`. The 10 hits inside `src/DungeonMaster.Tests/`
      are the suite building its own containers and all survive.
- [ ] `grep -rn 'GetRequiredService<AppDbContext>\|GetService<AppDbContext>' --include=*.cs src/`
      returns nothing (one hit on the base commit, `DependencyInjection.cs:48`).
- [ ] A test proves the lifetime rather than asserting it: two units of work produce
      non-reference-equal aggregates for the same id, and within one unit of work a loaded aggregate's
      `EntityState` is `Unchanged`, not `Detached`.

**The persistence contract**

- [ ] The two ports live in `src/DungeonMaster.Application/Common/Interfaces/` alongside
      `IUnitOfWork` and `IUnitOfWorkFactory` — four files, each declaring
      `namespace DungeonMaster.Application.Common.Interfaces;`.
- [ ] `grep -rl '^using DungeonMaster.Application.Features.Dungeons;' --include=*.cs src/ | wc -l`
      returns `39` on the base commit and `0` afterwards.
- [ ] `grep -c 'SaveChangesAsync' .../Repositories/DungeonWriteRepository.cs` returns `0` (it returns
      `3` on the base commit, lines 32, 45, 63). `SaveChangesAsync` survives in exactly one production
      file, inside `UnitOfWork.CommitAsync`.
- [ ] A failing mutation commits nothing, proved on **both** frozen `MutateAsync` overloads.
- [ ] `docs/persistence-contract.md` exists with the five sections the contract enumerates, and both
      timestamp properties in `Entity.cs` carry a `<remarks>` block naming it by path.

**Provider fit**

- [ ] `grep -rn 'DateTimeOffset' --include=*.cs src/ | grep -v '/Migrations/'` returns nothing
      (10 lines across 5 files on the base commit).
- [ ] `AreaConfiguration` supplies a `ValueComparer<IReadOnlyList<PathSegment>>`, and a test that
      loads an unmodified area and commits asserts `CommitAsync` returns `0` — the same test observed
      returning non-zero on the base commit.
- [ ] `grep -n 'HasDefaultValue' .../PlacedObjectConfiguration.cs` returns nothing, and a persisted
      `PlacedObject` round-trips a scale other than `1.0f`.
- [ ] `AppDbContext` declares one `DbSet` (three on the base commit), and the whole Infrastructure
      suite still passes on `ApplyConfigurationsFromAssembly` alone.
- [ ] `AppDbContextFactory` resolves the same path `MauiProgram` builds, and its `args` parameter is
      used.
- [ ] Exactly **one** new migration covers both the timestamp rewrite and the `Scale` default drop,
      and `dotnet ef migrations has-pending-model-changes` reports none pending.
- [ ] `AsSplitQuery` appears once in `DungeonWriteRepository.LoadAsync` and once in
      `DungeonReadRepository.GetDetailAsync` — and **not** in `GetSummariesAsync`.

**Startup and the detail page**

- [ ] `MigrateDatabaseAsync(IServiceProvider, CancellationToken)` replaces `MigrateDatabase`, calls
      `MigrateAsync(ct)`, and `grep -rn 'Database.Migrate()' --include=*.cs src/` returns nothing.
- [ ] Directory creation and migration sit inside one `try` in `CreateMauiApp`, with a filtered
      `catch` (no bare `catch`, no unfiltered `catch (Exception)`) that logs which step failed, a
      finite timeout, and a shell that still starts.
- [ ] `grep -n '_ = LoadDungeonCommand' .../DungeonDetailViewModel.cs` returns nothing, and an absent
      or unparsable `id` sets `ErrorMessage` instead of leaving `_dungeonId` at `Guid.Empty`.

**Throughout**

- [ ] `dotnet build src/DungeonMaster.sln` — 0 warnings, 0 errors under `TreatWarningsAsErrors`, both
      App target frameworks.
- [ ] `dotnet test src/DungeonMaster.sln --nologo` — 0 failures, total at or above **434**.

## Scope

The sixteen entries, by task:

| Task | Domain | Entries |
|---|---|---|
| T1 | backend | MASTER-048, 023, 092, 033, 004, 049, 011, 012, 029 (CLR types), 103 (the pointer) |
| T2 | database | MASTER-015, 075, 032, 050, 093, 103 (the document), 029 (the migration) |
| T3 | frontend | MASTER-017 |

T1 and T3 are independent and run concurrently. T2 depends on T1.

### Not in scope

- **The cold-start half of `MASTER-012`.** Moving migration off the construction path needs a loading
  state and a user-visible error surface — view work this session does not do. The migration stays
  awaited during `CreateMauiApp` via `.GetAwaiter().GetResult()`, which cannot deadlock because it
  runs before MAUI installs a `SynchronizationContext`. → backlog.
- **Two halves of `MASTER-093`.** `dotnet ef migrations has-pending-model-changes` in CI has nothing
  to attach to — `ls -d .github/workflows` fails — and no task may create a pipeline to host it. A
  startup refusal on an unrecognised `__EFMigrationsHistory` is the same error surface `MASTER-012`
  defers. → backlog.
- Reindenting `PlacedObjectConfiguration.cs` (INFRA-006, not in this session). New and edited lines
  are tabs; surrounding space-indented lines are left alone.
- `NotNullConverter` (MASTER-007, Session 5) and the App-suite reference problem (XCUT-002, Session 6).
- The five parked design questions — MASTER-044, 073, 074, 081, 107 — unblocked by Session 2 but not
  this session's work.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| What lifetime does `AppDbContext` get? | `IDbContextFactory<AppDbContext>`, one context per unit of work — not per call | 5 (E1) | `state.json` E1; [EF `AddDbContextFactory` remarks](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.entityframeworkservicecollectionextensions.adddbcontextfactory?view=efcore-10.0); [snapshot change tracking](https://learn.microsoft.com/ef/core/change-tracking/change-detection#snapshot-change-tracking) |
| Where is the transaction boundary? | Explicit `IUnitOfWork.CommitAsync` — INFRA-014 option 1 | 5 (E2) | `state.json` E2; `DungeonWriteRepository.cs:32,45,63` |
| Converter or Domain type change for timestamps? | Domain changes to `DateTime` (UTC) | 5 (E3) | `state.json` E3; INFRA-003; XCUT-007 |
| What *is* the unit of work? | One dispatched command. Created when the handler begins, disposed when it returns | 4 | `contracts/persistence-ports-and-unit-of-work.md` §1; 12 of 14 handlers are a single `MutateAsync` expression after Session 2, so the bracket is written in one place |
| Pooled or unpooled factory? | `AddDbContextFactory`, not `AddPooledDbContextFactory` | 4 | Contract §5. MS's published benefit is a server-throughput number; this is single-user desktop on local SQLite. Also keeps "no custom mutable state" from becoming an invisible tripwire. One-line reversal |
| `AsSplitQuery` per query or global? | Per query | 4 | INFRA-011 names the global option as the alternative; MASTER-004 and MASTER-049 need separate greppable evidence, and a global switch would also hit the summary query |
| Do the write-port methods keep their names? | No: `Add`, `Update`, `RemoveAsync` | 4 | Contract §3.1. The save moved out, so nothing async is left on two of them; `DeleteAsync` no longer deletes — it marks. Names now match `DbSet<T>` |
| Where does MASTER-103's contract live? | `docs/persistence-contract.md`, with `<remarks>` pointers from `Entity.cs` | 1–2 | XCUT-007 asks for it "next to the entities rather than in the configurations"; `Entity.cs` is where the type decision is made; `docs/` is where this repo keeps prose |
| Does `AddDbContextFactory` leave a scoped registration? | Yes — it also registers `AppDbContext` as scoped, and we don't control that descriptor | 3 | EF Core 10 API remarks, quoted verbatim in contract §5.1. Not confirmed by a run in this repo; the criteria built on it are checkable regardless |
| How are timestamps stored on SQLite today, and after? | **Deliberately not asserted.** T2 establishes it by observation before writing the rewrite SQL | 4 | Contract §5.1 gives no format string on purpose. The highest-risk unknown in the session |
| How many entity types declare the timestamps? | One. `Entity.cs:16,21`, inherited by all four | 1 | `grep -rn ': Entity$' --include=*.cs src/DungeonMaster.Domain` → `Area.cs:8`, `Dungeon.cs:6`, `Floor.cs:8`, `PlacedObject.cs:8`. The audit and E3 both say "on all four entity types"; the Domain edit is three lines in one file |
| Who owns the migration? | T2, and it is the session's only one. T1 changes CLR types and adds no migration | 4 | Contract §9 and §5; between the two tasks the snapshot is knowingly stale, and reviewers are told not to raise it against T1 |
| What is the no-regression floor? | 434 tests | 1 | `dotnet test src/DungeonMaster.sln --nologo` on `9903f405`: `Failed: 0, Passed: 434` |

## Contracts

- `contracts/coding-standards.md` — tabs, CRLF, XML documentation, one fixture pattern
- `contracts/persistence-ports-and-unit-of-work.md` — the frozen port surface, the unit of work, the
  composition root, what handlers and tests look like afterwards, and the T1→T2 migration hand-off
- `contracts/persistence-contract-and-timestamps.md` — the `DateTimeOffset` inventory, the migration,
  and the five sections `docs/persistence-contract.md` must carry

## Open questions

| Question | Who resolves it | When |
|---|---|---|
| The stored TEXT format of `DateTimeOffset` vs `DateTime` on SQLite, and whether the `Down` rewrite is lossless | T2's implementer, by running the experiment — not by assuming a format string | Before writing the migration's `Sql(...)` |
| Whether the cold-start and startup-refusal halves are carried or dropped | The user | Gate 3 |
