# Infrastructure -- code quality audit

## Scope

| Directory or glob | Files | Lines |
|---|---|---|
| `src/DungeonMaster.Infrastructure/DependencyInjection.cs` | 1 | 52 |
| `src/DungeonMaster.Infrastructure/Persistence/*.cs` | 2 | 69 |
| `src/DungeonMaster.Infrastructure/Persistence/Configurations/*.cs` | 4 | 281 |
| `src/DungeonMaster.Infrastructure/Persistence/Repositories/*.cs` | 2 | 133 |
| **Total audited** | **9** | **535** |

**Files read: 9. Files cited: 9.** Every file in the audited scope table is cited in the `locations`
of at least one record; no file needed the "read and found sound" disposition. One further path,
the generated `Migrations/20260805014736_InitialCreate.cs`, appears in INFRA-020's `locations` as
migration-history evidence; it is excluded from both counts, as explained below.

Not audited, and why:

- `src/DungeonMaster.Infrastructure/Migrations/20260805014736_InitialCreate.cs` and
  `20260805014736_InitialCreate.Designer.cs` -- **excluded as EF-generated code.** The scaffolder
  chooses the brace style, indentation, `#nullable disable` pragma and `x =>` lambda names; none of
  that is an authorial choice, so measuring it against `backend/dotnet-format.md` would produce
  false defects. Not counted in the files-read total.
- `src/DungeonMaster.Infrastructure/Migrations/20260805184106_AddFloorDescription.cs` and
  `20260805184106_AddFloorDescription.Designer.cs` -- excluded, same reason. Not counted.
- `src/DungeonMaster.Infrastructure/Migrations/AppDbContextModelSnapshot.cs` -- excluded, same
  reason. Not counted.
- `src/DungeonMaster.Infrastructure/DungeonMaster.Infrastructure.csproj` -- excluded because
  contract section 1 assigns all five `.csproj` files to layer `XCUT` (T6). The
  `SQLitePCLRaw.lib.e_sqlite3 2.1.12` transitive pin is T6's record, not duplicated here.

The five generated migration files were nonetheless **read for what the migration history reveals**,
which is an authorial artefact rather than generated style: the sequence of migrations, whether any
`Down` is destructive, and whether `AppDbContextModelSnapshot.cs` still agrees with the four
hand-written configurations. That reading produced INFRA-020 and confirmed no snapshot/configuration
drift. Where a generated file is cited as evidence in a record's `locations`, it is cited as
history, not as craft, and it is still not counted as a read file.

The nine hand-written files are audited as hand-written code at full depth, style included.
`DependencyInjection.cs` is judged as hand-written composition-root code -- service lifetimes,
registration correctness and the startup migration call are all in scope for it.

## Method

- Base commit: `45495c1bd39a52dbc942eceab897b3c1ac553ee9` (branch `feat/phosphor-pack-integration`,
  worktree `dungeon-master-worktrees/2026-08-17-code-quality-audit`).
- `dm-conventions` documents read:
  - `backend/dotnet-format.md` -- **794 lines, read in full** before any style, naming, formatting
    or XML-documentation record was written.
  - `backend/dotnet-guidelines.md` -- 532 lines; sections read and treated as **applicable**:
    `1. Async / Await` (1.1-1.4) and `2. Data Access & EF Core` (2.1-2.6), plus
    `4. Memory & Allocations` and `8. Performance Patterns`. Sections treated as **not applicable**,
    because this solution has no web host and no `HttpContext`: `3. HttpContext Safety`
    (3.1-3.4), `5. HTTP Clients`, and `7. Request & Response Handling`. Section 1.3
    ("Make Controller Actions Async") is likewise inapplicable -- there are no controllers.
    `6. Exception Handling` is applicable and was read; no record in this layer turns on it.
  - `backend/backend-conventions.md` -- 251 lines; sections `6. Async / Cancellation` and
    `7. Dependency Injection` bear directly on this layer.
- Microsoft Learn pages consulted (each fetched, not recalled):
  - https://learn.microsoft.com/en-us/ef/core/modeling/value-comparers
  - https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries
  - https://learn.microsoft.com/en-us/ef/core/querying/tracking
  - https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/
  - https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying
  - https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations
  - https://learn.microsoft.com/en-us/ef/core/cli/dbcontext-creation
  - https://learn.microsoft.com/en-us/ef/core/saving/disconnected-entities
  - https://learn.microsoft.com/en-us/ef/core/modeling/generated-properties
  - https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbset-1.addasync
