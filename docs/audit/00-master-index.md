# DungeonMaster solution -- code quality audit, master index

## Verdict

This is a well-built codebase whose architecture holds, and whose one systemic weakness is that
failure information is discarded at almost every seam it crosses. Six auditors read all 205 source
files across five projects, measured them against Microsoft's guidance, general practice and the
project's own `dm-conventions`, and produced 167 records. Exactly one is S1. That ratio is the
headline: for a solution of roughly 24,000 lines carrying a hand-rolled CQRS dispatcher, a
hand-written EF Core mapping of an aggregate with value objects, a nine-control MAUI kit and a
three-tier theme system, one critical finding is a good result, and it was found in the layer where
a critical finding is cheapest to fix.

The architecture holds without qualification. All four dependency directions are correct and not
violated anywhere: Domain references nothing, Application references Domain, Infrastructure
references both, App references Application and Infrastructure. The Domain contains no framework
type and no persistence vocabulary across all twenty of its files. The read/write repository split
is real rather than decorative, and no `IQueryable` escapes past either interface. The theme system
under `Resources/Theme/` is the strongest single subsystem in the repository: a genuine three-tier
token arrangement whose merge order, naming rule, C#-to-XAML parity and literal hygiene are all
enforced by facts that read the files off disk, and the classic bindable-property copy-paste defect
does not occur once across nine controls.

What does not hold is the treatment of failure, and it recurs in three layers by three unrelated
mechanisms. Domain mutators return `void` and quietly no-op on an unknown child id, so the handler
returns `Success(true)` for an operation that did not happen (`CORE-001`). The write repository
attaches a detached aggregate, which marks it `Unchanged`, so a save does nothing (`INFRA-012`). And
the detail view model throws away every `Result` the dispatcher returns, so a rejected write is
invisible to the user (`UI-004`, the S1). These three are not the same fix and this index does not
merge them, but they compose into one user-visible behaviour: an edit that silently does not happen.

Two further problems are structural rather than local. The composition root is split across
`DependencyInjection.cs` and `MauiProgram.cs`, and neither states a lifetime contract, so scoped
services resolve from the root provider and one `DbContext` lives for the process (`MASTER-023`).
And the editor's state machine has no seam: 2,226 lines of code-behind hold draw, selection, drag
and hover state whose invariants against the view model are maintained by hand.

Below that, the picture is ordinary and healthy. 79 records are S3 design weaknesses with no
correctness consequence, 40 are style and documentation, and 15 are observations. The build is clean
at zero warnings under `TreatWarningsAsErrors`, which is a demanding baseline honestly met -- but
nothing about the build is pinned, and no analyzer above the SDK default floor is running, which is
why all 40 style records had to be found by people reading files. Fixing that is the cheapest and
first-ordered session in this plan.

## Progress

The verdict and per-layer sections below record the codebase **as audited on 2026-08-17** and are left
unedited as that record. This section is the running state; the `Status` column in the master index is
its per-entry form.

| Session | Entries | State | Landed |
|---|---|---|---|
| 1 -- Pin the build | 10 | **Complete** 2026-08-18 | `66e41ef`, `cf49081`, `413be78`, merged `079195f` on `chore/repo-audit` |
| 2 -- Make failure observable in the Application core | 16 | **Complete** 2026-08-18 | `afbed1e`, `781f73b`, `91658eb`, `3b44d91`, `f9a665b`, `bcb49dc`, `09f4360`, `476259c`, `5e09334` on `chore/repo-audit` |
| 3 -- Settle the data-access lifetime and the persistence contract | 16 | **Complete** 2026-08-18 | `ef0b0b6`, `1453cbd`, `9051eeb` on `chore/repo-audit` |
| 4 -- Async, disposal and threading discipline | 7 | **Complete** 2026-08-25 | `33da60f`, `706b6da`, `7d00448`, `b461bab`, `5fa398a`, `32a020a` on `chore/repo-audit` |
| 5 -- Give the editor a seam and the control kit its own contract | 32 | **In progress** -- split into 5a/5b/5c; 5a **Complete** 2026-08-26 (5 of 32 entries) | 5a: `828327a`, `8eded17`, `1678d12`, `36e3b87`, merged `dfa49d7`, artifacts `65b9a8a` on `chore/repo-audit` |
| 6 -- Rebuild the App sub-suite and de-duplicate the fixtures | 29 | Not started | |
| 7 -- Style, naming and documentation sweep | 31 | Not started | |

**What Session 1 changed for the sessions after it.** Three things the plan was counting on are now
facts rather than intentions:

- **The indent character is settled: tabs**, enforced by `IDE0055` at `warning` under
  `TreatWarningsAsErrors`. Session 7's reformat work is now enforced by the build rather than by a
  reviewer, which is the dependency its rationale named.
- **`AnalysisMode` is `Recommended` and `EnforceCodeStyleInBuild` is on.** Twelve CA rules that fired
  are parked at `suggestion` inside a banner-delimited `ANALYZER BACKLOG` block in `.editorconfig`,
  each carrying its occurrence count -- `CA1707` (125), `CA1859` (14), `CA1816` (4), `CA1822` (4),
  `CA1001` (3), `CA1000` (2), `CA1826` (2), and one each of `CA1304`, `CA1711`, `CA1716`, `CA1825`,
  `CA1875`. That block is Session 7's sized worklist, and several entries overlap findings this audit
  produced by hand.
- **The build is reproducible**: SDK pinned by `global.json`, package graph pinned by Central Package
  Management plus five committed `packages.lock.json`, sources pinned by a root `nuget.config`.

One entry resolved differently from its record's suggested direction: `MASTER-043` moved EF Core to
`10.0.11`, which resolves `SQLitePCLRaw.lib.e_sqlite3` at 2.1.12 on its own, so the hand-rolled
security pin for GHSA-2m69-gcr7-jv3q was **deleted** rather than kept -- verified across Infrastructure
and both App target frameworks. `MASTER-104` was **not** resolved by moving to a stable release,
because none exists; see `.dir-sdlc/backlog.md`.

**What Session 4 changed for the sessions after it.** `src/DungeonMaster.Tests/App/AsyncDiscipline/` now encodes four rules as source-scanning guards over `src/DungeonMaster.App`, so the suite goes red on a reintroduced defect rather than waiting for a reader to notice it: **G1**, an `async void` unit with no `try` above its first `await`; **G2**, a discarded task -- a line whose trimmed form starts with `_ = `; **G3**, an `async void` that is not an event handler; and **G4**, a sync-over-async call (`.Wait()`, `.GetAwaiter().GetResult()`). Four rules, not eight: **G5 through G8 were withdrawn at Gate 2** along with the four rescheduled entries and were never written, and those identifiers are retired rather than renumbered, so this file, the session's contracts and its task reports all keep naming the same things.

Two limits on what those guards prove, both worth knowing before trusting them. **They read source text and never execute App code**, because `XCUT-002` / `MASTER-026` is still open -- they are evidence about the shape of the source, not about a leak, a race or a crash path at runtime. And G2's rule is a token match on a line-initial `_ = `, so a discard written `_=Foo();` or mid-line as `if (x) _ = Foo();` is invisible to it; no such line exists in the layer today. Separately, `src/DungeonMaster.Tests/Infrastructure/Async/ConfigureAwaitDisciplineTests.cs` is the **only** enforcement of the `ConfigureAwait(false)` convention `MASTER-153` established -- `CA2007` is off by default and no build failure catches a regression -- so deleting that test silently ends the convention.

## Per-layer verdicts

**CORE (Domain and Application)** -- The strongest layer structurally and the weakest at its seams.
Twenty Domain files with zero references and zero conceptual leakage is genuinely rare, and the
Application layer's thirty-odd command triplets are consistent enough that their shared skeleton has
become duplication rather than convention. But every failure path in the layer loses information:
mutators return `void`, one path throws where its siblings return `Result`, one miscategorises, and
`ErrorCategory` -- the type that exists to carry the distinction -- has no production consumer at
all. Fixing the aggregate's return types is the single change that unlocks most of the rest.

**INFRA (Infrastructure)** -- Correct in the hardest place and wrong about its own database. Backing
fields and `PropertyAccessMode` preserve the aggregate's encapsulation through persistence, which is
the thing most easily got wrong here and is right; the value objects cross cleanly as an owned type,
two string conversions and one JSON column. What the layer does not do is fit its provider:
`DateTimeOffset` on SQLite, eleven unenforced `HasMaxLength` calls, a store default that collides
with a CLR default, and a migration history squashed onto a baseline no database has applied. Its
20 records are the fewest of any layer and the highest proportion at S2.

