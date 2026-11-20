# Spike findings — Session 6b-ii

Measured 2026-09-08 in worktree `C:\dm-wt\6b-ii` at commit `850a10b`, by two `dir-test-engineer`
spikes dispatched before decomposition. **Every figure here was produced by a command.** Claims the
orchestrator re-verified independently are marked **[re-verified]** and carry the orchestrator's own
command, run in the main checkout.

> **The governing fact.** `docs/audit/05-tests.md` was committed **2026-08-17**
> (`git log --diff-filter=A -- docs/audit/05-tests.md` → `2603170`). **28 commits have touched
> `src/DungeonMaster.Tests` since `bcb49dc^`** (`git log --oneline bcb49dc^..HEAD -- src/DungeonMaster.Tests | wc -l`).
> **Where this document and the audit disagree, this document wins and the audit entry is stale.**
> Do not resolve a conflict by re-reading the audit. Re-measure, or use the figure here.

---

## Entries already remediated — no code change required for the entry as written

### MASTER-066 / TEST-010 — closed by `bcb49dc`, and the prescribed fix would be a regression

`bcb49dc` (2026-08-18, "fix(handlers): extract mutation skeleton and map outcomes to results") is an
ancestor of HEAD (`git merge-base --is-ancestor bcb49dc HEAD` → yes) and rewrote 12 handler test files.

- **[re-verified]** `grep -rn "UpdateAsync" --include=*.cs src | grep -v '/bin/\|/obj/' | wc -l` → **0**.
  The member the entry cites does not exist. The write port is `void Update(Dungeon)`
  (`src/DungeonMaster.Application/Common/Interfaces/IDungeonWriteRepository.cs:41`) plus
  `IUnitOfWork.CommitAsync`.
- The audit's cited lines (`:115-117`, `:147-149`, `:123-125`, `:165-167`) resolve exactly at
  `bcb49dc^`. At HEAD those lines are arrange-block statements.
- The four assertions are now `DidNotReceive().Update(...)`, at
  `RemovePlacedObjectCommandHandlerTests.cs:131,166` and `UpdatePlacedObjectCommandHandlerTests.cs:139,184`.
- The save already skips on the failure path: `DungeonMutationExtensions.cs:51-54` returns before both
  `Update` (`:56`) and `CommitAsync` (`:58`).

**Demonstration.** Injecting the pre-fix behaviour — deleting the `if (result.IsFailure) return result;`
block from both `MutateAsync` overloads — produced `Failed: 14, Passed: 681, Total: 695`. Every one of
the 14 failed **only** on the mock assertion; all `ErrorCategory` assertions stayed green.
**Positive control:** deleting `unitOfWork.Dungeons.Update(dungeon);` instead produced
`Failed: 6, Passed: 689` — a different six, including the two `Received(1).Update` assertions.
Baseline `Failed: 0, Passed: 695`. Apparatus sound in both directions.

**Consequence: deleting these assertions, as TEST-010 directs, would remove the only thing in the
suite that catches save-on-failure.** The entry is not merely stale; acting on it is harmful.

### MASTER-041 / TEST-011 — closed by `bcb49dc`; the entry's stated belief is now measured fact

- **[re-verified]** `grep -rn "Task\.CompletedTask" --include=*.cs src/DungeonMaster.Tests | grep -v '/bin/\|/obj/' | wc -l`
  → **0**. Nothing remains to remove.
- TEST-011 says: *"I could not confirm against NSubstitute's published documentation that a
  `Task`-returning member is auto-substituted with a completed task, so I state this as belief rather
  than fact."* **Settled by execution**, on NSubstitute **5.3.0** (`Directory.Packages.props:29`):
  an unconfigured `Task` member returns a non-null, already-`IsCompletedSuccessfully` task;
  `Task<int>` completes with `0`; `ValueTask` completes. It does not hang and does not throw.
- **Two controls, both fired.** A configured `Returns(7)` yielded `7`; a deliberately hanging
  `TaskCompletionSource().Task` was detected in 2 s (`Failed: 1`). The green in the unconfigured case
  is a real negative, not a blind pass.

---

## Live entries, as measured

### MASTER-014 / TEST-002 — `EnsureCreated`, and it is 7 files not 4