- Tooling run, all read-only: `git log`, `git show --stat`, `git status --porcelain`,
  `wc -l`, ripgrep over `src/`. No EF tooling was executed, no database was created or opened, and
  no migration was applied. Cross-layer consumption facts were established by reading
  `src/DungeonMaster.App/MauiProgram.cs` and
  `src/DungeonMaster.Application/DependencyInjection.cs`; neither file was
  modified, and neither is in this task's scope.

## Summary

| Severity | Count |
|---|---|
| S1 | 0 |
| S2 | 6 |
| S3 | 7 |
| S4 | 3 |
| S5 | 4 |
| **Total** | 20 |

## Records

### INFRA-001 -- AppDbContext exposes Floors and Areas DbSets that nothing in the solution reads

```yaml
id: INFRA-001
layer: INFRA
severity: S3
category: dead-code
authority: practice
citation: Evans, Domain-Driven Design, ch. 6 "Aggregates"
locations:
  - src/DungeonMaster.Infrastructure/Persistence/AppDbContext.cs:23
  - src/DungeonMaster.Infrastructure/Persistence/AppDbContext.cs:28
effort: XS
risk: low
independent: true
```

**What.** `AppDbContext` declares `Dungeons`, `Floors` and `Areas`. A search of
`src/DungeonMaster.Infrastructure` and `src/DungeonMaster.Tests` finds reads of `dbContext.Dungeons`
only; `Floors` and `Areas` have no consumer anywhere in the solution. `PlacedObject`, by contrast,
is mapped but deliberately has no `DbSet`.

**Why it matters.** `Dungeon` is the aggregate root and the two repositories reach `Floor`, `Area`
and `PlacedObject` only by navigation, which is the intended aggregate boundary. The two extra
`DbSet` properties are a public, documented invitation to query a child entity directly and bypass
that boundary -- the inconsistency with `PlacedObject` shows the boundary was understood and then
only partly enforced.

**Suggested direction.** Remove the `Floors` and `Areas` properties, or, if root-level access is
wanted for diagnostics, make them non-public and say so in the XML documentation. Nothing needs to
change in `OnModelCreating`: `ApplyConfigurationsFromAssembly` discovers the entity types
independently of the `DbSet` declarations.

### INFRA-002 -- HasMaxLength is configured on eleven string columns but SQLite enforces none of them

```yaml
id: INFRA-002
layer: INFRA
severity: S5
category: correctness
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/DungeonConfiguration.cs:30-41
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/FloorConfiguration.cs:38-48
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/AreaConfiguration.cs:38-49
effort: S
risk: low
independent: true
```

**What.** All four configurations set `HasMaxLength` on their string properties (100 for names, 2000
or 4000 for descriptions and notes). I believe none of these bounds reaches the database: the
generated `InitialCreate` emits `table.Column<string>(type: "TEXT", maxLength: 100, ...)`, and TEXT
in SQLite carries no length limit or CHECK constraint, so the annotation appears to survive only as
model metadata.

**Why it matters.** My belief is that the length limits are enforced today by exactly one thing --
the FluentValidation validators in the Application layer -- and that anything writing through the
repositories without passing a validator (a future import path, a repair routine, a test seeding
helper) would persist over-length text silently. A reader of the configuration files could
reasonably assume the database is the backstop, and I do not think it is.

**Suggested direction.** Either accept the situation and note in each configuration that the length
is model metadata only, or add explicit `ToTable(t => t.HasCheckConstraint(...))` length checks so
the constraint exists where the data does. The former is probably right for a single-user local
database; the point is to make the choice visible.

### INFRA-003 -- Every timestamp column is DateTimeOffset, a type the SQLite provider does not natively support

```yaml
id: INFRA-003
layer: INFRA
severity: S2
category: performance
authority: microsoft
citation: https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations
locations:
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/DungeonConfiguration.cs:43-49
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/FloorConfiguration.cs:50-56
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/AreaConfiguration.cs:51-57
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs:38-42
effort: L
risk: high
independent: true
```

**What.** `CreatedAt` and `LastModified` are `DateTimeOffset` on all four entity types and are
mapped without a value converter, producing TEXT columns. The cited page lists `DateTimeOffset`
among the types SQLite "doesn't natively support", states that operations other than equality
"will require evaluation on the client", and recommends `DateTime` converted to UTC instead.

**Why it matters.** No query orders or filters on these columns today, so nothing misbehaves yet.
The moment one does -- a "recently modified" sort on the dungeon list is the obvious next feature --
the ordering is evaluated client-side after the whole table is materialised, which is exactly the
failure mode `backend/dotnet-guidelines.md` section 2.5 forbids, and it will not be visible in the
LINQ. The four configuration files are the only place this is decidable.