**UI (App shell, views and view models)** -- Holds its shape at the edges and loses it in the
middle. `AppShell`, the list page, the create popup and all four view models' CommunityToolkit.Mvvm
usage are idiomatic, with no hand-rolled `INotifyPropertyChanged` and no generator misuse anywhere.
The middle is one 397-line view model with four responsibilities that discards every `Result`, and
one 2,226-line code-behind holding a state machine that has no home. Async discipline is the other
systemic weakness: three non-handler `async void` methods, nine unguarded handlers, eight discarded
tasks. This layer carries the session's only S1 and the largest record count.

**UIKIT (Controls, converters, behaviors and theme)** -- A sound, well-governed theme layer carrying
a control kit that is not yet reusable. The three-tier dictionary arrangement and the on-disk
hygiene facts that enforce it are the best-engineered thing in the repository, and `DmPopup` is the
best-designed control. The kit weakens in one consistent direction: state that should be declarative
is imperative, painted from `propertyChanged` handlers and private fields the theme cannot reach.
Three of ten control types name `AreaDto` and `FloorDto` directly, so the kit's contract is one
query's projection rather than its own. `FloorMapDrawable` is the weakest file in the layer.

**TEST (Test suite)** -- Sound in Domain and Application, structurally wrong in `App/`. The Domain
tests drive behaviour through the aggregate root and survive refactoring of entity internals; the
handler tests follow `dotnet-format.md` section 12 exactly; NSubstitute use is disciplined rather
than over-mocked, and the suite is safe under xUnit's default parallelism. `App/` is a linter
wearing a test's clothes: thirteen files asserting over XAML and C# as text because the project
cannot reference App. The guards themselves are excellent and have caught real shipped defects. The
mechanism is what needs replacing, and `MASTER-026` says why it cannot simply be a project reference.

**XCUT (Solution, build configuration and boundaries)** -- Deliberate where touched, untended
everywhere else. `Directory.Build.props` sets a demanding baseline the codebase meets at zero
warnings, and `DungeonMaster.Domain.csproj` at nine lines with no references is the cleanest file
here. But there is no `.editorconfig`, no `global.json`, no central package management, no
`nuget.config` and no lock file, so nothing is pinned: not the SDK, not the analyzer set, not the
package graph, not the indent character. The boundaries themselves are better than the
configuration -- weak on surface area and lifetime, never on direction.

## Boundaries

**Domain to Application.** Sound, and the only boundary with no record against it. The direction is
right and the surface is right: Application references Domain, Domain references nothing, and the
types crossing are entities, value objects and enums with no framework or persistence vocabulary
among them. T1 read all twenty Domain files looking for a leak and found four private parameterless
constructors as the sole ORM accommodation, none of which names persistence. The one thing worth
watching is that the surface is wide by default -- every Domain public member is visible to
Application -- but nothing has abused it.

**Application to Infrastructure.** Correct direction, inverted properly, with no address. Two
interfaces, `IDungeonReadRepository` and `IDungeonWriteRepository`, are declared in Application and
implemented in Infrastructure, so the arrow points inward as it should. The types crossing are
Domain aggregates in the write direction and Application DTOs in the read direction, both
appropriate; no `IQueryable` escapes, and both repositories return materialised results. The defect
is that the ports live inside `Features/Dungeons/` rather than at a port surface (`MASTER-048`), so
a persistence-driven change like `INFRA-014`'s unit-of-work fix edits a file sitting inside a feature
folder, and a second feature needing persistence has nowhere obvious to put its port.

**Application and Infrastructure to App.** The weakest of the four, and weak in two independent ways.
The lifetime contract does not exist: Infrastructure registers scoped, App registers transient and
opens no scope, and the two files never refer to each other (`MASTER-023`). The surface area is
undefined: App consumes Application through the dispatcher as intended, through the query DTOs as a
binding contract for reusable controls, and through `Common/Map` as a utility library whose three
types have no caller inside Application at all (`MASTER-087`, `MASTER-082`, `MASTER-027`). The
direction is correct at every one of those sites -- this is a surface-area finding, not a dependency
one -- but with nothing stating what App may touch, the seam is whatever happens to be `public`.

**Tests into each.** Correct and complete for three layers, structurally impossible for the fourth.
`DungeonMaster.Tests` references Domain, Application and Infrastructure and exercises all three
through their real public surfaces. It does not reference `DungeonMaster.App`, and `MASTER-026`
records why that is not an omission that can be corrected by adding a `ProjectReference`: the test
project targets `net10.0` while the App targets `net10.0-windows10.0.19041.0` and
`net10.0-maccatalyst`, which share no framework with it. Everything downstream -- the regex-over-source
sub-suite, the unreachable geometry helpers, the lexical assertions -- follows from that pair of
target frameworks rather than from any decision a test author made.

## Source documents

| Document | Layer | S1 | S2 | S3 | S4 | S5 | Total |
|---|---|---|---|---|---|---|---|
| `docs/audit/01-domain-application.md` | CORE | 0 | 5 | 16 | 9 | 3 | 33 |
| `docs/audit/02-infrastructure.md` | INFRA | 0 | 6 | 7 | 3 | 4 | 20 |
| `docs/audit/03-app-shell-and-views.md` | UI | 1 | 8 | 16 | 9 | 2 | 36 |
| `docs/audit/04-app-controls-and-theme.md` | UIKIT | 0 | 5 | 15 | 6 | 3 | 29 |
| `docs/audit/05-tests.md` | TEST | 0 | 4 | 17 | 9 | 2 | 32 |
| `docs/audit/06-cross-cutting.md` | XCUT | 0 | 4 | 8 | 4 | 1 | 17 |
| **All** | | 1 | 32 | 79 | 40 | 15 | 167 |

## Master index