- **[re-verified]** `grep -rn "EnsureCreated" --include=*.cs src/DungeonMaster.Tests | grep -v '/bin/\|/obj/'`
  → **7 call sites in 7 distinct classes.** Three appear in **neither** TEST-002's nor TEST-009's
  `locations` list, and two of those three live in `Configurations/` rather than `Repositories/`:

  | # | File | In TEST-002? | In TEST-009? |
  |---|---|---|---|
  | 1 | `Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs` | yes | yes |
  | 2 | `Infrastructure/Persistence/Repositories/DungeonReadRepositoryTests.cs` | yes | yes |
  | 3 | `Infrastructure/Persistence/Repositories/FloorPersistenceTests.cs` | yes | yes |
  | 4 | `Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs` | yes | yes |
  | 5 | `Infrastructure/Persistence/Repositories/UnitOfWorkTests.cs` | **no** | **no** |
  | 6 | `Infrastructure/Persistence/Configurations/AreaOutlineChangeTrackingTests.cs` | **no** | **no** |
  | 7 | `Infrastructure/Persistence/Configurations/PlacedObjectConfigurationTests.cs` | **no** | **no** |

- **[re-verified]** **3 migrations, not the 2 TEST-002 cites** —
  `ls src/DungeonMaster.Infrastructure/Migrations/*.cs | grep -v Designer | grep -v Snapshot | wc -l` → 3:
  `InitialCreate`, `AddFloorDescription`, `UtcDateTimeTimestampsAndScaleWithoutStoreDefault`.

**The fix is de-risked, proven in a scratch project without touching the worktree:**

- `Database.Migrate()` **works** against SQLite `Data Source=:memory:`. `APPLIED=3`, `PENDING=0`,
  `TABLES=6`.
- `dotnet ef migrations has-pending-model-changes` → *"No changes have been made to the model since
  the last migration."* Corroborated independently by `GetPendingMigrations()` returning empty.
  **The migrations are in sync with the model, so this change does not arrive red.**
- The `EnsureCreated` and `Migrate` schemas are **set-identical**: 11 objects each, none only-in-one,
  every table and index the same after sorting column definitions. The only raw differences are
  physical column *order* (`Floors."Description"`, `PlacedObjects."CreatedAt"/"LastModified"` land last
  under `Migrate` because they arrived by `ALTER TABLE ADD COLUMN`) and the two extra `__EFMigrations*`
  bookkeeping tables.

**Two residual risks to state as criteria rather than discover later:** a test doing positional
`SELECT *` reads (none found), and a test enumerating `sqlite_master` that would now see two extra
tables (none found).

### MASTER-065 / TEST-009 — the fixture is byte-identical; the *helpers* are not

> **[AMENDED 2026-09-08, at T7's review. The two bullets below are WRONG AS WRITTEN and are kept
> verbatim rather than silently rewritten.]**
>
> **Previous wording, verbatim:** *"The block … is **byte-identical across all 7 files** modulo the
> constructor's own name — md5 `af38458e567a2e4f8b577f22e7437cfa` for all 7."* and *"It is **17 lines**
> (13 non-blank, non-doc)."*
>
> **Measured**, `git show 850a10b:<file> | sed -n '/private readonly SqliteConnection/,/public void Dispose()/p' | wc -l`:
> **17 lines in six files, 21 in `UnitOfWorkTests.cs`**, which interleaves four XML doc lines.
> **Differing line counts make byte-identity impossible, so `af38458e…` cannot hold for all seven.**
> Identity holds only after stripping doc and blank lines: **13 lines, md5
> `5be5c19f761758c9211a53a50825f15f`**, reproduced per-file across all seven by T7's reviewer.
>
> **Classification: MEASUREMENT ERROR, and the orchestrator's.** Spike B's own report carried the
> caveat — *"Only `UnitOfWorkTests` carries XML-doc on the fixture members (its block is 21 raw lines
> vs 17)"* — and the orchestrator **dropped it** when composing this artifact, then stated the
> unqualified claim. The spike measured correctly; the summary lost it.
>
> **Consequence, checked:** the manifest's directed 19 → 17 swap for `TEST-009` would have replaced a
> statement true of four files with one false of the seventh. T7 refused the swap and recorded three
> scoped figures instead. **T1's extraction is unaffected** — it measured the helper asymmetry itself
> and provides both helpers unconditionally, and its reviewer verified the result empirically
> (`--list-tests` per class, 29 tests, clean build). **13, not 17, is the only figure true of all seven.**

- ~~The block from `private readonly SqliteConnection` through `public void Dispose()` is **byte-identical
  across all 7 files** modulo the constructor's own name — md5 `af38458e567a2e4f8b577f22e7437cfa` for all 7.~~
