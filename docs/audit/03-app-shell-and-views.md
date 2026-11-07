# App shell, views, and view models -- code quality audit

## Scope

| Path | Files | Lines |
|---|---|---|
| `src/DungeonMaster.App/` root files (`App.xaml`, `App.xaml.cs`, `AppShell.xaml`, `AppShell.xaml.cs`, `MauiProgram.cs`) | 5 | 243 |
| `src/DungeonMaster.App/Views/**` | 15 | 4,379 |
| `src/DungeonMaster.App/ViewModels/**` | 4 | 598 |
| `src/DungeonMaster.App/Extensions/**` | 1 | 58 |
| `src/DungeonMaster.App/Platforms/**` | 8 | 378 |
| `src/DungeonMaster.App/Properties/**` | 1 | 7 |
| **Total** | **34** | **5,663** |

**Files read: 34. Files cited in at least one record: 21.** `.xaml`, `.plist`, `.appxmanifest`, `.manifest`, and
`.json` files are counted and cited on the same terms as `.cs` files. The files-read count equals the total
file count in the table above.

Out of scope, and deliberately not audited:

- `Controls/`, `Converters/`, `Behaviors/`, `Graphics/`, `Theme/`, and `Resources/Theme/` are **out of scope
  because T4 owns them** (layer `UIKIT`). Types from those folders are referenced by name where an in-scope
  file consumes them, but no record judges their contents.
- `src/DungeonMaster.App/Assets/`, `Resources/` other than `Resources/Theme/`, and the `.csproj` are outside
  this task's `owns` set; the `.csproj` belongs to T6 (`XCUT`).
- No generated code exists in this scope as checked-in source. The XAML source generator
  (`MauiXamlInflator=SourceGen`) emits `InitializeComponent` and the compiled-binding partials into
  `obj/`; that output is **excluded**, because it is a build artefact and is not hand-written. The
  hand-written `.xaml` inputs that drive it **are** audited, by line, exactly as the `.cs` files are.

Read but cited in no record, and found sound. The 21 cited files plus the 13 listed here account for all
34 files in the table above:

- `App.xaml` -- seven merged theme dictionaries in a documented dependency order plus two converter
  instances; the dictionaries themselves are T4's.
- `App.xaml.cs` -- 39 lines, every public member XML-documented; its one `#if WINDOWS` call carries a
  six-line comment justifying why it must run after `InitializeComponent` and before the first window.
- `Platforms/Windows/App.xaml.cs` -- the WinUI system-brush overrides. Long, but every non-obvious choice
  is recorded in the file: the merge-not-replace `ThemeDictionaries` reasoning that UI-022 leans on, the
  measured evidence for which scrollbar keys actually paint, and the deliberate call site.
- `Extensions/NavigationExtensions.cs` -- one internal extension method, fully documented, correct
  parameter wrapping, no state.
- `Views/DungeonDetailPage/PaletteThumb.cs` -- a small internal static helper that correctly avoids eager
  `DmTheme` reads and correctly returns a fresh brush per call.
- `Views/DungeonDetailPage/ToolTileView.xaml` and `ToolTileView.xaml.cs` -- the cleanest control in the
  scope: three bindable properties, full XML documentation, decorative labels explicitly removed from the
  accessibility tree.
- `Platforms/Windows/App.xaml` -- an intentionally empty resource dictionary with the reason recorded in
  the file.
- `Platforms/Windows/app.manifest`, `Platforms/MacCatalyst/AppDelegate.cs`,
  `Platforms/MacCatalyst/Program.cs`, `Platforms/MacCatalyst/Info.plist`,
  `Platforms/MacCatalyst/Entitlements.plist` -- unmodified or minimally customised .NET MAUI templates with
  no defect visible on reading.

## Method

- **Base commit:** `45495c1bd39a52dbc942eceab897b3c1ac553ee9`, read in the worktree
  `dungeon-master-worktrees/2026-08-17-code-quality-audit`.
- **`dm-conventions` documents read:**
  - `backend/dotnet-format.md` -- 794 lines, **read in full, before any style, naming, formatting, or
    XML-documentation record in this document was written**. Every `S4` below names the section of that
    document it rests on, and no `S4` asserts a rule that document does not state.
  - No other `dm-conventions` document was needed for this layer. Its `frontend/` documents are
    React/TypeScript and are **not cited anywhere in this document**, because `dm-conventions` has no
    MAUI or XAML document. MAUI/XAML records therefore use `microsoft`, `practice`, or `judgment`.
- **Microsoft Learn pages consulted (each fetched, not recalled).** Four are cited by a record; the two
  marked *background* informed a record's reasoning without being its `citation`:
  - https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines
    -- cited by UI-001, UI-009, UI-011.
  - https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/
    -- *background*: informed UI-001 and UI-015. It is the source for the change-tracker and
    thread-safety consequences those two records describe; the records rest on the DI guidelines and on
    judgment respectively, so it is consulted rather than cited.
  - https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation
    -- *background*: informed UI-005 (`IQueryAttributable` as the trim-safe option) and UI-010 (the
    runtime-only failure mode of an unregistered route). Both records are `judgment` on their own
    assertions, so neither cites it.
  - https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/compiled-bindings
    -- cited by UI-030.
  - https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/accessibility
    -- cited by UI-031.
  - https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming
    -- cited by UI-005, UI-013, UI-014, UI-015.
- **Tooling:** read-only only. File and line counts via `wc -l`; brace-omission and pattern sweeps via
  `grep`/ripgrep. Nothing was built, run, formatted, or modified. The build is already clean under
  `TreatWarningsAsErrors=true` with zero warnings, so no record here rests on "the compiler would catch
  this".
- One boundary read outside this scope, for context only and not recorded here:
  `src/DungeonMaster.Infrastructure/DependencyInjection.cs`, to establish the registered lifetime of
  `AppDbContext`. The boundary record itself belongs to T6.

## Summary

| Severity | Count |
|---|---|
| S1 | 1 |
| S2 | 8 |
| S3 | 16 |
| S4 | 9 |
| S5 | 2 |
| **Total** | **36** |

## Records

### UI-001 -- Transient view models and pages resolve scoped EF Core services from the root provider

```yaml
id: UI-001
layer: UI
severity: S2
category: dependency
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines
locations:
  - src/DungeonMaster.App/MauiProgram.cs:123-131
  - src/DungeonMaster.App/MauiProgram.cs:137-141
effort: M
risk: medium
independent: true
```

**What.** All three view models and all three page/popup types are registered `AddTransient`, and the
Infrastructure registration they sit on top of registers `AppDbContext` and both repositories with the
scoped lifetime. A MAUI app has no ambient per-request scope: Shell and the popup call site resolve from
the root `IServiceProvider`, so the scoped graph is created once in the root scope and lives for the
process.

**Why it matters.** The Microsoft DI guidelines name this exact shape as the "Scoped service as singleton"
anti-pattern -- "if you're not creating a scope or within an existing scope, the service becomes a
singleton" -- and warn separately that disposable services resolved from the top-level container are held
by the container until shutdown. The concrete cost here is one `AppDbContext` shared by every screen for
the whole session: its change tracker never resets, so it accumulates every entity the app has ever read,
and stale tracked instances can be returned to a later query on a different page.

**Suggested direction.** Either resolve a fresh `IServiceScope` per navigation and dispose it when the page
is torn down, or move the data path onto `AddDbContextFactory` so each dispatched command and query owns a
short-lived context. The registration lifetimes in `MauiProgram.cs` and the Infrastructure registration
have to agree, so this is one decision taken in two places.

### UI-002 -- Database directory creation and migration run synchronously and unguarded inside CreateMauiApp

```yaml
id: UI-002
layer: UI
severity: S2
category: correctness
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/MauiProgram.cs:115-121
  - src/DungeonMaster.App/MauiProgram.cs:139
effort: S
risk: low
independent: true
```

**What.** `CreateMauiApp` creates the database directory with `Directory.CreateDirectory` and then calls
`app.Services.MigrateDatabase()` before returning. Neither call is wrapped in a `try`, and both run on the
thread that is constructing the application.

**Why it matters.** I believe this makes any storage-layer failure at first launch -- a locked file, a
read-only profile directory, a migration that cannot apply -- surface as an unhandled exception thrown out
of `CreateMauiApp`, which appears to the user as the app failing to start with no message at all. I also
believe the migration is the slowest single step on the launch path and that running it inline is what
determines the app's cold-start time on a schema change.

**Suggested direction.** Wrap both in a guard that records the failure and lets the shell start into a
readable error state rather than not starting, and consider moving the migration off the construction path
so first paint does not wait on it.

### UI-003 -- The Entry and Editor handler mappers repeat the same eight WinUI resource assignments verbatim

```yaml
id: UI-003
layer: UI
severity: S3
category: duplication
authority: practice
citation: Fowler, Refactoring 2e, ch. 3 "Duplicated Code"
locations:
  - src/DungeonMaster.App/MauiProgram.cs:75-100
  - src/DungeonMaster.App/MauiProgram.cs:102-129
effort: S
risk: low
independent: true
```

**What.** The `EntryHandler` and `EditorHandler` mappers each contain a `#if WINDOWS` block that sets the
same eight properties and resource keys on a `Microsoft.UI.Xaml.Controls.TextBox`. The two blocks are
byte-identical outright, the local variable's declaration site included; only the `#elif MACCATALYST`
halves differ.

**Why it matters.** Fowler's first bad smell: the same expression in more than one place has to be changed
in more than one place. On Windows both handlers already resolve to the same platform type, so the next
person adding a ninth key has to notice there are two copies, and a key added to only one of them produces
an `Entry` and an `Editor` that no longer look alike.

**Suggested direction.** Extract the Windows body into one private static method taking the `TextBox` and
call it from both mappers, leaving only the genuinely platform-divergent macCatalyst arms inline.