| Rank | ID | Sev | Layers | Category | Effort | Risk | Title | Sources | Status |
|---|---|---|---|---|---|---|---|---|---|
| 1 | MASTER-001 | S1 | UI | correctness | M | low | DungeonDetailViewModel discards the Result of every dispatched command | UI-004 | **done** |
| 2 | MASTER-002 | S2 | CORE | async | XS | low | ValidationBehavior validates synchronously and never passes the cancellation token | CORE-003 | **done** |
| 3 | MASTER-003 | S2 | CORE | structure | XS | low | Floor.UpdateNumber is the only entity mutator that does not call Touch | CORE-021 | **done** |
| 4 | MASTER-004 | S2 | INFRA | performance | XS | low | LoadAsync includes two sibling collections, producing a cartesian product per floor | INFRA-011 | **done** |
| 5 | MASTER-005 | S2 | UI | correctness | XS | low | The object palette swallows every exception from its WinUI theming block | UI-022 | **done** |
| 6 | MASTER-006 | S2 | UIKIT | disposal | XS | low | HoverOrLongPressBehavior never detaches the pan recogniser it adds | UIKIT-003 | rescheduled to Session 6 |
| 7 | MASTER-007 | S2 | UIKIT | correctness | XS | low | NotNullConverter reports an unresolved binding as a non-null value | UIKIT-004 | |
| 8 | MASTER-008 | S2 | TEST | correctness | XS | low | RepoPaths accepts any ancestor with a .git entry without checking the App project is beneath it | TEST-003 | |
| 9 | MASTER-009 | S2 | XCUT | dependency | XS | low | No global.json and no AnalysisLevel pin, so the analyzer set floats under TreatWarningsAsErrors | XCUT-003 | **done** |
| 10 | MASTER-010 | S2 | CORE | correctness | S | low | AddArea throws InvalidOperationException for a missing floor while siblings return a Result | CORE-002 | **done** |
| 11 | MASTER-011 | S2 | INFRA | async | S | low | MigrateDatabase applies migrations synchronously on the startup path and takes no cancellation token | INFRA-019 | **done** |
| 12 | MASTER-012 | S2 | UI | correctness | S | low | Database directory creation and migration run synchronously and unguarded inside CreateMauiApp | UI-002 | **done** |
| 13 | MASTER-013 | S2 | UIKIT | disposal | S | low | FloorMapDrawable caches IDisposable images and never disposes them | UIKIT-001 | part done -- rescheduled to Session 6 |
| 14 | MASTER-014 | S2 | TEST | correctness | S | low | Every persistence test builds its schema with EnsureCreated, so no test exercises a migration | TEST-002 | |
| 15 | MASTER-015 | S2 | INFRA | correctness | S | medium | Outline JSON conversion has no ValueComparer, so every tracked Area is reported as modified | INFRA-004 | **done** |
| 16 | MASTER-016 | S2 | INFRA | correctness | S | medium | UpdateAsync attaches a detached aggregate, which marks it Unchanged and saves nothing | INFRA-012 | **done** |
| 17 | MASTER-017 | S2 | UI | async | S | medium | ApplyQueryAttributes starts the load as a discarded task and silently ignores a bad id | UI-005 | **done** |
| 18 | MASTER-018 | S2 | UI | async | S | medium | async void on three methods that are not event handlers | UI-013 | **done** |
| 19 | MASTER-019 | S2 | UI | async | M | low | async void event handlers across the layer carry no exception guard | UI-014 | **done** |
| 20 | MASTER-020 | S2 | TEST | testability | M | low | The dispatcher, the validation pipeline and the Result type have no tests at all | TEST-001 | |
| 21 | MASTER-021 | S2 | CORE | correctness | M | medium | Aggregate mutators silently no-op on an unknown child id and the command still reports success | CORE-001 | **done** |
| 22 | MASTER-022 | S2 | CORE | naming | M | medium | Entity namespaces share the name of the type they contain, forcing Floor.Floor qualification | CORE-004 | |
| 23 | MASTER-023 | S2 | INFRA, UI, XCUT | dependency | M | medium | Agree one DbContext and repository lifetime across the Infrastructure and App composition roots | INFRA-018, UI-001, XCUT-001 | **done** |
| 24 | MASTER-024 | S2 | UI | async | M | medium | Discarded tasks in the keyboard, gesture, and popup dispatch paths | UI-015 | **done** |
| 25 | MASTER-025 | S2 | UIKIT | async | M | medium | Image loading blocks on async file I/O from inside the Draw call path | UIKIT-002 | **done** |
| 26 | MASTER-026 | S2 | XCUT | boundary | M | medium | Tests targets net10.0 and App targets platform frameworks, so no test can reference App | XCUT-002 | **done** (5a) |
| 27 | MASTER-027 | S2 | UIKIT | boundary | L | medium | The reusable control kit binds directly to Application query DTOs | UIKIT-005 | **done** (5a) |
| 28 | MASTER-028 | S2 | TEST | correctness | L | medium | Eleven of the thirteen App files assert over XAML as text; a twelfth parses it as XML | TEST-004 | |
| 29 | MASTER-029 | S2 | INFRA | performance | L | high | Every timestamp column is DateTimeOffset, a type the SQLite provider does not natively support | INFRA-003 | **done** |
| 30 | MASTER-030 | S2 | UI, XCUT | correctness | L | high | Resolve the macCatalyst target framework: implement it fully or drop it from the solution | UI-021, UI-036, XCUT-004 | **done** (5a) -- parked, see ADR-001 |
| 31 | MASTER-031 | S3 | CORE | api-design | XS | low | AddPlacedObject reports a missing floor as a Validation failure with an opaque message | CORE-014 | **done** |
| 32 | MASTER-032 | S3 | INFRA | dead-code | XS | low | AppDbContext exposes Floors and Areas DbSets that nothing in the solution reads | INFRA-001 | **done** |
| 33 | MASTER-033 | S3 | INFRA | async | XS | low | AddAsync is used where the API documentation prescribes the synchronous Add | INFRA-013 | **done** |
| 34 | MASTER-034 | S3 | UIKIT | api-design | XS | low | One-way converters throw from ConvertBack where the guidance is to return null | UIKIT-006 | |
| 35 | MASTER-035 | S3 | UIKIT | duplication | XS | low | DrawAreasViewMode and DrawAreasEditMode differ only in the alpha rule | UIKIT-010 | |
| 36 | MASTER-036 | S3 | UIKIT | duplication | XS | low | The 44-unit touch-target minimum is a bare literal repeated ten times | UIKIT-014 | |
| 37 | MASTER-037 | S3 | UIKIT | api-design | XS | low | DmTheme.Gold and DmTheme.ScrollGoldLight resolve the identical token key | UIKIT-015 | |
| 38 | MASTER-038 | S3 | UIKIT | async | XS | low | DmConfirmPopup discards the CloseAsync task without observing it | UIKIT-018 | **done** |
| 39 | MASTER-039 | S3 | UIKIT | performance | XS | low | HandCursorBehavior reflects for ProtectedCursor on every pointer event | UIKIT-020 | **done** |
| 40 | MASTER-040 | S3 | TEST | correctness | XS | low | The accessibility scan truncates supplementary-plane character entities with a char cast | TEST-006 | |
| 41 | MASTER-041 | S3 | TEST | testability | XS | low | Task-returning substitutes are given explicit Returns(Task.CompletedTask) stubs | TEST-011 | |
| 42 | MASTER-042 | S3 | TEST | dead-code | XS | low | AreaPersistenceDiagnosticTests is a committed debugging session with two subsumed tests | TEST-012 | |
| 43 | MASTER-043 | S3 | XCUT | dependency | XS | low | EF Core is pinned three patch releases behind a version the pin comment shows was evaluated | XCUT-016 | **done** |
| 44 | MASTER-044 | S3 | CORE | api-design | S | low | Result<T>.Value is T? on an unconstrained T, so null on failure is unreachable for value types | CORE-005 | |
| 45 | MASTER-045 | S3 | CORE | api-design | S | low | Two ErrorCategory members are never produced and no production caller reads the category | CORE-007 | **done** |
| 46 | MASTER-046 | S3 | CORE | api-design | S | low | ValidationBehavior discards every validation failure except the first | CORE-008 | **done** |
| 47 | MASTER-047 | S3 | CORE | dead-code | S | low | Five Domain overloads have no production caller and are reachable only from the test assembly | CORE-019 | |
| 48 | MASTER-048 | S3 | CORE, XCUT | boundary | S | low | Move the two persistence port interfaces to Application/Common/Interfaces | CORE-029, XCUT-006 | **done** |
| 49 | MASTER-049 | S3 | INFRA | performance | S | low | The detail projection materialises two sibling collections per floor in one query | INFRA-010 | **done** |
| 50 | MASTER-050 | S3 | INFRA | correctness | S | low | The design-time factory hard-codes a relative database path that differs from the runtime one | INFRA-016 | **done** |
| 51 | MASTER-051 | S3 | UI | duplication | S | low | The Entry and Editor handler mappers repeat the same eight WinUI resource assignments verbatim | UI-003 | |
| 52 | MASTER-052 | S3 | UI | api-design | S | low | Five public async methods sit outside the command surface, three re-taking an id the type holds | UI-008 | |
| 53 | MASTER-053 | S3 | UI | testability | S | low | The view model navigates through the static Shell.Current | UI-009 | |
| 54 | MASTER-054 | S3 | UI | testability | S | low | DungeonListPage injects IServiceProvider and resolves the popup from it | UI-011 | |
| 55 | MASTER-055 | S3 | UI | disposal | S | low | The Windows scroll-zoom unsubscribe re-resolves a handler that may already be gone | UI-018 | rescheduled to Session 6 |
| 56 | MASTER-056 | S3 | UI | structure | S | low | A user32 P/Invoke sits directly in shared page code-behind | UI-020 | |
| 57 | MASTER-057 | S3 | UI | duplication | S | low | The two palettes build the same 48x48 chip with duplicated construction code | UI-023 | |
| 58 | MASTER-058 | S3 | UI | correctness | S | low | Back navigation from the detail page depends entirely on a platform-supplied affordance | UI-033 | |
| 59 | MASTER-059 | S3 | UIKIT | api-design | S | low | FloorMapDrawable exposes concrete mutable collections as public API | UIKIT-008 | |
| 60 | MASTER-060 | S3 | UIKIT | structure | S | low | DmSearchBar paints its focus state imperatively rather than through visual states | UIKIT-013 | |
| 61 | MASTER-061 | S3 | UIKIT | correctness | S | low | NotebookView resolves a theme colour through a raw indexer and a null-forgiving operator | UIKIT-016 | |
| 62 | MASTER-062 | S3 | UIKIT | threading | S | low | Long-press state is written on the UI thread and read on a timer thread with no synchronisation | UIKIT-017 | rescheduled to Session 6 |
| 63 | MASTER-063 | S3 | TEST | performance | S | low | Compiled Regex objects are constructed inside per-item loops rather than reused | TEST-005 | |
| 64 | MASTER-064 | S3 | TEST | duplication | S | low | The four validator test classes are identical except for the command type | TEST-007 | |
| 65 | MASTER-065 | S3 | TEST | duplication | S | low | The SQLite in-memory fixture is copy-pasted verbatim into four Infrastructure test classes | TEST-009 | |
| 66 | MASTER-066 | S3 | TEST | testability | S | low | Four tests assert Received(1) on a save that happens on a path where nothing changed | TEST-010 | |
| 67 | MASTER-067 | S3 | TEST | duplication | S | low | Five geometry facts and five snap-zone facts differ only in their data | TEST-013 | |
| 68 | MASTER-068 | S3 | TEST | performance | S | low | Every test in the type-matching class recomputes the full disk scan from scratch | TEST-017 | |
| 69 | MASTER-069 | S3 | TEST | testability | S | low | The two query-handler tests assert only that the stubbed DTO came back | TEST-018 | |
| 70 | MASTER-070 | S3 | TEST | async | S | low | No test in the suite passes a cancellable or cancelled CancellationToken | TEST-019 | |
| 71 | MASTER-071 | S3 | XCUT | dependency | S | low | Package versions and the security pin are declared per project with no central management | XCUT-008 | **done** |
| 72 | MASTER-072 | S3 | XCUT | security | S | low | No nuget.config, no package source mapping and no lock file at the repository root | XCUT-009 | **done** |
| 73 | MASTER-073 | S3 | CORE | api-design | S | medium | ObjectKind has no zero value, so default(ObjectKind) silently means DoorWood | CORE-017 | |
| 74 | MASTER-074 | S3 | CORE | api-design | S | medium | Entity provides no identity-based equality, so two instances of the same entity compare unequal | CORE-018 | |
| 75 | MASTER-075 | S3 | INFRA | correctness | S | medium | Scale is configured with a store default, which makes the CLR default 0f unsendable | INFRA-008 | **done** |
| 76 | MASTER-076 | S3 | UI | api-design | S | medium | The create view model reports its outcome through two events that the popup converts to a result | UI-012 | |
| 77 | MASTER-077 | S3 | UIKIT | structure | S | medium | propertyChanged handlers imperatively write properties the XAML already binds | UIKIT-007 | |
| 78 | MASTER-078 | S3 | XCUT | dependency | S | medium | Analysis mode is left at Default and code-style rules are not enforced on build | XCUT-010 | **done** |
| 79 | MASTER-079 | S3 | CORE | structure | M | low | Ten of the fourteen commands have no validator, making ValidationBehavior a no-op for them | CORE-009 | **done** |
| 80 | MASTER-080 | S3 | CORE | structure | M | low | Queries bypass the pipeline entirely; IPipelineBehavior applies only to commands | CORE-012 | **done** |
| 81 | MASTER-081 | S3 | CORE | structure | M | low | The Domain has no policy layer and scale validation is inline and duplicated | CORE-016 | |
| 82 | MASTER-082 | S3 | CORE | structure | M | low | Common/Map holds three geometry types that no command, query or handler in the layer uses | CORE-020 | **done** (5a) |
| 83 | MASTER-083 | S3 | UI | performance | M | low | The create-dungeon popup's six bindings are reflection-based because it declares no x:DataType | UI-030 | |
| 84 | MASTER-084 | S3 | UI | accessibility | M | low | The map canvas and the floor sidebar have no accessibility affordances | UI-031 | |
| 85 | MASTER-085 | S3 | TEST | duplication | M | low | Sixteen handler test classes repeat an identical fixture constructor and an identical not-found test | TEST-008 | |
| 86 | MASTER-086 | S3 | TEST | testability | M | low | Domain coverage is happy paths and unknown-id no-ops, with no constructor or boundary cases | TEST-020 | |
| 87 | MASTER-087 | S3 | XCUT | boundary | M | low | The Application assembly's public surface is a dispatcher contract, a DTO set and a utility library | XCUT-005 | **done** (5a) |
| 88 | MASTER-088 | S3 | CORE | api-design | M | medium | Result<T> permits invalid states and every state-changing command returns an always-true bool | CORE-006 | **done** |
| 89 | MASTER-089 | S3 | CORE | structure | M | medium | Dispatcher is a service locator over a hand-maintained handler registration list | CORE-010 | **done** |
| 90 | MASTER-090 | S3 | CORE | performance | M | medium | Every dispatch performs reflection and dynamic binding with no caching of the closed handler type | CORE-011 | **done** |
| 91 | MASTER-091 | S3 | CORE | duplication | M | medium | Fourteen command handlers repeat one load / null-check / mutate / save skeleton | CORE-013 | **done** |
| 92 | MASTER-092 | S3 | INFRA | structure | M | medium | Each write method commits its own transaction, so the repository is also the unit of work | INFRA-014 | **done** |
| 93 | MASTER-093 | S3 | INFRA | correctness | M | medium | The migration history was squashed onto a new baseline identifier no database has applied | INFRA-020 | **done** |
| 94 | MASTER-094 | S3 | UI | performance | M | medium | Every mutation reloads the whole dungeon aggregate, and exactly one does not | UI-007 | |
| 95 | MASTER-095 | S3 | UI | testability | M | medium | Geometry and model transformation are performed in the page rather than in a testable type | UI-017 | |
| 96 | MASTER-096 | S3 | UI | correctness | M | medium | The canvas cursor is set through private reflection with the failure silently swallowed | UI-019 | |
| 97 | MASTER-097 | S3 | UIKIT | performance | M | medium | Draw allocates per area, per segment and per frame | UIKIT-009 | |
| 98 | MASTER-098 | S3 | UIKIT | api-design | M | medium | Sidebar items expose visual state through imperative methods, not bindable properties | UIKIT-011 | |
| 99 | MASTER-099 | S3 | UIKIT | structure | M | medium | FloorSidebarItem re-declares and hand-forwards five of AreaSidebarItem's events | UIKIT-012 | |
| 100 | MASTER-100 | S3 | TEST | structure | M | medium | MAUI property types and the conversion matrix are hand-transcribed into the test at a fixed date | TEST-014 | |
| 101 | MASTER-101 | S3 | TEST | structure | M | medium | A TrueType cmap parser lives in a test class and three of its four subtable branches are unexercised | TEST-015 | |
| 102 | MASTER-102 | S3 | TEST | testability | M | medium | Assertions live inside shared scan helpers, so failures are reported against the helper | TEST-016 | |
| 103 | MASTER-103 | S3 | XCUT | boundary | M | medium | Domain CLR type choices set Infrastructure's storage cost with no port between the two | XCUT-007 | **done** |
| 104 | MASTER-104 | S3 | XCUT | dependency | M | medium | A prerelease package is referenced by the only project that ships | XCUT-011 | **done** |
| 105 | MASTER-105 | S3 | UIKIT | structure | M | high | DmInlineEditor concentrates five responsibilities and hides a state change in ClearFocusGlow | UIKIT-019 | |
| 106 | MASTER-106 | S3 | UI | structure | L | medium | DungeonDetailViewModel carries four unrelated responsibilities in one 397-line type | UI-006 | |
| 107 | MASTER-107 | S3 | CORE | structure | L | high | Dungeon is a 453-line delegation facade that re-implements Floor's API rather than exposing it | CORE-015 | |
| 108 | MASTER-108 | S3 | UI | structure | L | high | The detail page's code-behind is 2,226 lines and owns the editor's entire interaction state machine | UI-016 | |
| 109 | MASTER-109 | S3 | TEST | testability | L | high | App facts assert against the lexical form of application C#, not its behaviour | TEST-021 | |
| 110 | MASTER-110 | S4 | CORE | structure | XS | low | Floor is split across three partial files where the conventions define a two-file split | CORE-022 | |
| 111 | MASTER-111 | S4 | CORE | naming | XS | low | RotateDirection.cs declares a type named RotationDirection | CORE-023 | |
| 112 | MASTER-112 | S4 | CORE | docs | XS | low | Two files indent XML documentation continuation lines with spaces instead of two tabs | CORE-026 | |
| 113 | MASTER-113 | S4 | CORE | style | XS | low | Twelve handlers carry a redundant using for their own enclosing namespace, and four do not | CORE-028 | |
| 114 | MASTER-114 | S4 | CORE | performance | XS | low | Every collection property allocates a fresh ReadOnlyCollection wrapper on each access | CORE-030 | |
| 115 | MASTER-115 | S4 | INFRA | style | XS | low | PlacedObjectConfiguration is formatted with spaces and inline lambdas unlike its three siblings | INFRA-006 | |
| 116 | MASTER-116 | S4 | INFRA | naming | XS | low | Single-letter lambda variables in the owned-type configuration block | INFRA-007 | |
| 117 | MASTER-117 | S4 | INFRA | naming | XS | low | The file DependencyInjection.cs does not match the type it contains | INFRA-017 | |
| 118 | MASTER-118 | S4 | UI | structure | XS | low | The detail route is a bare string literal repeated at its registration and its call site | UI-010 | |
| 119 | MASTER-119 | S4 | UI | docs | XS | low | MapTool's enum members use single-line summary comments | UI-025 | |
| 120 | MASTER-120 | S4 | UI | style | XS | low | Primary constructor parameters and base lists are inline on all three view models | UI-027 | |
| 121 | MASTER-121 | S4 | UIKIT | style | XS | low | Single-statement control flow omits braces at twelve sites | UIKIT-021 | |
| 122 | MASTER-122 | S4 | UIKIT | style | XS | low | FloorMapDrawable is space-indented, and four files indent doc-comment bodies with spaces | UIKIT-022 | |
| 123 | MASTER-123 | S4 | UIKIT | structure | XS | low | Four public types are declared in files named for a different type | UIKIT-023 | |
| 124 | MASTER-124 | S4 | UIKIT | style | XS | low | Blank-line and trailing-line formatting deviations | UIKIT-025 | |
| 125 | MASTER-125 | S4 | UIKIT | dead-code | XS | low | FloorMapDrawable declares a redundant empty public constructor | UIKIT-026 | |
| 126 | MASTER-126 | S4 | TEST | naming | XS | low | Single-letter lambda parameters appear in thirteen places | TEST-023 | |
| 127 | MASTER-127 | S4 | TEST | disposal | XS | low | Four non-sealed test classes implement IDisposable with no Dispose(bool) overload | TEST-026 | |
| 128 | MASTER-128 | S4 | TEST | style | XS | low | Assertions appear inside Arrange blocks | TEST-030 | |
| 129 | MASTER-129 | S4 | XCUT | duplication | XS | low | Directory.Build.props holds four properties that the five project files then re-declare | XCUT-013 | **done** |
| 130 | MASTER-130 | S4 | CORE | naming | S | low | Abbreviated identifiers ct, obj and CW90 are used throughout both layers | CORE-024 | |
| 131 | MASTER-131 | S4 | CORE | docs | S | low | Six public Domain types carry no XML documentation | CORE-025 | |
| 132 | MASTER-132 | S4 | CORE, UI, TEST, XCUT | style | S | low | Adopt an .editorconfig fixing tabs as the indent character and reformat the space-indented files | CORE-027, UI-029, TEST-024, XCUT-012 | **done** |
| 133 | MASTER-133 | S4 | UI | docs | S | low | Three view types carry no XML documentation on their public members | UI-024 | |
| 134 | MASTER-134 | S4 | UI | style | S | low | A public record and a private enum are nested inside larger types | UI-026 | |
| 135 | MASTER-135 | S4 | UI | style | S | low | Braces are omitted on single-statement control flow in roughly twenty places | UI-028 | |
| 136 | MASTER-136 | S4 | UIKIT | docs | S | low | Public types, members and parameters across the behaviors lack XML documentation | UIKIT-024 | |
| 137 | MASTER-137 | S4 | TEST | style | S | low | Arrange/Act/Assert structure is used in one sub-tree and absent from three | TEST-022 | |
| 138 | MASTER-138 | S4 | TEST | duplication | S | low | Comment-stripping regexes and a line-number helper are re-declared instead of shared | TEST-028 | |
| 139 | MASTER-139 | S4 | TEST | style | S | low | Several tests carry two or more independent reasons to fail | TEST-029 | |
| 140 | MASTER-140 | S4 | XCUT | structure | S | low | The solution declares x64 and x86 platforms that every project maps back to Any CPU | XCUT-014 | **done** |
| 141 | MASTER-141 | S4 | UI | style | S | medium | Layout dimensions are written as literals in markup that has tokens for everything else | UI-032 | |
| 142 | MASTER-142 | S4 | UI, XCUT | structure | S | medium | Settle the app's shipping identity in the Windows manifest and the project file together | UI-034, XCUT-015 | |
| 143 | MASTER-143 | S4 | TEST | docs | M | low | XML documentation is present on 21 of 45 files and absent from 24 | TEST-025 | |
| 144 | MASTER-144 | S4 | TEST | naming | M | low | Two incompatible test-naming schemes are in use | TEST-027 | |
| 145 | MASTER-145 | S5 | CORE | async | XS | low | Both query handlers wrap a single passthrough call in an async state machine | CORE-032 | |
| 146 | MASTER-146 | S5 | CORE | boundary | XS | low | The only persistence-shaped concession in the Domain is the private parameterless constructors | CORE-033 | |
| 147 | MASTER-147 | S5 | INFRA | performance | XS | low | AsNoTracking on the summary query has no effect because the projection contains no entity types | INFRA-009 | |
| 148 | MASTER-148 | S5 | UIKIT | correctness | XS | low | BookmarkTabShape's notch clamp permits the degenerate case its comment describes | UIKIT-029 | |
| 149 | MASTER-149 | S5 | TEST | dead-code | XS | low | One theme ratchet is structurally incapable of failing in its current state | TEST-031 | |
| 150 | MASTER-150 | S5 | XCUT | style | XS | low | Build files disagree on byte-order mark and the repository has no .gitattributes | XCUT-017 | **done** |
| 151 | MASTER-151 | S5 | UIKIT | duplication | XS | high | DmConfirmButton restates the full visual-state group its base already defines | UIKIT-028 | |
| 152 | MASTER-152 | S5 | INFRA | correctness | S | low | HasMaxLength is configured on eleven string columns but SQLite enforces none of them | INFRA-002 | |
| 153 | MASTER-153 | S5 | INFRA | async | S | low | No ConfigureAwait(false) anywhere in the layer, in a host that has a UI SynchronizationContext | INFRA-015 | **done** |
| 154 | MASTER-154 | S5 | UI | dead-code | S | low | The Start Session button is present, prominent, and unwired in two views | UI-035 | |
| 155 | MASTER-155 | S5 | TEST | dependency | S | low | coverlet is referenced but there is no runsettings, no collection configuration and no CI invocation | TEST-032 | |
| 156 | MASTER-156 | S5 | INFRA | correctness | S | high | The (DungeonId, Number) index is non-unique, so floor numbering has no database guarantee | INFRA-005 | |
| 157 | MASTER-157 | S5 | CORE | structure | L | high | The hand-rolled dispatcher is 233 lines replacing a mediator library and earns its keep | CORE-031 | |
| 158 | MASTER-158 | S5 | UIKIT | api-design | L | high | DmConfirmPopup and NotebookView expose no bindable surface at all | UIKIT-027 | |
| 159 | MASTER-159 | S2 | UIKIT, UI | correctness | XS | low | DmCard never assigns its trailing presenter's content, so a list card's Edit and Delete buttons never render | Session 4 | |
| 160 | MASTER-160 | S3 | UIKIT | disposal | XS | low | PreloadObjectImagesAsync can overwrite a decoded image with null, undisposed, if the stream throws on close | Session 4 | |
| 161 | MASTER-161 | S3 | UIKIT | accessibility | XS | low | FloorSidebarItem's card body is tappable but carries no HandCursorBehavior | Session 4 | |