**Suggested direction.** Either add `.HasConversion<DateTime>()` (storing UTC) on the eight
timestamp properties, or add a converter to a sortable canonical string, and add a migration for the
column-value rewrite. The property types themselves live in the Domain layer, so coordinate with
whoever owns `Domain/Entities/*`; this record is about the mapping decision, which is Infrastructure's.

### INFRA-004 -- Outline JSON conversion has no ValueComparer, so every tracked Area is reported as modified

```yaml
id: INFRA-004
layer: INFRA
severity: S2
category: correctness
authority: microsoft
citation: https://learn.microsoft.com/en-us/ef/core/modeling/value-comparers
locations:
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/AreaConfiguration.cs:59-74
effort: S
risk: medium
independent: true
```

**What.** `Area.Outline` is mapped through a `HasConversion` pair that serialises
`IReadOnlyList<PathSegment>` to JSON, with no `ValueComparer` supplied. The cited page states that
for a converted reference type EF Core falls back to the type's default equality and that "no
copying occurs, and the same instance is used as the snapshot". The property access mode is
`PreferFieldDuringConstruction`, so outside materialisation EF reads through the property -- and
`Area.Outline` returns `_outline.AsReadOnly()`, a freshly allocated wrapper on every call.

**Why it matters.** Snapshot and current value are therefore never the same reference, so
`DetectChanges` marks `Outline` modified on every `SaveChanges` for every tracked `Area`, re-running
the serializer and rewriting the column even when nothing was edited. With the long-lived context
described in INFRA-018 this compounds across a session. The inverse hazard is latent: if `Outline`
were ever changed to return `_outline` directly, the in-place `Clear`/`AddRange` in
`Area.SetOutline` would become invisible to change tracking and outline edits would be silently
lost.

**Suggested direction.** Supply the third `HasConversion` argument -- a
`ValueComparer<IReadOnlyList<PathSegment>>` with `SequenceEqual` for equality, an aggregated hash,
and `ToList()` for the snapshot -- exactly as the cited page's `List<int>` example does. That makes
change detection value-based and independent of which instance the property hands back.

### INFRA-005 -- The (DungeonId, Number) index is non-unique, so floor numbering has no database guarantee

```yaml
id: INFRA-005
layer: INFRA
severity: S5
category: correctness
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/FloorConfiguration.cs:58-59
effort: S
risk: high
independent: true
```

**What.** `HasIndex(floor => new { floor.DungeonId, floor.Number })` is declared without
`IsUnique()`. The migration history shows this was deliberate: a migration named
`20260511120000_RemoveFloorNumberUniqueIndex` existed before the baseline squash and dropped the
unique variant.

**Why it matters.** I believe the reason was the floor-reorder command, which must be able to pass
through a state where two floors briefly share a number inside one `SaveChanges` -- a unique index
would reject that. The consequence, as I read it, is that "floor numbers are unique within a
dungeon" is now an invariant held only by `Dungeon`'s domain methods, with nothing behind them; a
partially-applied reorder or a future bulk path could leave duplicates that no layer would notice.

**Suggested direction.** If the invariant matters, consider a deferrable check applied after the
reorder completes, or a reorder implementation that renumbers into a disjoint range first so a
unique index becomes viable. If it does not matter, a one-line comment on the `HasIndex` call
recording why unique was rejected would stop the next reader from "fixing" it.

### INFRA-006 -- PlacedObjectConfiguration is formatted with spaces and inline lambdas unlike its three siblings

```yaml
id: INFRA-006
layer: INFRA
severity: S4
category: style
authority: conventions
citation: backend/dotnet-format.md#7. Lambda Expressions
locations:
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs:8-55
effort: XS
risk: low
independent: true
```

**What.** This file is indented with four spaces; `AreaConfiguration.cs`,
`DungeonConfiguration.cs` and `FloorConfiguration.cs` are indented with tabs. It also writes
`builder.HasKey(obj => obj.Id)` and `builder.Property(obj => obj.Kind)` inline, where the other
three break after `=>` onto an indented continuation line, and it documents `Configure` with a bare
`/// <inheritdoc/>` (line 13) where the siblings carry a full `<summary>` and `<param>` block.

**Why it matters.** `backend/dotnet-format.md` section 7 states that "everything after `=>` goes on
a new line, indented one level", and section 9 requires XML documentation on public members with
descriptions on their own indented lines; the other three configurations follow both. The
divergence makes the four files read as though written to two different rulebooks, and a diff
touching this file will show whitespace churn the first time anyone reformats it.

**Suggested direction.** Reindent to tabs, break the single-expression lambdas onto continuation
lines to match the sibling files, and replace the `<inheritdoc/>` with the same `<summary>`/`<param>`
pair the other three use. This is the file to fix, not the other three.

