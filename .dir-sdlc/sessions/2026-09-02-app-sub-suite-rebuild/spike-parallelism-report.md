# Spike report — xunit parallelism against the one MAUI host, and real-page behavioural assertions

Run 2026-09-02 in the session worktree `C:\dm-wt\2026-09-02-app-sub-suite-rebuild`, branch
`dir/2026-09-02-app-sub-suite-rebuild`, clean at `83ba80c`. Verbatim record of the measurement
spike. Every claim below names the command that produced it.

All commands were run from `C:\dm-wt\2026-09-02-app-sub-suite-rebuild`. Throwaway probe classes
lived in `src/DungeonMaster.App.Tests/Spike*.cs` (all deleted; see §Cleanup). **No existing file was
edited at any point**, so no byte copy / `cmp` restore was needed — the only writes were new files
that were then removed.

## 0. Baseline and environment

```
dotnet build src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj -v:m
  → Build succeeded. 0 Warning(s) 0 Error(s)

dotnet test src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj --no-build --nologo   (×3)
  → Passed!  - Failed: 0, Passed: 77, Skipped: 0, Total: 77, Duration: 6 s - DungeonMaster.App.Tests.dll (net10.0)
  → Passed!  - Failed: 0, Passed: 77, Skipped: 0, Total: 77, Duration: 4 s
  → Passed!  - Failed: 0, Passed: 77, Skipped: 0, Total: 77, Duration: 4 s
```

**Baseline is 77 tests in `DungeonMaster.App.Tests`, not 30.** The 30 figure in
`spike-host-report.md` §6 was 6a's post-cleanup count; the assembly has grown since. Reported here
per assembly with its command, as required.

`nproc` → `16`, so xunit 2's default `maxParallelThreads` is 16 — parallelism is real, not nominal.

Confirmed by search, matching what the orchestrator stated:

```
find src -name 'xunit.runner.json' -not -path '*/bin/*' -not -path '*/obj/*'   → (no output)
grep -rn "CollectionBehavior" src --include=*.cs ... | grep -v /bin/ | grep -v /obj/   → (no output)
```

So xunit 2 defaults are in force: collections run in parallel with each other; classes within one
collection run sequentially.

One incidental discrepancy against the orchestrator's briefing, stated for the record: the briefing
said `MauiHostFixture` is an `ICollectionFixture` on `MauiHostCollection`, and it is — but it is
also the case that `Element.AutomationIdProperty` is *not* universally unreachable from the xunit
thread. See Q2. The 6a boundary statement is correct only about a *cold* process.

---

## Q1 — Do two host-requiring test classes coexist?

**Already answered by the suite as it stands; no new probe written.**

`Hosting/MauiHostFixtureTests.cs` and `Views/DungeonDetailPageTeardownTests.cs` are already two
distinct classes, in two files and two namespaces, both carrying
`[Collection(MauiHostCollection.Name)]`, and both do real MAUI work inside `host.Invoke(...)`:
`new DmCard()` and `new ContentPage()` (Element construction), `host.Resolve<DungeonDetailPage>()`
(container resolution), and `canvas.Drawable` via `FindByName<GraphicsView>("MapCanvas")` (a
`BindableProperty` read). That is exactly the probe Q1 asked for.

```
dotnet test src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj --no-build --nologo \
  --filter "FullyQualifiedName~Hosting.MauiHostFixtureTests|FullyQualifiedName~Views.DungeonDetailPageTeardownTests" \
  --logger "console;verbosity=normal"
```

```
  Passed ...MauiHostFixtureTests.ResolvingDungeonDetailPageFromTheHostContainerReturnsANonNullPage [1 s]
  Passed ...MauiHostFixtureTests.ConstructingAContentPageUnderTheHostReturnsANonNullPage [454 ms]
  Passed ...MauiHostFixtureTests.ConstructingADmCardUnderTheHostReturnsANonNullContentView [12 ms]
  Passed ...MauiHostFixtureTests.EveryExceptionTheHostMarksHandledIsRecordedWithATypeAndAMessage [3 ms]
  Passed ...DungeonDetailPageTeardownTests.DisconnectingThePageHandlerDisposesTheMapDrawable [38 ms]
  Passed ...DungeonDetailPageTeardownTests.ConnectingThePageHandlerLeavesTheMapDrawableUsable [13 ms]
```