**Three entries added after the audit.** `MASTER-159`, `MASTER-160` and `MASTER-161` were found during Session 4 rather than by the 2026-08-17 audit, so they cite that session instead of a source record, and they are appended at ranks 159-161 rather than sorted into the severity ranking above -- renumbering 158 existing ranks would break every rank this file has ever been cited by. None of the three is scheduled; each is described, with a suggested home, under `## Suggested fix sessions`.

## Merged groups

- **MASTER-023** (`INFRA-018`, `UI-001`, `XCUT-001`) -- Agree one DbContext and repository lifetime across the Infrastructure and App composition roots
- **MASTER-030** (`UI-021`, `UI-036`, `XCUT-004`) -- Resolve the macCatalyst target framework: implement it fully or drop it from the solution
- **MASTER-048** (`CORE-029`, `XCUT-006`) -- Move the two persistence port interfaces to Application/Common/Interfaces
- **MASTER-132** (`CORE-027`, `UI-029`, `TEST-024`, `XCUT-012`) -- Adopt an .editorconfig fixing tabs as the indent character and reformat the space-indented files
- **MASTER-142** (`UI-034`, `XCUT-015`) -- Settle the app's shipping identity in the Windows manifest and the project file together

Three merges that were considered and rejected, because the test is the fix and not the family:

- **The silent-success trio** (`CORE-001`, `INFRA-012`, `UI-004`) is the closest call in this index
  and does not collapse. The three produce one user-visible behaviour -- an edit that appears to
  succeed and did not -- by three unrelated mechanisms in three assemblies, and no single change
  resolves more than one of them. Giving the Domain mutators return values does not make the view
  model read the `Result` it is already being handed; making the view model read it does not stop a
  mutator no-opping; and neither touches `Attach` marking a detached graph `Unchanged`. They are
  ordered rather than merged: `CORE-001` produces the signal and `UI-004` consumes it, so both sit
  in Session 2 with `INFRA-012`.
- **The lambda-naming and brace records** (`INFRA-007` with `TEST-023`; `UI-028` with `UIKIT-021`)
  each pair cites the same `dotnet-format.md` rule, but the fix is a separate hand edit in a
  separate project with no shared decision behind it. Same rule is not same fix. This is the
  distinction that justifies merging `MASTER-132`, where the four sources genuinely cannot be
  resolved independently: none of them can be fixed at all until the repository decides which
  indent character it uses, and once that one decision is made a single format pass resolves all
  four.
- **`CORE-020` with `UI-017`** -- map geometry with no caller in Application, and map geometry
  trapped in a page -- are the same question but not provably the same fix. Consolidating into
  `Common/Map` resolves both; consolidating into the App project resolves `CORE-020` and leaves
  `UI-017` untestable, because of `MASTER-026`. Since one resolution of the decision fails to close
  both records, they stay separate and share Session 5.