- ~~It is **17 lines** (13 non-blank, non-doc), **not the "nineteen" TEST-009 claims**. Nineteen is only
  reachable by including `CreateContext()`, which is not present everywhere.~~
- **Corrected:** the block is **17 lines in six files and 21 in `UnitOfWorkTests.cs`**; it is identical
  across all seven only at **13 non-blank, non-doc lines**, md5 `5be5c19f761758c9211a53a50825f15f`.
  TEST-009's "nineteen" was **correct when written** — the four files it named were each 19 lines at
  `2603170`, md5 `e61c65123b15ae41c5de23e362ab41fa`. That is drift, not an authoring error.
- **The variation an extract will get wrong:**

  | Class | `CreateContext()` | `CreateFactory()` |
  |---|---|---|
  | `AreaOutlineChangeTrackingTests` | **no** | yes |
  | `PlacedObjectConfigurationTests` | **no** | yes |
  | `AreaPersistenceDiagnosticTests` | yes | yes |
  | `DungeonReadRepositoryTests` | yes | **no** |
  | `DungeonWriteRepositoryTests` | yes | yes |
  | `FloorPersistenceTests` | yes | yes |
  | `UnitOfWorkTests` | yes | yes |

  5/7 have `CreateContext`, 6/7 have `CreateFactory`, 4/7 have both. A base must provide **both
  unconditionally**. All 7 files are CRLF throughout.

- **`TestDbContextFactory.cs` is an existing partial consolidation that neither entry mentions.** An
  `internal sealed class` implementing `IDbContextFactory<AppDbContext>` over the fixture's
  `DbContextOptions`, with **10 use sites across 7 files** (corrected by `dir-architect` at decomposition and re-verified by the orchestrator: `grep -rl "TestDbContextFactory" --include=*.cs src/DungeonMaster.Tests | grep -v '/bin/\|/obj/' | grep -v "Repositories/TestDbContextFactory.cs"` -> 7; this document originally said 6). **The shared base must keep `_options` (or a
  factory accessor) reachable, or these 10 sites break.**

### MASTER-085 / TEST-008 — two equivalence classes, and a ctor the audit describes no longer exists

- **16 confirmed**: 14 `*HandlerTests.cs` under `Commands/`, 2 under `Queries/`.
- **Not one shared constructor — two, of 14 and 2.** The 14 command classes use
  `UnitOfWorkSubstitute` + `new HANDLER(_unitOfWork.Factory)`. The 2 query classes use
  `Substitute.For<IDungeonReadRepository>()` + `new HANDLER(_readRepository)`.
- **TEST-008's described constructor is stale.** It says `Substitute.For<IDungeonWriteRepository>()` and
  `new XHandler(_writeRepository)`. A prior session already extracted
  `Application/Common/Handlers/UnitOfWorkSubstitute.cs` — *"The one place the substituted unit-of-work
  chain is wired"* — and all 14 use it. Its cited line ranges are stale to match.
- **The not-found test: 12 extractable, but "identical" is far too strong.** 25 tests assert
  `ErrorCategory.NotFound` across 13 classes. 12 use the dungeon-missing shape
  (`LoadAsync(...).Returns((Dungeon?)null)`), one each in 12 command classes. Within those 12:
  **5 are textually identical** (AddArea, RemoveArea, ReorderFloor, UpdateArea, UpdateFloor),
  **2 more form a second identical group** (RemoveFloor, UpdateDungeon), and **5 are unique**.
- **[re-verified]** **TEST-008 names the wrong exclusions.** It says CreateDungeon and DeleteDungeon are
  the two classes with no not-found test. `grep -n "NotFound" .../DeleteDungeonCommandHandlerTests.cs`
  → `65: HandleAsync_WithUnknownId_ReturnsNotFound` and `85: ErrorCategory.NotFound`. **DeleteDungeon
  has one**, reached via `RemoveAsync(...).Returns(false)` rather than a null load. **CreateDungeon is
  the only command class with no `NotFound` assertion at all.**
- **Does not fit the 12-way extraction:** `CreateDungeon` (no not-found path exists), `DeleteDungeon`
  (different signalling, and it also asserts the error contains the id and that `CommitAsync` did not
  happen), and both query classes (different fixture shape entirely).

### MASTER-069 / TEST-018 — **[OMITTED FROM THIS DOCUMENT. AMENDED 2026-09-08, at T3's review.]**

**This document had no MASTER-069 section.** Spike A measured the entry in full, and the orchestrator
wrote the result into `log.md` and into the Gate-1 report to the user, **but never into this file** —
`grep -c "MASTER-069" spike-findings.md` → `0`.