**Finding.** Two host-requiring classes sharing `[Collection("maui-host")]` coexist with no
interference; all six tests pass, and they pass in the full 77-test run too. The single collection
fixture plus the `SemaphoreSlim` dispatch gate does what the contract says.

---

## Q2 — What does a class in a *different* collection do when it runs concurrently with the host?

This is the risk, and it splits cleanly into two orderings that behave completely differently.

Probe shape: a `SpikeSignal.HostBusy` (`ManualResetEventSlim`) let the probes coordinate across
collections so the overlap was genuine and not assumed. `SpikeHostBusyTests`
(`[Collection(MauiHostCollection.Name)]`) dispatched real MAUI work in a loop for 5 s and set the
signal; the parallel probes, in **no** collection, waited on it before acting.

### Q2 variant (a) — non-MAUI work in parallel with the host

```
dotnet test ... --filter "FullyQualifiedName!~SpikeElementOnXunitThreadTests"      (×3)
  → Passed!  - Failed: 0, Passed: 79, Total: 79, Duration: 9 s     (wall 12 s)
  → Passed!  - Failed: 0, Passed: 79, Total: 79, Duration: 9 s     (wall 12 s)
  → Passed!  - Failed: 0, Passed: 79, Total: 79, Duration: 9 s     (wall 12 s)
```

With `--logger "console;verbosity=detailed"`:

```
 host was busy when this started: True
 apartment: MTA
 host Invoke iterations in 5 s: 200437
```

**Finding.** A non-MAUI class running on an MTA xunit thread genuinely concurrently with the host
(200,437 completed `Invoke` round-trips during the overlap window) is completely harmless. Stable
across three runs.

### Q2 variant (b) — touching `Element` on the xunit thread, host-initialised first

The probe read `Element.AutomationIdProperty` and constructed a `BoxView` directly on the xunit
thread, **without** `Invoke`, after waiting for the host to be busy.

```
dotnet test ... --logger "console;verbosity=detailed"      (full assembly, ×3)
```

```
 host was busy when this started: True
 apartment: MTA
 property: AutomationId
 host was busy when this started: True
 apartment: MTA
 host Invoke iterations in 5 s: 200967
Total tests: 81
     Passed: 81
```

Runs 2 and 3 identical (`Passed: 81`, iterations 201489 / 202769).

**Finding, and it contradicts the naive reading of the 6a boundary.** Once the host has run
`Element`'s static constructor successfully on its STA thread, `Element.AutomationIdProperty` reads
and `new BoxView()` **succeed on the MTA xunit thread with no `Invoke` at all**. 81/81, three runs,
stable. The wall in 6a is the *type initializer*, which runs once per process; it is not a
per-access apartment check.

### Q2 variant (b′) — touching `Element` on the xunit thread *before* the host boots

The same touch with no wait, so it races the host's ~2 s startup and wins.

First, in isolation, with no host in the process at all:

```
dotnet test ... --filter "FullyQualifiedName~SpikeElementFirstOnXunitThreadTests" --logger "console;verbosity=detailed"
```

```
  Failed ...SpikeElementFirstOnXunitThreadTests.ConstructsABoxViewImmediatelyOnTheXunitThread [104 ms]
   System.TypeInitializationException : The type initializer for 'Microsoft.Maui.Controls.VisualElement' threw an exception.
---- System.TypeInitializationException : The type initializer for 'Microsoft.Maui.Controls.Element' threw an exception.
-------- System.TypeInitializationException : The type initializer for 'Microsoft.Maui.Handlers.ViewHandler' threw an exception.
------------ System.Runtime.InteropServices.COMException :
 apartment: MTA
 host already busy: False
Total tests: 1
     Failed: 1
```

That reproduces 6a's boundary exactly (`ViewHandler..cctor` → `FocusManager.add_GotFocus` →
`ThrowExceptionForHR`). A clean single test failure.