**[AMENDED 6b-xvi, 2026-10-07 -- both of this entry's `locations:` citations were stale when re-measured
at `fe90bed`, and one specific in the `What.` paragraph was measurably FALSE. Classification: DRIFT for
the citations, ERROR for the specific.** The entry's substance stands and was acted on in full: the eight
assignments now live in one `private static ApplyDmNativeStyle` method and both mappers call it. Unlike
the `AMENDED 6b-xiii` block on `CORE-019` in `01-domain-application.md`, which is this block's model and
which corrected citations only, this amendment also corrects a sentence in the `What.` paragraph. **That
model is cited for its form and not as a clean exemplar:** `AMENDED 6b-xiv`, at
`01-domain-application.md:1293`, records that the 6b-xiii block *"carries three defects of its own: one
false claim, one causal attribution that is impossible on dates, and one sentence a backlog item wrongly
calls unsound"*. Its structure is worth copying; its contents are not, and a reader who follows the
pointer should arrive already knowing that. The correction here is deliberate and it is narrow: the
claim corrected is a *measurement* the entry got wrong, not a record of
how the code then stood, and leaving a false measurement standing would make the entry's own evidence
unciteable. The `Why it matters.` and `Suggested direction.` paragraphs are left exactly as written.

*Correction 1 -- both `locations:` ranges, corrected in place.* Previous wording, verbatim:
`src/DungeonMaster.App/MauiProgram.cs:59-84` and `src/DungeonMaster.App/MauiProgram.cs:86-113`. The true
ranges at `fe90bed` are **`:75-100`** and **`:102-129`** -- the two `AppendToMapping` calls, opening
paren to closing `});`, read directly from the file. What the stale ranges actually held, also read
directly: `:59-84` **straddles** the tail of the `ConfigureFonts` lambda (three `fonts.AddFont` calls and
its closing `});` at `:71`) and the head of the `EntryHandler` mapping, ending *inside* the Entry
`TextBox` body -- its last line, `:84`, is `tb.Resources["TextControlBorderBrush"] = ...`. `:86-113`
straddles the tail of the Entry `TextBox` body -- it opens *inside* that body, on `:86`,
`tb.Resources["TextControlBorderBrushFocused"] = ...` -- then the whole Entry `#elif MACCATALYST` arm,
and it ends *inside* the Editor `TextBox` body, on `:113`, which is the Editor copy's own
`tb.Resources["TextControlBorderBrushFocused"] = ...`. **Each boundary is named by its line number and
its content rather than by a count derived from the range, and that form is deliberate:** this block's
first revision described the first boundary with a subtraction-derived count of assignments and got it
wrong, and described the second merely as "the head of the Editor mapping" when the range in fact
reaches well into that body -- an understatement rather than a false figure, but the same class of
error the block exists to repair. A named anchor cannot be miscounted, and if the line moves the
citation fails loudly instead of going quietly stale. **The evidence that this is drift and not a
re-identification of a different defect is that each cited span's own LENGTH is right and both are
shifted by the SAME constant.** `59-84` is 26 lines and the true `75-100` is 26 lines; `86-113` is 28
lines and the true `102-129` is 28 lines; `75 - 59 = 16` and `102 - 86 = 16`. Arithmetic over the four
ranges, stated so it can be rechecked. Equal lengths plus one constant offset is the signature of content
inserted above the first citation after the entry was written -- sixteen lines of it. **This session did
not locate the inserting commit**, so the offset is measured and the cause is inferred from its shape,
not read from history.

*Correction 2 -- the `What.` paragraph's qualifier, which was false.* Previous wording, verbatim: the two
blocks are *"byte-identical apart from the local variable's declaration site"*. They are byte-identical
**outright**; there is nothing to except, because the declaration site is identical too and sits inside
the compared span. Measured at `fe90bed` over `:80-90` and `:107-117`, the two `if (handler.PlatformView
is Microsoft.UI.Xaml.Controls.TextBox tb)` narrowings and their braced bodies: `cmp` reports no
difference, `wc -c` gives **1021 bytes** each, and both sha256 to
`8d0fa963fe1c4fe2227fa2bcc957ff44f395849db41b0420e3a175ce151104a7`. The narrowing line is line one of
each compared span, which is exactly why the qualifier cannot stand. The sentence now reads
"byte-identical outright, the local variable's declaration site included".

**One of this entry's claims was LEFT UNMEASURED, and is recorded as unmeasured rather than resolved.**
The `Why it matters.` paragraph asserts that *"on Windows both handlers already resolve to the same
platform type"*. This session neither confirmed nor refuted it. What is established: the cross-platform
handler surface types the platform view as `object` -- `IElementHandler.PlatformView` is declared
`public object? PlatformView { get; }` and `IEntryHandler.PlatformView` as `public object PlatformView
{ get; }` ([Microsoft Learn, `IElementHandler.PlatformView`](https://learn.microsoft.com/dotnet/api/microsoft.maui.ielementhandler.platformview?view=net-maui-10.0)),
which is why this code pattern-matches rather than casts. What is not established: `Editor`'s native
Windows type. Nothing read in this session names it, and it was not derived from the running
application. **The remedy does not rest on the claim.** Both call sites narrow with
`is Microsoft.UI.Xaml.Controls.TextBox tb` before calling, so the extracted method runs only when the
platform view genuinely is a `TextBox`, whatever `EditorHandler` resolves to in general; if it ever
resolves to something else, the narrowing fails closed exactly as the inline copy did. A verdict either
way would be a guess dressed as a measurement, which is the failure mode these blocks exist to prevent.
**Both corrected ranges, and the `:80-90` / `:107-117` spans above, are historical positions at
`fe90bed` and are NOT navigable at `HEAD`** -- this session's own remedy deleted one of the two bodies
and moved the other into a method, which shifts everything below it.]

### UI-004 -- DungeonDetailViewModel discards the Result of every dispatched command

```yaml
id: UI-004
layer: UI
severity: S1
category: correctness
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:128-134
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:159-163
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:176-185
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:258-268
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:280-288
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:384-394
effort: M
risk: low
independent: true
```

**What.** Every `dispatcher.SendAsync<T>(...)` call in this view model except one (`AddAreaWithOutlineAsync`,
line 302) throws away the returned `Result`. The type carries `IsSuccess`, `Error`, and an `ErrorCategory`,
and the Application layer's `ValidationBehavior` reports rejected writes through it rather than by throwing.
This view model has no `ErrorMessage` property at all, unlike its two siblings.

**Why it matters.** I believe this makes a rejected write indistinguishable from a successful one from the
user's side: the command completes, the unconditional reload that follows repaints the pre-edit values, and
nothing is shown. The page's own code-behind states that `UpdateAreaCommandValidator` rejects an empty area
name and drops the whole write including description and notes
(`Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:548-553`), which is a rejection reachable by typing in
the notebook -- so I believe an ordinary edit can be silently lost today.

**Suggested direction.** Give this view model the same `ErrorMessage` channel the list and create view
models already have, and branch on `result.IsSuccess` at each call site before reloading, so a failure both
skips the reload and says why.

### UI-005 -- ApplyQueryAttributes starts the load as a discarded task and silently ignores a bad id

```yaml
id: UI-005
layer: UI
severity: S2
category: async
authority: microsoft
citation: https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming
locations:
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:83-89
effort: S
risk: medium
independent: true
```

**What.** `ApplyQueryAttributes` parses the `id` query parameter and, on success, calls
`_ = LoadDungeonCommand.ExecuteAsync(id)` -- the task is discarded, never awaited, and never observed. If
`id` is absent or does not parse as a `Guid`, the method falls through and does nothing at all.

**Why it matters.** Discarding the task has the same failure semantics the Microsoft async guidance
describes for `async void`: the exception is captured on a `Task` object nobody holds, so a failed load is
neither surfaced nor logged, and no caller can tell when the load has finished. It also means the page is
constructed and shown against an empty view model and repaints when the load lands, which is a visible
first-frame flash rather than a loading state. The silent branch on an unparsable `id` leaves the page in
that empty state permanently, with `_dungeonId` left at `Guid.Empty` so every subsequent save targets a
dungeon that does not exist.

**Suggested direction.** Keep `IQueryAttributable` -- it is the trim-safe option the Shell navigation
documentation recommends over `QueryProperty` -- but hold the returned task in a field the page can await,
or set a `IsLoading`/`ErrorMessage` pair from a continuation, and give the missing-or-unparsable branch an
explicit error state.

### UI-006 -- DungeonDetailViewModel carries four unrelated responsibilities in one 397-line type

```yaml
id: UI-006
layer: UI
severity: S3
category: structure
authority: practice
citation: Fowler, Refactoring 2e, ch. 3 "Large Class"
locations:
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:27-397
effort: L
risk: medium
independent: true
```

**What.** The type is nine `[RelayCommand]` methods, five public non-command async methods, six observable
properties, a drawing session, a floor collection, and a nested public record, spanning fourteen distinct
Application-layer commands and queries. It is roughly four times the size of the other three view models
combined excluding itself.

**Why it matters.** Fowler's Large Class smell applies literally: the class has more state than any one of
its clients needs, and the reader has to hold all of it to change any of it. Concretely, three seams are
visible in the code as written -- dungeon metadata editing (`DungeonName`/`Description`/`Notes` plus
`SaveDungeon`), floor lifecycle (`AddFloor`, `RemoveFloor`, `UpdateFloor`, `ReorderFloor`), and canvas
content (`AddArea`, `RemoveArea`, `UpdateArea`, `MoveAreaOutline`, `AddAreaWithOutline`, and the four
placed-object methods) -- with the drawing session and `ActiveTool` forming a fourth, purely
presentational, cluster.

**Suggested direction.** Split on those seams: a `DungeonHeaderViewModel` for the metadata trio, a
`FloorListViewModel` owning `Floors` and the four floor commands, and a `MapCanvasViewModel` owning
`ActiveTool`, `DrawingSession`, `EditingAreaId`, and the area/object commands, with the detail view model
reduced to a composition root that holds the three and the shared `_dungeonId`.

### UI-007 -- Every mutation reloads the whole dungeon aggregate, and exactly one does not

```yaml
id: UI-007
layer: UI
severity: S3
category: performance
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:147
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:185
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:243
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:296-312
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:335
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:394
effort: M
risk: medium
independent: false
blocked-by:
  - UI-004
```

**What.** Eleven of the twelve mutating operations end with `await LoadDungeonAsync(_dungeonId)`, which
re-runs `GetDungeonDetailQuery` for the entire dungeon -- all floors, all areas, all placed objects -- and
clears and refills the `Floors` collection. `AddAreaWithOutlineAsync` (line 296) is the single exception and
does not reload; its caller in the page reloads separately.

**Why it matters.** I believe this makes the cost of nudging one placed object proportional to the size of
the whole dungeon, and I believe the `Floors.Clear()` that each reload performs is what forces the page to
tear down and rebuild every sidebar row -- the page's own comments at
`Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:545-546` and `:587` document having to re-resolve
objects by id after every await because of it. I also believe the one exception is an inconsistency rather
than an optimisation, since it leaves the reload obligation with the caller and nothing in the type says so.

**Suggested direction.** Make the reload policy uniform and explicit, then narrow it: have the mutating
commands return or apply the changed projection rather than re-querying the aggregate, so the observable
collection is patched instead of rebuilt.

**[AMENDED 20261106-2050-detail-page-trio, 2026-11-06 -- this entry's second remedy was acted on as
`MASTER-094`, which is closed, and two specifics in its `What.` paragraph were stale at this session's
start: every view-model mutation reloaded, `AddAreaWithOutlineAsync` included.** Form follows the
`AMENDED 6b-xvi` block on `UI-003` in this file at `:201`.

*Closure.* `MASTER-094` (`docs/audit/00-master-index.md:282`) is closed `done` by this session. Measured
by task `T4` on the session worktree: the nine floor-scoped view-model commands of C3 section 3.1 now
await `context.RefreshFloorAsync`, which re-reads only their own floor through `GetFloorDetailQuery`
(`src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:193`) and replaces that one `FloorItem` in
`FloorList.Items` in place, falling back to the full reload on each case C3 section 3.1 lists; P-REFRESH
(C1 section 4.2) prints 9 lines (0 at session start), and P-RELOAD (C1 section 4.1) prints 4 lines,
against 14 at session start. The design is
`.sir/sessions/20261106-2050-detail-page-trio/contracts/C2-floor-detail-query.md` and section 3 of
`.sir/sessions/20261106-2050-detail-page-trio/contracts/C3-editor-state-and-floor-refresh.md`.

*Correction of `What.`* The entry says "Eleven of the twelve mutating operations end with `await
LoadDungeonAsync(_dungeonId)`" and "`AddAreaWithOutlineAsync` (line 296) is the single exception and does
not reload". At this session's start both were stale. The reload was `context.ReloadAsync()`, and it
ended **all** thirteen view-model mutations: P-RELOAD (C1 section 4.1), re-run by `T4` at the session's
starting commit with read-only `git grep`, prints 14 lines, 13 in `src/DungeonMaster.App/ViewModels`
and the page's 1 (`DungeonDetailPage.xaml.cs:1335`). `AddAreaWithOutlineAsync` reloaded once, after its
second write (`MapCanvasViewModel.cs:231` at session start), and its doc comment there records the change
in its `<remarks>` (`:177-183`: "Before this session the outline write did not reload at all"). No
classification word is given: this session traced no history between the audited tree and its own.

*"Why it matters.", re-stated as measured.* The `Floors.Clear()` the entry names is now `FloorList.Items`
(`src/DungeonMaster.App/ViewModels/FloorListViewModel.cs:32`). A floor-scoped mutation no longer clears
and refills it: `ReplaceAt` (`:108-113`) assigns the one element, which raises one `Replace` and no
`Clear` or `Add` (C3 section 3.1), and the templated floor list (`UI-016` below) re-binds the replaced row
and leaves every other row instance alone (**told**: measured by task `T3`'s implementer, not reproduced
by its reviewer). The page-comment citations `DungeonDetailPage.xaml.cs:545-546` and `:587` in that
paragraph are not re-derived, and `locations:` is kept unchanged as a record of the audited state;
nothing above is deleted or edited.

*What remains.* Four mutations still re-query the whole dungeon, by design (C3 section 3.2):
`AddFloorCommand` changes the floor list's membership, `RemoveFloorCommand` changes membership and
renumbers every later floor, `ReorderFloorCommand` renumbers every floor between the two positions, and
the header `SaveCommand` changes the dungeon-level projection itself. Each still leaves `FloorList.Items`
as `Populate` does, `Clear()` then one `Add` per floor (`FloorListViewModel.cs:42-65`). The record's
severity, category, effort and risk are unchanged.]**

### UI-008 -- Five public async methods sit outside the command surface, three re-taking an id the type holds

```yaml
id: UI-008
layer: UI
severity: S3
category: api-design
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:275-289
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:341-354
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:359-395
effort: S
risk: low
independent: true
```

**What.** `MoveAreaOutlineAsync`, `RemovePlacedObjectAsync`, and the two `UpdatePlacedObjectAsync` overloads
are public `Task`-returning methods rather than `[RelayCommand]` members, unlike the nine operations beside
them. Three of them take a `dungeonId` parameter, although the type already holds `_dungeonId` and uses it
for every other operation; the page supplies it as `_viewModel.Dungeon!.Id` from four call sites.

**Why it matters.** I believe the split surface is what forces the page to reach for the view model in two
different idioms -- `_viewModel.RemoveAreaCommand.ExecuteAsync(...)` on one line and
`_viewModel.UpdatePlacedObjectAsync(...)` on the next -- and I believe the redundant `dungeonId` parameter
is a live correctness hazard, because nothing constrains the caller to pass the same dungeon the view model
is loaded against, and the null-forgiving `Dungeon!.Id` at the call sites will throw if the load has not
landed.

**Suggested direction.** Bring the four onto `[RelayCommand]` with argument records like the existing
`PlaceObjectArgs`, and drop `dungeonId` from their signatures so `_dungeonId` is the single source for it.

### UI-009 -- The view model navigates through the static Shell.Current

```yaml
id: UI-009
layer: UI
severity: S3
category: testability
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines
locations:
  - src/DungeonMaster.App/ViewModels/DungeonListViewModel.cs:101-103
effort: S
risk: low
independent: true
```

**What.** `NavigateToDetailAsync` calls `await Shell.Current.GoToAsync($"dungeondetail?id={id}")` directly.
`Shell.Current` is a static ambient accessor; the view model takes no navigation abstraction in its primary
constructor, which has one parameter, the dispatcher.

**Why it matters.** The Microsoft DI guidelines list "Avoid static access to services" among their
recommendations and note that DI is an alternative to global object access which you do not benefit from
if you mix the two. The concrete cost is that this command cannot be exercised without a live Shell: any
test of the list view model that touches it needs the MAUI application object to exist, which is precisely
why the Tests project does not reference App at all.

**Suggested direction.** Inject a small `INavigationService` with a `GoToAsync(string route, ...)` member,
implement it over `Shell.Current` in the App layer, and register it in `MauiProgram.cs`. The view model
then asserts on a substitute.

### UI-010 -- The detail route is a bare string literal repeated at its registration and its call site

```yaml
id: UI-010
layer: UI
severity: S4
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/AppShell.xaml.cs:16-18
  - src/DungeonMaster.App/ViewModels/DungeonListViewModel.cs:103
  - src/DungeonMaster.App/AppShell.xaml:12-15
effort: XS
risk: low
independent: true
```

**What.** `"dungeondetail"` is written twice: once in `Routing.RegisterRoute` and once, lower-cased and
interpolated with a query string, in the view model. The `Route="DungeonListPage"` on the `ShellContent` is
a third route name declared in a third place, with different casing convention from the registered one.

**Why it matters.** I believe route strings are the one part of Shell navigation with no compile-time check
of any kind -- the Microsoft Shell navigation page listed in `## Method` states that navigating to a route
that has not been registered throws `ArgumentException` at runtime -- so a typo or a rename in one of the two places is only discoverable by
running the app and clicking through. I also believe the two casing conventions in use suggest no rule has
been settled.

**Suggested direction.** Put the route names and their query-parameter keys in one internal static class of
constants that both the registration and the navigation call read, and settle a single casing convention
for them.

**[AMENDED 6b-xix, 2026-10-12 -- this entry is CONFIRMED IN SUBSTANCE and was acted on in full. One
citation is stale, its "written twice" count is three textual occurrences and two functional ones, and its
casing suggestion carries NO AUTHORITY and was deliberately not followed. Classification: CONFIRMED;
DRIFT for one citation; NARROWED for the "twice" clause.** Form follows the `AMENDED 6b-xvi` block on
`UI-003` in this file at `:201`. Every line below was re-derived by content search on the post-change tree.

**The substance stands and is now centralised.** The route name and the query-parameter key are each
declared once, in a new `internal static class ShellRoutes` with one type per file and the file named for
the type per section 2 at `dotnet-format.md:56` and `:57`, in a folder mirroring the namespace per `:58`.
Post-change, located by content search: **`Services/Navigation/ShellRoutes.cs:13`**
`internal const string DungeonDetail = "dungeondetail";` and **`:19`**
`internal const string IdQueryKey = "id";`, with `namespace DungeonMaster.App.Services.Navigation;` at
`:1` and the type at `:7`. Both live sites read them: the registration at **`AppShell.xaml.cs:17-18`**,
where `:17` is `Routing.RegisterRoute(` and `:18` is `ShellRoutes.DungeonDetail,`; and the navigation call
at **`ViewModels/DungeonListViewModel.cs:107`**, now
`await navigation.GoToAsync($"{ShellRoutes.DungeonDetail}?{ShellRoutes.IdQueryKey}={id}");`. The key is
read back at **`ViewModels/DungeonDetailViewModel.cs:115`**, inside `ApplyQueryAttributes`.

*Correction 1 -- one `locations:` citation, stale by four lines.* Previous wording, verbatim:
`src/DungeonMaster.App/ViewModels/DungeonListViewModel.cs:103`. **`:103` holds the `<param>` description
for the dungeon identifier, not the construct**; the navigation call was and remains at **`:107`**, cited
by its content rather than by an offset.

*Correction 2 -- "written twice" is three textual occurrences and two functional ones.* Previous wording,
verbatim: "`"dungeondetail"` is written twice". **There were THREE textual occurrences and only two
functional ones.** The third is **illustrative doc prose at
`Services/Navigation/INavigationService.cs:11`**, which describes the wire format a caller may pass:
`///` then `a query, e.g. <c>"dungeondetail?id={guid}"</c>.` **Nothing compiles that into a route match**,
it is outside the ownership set of the task that acted on this row, and it was deliberately left alone.
**It is recorded as a live documentation-drift risk**: it restates the route's shape in prose that no
compiler checks, which is the same mechanism behind every stale citation in this row set, and it is carried
to `.dir-sdlc/backlog.md`. Post-change the repo-wide census returns **exactly two** occurrences of the
quoted literal -- `grep -rn '"dungeondetail' src --include=*.cs --include=*.xaml` with `/obj/` and `/bin/`
excluded gives `ShellRoutes.cs:13` and that prose line -- **with both former call sites gone.** The
predicate's near-miss class is named rather than assumed: the case-insensitive superset is dominated by the
`DungeonDetailPage` and `DungeonDetailViewModel` identifiers, and **the leading double quote is what
excludes them** -- `MauiProgram.cs:148`'s `AddTransient<DungeonDetailViewModel>()` correctly does not
match.

**A POPULATION THE ENTRY'S `What.` NEVER MENTIONS, AND WHICH ITS REMEDY NONETHELESS COVERS -- so the
remedy's query-key clause is independently validated rather than decorative.** The query-parameter key
`"id"` was duplicated across the same producer/consumer pair as the route name. The **raw census** of the
quoted `"id"` over the App project is **7 lines**, and the figure that matters is **n = 1** key literal.
**The six near-misses are enumerated rather than asserted, and all six are `<param>` or `<paramref>` doc
text**: `DungeonDetailViewModel.cs:162` and `:165`, `DungeonListViewModel.cs:77`, `:79`, `:100` and `:102`.
All six are unchanged by this work and were deliberately left untouched; **the single key literal is now
the constant at `ShellRoutes.cs:19`**, which is where the raw 7 still resolves post-change.

**BOTH LITERAL VALUES ARE DELIBERATELY UNCHANGED, and the entry's casing suggestion was NOT followed.**
Previous wording, verbatim: "settle a single casing convention for them." **That suggestion has no
authority behind it.** This entry carries `authority: judgment` and `citation: n/a` -- **stated explicitly:
no convention text supports or contradicts this row** -- and **MAUI Shell route matching is by exact
string**, so changing a literal's value is a **behavioural change to navigation rather than a formatting
change.** Both constants are `const string`, so the compiler inlines the same bytes at every use, and the
three before-and-after values are byte-identical: `"dungeondetail"` at registration,
`dungeondetail?id=<guid>` at the call site, and `"id"` read back.

**THE DISPOSITION OF `AppShell.xaml:15`, per the architect's escalation, with the measured reason.**
`Route="DungeonListPage"` is confirmed at **`:15`** -- `grep -rn 'Route="' src --include=*.xaml` returns
**exactly one hit** solution-wide -- and the entry is right that it is a third route name in a third place
with different casing. **It is OUT OF SCOPE by an explicit Gate 2 decision, and the reason is measured
rather than asserted: XAML `Route="..."` CANNOT read a C# constant without `{x:Static}` plus a namespace
mapping, and `{x:Static}` has ZERO occurrences in this repository's XAML.** By
`grep -rn 'x:Static' src --include=*.xaml` with `/obj/` excluded, the search **exits 1 with no output**.
**That zero is CONTROLLED: the same machinery finds `clr-namespace:` mappings at `App.xaml:4` and
`Views/DungeonListPage/DungeonListPage.xaml:7`**, so the search reaches this repository's XAML and the
absence is real. **And `DungeonListPage` is a different route from the one centralised here, so leaving it
literal duplicates nothing** -- which is why a literal reading of the remedy's "both the registration and
the navigation call" correctly leaves `:15` untouched, even though it means the entry's third-place clause
survives the fix.

**KNOWN DEVIATION in this session's own new prose, recorded rather than silently carried.** The three XML
documentation blocks in `ShellRoutes.cs` each **break a sentence across two `///` lines**, which section 9
at `dotnet-format.md:378` forbids -- "Never break a sentence across multiple `///` lines." The acceptance
criterion governing those blocks enumerated `:372`, `:375` and `:376` and **omitted `:378`**, so review
classified this advisory rather than blocking: the implementer satisfied what it was given, and **the
enumeration gap was the orchestrator's own, asymmetrically against a sibling task whose equivalent
criterion did cite `:378`.** The same violation exists in pre-existing prose elsewhere in this layer. **It
is carried forward to `.dir-sdlc/backlog.md` rather than repaired here.**]

### UI-011 -- DungeonListPage injects IServiceProvider and resolves the popup from it

```yaml
id: UI-011
layer: UI
severity: S3
category: testability
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines
locations:
  - src/DungeonMaster.App/Views/DungeonListPage/DungeonListPage.xaml.cs:17
  - src/DungeonMaster.App/Views/DungeonListPage/DungeonListPage.xaml.cs:29-36
  - src/DungeonMaster.App/Views/DungeonListPage/DungeonListPage.xaml.cs:71
effort: S
risk: low
independent: true
```

**What.** The page's constructor takes an `IServiceProvider` alongside its view model, stores it, and calls
`_services.GetRequiredService<CreateDungeonPopup>()` when the create button is clicked.

**Why it matters.** The Microsoft DI guidelines name this directly: "Avoid using the service locator
pattern. For example, don't invoke `GetService` to obtain a service instance when you can use DI instead",
and add that injecting a factory that resolves dependencies at runtime is the same anti-pattern in another
form. The concrete cost is that the page's real dependency -- the ability to produce a popup -- is invisible
in its signature, so nothing but reading the body reveals it.

**Suggested direction.** Inject a `Func<CreateDungeonPopup>` or a small popup-factory abstraction so the
dependency is declared, and keep the container out of the page. Note that the popup being resolved per
click is correct and should be preserved.

### UI-012 -- The create view model reports its outcome through two events that the popup converts to a result

```yaml
id: UI-012
layer: UI
severity: S3
category: api-design
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/ViewModels/CreateDungeonViewModel.cs:35
  - src/DungeonMaster.App/ViewModels/CreateDungeonViewModel.cs:40
  - src/DungeonMaster.App/ViewModels/CreateDungeonViewModel.cs:58
  - src/DungeonMaster.App/ViewModels/CreateDungeonViewModel.cs:71
  - src/DungeonMaster.App/Views/CreateDungeonPopup/CreateDungeonPopup.xaml.cs:24-28
effort: S
risk: medium
independent: true
```

**What.** `CreateDungeonViewModel` raises `CreatedSuccessfully` and `Cancelled`; the popup subscribes to
both in its constructor with lambdas that call `_ = CloseAsync(true)` and `_ = CloseAsync(false)`. The
subscriptions are never removed, and both close tasks are discarded.

**Why it matters.** I believe this is a weaker seam than the one the toolkit already provides: the popup is
a `Popup<bool>` and `ShowPopupAsync<bool>` already returns that result to the caller, so the two events
duplicate a channel that exists. I believe the discarded `CloseAsync` tasks mean a failure to close is
unobservable, and while the page and view model are both transient and co-owned -- so the missing
unsubscribe appears not to leak in practice -- the pattern does not survive the view model becoming
longer-lived than the popup.

**Suggested direction.** Have the commands expose their outcome as observable state and let the popup close
itself from a single place, or keep the events and unsubscribe them when the popup closes so the pairing
is complete.

### UI-013 -- async void on three methods that are not event handlers

```yaml
id: UI-013
layer: UI
severity: S2
category: async
authority: microsoft
citation: https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming
locations:
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1038
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1050-1055
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1602
effort: S
risk: medium
independent: true
```

**What.** `EraseAreaAsync`, `PlaceObjectAsync`, and `FinishDrawing` are declared `async void` but are not
event handlers. They are called as ordinary statements from `OnMapStartInteraction` (lines 1277, 1289),
`OnMapDoubleTapped` (line 1645), and `OnMapStartInteraction`'s draw branch (line 1336).

**Why it matters.** The Microsoft guidance is explicit -- "Avoid async void ... prefer async Task methods
over async void methods", with event handlers the only exception. The consequence it describes applies
here in full: an exception escaping one of these is raised directly on the synchronisation context rather
than captured on a task, so a failed erase or place takes the process down instead of failing the
operation. The callers also cannot sequence against them: `OnMapStartInteraction` returns immediately after
calling `PlaceObjectAsync`, so a second click can begin a second place before the first has reloaded.

**Suggested direction.** Change all three to `async Task` and `await` them from their callers -- which
means the three callers that are themselves genuine event handlers become `async void` handlers with a
single awaited call and a `try`, the shape the guidance recommends.

### UI-014 -- async void event handlers across the layer carry no exception guard

```yaml
id: UI-014
layer: UI
severity: S2
category: async
authority: microsoft
citation: https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming
locations:
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:554
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1502
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:2211
  - src/DungeonMaster.App/Views/DungeonListPage/DungeonListPage.xaml.cs:69
  - src/DungeonMaster.App/Views/DungeonListPage/DungeonListPage.xaml.cs:100
  - src/DungeonMaster.App/Views/DungeonListPage/DungeonListPage.xaml.cs:114
  - src/DungeonMaster.App/Views/DungeonListPage/DungeonListPage.xaml.cs:124
  - src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs:137
  - src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs:148
effort: M
risk: low
independent: true
```

**What.** Nine `async void` event handlers await work that reaches the dispatcher, the pack registry, or
the file system, and none of them wraps that await in a `try`. The one handler in the layer that does --
`DungeonListPage.OnAppearing` at lines 55-67 -- documents in its own `<remarks>` exactly why the guard is
load-bearing, and that reasoning applies unchanged to the other nine.

**Why it matters.** As the Microsoft guidance puts it, exceptions thrown out of an `async void` method are
raised directly on the synchronisation context and "can't be caught naturally" by the caller. Every one of
these nine is therefore a crash path: a failed dungeon load in `OnPackPickerChanged`, a failed notebook
save in `OnNotebookSaveRequested`, or a failed delete in `OnDeleteClicked` ends the process rather than the
gesture.

**Suggested direction.** Apply the pattern `OnAppearing` already establishes -- a `try` around the awaited
call routing the failure into the page's or view model's error channel -- to the remaining handlers, or
factor the guard into one helper the handlers call so the shape is stated once.

### UI-015 -- Discarded tasks in the keyboard, gesture, and popup dispatch paths

```yaml
id: UI-015
layer: UI
severity: S2
category: async
authority: microsoft
citation: https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming
locations:
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:143
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:163
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:351
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:369
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:388
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1209-1224
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1522
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1662
effort: M
risk: medium
independent: true
```

**What.** Eight call sites start `RotateObjectAsync`, `ScaleObjectAsync`, `DeleteSelectedObjectAsync`, or
`MoveObjectAsync` with a `_ =` discard. Each of those methods dispatches a write and then calls
`RefreshFloorAsync`, which reloads the entire dungeon.

**Why it matters.** The discard has the same error semantics as `async void` -- the failure lands on a task
nobody holds -- and it additionally removes all ordering. Holding `]` in Place mode, or clicking the popup's
rotate button twice, starts a second write-then-reload before the first has completed, against a shared
`AppDbContext` whose documentation states plainly that it is not thread-safe and that concurrent use can
result in "undefined behavior, application crashes and data corruption". The hand-rolled hit-zone dispatch
at lines 1209-1224 fires one of five of these per touch, so the popup is the densest of the eight.

**Suggested direction.** Make the enclosing handlers `async void` with a guard and `await` these calls, or
serialise them behind a single in-flight guard so a second gesture is dropped or queued rather than run
concurrently.

### UI-016 -- The detail page's code-behind is 2,226 lines and owns the editor's entire interaction state machine

```yaml
id: UI-016
layer: UI
severity: S3
category: structure
authority: practice
citation: Fowler, Refactoring 2e, ch. 3 "Large Class"
locations:
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:26-77
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1184-1668
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:651-820
effort: L
risk: high
independent: true
```

**What.** The page declares twenty-two private fields of interaction state -- draw state machine, move-drag
state, pan anchor, ghost rotation, hovered/selected object ids, popup bounds, drag thresholds, placeholder
targets -- and holds every transition between them across five gesture handlers and a keyboard handler. It
also builds the entire floor sidebar imperatively in `RebuildFloorList`, wiring twelve event handlers per
row inside the loop.

**Why it matters.** Fowler's Large Class again, but the specific cost here is the split of one model across
two types: `ActiveTool` and `DrawingSession` live in the view model while `_drawState`, `_focusedAreaId`,
`_selectedObjectId`, and `_isMoveDragging` live in the page, and the invariants between them -- such as
"`_drawState` is `Drawing` only when `ActiveTool` is `Draw` and the session is active" -- are maintained by
hand in `SetActiveTool` and `ResetDrawState` with no type enforcing them. Some legitimate view work is
correctly here and should stay: gesture plumbing, cursor shape, opacity and `InputTransparent` toggling,
and the placeholder `Border` insertion are all genuine visual concerns.

**Suggested direction.** Move the tool/draw/selection state and its transitions into the canvas view model
proposed in UI-006, leaving the page with gesture-to-intent translation and visual state only; and turn
`RebuildFloorList`'s hand-rolled loop into a templated `BindableLayout` over `Floors` so row wiring is
declarative.

**[AMENDED 20261106-2050-detail-page-trio, 2026-11-06 -- this entry's second remedy was acted on as
`MASTER-108`, which is closed: the floor sidebar is a templated `BindableLayout`, and the `What.`
paragraph's "wiring twelve event handlers per row" was stale at this session's start.** Form follows the
`AMENDED 6b-xvi` block on `UI-003` in this file at `:201`.

*Closure.* `MASTER-108` (`docs/audit/00-master-index.md:296`) is closed `done` by this session. Measured
by task `T4` on the session worktree: `FloorListContainer`
(`src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml:694`) carries
`BindableLayout.ItemsSource="{Binding FloorList.Items}"` (`:698`) and a `BindableLayout.ItemTemplate`
(`:754`) whose root is `controls:FloorSidebarItem` (`:756`). The hand-rolled loop is gone: the C1
section 4.5 census prints 0 lines for `RebuildFloorList` in `src/DungeonMaster.App` and
`src/DungeonMaster.App.UiTests` (4 at session start), 0 for `OnFloorsChanged` (3), and 0 for
`new FloorSidebarItem` in `src/DungeonMaster.App` (1). Row wiring is declarative: the template declares
14 event and command attributes on `FloorSidebarItem`, read line by line, nine events at `:765-773` and
five `Area*Command` bindings at `:760-764`, where `RebuildFloorList` wired the same nine subscriptions and
five assignments by hand on every row (**told**, from this session's contract
`.sir/sessions/20261106-2050-detail-page-trio/contracts/C4-register.md` section 2.1). The entry's "over
`Floors`" names a collection that has been `FloorList.Items` since 5b split the view model.

*Correction of `What.`* "wiring twelve event handlers per row" was stale at this session's start: the
loop wired nine event subscriptions and five command assignments per row, read from
`DungeonDetailPage.xaml.cs:807-1000` at session start (**told**, from the same contract, section 2.2).
No classification word is given: this session traced no history between the audited tree and its own.

*What stays in code-behind, as the entry says it should.* "the placeholder `Border` insertion", which the
entry counts among the "genuine visual concerns", still lives in the page: P-CHILDMUT (C1 section 4.4)
prints 3 lines, all the drag placeholder's, `Remove(_dragPlaceholder)` at `DungeonDetailPage.xaml.cs:2594`
and `:2670` and `Insert(insertIdx, _dragPlaceholder)` at `:2658`, and no `Clear(` or `Add(` line. The
first remedy (5b) is not re-assessed here. `locations:` is kept unchanged as a record of the audited
state; nothing above is deleted or edited. The record's severity, category, effort and risk are
unchanged.]**

### UI-017 -- Geometry and model transformation are performed in the page rather than in a testable type

```yaml
id: UI-017
layer: UI
severity: S3
category: testability
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1573-1591
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1672-1759
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1763-1789
effort: M
risk: medium
independent: true
```

**What.** Three pieces of pure computation sit in the page: the translation of every `PathSegment` of an
area outline by a tile delta (including both optional control points), a ray-casting point-in-polygon test
with a smallest-area tie-break, and a bounding-box object hit test with the same tie-break.

**Why it matters.** I believe these are the highest-value units to test in the whole editor and currently
the least testable ones in the repository: they are private members of a `ContentPage`, and the Tests
project does not reference the App project at all, so nothing can reach them. I also believe the outline
translation at lines 1579-1591 is model transformation rather than view work -- it constructs the exact
value that is then persisted -- and that an off-by-one in the control-point arithmetic would be silent.

**Suggested direction.** Move all three into internal static helpers in a non-view type, alongside the
existing `TileCoordinateHelper`, and note for T6 that reaching them from tests requires the Tests project
to take a reference on App or the helpers to move to a project it already references.

### UI-018 -- The Windows scroll-zoom unsubscribe re-resolves a handler that may already be gone

```yaml
id: UI-018
layer: UI
severity: S3
category: disposal
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:268-279
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:440-474
effort: S
risk: low
independent: true
```

**What.** `AttachScrollZoom` subscribes `PointerWheelChanged` on `MapCanvas.Handler.PlatformView`.
`DetachScrollZoom` unsubscribes by re-resolving `MapCanvas.Handler?.PlatformView` rather than by
remembering the element it subscribed to. `DetachRuntimeHandlers`, which calls it, runs from
`OnHandlerChanged` when the page's own `Handler` becomes null.

**Why it matters.** I believe the unsubscribe can silently do nothing: the null-conditional makes the whole
statement a no-op if the canvas handler has been disconnected by the time the page's handler goes null, and
the two disconnects are not ordered by anything in this file. I believe the result would be a native
`FrameworkElement` still holding a delegate onto the page, which is the shape of a leak that survives
navigation. The page's own comment block at lines 171-193 argues convincingly that the *other* runtime
subscriptions are safe because the page owns their targets outright; that argument does not extend to this
one, because the target is a native object with its own lifetime.

**Suggested direction.** Capture the `FrameworkElement` in a field when subscribing and unsubscribe from
the captured reference, clearing the field afterwards, so detach cannot depend on the handler still being
resolvable.

### UI-019 -- The canvas cursor is set through private reflection with the failure silently swallowed

```yaml
id: UI-019
layer: UI
severity: S3
category: correctness
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:911-934
effort: M
risk: medium
independent: true
```

**What.** `SetCanvasCursor` obtains the `ProtectedCursor` property of `Microsoft.UI.Xaml.UIElement` with
`BindingFlags.NonPublic` and invokes its setter through `?.SetValue`, so a null `PropertyInfo` produces no
effect and no signal.

**Why it matters.** I believe reflecting over a non-public member of a framework type is a dependency on
an implementation detail that carries no compatibility promise, and that the null-conditional turns the
day it stops resolving into a cursor that quietly stops changing rather than an error anyone would notice.
I also believe this is invisible to the test suite, since the theme guard tests read source as text and
cannot exercise it.

**Suggested direction.** Move the cursor work behind a small platform abstraction with a Windows
implementation, and if reflection stays, have it resolve the `PropertyInfo` once into a static field and
fail loudly -- a log line at minimum -- when the member is not found.

### UI-020 -- A user32 P/Invoke sits directly in shared page code-behind

```yaml
id: UI-020
layer: UI
severity: S3
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:71-74
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1186-1203
effort: S
risk: low
independent: true
```

**What.** The page declares `[DllImport("user32.dll")] GetAsyncKeyState` behind `#if WINDOWS` and polls the
right mouse button synchronously at the top of `OnMapStartInteraction` to suppress the interaction when the
secondary button is down.

**Why it matters.** I believe native interop in a page's code-behind is the wrong home for it on two
counts: the `Platforms/Windows` folder exists precisely to hold Windows-only code and this bypasses it, and
polling a global input state to reconstruct which button caused an event is a workaround for
`GraphicsView` not surfacing the button, which is a platform concern rather than a page concern. I also
note the file itself records that the macCatalyst equivalent is unimplemented and unverified.

**Suggested direction.** Move the button-state query behind a partial method or small service with a
`Platforms/Windows` implementation and a `Platforms/MacCatalyst` one, so the page asks "was this a
secondary press" and each platform answers in its own file.

### UI-021 -- macCatalyst input handling is declared unimplemented in five places while remaining a shipped target

```yaml
id: UI-021
layer: UI
severity: S2
category: correctness
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:294-301
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:445-451
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:935-941
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1196-1202
  - src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs:39-41
effort: L
risk: high
independent: true
```

**What.** `net10.0-maccatalyst` is a declared target framework, and five `TODO(macOS)` blocks in the layer
record that specific input paths are unverified or absent on it: `IsTextInputActive` returns `false`
unconditionally on macCatalyst so keyboard shortcuts are not suppressed while typing; pinch zoom has no
centroid so it always zooms to the canvas centre; the cursor shapes have no 1:1 mapping; right-click
suppression has no equivalent guard; and the pack picker's popup styling is untested.

**Why it matters.** I believe the first of these is user-visible data loss on that platform: with no
text-input guard, typing a name containing `d`, `m`, `h`, `p`, `s`, `e`, or `r` into the notebook would
also switch the active map tool, and the fourth means a right-click may execute draw or place logic before
the secondary handler runs. The code states both risks itself; what I am asserting as belief is that they
remain reachable, since nothing gates the macCatalyst build behind a feature flag.

**Suggested direction.** Either implement the four input guards -- a `UIResponder.IsFirstResponder` walk
for focus, a native `UIPinchGestureRecognizer` for the centroid, an explicit secondary-button check -- or
make the macCatalyst target's status explicit in the repository so the gap is a decision rather than a
comment. T6 owns the `.csproj` and therefore any change to the target framework list.

### UI-022 -- The object palette swallows every exception from its WinUI theming block

```yaml
id: UI-022
layer: UI
severity: S2
category: correctness
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs:72-108
effort: XS
risk: low
independent: true
```

**What.** The `ComboBox.Loaded` handler builds a WinUI `ResourceDictionary`, assigns it into
`ThemeDictionaries["Light"]`, and walks the visual tree, all inside `try { ... } catch { /* non-critical
visual styling -- fail silently */ }`. The `catch` has no exception filter and no type.

**Why it matters.** I believe an unfiltered, empty `catch` over a block this size hides far more than the
visual styling the comment scopes it to: `SetTextTrimmingOnChildren` recurses eight levels through the
visual tree, and an `OutOfMemoryException`, a `StackOverflow`-adjacent failure, or a genuine bug in the
recursion is caught identically to a missing resource key. I also believe the swallow is what would make
the `ThemeDictionaries` assignment fail invisibly -- a failure mode the sibling file at
`Platforms/Windows/App.xaml.cs:62-68` documents having already been bitten by once.

**Suggested direction.** Narrow the `try` to the single statement that can plausibly fail, catch a specific
exception type, and log rather than discard -- `Microsoft.Extensions.Logging.Debug` is already registered
in `MauiProgram.cs`.

### UI-023 -- The two palettes build the same 48x48 chip with duplicated construction code

```yaml
id: UI-023
layer: UI
severity: S3
category: duplication
authority: practice
citation: Fowler, Refactoring 2e, ch. 3 "Duplicated Code"
locations:
  - src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs:240-290
  - src/DungeonMaster.App/Views/DungeonDetailPage/SecondaryPaletteView.xaml.cs:32-87
effort: S
risk: low
independent: true
```

**What.** `BuildThumbButton` and the body of `Populate` each construct a `Border` of the same size, padding,
surface, stroke, and corner radius; each resolves the object's image or falls back to a one-character
`Label`; each sets the tooltip and semantic description from the same display name; each attaches a tap
recogniser and calls `PaletteThumb.AttachHover`. The two blocks differ only in the image margin (2 versus
3) and in which event they raise.

**Why it matters.** Fowler's Duplicated Code, with the drift already visible: the image margin differs by
one device-independent unit between the two palettes for no stated reason, which is exactly the divergence
`PaletteThumb` was extracted to prevent for the hover colours. The comment about the tooltip being the
chip's only text alternative is itself duplicated verbatim in both files.

**Suggested direction.** Extend `PaletteThumb` -- which already owns the shared hover treatment -- with a
`Build(ObjectKind kind, Action<ObjectKind> onSelected)` factory and have both palettes call it, so the chip
exists once.

### UI-024 -- Three view types carry no XML documentation on their public members

```yaml
id: UI-024
layer: UI
severity: S4
category: docs
authority: conventions
citation: backend/dotnet-format.md#9. XML Documentation Comments
locations:
  - src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs:7-21
  - src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs:372-382
  - src/DungeonMaster.App/Views/DungeonDetailPage/SecondaryPaletteView.xaml.cs:7-20
  - src/DungeonMaster.App/Views/DungeonDetailPage/ControlsPaletteView.xaml.cs:6-21
effort: S
risk: low
independent: true
```

**What.** `ObjectPaletteView` (public type, one public property, five public events, two public methods),
`SecondaryPaletteView` (public type, two public events, one public method), and `ControlsPaletteView`
(public type, two public events, two public methods) carry no `///` comments at all. Every other public
type in this scope -- including `ToolTileView` in the same folder -- is fully documented.

**Why it matters.** `dotnet-format.md` section 9 opens with "All public types, members, and parameters must
have XML documentation comments." These are the only public API surfaces in the layer that do not comply,
and the gap is concentrated on the types with the most implicit contract: nothing states that
`ActiveObjectChanged` can carry `null` to mean cleared, or that `Populate` must be called before the
secondary palette is shown.

**Suggested direction.** Document the three types and their public members in the house style already used
by `ToolTileView.xaml.cs`, giving the nullable event payloads and the call-order requirement on `Populate`
explicit prose.

**[AMENDED 6b-xix, 2026-10-12 -- the defect is REAL and was acted on in full, but this entry MIS-STATES IT
IN FOUR WAYS, one of its cited locations DOES NOT EXIST, and its layer-wide universal is FALSE.
Classification: CONFIRMED in substance; ERROR for the nonexistent span, for the event count, for the
"no doc comments at all" claim and for the layer-wide universal; DRIFT for two further spans. The
folder-level universal, by contrast, HOLDS.** Form follows the `AMENDED 6b-xvi` block on `UI-003` at
`:201`. Every line below was re-derived by content search on the post-change tree.

*Correction 1 -- a cited span that does not exist.* Previous wording, verbatim:
`src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs:372-382`. **Those lines did not
exist: the file was 370 lines at this session's base.** The file is **412** lines post-change.

*Correction 2 -- the other three spans: one exact, two stale.* `ControlsPaletteView.xaml.cs:6-21` was
**CONFIRMED exact** -- type at `:6`, last public member `ResetScale` at `:21`.
`SecondaryPaletteView.xaml.cs:7-20` was **STALE to `:5-18`**, because the type declaration sat at `:5`,
outside the cited span. `ObjectPaletteView.xaml.cs:7-21` was **STALE**: the type declaration at `:7` was
right, but the span ended on the summary **opener** of the one event that was already documented, where the
public surface ran `:7` to `:358`.

*Correction 3 -- the event count, and the arithmetic that explains the mistake.* Previous wording,
verbatim: `ObjectPaletteView` has "five public events". **It has SIX.** Post-change they are at `:25, :30,
:36, :41, :46` and `:53`. **The sixth is the one that WAS documented**, which is why the entry's figure is
five: it appears to have counted the *undocumented* events and reported the figure as the event total.

*Correction 4 -- "carry no `///` comments at all", which is FALSE for one of the three files.* Previous
wording, verbatim: all three "carry no `///` comments at all." **FALSIFIED for `ObjectPaletteView`**, and
CONFIRMED for the other two. That file carried **ten** doc-marker lines at base, in two blocks: `:21-25`
documenting the `LoadFailed` event, and `:275-279` on a private member. **This created a real clobbering
risk, and it is the specific failure the acceptance criteria for this work were written to prevent** -- a
fresh block per member would have overwritten the existing `LoadFailed` summary. **That block was preserved
byte for byte**, snapshotted before the first edit and compared after, with `md5sum` identical on both
sides and `diff` empty, confirmed independently against the base blob.

**The two zeros on the other two files were CONTROLLED.** The doc-marker count returned 0 on
`ControlsPaletteView.xaml.cs` and `SecondaryPaletteView.xaml.cs` while the same predicate returned
non-zero on the five other files in the same folder -- the figures 10 / 20 / 45 / 57 / 103 are ATTRIBUTED
to this session's `measurement.md` section 6. **A zero with no control is indistinguishable from a blind
predicate; these were not.** Post-change, measured here with `grep -c '///'` over every `.cs` file in
`Views/DungeonDetailPage/`: `ControlsPaletteView` **21**, `DrawSessionResultGuard` **20**,
`DungeonDetailPage` **103**, `ObjectPaletteView` **46**, `PaletteThumb` **57**, `SecondaryPaletteView`
**24**, `ToolTileView` **45**.

**THE POST-CHANGE DOCUMENTED-SITE COUNT, with its predicate and both its validation cases.** The public-site
predicate `grep -cE '^[[:space:]]*public[[:space:]]'` returns **11 / 5 / 6 = 22** public sites across the
three files; **22 minus the one already-documented `LoadFailed` event = 21 newly documented sites**, split
10 + 5 + 6. **The site total is 21 and not 20, because this session's own `measurement.md` section 6
OMITTED THE PUBLIC CONSTRUCTOR** from its enumeration of one property, six events and two methods; the
constructor is public, was undocumented, and is now at `ObjectPaletteView.xaml.cs:59`. **The omission was
internal inconsistency rather than a different convention**, since that same section's
`SecondaryPaletteView` enumeration does include that type's constructor. The predicate is validated in both
directions on the same run: **must-match**, `ObjectAssetCatalogue.cs:14` `Paths` is an undocumented public
member and is counted; **near-miss**, `ObjectPaletteView.xaml.cs:19` `ActiveObject` is documented -- its
immediately preceding line is `</summary>` -- and the three palette views return **0** undocumented sites
each under an undocumented-site predicate, so documented members are correctly not counted.

**THE LAYER-WIDE UNIVERSAL IS FALSIFIED, AND IT IS RECORDED HERE AS FALSE RATHER THAN REPEATED.** Previous
wording, verbatim: "These are the only public API surfaces in the layer that do not comply."
**FALSE.** At this session's base there were **33 undocumented public sites across 9 files** in
`src/DungeonMaster.App`, of which **12 were OUTSIDE the three cited views -- so the universal was false by
12 sites.** Those figures are ATTRIBUTED to `measurement.md` section 6; **this block did not reproduce them
and says so**, because its own validated predicate is not the same instrument. What this block measures
post-change, with the predicate and controls described above, is **11 undocumented public sites across 5
files**: `Assets/ObjectAssets/ObjectKindInfo.cs` 4 (`:5`, `:24`, `:76`, `:118`),
`Assets/ObjectAssets/ObjectPackRegistry.cs` **4** (`:6`, `:8`, `:11`, `:54`),
`Assets/ObjectAssets/ObjectAssetCatalogue.cs` 1 (`:14`), `Controls/NotebookView/NotebookView.xaml.cs` 1
(`:85`), and `Platforms/MacCatalyst/Program.cs` 1 (`:6`). **Either way the universal is false and the
direction is the same: public sites in this layer remain undocumented outside the three views, and no row
covers them.** They are carried to `.dir-sdlc/backlog.md`.

**A CORRECTION TO THIS BLOCK'S OWN PREDICATE, and the defect is worth the words because the predicate's
whole purpose is that no undocumented public member is lost from the record.** An earlier draft returned
**10 across 5 files** with `ObjectPackRegistry.cs` at **3**, and it **dropped a real undocumented public
method**: `ObjectPackRegistry.cs:11`,
`public static async Task<PackLoadResult?> LoadPackAsync(string packId, CancellationToken ct = default)`.
**The cause was the attribute test.** The predicate skips a `public` line whose nearest preceding non-blank
line opens with `[`, so as not to count a member documented above its attributes -- but `:11`'s preceding
non-blank line is `:9`, `[("2_Minute_Tabletop", "2 Minute Tabletop")];`, a **collection-expression
continuation of the property initialiser at `:8`**, which opens with `[` and was therefore **misread as an
attribute**. **The fix is to require a COMPLETE attribute**, `^[[:space:]]*\[.*\][[:space:]]*$`: a real
attribute line closes its `]` and ends there, while the continuation ends `];` and is a statement. **This
class of error is why the validation cases matter more than the figure.** The earlier draft's two stated
cases -- a must-match on `ObjectAssetCatalogue.cs:14` and a near-miss on a documented member -- **both
passed, and NEITHER exercised this shape**, so the controls could not have caught it: a control that does
not match the defect class licenses nothing. **The corrected predicate is validated on four cases in the
same run**, the first of which is now the defect shape itself: **must-match** `ObjectPackRegistry.cs:11`,
the `public` line preceded by that collection-expression continuation, counted; **must-match**
`ObjectPackRegistry.cs:8`, a `public` line preceded by a bare `{`, counted; **near-miss**
`CreateDungeonViewModel.cs:21`, preceded by the genuine attribute
`[NotifyCanExecuteChangedFor(nameof(CreateCommand))]` at `:20`, **not** counted; **near-miss** every
`public` member in that same file, each preceded by a `///` line, **none** counted. **And the
`ObjectPackRegistry.cs` figure of 4 is corroborated by a second route that could not share the first's
mistake**, since it never looks at a preceding line at all: `grep -nE '^[[:space:]]*public[[:space:]]'`
returns exactly `:6`, `:8`, `:11` and `:54`, and `grep -cE '^[[:space:]]*///'` returns **0** on that file,
so all four `public` members are undocumented and the two routes agree at 4.

**THE FOLDER-LEVEL UNIVERSAL, by contrast, HOLDS, and it held at n = 5 when the entry was written.**
Previous wording, verbatim: "Every other public type in this scope -- including `ToolTileView` in the same
folder -- is fully documented." There were five public types in `Views/DungeonDetailPage/`, and
`DungeonDetailPage` and `ToolTileView` each returned zero undocumented sites from a predicate that returned
six on `ControlsPaletteView`. **Post-change the whole folder returns zero undocumented public sites on all
seven of its `.cs` files**, by this block's own validated predicate.

**NOTHING HERE IS COMPILER-CHECKED, and that is measured rather than assumed.** A recursive search for
`GenerateDocumentationFile` across `*.csproj`, `*.props` and `*.targets`, with `obj` and `bin` excluded,
**exits 1 with no output**, while the **CONTROL** search for `TargetFramework` over the same file set
returns **9** files. **So the search reaches the build files and the absence is real: a missing tag raises
no warning, and a malformed `cref` raises none either, in both directions.** One figure corrected in
passing: this session's evidence base records the control as "9 of the 10 build files that exist"; **the
denominator is 11**, enumerated here by `find` with the same exclusions -- the two `Directory.*.props`
files are the two without a `TargetFramework`.

**THE THREE PROSE PREMISES THE NEW DOCUMENTATION RESTS ON, all three measured, because NO COMMAND CAN
VERIFY THAT PROSE IS TRUE.** That is why this row's criteria named a party with authority over the prose's
accuracy rather than leaving it to a predicate: 6b-xviii committed a known-false sentence in a new XML
summary and had to repair it, and review classified that advisory rather than blocking -- correctly,
because no acceptance criterion covered whether a doc comment was true.

- **`ActiveObjectChanged` carries `null` to mean cleared, and it arrives by TWO paths, not one.** Of its
  three invoke sites, `:394` passes the kind while **`:403` and `:409` both pass `null`** -- the first from
  `ClearActiveObject` at `:400`, the second from the `OnClosePalette` handler -- each after setting
  `ActiveObject = null`. **The prose names both.** The consumer reads it as cleared at
  `DungeonDetailPage.xaml.cs:1239`, `ControlsPalette.IsVisible = kind is not null`. **The entry's own
  wording implied a single path, and the criterion governing this work cited that consumer at `:1219`,
  which had already drifted to `:1239` under a peer's edit during this very session** -- the citation went
  stale inside the session that wrote it, which is this row set's finding reproduced in miniature.
- **`Populate`'s call-order requirement is REAL BUT SOFT, and this is the trap the entry walks into.**
  Previous wording, verbatim: "nothing states that ... `Populate` **must** be called before the secondary
  palette is shown." **There is no throw and no guard.** Skipping the call renders a blank, untitled panel.
  **So a doc comment saying `Populate` "must" be called, in a way implying an exception, would be FALSE**,
  and the prose therefore states it as a **consequence rather than an obligation** -- the word "must"
  appears nowhere in it. `Populate` is declared at `SecondaryPaletteView.xaml.cs:43` and its **sole caller
  is `ObjectPaletteView.xaml.cs:380`**, two lines before the `SecondaryPaletteRequested` invoke at `:382`.
- **`Populate` is RE-ENTRANT across repeat opens**, because it clears the grid's children and row
  definitions on entry -- `ObjectGrid.Children.Clear()` at `SecondaryPaletteView.xaml.cs:46` and
  `ObjectGrid.RowDefinitions.Clear()` at `:47`, before anything is added. **True, useful, and absent from
  the entry.**

**TWO DEVIATIONS ARE RECORDED RATHER THAN FIXED.** The preserved `LoadFailed` block **itself breaks one
sentence across `///` lines, contrary to section 9 at `dotnet-format.md:378`**; byte-for-byte preservation
was required, so this work could not repair it, and it is flagged here so a later conventions sweep does
not attribute it to this change. And one new sentence is **true only through a non-local chain** --
"lists every object of a single category" holds because `ObjectPackRegistry` builds its categories by
grouping the asset-paths dictionary, so no kind in a category is missing from `Paths` -- which was verified
and judged sound, **while noting the sentence becomes false the day that derivation changes, and nothing
local to the file would reveal it.**]

### UI-025 -- MapTool's enum members use single-line summary comments

```yaml
id: UI-025
layer: UI
severity: S4
category: docs
authority: conventions
citation: backend/dotnet-format.md#9. XML Documentation Comments
locations:
  - src/DungeonMaster.App/ViewModels/MapTool.cs:8-24
effort: XS
risk: low
independent: true
```

**What.** All six members are documented on a single line, with the opening tag, the description, and the
closing tag sharing one `///` marker. The `Draw` member at line 8 is representative: a `<summary>` tag, the
words "Draw tool", an em dash, "click to place polyline vertices.", and the closing tag, all on that line.
Descriptions are described rather than quoted here, because this document is ASCII-only and the source uses
em dashes.

**Why it matters.** `dotnet-format.md` section 9 states that tag descriptions "go on a **new line**,
indented with **two tabs** from the `///` marker", and every worked example in that section places
`<summary>`, the description, and `</summary>` on three separate lines. The enum's own type-level summary
at lines 3-5 already follows that form, so the file is internally inconsistent as well as non-conforming.

**Suggested direction.** Expand the six member comments to the three-line form the file's own type comment
already uses.

**[AMENDED 6b-xix, 2026-10-12 -- the core defect is CONFIRMED 6 of 6 and was acted on in full; the file
path is STALE BY PROJECT; one authority gloss is overstated; and the remedy's own wording would have
UNDER-INDENTED all six edits. Classification: CONFIRMED in substance, DRIFT by project for the citation,
ERROR for the "every worked example is three lines" gloss, and ERROR for the remedy's mechanical
instruction.** Form follows the `AMENDED 6b-xvi` block on `UI-003` at `:201`.

*Correction 1 -- the `locations:` path, stale BY PROJECT rather than by line.* Previous wording, verbatim:
`src/DungeonMaster.App/ViewModels/MapTool.cs:8-24`. **That path does not exist.**
`find src -name 'MapTool*'` with `obj` and `bin` pruned returns **exactly one path**,
**`src/DungeonMaster.App.Core/Editor/MapTool.cs`**, and the type was moved there by `3d34a69`
("refactor(app): extract editor logic to App.Core layer"); it was originally added at the cited path by
`3b52377`. The cited span `8-24` was correct at the new path before this session's work. **The row stays
filed under UI**, following this arc's own precedent in both directions: **this file's `UI-037` already
cites an `App.Core` path in its `locations:` block, at `:2426`,
`src/DungeonMaster.App.Core/Map/MapDrawingSession.cs:8-24`** -- a pre-existing citation rather than
amendment prose, which is what makes it a precedent for a `locations:` path; and `MASTER-114`, filed under
CORE, records one of its own citations as stale by project into `App.Core` without refiling the row.
**An earlier draft of this sentence cited `:1393` for that precedent, and the anchor was wrong while the
claim was true** -- `:1393` holds no path at all. Both the corrected anchor and the entry that owns it were
re-derived by content search on the post-change tree, after this session's own insertions had landed, by
searching the file for `DungeonMaster.App.Core/` and mapping each hit back to its nearest preceding
`### UI-0` heading. **A second instance exists inside this session's own prose**, `UI-026`'s block at
`:1425` citing `App.Core/Editor/DrawState.cs:6`, but it is deliberately not offered as the precedent: prose
this session wrote cannot establish the arc's prior practice.

*Correction 2 -- the "every worked example" gloss, which is overstated.* Previous wording, verbatim:
"every worked example in that section places `<summary>`, the description, and `</summary>` on three
separate lines." **Two of the three examples are three lines; the example at `dotnet-format.md:399-402` is
FOUR**, because `:378` requires a line break at each sentence boundary and that example's description is
two sentences -- `:400` and `:401` each carry one, read directly from the conventions file. **This does not
weaken the row**, and the reason is worth stating so the correction is not mistaken for a retraction: **all
six `MapTool` descriptions are single sentences**, so `:378` permits no break in any of them and the
three-line form is the right target here.

**The entry's authority citation is CONFIRMED VERBATIM.** `dotnet-format.md:375` reads "Tag descriptions go
on a **new line**, indented with **two tabs** from the `///` marker", read directly from the conventions
file. The entry's observation that the file was internally inconsistent is also confirmed: the type-level
summary was already in the three-line form, with `:4` carrying `///` followed by exactly two tabs.

*Correction 3 -- the remedy's mechanical instruction, which would have broken all six edits, and the
correct BYTE FORM.* Previous wording, verbatim: expand them "to the three-line form the file's own type
comment already uses." **Pattern-matching the type comment UNDER-INDENTS all six**, because the type
comment sits at column 0 while the members sit one tab inside the enum body. **The correct byte form is one
leading TAB, then the `///` marker, then TWO tabs, then the description** -- that is, section 9's two tabs
are measured **from the marker**, and inside a type body the marker itself is already one tab in.
**This session's own evidence base compressed that into the phrase "three tabs of leading whitespace",
which reads as three tabs BEFORE the marker and is wrong; the arithmetic was right and the summary phrase
was the wrong compression.** The authority is the byte form, not the phrase. The in-repo worked example of
the correct member form is **`ViewModels/CreateDungeonViewModel.cs:16-18`** -- `:16` `/// <summary>`, `:17`
`///` followed by exactly two tabs and then the description, `:18` `/// </summary>`, with one leading tab on
each. Located by `grep -nE "^${TAB}///${TAB}${TAB}"` with `TAB` interpolated from `printf`, which returns
`:17` as the first hit in that file; `:16` and `:18` were then read directly rather than derived by
subtracting from a span.
**A CORRECTION TO AN ANCHOR THIS SESSION ITSELF MOVED, recorded because it is the cleanest example in the
row set of the very drift this entry is about.** An earlier draft of this paragraph cited `:14-16`, taken
from this session's evidence base, where it was measured **before** the session's own work landed. It was
correct then. **It is wrong now because a SIBLING TASK of this same session wrapped that type's constructor
and added two lines above the comment**: `:12` now opens
`public partial class CreateDungeonViewModel(`, `:13` carries `AppDispatcher dispatcher)`, `:14` carries
`: ObservableObject` and `:15` is the opening `{` -- so the member comment moved from `:14-16` to `:16-18`,
a **+2** shift. **This is intra-session drift, not inherited staleness:** the span was accurate at the
session base and was invalidated by a change made in parallel beside it, which is the one case a
"re-derive against the base" habit does not catch and only a re-derivation against the POST-change tree
does.

**The post-change state, measured with predicates validated in both directions.** The six members are now
at **`:11, :16, :21, :26, :31, :36`**, re-derived by
`grep -nE "^[[:space:]]*(Draw|Move|Pan|Place|Select|Erase),"`, and the file is **37 lines**. The
single-line-form census, `grep -cE '///[[:space:]]*<summary>.*</summary>'`, returns **0** on this file --
**and that zero is CONTROLLED: the same predicate returns 3 on `App.Core/Editor/DrawState.cs`**, an
independent unedited file, so it is a measured negative rather than a blind predicate. The correct byte
form returns **6** by `grep -cE "^${TAB}///${TAB}${TAB}"` with `TAB=$(printf '\t')`, while **three tabs
after the marker returns 0**, so the one wrong reading that mattered is excluded by the bytes.

**THE TAB-IN-GREP HAZARD WAS REPRODUCED LIVE ON THIS VERY FILE**, which is why every tab predicate above
uses the interpolated form: with the backslash-escaped `\t`, `grep -E` matches a **literal `t`** and the
same intended predicate returns **0** where the interpolated form returns 6. **Same file, same intended
predicate, 6 versus 0, and the wrong one fails silently with a plausible number.**

**The six descriptions were re-indented, not re-authored**, and the six em dashes survive: a high-byte
census over the file -- `od -An -tx1 -v` piped to a count of bytes at or above `0x80` -- returns **18**,
exactly six em dashes at three bytes each. **That instrument replaced a wrong one found in this session:**
`grep -c '[^ -~]'`, used elsewhere as a non-ASCII census, counts **tabs**, because the tab is outside the
printable-ASCII range, so in a tab-indented repository it catches every indented line and can return a
spurious *presence*. It is the first instrument in this arc's register whose failure mode is a false
positive confirming a false claim.

**SCOPE THE ROW DOES NOT ADDRESS, recorded as a population rather than folded into this row.** The same
validated single-line-form predicate over all of `src/`, with `obj` and `bin` excluded, returned **76**
such comments across four projects at this session's base -- `DungeonMaster.App` 35, `App.Core` 9,
`App.Tests` 7, `App.UiTests` 25. **Post-change it returns 70: 35 / 3 / 7 / 25**, and the two figures
reconcile exactly, because `App.Core` went from 9 to 3 and **this row's six are the difference.** So
`MapTool`'s six were a fraction of the population: **the row scopes itself to one file while the convention
it cites is project-wide, and 70 comments across four projects remain.** That remainder is carried to
`.dir-sdlc/backlog.md` rather than claimed as closed here.]

### UI-026 -- A public record and a private enum are nested inside larger types

```yaml
id: UI-026
layer: UI
severity: S4
category: style
authority: conventions
citation: backend/dotnet-format.md#2. File Organization
locations:
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:317
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:40-41
  - src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs:333-335
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:75
effort: S
risk: low
independent: true
```

**What.** `PlaceObjectArgs` is a public record declared inside `DungeonDetailViewModel` and referenced from
the page as `DungeonDetailViewModel.PlaceObjectArgs`. `DrawState` is an enum declared mid-class in the page.
`ObjectPaletteView` declares three fields at line 333, between two methods and 320 lines below its other
fields. `DungeonDetailViewModel` declares `_dungeonId` at line 75, after all its properties.

**Why it matters.** `dotnet-format.md` section 2 states "One type per file (class, record, interface, enum)"
and "File name matches the type name exactly". `PlaceObjectArgs` is the clearest case: it is a public type
on this layer's API surface, consumed from another file, and it has no file of its own. The mid-class field
declarations are a readability cost rather than a rule breach, but they are why the palette's secondary-view
state is easy to miss when reading `OpenSecondaryPalette`.

**Suggested direction.** Give `PlaceObjectArgs` its own file beside the view model, move `DrawState` to its
own file or leave it private and grouped with the other field declarations, and gather each type's fields
into one block at the top.

**[AMENDED 6b-xix, 2026-10-12 -- this entry is FALSIFIED, and was RE-SCOPED by user escalation E1 to the
whole measured nine-record population. Three of its four factual claims are false, ALL FOUR of its cited
locations are wrong or stale, and one clause of its remedy is IMPOSSIBLE -- it would have broken the build
of `DungeonMaster.App`. Classification: ERROR throughout the specifics; the underlying section 2 concern is real
for exactly one of the four locations, and WIDENED from one record to nine by explicit decision.** Form
follows the `AMENDED 6b-xvi` block on `UI-003` at `:201`. Both constructs the entry describes were moved
out from under it by `c5b5ccf` ("refactor(app): split DungeonDetailViewModel into a composition root") and
`3d34a69` ("refactor(app): extract editor logic to App.Core layer"). Every line below was re-derived by
content search on the post-change tree.

*Correction 1 -- ALL FOUR cited locations are wrong or stale.* Previous wording, verbatim:
`DungeonDetailViewModel.cs:317` and `DungeonDetailPage.xaml.cs:40-41`. **`DungeonDetailViewModel.cs:317`
is past the end of a file that is 202 lines.** `DungeonDetailPage.xaml.cs:40-41` holds floor-drag fields
and a comment, **not an enum**. Previous wording, verbatim: `DungeonDetailViewModel.cs:75` as the
`_dungeonId` declaration -- **`_dungeonId` is at `:34`**, and `:75` is inside the `ErrorMessage` property
region.

**The FOURTH citation, which an earlier draft of this block left standing uncorrected.** Previous wording,
verbatim: `ObjectPaletteView.xaml.cs:333-335`, described in the `What.` paragraph as "three fields at line
333, between two methods and 320 lines below its other fields". **The three fields are real and the
reading is right; the line numbers are stale.** At this session's base `:333` is `};`, `:334` is `}` and
`:335` is `else if (_secondaryPaletteHandler is not null)` -- the tail of a lambda and the start of a
branch, not a field in sight. **The three fields are at `:313-315`**, located by content on their
declarations: `:313` `private SecondaryPaletteView? _secondaryPalette;`, `:314`
`private EventHandler<ObjectKind>? _secondaryPaletteHandler;`, `:315`
`private string? _currentSecondaryCategory;`. **The entry's structural observation is CONFIRMED by those
anchors:** the type's other field declarations are `:9` `private IReadOnlyList<string> _categoryOrder = [];`
and `:10`, a wrapped declaration opening
`private IReadOnlyDictionary<string, IReadOnlyList<ObjectKind>> _categories =`; the three sit far below both
and between the methods at `:283` and `:317`, exactly as the entry describes. **All of those lines are
cited, and the distance between them deliberately is not** -- the entry's own "320 lines below" is a
subtraction, and this block anchors on the boundary lines instead. **The field census is enumerated rather
than counted by predicate:** `grep -nE '^<TAB>(private|public|internal|protected)'` with the tab
interpolated lists every modifier-opening member line of the type, and the five field declarations among
them -- `:9`, `:10`, `:313`, `:314`, `:315` -- were identified by eye from that listing. **An earlier draft of this paragraph called `:9` the only other field**, which
`:10` falsifies; the first predicate written for this census returned **0** on a file that plainly has five
fields, and was discarded as blind rather than believed.
Post-change the same three fields are at **`:346-348`** with the other at **`:12`**, re-derived by the same
content search on the post-change tree. **ATTRIBUTION, stated because it bounds the claim:** the base
positions were read from the orchestrator's base snapshot of this file rather than from history, which a
task that cannot run git could not re-derive; the snapshot is corroborated as unchanged at this session's
base by its file length of **370** lines, the figure `UI-024`'s block independently carries for the base,
and by its `foreach` at `:33`, the anchor `UI-028`'s block independently carries for the base.

*Correction 2 -- `PlaceObjectArgs` was never nested, and the page's reference to it was never qualified.*
Previous wording, verbatim: "`PlaceObjectArgs` is a public record declared inside `DungeonDetailViewModel`
and referenced from the page as `DungeonDetailViewModel.PlaceObjectArgs`." **It was top-level, in a
DIFFERENT FILE** -- `ViewModels/MapCanvasViewModel.cs`, where `cat -A` showed the declaration line
beginning `public` with **no leading tab** -- and **the page's reference was UNQUALIFIED.** So the breach
was real but **not for the stated reason**: it was a public type co-located in a differently-named file,
never a nested type. **The nested-type census returns 0 on BOTH cited files**, by
`grep -cE "^<TAB>+(public|private|internal|protected).*(class|record|enum|struct|interface)[[:space:]]"`
with the tab interpolated via `TAB=$(printf '\t')`, because `\t` inside `grep -E` matches a literal `t`.
**Those two zeros are CONTROLLED: the same predicate returns non-zero where nested types genuinely exist**
-- `Controls/NotebookView/NotebookView.xaml.cs:17` `private enum NotebookPane` and `:80`
`private sealed class RibbonBrushes`.

*Correction 3 -- `DrawState` is STRUCK AS ALREADY SATISFIED, and one clause of the remedy is impossible.*
Previous wording, verbatim: "`DrawState` is an enum declared mid-class in the page", and the remedy's "move
`DrawState` to its own file **or leave it private**". **`DrawState` is a top-level, fully documented
`public enum` in its own correctly-named file in a different project**, `App.Core/Editor/DrawState.cs:6`.
So it is **already compliant on all three of section 2's rules**, and the remedy clause has nothing to act
on: **`private` is NOT EXPRESSIBLE for a top-level type**, and narrowing it to `internal` would break
**`DungeonMaster.App`**.
**A CRITERION-PREMISE CORRECTION, because a correction is new prose and gets no exemption:** the frozen
criterion mandated that `internal` would break "two projects and 26 test assertions", and an earlier draft
of this paragraph wrote that verbatim. **It breaks ONE project, and the 26 assertions still compile.**
`DrawState` is declared in `DungeonMaster.App.Core`, and
`src/DungeonMaster.App.Core/AssemblyInfo.cs:3` is `[assembly: InternalsVisibleTo("DungeonMaster.Tests")]`,
read directly -- **so `DungeonMaster.Tests` already sees that project's `internal` types** and the **26**
assertions in `src/DungeonMaster.Tests/AppCore/Editor/MapEditorStateTests.cs` would be unaffected. The
consumers are two projects by file count -- `grep -rn 'DrawState'` over `src` with `/obj/` excluded returns
19 lines in `DungeonMaster.App`, 12 in `DungeonMaster.App.Core` and 26 in `DungeonMaster.Tests` -- but
**only `DungeonMaster.App` has no `InternalsVisibleTo` grant from `App.Core`**, and it is therefore the sole
breakage. **The remedy clause is still impossible and the direction of the finding is unchanged**; what
changes is the blast radius, and it changes DOWNWARD, which is the direction a reader is least likely to
check. **The 26 is a real figure about a real file and is kept** -- it is simply not evidence of breakage.
**This is the same
shape as 6b-xviii's "Delete the line": an instruction that reads as a one-liner and breaks the build and
the tests.** The correct action was to strike the clause, which is what was done.

*Correction 4 -- `_dungeonId` was already in the state the remedy asks for.* Previous wording, verbatim:
"`DungeonDetailViewModel` declares `_dungeonId` at line 75, after all its properties." Its fields sit at
`:22, :26, :30, :32, :34`, its constructor at `:47`, and its first property at `:67`. **The field block was
already gathered at the top, ahead of the constructor and every property -- the state the remedy asks for
already existed.** The entry's "320 lines below its other fields" figure for the `ObjectPaletteView` block
**was not reproduced, because it is a subtraction**; both positions were instead measured directly.
**Section 2 covers exactly ONE of the four locations.** The `ObjectPaletteView` field ordering and
`_dungeonId` are **not section 2 matters at all** -- the entry's own hedge, "a readability cost rather than
a rule breach," is correct, and **the section 2 citation does not belong on them. The field-ordering point
is recorded here as a READABILITY NOTE CARRYING NO SECTION 2 CITATION.**

**ALL NINE co-located records were extracted, not just the one the entry names, and that was an explicit
decision rather than scope creep.** `PlaceObjectArgs` sat with **six sibling records** in
`MapCanvasViewModel.cs` and two more in `FloorListViewModel.cs`; **extracting only `PlaceObjectArgs` would
have left six identical section 2 breaches in the same file and made it LESS consistent, not more.** Either
all or none, and the user chose all. The nine new files, each re-derived by content search on its own
declaration and each holding exactly one type named for its file:

- `ViewModels/AddAreaWithOutlineArgs.cs:10` · `ViewModels/MoveAreaOutlineArgs.cs:11`
- `ViewModels/PlaceObjectArgs.cs:14` · `ViewModels/RemoveAreaArgs.cs:8`
- `ViewModels/RemovePlacedObjectArgs.cs:9` · `ViewModels/ReorderFloorArgs.cs:8`
- `ViewModels/UpdateAreaArgs.cs:12` · `ViewModels/UpdateFloorArgs.cs:11`
- `ViewModels/UpdatePlacedObjectArgs.cs:20`

**Each of the nine names resolves to exactly one declaration in `src`**, by
`grep -rlE "record[[:space:]]+<Name>([^A-Za-z0-9_]|\$)"` -- **and the trailing `|$` is load-bearing rather
than decorative.** Without it the predicate returns 0 for a type whose declaration line **ends at the type
name**, because `grep -E` has already stripped the CR and the trailing character class has nothing left to
match. That is the **exact mirror** of this arc's catalogued `[^.]`-consumes-the-CR trap, and it was found
in this session by the party whose own predicate was wrong.

**The blast radius was measured, with its near-miss partition, and the extraction cost was zero call-site
edits.** `PlaceObjectArgs` post-change resolves to **3 lines in `src`**, all compiled: the declaration at
`PlaceObjectArgs.cs:14`, the consumer at `MapCanvasViewModel.cs:283`, and the page at
`DungeonDetailPage.xaml.cs:1226`. **Zero call-site edits were needed and none were made** -- that outcome is
verified, and the reason is not the one the frozen criterion gave.
**A CRITERION-PREMISE CORRECTION, because a correction is new prose and gets no exemption:** the criterion
mandated the reason "because declaration and usages share the namespace". **They do not all share it.** The
declaration is `namespace DungeonMaster.App.ViewModels;` at `PlaceObjectArgs.cs:4`, and the consumer at
`MapCanvasViewModel.cs:21` does share it -- but **the page does not**: `DungeonDetailPage.xaml.cs:20` is
`namespace DungeonMaster.App.Views;` and reaches the type through `using DungeonMaster.App.ViewModels;` at
`:12`. All four lines read directly. **The correct reason is that the extraction did not change the
namespace:** the record moved to a new FILE inside the namespace it already had, so the page's existing
`using` kept resolving it and the one same-namespace consumer needed nothing either. **The distinction
matters for the next extraction**, because the stated reason would license moving a type across namespaces
with no call-site edits, and that is false. **`PlaceObjectArgs` has
ZERO test references** -- the same command over all four test projects exits 1 with no output -- **and that
zero is CONTROLLED, because the identical command shape finds 26 `DrawState` hits in
`MapEditorStateTests.cs`, so it does reach test code.**

**ONE CORRECTION THIS BLOCK MAKES TO ITS OWN SESSION'S FROZEN CRITERIA, because a correction is new prose
and gets no exemption.** A frozen acceptance criterion for the task that did this work asserted "ZERO test
references" **of all nine records**. That was measured on `PlaceObjectArgs` alone and **is FALSE of
`ReorderFloorArgs`, which has four references across TWO test projects** -- one compiled at
`src/DungeonMaster.App.Tests/Views/FloorReorderCommandArgsTests.cs:112`, and three in prose at `:23`,
`:109` and `src/DungeonMaster.Tests/AppCore/FloorReorderDropTests.cs:78`. **The outcome is unaffected**,
because the namespace was preserved and that test file already carries the `using`, so zero edits were
still needed. **But the clause generalised from a population of one to a population of nine**, which is
this arc's own catalogued "a survey answers the population you scope it to" failure, reproduced inside a
frozen artefact rather than in the register.]

**[AMENDED 6b-xix, 2026-10-12 -- a SECOND observation on this entry, recorded as a second block because it
is a second observation: THREE CORRECTIONS to the carried type-versus-filename backlog item this row
overlaps, and the newly measured SECTION 3 SIDE-EFFECT of the extraction.** The item is in
`.dir-sdlc/backlog.md`; these corrections are recorded here because this row is where they were measured.

**Correction A -- the population figure.** The item states **14**. At this session's base the population was
**11** by a modifier-agnostic predicate and **10** by the item's own gapped one. **The item's 14 was
measured before `MASTER-123` closed the four UIKIT rows.** Post-change the population is **2**, measured
here with a predicate that strips all modifiers and the `record struct` compound before taking the type
name, and validated in both directions on the same run: **2 mismatches against 58 correctly matching
declarations**, the matching class being the near-miss that must not count. **The two survivors are
`Assets/ObjectAssets/ObjectPackRegistry.cs:54` `PackLoadResult` and `Graphics/FloorMapDrawable.cs:40`
`FloorMapPalette`.** **11 minus the 9 extracted equals 2, so the figures reconcile exactly.**

**Correction B -- "8 are in `ViewModels/`" is STALE to 9, and is now 0.** It was 9 at this session's base;
**all nine were the extracted records**, so the same predicate now returns **zero** mismatches anywhere in
`ViewModels/`.

**Correction C -- THE GAP THE ITEM NAMES IS NOT THE GAP THAT BITES, and this is the correction that
matters.** The item warns that its predicate drops `public static` and `public abstract` declarations.
Measured here: **there are no `public abstract` types in the layer at all** --
`grep -rnE '^public abstract'` over `src/DungeonMaster.App` exits 1 with no output -- and **every
`public static` type in the layer matches its filename and contributes ZERO mismatches**:
`ObjectAssetCatalogue`, `ObjectKindInfo`, `ObjectPackRegistry`, `MauiProgram` and `DmTheme`, found by
`grep -rnE '^public static (class|partial class)'`. **That is FIVE such types, not the two this session's
own evidence base recorded**, and the correction is made here with the command rather than carried.
**The modifier that actually hides a mismatch is `readonly`**, at `Graphics/FloorMapDrawable.cs:40`, whose
line reads `public readonly record struct FloorMapPalette(` -- and a predicate that takes the word after
`record` captures `struct` as the type name, which is how this block's own first predicate went wrong
before it was corrected and re-validated.

**And the membership question the row raises: `PlaceObjectArgs` IS inside that population, and `DrawState`
IS NOT and never could be, because its filename matches its type.**

**THE EXTRACTION MOVED EIGHT SECTION 3 BREACHES INTO EIGHT NEW FILES, DELIBERATELY AND VISIBLY.** Eight of
the nine extracted records declare their positional parameters **INLINE**, violating section 3 at
`dotnet-format.md:68` -- **the very rule the other half of this session's work enforces on `UI-027`.** The
8-and-1 split is taken from the report of the task that did the work, which counted it with two
complementary predicates that partition the nine, and is independently reproduced here by content search
on the nine declaration lines: **the eight inline are `RemoveAreaArgs.cs:8`, `ReorderFloorArgs.cs:8`,
`RemovePlacedObjectArgs.cs:9`, `AddAreaWithOutlineArgs.cs:10`, `MoveAreaOutlineArgs.cs:11`,
`UpdateFloorArgs.cs:11`, `UpdateAreaArgs.cs:12` and `PlaceObjectArgs.cs:14`; the one already wrapped is
`UpdatePlacedObjectArgs.cs:20`.** They were **NOT reformatted**, and the reason is recorded rather than
left implicit: they are a different population, were never measured as part of either row, and **silently
widening that diff is exactly what leaves a later register task unable to describe what happened.** The
eight are carried to `.dir-sdlc/backlog.md` as their own item.]

### UI-027 -- Primary constructor parameters and base lists are inline on all three view models

```yaml
id: UI-027
layer: UI
severity: S4
category: style
authority: conventions
citation: backend/dotnet-format.md#3. Parameter Formatting
locations:
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:27
  - src/DungeonMaster.App/ViewModels/DungeonListViewModel.cs:13
  - src/DungeonMaster.App/ViewModels/CreateDungeonViewModel.cs:11
effort: XS
risk: low
independent: true
```

**What.** Each view model is declared as
`public partial class DungeonDetailViewModel(AppDispatcher dispatcher) : ObservableObject, IQueryAttributable`
on a single line -- the primary constructor parameter inline with the declaration, and the base type and
interface list on the same line after it.

**Why it matters.** `dotnet-format.md` section 3 is unambiguous: "**Primary constructor parameters**: always
one per line, regardless of count", with a worked example wrapping a one-parameter primary constructor.
Section 6 then requires the inheritance list on its own line at +1 tab from the parameters. The correct form
for these three is a three-line declaration. Every handler in the Application layer follows this shape, so
the App layer's view models are the outliers.

**Suggested direction.** Reformat the three declarations to the section 3 and section 6 shape: declaration,
parameter at +1 tab, base list at +2 tabs.

**[AMENDED 6b-xix, 2026-10-12 -- this entry is FALSIFIED AS WRITTEN and was RE-SCOPED to the measured
four-site population. Following its remedy verbatim would have BROKEN THE BUILD. Classification: ERROR for
the quoted declaration, which does not exist in the tree, ERROR for two of its three cited locations, DRIFT
for the third, and CONFIRMED for its one universal -- which inverts this arc's usual pattern, since the
universal is the clause that survived testing while every specific collapsed.** Form follows the
`AMENDED 6b-xvi` block on `UI-003` at `:201`. Every line below was re-derived by content search on the
post-change tree.

*Correction 1 -- the quoted declaration, which is not in the tree.* Previous wording, verbatim: each view
model is declared as `public partial class DungeonDetailViewModel(AppDispatcher dispatcher) :
ObservableObject, IQueryAttributable` on a single line. **No such line exists.** The real declaration, read
directly, is at **`:20`**: `public partial class DungeonDetailViewModel : ObservableObject,
IQueryAttributable, IDungeonEditingContext` -- **it has no parameter list at all, and it carries a THIRD
interface the entry omits.** That class's constructor is a **regular** one at **`:47`**, taking
`AppDispatcher dispatcher, INavigationService navigation`. **So `DungeonDetailViewModel` has no primary
constructor, and "the correct form for these three is a three-line declaration" has no referent for it.**

*Correction 2 -- the three cited locations: one confirmed, one stale by one line, two falsified.* Previous
wording, verbatim: `DungeonDetailViewModel.cs:27`, `DungeonListViewModel.cs:13`,
`CreateDungeonViewModel.cs:11`. **`DungeonDetailViewModel.cs:27` is FALSIFIED** -- it is a string argument
inside a static field initialiser, not a declaration. **`DungeonListViewModel.cs:13` is FALSIFIED twice
over**: `:13` is a doc comment, and that type **was already correctly wrapped under section 3 before this
session touched anything** -- `:15` opens the primary constructor, `:16` carries `AppDispatcher
dispatcher,` and `:17` carries `INavigationService navigation) : ObservableObject`.
**`CreateDungeonViewModel.cs:11` is STALE to `:12`**, `:11` being the closing `</summary>`; and `:12` is
**the only one of the three cited locations where the defect actually existed.**

*Correction 3 -- "all three view models" and "the App layer's view models are the outliers".* Previous
wording, verbatim: both clauses. **1 of the 3 cited files exhibited the defect, and the real population was
FOUR, three of them uncited.** The four, each located by content search on its declaration:
`ViewModels/CreateDungeonViewModel.cs:12`, `ViewModels/DungeonHeaderViewModel.cs:19`,
`ViewModels/FloorListViewModel.cs:24`, and `ViewModels/MapCanvasViewModel.cs:34`. **The three the entry
never names are the WORSE ones: all three take two parameters, which is where section 3's "regardless of
count" bites hardest.** The "outliers" clause is directionally true but misidentifies which -- two cited
outliers were not outliers, and two real outliers went unnamed. There is also **n = 1 outside the App
project**, `src/DungeonMaster.App.Tests/Graphics/FakeAssetSource.cs:86`, a private nested test helper with
the same deviation, **deliberately left untouched** as outside this row's layer.

**The population was derived twice by two independently built predicates that agree, with one positive and
TWO negative controls -- and the two negatives are the two confusable shapes that actually occur in this
tree, which is what makes them worth running.** The predicate is an inline-primary-constructor one, and the
**modifier alternation is load-bearing** rather than decorative:

```
grep -cE '^[[:space:]]*(public|private|internal|protected).*class.*\(.*\)[[:space:]]*:'
```

Post-change it returns **0** on each of the four owned files -- `CreateDungeonViewModel.cs`,
`DungeonHeaderViewModel.cs`, `FloorListViewModel.cs` and `MapCanvasViewModel.cs`. The **POSITIVE CONTROL**
returns **1** at `FakeAssetSource.cs:86`, on
`private sealed class LabelledStream(string path, bool throwOnDispose) : MemoryStream`, and **that line is
the only instance in the whole of `src`** -- the same predicate run recursively over `src` with `obj` and
`bin` excluded returns exactly that one line. **Negative control 1** is the already-wrapped
primary-constructor shape, `DungeonListViewModel.cs:15-17`, where `:15` opens
`public partial class DungeonListViewModel(` and `:17` carries
`INavigationService navigation) : ObservableObject`; **negative control 2** is the inline-base-list shape
with no primary constructor, `DungeonDetailViewModel.cs:20`. Both return **0** against the positive
control's 1 in the same run, so **both confusable shapes are discriminated in both directions.**

**A CORRECTION TO THE INSTRUMENT AS THIS BLOCK FIRST PRINTED IT, recorded because a wrong instrument in the
permanent record is worse than no instrument.** An earlier draft printed the predicate with `public` alone
in place of the alternation, while claiming the positive control returned 1 on a `private` declaration.
**Those two statements cannot both be true, and the predicate is the half that was wrong:** the narrow form
returns **0** at `FakeAssetSource.cs:86` and **exits 1 with no output across the whole of `src`**, because
the only instance in the tree is `private`. **A predicate that returns 0 on every input discriminates
nothing**, so as printed the positive control was dead and neither negative control could mean anything
either -- every file returned 0 for the same reason. **The four zeros survive intact under the widened
form**, which is why this is an instrument correction and not a retraction: re-run with the alternation, the
four owned files are still 0, the positive control is 1, and both negatives are 0. **Widening rather than
swapping the control was forced:** there is **no `public` instance of this shape anywhere in `src`**, by the
widened predicate's single repo-wide hit being the `private` one, so no `public` positive control exists to
be found. **This is the dead-positive-control failure class, and it is the second instance in this session**
-- a control can pass review by being *stated* while never having been *run*, and the only defence is
printing the predicate beside the figure so the two can be checked against each other, which is what the
block now does.

**The entry's one UNIVERSAL is CONFIRMED, 16 of 16.** All 16 handler declarations in
`src/DungeonMaster.Application` are in the wrapped form -- `grep -rnE '^[[:space:]]*public.*class.*Handler\($'`
returns 16 -- and that figure is **corroborated by the OPPOSITE PREDICATE returning 0** for the same
project, so the 16 is not an artefact of a predicate that can only match one shape.

**WHY FOLLOWING THE REMEDY VERBATIM WOULD HAVE BROKEN THE BUILD.** An implementer who trusted the entry's
quoted line and reformatted *it* into three lines would write a primary constructor that does not exist,
**drop the third interface `IDungeonEditingContext`**, and introduce a parameter `dispatcher` colliding
with the private readonly `_dispatcher` field and with the real constructor at `:47`. **This is marked NOT
MEASURED as a compiler error**: it is a static argument from the name collision, not an observed build
failure, because no build was run for this row. **This is the second row in two sessions with that
property**, by the same mechanism as 6b-xviii's `MASTER-125`: the entry quotes a declaration line that is
not in the tree and then instructs an implementer to reformat it.

**THREE DEVIATIONS IN THE TWO FALSIFIED FILES ARE REAL BUT BELONG TO DIFFERENT RULES, and are recorded
here so they are neither lost nor silently folded into this row.** All three are outside this row's scope
and outside the ownership set of the task that acted on it:

- **`DungeonDetailViewModel.cs:20`** -- base and interface list inline with three interfaces. Violates
  **section 6 at `dotnet-format.md:240`**, and is not a primary-constructor defect.
- **`DungeonDetailViewModel.cs:47`** -- a two-parameter **regular** constructor inline. Violates
  **section 3 at `dotnet-format.md:70`**, "one per line when there are 2 or more" -- a rule this entry
  never cites.
- **`DungeonListViewModel.cs:17`** -- a FOURTH deviation, and the one most likely to be lost, because this
  file is otherwise recorded as already correct. Its base list shares the last parameter line rather than
  sitting on its own at +2 tabs: `:17` reads `INavigationService navigation) : ObservableObject`. Violates
  **section 6 at `dotnet-format.md:240`**. **So this file is correctly wrapped under section 3 and NOT
  under section 6, and must not be recorded as clean.**

**Both convention citations were CONFIRMED VERBATIM**, read directly from
`C:/Users/dmarq/Development/Repos/dm-conventions/backend/dotnet-format.md`: section 3 at `:68` --
"**Primary constructor parameters**: always one per line, regardless of count" -- with its one-parameter
worked example at `:73-76`, and section 6 at `:240`, which puts base classes and interface implementations
each on their own line at +1 tab from the previous element. **`CreateDungeonViewModel` has ONE parameter
and is wrapped anyway**, which is what "regardless of count" means and the half of the rule most likely to
be skipped. **This wave's pattern, stated because it is the finding rather than an aside: all three rows'
citations into `dotnet-format.md` were accurate, while 7 of the 7 cited SOURCE line numbers were wrong or
stale. The register was reliable about authority and unreliable about location.** The seven are `UI-026`'s
four and `UI-027`'s three, enumerated so the denominator is not a bare figure: `UI-026`'s
`DungeonDetailViewModel.cs:317` past the end of a 202-line file, `DungeonDetailPage.xaml.cs:40-41` holding
floor-drag fields rather than an enum, `ObjectPaletteView.xaml.cs:333-335` stale to `:313-315`, and
`DungeonDetailViewModel.cs:75` for a `_dungeonId` that is at `:34`; and `UI-027`'s
`DungeonDetailViewModel.cs:27` falsified, `DungeonListViewModel.cs:13` falsified twice over, and
`CreateDungeonViewModel.cs:11` stale by one to `:12`. **An earlier draft of this sentence said 5 of 7, and
it undercounted on two separate grounds:** it omitted `ObjectPaletteView.xaml.cs:333-335`, which no block
had yet corrected, and it counted only the outright-wrong ones while excluding the stale-by-one `:11`.
**The predicate is "wrong or stale", because for an implementer following a citation the two are the same
failure**, and under it the figure is 7 of 7. `UI-025`'s single citation is deliberately NOT in this
denominator: it is stale by PROJECT rather than by line, and is classified separately in that entry's own
block.]

### UI-028 -- Braces are omitted on single-statement control flow in roughly twenty places

```yaml
id: UI-028
layer: UI
severity: S4
category: style
authority: conventions
citation: backend/dotnet-format.md#4. Control Flow Braces
locations:
  - src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:302-303
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:308-321
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:326-329
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:393-395
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:401-406
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:419-420
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:443-444
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:464-465
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1159
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1213-1223
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1741-1757
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1765-1766
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:2037-2038
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:2073-2074
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:2109-2117
effort: S
risk: low
independent: true
```

**What.** Single-statement `if`, `else`, and `for` bodies are written without `{ }` at these sites. The
densest is the popup hit-zone dispatch at lines 1213-1223, a five-branch `if/else if` chain in which every
branch is an unbraced `_ =` discard, and `PolygonArea` at lines 1756-1757, whose `for` body is unbraced.
The same file uses full braces everywhere else, so the deviation is local rather than a house style.

**Why it matters.** `dotnet-format.md` section 4 states that all control flow statements "must use full
brace notation `{ }`, even when the body is a single statement. Never omit braces." The concrete cost is
highest in the five-branch chain, where adding a second statement to a branch -- an `await`, a log line, a
guard -- silently detaches it from its condition.

**Suggested direction.** Add braces at every listed site. This is mechanical and behaviour-preserving; it
would be caught automatically if the repository gained an `.editorconfig`, which is T6's to decide.

**[AMENDED 6b-xix, 2026-10-12 -- NEARLY EVERY CLAUSE OF THIS ENTRY WAS STALE, with only 1 of its 15 cited
spans still containing a braceless site, and yet its substantive claim -- "roughly twenty places" -- is
CONFIRMED and the population reconciles arithmetically. Classification: DRIFT for ten spans, ERROR for five
that name eliminated sites, DRIFT for the `for` clause, ERROR for the discard clause, and NARROWED for the
entry's universal, which holds for its own file and is FALSE as a project-wide generalisation. One of the
ten drifted spans, `DungeonDetailPage.xaml.cs:401-406`, nevertheless still contains a live braceless site,
so 14 of the 15 point at nothing rather than all 15.** Form follows the
`AMENDED 6b-xvi` block on `UI-003` at `:201`. The row was acted on in full and the scope was deliberately
extended beyond it; see the reconciliation below.

*Correction 1 -- 14 of the 15 `locations:` spans point at nothing.* **A CRITERION-PREMISE CORRECTION,
because a correction is new prose and gets no exemption:** the frozen criterion for this block mandated
"0 of 15 cited spans valid", and an earlier draft of this paragraph wrote that as "ZERO of the measured
braceless sites fell inside any of the 15 cited spans". **It is 1 of 15.** The exception is
`DungeonDetailPage.xaml.cs:401-406`: at this session's base `:402` is a braceless `else` with its body at
`:403`, both inside the cited span, and `:400` is a braceless `if` whose body at `:401` is inside it too.
Read from the base blob and classified by this block's own detector, which places braceless sites at `:400`
and `:402`. **So 14 of the 15 spans point at nothing and one is still live** -- which is a materially
different statement for an implementer, because it means the spans cannot be discarded wholesale either.
**The per-span classification is unchanged and still sums to 15:** ten had drifted by a **NON-UNIFORM**
offset and five named sites that refactors had already eliminated. `:401-406` is one of the **ten drifted**
-- the site it was written to name had moved -- and it is live only because the span is six lines wide and
two base sites happen to fall inside it. **Whether the entry meant either of them is not measurable**, so
it is recorded as a drifted span that still contains a site, not as a span that was right.
The ten drifts, in the order the entry lists them, were **+47, +74, +74, +79, +79, +153, +150,
+135, +135 and +354**. **A single constant-offset remap would have been wrong at 8 of the 10** -- a second
CRITERION-PREMISE CORRECTION, since the frozen criterion mandated the figure "9 of the 10" verbatim and it
is one high: three of the ten values recur -- 74, 79 and 135 each appear **twice** -- so the best single
constant is right at two of the ten rather than at one, leaving **8** wrong. The ten values are printed
above precisely so this is checkable by eye rather than by subtraction. The direction of the finding is
unchanged, and it remains the clearest demonstration in the row set of why every site must be relocated by
content search rather than by arithmetic. Of the five eliminated, two were removed by `3c22647`
("refactor(app): extract the shared WinUI handler-mapper body to one method"), and the span cited at
`DungeonDetailViewModel.cs:302-303` points past the end of a file that is now **202** lines, with **zero
braceless sites in it**.

**The entry was EXACT WHEN WRITTEN, which is what makes this drift rather than carelessness.** It was
authored at `b2a7383`, when `DungeonDetailPage.xaml.cs` was **2226** lines against 2594 at this session's
base, and `DungeonDetailViewModel.cs` was **397** against 202. **The mapping reconciles arithmetically:**
the surviving cited sites totalled **20**, plus **one site the register never cited at all**, gives **21**
-- exactly the count measured in that file. So the 15 spans covered 25 of the 26 sites present when the
entry was written, and **one was missed at authoring time**, falling between two spans. The pre-change
figures in this paragraph are ATTRIBUTED to this session's `measurement.md` section 10 and to task `T5`'s
report; they rest on reading historical blobs for content, which a task that cannot run git could not
re-derive here.

*Correction 2 -- the `for` clause has DRIFTED, and the discard clause is FALSIFIED.* Previous wording,
verbatim: "Single-statement `if`, `else`, and `for` bodies are written without `{ }` at these sites."
**There was no unbraced `for` anywhere in `DungeonMaster.App` at this session's base** -- the narrower
clause, which stands as measured. **But the population was NOT all `if` or `else`, and an earlier draft of
this block asserted that it was while quoting the counter-example nine paragraphs below.** The keyword
census over the 27 base sites, by the matching-paren detector described later in this block reporting the
keyword of each site it classifies, is **23 `if`, 3 `else` and 1 `foreach`** -- per file,
`DungeonDetailPage.xaml.cs` 15 `if` / 3 `else if` / 3 `else`, `ObjectKindInfo.cs` 3 `if`,
`ObjectPackRegistry.cs` 2 `if`, and `ObjectPaletteView.xaml.cs` 1 `foreach`, with `else if` counted as an
`if` header. **The one `foreach` is `ObjectPaletteView.xaml.cs:33`**, which this block already quotes as a
`foreach` in its hazard paragraph, so the false universal was contradicted by its own evidence. **And the
`for` clause itself is DRIFT, not ERROR:** at the authoring commit `b2a7383` there **was** an unbraced
`for`, at `:1756`, with its body at `:1757` inside `PolygonArea` at `:1752` -- read from the historical blob
and classified by the same detector, which returns census 177 / braced 151 / braceless **26** on that file
at that commit and names `:1756` as a braceless `for`. **So the entry's `for` clause was EXACT WHEN
WRITTEN**, which is the classification this block's own "EXACT WHEN WRITTEN" paragraph above already
implies, and an earlier draft marked it `ERROR` against that. And previous
wording, verbatim: the densest site is "a five-branch `if/else if` chain in which every branch is an
unbraced `_ =` discard". The five-branch chain is real and its every body was unbraced, **but the bodies
are `await` expressions, not `_ =` discards** -- that was true at `b2a7383` and had ceased to be true by
this session's base. Previous wording, verbatim: "`PolygonArea` at lines 1756-1757, whose `for` body is
unbraced." **`PolygonArea` does not exist in `src/` at all**; its sole hit is a test method name. Its
successors live in `App.Core/Map/AreaOutlineGeometry.cs` and **both relocated `for` loops are braced**,
with the census as positive control -- the detector sees 8 control-flow sites in that file and finds all 8
braced.

*Correction 3 -- the entry's universal, NARROWED rather than struck.* Previous wording, verbatim: "The same
file uses full braces everywhere else, so the deviation is local rather than a house style." **As a
statement about `DungeonDetailPage.xaml.cs` this holds as a ratio and the ratio is worth having: that file
was 155 braced of 176, an 88 per cent ratio, so the deviation genuinely was local to it. As a project-wide
generalisation it is FALSE**: `Assets/ObjectAssets/ObjectKindInfo.cs` was **3 braceless of 3, 100 per cent
unbraced**, and it is outside every one of this row's cited locations. **That caveat travels with the scope
extension:** closing `ObjectKindInfo.cs` took it from wholly unbraced to wholly braced, which is the right
outcome, but **the entry's "local rather than a house style" framing is a `DungeonDetailPage.xaml.cs`
observation and does not transfer to it.**

**"Roughly twenty places" is CONFIRMED at 21 in the entry's own file, and 27 across the layer.** Taken by
file, which is the figure a task can act on, the population was `DungeonDetailPage.xaml.cs` **21** (cited by
14 of the entry's 15 spans), `DungeonDetailViewModel.cs` **0** (cited by one span, and empty),
`ObjectKindInfo.cs` **3**, `ObjectPackRegistry.cs` **2**, and `ObjectPaletteView.xaml.cs` **1** -- the last
three **cited by no row at all**. So 21 of 27 were inside the row's intended file and 6 of 27 were in three
files the row never names. **The work was scoped to all 27**, which closes this row and the carried
37-site backlog item together; the 26 in the first four files were done under this row and
`ObjectPaletteView.xaml.cs:33` under `UI-024`'s task.

**THE POST-CHANGE ZEROS, AND THE INSTRUMENT THAT LICENSES THEM.** Measured on the post-change tree with a
matching-paren detector built independently for this block, each zero paired with a non-zero census from
the same file in the same run:

- `Views/DungeonDetailPage/DungeonDetailPage.xaml.cs` -- **census 176, braced 176, braceless 0**
- `Assets/ObjectAssets/ObjectKindInfo.cs` -- **census 3, braced 3, braceless 0**
- `Assets/ObjectAssets/ObjectPackRegistry.cs` -- **census 3, braced 3, braceless 0**
- `Views/DungeonDetailPage/ObjectPaletteView.xaml.cs` -- **census 19, braced 19, braceless 0**
- `ViewModels/DungeonDetailViewModel.cs` -- **census 2, braced 2, braceless 0**, the controlled zero on the
  file the row cites and where the count was already zero before the work

**And the layer-wide zero: running the same detector over every `.cs` file in `src/DungeonMaster.App`,
with `obj` and `bin` excluded, returns no file with a braceless count above zero.** Every one of those
zeros sits beside a non-zero census from the same run, so none is a blind predicate.

**A GREP CANNOT DO THIS JOB, and that is measured rather than asserted.** Three implementations disagreed
on `DungeonDetailPage.xaml.cs` at **21, 15 and 32**. The 21 and the 15 are independently reproduced;
**the 32 is ATTRIBUTED AND UNREPRODUCED** -- the implementer that rebuilt that route reported it returns 0
everywhere rather than 32 and declined to claim the inherited figure, and the reviewer that read its source
established why, namely that it appends a hit only when no brace exists anywhere later in the file and so
is **structurally inert on essentially any input**. Two independent parties failed to reproduce the 32 and
one explained why its own attempt could not. **The conclusion is unaffected and in fact strengthened**: the
other naive route was shown wrong in both directions, with four misses plus a genuine false positive on a
multi-line header that is in fact braced. **Only the specific figure 32 is unreproduced.**

**The detector was shown NON-BLIND IN BOTH DIRECTIONS before any count on real code was trusted.** It masks
the contents of strings, chars and both comment forms while preserving byte positions, then locates the
**matching** close paren by a depth walk across newlines, and only then inspects what follows. On a fixture
holding a multi-line nested-paren braceless header beside a **byte-identical correctly braced twin**, plus
a brace-inside-the-header site in both braced and braceless form, plus `if` decoys inside a string literal
and a comment, it returns **census 6, braced 4, braceless 2** -- flagging exactly the two real defects,
**classifying rather than ignoring the braced twins, and counting neither decoy.**

**Two hazards in this file are why the instrument had to be built rather than grepped, and both are live
instances rather than hypotheticals.** `ObjectPaletteView.xaml.cs:33` is itself an instance of the defect
class -- `foreach (var (_, displayName) in ObjectPackRegistry.AvailablePacks)` -- a nested-paren header on
which one naive route false-negatives and the other reports it with the wrong class while also
false-positiving a second line. And one of the sites in `DungeonDetailPage.xaml.cs` carries
`is not { } floor`, an **empty property pattern that puts a literal open brace INSIDE the header text** --
a brace that opens no block. **Every hazard in this arc's inherited instrument list concerns nested PARENS;
this is a brace in the header, a distinct route to the same confident wrong answer**, and a detector keying
on "the first open brace after the header" reads that site as already braced and silently drops it.

**"Mechanical and behaviour-preserving" is CONFIRMED, and the one body form that would change scope when
wrapped is absent.** All bodies were extracted **by content**, never by offset, and classified: 8
return-family, 2 `continue;`, 6 method call, 6 `await` of a call, and 4 assignment to an existing variable
or indexer -- **and ZERO declarations**, which is the single case that genuinely changes scope when braced.
No `else if (cond)` was rewritten as `else { if (cond) }`. No comment sits between any header and its body;
the one trailing comment stays inside the new braces with its text and its internal spacing byte-identical.
`out var` and pattern variables in conditions are safe, because they scope to the enclosing block
regardless of bracing. **The entry's `citation` was also confirmed verbatim and carries NO carve-out:**
`dotnet-format.md:154` is the entire normative text, read directly -- "All control flow statements (`if`,
`else`, `for`, `foreach`, `while`, `do`, `using`, `lock`) must use full brace notation `{ }`, even when the
body is a single statement. Never omit braces." -- and it contains no "unless", no "except", no "may" and
no `.editorconfig` escape.]

### UI-029 -- Two files indent with spaces where the rest of the layer's C# uses tabs

```yaml
id: UI-029
layer: UI
severity: S4
category: style
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs:1-390
  - src/DungeonMaster.App/Views/DungeonDetailPage/SecondaryPaletteView.xaml.cs:1-89
effort: XS
risk: low
independent: true
```

**What.** These two files indent with four spaces. Every other `.cs` file in this scope -- including
`ControlsPaletteView.xaml.cs`, `ToolTileView.xaml.cs`, and `PaletteThumb.cs`, which sit in the same folder
-- indents with tabs.

**Why it matters.** `dotnet-format.md` does not state an indent character in prose, so I am not asserting a
rule breach; every code sample in that document is tab-indented, which I take as the house style but not as
a stated requirement. What I do believe is that a mixed convention inside a single folder produces
whitespace-only diff noise whenever an editor with different settings touches one of these files, and that
the repository has nothing to prevent it, since there is no `.editorconfig` anywhere.

**Suggested direction.** Convert both files to tabs to match their neighbours. The durable fix is an
`.editorconfig`, which belongs to T6.

### UI-030 -- The create-dungeon popup's six bindings are reflection-based because it declares no x:DataType

```yaml
id: UI-030
layer: UI
severity: S3
category: performance
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/compiled-bindings
locations:
  - src/DungeonMaster.App/Views/CreateDungeonPopup/CreateDungeonPopup.xaml:15-29
  - src/DungeonMaster.App/Views/CreateDungeonPopup/CreateDungeonPopup.xaml:38
  - src/DungeonMaster.App/Views/CreateDungeonPopup/CreateDungeonPopup.xaml:57
  - src/DungeonMaster.App/Views/CreateDungeonPopup/CreateDungeonPopup.xaml:75
  - src/DungeonMaster.App/Views/CreateDungeonPopup/CreateDungeonPopup.xaml:83
  - src/DungeonMaster.App/Views/CreateDungeonPopup/CreateDungeonPopup.xaml:87
  - src/DungeonMaster.App/Views/CreateDungeonPopup/CreateDungeonPopup.xaml:99
effort: M
risk: low
independent: true
```

**What.** The popup carries no `x:DataType`, and its comment at lines 15-29 records why: adding it produces
`MAUIG2045` for `CreateCommand` -- emitted by CommunityToolkit.Mvvm's `[RelayCommand]` generator, which
MAUI's binding generator cannot see -- and for `ButtonRowSpacing`, whose binding sets an explicit
`Source={x:Reference Chrome}`. Its six bindings therefore resolve by reflection. The two pages in this
scope do declare `x:DataType` at their roots, and `DungeonListPage.xaml` also declares it correctly on its
`DataTemplate` at line 42.

**Why it matters.** The compiled-bindings documentation puts the cost at roughly 8x for a binding with
property-change notification and about 20x for a `OneTime` one, and notes that compiled bindings are
required, not merely preferred, under full trimming and NativeAOT. It also documents
`$(MauiEnableXamlCBindingWithSourceCompilation)` as the switch for exactly the second of the two blockers
described in the file's comment, which suggests the `ButtonRowSpacing` half is addressable today even
though the generator half is not.

**Suggested direction.** Try enabling `$(MauiEnableXamlCBindingWithSourceCompilation)` and annotating the
`Source`-bearing binding with its own `x:DataType`, which would leave only the `[RelayCommand]` members
uncompiled; the project property itself is T6's file. Re-test the generator limitation on each MAUI update
and delete the comment when it lifts.

### UI-031 -- The map canvas and the floor sidebar have no accessibility affordances

```yaml
id: UI-031
layer: UI
severity: S3
category: accessibility
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/accessibility
locations:
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml:269-280
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml:613-616
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml:631-638
effort: M
risk: low
independent: true
```

**What.** The `GraphicsView` that is the entire map editing surface carries no `AutomationId`, no
`SemanticProperties.Description`, and no `SemanticProperties.Hint`; the floor list is an empty
`VerticalStackLayout` filled from code-behind with no semantics set on the rows there either; and the
notebook overlay spans all four rows of the page with no heading semantics. This is in a page where every
toolbar tile, zoom stepper, and popup button is carefully described.

**Why it matters.** The accessibility documentation notes that controls without associated text "might not
be in the accessibility tree", and its checklist asks that the UI be self-describing and that all elements
be screen-reader accessible. The concrete gap is that the app's primary function -- drawing and placing on
the map -- is announced as nothing at all, and the floor list has no headings for a screen reader to jump
between, so the effort already invested in the toolbar does not reach the content it operates on.

**Suggested direction.** Give the `GraphicsView` a description and a hint naming the active tool, set
`SemanticProperties.HeadingLevel` on the FLOORS caption and on each floor row as it is built, and give the
notebook a description when it opens so its appearance is announced.

### UI-032 -- Layout dimensions are written as literals in markup that has tokens for everything else

```yaml
id: UI-032
layer: UI
severity: S4
category: style
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml:34
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml:137
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml:638
  - src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml:12
  - src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml:52
  - src/DungeonMaster.App/Views/DungeonDetailPage/ControlsPaletteView.xaml:34
  - src/DungeonMaster.App/Views/DungeonDetailPage/SecondaryPaletteView.xaml:10
effort: S
risk: medium
independent: true
```

**What.** `Padding="16,12,16,0"`, the `300` sidebar column, `Margin="32,158,0,0"` on the notebook, and the
palette widths `220`, `220`, `272` and height `280` are bare numbers, in files where colours, spacings,
radii, stroke weights, and font sizes all come from `StaticResource` tokens.

**Why it matters.** I believe the page distinguishes two kinds of literal well and that the distinction is
worth preserving: the zoom-stepper geometry at lines 333-378 is derived, and its comment shows the
derivation, so it is correctly not a token. The seven listed here are neither derived nor tokenised. I
believe the notebook's `32,158` in particular is a measured resting position of the sort the file elsewhere
argues against -- its own comment calls it an approximation -- and that it will drift the next time the
banner or divider above it changes height. I also believe the palette widths agreeing at `220` across two
files is coincidence rather than a shared value, since nothing links them.

**Suggested direction.** Promote the sidebar width and the palette widths to tokens, and replace the
notebook margin with a layout relationship rather than a measured offset. The token set itself is T4's, so
the token names would have to be agreed with that work.

**[AMENDED 6b-xix, 2026-10-12 -- this entry is RE-ANCHOR ONLY WITH NO MARKUP CHANGE, per user escalation
E2. Its DIAGNOSIS survives measurement intact in every particular; its COORDINATES and its DEPENDENCY do
not. Classification: DRIFT for three of its seven citations, ERROR for the carve-out span's end and for the
`T4` dependency.** Form follows the `AMENDED 6b-xvi` block on `UI-003` at `:201`. **Authority, stated
first because it bounds everything below:** this entry carries `authority: judgment` and `citation: n/a`,
so there is no convention text to verify, **every clause rests on the entry author's judgment rather than
on a rule, and nothing in it is adjudicable as a convention violation.** All seven citations were
re-derived by content search on the post-change tree.

*Correction 1 -- three of the seven `locations:` citations, corrected in place, by NON-UNIFORM offsets.*
Previous wording, verbatim: `DungeonDetailPage.xaml:34`, `:137` and `:638`. The true positions, each cited
by the content that identifies it rather than by a count: `Padding="16,12,16,0"` is at **`:64`**, on the
`<Grid RowDefinitions="Auto,Auto,Auto,Auto,*" RowSpacing="0" ...>` line; the `300` sidebar column is at
**`:186`**, `ColumnDefinitions="*,300"`; and `Margin="32,158,0,0"` is at **`:746`**, the last attribute of
the `NotebookView`. **What the stale lines actually hold now, read directly: `:34` is comment prose about
the back-button behaviour, `:137` is a comment about a decorative crossed-sword icon, and `:638` is
blank.** An implementer following the citations literally would edit comment prose at two of the three.
**The offsets are +30, +49 and +108 -- they DIFFER, so this is not a constant-offset drift and a single
shift would be wrong at all three.** The three were exact when written: at `50e3572` ("chore(theme): close
out the design-system cleanup", 2026-08-12) all three read exactly 34 / 137 / 638, which is why this is
drift from insertions landing between the anchors rather than carelessness. **This session did not locate
the inserting commits**, so the offsets are measured and the cause is inferred from their shape rather than
read from history.

**The four palette citations are EXACT and need no correction**, which is worth recording because it makes
the three stale ones an isolated cluster rather than a pattern across the entry:
`ObjectPaletteView.xaml:12` is `WidthRequest="220"`, `:52` carries `HeightRequest="280"`,
`ControlsPaletteView.xaml:34` is `WidthRequest="220"`, and `SecondaryPaletteView.xaml:10` is
`WidthRequest="272"`.

*Correction 2 -- the zoom carve-out span's END, which is 53 lines short and whose shortfall is the one
clause that could have caused damage.* Previous wording, verbatim: "the zoom-stepper geometry at lines
333-378 is derived". `:333` opens the stepper stack correctly, on `<VerticalStackLayout`. The real end of
that stack is **`:431`**, cited by content as the `</VerticalStackLayout>` that closes it.
**A CRITERION-PREMISE CORRECTION, because a correction is new prose and gets no exemption:** the frozen
criterion for this block, and an earlier draft of this paragraph, said the cited end "lands mid-attribute".
**It does not.** `:378` reads `ToolTipProperties.Text="Zoom in">` -- a **complete** attribute, and its `>`
**closes the opening tag** of the `<Grid` begun at `:375`. What the span truncates is the **element**: that
`Grid` runs on to its `</Grid>` at `:430`, inside the `<VerticalStackLayout` that does not close until
`:431`. All five boundaries were read directly. **The distinction is worth the words, because a reader told
the span ends mid-attribute would look for a broken attribute and find a well-formed one**, and would then
have no reason to distrust the span at all. **The shortfall matters because of what it leaves outside the
carve-out:** `StrokeThickness="1.25"` at **`:393`**, `StrokeThickness="1.45"` at **`:426`**, and the second
`42`/`36` stepper cell at **`:408`** would all look like unprotected bare literals to an implementer
treating `:379-431` as fair game -- **when the 33-line derivation comment at `:340-372` says in terms that
"correcting" those weights is wrong.** The comment is where the entry says it is, and `:340` opens it and
`:372` closes it, read directly. **The FIRST `42`/`36` cell, at `:375`, is deliberately NOT in that list**,
and an earlier draft wrongly included it: `:375` falls **inside** the cited span `333-378`, so the entry's
own carve-out already protects it. Only the literals from `:379` on are exposed by the short end, which is
what makes `:393`, `:408` and `:426` the exposed set and `:375` a near-miss.

**The five-part token universal is CONFIRMED on four of the five categories and confirmed with a named
qualification on the fifth.** Colours, spacings, radii and font sizes all come from `StaticResource` tokens
across all four files -- `DmSpace` 8 / 2 / 4 / 3, `DmRadius` 9 / 1 / 5 / 1, `DmFontSize` 8 / 2 / 3 / 1, and
**zero bare numeric `FontSize` anywhere in the four**. Stroke weights are the qualified one: `DmStroke` 4 /
1 / 5 / 1, and the only bare non-zero values are the `"1.25"` at `:393` and `"1.45"` at `:426`, **both
inside the entry's own carve-out**. Every other bare value is `StrokeThickness="0"`, which is "no stroke"
rather than a weight choice.

**A universal can only be falsified by finding a bare literal, so the counter-census is what makes it
mean anything.** The raw census of bare numeric values on the five named categories -- colour, spacing,
radius, stroke weight, font size -- across the four files returns **17 lines; the figure that matters is
2.** The predicate is printed so the figure can be reproduced:
`grep -nE '(StrokeThickness|CharacterSpacing|[A-Za-z]*Spacing|CornerRadius|FontSize)="[0-9]|="#[0-9A-Fa-f]{3,8}"'`
over the four files. The fifteen near-misses are enumerated rather than asserted: **six**
`StrokeThickness="0"` (`DungeonDetailPage.xaml:525`, `:542`, `:548` and `ControlsPaletteView.xaml:154`,
`:184`, `:190`), **six** `CharacterSpacing` -- letter-spacing, not one of the five named categories --
(`DungeonDetailPage.xaml:100`, `:647`, `ObjectPaletteView.xaml:26`, `ControlsPaletteView.xaml:46`, `:127`,
`SecondaryPaletteView.xaml:24`), **two** zero-valued `Spacing` (`DungeonDetailPage.xaml:688`,
`ControlsPaletteView.xaml:39`), and `:64` itself matching on its own `RowSpacing="0"`.
**A CRITERION-PREMISE CORRECTION, recorded because a correction is new prose and gets no exemption:** the
frozen criterion for this block mandated "the counter-census of 16 lines ... with its fourteen enumerated
near-misses", and **both figures are one low**. The single cause is the `CharacterSpacing` family, which is
**6 and not 5** -- `grep -c 'CharacterSpacing'` returns 2 / 1 / 2 / 1 across the four files, and all six
lines are enumerated above so the count can be checked by eye rather than re-derived. Every other component
of the census reproduced exactly. **There are zero hex colours anywhere in the four files**, by the hex half
of the same predicate returning 0 on each -- **and that zero is CONTROLLED, because the identical hex
predicate returns 87 on `Resources/Theme/Tokens.xaml`**, so it is a measured absence and not a blind
pattern. The two real exceptions are the carved-out zoom thicknesses, which the entry already excludes.

**The two `220`s are coincidence, and nothing links them.** `grep -rn '220'` over
`Views/DungeonDetailPage` and `Resources/Theme` returns **exactly two lines**,
`ControlsPaletteView.xaml:34` and `ObjectPaletteView.xaml:12` -- no shared token, no cross-reference, no
`Style`. **The notebook margin's comment calls its own value an approximation**, confirmed at `:733-737`:
the word is "approximates", and the sentence reads "The margin below approximates the map's on-screen
resting position now that it is no longer computed relative to MapOverlay." So the entry's belief that the
value will drift the next time the banner above it changes height rests on the file's own admission.]

**[AMENDED 6b-xix, 2026-10-12 -- a SECOND observation on this entry, recorded as a second block because it
is a second observation: the TOKEN-FAMILY finding, which decides whether this row is workable at all, and
the fact that its `T4` dependency is REAL BUT MISDESCRIBED.** The entry's `Suggested direction.` says "The
token set itself is T4's, so the token names would have to be agreed with that work." **Both halves need
correcting, and in opposite directions: the token SET exists and is substantial, while the token NAMES this
row needs do not exist at all.**

**The `Dm*` token set is substantial: 127 unique `x:Key="Dm*"` definitions** across
`src/DungeonMaster.App/Resources/Theme/`, counted at their DEFINITION sites rather than by markup
references, by `grep -rhoE 'x:Key="Dm[A-Za-z0-9]*"'` over that directory piped through `sort -u`. Its
layout-related families, by the same pipeline restricted per prefix: **`DmRadius` 3, `DmStroke` 3,
`DmSpace` 6, `DmPadding` 6, `DmFontSize` 12, `DmIconSize` 6, and `DmTouchTargetMinimum` 1.** The
`DmIconSize` family spans **12 to 22** -- `Tiny` 12, `Small` 14, `Medium` 16, `Large` 18, `XL` 20, `Tool`
22 -- and `DmTouchTargetMinimum` is **44**, at `Tokens.xaml:715`.

**THE DECISIVE MEASUREMENT IS ABOUT VALUE COVERAGE, NOT NAME SHAPES, AND THIS BLOCK CORRECTS ITS OWN
SESSION'S INSTRUMENT ON THAT POINT.** This session's evidence base recorded that filtering the 127 keys for
width, column, panel, sidebar or palette name shapes "returns nothing". **Re-run here, that filter returns
THREE keys, so the claim as worded is FALSE** -- `DmSurfaceToolPanel` at `Tokens.xaml:297` and
`DmTextToolPanel` at `:301`, both `<Color>` elements, and `DmPaddingPanel` at `:688`, a `<Thickness>` of
`14,10`. **None of the three is a width or any other panel dimension**, so the conclusion survives, but it
needed a predicate that measures the gap directly. That predicate is value coverage: **no `Dm*` key carries
any of the values 220, 272, 280 or 300** -- `grep -rnE 'x:Key="Dm[A-Za-z0-9]*">(220|272|280|300)<'` exits 1
with no output -- and **no `Dm*` key carries any bare numeric value of 100 or more**, by the same pipeline
filtered on magnitude. **Both zeros are CONTROLLED by the same predicate finding a value that does exist**,
`DmTouchTargetMinimum` at 44, so neither is a blind predicate.

**So the conclusion is neither "dimension tokens are absent" nor "the names exist".** Dimension tokens
exist as **a glyph-scale family of 12 to 22 plus one accessibility floor of 44**, and **nothing covers the
values this row needs -- 220, 220, 272, 280, 300, or the composite `32,158`.** Those belong to a different
semantic family, panel and container geometry, **which does not exist in this token set.**

**Promoting a width therefore means OPENING A NEW TOKEN FAMILY**: choosing its naming scheme, and deciding
whether `220` and `272` are one token with an override or two separate tokens. **That is materially more
than editing seven attributes, and it is a plausible explanation for this row's `risk=medium` against an
`S` effort** -- a mismatch the entry itself never explains. The naming decision has API-like consequences,
because a token name added to a shared theme dictionary is read by every later call site.

**And the `T4` dependency has no live counterparty.** `T4` belongs to an earlier, already-closed session of
this arc, so **there is no concurrent task to agree names with**, and the `Suggested direction.`'s
instruction to agree them with that work cannot be followed as written. **That is why this row was
re-anchored and not acted on, and why the token-family design is carried to `.dir-sdlc/backlog.md` rather
than decided inside a session scoped to formatting.** The `What.` and `Why it matters.` paragraphs are left
exactly as written.]

### UI-033 -- Back navigation from the detail page depends entirely on a platform-supplied affordance

```yaml
id: UI-033
layer: UI
severity: S3
category: correctness
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml:11
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml:36-40
effort: S
risk: low
independent: true
```

**What.** The page sets `Shell.NavBarIsVisible="False"`, and its comment records that an in-page back
button was removed because the Shell's own arrow in the Windows title bar duplicated it. No code path in
the layer calls `GoToAsync("..")` or otherwise pops the detail page, so returning to the list depends
entirely on that arrow.

**Why it matters.** The detail page is pushed as a global route, so a navigation stack does exist and the
back operation is well-formed; what I doubt is the affordance. I believe the removal was verified on
Windows only -- the comment says so explicitly -- and that with the nav bar hidden there is no reason to
expect a macCatalyst build to render an equivalent arrow, in which case the page would be a one-way trip on
that platform. I also believe there is no `BackButtonBehavior` anywhere in the layer to make the intent
explicit either way.

**Suggested direction.** Verify the affordance on macCatalyst, and if it is absent, restore an in-page back
control gated to that platform or declare a `Shell.BackButtonBehavior` so the back action is owned by the
page rather than inferred from the chrome.

### UI-034 -- The Windows package manifest still carries the template's placeholder identity

```yaml
id: UI-034
layer: UI
severity: S4
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Platforms/Windows/Package.appxmanifest:9
  - src/DungeonMaster.App/Platforms/Windows/Package.appxmanifest:14-16
effort: XS
risk: low
independent: true
```

**What.** `<Identity Name="maui-package-name-placeholder" Publisher="CN=User Name" Version="0.0.0.0" />`,
and `PublisherDisplayName` is `User Name`. The `$placeholder$` tokens elsewhere in the file are
substituted at build time from the project file; the identity values are not.

**Why it matters.** I believe these are the values that would ship: the package name and publisher become
the app's durable identity on Windows, and `0.0.0.0` is not a version any store accepts. I believe the cost
is deferred rather than present -- nothing misbehaves in development -- but that changing package identity
after a first install is not a free operation, which makes this cheaper to fix now than later.

**Suggested direction.** Set a real package name, publisher, and starting version. The `$placeholder$`
values are driven by properties in the `.csproj`, which is T6's file, so the two changes want coordinating.

**[AMENDED 20261106-0120-shipping-identity, 2026-11-06 (UTC; local 2026-11-05) -- this entry was acted on
in full as `MASTER-142`, together with `XCUT-015`, and its `What.` paragraph is CORRECTED: the identity values ARE substituted
at build time, except Publisher.** Form follows the `AMENDED 6b-xvi` block on `UI-003` in this file at
`:201`.

*Correction -- the claim "the identity values are not" substituted.* `Identity Name` and
`Identity Version` are build-substituted, exactly as the `$placeholder$` tokens are: `Name` from the
csproj's `ApplicationId`, `Version` from `ApplicationDisplayVersion` and `ApplicationVersion`. The
evidence is the generated manifest at
`src/DungeonMaster.App/obj/Debug/net10.0-windows10.0.19041.0/win-x64/resizetizer/m/Package.appxmanifest`.
Built 2026-11-03, before this change (read by task `T1` before its rebuild; the file has since been
regenerated), it read `Name="com.dmarquezdev.dungeonmaster"
Publisher="CN=User Name" Version="1.0.0.1"`, `DisplayName` `DungeonMaster.App` and
`PublisherDisplayName` `User Name`, against a source manifest still reading
`Name="maui-package-name-placeholder"` and `Version="0.0.0.0"`. So `maui-package-name-placeholder` and
`0.0.0.0` were never values that would ship; only `Publisher` and `PublisherDisplayName` were identity
and display values the manifest ships as literals, and the entry's "0.0.0.0 is not a version any store accepts" does not describe the shipped
package.

*Action taken.* With the user's decision, `Publisher` is now `CN=Dungeon Master` and
`PublisherDisplayName` is `Dungeon Master`; the substituted tokens were left as tokens, with an XML
comment above `<Identity` saying the csproj is their single source. The csproj now sets
`ApplicationTitle` `Dungeon Master`, `ApplicationDisplayVersion` `0.1.0` and `ApplicationVersion` `1`,
and keeps `ApplicationId` `com.dmarquezdev.dungeonmaster`. After the change, `dotnet build
src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 -c Debug` ended `Build
succeeded.`, `0 Warning(s)`, `0 Error(s)`, and the regenerated manifest reads
`Name="com.dmarquezdev.dungeonmaster" Publisher="CN=Dungeon Master" Version="0.1.0.1"`,
`<DisplayName>Dungeon Master</DisplayName>` and `<PublisherDisplayName>Dungeon Master</PublisherDisplayName>`.
Both `locations:` citations have drifted by the new six-line comment: `<Identity`, cited `:9`, is now
at `Package.appxmanifest:15`, and the `<DisplayName>`/`<PublisherDisplayName>`/`<Logo>` span, cited
`:14-16`, is now `:20-22`.]**

### UI-035 -- The Start Session button is present, prominent, and unwired in two views

```yaml
id: UI-035
layer: UI
severity: S5
category: dead-code
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml:69-96
  - src/DungeonMaster.App/Views/DungeonListPage/DungeonListPage.xaml:72-111
effort: S
risk: low
independent: true
```

**What.** Both views carry a Start Session button with an `AutomationId`, a tooltip, semantic properties, a
`CommandParameter` binding, a `DataTrigger` dimming it, and a disabled-state tooltip overlay -- and neither
carries a `Clicked` handler or a `Command`. Both are annotated as deliberately unwired, and the list page's
`SemanticProperties.Hint` is worded to say the button performs no action.

**Why it matters.** This deviates from no authority I can cite, and I think the handling is genuinely good:
the accessibility hint is honest rather than aspirational, the comments say clicking was checked at runtime,
and both sites cross-reference each other. I record it only because a reader arriving at the
`CommandParameter="{Binding Id}"` on line 95 of the list page may reasonably assume a `Command` exists
somewhere and go looking.

**Suggested direction.** No action needed. If the feature stays unbuilt for another cycle, consider
dropping the two `CommandParameter` bindings, which are the one part of the markup that implies a binding
target that does not exist.

**[AMENDED 6b-xix, 2026-10-12 -- this entry is DELIBERATELY UNCHANGED WITH NO CODE CHANGE, per user
escalation E3. Its LIST-page claims are confirmed in full, its DETAIL-page span is the WRONG REGION rather
than drift, and three of its detail-page clauses are overstated. Classification: ERROR for the detail span
and the three clauses; the entry's JUDGMENT is upheld and independently corroborated by a frozen contract
from an earlier session of this programme.** This block follows the form of the `AMENDED 6b-xvi` block on
`UI-003` in this file at `:201`, which is this file's only prior amendment block; the arc's `6b-xviii`
blocks live in `04-app-controls-and-theme.md` and `01-domain-application.md`, not here. Every line number
below was re-derived by content search on the post-change tree, never by arithmetic from a span.

*Correction 1 -- the detail-page `locations:` citation is the wrong region, not a drifted one.* Previous
wording, verbatim: `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml:69-96`. The cited
boundaries hold neither end of the construct: `:69` is back-button prose inside a comment, reading "present
and working on this page even with Shell.NavBarIsVisible false, and it is", and `:96` is a bare `<Label`.
The Start Session construct runs **`:106-154`** -- its explanatory comment opens at `:106`, `<Button` is at
`:116`, `Text="Start Session"` at `:117`, `AutomationId="StartSessionButton"` at `:118`, and the enclosing
`</Grid>` closes at `:154`. **Start +37 and end +58, so this is NOT a constant offset and cannot be
corrected by one shift.** Each boundary is named by its line number and by what sits on it, never by a
length subtracted from a range.

*Correction 2 -- three detail-page clauses are overstated, and one of the three is the entry's own
premise.* The `What.` paragraph says both views carry "an `AutomationId`, a tooltip, semantic properties, a
`CommandParameter` binding, a `DataTrigger` dimming it, and a disabled-state tooltip overlay". For the
DETAIL page three of those six do not hold as stated. **(a) There is no tooltip on the button**: the only
`ToolTipProperties.Text` in the construct is on the transparent overlay at `:152`, which is visible only
when the button is disabled. **(b) There are no semantic properties, and the omission is deliberate** --
the comment at `:137-138` states the icon is decorative because "the button it sits on is already labelled
`Start Session`". **(c) There is no `CommandParameter` on the detail page at all.** `CommandParameter`
appears on exactly **four** XAML lines solution-wide, by `grep -rn 'CommandParameter' src --include=*.xaml`
with `/obj/` excluded, and **all four are in `DungeonListPage.xaml`** -- `:61` Edit, `:77` comment prose,
`:95` Start Session, `:129` Delete. So the entry's conditional action, "dropping the **two**
`CommandParameter` bindings", **is not executable as written: there is one, not two.**

**Every LIST-page claim is CONFIRMED, including the two the entry states exactly.** The comment opens at
`:72` and closes at `:85`; `AutomationId="StartSessionButton"` is at `:96`; and
`SemanticProperties.Hint` at `:98` reads verbatim "Sessions are not available yet; this button performs no
action." The entry's `CommandParameter="{Binding Id}"` on **line 95** is exact.

**The load-bearing absence is CONTROLLED in four directions, and the control is what licenses the zeros.**
A predicate distinguishing `Command=` from `CommandParameter=` without `\b` -- a shell-quoted `\b` becomes a
literal backspace byte and would have returned a plausible zero while still reporting a number -- gives
`grep -cE 'Clicked=|[^r]Command='`: **0** inside the detail button span `:116-136`, **0** inside the list
button span `:87-111`, against **10** on the whole of `DungeonDetailPage.xaml` and **3** on the whole of
`DungeonListPage.xaml`. The control is decisive because it finds `Clicked` on the sibling Edit and Delete
buttons **in the same `DataTemplate`, one line below their own `CommandParameter`s** -- so the predicate is
reading the right region and discriminating the right token. A second controlled pair rules out wiring by
gesture recognizer on a wrapper: **0** inside both spans against **12** and **6** on the whole files, by
`grep -cE 'GestureRecognizers|TapGestureRecognizer|PointerGestureRecognizer'`.

**This is an UNBUILT feature, not a feature wired elsewhere whose two sites are stale.** A case-insensitive
recursive search for the phrase in any spelling, `grep -rniE 'start[ _-]?session' src --include=*.cs
--include=*.xaml --include=*.csproj --include=*.json` with `obj` and `bin` excluded, returns **10 lines and
zero executable wiring**. **POSITIVE CONTROL, same machinery, pattern `dungeon`: 407 files**, so the search
reaches the solution and the 10 is not a path or include-filter artefact. Three corroborating negatives,
each run separately: `grep -rn 'RegisterRoute'` over the App project returns **exactly one** call, at
`AppShell.xaml.cs:17`, so **the only registered route is the detail route and there is no session route**;
a search for `featureflag`, `feature_flag` or `IFeatureManager` over `src` exits 1 with no output, so there
is **no feature-flag infrastructure anywhere in the solution**; and `StartSessionButton` over all four test
projects restricted to `*.cs` exits 1 with no output, so **no test asserts on the `AutomationId`**. That
last zero is controlled rather than bare: the same command shape over `src/DungeonMaster.App.UiTests`
reaches live AutomationId assertions, among them
`DungeonListPageInteractionTests.cs:50`. **One nuance the figure alone hides:** `StartSessionButton`
does appear on **five** lines in `src` -- the two `AutomationId` declarations, `Implicit.xaml:222` and
`:223`, and `src/DungeonMaster.App.UiTests/README.md:103`. **That README line is a documentation claim, not
an assertion**, and it is recorded here so a later reader who reproduces five hits does not conclude the
test zero was fabricated. **What that line documents, corrected here because an earlier draft of this block
had it backwards:** the test is
`TabbingFromTheDetailPageEntryReachesFloorExpandToggle`, and the id it asserts at **press 4 is
`FloorExpandToggle`**, not `StartSessionButton`. `README.md:103-104` prints the whole sequence --
`StartSessionButton`, `DungeonNotebookButton`, `AddFloorButton`, `FloorExpandToggle` -- in which
**`StartSessionButton` is press 1**, read directly from those two lines. **That it is press 1 sharpens the
row rather than softening it:** the unwired button is the FIRST element a keyboard user reaches on the
detail page, and the test traverses past it to reach its real subject. The test still asserts nothing about
`StartSessionButton`, so the four test projects' zero stands exactly as measured.

**The disposition is corroborated by a FROZEN CONTRACT, which is what settles it.**
`.dir-sdlc/sessions/2026-08-28-the-control-kit/manifest.json`, task **`T2`, acceptance criterion 6**, states
as a frozen contract invariant that `AutomationId="EditDungeonButton"`, `"StartSessionButton"` and
`"DeleteDungeonButton"` keep those exact strings, and that the Start-Session button stays unwired with the
comment at `DungeonListPage.xaml:72-85` intact and unsoftened.
**A CORRECTION THIS BLOCK MAKES TO ITS OWN SESSION'S FROZEN CRITERIA, because a correction is new prose and
gets no exemption.** The frozen criterion for this row attributed that invariant to "acceptance criterion 5
of task `T1`". **The invariant is task `T2`'s acceptance criterion 6.** Located by searching that manifest
for the span itself rather than by trusting the attribution: the only acceptance string in the file
containing `DungeonListPage.xaml:72-85` is the **sixth** entry of **`T2`**'s `acceptance` array, and `T1`'s
fifth criterion is about converter tests under `src/DungeonMaster.App.Tests/Converters/` and names neither
the button, the span, nor any `AutomationId`. **The corroboration itself is unaffected** -- the invariant
exists, in that manifest, frozen by that session -- **only the task and criterion naming it were wrong**,
which matters because a reader checking the citation would have looked at the wrong criterion and found
nothing. **That span is still byte-exact**: `:72`
opens "Start Session button." and `:85` closes "the same time as the handler, not before. -->", both read
directly from the file. So the deliberate-unwired state is not merely annotated in a comment -- **it was
ratified as a frozen invariant by an earlier session of this same audit programme**, and the
`AutomationId` string is contractually frozen independently of this row.

**Two things the entry never mentions, and they change what its own rationale implies.** First, **all three
`CommandParameter`s in that `DataTemplate` are INERT**, not just `:95`. The Edit and Delete buttons at
`:61` and `:129` each carry `CommandParameter="{Binding Id}"` *and* a `Clicked=` handler one line below,
and a `Clicked` handler receives no `CommandParameter`. By the entry's own stated rationale -- that the
parameter misleads a reader into assuming a `Command` exists -- **`:61` and `:129` mislead MORE**, because
their buttons visibly work and carry no warning comment. Scoping the action to `:95` alone would leave the
two more misleading instances three lines above and below. Second, **the `DataTrigger`s are LOAD-BEARING
and must not be swept up as part of the dead markup.** `Implicit.xaml` deliberately omits a `Disabled`
`VisualState`, and says so at `:215-218`, *because* a `VisualState` setter outranks both a `DataTrigger`
and a local value; `:220-224` then names the three unstyled buttons that set their own `Opacity 0.35`,
including both Start Session sites at `:222` and `:223`. Removing either `DataTrigger` would silently lose
the disabled dimming with no compensating style. **That comment block is itself corroboration of this
session's finding**, since `:226-228` records that its own line citations "were wrong on all three within
one wave of being written" and that "a name survives an edit above it."

**NOT MEASURED, and not inferred.** Both comments assert that clicking was "confirmed at runtime" to do
nothing. **This session did not re-measure that** -- no build and no application were run, the build and
suite result being attributed to the orchestrator -- because **the absence of four source constructs is not
an observation of runtime behaviour.** The source evidence is strong and four-way controlled, and it is
still evidence about source. **The entry's `authority` is `judgment` with `citation: n/a`**, so nothing in
it is adjudicable as a convention violation either way, and "No action needed" stands as the disposition.]

### UI-036 -- launchSettings.json declares only a Windows profile for a two-platform app

```yaml
id: UI-036
layer: UI
severity: S5
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Properties/launchSettings.json:2-7
effort: XS
risk: low
independent: true
```

**What.** The file contains a single `Windows Machine` profile with `nativeDebugging` disabled, although the
project targets both `net10.0-windows10.0.19041.0` and `net10.0-maccatalyst`.

**Why it matters.** This breaches no authority -- macCatalyst launch profiles are supplied by the IDE on
macOS and are not conventionally checked in -- and I note it only because it is a small piece of evidence
consistent with UI-021: the day-to-day loop appears to be Windows-only, which is a plausible explanation
for the macCatalyst input paths being unverified.

**Suggested direction.** No action needed as a file change. Worth reading alongside UI-021 when deciding
what macCatalyst support is meant to mean.

### UI-037 -- DrawSessionResult's zero value was Added, and the two consumer sites still do not reject the new zero member

```yaml
id: UI-037
layer: UI
severity: S3
category: correctness
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/enum
locations:
  - src/DungeonMaster.App.Core/Map/MapDrawingSession.cs:8-24
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1473
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1489
effort: XS
risk: low
independent: true
```

**What.** `DrawSessionResult` declared `{ Added, Closed, Ignored }` with no explicit values, so
`default(DrawSessionResult)` was `Added`. It is one of the two class A enums in the triage recorded as
`CORE-035` -- the two whose zero value asserts a consequential outcome rather than merely picking a default.
The enum is consumed by two equality comparisons in the detail page's vertex-placement path, and by nothing
else in `DungeonMaster.App` -- `grep -rn "DrawSessionResult" --include=*.cs src/DungeonMaster.App/` returned
**2 lines** when this was written, **and returns 13 as of 6b-xi** (`6d45515`), because that session added
`DrawSessionResultGuard.cs` to this project. Both figures are stated because the 2 is what the finding rests
on and the 13 is what a reader re-running the command today will see:
`if (result == DrawSessionResult.Added)` (`DungeonDetailPage.xaml.cs:1473`) and
`else if (result == DrawSessionResult.Closed)` (`:1489`). A leaked default therefore read as "a vertex was
added" and the page committed a vertex for a placement attempt that never happened.

**What landed in session 6b-ix.** `None` was added at zero, **in `App.Core` only**. `MapDrawingSession.PlaceVertex`
is unchanged -- it already returned one of `Added`, `Closed` or `Ignored` on every path, from **three** explicit
`return` statements with no fall-through, so it never returns `None` and needed no edit. Measured:
`grep -n "return DrawSessionResult" src/DungeonMaster.App.Core/Map/MapDrawingSession.cs` returns **3 lines**
(`:70` `Ignored`, `:75` `Closed`, `:79` `Added`), all three inside `PlaceVertex`
(`MapDrawingSession.cs:66-80`), and the last of them is unconditional and is the method's final statement, so
there is no path that falls off the end. The behavioural effect
is confined to what a leaked default now does: `None` compares unequal at both sites, takes neither branch, and
**no vertex is committed**. The failure mode changed from a silently fabricated vertex to a silently ignored
placement.

**The follow-up deliberately not taken in 6b-ix -- CLOSED in 6b-xi, see below.** The two equality sites at `DungeonDetailPage.xaml.cs:1473` and `:1489`
**still carry no explicit rejection of the zero member.** Neither site has an `else` that throws, logs, or
otherwise names `None`; a `None` arriving there is absorbed by falling off the end of an if/else-if chain that
has no final `else`. Read from `sed -n '1473,1494p'
src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`: the chain is
`if (result == DrawSessionResult.Added) { ... }` at `:1473` and `else if (result == DrawSessionResult.Closed)
{ await FinishDrawing(); }` at `:1489`, closing at `:1492`; `:1493` closes the enclosing `try` and `:1494` is
its `catch`, so there is no `else` after the chain. The same grep cited under **What** establishes that these
two are the only sites. This was a decision, not an omission: the frozen contract for this session
(`.dir-sdlc/sessions/2026-09-17-enum-zero-values/contracts/enum-zero-values.md` C2) places the two consumer
sites out of scope and states that no task in the session modifies them. The result is that the defect's
**consequence** is fixed -- no fabricated vertex -- while its **detectability** is not: a leaked default is now
silent rather than wrong, which is better but is still silent.

**Why it matters that this is recorded rather than left.** This is the same shape as the `RotationDirection` and
`ScaleDirection` finding in `CORE-035`: adding a zero member to an enum whose consumers compare for equality
changes what the else-branch absorbs, and does not by itself make the leak observable. Here the absorption is
benign and there it is not, but the mechanism is identical and the remedy is the same -- the rejection has to be
written at the consumer, because the enum declaration cannot express it.

**Suggested direction.** Add an explicit final `else` at the vertex-placement chain that rejects
`DrawSessionResult.None` -- throw, or log and no-op with a named reason -- so that a leaked default is reported
rather than absorbed. XS effort and low risk: `PlaceVertex` never returns `None` today, so the new branch is
unreachable from production paths and can be covered by a test that passes `None` directly.

**What landed in session 6b-xi (2026-09-18), which closes this record.** Commits `aadf0c8` and `4df98c2`,
merged `6d45515`. The suggested direction was taken, with one change forced by the build and one
correction to its final sentence.

*The rejection is not a final `else` at the chain.* `CA2208` is an error in this solution and rejects
`nameof()` naming a **local** rather than a parameter of the enclosing method; `result` at `:1471` is a local,
so the inline form **does not compile** -- measured by build probe during decomposition and reproduced
independently by the orchestrator under `AnalysisLevel 10.0` / `AnalysisMode Recommended` /
`TreatWarningsAsErrors=true` / `EnforceCodeStyleInBuild=true`. The rejection therefore lives in
`DrawSessionResultGuard.EnsurePlacementOutcome`, an extension method taking the value as a parameter, and the
call site becomes `PlaceVertex(snapped).EnsurePlacementOutcome()` -- a one-statement change at `:1471`.

*The shape was counted, not chosen.* A survey of this repository found exactly **one** production site
handling an enum member documented as never occurring -- `MutationOutcomeExtensions.ToResult` -- and this
follows it: a dedicated `None` arm, a separate `_` arm with a different message, and an `<exception>` block
naming both cases. `n = 1` is recorded as a **weak** convention rather than a rule. Also counted: **0**
`Debug.Assert` and **0** `Trace.Assert` in production, so an assertion-based rejection would have introduced a
construct that appears nowhere.

*The final sentence above was right, and it is the residual.* "The new branch is unreachable from production
paths and can be covered by a test that passes `None` directly" -- both halves hold, and the second does not
rescue the first. The guard **function** is covered: deleting its dedicated `None` arm reddens exactly one
fact, and under that defect the exception type and `ActualValue` both still match, so the **message**
assertion is the sole discriminator. But the **application** of the guard is covered by nothing: deleting
`.EnsurePlacementOutcome()` from `:1471` leaves `App.Tests` green at **102 / 102** with zero diagnostics --
measured by the reviewer, not argued. Because `None` is unreachable through `PlaceVertex`, **no runtime test
can distinguish a guarded call site from an unguarded one**; only a static check could. Carried to
`.dir-sdlc/backlog.md` with that remedy.

*Observed.* Drawing was confirmed working in the running application by the user before the merge -- vertices
added, a repeated vertex ignored, the polygon closing on the origin tap, and no "Could not complete that map
action" banner, which is the signature a misfiring guard would produce.

## Layer verdict

This half of the App layer holds its shape at the edges and loses it in the middle. The shell, the list
page, the create popup, and the small shared pieces are in good order: `AppShell` registers one global
route and hosts one `ShellContent`; `CreateDungeonViewModel` is a textbook CommunityToolkit.Mvvm type using
`[ObservableProperty]` partial properties, `[NotifyCanExecuteChangedFor]`, and a `CanExecute` predicate
correctly; `NavigationExtensions`, `PaletteThumb`, and `ToolTileView` are exactly as large as they need to
be. Source-generator usage across all four view models is idiomatic -- there is no hand-rolled
`INotifyPropertyChanged` anywhere and no generator misuse.

The middle is `DungeonDetailViewModel` and `DungeonDetailPage`. The view model is a god object by any
reading: 397 lines, fourteen dispatched operations, four separable responsibilities, and -- the defect that
matters most in this document -- it discards every `Result` the dispatcher returns, so a rejected write is
invisible to the user. The page is 2,226 lines because the seam that should carry the editor's state
machine does not exist: `ActiveTool` and `DrawingSession` sit in the view model while the draw state,
selection, drag, and hover state sit in the page, with the invariants between them hand-maintained. Much of
what is in that code-behind belongs there -- gesture plumbing, cursor shape, visual state -- but the tool
state machine, the outline arithmetic, and the two hit tests do not. Async discipline is the other systemic
weakness: three non-handler methods are `async void`, nine handlers are unguarded, and eight gesture paths
discard their tasks, all against a `DbContext` that is effectively process-lifetime because `MauiProgram`
resolves scoped services from the root provider. `Platforms/` is thin and correct on Windows and explicitly
unfinished on macCatalyst.

Two problems are boundaries, not internals, and **T6 owns the boundary records**: the DI lifetime
disagreement between `MauiProgram.cs` and the Infrastructure registration, and the fact that the Tests
project does not reference App, which is why nothing in UI-017 can be tested where it currently lives.

## Attestation

- No file under `src/` was created, modified, or deleted by this task: yes
- Every record cites at least one file path with a line number: yes
- Every record names its authority, and every non-`judgment` record carries a real citation: yes