**The omission propagated.** `manifest.json:83` and `:84` cite this document for a "smaller half"
framing it never contained; `grep -rn "smaller half"` over the session directory returns only
`manifest.json:83` and `log.md:79`, both attributing the phrase to a document that does not say it.
The orchestrator then told the user T3 had "overturned this session's own governing artifact". **That
was false: the artifact held no position to overturn.** Found by T3's reviewer, not by reading.

**This section is deliberately NOT backfilled with the measurement.** Writing a dated measurement
record retroactively would make the citations resolve while destroying the evidence that they ever
failed to. The measurement lives where it was actually recorded on the day:

> `log.md:21` — *"MASTER-069/TEST-018 is live, and its blind spot is LARGER than the entry states:
> measured, the two query tests are blind both to identity (M1) and to every DTO field the assertions
> do not name (M3)."*

**What T3 actually did**, per its reviewer's independent reproduction: it confirmed that recorded
claim by run demonstration, and established the *mechanism* nobody had measured — both DTOs are
positional records with init-only members (`DungeonSummaryDto.cs:24`, `DungeonDetailDto.cs:30`), so no
field can change without allocating a new instance, and reference identity therefore implies field
equality. The two halves separate only for the **list**, which is why per-element identity rather than
list identity was the right criterion. That is a real result. It is not a reversal.

### MASTER-064 / TEST-007 — the one entry whose counts survived checking

- 14 `*ValidatorTests.cs` exist, but **the four the entry names are exactly the four that share the
  shape**, and the other ten differ. **Scope does not grow.**
- 6 validators mention `MaximumLength(100)`; only 4 have the sole rule
  `RuleFor(c => c.Name).NotEmpty().MaximumLength(100)`. `AddFloorCommandValidator` and
  `UpdateFloorCommandValidator` have `MaximumLength(100)` **without** `NotEmpty()`, plus separate
  `NotEmpty()` rules on `DungeonId`/`FloorId`; their test files are 76 and 108 lines, not 55.
- The four test files are 55 lines each and differ on **exactly three lines** — the three command
  constructions.
- **`CommandValidatorCoverageTests.cs` is not broken by a generic base.** It reflects over the
  *Application* assembly, never the test assembly, so a base class in `DungeonMaster.Tests` is
  invisible to it. It already carries its own non-emptiness guards
  (`commandTypes.Should().NotBeEmpty()`, `noPayload`/`valueProducing` likewise). **But it consumes
  `UnitOfWorkSubstitute`, which MASTER-085 also touches.**

### MASTER-067 / TEST-013 — half the entry refers to deleted code