### INFRA-007 -- Single-letter lambda variables in the owned-type configuration block

```yaml
id: INFRA-007
layer: INFRA
severity: S4
category: naming
authority: conventions
citation: backend/dotnet-format.md#Lambda Variable Names
locations:
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs:46
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs:51
effort: XS
risk: low
independent: true
```

**What.** Inside the `OwnsOne` block, the two property lambdas are written `p => p.X` and
`p => p.Y`. The enclosing lambda in the same statement already uses the descriptive name `position`,
and every other lambda in the four configuration files uses a descriptive name (`dungeon`, `floor`,
`area`, `obj`).

**Why it matters.** `backend/dotnet-format.md` states the rule without exception: "Use descriptive
names in lambda expressions -- never single-letter variables." These two are the only violations in
the layer, so the cost is purely that the rule now looks optional to the next contributor.

**Suggested direction.** Rename both to `point` (or reuse `position`, which is already the outer
name and would read `position.Property(point => point.X)`).

### INFRA-008 -- Scale is configured with a store default, which makes the CLR default 0f unsendable

```yaml
id: INFRA-008
layer: INFRA
severity: S3
category: correctness
authority: microsoft
citation: https://learn.microsoft.com/en-us/ef/core/modeling/generated-properties
locations:
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs:34-36
effort: S
risk: medium
independent: true
```

**What.** `builder.Property(obj => obj.Scale).HasColumnType("REAL").HasDefaultValue(1.0f)` makes
`Scale` store-generated on add -- `AppDbContextModelSnapshot.cs` records it as
`ValueGeneratedOnAdd()`. The cited page states that to override value generation you must "set the
property to any value that is not the CLR default value for that property's type". The inline
comment at line 36 shows the author knew this and relied on `PlacedObject`'s constructor guard to
keep `0f` from ever arriving.

**Why it matters.** The guard and the mapping are two facts in two projects that must stay true
together, with no compiler or test link between them. If the domain range for `Scale` were ever
widened to include zero, or a future value object made `0f` legal, every such insert would silently
land in the database as `1.0f`. EF Core 9 added `HasSentinel` precisely so this coupling can be
stated in one place.

**Suggested direction.** Keep the store default for the benefit of pre-existing rows, and add
`.HasSentinel(0f)` explicitly -- or better, since `Scale` is always supplied by the constructor,
drop `HasDefaultValue` from the model now that the backfill it existed for has been applied and let
the value travel as ordinary data.

### INFRA-009 -- AsNoTracking on the summary query has no effect because the projection contains no entity types

```yaml
id: INFRA-009
layer: INFRA
severity: S5
category: performance
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonReadRepository.cs:17-18
effort: XS
risk: low
independent: true
```

**What.** `GetSummariesAsync` calls `AsNoTracking()` and then projects to `DungeonSummaryDto`, whose
members are all scalars (`Guid`, `string`, `int`, `DateTimeOffset`). EF Core's tracking
documentation states that when the result set contains no entity types, no tracking is done
regardless.

**Why it matters.** I believe this particular `AsNoTracking()` is therefore a no-op, while the one
in `GetDetailAsync` is not -- that projection includes `obj.Position`, an owned `CanvasPoint`, which
is an entity type in EF's model. So the two calls look identical and are not. I do not consider the
redundant call a defect; my concern is only that it reads as load-bearing when it is not, which
could mislead someone tuning the read path later.

**Suggested direction.** I would leave both in place as defensive uniformity and add a one-line
comment on the detail query noting that its `AsNoTracking` is the load-bearing one because of the
owned type. Removing the redundant call is equally defensible and equally safe.

### INFRA-010 -- The detail projection materialises two sibling collections per floor in one query

```yaml
id: INFRA-010
layer: INFRA
severity: S3
category: performance
authority: microsoft
citation: https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries
locations:
  - src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonReadRepository.cs:33-75
effort: S
risk: low
independent: true
```

**What.** `GetDetailAsync` projects `Floors`, and within each floor both `Areas` and `Objects`, in a
single LINQ tree terminated by `FirstOrDefaultAsync`. `Areas` and `Objects` are sibling collection
navigations of the same principal, `Floor`.

**Why it matters.** The cited page describes exactly this shape: sibling collection navigations at
the same level produce a cross product, so a floor with 20 areas and 40 placed objects returns 800
rows for that floor alone, and the floor's own columns are duplicated across all of them. On a
local SQLite file this is CPU and allocation rather than network, but the map canvas re-reads the
detail projection on every floor edit, which puts it on a hot path.

