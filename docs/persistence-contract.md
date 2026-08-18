# The persistence contract

This project has **no mapping layer between the Domain and the database**. The EF Core configurations
map the Domain entities directly, so a CLR type chosen in `src/DungeonMaster.Domain/` *is* the storage
decision. This file records what that costs, so the next type choice is made with the information
rather than after it.

It is the document `XCUT-007` asks for and `MASTER-103` tracks.

## 1. The target

| | |
|---|---|
| Provider | SQLite, via `Microsoft.EntityFrameworkCore.Sqlite` |
| Deployment | One local file per user, no server, no network |
| Path | `{LocalApplicationData}/DungeonMaster/dungeon-master.db`, built in `src/DungeonMaster.App/MauiProgram.cs` and defaulted to by `src/DungeonMaster.Infrastructure/Persistence/AppDbContextFactory.cs` so design time and run time resolve to the same file |
| Concurrency | Single writer. The application is a single-user desktop app; there is no connection pool to size and no contention to design against |
| Schema management | EF Core migrations, applied at startup by `MigrateDatabaseAsync` |

Two consequences follow from "SQLite" and are the root of most of the rows in the next section.
SQLite has a **very small set of storage classes** — `NULL`, `INTEGER`, `REAL`, `TEXT`, `BLOB` — so
every richer CLR type is encoded into one of them by the provider, and the encoding is what queries
must then operate on. And SQLite **enforces almost nothing** it is told about a column beyond
nullability and foreign keys.

## 2. Which CLR types are cheap, and which are not