Now the same class alongside the host, full assembly, three runs:

```
dotnet test src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj --no-build --nologo --logger "console;verbosity=detailed"
```

```
=== RACE RUN 1 ===   host already busy: False
The active test run was aborted. Reason: Test host process crashed
Total tests: Unknown
     Passed: 53
=== RACE RUN 2 ===   The active test run was aborted. Reason: Test host process crashed   Passed: 54
=== RACE RUN 3 ===   The active test run was aborted. Reason: Test host process crashed   Passed: 54
```

Attribution, same build, offending class excluded:

```
dotnet test ... --filter "FullyQualifiedName!~SpikeElementFirstOnXunitThreadTests"
  → Passed!  - Failed: 0, Passed: 81, Skipped: 0, Total: 81, Duration: 9 s
```

**Finding — this is the headline risk.** One test class that is *not* in the host collection and
touches any `Element`-derived type on the xunit thread before the host has booted does not merely
fail itself: it **takes down the whole test host process**. `Total tests: Unknown`, ~53 of 82
results reported, the rest lost, and the host collection never reports at all. Stable across three
runs, and cleanly attributable — removing that one class restores 81/81.

### Q2 follow-up — does disabling parallelization fix it?

The obvious remedy, measured rather than assumed. Added
`[assembly: CollectionBehavior(DisableTestParallelization = true)]` with the offending class present:

```
dotnet test ... --filter "FullyQualifiedName!~SpikeQ5"      (11 runs total)
run 1  → The active test run was aborted. Reason: Test host process crashed
run 2  → Passed!  - Failed: 0, Passed: 83, Total: 83, Duration: 2 s
run 3  → Passed!  - Failed: 0, Passed: 83, Total: 83, Duration: 4 s
run 4  → CRASH      run 5  → CRASH      runs 6-11 → pass
SERIALIZED TOTALS: pass = 8, crash = 3, of 11
```

**Finding.** `DisableTestParallelization = true` is **not** a fix — it is worse than no fix. It
makes the outcome nondeterministic (3 crashes in 11 runs, ~27%), because serialising collections
does not pin their *order*: whichever of the host collection or the offending class runs first
decides whether the process survives. An intermittent green is the failure mode that costs the most
to diagnose later.

---

## Q3 — How many host-requiring classes scale?

Five new classes, each `[Collection(MauiHostCollection.Name)]`, each with three facts doing
identical host work (`new DmCard()`, `new ContentPage()`, `host.Resolve<DungeonDetailPage>()`,
`FindByName<GraphicsView>("MapCanvas")`) — 15 added host tests.

Back-to-back in one session, same build, `$SECONDS` wall-clock:

```
dotnet test ... --filter "FullyQualifiedName!~Spike"        # 77-test baseline
BASELINE-77      run1 wall=8s  Duration: 4 s
BASELINE-77      run2 wall=6s  Duration: 4 s
BASELINE-77      run3 wall=7s  Duration: 3 s

dotnet test ...                                             # 92 tests
PLUS5CLASSES-92  run1 wall=8s  Duration: 4 s   Passed: 92
PLUS5CLASSES-92  run2 wall=7s  Duration: 4 s   Passed: 92
PLUS5CLASSES-92  run3 wall=7s  Duration: 4 s   Passed: 92
```

Per-test durations across the five classes (`--logger "console;verbosity=normal"`):

```
SpikeScaleOneTests   A 11 ms  B 17 ms  C 27 ms
SpikeScaleTwoTests   A 32 ms  B 10 ms  C 12 ms
SpikeScaleThreeTests A 14 ms  B 31 ms  C 11 ms
SpikeScaleFourTests  A 12 ms  B 46 ms  C 14 ms
SpikeScaleFiveTests  A 13 ms  B 23 ms  C 15 ms
```

**Finding.** The cost is flat, and nothing grows with the number of classes. 15 extra host tests
across 5 extra classes cost no measurable wall-clock over the 77-test baseline (6–8 s either way,
VSTest `Duration` 3–4 s either way), and per-test cost is 10–46 ms regardless of whether the class
is the first or the fifth. The host's expense is the one-time startup, already paid; joining the
collection is close to free.