**Suggested direction.** Add `.AsSplitQuery()` to the detail query, or split it by hand into a floor
query plus two child queries keyed by floor id. Note the trade-off the cited page raises -- split
queries give up cross-query consistency -- which is acceptable for a single-writer local database.

### INFRA-011 -- LoadAsync includes two sibling collections, producing a cartesian product per floor

```yaml
id: INFRA-011
layer: INFRA
severity: S2
category: performance
authority: microsoft
citation: https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries
locations:
  - src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonWriteRepository.cs:16-26
effort: XS
risk: low
independent: true
```

**What.** `LoadAsync` chains `Include(Floors).ThenInclude(Areas)` and
`Include(Floors).ThenInclude(Objects)` with no `AsSplitQuery()`. `Areas` and `Objects` are both
collection navigations of `Floor`, so they are siblings.

**Why it matters.** This is the cited page's headline example. Every command handler in the
Application layer calls `LoadAsync` before mutating, so the whole dungeon aggregate is fetched as
`floors x areas x objects` rows on every single write -- adding one placed object loads the cross
product of the entire dungeon. EF Core also raises a runtime warning when it detects a query loading
multiple collections with no splitting behaviour configured; that warning is logged, not compiled,
so the clean `TreatWarningsAsErrors` build says nothing about it.

**Suggested direction.** Add `.AsSplitQuery()` to this query, or configure
`UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)` once in `AddInfrastructureServices`
and opt specific queries back out. The doubled `Include(dungeon => dungeon.Floors)` is correct and
required for the two `ThenInclude` chains -- that is not the defect here.

### INFRA-012 -- UpdateAsync attaches a detached aggregate, which marks it Unchanged and saves nothing

```yaml
id: INFRA-012
layer: INFRA
severity: S2
category: correctness
authority: microsoft
citation: https://learn.microsoft.com/en-us/ef/core/saving/disconnected-entities
locations:
  - src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonWriteRepository.cs:36-44
effort: S
risk: medium
independent: true
```

**What.** `UpdateAsync` tests `dbContext.Entry(dungeon).State is EntityState.Detached` and, when
true, calls `dbContext.Attach(dungeon)` before `SaveChangesAsync`. The cited page states that
`Attach` walks the graph marking entities `Unchanged` -- "do nothing" -- for anything with a key
value set, and all four entity types here use `ValueGeneratedNever` keys assigned in the
constructor.

**Why it matters.** A genuinely detached aggregate reaching this method is saved as a no-op: no
exception, no return value to inspect (`Task`, not `Task<bool>`), and the caller's `Result.Success`
still flows to the UI. Today every handler loads through `LoadAsync` on the same context first, so
the entity is already tracked and the branch is not taken -- which is precisely why the bug would
survive review and then fire the first time a handler is refactored to construct an aggregate
in-memory or the context lifetime changes.

**Suggested direction.** Use `dbContext.Update(dungeon)` for the detached case, which marks the
graph `Modified`, or delete the branch and document that `UpdateAsync` requires an aggregate loaded
by this repository -- enforcing it with a thrown `InvalidOperationException` on `Detached` rather
than silently succeeding.

### INFRA-013 -- AddAsync is used where the API documentation prescribes the synchronous Add

```yaml
id: INFRA-013
layer: INFRA
severity: S3
category: async
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbset-1.addasync
locations:
  - src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonWriteRepository.cs:31
effort: XS
risk: low
independent: true
```

**What.** `AddAsync` calls `dbContext.Dungeons.AddAsync(dungeon, ct)`. The cited API page's Remarks
say the method "is async only to allow special value generators, such as the one used by
`SqlServerValueGenerationStrategy.SequenceHiLo`, to access the database asynchronously. For all
other cases the non async method should be used." No property in this model uses a database-backed
generator; the four key properties are explicitly `ValueGeneratedNever`.

**Why it matters.** The awaited `ValueTask` always completes synchronously here, so this is a state
machine and an await for nothing. More usefully, it signals to a reader that adding touches the
database, which it does not -- the write happens on the next line, at `SaveChangesAsync`. This is a
different point from `backend/dotnet-guidelines.md` section 2.1: that rule is about database calls,
and `Add` is not one.

**Suggested direction.** Change to `dbContext.Dungeons.Add(dungeon);` and keep the method `async`
for the `SaveChangesAsync` that follows.

### INFRA-014 -- Each write method commits its own transaction, so the repository is also the unit of work