| CLR type | Stored as | Cheap? | Why |
|---|---|---|---|
| `DateTime` (UTC) | `TEXT`, e.g. `2026-08-18 14:05:06.7891234` | **Yes** | Natively supported by the provider. The format is lexicographically sortable, so ordering and range comparison happen in SQL. |
| `DateTimeOffset` | `TEXT` with an offset suffix, e.g. `2026-08-18 14:05:06.7891234+00:00` | **No** | [SQLite limitations](https://learn.microsoft.com/ef/core/providers/sqlite/limitations) lists `DateTimeOffset` as not natively supported and states that operations other than equality "will require evaluation on the client". This is what `INFRA-003` found and `MASTER-029` changed. |
| `float` / `double` | `REAL` | **Yes** | Direct mapping to a SQLite storage class. `Scale`, `CanvasPoint.X` and `CanvasPoint.Y` all take this route. |
| `string` | `TEXT` | **Yes, with a caveat** | The mapping is free. But **`HasMaxLength` is model metadata only and SQLite enforces none of it** — there are eleven such calls in the configurations today and not one of them is a database constraint. Tracked as `INFRA-002` / `MASTER-152`, recorded here as context and *not* fixed by this session. Treat length limits as a validation concern, in the Application layer, not a storage guarantee. |
| `Guid` (keys) | `TEXT` | **Yes** | All four keys are `ValueGeneratedNever()`: identity is assigned in the Domain constructor, not by the store. That is also why `DbSet<T>.Add` is used rather than `AddAsync` — the [`AddAsync` remarks](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbset-1.addasync) reserve the async form for models with a database-backed value generator, which this model has none of. |
| `enum` | `TEXT`, via `HasConversion<string>()` | **Yes** | `ObjectKind` and `ObjectRotation`. Storing the name rather than the ordinal keeps the database readable and survives a reordering of the enum. |
| Collection of value objects | `TEXT`, via a JSON `HasConversion` pair | **Yes, with a caveat** | `Area.Outline` is a JSON column. It is not queryable as a collection, which is fine because nothing queries into it. **A converted reference type needs a `ValueComparer` or change tracking is broken** — see section 3. |

### 2.1 The timestamp formats, measured

These were established by observation against the EF Core 10.0.11 SQLite provider, not assumed, because
the migration that rewrote the stored values had to be written against the real shapes:

```
DateTimeOffset (UTC)  ->  2026-08-18 14:05:06.7891234+00:00     (33 characters)
DateTime              ->  2026-08-18 14:05:06.7891234           (27 characters)
```

The two forms are identical apart from the six-character offset suffix. A `DateTime` read back out of
the second form arrives with `Kind == DateTimeKind.Unspecified` — the provider does not record a kind,
so **the "these are UTC" invariant lives in the Domain, in `Entity`, and nowhere else**. Every write
goes through `DateTime.UtcNow`; nothing may introduce `DateTime.Now`.

## 3. Two hazards that are not visible in the type

### 3.1 A store default makes the CLR default unsendable

Generalising `INFRA-008`: when a property is configured with `HasDefaultValue(...)`, EF marks it
**store-generated on add**. The [generated-values documentation](https://learn.microsoft.com/ef/core/modeling/generated-properties)
states that to override that generation you must "set the property to any value that is not the CLR
default value for that property's type". So the CLR default becomes a value the application *cannot
send*: an insert carrying it is silently replaced by the store default.

That is safe only while the domain range provably excludes the CLR default — and that is two facts in
two projects with no compiler or test link between them.

**The worked example.** `PlacedObject.Scale` was mapped
`.HasColumnType("REAL").HasDefaultValue(1.0f)`, which made `0f` unsendable. It was safe only because
`PlacedObject`'s constructor rejects anything outside `[0.25, 4.0]`. Widen that range to admit zero and
every such insert lands as `1.0f` with no error.

**What this session did (`MASTER-075`).** Dropped `HasDefaultValue` outright. The backfill it existed
for has long since been applied, `Scale` is always supplied by the constructor, and the value now
travels as ordinary data. `AppDbContextModelSnapshot.cs` no longer records the property as
`ValueGeneratedOnAdd()`, and the `PlacedObjects` DDL no longer carries `DEFAULT 1`.

EF Core 9's `HasSentinel(0f)` is the other way to state this coupling in one place, and it is the right
tool if a store default is ever genuinely needed. It was **not** used here, because with the backfill
done the default itself had no remaining purpose.

### 3.2 A converted reference type needs a `ValueComparer`

Generalising `INFRA-004`: for a property mapped through `HasConversion`, EF falls back to the CLR
type's default equality when comparing the change-tracking snapshot with the current value, and for a
reference type with no comparer the [value-comparers documentation](https://learn.microsoft.com/ef/core/modeling/value-comparers)
notes that "no copying occurs, and the same instance is used as the snapshot".

`Area.Outline` returns `_outline.AsReadOnly()` — a freshly allocated wrapper on every read — so the
snapshot and the current value were never the same reference and every tracked `Area` was reported
modified on every `SaveChanges`, re-running the serializer and rewriting the column with identical
bytes. `AreaConfiguration` now supplies the third `HasConversion` argument: a
`ValueComparer<IReadOnlyList<PathSegment>>` built from `SequenceEqual`, an aggregated per-element hash,
and `ToList()` for the snapshot. `PathSegment` is a `record`, so the comparer composes its
compiler-generated structural equality rather than hand-rolling element comparison.

The inverse hazard is latent and worth stating: if `Outline` were ever changed to hand back `_outline`
directly, the in-place mutation in `Area.SetOutline` would become invisible to change tracking and
edits would be silently lost. The snapshot lambda (`ToList()`) is what protects against that, and it is
why the comparer takes three lambdas rather than one.

## 4. The rule for the next type choice

**A Domain type choice is a persistence choice on this project.** There is no mapping layer between the
two, so the type on the entity is the type EF maps and the shape SQLite stores. Before adding a new
timestamp, decimal, collection or value-object property to an entity, check it against the table in
section 2 and against both hazards in section 3: does the provider support it natively, or will
comparisons fall back to the client; does it need a `ValueComparer`; and would a store default make
some legal domain value unsendable. If the answer is not obvious from this file, the honest move is to
run the experiment — create a database, write a row, read the raw column — rather than to reason about
it, because that is how the two entries above were settled.

## 5. Where this is pointed at from

`src/DungeonMaster.Domain/Entities/Entity.cs` declares `CreatedAt` and `LastModified`, and each carries
a `<remarks>` block naming this file. That is the one place in the Domain where a persistence-relevant
type decision is made for all four entity types, and the link runs in both directions: the entity says
"this is a persistence decision, see here", and this file says "the decision is made there".

## Migration history

The migration history in `src/DungeonMaster.Infrastructure/Migrations/` **does not run continuously
back to the first release.** It was squashed, and the decision that came with the squash lived only in
a commit message until now. This section is what `INFRA-020` / `MASTER-093` asks for.

Commit `7a4bb2d` ("chore(db): squash migrations to a single floor-level baseline") replaced the
migrations then in the tree with a single baseline. Five were deleted outright —
`RemoveFloorNumberUniqueIndex`, `AddDungeonNotesMaxLength`, `AddPlacedObjects`, `MigrateToCanvasPoint`
and `AddPlacedObjectScale` — while `20260511004006_InitialCreate` was rewritten to create the full
four-table schema and **renamed onto `20260805014736_InitialCreate`**. The baseline's identifier was
therefore *changed*, not kept: `20260805014736` had never existed in the repository before that commit.

The decision recorded in that commit's message, verbatim:

> Existing local databases are not compatible and must be deleted; nothing shipped on the old history.

That is what contains the blast radius to developer machines. A database created before the squash
holds `20260511004006` in `__EFMigrationsHistory` and no row for `20260805014736`, so `Migrate()` treats
the baseline as pending and its `CreateTable` calls run against tables that already exist. Deleting the
local file is the supported recovery.

**Two related suggestions from `INFRA-020` are open, not done.** Neither is closed by this document and
both are on the backlog:

- Adding `dotnet ef migrations has-pending-model-changes` to CI. **There is no `.github/workflows`
  directory in this repository**, so there is nothing for it to attach to, and creating a pipeline was
  out of scope. Until then, run it locally after editing any configuration file.
- Having startup detect an unrecognised `__EFMigrationsHistory` and refuse with a clear message. That
  is a new user-visible startup error surface, which is the same view work `MASTER-012` deferred.

### The migrations in the tree

| Identifier | What it does |
|---|---|
| `20260805014736_InitialCreate` | The squashed baseline. Creates `Dungeons`, `Floors`, `Areas`, `PlacedObjects`. Its `Down` drops all four tables, so rolling back past it is total data loss — expected for a baseline, but worth knowing. |
| `20260805184106_AddFloorDescription` | Adds `Floors.Description`. |
| `20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault` | `MASTER-029` and `MASTER-075`. Rewrites the stored values of the eight timestamp columns from the `DateTimeOffset` shape to the `DateTime` shape, and drops the `Scale` store default. The value rewrite is hand-written `Sql(...)`: `migrations add` emits schema operations, never data ones, and here the schema did not change at all because both CLR types land in SQLite as `TEXT`. |