---

## Q4 — Can a real application page be constructed and its visual tree walked?

**Yes, completely, and further than expected.** All work inside `host.Invoke(...)`.

```
dotnet test ... --filter "FullyQualifiedName~SpikeQ4" --logger "console;verbosity=detailed"
Total tests: 5      Passed: 5
```

Construction — **both routes work**:

```
 Q4 resolve: DungeonMaster.App.Views.DungeonDetailPage
 Q4 resolve content null: False
 Q4 direct ctor: DungeonMaster.App.Views.DungeonDetailPage, content null: False
 Q4 list page: OK, content null: False, fab: True
```

- `host.Resolve<DungeonDetailPage>()` — works.
- Direct `new DungeonDetailPage(host.Resolve<DungeonDetailViewModel>())` — works. Direct
  construction is viable, so a test can inject its own view model rather than take the container's.
- `host.Resolve<DungeonListPage>()` — works too, including its `Func<CreateDungeonPopup>` factory
  dependency.

Visual tree walk, via `((IVisualTreeElement)element).GetVisualChildren()`:

```
 Q4 walk: total descendants=184, maxDepth=12
 Q4 walk types: Label=39, Border=26, RoundRectangle=24, Grid=23, HorizontalStackLayout=13,
                BoxView=12, VerticalStackLayout=7, Button=6, ToolTileView=6, Rectangle=6,
                Editor=4, DmInlineEditor=2
 Q4 FindByName: MapCanvas=GraphicsView; MapOverlay=Grid; MapToolbar=Border; TitleBanner=DmFlagBanner;
                ZoomPercentLabel=Label; ControlsPalette=ControlsPaletteView;
                ObjectPalette=ObjectPaletteView; FloorListContainer=VerticalStackLayout;
                Notebook=NotebookView; UndoButton=Button; NoSuchElementXyz=null;
 Q4 MapCanvas reached by tree walk at depth: 4
 Q4 nested ControlsPalette=ControlsPaletteView ScaleReadout=Label depthWithinPalette=5
 Q4 DmCard walk: total=7, maxDepth=4, border=Border, subtitle=Label
```

**Concrete example as asked.** Page: `DungeonMaster.App.Views.DungeonDetailPage`, resolved from the
container inside `Invoke`. Named element reached: `MapCanvas`, a `GraphicsView`. The walk was a
recursive descent over `IVisualTreeElement.GetVisualChildren()` from the page, matching on
*reference identity* against the object `page.FindByName<GraphicsView>("MapCanvas")` returned — it
was found at depth 4. That is deliberately stronger than a `FindByName` lookup: it proves the
element is in the live visual tree, not merely registered in the name-scope dictionary.

Nested XAML inflates too: `ControlsPalette` is a `ControlsPaletteView` (a separate XAML
`ContentView`), and its own `ScaleReadout` `Label` sits at depth 5 *within that palette*, reached
the same way.

`InitializeComponent` demonstrably runs: `page.Content` is non-null, 184 descendants exist across 12
levels, and every one of the ten `x:Name`s probed resolves to the type the XAML declares.

**Discrimination check** (per the standing rule that a probe never seen discriminating proves
nothing): the `FindByName` probe carries a negative control, `NoSuchElementXyz`, which returns
`null` in the same output line where the ten real names return their declared types. The walk
discriminates too — the same code returns 184 descendants / depth 12 for the page and 7 / depth 4
for a standalone `DmCard`.

**No boundary was hit.** The fallback investigation Q4 asked for (how far up the hierarchy one can
get) was not needed: the full page, with nested XAML `ContentView`s, works.

---

## Q5 — Is the `HandledExceptions` capture path live?

Measured, and the answer is a clear **no** for dispatcher-raised exceptions.

The probe raised an exception on the WinUI dispatcher *outside* `Invoke` (so the fixture's own
try/catch could not intercept it): `Application.Current.Dispatcher.Dispatch(() => throw new
InvalidOperationException(marker))`, then polled `host.HandledExceptions` for the marker.