```yaml
id: INFRA-014
layer: INFRA
severity: S3
category: structure
authority: practice
citation: Fowler, Patterns of Enterprise Application Architecture, "Unit of Work"
locations:
  - src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonWriteRepository.cs:32
  - src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonWriteRepository.cs:43
  - src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonWriteRepository.cs:54
effort: M
risk: medium
independent: true
```

**What.** `AddAsync`, `UpdateAsync` and `DeleteAsync` each call `SaveChangesAsync` themselves, and
`IDungeonWriteRepository` exposes no commit operation. The transaction boundary is therefore fixed
at one repository call, and command handlers have no way to widen it.

**Why it matters.** `DbContext` already is a unit of work; wrapping each mutation in its own commit
discards that. A command handler that needed to touch two aggregates, or to add a floor and reorder
the remaining floors atomically, cannot express it -- it would issue two commits with a window
between them, and on a local SQLite file a crash in that window leaves inconsistent data. It also
means a handler cannot compose repository calls at all without accepting partial application.

**Suggested direction.** Either move `SaveChangesAsync` behind an explicit `IUnitOfWork.CommitAsync`
that handlers call, or document on `IDungeonWriteRepository` that one call equals one transaction
and that multi-aggregate commands are out of scope by design. The interface lives in the Application
layer, so this fix crosses the boundary and needs the interface owner.

### INFRA-015 -- No ConfigureAwait(false) anywhere in the layer, in a host that has a UI SynchronizationContext

```yaml
id: INFRA-015
layer: INFRA
severity: S5
category: async
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonReadRepository.cs:27
  - src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonWriteRepository.cs:32
effort: S
risk: low
independent: true
```

**What.** Every `await` in both repositories is written without `ConfigureAwait(false)`. Unlike the
ASP.NET Core setting `backend/dotnet-guidelines.md` was written for, the consumer here is .NET MAUI,
which installs a `SynchronizationContext` on the UI thread, and the call chain runs
view model to dispatcher to handler to repository on that thread.

**Why it matters.** I believe every intermediate continuation in these repositories is therefore
posted back to the UI thread rather than resuming on the thread-pool thread the database work
completed on. For a two-await method the cost is small, and it is not a deadlock risk here because
nothing in the chain blocks on `.Result` or `.Wait()`. My concern is proportion, not correctness:
this is library code and I would expect a library to be context-agnostic.

**Suggested direction.** I would add `.ConfigureAwait(false)` to the awaits in both repositories,
which is safe because neither touches UI state, and leave the view models unchanged so their final
continuations still land on the UI thread. Doing nothing is also defensible for an app this size --
the point is that the decision should be deliberate.

### INFRA-016 -- The design-time factory hard-codes a relative database path that differs from the runtime one

```yaml
id: INFRA-016
layer: INFRA
severity: S3
category: correctness
authority: microsoft
citation: https://learn.microsoft.com/en-us/ef/core/cli/dbcontext-creation
locations:
  - src/DungeonMaster.Infrastructure/Persistence/AppDbContextFactory.cs:21-27
effort: S
risk: low
independent: true
```

**What.** `CreateDbContext` builds options with the literal connection string
`"Data Source=dungeon-master.db"` -- a path relative to the tooling's working directory. At run time
`MauiProgram` builds `Data Source={LocalApplicationData}/DungeonMaster/dungeon-master.db`. The
`args` parameter is accepted, documented as "Not used", and discarded.

**Why it matters.** The cited page instructs: "Configure the same provider, model options, and
migrations assembly at design time that the application uses at run time. Otherwise, the tools can
scaffold migrations for a model that differs from the model used by the application," and it notes
that `CreateDbContext` receives command-line arguments precisely so this can be parameterised. The
provider matches here, so `migrations add` is safe; the hazard is `dotnet ef database update`, which
will create or migrate a stray `dungeon-master.db` in the project folder while leaving the real
application database untouched -- and the developer gets no signal, because that file is covered by
`*.db` in `.gitignore`.

**Suggested direction.** Read the path from `args` or an environment variable with the
`LocalApplicationData` location as the default, so design-time and run-time resolve to the same
file; failing that, state in the XML documentation that this factory exists for `migrations add`
only and that `database update` must be given `--connection`.

### INFRA-017 -- The file DependencyInjection.cs does not match the type it contains

```yaml
id: INFRA-017
layer: INFRA
severity: S4
category: naming
authority: conventions
citation: backend/dotnet-format.md#2. File Organization
locations:
  - src/DungeonMaster.Infrastructure/DependencyInjection.cs:12
effort: XS
risk: low
independent: true
```

**What.** `DependencyInjection.cs` declares `public static class
InfrastructureServiceCollectionExtensions`. The Application layer's composition root carries the
identical mismatch: `src/DungeonMaster.Application/DependencyInjection.cs:27` declares
`ApplicationServiceCollectionExtensions`, and no file of that name exists in the solution.

