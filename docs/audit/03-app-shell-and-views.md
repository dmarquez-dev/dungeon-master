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
  - src/DungeonMaster.App/MauiProgram.cs:59-84
  - src/DungeonMaster.App/MauiProgram.cs:86-113
effort: S
risk: low
independent: true
```

**What.** The `EntryHandler` and `EditorHandler` mappers each contain a `#if WINDOWS` block that sets the
same eight properties and resource keys on a `Microsoft.UI.Xaml.Controls.TextBox`. The two blocks are
byte-identical apart from the local variable's declaration site; only the `#elif MACCATALYST` halves differ.

**Why it matters.** Fowler's first bad smell: the same expression in more than one place has to be changed
in more than one place. On Windows both handlers already resolve to the same platform type, so the next
person adding a ninth key has to notice there are two copies, and a key added to only one of them produces
an `Entry` and an `Editor` that no longer look alike.

**Suggested direction.** Extract the Windows body into one private static method taking the `TextBox` and
call it from both mappers, leaving only the genuinely platform-divergent macCatalyst arms inline.

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
