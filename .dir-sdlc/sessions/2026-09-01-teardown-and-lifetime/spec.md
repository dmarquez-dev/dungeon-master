# Session 6a — Teardown and lifetime

Close the five disposal/threading audit entries the previous sessions could not produce evidence for, by making the code under them executable in a test.

## Context

Session 6 of the repository audit was split at Gate 1 into 6a and 6b. This is 6a: the disposal and lifetime cluster — three leaks, a memory-visibility race, and one narrow undisposed path. Four of the five (`MASTER-006`, `MASTER-013`, `MASTER-055`, `MASTER-062`) were rescheduled out of Session 4 because the only evidence available there was a structural assertion over source text, which the user judged insufficient for that defect class. Half of `MASTER-013` landed in Session 4 — `ClearObjectImageCache()` disposes each cached image before clearing; the page-teardown half is this session's.

What changed since is the evidence surface. Session 5a built `DungeonMaster.App.Core` (`net10.0`, MAUI-free) and `DungeonMaster.App.Tests` (`net10.0-windows10.0.19041.0`, able to reference the App project). A spike run in this session's worktree on 2026-09-01 established that a **full MAUI application host runs in-process under `dotnet test`** — see `spike-host-report.md`, which is the evidence base for every criterion here. It also fixed the boundary precisely: without a started WinUI `Application`, nothing deriving from `Microsoft.Maui.Controls.Element` can be constructed. The handoff attributed that wall to `ContentView`; it is one level lower.

The user's decision at escalation E1 is **hybrid**: constructor seams where they reach the defect, the host fixture only where nothing else does.

## Definition of done

1. `MASTER-006` — `HoverOrLongPressBehavior` removes its pan recogniser on detach, demonstrated by observed recogniser count rather than by source assertion.
2. `MASTER-013` drawable half — `FloorMapDrawable` is `IDisposable`, disposes every cached image, and exposes disposal observably.
3. `MASTER-013` page half — `DungeonDetailPage` disposes the drawable at handler teardown, demonstrated under the host.
4. `MASTER-055` — the Windows scroll-zoom unsubscribe uses a captured element reference and no longer re-resolves through `MapCanvas.Handler`.
5. `MASTER-062` — no cross-thread read of the long-press flag remains in either `HoverOrLongPressBehavior` or `CanvasLongPressDetector`.
6. `MASTER-062` — the race itself is **not** claimed as demonstrated, in code comments or in any report. The source record says the misfire was never observed; no test can force a memory-visibility reordering.
7. `MASTER-160` — a decoded image is never overwritten by `null` while undisposed, demonstrated by injecting a stream whose close throws.
8. E2 — running the test suite does not write the user's real database at `%LOCALAPPDATA%\DungeonMaster\dungeon-master.db`.
9. The host fixture is reproducible without manual steps: the `testhost.pri` copy is a build step, proved by a clean-then-build run performed twice.
10. Build clean under `TreatWarningsAsErrors`, `0 Warning(s)` and `0 Error(s)`, for both App target frameworks.
11. Both test suites pass, quoted **per assembly** with the command that produced each line. This repository has no aggregate test count.
12. Every added or edited C# file is tab-indented and CRLF throughout.

## Scope

The five entries `MASTER-006`, `MASTER-013` (page-teardown half), `MASTER-055`, `MASTER-062`, `MASTER-160`, plus one accepted addition: a database-location seam in `MauiProgram` (escalation E2), so the test host does not migrate the user's real database. That file is named by no audit entry and is in scope by explicit decision.

### Not in scope