**Why it matters.** `backend/dotnet-format.md` section 2 states "File name matches the type name
exactly". Both composition-root files break it the same way, so this is a consistent layer-wide
naming deviation rather than a one-off: neither extension class can be found by typing its name into
a file search, which is the first thing anyone onboarding does when asked "where is DI wired up".
The Application-side rename belongs to T1/T6; only the Infrastructure file is in this task's scope.

**Suggested direction.** Rename the file to `InfrastructureServiceCollectionExtensions.cs`.

### INFRA-018 -- Scoped registrations resolve from the root provider, so one DbContext lives for the whole process

```yaml
id: INFRA-018
layer: INFRA
severity: S2
category: threading
authority: microsoft
citation: https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/
locations:
  - src/DungeonMaster.Infrastructure/DependencyInjection.cs:30-34
effort: M
risk: medium
independent: true
```

**What.** `AddDbContext<AppDbContext>` registers the context with the default scoped lifetime, and
both repositories are registered `AddScoped`. The consumer is .NET MAUI: `MauiProgram` registers
view models and pages as transient, which are believed to resolve from the root provider for the
app's lifetime -- MAUI's own scoping behaviour is not something the cited page covers -- and
`MigrateDatabase` is the only place in the solution that calls `CreateScope`. Command and query
handlers in the Application layer are scoped too, so the whole dispatch chain resolves from the same
provider as the context.

**Why it matters.** The cited page states that a `DbContext` instance "is designed to be used for a
single unit-of-work", that "the lifetime of a DbContext instance is usually very short", that it is
"Not thread-safe", and that the scoped default is safe in ASP.NET Core specifically because "each
request gets a separate dependency injection scope". Neither condition holds here. A scoped service
resolved outside any explicit scope comes from the root scope and is disposed only with the
provider, so a single `AppDbContext` accumulates every tracked entity for the life of the
application -- the change tracker grows without bound, identity resolution keeps returning
first-loaded instances, and any command started before a previous one is awaited hits the
"second operation started on this context" exception. INFRA-004's spurious `Outline` writes compound
against exactly this long-lived tracker.

**Suggested direction.** Register `AddDbContextFactory<AppDbContext>` and have the repositories take
`IDbContextFactory<AppDbContext>`, creating and disposing a context per operation -- the pattern the
cited page recommends for hosts that "do not create a service scope that aligns with the desired
DbContext lifetime", naming Blazor, which has the same shape as MAUI. Alternatively have the
dispatcher open an `IServiceScope` per command or query. The App-side half of this is T3/T6
territory; the registration is Infrastructure's.

### INFRA-019 -- MigrateDatabase applies migrations synchronously on the startup path and takes no cancellation token

```yaml
id: INFRA-019
layer: INFRA
severity: S2
category: async
authority: microsoft
citation: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying
locations:
  - src/DungeonMaster.Infrastructure/DependencyInjection.cs:45-51
effort: S
risk: low
independent: true
```

**What.** `MigrateDatabase` resolves the context in a scope and calls `db.Database.Migrate()`. It is
invoked from `MauiProgram.CreateMauiApp` at line 139, after `builder.Build()`, on the thread that
constructs the app. The method signature has no `CancellationToken`.

**Why it matters.** The cited page's runtime-migration section shows the recommended call as
`await db.Database.MigrateAsync()`; `backend/dotnet-guidelines.md` section 2.1 requires the async
form of every EF Core database call and `backend/backend-conventions.md` section 6 requires every
async operation to accept a cancellation signal. The concrete cost is that the app's startup thread
blocks for the full duration of schema work with no progress signal and no way to abandon it. The
same page also warns that on SQLite the EF 9+ migration lock uses an `__EFMigrationsLock` table
whose row is not cleaned up if the process is killed mid-migration, after which "each attempt will
wait indefinitely for the lock to be released" -- a hang on startup with no diagnostic, and no
timeout to cut it short.

**Suggested direction.** Make this `MigrateDatabaseAsync(this IServiceProvider services,
CancellationToken ct)` returning `Task`, call `MigrateAsync(ct)`, and await it from a startup path
that can surface a failure to the user rather than blocking app construction.

### INFRA-020 -- The migration history was squashed onto a new baseline identifier no database has applied

```yaml
id: INFRA-020
layer: INFRA
severity: S3
category: correctness
authority: microsoft
citation: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying
locations:
  - src/DungeonMaster.Infrastructure/Migrations/20260805014736_InitialCreate.cs:12
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs:36
effort: M
risk: medium
independent: true
```