```
dotnet test ... --filter "FullyQualifiedName~SpikeQ5"      (×3)
  → The active test run was aborted. Reason: Test host process crashed
  → The active test run was aborted. Reason: Test host process crashed
  → The active test run was aborted. Reason: Test host process crashed
```

Attribution control — the same raw dispatch outside `Invoke`, but not throwing:

```
dotnet test ... --filter "FullyQualifiedName~DispatchingANonThrowingActionOutsideInvokeIsHarmless" --logger "console;verbosity=detailed"
 Q5 control dispatch signalled: True
Total tests: 1
     Passed: 1
```

**Finding.** An exception thrown from a `Dispatcher.Dispatch` callback **fail-fasts the test host
process**; it does not reach `MauiHostFixture`'s `app.UnhandledException` handler and it never lands
in `HandledExceptions`. Stable across three runs, and cleanly attributed — the identical raw
dispatch without a throw is harmless.

This does **not** disprove the fixture's startup-time rationale: 6a's masked exception arose during
`OnLaunched`, a different path, which this probe did not exercise. What it establishes is narrower
and still useful — `HandledExceptions` has now been shown *not* to capture the one category of
exception a test is most likely to generate by accident, and the backlog note that "nothing has ever
shown the recorder fires" remains true after this spike.

---

## Cleanup

Every probe file was newly created and then deleted: `SpikeQ2.cs`, `SpikeQ2b.cs`, `SpikeQ3.cs`,
`SpikeQ4.cs`, `SpikeQ5.cs`, `SpikeAssemblyInfo.cs`. **No existing file was edited**, so the
byte-copy/`cmp` restore procedure did not apply. Every probe file was written tab-indented and
converted to CRLF before building, verified per file by comparing `tr -cd '\r' | wc -c` against
`tr -cd '\n' | wc -c` (equal in every case) and `awk '/^ /' | wc -l` → 0 (no space indentation).

```
cd C:\dm-wt\2026-09-02-app-sub-suite-rebuild
dotnet build src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj -v:m
  → Build succeeded. 0 Warning(s) 0 Error(s)

dotnet test src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj --no-build --nologo
  → Passed!  - Failed: 0, Passed: 77, Skipped: 0, Total: 77, Duration: 4 s - DungeonMaster.App.Tests.dll (net10.0)

git status --porcelain
```

`git status --porcelain` output, pasted verbatim:

```
```

Empty. The worktree is clean and back at the 77-test baseline.

---

## What this means for 6b-i's decomposition

### How many host-requiring test classes the assembly can carry

**Unlimited, for practical purposes, and nothing must change to make that safe.** Five extra
host-collection classes with 15 tests cost no measurable wall-clock over the 77-test baseline (Q3),
and per-test cost stayed at 10–46 ms from the first class to the fifth. Two such classes already
coexist today (Q1). 6b-i can add as many `[Collection(MauiHostCollection.Name)]` classes as the work
needs without a budget conversation.

No `xunit.runner.json`, no `CollectionBehavior` attribute, and no second collection is required —
and the `CollectionBehavior` route was measured actively harmful (below).

### The real constraint is not a count — it is a rule about *where* MAUI types are touched

This is the thing a decomposition would get wrong if it did not know it, and it is the single most
important output of this spike:

> **Any test class outside `[Collection("maui-host")]` that constructs or touches a
> `Microsoft.Maui.Controls.Element`-derived type crashes the entire test host process, losing every
> other result in the assembly.**

Measured, three runs, cleanly attributed (Q2 variant b′): `Test host process crashed`,
`Total tests: Unknown`, ~53 of 82 results reported. Removing that one class restored 81/81.

Three consequences a decomposition must carry:

1. **`DisableTestParallelization` does not protect against this.** Measured over 11 runs: 8 pass, 3
   crash. Serialising collections does not pin their order, so the outcome becomes an intermittent
   green — strictly worse than a deterministic failure. Do not let a task adopt it as a mitigation.