- **[re-verified]** `git ls-tree -r --name-only HEAD | grep -i snapzone` → **nothing**.
  `SnapZoneCalculator` and its five facts were deleted in `1678d12` ("Remove SnapZoneCalculator and
  MapDrawingSession.LastVertex — neither has production callers"). **TEST-013 location 4 does not
  exist.** Location 5's path is also dead: `TileCoordinateHelperTests.cs` moved from
  `Application/Common/Map/` to `AppCore/Map/` in the same commit.
  **The "five snap-zone facts" in the title should be struck.**
- **[re-verified]** The geometry half is **four uniform facts, not three**.
  `grep -n "NoBare" App/Theme/GeometryScaleTests.cs` → six facts: `Spacing:436`, `Padding:469`,
  `Radius:514`, `Margin:555`, `Stroke:591`, `MinimumSize:629`.
  **Spacing, Margin, Stroke and MinimumSize** share one pipeline
  (`ScanXaml → Concat(ScanCSharp) → Where(!= ZeroMeansNone) → Select(Describe) → OrderBy → BeEmpty`),
  differing only in the number of concatenated literals in the `because` message. `Padding` adds a
  `PaddingScaleExceptions` filter and an `IsShapingPadding` projection; `Radius` adds a
  `RadiusGeometryExceptions` filter. TEST-013 correctly excludes Padding and Radius but **misses
  `NoBareMinimumSizeValues`. A `[Theory]` sized for three under-covers.**
- All three of TEST-013's cited geometry line ranges (`300-324`, `428-453`, `466-492`) are stale.

**The vacuity trap, measured.** There is no `Assert.All` or bare `foreach` in an assertion position
here — the 6 `Assert.All` sites in the repo are all in `App.Tests/Graphics/FloorMapDrawableDisposalTests.cs`
and **all six are preceded by an `Assert.NotEmpty` on the same collection**, closed by a prior session.
Non-emptiness guards do exist as two dedicated facts,
`TheGeometryXamlScanReachesAtLeastOneApplicationSource:410` and
`TheGeometryCSharpScanReachesAtLeastOneApplicationSource:423`, matching the shape mandated by
`.dir-sdlc/sessions/2026-09-02-app-sub-suite-rebuild/contracts/anti-vacuity.md` §2.

> **The new hole the conversion opens.** Those two guards protect the *file scan*, not the *row set*.
> Once four facts become four rows of a data table, **silently dropping a row removes a whole ratchet
> and every remaining test still passes green.** xUnit errors only on a completely empty theory source,
> never on a short one. A non-emptiness assertion is **not sufficient**; a **cardinality assertion** is
> required, in a new named `[Fact]` beside the two existing scan guards, asserting the row set equals
> the expected named set. **This must be an acceptance criterion, not left to the implementer.**

### MASTER-042 / TEST-012 — one of the two claimed subsumptions is false

- **`RemoveArea_PersistsAcrossReload` is genuinely subsumed** by
  `RemoveArea_WithEmptyOutline_PersistsAcrossReload` (same file, `:75`). The two are line-for-line
  identical except the wider one adds `loaded!.Floors.Single().Areas.Single().Outline.Should().BeEmpty();`.
  **Deleting the narrow one loses nothing.**
- **`UpdateArea_RenamePersistsAcrossReload` is NOT subsumed. Deleting it loses coverage.**
  `UpdateArea_SecondRenamePersistsAcrossReload:116` does rename→commit, then reload→rename→commit, and
  asserts only the final name. The second unit of work reloads from the database and overwrites the
  name **regardless of whether the first commit persisted**, so the first rename's persistence is
  never observed.

  **Demonstrated by injection, with a passing control.** Removing the `CommitAsync` from the first
  rename block:
  ```
  SecondRename assertion (== "Crypt")   would PASS with the first commit removed
  RenamePersists assertion (== "Chapel") would FAIL with the commit removed
  ```
  The control fails, so the injection is live.
- **The coverage is not replaceable.** Every other `RemoveArea`/`UpdateArea` hit is a Domain aggregate
  test (in-memory, no persistence) or a handler test against a substituted repository.
  `AreaOutlineChangeTrackingTests` covers `UpdateAreaOutline`, a different method.
  **This class holds the only persistence round-trip coverage of area removal and area rename.**
- **The audit's prose points the wrong way**, presenting `WithEmptyOutline` as the redundant one. It is
  the *narrow* test that is deletable. **A criterion saying "drop the two subsumed tests" destroys
  real coverage. It is one deletion.**

### MASTER-127 / TEST-026 — seven, not four

- **[re-verified]** `grep -rn "class .*: *IDisposable\|class .*, *IDisposable" --include=*.cs src/DungeonMaster.Tests | grep -v sealed | wc -l`
  → **7**, and they are **exactly the same 7 classes** that call `EnsureCreated`.
- TEST-026's title and body say "four" while its own `locations` list has five entries across four
  files — **it under-counts against itself**, and misses the same three files TEST-002 and TEST-009 miss.
- Already sealed and correct: `RepoPathsTests`, `ObjectCatalogueScope`,
  `FloorMapDrawableDisposalTests.Harness`, `MauiHostFixture`, `AppFixture`. No `IAsyncDisposable` or
  `IAsyncLifetime` anywhere in `DungeonMaster.Tests`.

---

## The decision the user took at the re-scope gate

**MASTER-065 uses the abstract base class route.** A non-sealed base with
`protected virtual Dispose(bool)`; the 7 leaf classes inherit and are **sealed**. This was chosen over
`IClassFixture<SqliteFixture>` because it preserves the per-test database isolation the layer verdict
says is currently wanted, and because the `IClassFixture` route would make MASTER-127 a no-op for all
7 files — the outcome most likely to produce a "done" report claiming work that was never needed.

**Consequences that are now fixed and not the architect's to revisit:**
- MASTER-127 is **real work**: 7 sealed leaves plus exactly one correctly-patterned non-sealed base.
- **Promoting one of the 7 to be the base is prohibited** — sealing it would then contradict MASTER-127
  directly. The base must be a **new** type.
- The base must keep `_options` reachable so `TestDbContextFactory`'s 10 use sites keep working.

**MASTER-066 and MASTER-041 are marked done against `bcb49dc` with the evidence above, and the session
additionally acts on two gaps the spike exposed that no entry covers:**
1. **The commit-boundary assertion.** `DungeonMutationExtensions.cs:51-58` shows the failure path skips
   both `Update` and `CommitAsync`. `_unitOfWork.UnitOfWork.DidNotReceive().CommitAsync(...)` is
   strictly more observable than the repository-boundary `Dungeons.DidNotReceive().Update(...)`.
   **Not measured by the spike — it is reasoning from read evidence, and the implementing task must
   demonstrate it before relying on it.**
2. **`[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` is absent from `DungeonMaster.Tests`**,
   so `Substitute.For<T>()` on an `internal` interface throws
   `System.ArgumentException : Can not create proxy for type … because it is not accessible`.
   Found incidentally when a spike probe failed on exactly this.

---

## Collisions — what the decomposition must respect

**Five of these entries carry `independent: true`. Only MASTER-064 actually is.**

### Same-file collisions

| File | Entries that must edit it |
|---|---|
| `Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs` | **014, 065, 042, 127 — four-way, the worst in the set.** 042 also **renames the file**, invalidating every other entry's path. |
| `Infrastructure/Persistence/Repositories/DungeonReadRepositoryTests.cs` | 014, 065, 127 (and the only one of the 7 with no `CreateFactory`) |
| `Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs` | 014, 065, 127 |
| `Infrastructure/Persistence/Repositories/FloorPersistenceTests.cs` | 014, 065, 127 |
| `Infrastructure/Persistence/Repositories/UnitOfWorkTests.cs` | 014, 065, 127 — **listed in none of the three entries** |
| `Infrastructure/Persistence/Configurations/AreaOutlineChangeTrackingTests.cs` | 014, 065, 127 — **listed in none of the three** |
| `Infrastructure/Persistence/Configurations/PlacedObjectConfigurationTests.cs` | 014, 065, 127 — **listed in none of the three** |
| `App/Theme/GeometryScaleTests.cs` | 067, plus the frozen `anti-vacuity.md` contract |
| `Application/Common/Handlers/UnitOfWorkSubstitute.cs` | 085 directly; 064 indirectly, because `CommandValidatorCoverageTests` consumes it from another directory |

**The seven Infrastructure files are touched by `014` + `065` + `127` + `042` simultaneously.**
DIR forbids concurrent overlapping ownership, so these four entries cannot be four parallel tasks.

### Constraint collisions

1. **065 → 127.** Settled by the user's decision above.
2. **014 → 065.** Land together so the schema strategy changes once. De-risked by the `Migrate()`
   proof, but it is a **7-file** change, not 4.
3. **042 ↔ 014/065/127 (undeclared by the audit).** 042 renames a file the other three all edit. It is
   marked `independent: true, effort: XS`; **both are wrong.**
4. **085 ↔ 064 (undeclared).** Both would introduce an abstract generic test base in
   `DungeonMaster.Tests`. **One convention decision, not two** — otherwise the diff carries two shapes
   for the same idea.
5. **064 ↔ `CommandValidatorCoverageTests` (undeclared).** No functional conflict, but it consumes
   `UnitOfWorkSubstitute`, which 085 touches. Cross-task, cross-directory.
6. **067 ↔ the frozen `anti-vacuity.md` contract (undeclared).** §2 mandates guard facts be `[Fact]`,
   not `[Theory]`, named for what they guard; §5 says an offenders list empty from a dead scan must not
   look like one empty from a clean codebase.

---

## What the spikes did not settle

- **Neither spike ran `dotnet test` at baseline in a clean worktree with no peer activity.** Spike A
  measured `Passed: 695, Total: 695` before its first injection and again after full restore, which
  matches the inherited figure; Spike B deliberately ran nothing, to avoid racing the peer. Treat 695
  as confirmed by Spike A and re-measure before relying on it.
- Whether `DidNotReceive().CommitAsync(...)` is strictly better than `DidNotReceive().Update(...)` is
  **reasoning from read evidence, not measurement.**
- Whether audit entries outside these ten are equally stale. Strongly suspected. **6b-iii inherits five
  TEST-* entries and should be told.**

## Cleanliness

Both spikes finished with `git status --porcelain` empty in `C:\dm-wt\6b-ii`; Spike A additionally ran
`git update-index --refresh` and re-measured `Passed: 695`. Spike A temporarily edited three production
files and restored each from a scratch byte-copy, reporting this loudly because the orchestrator's brief
had contradicted itself by declaring Application source read-only while asking for handler injections.
Spike B modified nothing.