Two notes on effective severity, since the source records are frozen and this index does not
re-litigate them. `MASTER-030` merges `UI-036` (S5, a launch profile listing only Windows) into an
S2 entry, because under the shared macCatalyst decision that observation is not optional -- it is
part of what "implement it fully" means. `MASTER-048` merges `CORE-029` (S4, a file-layout
deviation) into an S3 boundary entry, because the same move that satisfies the layout convention is
the one that gives the persistence port an address.

## Suggested fix sessions

### Session 1 -- Pin the build -- COMPLETE (2026-08-18)
- **Entries:** MASTER-009, MASTER-043, MASTER-071, MASTER-072, MASTER-078, MASTER-104, MASTER-129, MASTER-132, MASTER-140, MASTER-150
- **Combined effort:** ~1.2 days
- **Highest risk:** medium
- **Rationale:** Every entry here changes build configuration and no line of `src/` behaviour, so it is the one session that can land with zero regression risk and it goes first for that reason. It is also enabling work: `MASTER-132` decides the indent character that Session 7 depends on, and `MASTER-078` turns on the enforcement that keeps Session 7 from having to happen again.
- **Outcome:** All ten entries landed. 26 of 26 acceptance criteria met across three tasks, one revision round, no unresolved blocking findings. Build clean at 0 warnings for both App target frameworks under `TreatWarningsAsErrors`; 292 of 292 tests passing. The `no src/ behaviour change` property held -- the only `src/` edits were whitespace, verified by comparing 1,181 string literals byte-for-byte. Session artifacts: `.dir-sdlc/sessions/2026-08-17-pin-the-build/`.

### Session 2 -- Make failure observable in the Application core -- COMPLETE (2026-08-18)
- **Entries:** MASTER-001, MASTER-002, MASTER-003, MASTER-005, MASTER-010, MASTER-016, MASTER-021, MASTER-031, MASTER-045, MASTER-046, MASTER-079, MASTER-080, MASTER-088, MASTER-089, MASTER-090, MASTER-091
- **Combined effort:** ~4.6 days
- **Highest risk:** medium
- **Rationale:** The solution's one S1 sits here alongside the Domain and Application changes that make it meaningful. `MASTER-021` gives the aggregate mutators something to report and `MASTER-001` makes the view model read it; neither is worth doing without the other, which is why they share a session while remaining separate entries. The dispatcher and validation entries ride along because they touch the same files and the same failure path.
- **Outcome:** All sixteen entries landed across nine tasks. **83 of 83 acceptance criteria met**, one revision round, no unresolved blocking findings. Tests **292 -> 434**; build clean at 0 warnings for both App target frameworks under `TreatWarningsAsErrors`, verified on a `--no-incremental` full build. The three silent-success mechanisms are closed by three separate fixes exactly as this index predicted they would have to be: `MASTER-021` gave the Domain mutators a `MutationOutcome` to report, `MASTER-016` replaced `Attach` with `Update` so a detached aggregate persists, and `MASTER-001` made the detail view model surface every `Result` through the `ErrorMessage` channel the other two view models already used. `MASTER-088` was resolved as a genuinely **non-generic `Result` alongside `Result<T>`**, sibling records rather than a hierarchy because a base class cannot be `sealed` and `sealed` is the invariant that entry exists to establish -- which is the answer the five parked design questions (`MASTER-044`, `073`, `074`, `081`, `107`) were waiting on. `MASTER-045` kept the five-member `ErrorCategory` named by `backend-conventions.md` §2 and gave it a production consumer in `ResultMessage`. Two criteria needed a human at the keyboard rather than a test, because `DungeonMaster.Tests` cannot reference the multi-targeted App project (`MASTER-026`, Session 5): both were performed on Windows and observed. Session artifacts: `.dir-sdlc/sessions/2026-08-18-make-failure-observable/`.

### Session 3 -- Settle the data-access lifetime and the persistence contract -- COMPLETE (2026-08-18)
- **Entries:** MASTER-004, MASTER-011, MASTER-012, MASTER-015, MASTER-017, MASTER-023, MASTER-029, MASTER-032, MASTER-033, MASTER-048, MASTER-049, MASTER-050, MASTER-075, MASTER-092, MASTER-093, MASTER-103
- **Combined effort:** ~4.1 days
- **Highest risk:** high
- **Rationale:** One blast radius: `Persistence/`, the two repositories and the two composition roots. `MASTER-023` is the gate -- deciding the `DbContext` lifetime changes what `MASTER-015`'s spurious writes and `MASTER-092`'s transaction boundary even mean -- so it is taken first and the provider-fit findings follow behind it. This session carries the highest risk of the early ones and should not be attempted before Session 2 has stabilised the handlers above it.
- **Outcome:** All sixteen entries landed across three tasks. **48 of 48 acceptance criteria met**, **zero revision rounds**, no unresolved blocking findings. Tests **434 -> 448**; build clean at 0 warnings for both App target frameworks under `TreatWarningsAsErrors`, verified on a `--no-incremental` full build. `MASTER-023` resolved as **`IDbContextFactory<AppDbContext>`, one context per unit of work** -- chosen on performance grounds after a search round, because a root-scoped context accumulates every entity ever loaded and snapshot change tracking compares all of them at every `SaveChanges`. Per-*call* contexts were explicitly rejected: they would leave the aggregate detached at save and send `MASTER-016`'s `Update` down the path that marks the whole graph modified. That decision then settled `MASTER-092` as an explicit `IUnitOfWork.CommitAsync` -- a factory makes the unit of work explicit rather than ambient -- and `SaveChangesAsync` now appears at exactly one production site. `MASTER-029` was taken at the Domain rather than through a converter, and proved smaller than this index recorded: the timestamps are declared **once** on `Entity.cs:16,21` and inherited by all four entity types, so the Domain edit was three lines in one file, not eight properties across four. The stored-format question was settled by running the experiment rather than assuming a format -- the two forms differ only by a six-character `+00:00` suffix, and the reviewer independently confirmed the migrated value is byte-identical to what the `DateTime` model writes natively. Two entries closed partially by design, both declared in the contracts rather than discovered at review: `MASTER-012`'s cold-start half and `MASTER-093`'s startup-refusal and CI halves, the latter because no `.github/workflows` exists to attach to. See `.dir-sdlc/backlog.md`. Session artifacts: `.dir-sdlc/sessions/2026-08-18-settle-the-data-access-lifetime/`.

