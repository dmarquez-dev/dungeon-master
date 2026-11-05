<!-- frozen: 20261105-1907-core-heavy-rows, Gate 2 -->

# C2 -- ObjectRotation member rename and its data migration

No callable operation crosses a task boundary: T1 implements and tests everything here. This contract fixes
the names, the stored-value mapping, the error cases and the injection protocol so that T1's reviewer and
T4 (which cites the migration in the register) read one definition. Baselines are at startRef `62ba566`
(C1 header).

## 1. Names

| Thing | Value |
| --- | --- |
| Enum file | `src/DungeonMaster.Domain/Entities/Floors/ObjectRotation.cs` |
| Members after T1, in this order | `None`, `Clockwise90`, `Clockwise180`, `Clockwise270` |
| Migration name (scaffold argument) | `RenameObjectRotationClockwiseMembers` |
| Migration ID | `<UTC timestamp, 14 digits>_RenameObjectRotationClockwiseMembers`, the timestamp chosen by the scaffolder; it sorts after the previous ID |
| Previous (newest at startRef) migration ID | `20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault` |
| Migration files | `src/DungeonMaster.Infrastructure/Migrations/<ID>.cs` and `<ID>.Designer.cs` |
| Snapshot | `src/DungeonMaster.Infrastructure/Migrations/AppDbContextModelSnapshot.cs` |
| Proof test file | `src/DungeonMaster.Tests/Infrastructure/Persistence/RenameObjectRotationClockwiseMembersMigrationTests.cs`, class `RenameObjectRotationClockwiseMembersMigrationTests`, namespace `DungeonMaster.Tests.Infrastructure.Persistence` |

Scaffold command (it builds `DungeonMaster.Infrastructure`; C1 section 9 applies):

```
dotnet ef migrations add RenameObjectRotationClockwiseMembers --project C:/dm-wt/20261105-1907-core-heavy-rows/src/DungeonMaster.Infrastructure --startup-project C:/dm-wt/20261105-1907-core-heavy-rows/src/DungeonMaster.Infrastructure
```

`AppDbContextFactory` (an `IDesignTimeDbContextFactory`) is in `DungeonMaster.Infrastructure`, which is why the
project is its own startup project; the orchestrator's `has-pending-model-changes` probe used the same pair.

Ordinals do not move: `None` stays 0 and the three renamed members keep 1, 2, 3.
`DungeonDetailPage.xaml.cs:140-141, :430, :2066-2067` cycle rotation by ordinal `% 4` (orchestrator's reading;
T1 owns no file there and changes none). The three member `///` summaries already read "The object is rotated
90 / 180 / 270 degrees clockwise." (`ObjectRotation.cs:14, :19, :24` at startRef), which stays accurate for the
new names, so their text is kept.

## 2. Stored-value mapping (the migration's whole behaviour)

Scope: table `"PlacedObjects"`, column `"Rotation"` (TEXT; `PlacedObjectConfiguration.cs:39-42` maps
`obj.Rotation` with `.HasConversion<string>()`; `20260805014736_InitialCreate.cs:86` creates the column). No other
table or column is read or written.

| Stored before `Up` | Stored after `Up` |
| --- | --- |
| `'None'` | `'None'` (untouched) |
| `'CW90'` | `'Clockwise90'` |
| `'CW180'` | `'Clockwise180'` |
| `'CW270'` | `'Clockwise270'` |

`Down` is the exact inverse: `'Clockwise90'` -> `'CW90'`, `'Clockwise180'` -> `'CW180'`, `'Clockwise270'` -> `'CW270'`,
`'None'` untouched.

**Error cases.**
- A value outside the direction's source set (for `Up`: anything but the three `CW` strings; for `Down`: anything
  but the three `Clockwise` strings) is left exactly as it is. Both directions are therefore idempotent, the
  same property the sibling `20260818232300_...cs` states for its own rewrite.
- A database with no `PlacedObjects` rows is a no-op in both directions.
- `Down` produces strings the renamed enum cannot materialise. That is inherent in reverting a data migration
  behind a renamed enum and is the same contract every `Down` here has: it restores the previous schema and data
  for the previous code.