**What.** Commit `7a4bb2d` ("chore(db): squash migrations to a single floor-level baseline")
replaced the six migrations then in the tree with a single baseline. Its diffstat shows five deleted
outright -- `RemoveFloorNumberUniqueIndex`, `AddDungeonNotesMaxLength`, `AddPlacedObjects`,
`MigrateToCanvasPoint` and `AddPlacedObjectScale` -- while the sixth, `20260511004006_InitialCreate`,
was rewritten to create the full four-table schema and renamed onto `20260805014736_InitialCreate`.
The baseline's identifier was therefore *changed*, not kept: `20260805014736` had never existed
before that commit. Only `AddFloorDescription` was added afterwards. The inline comment at
`PlacedObjectConfiguration.cs:36` ("DB default backs existing rows") is written for a world where
`Scale` arrived as an added column, which no migration in the tree does any more.

**Why it matters.** The cited page states that "migrations are applied to the database only if they
have not been already applied", the check being the recorded identifier. A database created before
the squash holds `20260511004006` in `__EFMigrationsHistory` and no row for `20260805014736`, so
`Migrate()` treats the new baseline as pending; the expectation is that its `CreateTable` calls then
run against tables that already exist and fail at startup, rather than being skipped quietly. The
project's decision on that case is recorded in `7a4bb2d`'s own message -- "Existing local databases
are not compatible and must be deleted; nothing shipped on the old history" -- which contains the
blast radius to developer machines and is why this is recorded at S3 rather than higher. What
survives is that the decision lives only in a commit message: nothing in the repository detects an
unrecognised history and refuses with a usable diagnostic, and nothing verifies that the
configuration files still agree with the squashed snapshot. The stale comment at
`PlacedObjectConfiguration.cs:36` is the visible residue of exactly that drift. `Down` on the
baseline drops all four tables, so a rollback is total data loss -- expected for a baseline, but
worth stating.

**Suggested direction.** Put the pre-squash decision where a reader will find it -- a note beside
the migrations or in the project README rather than only in `7a4bb2d`'s message -- and have startup
detect an unrecognised `__EFMigrationsHistory` and refuse with a clear message instead of failing
inside `Migrate()`. Update the stale comment at `PlacedObjectConfiguration.cs:36`, and add
`dotnet ef migrations has-pending-model-changes` to CI so future configuration edits cannot drift
from the snapshot unnoticed.

## Layer verdict

The Infrastructure layer holds its shape. It depends only on Domain and Application, it leaks
nothing outward -- both repositories return materialised `IReadOnlyList` or DTOs, never `IQueryable`,
so no EF expression tree escapes past `IDungeonReadRepository` or `IDungeonWriteRepository` -- and
`AppDbContext` is a thin, correct context whose only job is `ApplyConfigurationsFromAssembly`. The
value objects cross the boundary cleanly: `CanvasPoint` as an owned type with explicitly named
columns, `ObjectKind` and `ObjectRotation` as string conversions, `PathSegment` as JSON. Backing
fields and `PropertyAccessMode` are used correctly so the aggregate's encapsulation survives
persistence, which is the hardest thing to get right here and is right.

The seams are mostly in the right places, with two exceptions. The read/write repository split earns
its keep: the read side projects to DTOs in SQL and never materialises an entity, the write side
loads the aggregate root by navigation only, and the two have genuinely different query shapes
rather than being a naming exercise. But the write repository also owns the transaction boundary
(INFRA-014), which puts a unit-of-work responsibility inside a repository and closes off any
multi-aggregate command. And the composition root registers scoped services into a host that never
opens a scope (INFRA-018) -- the single most consequential finding in this layer, because the
lifetime mismatch is what turns INFRA-004's spurious writes and the unbounded change tracker from
inefficiencies into a slowly degrading session. The configuration files are individually sound but
inconsistently written, with `PlacedObjectConfiguration.cs` diverging from its three siblings in
indentation, lambda style and documentation.

The remaining weakness is the layer's fit to its actual database. `DateTimeOffset` on SQLite
(INFRA-003) and the unenforced length limits (INFRA-002) both reflect a model written against a
generic relational target rather than the provider in use, and the rewritten migration history
(INFRA-020) means the schema's provenance is not reproducible from the repository. None of this is
broken today; all of it is expensive to discover later. No boundary violation with an adjacent layer
was found, so T6 has no boundary record to inherit from this task beyond the App-side half of
INFRA-018.

## Attestation

- No file under `src/` was created, modified, or deleted by this task: yes
- Every record cites at least one file path with a line number: yes
- Every record names its authority, and every non-`judgment` record carries a real citation: yes