### Session 4 -- Async, disposal and threading discipline in the presentation layers -- COMPLETE (2026-08-25)
- **Entries:** MASTER-018, MASTER-019, MASTER-024, MASTER-025, MASTER-038, MASTER-039, MASTER-153
- **Combined effort:** ~2.2 days
- **Highest risk:** medium
- **Rationale:** A single discipline applied across three layers: no `async void` outside a guarded handler and no discarded task, plus the layer's `ConfigureAwait` decision. These are individually small, mechanically similar, and each one is a latent crash, so batching them is worth more than spreading them. It comes after Session 3 because a shorter-lived `DbContext` changes what an unobserved task can corrupt. The other half of this rationale as first written -- no undisposed `IDisposable`, no cross-thread field -- is **not** what the session did: `MASTER-006`, `MASTER-013`, `MASTER-055` and `MASTER-062` were rescheduled to Session 6 at Gate 2, so the disposal and threading half is now Session 6's.
- **Outcome:** Seven entries landed across six tasks: `MASTER-018`, `MASTER-019`, `MASTER-024`, `MASTER-025`, `MASTER-038`, `MASTER-039` and `MASTER-153`. Of the **61** acceptance criteria across those six tasks, 59 were met outright, one was met in part and one was not met; both exceptions are manual-observation criteria that no revision round could have closed, and the user accepted each at escalation. Tests **448 -> 469** (`Failed: 0, Passed: 469, Total: 469`), the 21 added being T1's 20 async-discipline guards and T6's one `ConfigureAwait` guard; build clean at 0 warnings for both App target frameworks -- `net10.0-windows10.0.19041.0` and `net10.0-maccatalyst` -- under `TreatWarningsAsErrors`. Three of the seven need qualifying -- `MASTER-019`, `MASTER-025` and `MASTER-039` -- and for two of those, `MASTER-019` and `MASTER-025`, a bare **done** would overstate what was seen. `MASTER-013`, which is one of the four rescheduled rather than one of the seven, needs a fourth. **`MASTER-019`'s four `DungeonListPage` handlers are guarded, and two of them were never seen running**: each opens a `try` above its first `await` and routes the failure to `_viewModel.ErrorMessage`, which rule G1 verifies over the source -- but only `OnCreateClicked` and `OnCardTapped` were observed executing in the running app. `OnEditClicked` and `OnDeleteClicked` never ran once, because the Edit and Delete buttons never materialize on a dungeon-list card: that is `MASTER-159` above, and until it is fixed there is no route to either handler for a human or for a UIA driver. **`MASTER-039` is the session's one criterion closed by a human at the keyboard**: an automated `SetCursorPos` / `GetCursorInfo` probe reported ARROW at every point on the changed *and* the unchanged build, including over a native WinUI `TextBox` whose I-beam the platform sets independently of any behavior, which isolated the apparatus rather than the change -- so the reviewer refused "met by baseline parity", and the user then hovered dungeon cards on Windows and reported the hand cursor working across multiple cards without degrading. That observation also settles the review's one unconfirmed risk, that a shared cached `InputSystemCursor` might be closed out from under other elements. **`MASTER-025` landed with one path unobserved**: the blocking file I/O is gone from the `Draw` call path, `LoadOrGetImage` is now a pure cache read and `PreloadObjectImagesAsync` is the only writer, so the artwork observed rendering in place of the `?` placeholder entails the preload ran -- but the **pack-change path was never seen working**, because `src/DungeonMaster.App/Assets/ObjectAssets/ObjectPackRegistry.cs:8-9` declares exactly one object pack, so there is nothing to switch to and re-selecting index 0 raises no event. That path is structurally correct and unexercised until a second pack exists. **`MASTER-013` is not closed, and half of it landed here**: `ClearObjectImageCache()` now disposes each cached image before clearing, purely as a no-regression measure, because the preload changes the cache from holding the kinds a floor happened to draw to holding up to one entry per declared `ObjectKind`, of which there are **37**; the page-teardown half -- `IDisposable`, `Dispose()`, and the call site -- remains Session 6's, and nothing here releases a handle at teardown. **The four rescheduled entries, and why.** `MASTER-006`, `MASTER-013`, `MASTER-055` and `MASTER-062` are three leaks and a memory-visibility race. No test in this repository executes App code -- `DungeonMaster.Tests` targets `net10.0` and cannot reference the multi-targeted App project (`XCUT-002` / `MASTER-026`) -- and none of the four is visible in a running app, so the only evidence available here was a structural assertion over source text, which the user judged insufficient for that defect class. They move to Session 6 because that is where `MASTER-026` lands and they can be demonstrated rather than asserted. Finally, three defects this session found that no audit entry recorded are filed above as `MASTER-159`, `MASTER-160` and `MASTER-161`. Session artifacts: `.dir-sdlc/sessions/2026-08-24-async-disposal-and-threading-discipline/`.

### Session 5 -- Give the editor a seam and the control kit its own contract
- **Entries:** MASTER-007, MASTER-026, MASTER-027, MASTER-030, MASTER-034, MASTER-035, MASTER-036, MASTER-037, MASTER-052, MASTER-053, MASTER-054, MASTER-056, MASTER-057, MASTER-058, MASTER-059, MASTER-060, MASTER-061, MASTER-076, MASTER-077, MASTER-082, MASTER-083, MASTER-084, MASTER-087, MASTER-094, MASTER-095, MASTER-096, MASTER-097, MASTER-098, MASTER-099, MASTER-105, MASTER-106, MASTER-108
- **Combined effort:** ~11.5 days
- **Highest risk:** high
- **Rationale:** The largest and least mechanical session: the editor's missing seam, the control kit's DTO coupling, and the boundary decisions those two force. `MASTER-026` is taken first because it constrains everything else -- where the extracted geometry can live determines whether it is testable at all -- and `MASTER-030`'s macCatalyst decision is here because dropping the target would delete a meaningful fraction of the remaining work.
- **Split into three sub-sessions at Gate 1 (2026-08-26).** At 32 entries and ~11.5 days this is twice the largest session that has run cleanly here, and it spans four independent subsystems, so it was decomposed rather than attempted whole: **5a** the boundary gates (`MASTER-026`, `MASTER-027`, `MASTER-030`, `MASTER-082`, `MASTER-087`), **5b** the editor seam, **5c** the control kit. 5a went first because its two decisions constrain the other two and Session 6.
- **5a outcome -- COMPLETE 2026-08-26.** All five entries landed across four tasks. **67 of 67 acceptance criteria met, zero revision rounds, zero re-dispatches**, three escalations all resolved, five user interactions. Tests **469 -> 506** (`Failed: 0, Passed: 506`, plus a new `Failed: 0, Passed: 1` App suite); solution and both App target frameworks clean at 0 warnings under `TreatWarningsAsErrors`, verified in the main checkout after the merge rather than only in the worktree. Change surface 32 files, +1,867 / -170.

  **`MASTER-026` was resolved as both shapes, not the audit's preferred one.** `XCUT-002` recommended extracting a `net10.0` library and treated multi-targeting the Tests project as the expensive alternative. The cheap shape alone reaches nothing MAUI-typed -- which would have left Session 6's four rescheduled entries (`MASTER-006`, `013`, `055`, `062`) and the user-facing `MASTER-159` with exactly the evidence problem that got them rescheduled out of Session 4. So both were built: `DungeonMaster.App.Core` (`net10.0`, referenced by App and by Tests) **and** `DungeonMaster.App.Tests`, single-targeted to `net10.0-windows10.0.19041.0`. **A test in this repository now executes App code**: `FloorMapDrawableExecutionTests` constructs a real `FloorMapDrawable` and calls `ScreenToCanvas(PointF)`, and the reviewer reproduced its failure under an injected wrong value before restoring it. Multi-targeting that project was measured impossible here -- `NU1201` against `net10.0-maccatalyst26.0`, then an empty `ResolveFrameworkReference.RuntimePackPath` -- and the deviation was approved at Gate 2. **Two limits stand**: the project runs outside a MAUI application host, so `MauiProgram.CreateMauiApp()` is never called, there is no `Application.Current`, and every control deriving from `ContentView`/`ContentPage`/`Shell` is still out of reach -- whether that half is reachable is Session 6's question; and a solution-wide `dotnet test` runs both suites but prints **two per-assembly result lines with no aggregate total**, so a script grepping for one `Failed:` line sees only the first.

  **`MASTER-030` was parked explicitly rather than implemented or dropped**, recorded as `ADR-001` in `docs/architecture-decisions.md`. The decision rests on a fact this index did not have: `net10.0-maccatalyst` **builds clean on this machine** -- `Build succeeded. 0 Warning(s). 0 Error(s).` with the `maccatalyst 26.5.10301` workload installed -- so `XCUT-004`'s claim that nothing here builds the Mac target is false, and `docs/superpowers/plans/2026-08-05-theme-systemisation.md:22` is stale. What is absent is *runtime*: nothing here can run a macCatalyst build, so "implement it fully" could produce no observed evidence for `UI-021`'s five input paths, and no criterion demanding one was written. The ADR records the target as build-verified and not run-verified, names the five `TODO(macOS)` paths as an accepted non-cosmetic gap, accepts `UI-036` and the missing solution platform unchanged, and states a revisit condition. It also unblocks the `Plugin.Maui.KeyListener` evaluation parked in `.dir-sdlc/backlog.md`, which was waiting on this decision.

  **`MASTER-082` and `MASTER-087` were taken together, and one conflict between two audit records was resolved.** `UIKIT-005` suggested moving the feet-per-tile arithmetic *into* Application beside the map helpers while `CORE-020` moves those helpers *out*; `MASTER-082` won -- the helpers left, and the arithmetic followed them as `TileDistance.InFeet`. `SnapZoneCalculator` and `MapDrawingSession.LastVertex` were **deleted rather than relocated** (a Gate 2 decision: zero production callers each), and `docs/app-application-seam.md` now states what the Application layer's public surface is. Two of this index's own records were falsified in passing: `XCUT-004`'s solution platform list is `Debug|Any CPU` and `Release|Any CPU` only, not `Any CPU`/`x64`/`x86`; and `UIKIT-005` cites the feet arithmetic at `FloorMapDrawable.cs:519-524`, which is at **553-558** after Session 4.

  **One falsified claim moves open work and is deliberately not fixed here.** `UIKIT-022` says `FloorMapDrawable.cs` has "429 space-indented lines and 0 tab-indented lines"; it is tab-indented -- 961 tabs, 459 tab-leading lines, **0** lines starting with four spaces -- and the claim is repeated at `:838`, `:870`, `:1126` and in `MASTER-122`'s row, which carries no done marker. The file-indentation half was almost certainly completed by `MASTER-132` in Session 1 without reconciling `MASTER-122`; the doc-comment half is real and unfixed (36 lines of `///` followed by five spaces). At Gate 3 the user chose to **re-measure the whole 19-file leading-whitespace census before touching the row**, since only this one file was checked. See `.dir-sdlc/backlog.md`. Session artifacts: `.dir-sdlc/sessions/2026-08-26-settle-the-app-boundaries/`.