- `MASTER-167` (inverted `GreaterThanZeroConverter` sentinel) — assigned to sub-session 6b with the fixture de-duplication and the `App/` sub-suite rebuild.
- `MASTER-161`, `163`, `164`, `165`, `166` — sidebar and UI defects, left out of Session 6 entirely at Gate 1. Folding them into a test-suite session is the theme collision that got Session 5 split.
- The remaining 24 entries of Session 6's declared list — 6b's.
- Any claim about observed macCatalyst behaviour. It builds; it cannot be run here (`ADR-001`).
- Identifying the startup `InvalidOperationException` the host masks. T1 must report what it observes; nothing here requires explaining it.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Can a test reach `MapCanvas` from the page? | Yes, via `page.FindByName<GraphicsView>("MapCanvas")` | repo evidence | `x:Name="MapCanvas"` and `AutomationId="MapCanvas"` in `DungeonDetailPage.xaml`; `Element.FindByName` is public |
| Can a test drive page teardown? | Yes — `VisualElement.Handler` is publicly settable, so `page.Handler = null` raises `OnHandlerChanged` | repo evidence | `DungeonDetailPage.OnHandlerChanged` reads `Handler`, and the file's own comment treats a null handler as the teardown signal |
| Can a test drive `OnPanUpdated`? | Assumed yes via `Microsoft.Maui.Controls.Internals.IPanGestureController` | judgment | Not verified reachable from `DungeonMaster.App.Tests`. T4 criterion 6 requires an escalation, not a substitution, if it proves unreachable |
| Can `App.Core` host `ILongPressTimer` and `CapturedSubscription<T>`? | Yes | repo evidence | `DungeonMaster.App.Core.csproj` is bare `net10.0` referencing only Domain and Application; both types are MAUI-free |
| How can the fixture redirect the database? | A process-wide static on `MauiProgram` is the only seam that reaches `CreateMauiApp()` | repo evidence | `Platforms/Windows/App.xaml.cs` — `protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();` takes no parameter, and the fixture must start the real `App` (spike §2c: a bare WinUI `Application` lost the app resources, P19) |
| Is the host recipe supported? | It works here, and it is off Microsoft's documented path | external | Measured in spike §2c/§2d under VSTest; Microsoft documents an `OutputType=Exe` app under MTP and notes "VSTest doesn't support this unpackaged WinUI configuration" (https://learn.microsoft.com/dotnet/core/testing/unit-testing-mstest-winui#configure-an-unpackaged-test-app) |
| Does `MapCanvas` acquire a real handler under the host? | **Unknown — deliberately not assumed** | repo evidence | Spike §5, first bullet: not measured. No criterion rests on it; this is what escalation E3 is about |

## Contracts

- `contracts/maui-host-fixture.md` — T1 produces; T4 and T5 consume. The fixture surface, the one-host-per-process and exception-marshalling rules, the two measured build requirements, and `MauiProgram.DatabasePathOverride`. States explicitly that it does **not** promise `MapCanvas` acquires a handler.
- `contracts/floor-map-drawable-lifetime.md` — T2 produces; T5 consumes. The three-argument seam constructor, `IsDisposed` as the disposal signal T5 asserts on, and the rule that nothing throws `ObjectDisposedException` after `Dispose()`.
- `contracts/long-press-timer.md` — T3 produces; T4 and T5 consume. The interface, the dispatcher implementation, and the test double. Records that the `volatile` route was rejected as assertable only by reflecting over a keyword.

## Open questions

Both were resolved at Gate 2, before the manifest froze.

| Question | Resolution |
|---|---|
| E3 — does `MASTER-055` take the host route E1 assigned it, or the `CapturedSubscription<T>` seam the manifest drafts? | **The seam route, as drafted.** The spike never measured whether `MapCanvas` acquires a real handler under the host, and that is the exact thing the entry is about, so the host route's criterion would be one nobody knows is reachable. T5 criterion 5 states explicitly that no criterion rests on handler acquisition |
| Seven contract operations have no consuming task | **Five removed from the contracts** — `MauiHostFixture.IsUp`, `.DatabasePath`, `FakeLongPressTimer.LastDelay`, `.StartCount`, `.StopCount`. The two `InvokeAsync` overloads kept as provisioned surface for T5's asynchronous preload path, and recorded as uncovered in `coverage.md` |