2. **The failure is order-dependent and therefore will not reproduce reliably.** Once the host has
   initialised `Element` on its STA thread, the *same* MTA touch succeeds (Q2 variant b, 81/81 ×3).
   A task author who writes a non-collection class touching `BoxView`, runs it once alongside a warm
   host, and sees green has learned nothing — it will crash CI on a different scheduling. This is
   the "passes while asserting nothing" trap in a new costume.
3. **6a's boundary sentence needs a qualifier when it is quoted into 6b-i's contracts.** "Nothing
   deriving from `Element` can be constructed without a started WinUI `Application`" is true only of
   a *cold* process. The wall is `Element`'s **static constructor**, which runs once per process; it
   is not a per-access apartment check. A contract that states the unqualified form will mislead
   whoever tries to explain why an out-of-collection test passed.

The safe rule to put in front of every 6b-i task, in one line: *if a test class touches any MAUI
`Element` type at all, it joins `[Collection(MauiHostCollection.Name)]` and does the touching inside
`host.Invoke(...)` — no exceptions, and "it passed locally" is not evidence it is safe.*

### Whether behavioural assertions over real pages are achievable

**Yes, at the top of the hierarchy — the real page itself. No fallback level is needed.** (Q4)

- Real pages construct, by **both** routes: `host.Resolve<DungeonDetailPage>()` and direct
  `new DungeonDetailPage(viewModel)` with a resolved view model. Direct construction means a test
  can inject its own view model rather than accept the container's — worth knowing for tasks that
  need controlled state. `DungeonListPage` resolves too.
- `InitializeComponent` runs and the XAML-declared visual tree is fully inflated: 184 descendants
  across 12 levels for `DungeonDetailPage`, including nested XAML `ContentView`s
  (`ControlsPaletteView`, whose own `ScaleReadout` `Label` is reachable at depth 5 within it).
- The tree is walkable with `((IVisualTreeElement)element).GetVisualChildren()`, and named children
  are reachable both by `FindByName<T>` and by reference identity through that walk — `MapCanvas`
  (a `GraphicsView`) at depth 4.

So the premise behind `MASTER-028` / `MASTER-109` holds: the lexical-form assertions over XAML and
C# text in `src\DungeonMaster.Tests\App\` **can** be replaced with behavioural assertions over a
constructed page and its live visual tree. Nothing in the capability set limits how much of those
two entries is achievable. The limiting factor is design effort per assertion, not runtime
capability.

Two practical notes for whoever writes those assertions:

- Prefer identity-through-the-walk over bare `FindByName` where the claim is "this element is in the
  tree" — `FindByName` reads the name scope, which is populated independently of the visual tree.
  Both were available here; the walk is the stronger evidence.
- Give every such probe a negative control (a name that must return `null`). The `FindByName` probe
  in this spike carried one and it earned its place.

### Anything else a decomposition would get wrong

- **A thrown exception on the WinUI dispatcher fail-fasts the process** (Q5, 3/3 runs), and does not
  reach `HandledExceptions`. Two implications: a 6b-i task must never dispatch work to
  `Application.Current.Dispatcher` directly — always through `host.Invoke`, whose try/catch is what
  turns a MAUI-thread failure into a readable test failure; and `HandledExceptions` still has **no**
  demonstrated capture, so no acceptance criterion should rest on it firing. The non-throwing
  control dispatch was harmless, so the hazard is specifically the throw, not the raw dispatch.
- **The baseline is 77 tests, not the 30 recorded in `spike-host-report.md` §6.** Any criterion that
  hard-codes an expected assembly test count is already stale; state counts with the command that
  produces them.
- **Note the `src\` segment.** The solution is `<worktree>\src\DungeonMaster.sln`; a previous session
  wrote `<worktree>\DungeonMaster.sln` into five acceptance criteria and no such file exists. Also
  prefer building and running `src\DungeonMaster.App.Tests\DungeonMaster.App.Tests.csproj` directly
  rather than a solution-wide `dotnet test`, which prints two per-assembly lines and no aggregate.
- **Build flakiness did not recur.** 6a recorded five distinct spurious build failures. Across every
  build and test invocation in this spike, none appeared and no retry was needed. (Not a counted
  figure — no command produced a tally; it is the absence of any observed occurrence.)