### Session 6 -- Rebuild the App sub-suite and de-duplicate the fixtures
- **Entries:** MASTER-006, MASTER-008, MASTER-013, MASTER-014, MASTER-020, MASTER-028, MASTER-040, MASTER-041, MASTER-042, MASTER-055, MASTER-062, MASTER-063, MASTER-064, MASTER-065, MASTER-066, MASTER-067, MASTER-068, MASTER-069, MASTER-070, MASTER-085, MASTER-086, MASTER-100, MASTER-101, MASTER-102, MASTER-109, MASTER-127, MASTER-138, MASTER-149, MASTER-155
- **Combined effort:** ~6.6 days
- **Highest risk:** high
- **Rationale:** The whole test suite in one pass, after the code it tests has settled. Sequencing matters more here than anywhere: rebuilding the `App/` sub-suite before Session 5 has decided where App logic lives would rebuild it against a shape that is about to change. `MASTER-065` and `MASTER-014` are taken together because the shared fixture and the `EnsureCreated` schema are the same four files. It also now carries `MASTER-006`, `MASTER-013`, `MASTER-055` and `MASTER-062`, rescheduled here from Session 4 at Gate 2 -- see the Session 4 outcome above for why, and note that half of `MASTER-013` already landed there.

### Session 7 -- Style, naming and documentation sweep
- **Entries:** MASTER-022, MASTER-110, MASTER-111, MASTER-112, MASTER-113, MASTER-114, MASTER-115, MASTER-116, MASTER-117, MASTER-118, MASTER-119, MASTER-120, MASTER-121, MASTER-122, MASTER-123, MASTER-124, MASTER-125, MASTER-126, MASTER-128, MASTER-130, MASTER-131, MASTER-133, MASTER-134, MASTER-135, MASTER-136, MASTER-137, MASTER-139, MASTER-141, MASTER-142, MASTER-143, MASTER-144
- **Combined effort:** ~3.3 days
- **Highest risk:** medium
- **Rationale:** Pure style, naming and documentation, every entry `independent: true`, and it goes last for two reasons: reformatting files that six earlier sessions are still rewriting wastes the work, and by this point Session 1's `.editorconfig` and `EnforceCodeStyleInBuild` mean most of it can be verified by the build rather than by a reviewer.

Seventeen `S3` and `S5` entries are deliberately unassigned:

- **Five design questions that should be re-asked after Session 2 lands, not before**: `MASTER-044`
  (`Result<T>.Value` nullability), `MASTER-073` (`ObjectKind` has no zero value), `MASTER-074`
  (entity identity equality), `MASTER-081` (no policy layer) and `MASTER-107` (the `Dungeon`
  delegation facade). Each describes a shape that the `Result`-flow work in Session 2 will either
  justify or contradict -- `MASTER-088`, which is assigned, redesigns `Result<T>` itself, and the
  answer to that changes what these five should become. Deciding them now risks doing the work twice.
- **Two isolated cleanups with no natural companion**: `MASTER-047` (five Domain overloads reachable
  only from tests -- their fate depends on `MASTER-086`'s coverage work) and `MASTER-051` (the
  duplicated Entry/Editor handler mappers, which `MASTER-030` may delete outright if macCatalyst
  goes).
- **Ten S5 observations** -- `MASTER-145`, `MASTER-146`, `MASTER-147`, `MASTER-148`, `MASTER-151`,
  `MASTER-152`, `MASTER-154`, `MASTER-156`, `MASTER-157`, `MASTER-158`. By definition these deviate
  from no authority. `MASTER-157` in particular is an affirmation, not a defect: it records that the
  hand-rolled dispatcher earns its keep. They are listed so a maintainer can act on them, not
  scheduled.

The three entries added after the audit are also unassigned, and each carries a suggested home rather than a scheduled one. Scheduling them is a decision for whoever plans the next session.

- **`MASTER-159` -- `DmCard`'s trailing content never renders, so the Edit and Delete buttons are unreachable on every dungeon-list card.** `DmCard.OnHoverEntered` (`src/DungeonMaster.App/Controls/DmCard/DmCard.xaml.cs:101-105`) sets `TrailingPresenter.IsVisible = true`, but that presenter is a bare `ContentView` (`DmCard.xaml:52-55`) whose `Content` is only ever assigned from the `TrailingContent` property-changed callback at `DmCard.xaml.cs:46` -- which does not run for the `TrailingContent` set inside the list's `DataTemplate` (`DungeonListPage.xaml:49-141`), so the presenter renders empty. Hover fires -- the card lightens and `PointerOver` applies -- but neither button appears, in a 19-descendant UIA tree or in a full-window screenshot. **This is user-facing: a user cannot edit or delete a dungeon from the list.** Found by Session 4's T5 while trying to satisfy an observation criterion, and pre-existing -- `Controls/DmCard/` is unmodified against the session's base commit. Rated `S2` rather than `S1` because it removes two affordances from one control's trailing region and corrupts nothing; this index's sole `S1`, `MASTER-001`, was a silent failure on every write path of a view model. Suggested home: **Session 5**, which owns the control kit.
- **`MASTER-160` -- a narrow undisposed path inside `PreloadObjectImagesAsync`.** If `stream.Dispose()` throws after `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:76` has already assigned a decoded image, control reaches line 84 and overwrites that non-null entry with `null` without disposing it. This is conformant with the contract Session 4 froze -- a per-entry failure stores `null` -- and the window is vanishingly small, so it is not a defect in what that session built; it is simply real, and nothing else in the file has this shape. Fix shape: decode into a local, close the stream, then assign. Suggested home: **Session 6**, alongside `MASTER-013`, whose teardown disposal touches the same cache.
- **`MASTER-161` -- `FloorSidebarItem`'s card body is tappable but offers no cursor affordance.** The `VerticalStackLayout` filling the control's root `Border` carries `TapGestureRecognizer Tapped="OnCardTapped"` (`src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml:33-35`) and expanding the floor is the only route to its areas, but no `HandCursorBehavior` is attached to it; the control's three declarations of that behavior are all on child `Button`s -- notebook (`:86`), delete (`:106`) and add-area (`:158`) -- and the root `Border`'s own recognisers (`:164-172`) are pointer and drop only. The analogous `DmCard` self-attaches the behavior at `DmCard.xaml.cs:93`, so the two card types disagree. A clickable surface that does not look clickable. Suggested home: **Session 5**, with the rest of the control kit.

## Coverage attestation

- Every record ID in every source document appears in exactly one master entry: yes
- Source record count 167 reconciles to master entry count 158 with 9 merges: yes -- 167 - 9 = 158
- Every S1 and S2 master entry is assigned to a suggested fix session: yes -- all 30 of them
  (1 S1, 29 S2) appear in exactly one session
- No file under `src/` was created, modified, or deleted by this task: yes
- The four statements above are about the 158 entries the 2026-08-17 audit produced, and are left as that record. `MASTER-159`, `MASTER-160` and `MASTER-161` were added afterwards by Session 4, have no source record, and fall outside all four -- including the assignment statement: `MASTER-159` is an unassigned `S2`.