**Shape.** The migration changes data only. Its `Up` and `Down` contain `migrationBuilder.Sql(...)` calls and no
schema operation (`database.md`, Migrations: "A migration that changes data is separate from one that changes
shape"). It is reversible (`database.md`: "Every migration is independently reversible or explicitly declares
that it is not"). Instrument, run on the migration `.cs` file: `grep -c 'migrationBuilder\.' <file>` equals
`grep -c 'migrationBuilder\.Sql(' <file>`. Control (spec phase): on
`20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault.cs` the two print 4 and 2 (it holds two
`AlterColumn` calls, so it must and does differ); on `20260805184106_AddFloorDescription.cs` they print 2 and 0.

**Snapshot.** The rename changes no model shape (the column is a string either way), so the scaffolder's `Up` and
`Down` are empty and are filled by hand, and the regenerated snapshot is expected to equal startRef: raw
`sha256sum` `7908b66a8da2ecdc8dfbdfa3e89542e7419b5695b278be3a5ebf56c0b9dffe8d`, CR-stripped
(`tr -d '\r' < <file> | sha256sum`) `dc7624599058c675cc2efef4ddb2919ca69a0880f492aff7836d9bad137ca38c`. A
difference is not by itself a failure; T1 reports it line by line.

**Documentation.** The migration class carries a `<summary>` in the form of the sibling `20260818232300_...cs`
(`///` lines inside the scaffolded four-space layout): what `Up` and `Down` rewrite, the three mappings, that
`None` and unrecognised values are untouched, idempotency, and why the migration exists (MASTER-130 / CORE-024:
`ObjectRotation` is persisted by member name, so the rename rewrites stored values).

## 3. Proof test (DoD D3)

The test file is self-contained: each test opens its own `Data Source=:memory:` connection (or derives from
`SqliteInMemoryTestBase`, whose constructor migrates to latest) and moves the schema with
`context.GetService<IMigrator>().Migrate(<target ID>)`; nothing in the repository does this yet. Rows enter the
old-value state by raw SQL, because the renamed enum can no longer write `'CW90'`. Required observations, each a
separate assertion a reader can find:

- **O1 (Up rewrites).** From a database at the previous migration holding four `PlacedObjects` rows whose
  `Rotation` is `'None'`, `'CW90'`, `'CW180'`, `'CW270'`, migrating to latest leaves, read back by raw SQL,
  `'None'`, `'Clockwise90'`, `'Clockwise180'`, `'Clockwise270'` on the respective rows.
- **O2 (Up yields loadable rows).** After that `Up`, loading the rows through EF materialises
  `ObjectRotation.None`, `Clockwise90`, `Clockwise180`, `Clockwise270` respectively.
- **O3 (Down reverses).** From a database at latest holding rows with each of the four members (written through
  EF or raw SQL), migrating to `20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault` leaves, by raw SQL,
  `'None'`, `'CW90'`, `'CW180'`, `'CW270'` on the respective rows.
- **O4 (Up leaves an unrecognised value alone).** A row at the previous migration whose `Rotation` already reads
  `'Clockwise90'` still reads `'Clockwise90'` after migrating to latest (the first error case of section 2).
- **O5 (Down leaves an unrecognised value alone).** A row at latest whose `Rotation` reads `'CW90'` (written by raw
  SQL) still reads `'CW90'` after migrating to `20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault`.

Every member of `ObjectRotation` (four) appears in O1, O2 and O3; none is left uncovered. Test names state the
condition and the outcome (`test.md`, Naming).

## 4. Migration-count ratchet

`src/DungeonMaster.Tests/Infrastructure/Persistence/SqliteInMemoryTestBaseTests.cs:18` holds
`private const int AppliedMigrationCount = 3;`, and its doc comment at `:14-16` names the command
`ls src/DungeonMaster.Infrastructure/Migrations/*.cs | grep -v Designer | grep -v Snapshot | wc -l` and the three
migrations. After T1 the constant is 4 and the comment names the command, its result 4 and all four migrations.
`AppliedMigrations_CountMatchesTheMigrationFilesOnDisk` fails if the constant does not move, which is its purpose.

## 5. Injection protocol (T1 only; demonstrated discrimination)

T1 also shows the red step: with the scaffolded empty `Up`/`Down`, O1 and O3 fail. In addition:

- **J1:** in `Up`, remove only the mapping of `'CW180'` (one site). Narrowest command:
  `dotnet test C:/dm-wt/20261105-1907-core-heavy-rows/src/DungeonMaster.Tests/DungeonMaster.Tests.csproj --filter FullyQualifiedName~RenameObjectRotationClockwiseMembersMigrationTests`.
  Expected: an O1 assertion fails on the `CW180` row, and the O3 tests still pass.
- **J2:** in `Down`, remove only the mapping of `'Clockwise270'` (one site). Same command. Expected: an O3
  assertion fails on the `Clockwise270` row, and the O1/O2 tests still pass.

Before each injection, record `tr -d '\r' < <migration .cs> | sha256sum`; after reverting, the digest must equal
it again, and B must show the file CRLF with loneCR=0. Report each window: the file and line injected at, the
text removed, the command, its failing output, and the revert digest. The injection lives only in T1's own file,
and no other task runs a `dotnet` command (C1 section 9), so no peer reads the injected state; T2 and T3, which
may run alongside T1, read only the Application layer (T2, C4 section 1.2) and enum declarations under `src` (T3, C3), neither of which
an edit to a migration's SQL can change.
