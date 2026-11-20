# Spec — 6b-iv: make the floor sidebar operable

Session slug `2026-09-11-make-the-floor-sidebar-operable`. Base branch `chore/repo-audit` at
`b3b164103c63de5176c30690c1e5fe52fd34e4da`. Session branch
`dir/2026-09-11-make-the-floor-sidebar-operable`, worktree `C:/dm-wt/6b-iv`. Track **Standard**.

## The problem

Session 6 excluded five sidebar entries at its Gate 1 on the stated ground that folding sidebar
defects into a test-suite session was the theme collision that split Session 5. They have never been
scheduled since. This session takes them.

**It does not take them as written.** Every entry was re-measured against `b3b1641` before any task
was drafted, per `measurement.md`, and four of the five changed disposition. Decomposing from the
entry prose would have produced tasks that fix working code.

| Entry | What the entry says | What is true at `b3b1641` |
|---|---|---|
| `MASTER-164` | card body and toggle take no focus | **True as written.** The only one. |
| `MASTER-163` | handle under the scrollbar, reorder impossible **at all** | Handle placement true; **"at all" is false** — human-observed, the scrollbar impedes grabbing, it does not block it |
| `MASTER-161` | card body "carries no `HandCursorBehavior`" | **False.** It carries one (`FloorSidebarItem.xaml:88`) and it **works** — human-observed |
| `MASTER-165` | works on `Button`, not on layouts/labels | **Falsified.** Its only evidenced surface works; `DmCard`'s `ContentView` root already contradicted it |
| `MASTER-166` | ghost frame never reaches the screen | **Wiring sound**, four tests exist. A timing question only, and its own record concedes the evidence is negative |

And the measurement found a defect no entry records, which is now the most serious item in the
session:

> **The floor reorder drop does not reorder.** The drag starts; nothing happens. Human-observed.
> Everything from the gesture to `ReorderFloorCommand` is covered by no test at all.

## Definition of done

1. The reorder drop defect is **diagnosed before it is fixed**, and the fix is confirmed against the
   running application by a person.
2. The reorder path gains its first test coverage.
3. `MASTER-164`: the expand toggle takes keyboard focus, and a floor's areas are reachable by
   keyboard alone.
4. `MASTER-164`: **the header row height is unchanged.** This is the condition on which the user's
   5c escalation-E5 refusal was reversed, not a cosmetic preference.
5. `MASTER-164`: the two questions documentation could not settle are **run, not assumed** — whether
   `MinimumWidthRequest`/`MinimumHeightRequest` override the WinUI default `Button` minimum, and
   whether the header height moves.
6. `MASTER-163`: the drag handle can be grabbed without fighting the overlay scrollbar.
7. `MASTER-163`: the false "cannot be performed at all" claim is not restated anywhere.
8. `MASTER-166`: the write latency is measured against the frame interval, the conclusion is **not
   overstated as a proven cause**, and `ClearGhostAreas()` moves after `RefreshFloorAsync()`.
9. `MASTER-166`: the optimistic area-move site is decided; the Place-tool ghost is **not** folded in.
10. The audit record is corrected: `MASTER-161` → done, `MASTER-165` falsified rather than
    re-attempted, `MASTER-163`'s claim and severity corrected, `MASTER-098`'s void deferral premise
    struck, and the five stale citations fixed.
11. The reorder defect and the `AreaSidebarItem` missing-behavior finding are **filed as numbered
    entries** (`MASTER-174`, `MASTER-175`).
12. The Appium harness gains floor-sidebar facts, and **no criterion rests on an unproven
    instrument**.

### Not in scope

- `MASTER-098`'s own remedy (bindable visual-state properties). Excluded at Gate 1; its blocking
  premise is struck by T6 and the entry is re-verified at Gate 3.
- Remediation of `MASTER-165`'s underlying claim — there is nothing to remediate; the claim is false.
- Attaching `HandCursorBehavior` to `AreaSidebarItem`'s card body. T6 **files** it as `MASTER-175`.

## Tasks

Six tasks. **T1 → T2 → T3 → T4 is a strict serial chain**, because `FloorSidebarItem.xaml` is wanted
by T1/T2/T3 and `DungeonDetailPage.xaml.cs` by T1/T4, and any glob intersection forbids concurrency.
T5 and T6 are the only pair that runs concurrently, and their ownership sets are disjoint
(`App.UiTests/**` versus `docs/audit/` + `.dir-sdlc/backlog.md`). T6 runs no `dotnet` command, so the
shared `obj/`/`bin/` cannot contend.

| Task | Agent | Owns (summary) | Depends on | Criteria |
|---|---|---|---|---|
| T1 Diagnose then fix the reorder drop defect | frontend | `FloorSidebarItem.*`, `DungeonDetailPage.xaml.cs`, `FloorReorder*` | — | 9 |
| T2 `MASTER-164` focusable pinned-footprint glyph `Button` | frontend | `FloorSidebarItem.*`, `FloorExpandToggle*` | T1 | 9 |
| T3 `MASTER-163` handle clear of the gutter | frontend | `FloorSidebarItem.xaml`, `DungeonDetailPage.xaml`, `FloorDragHandle*` | T2 | 9 |
| T4 `MASTER-166` measure, then order the clear | frontend | `DungeonDetailPage.xaml.cs`, `OptimisticGhost*`, `GhostFrameOrdering*` | T3 | 9 |
| T5 Appium floor-sidebar facts | test | `src/DungeonMaster.App.UiTests/**` | T3, T4 | 9 |
| T6 Audit record corrections + two new entries | architecture | `docs/audit/00-master-index.md`, `04-app-controls-and-theme.md`, `.dir-sdlc/backlog.md` | T1, T4 | 10 |

## Contracts

Four, frozen at Gate 2:

- `floor-reorder.md` — the `"FloorId"` payload, the five error cases, the `ReorderFloorCommand`
  invocation, and the three refuted theories restated so no task re-opens them. States candidate #1
  **as a candidate, not a diagnosis.**
- `floor-sidebar-automation-surface.md` — the six `AutomationId`s T2/T3 must preserve, the Mac
  Catalyst `Description` trap, and the frozen split between what Appium **can** assert and what it
  **cannot**.
- `floor-card-header-footprint.md` — the pinned 16x23 visual / >=23x23 hit area, the height invariant
  as the condition on which E5 was reversed, the two must-run questions, four prohibitions.
- `audit-record-allocation.md` — `MASTER-174`/`MASTER-175` allocated, ten enumerated corrections, and
  the correction/closure split: T6 writes claims, the orchestrator writes statuses with commit
  evidence at Gate 3, because a task cannot invoke git.

## Assumptions

| # | Assumption | Rung | Basis |
|---|---|---|---|
| A1 | The definition of done is the twelve items above | handed material | Enumerated in the Gate 1 scope decision and the architect dispatch; **confirm at Gate 2** |
| A2 | `MASTER-174`/`175` are the next free entry ids | repo evidence | `grep -o "MASTER-1[0-9][0-9]" docs/audit/00-master-index.md \| sort -u \| tail` → highest `MASTER-173`; re-run by the orchestrator |
| A3 | Solution-wide commands are `dotnet build src/DungeonMaster.sln` and `dotnet test src/DungeonMaster.sln --no-build` | repo evidence | 6b-iii's `log.md:29`, `handover.md:179` |
| A4 | Structural/source-scan facts live in `DungeonMaster.Tests/App/*`; MAUI-host and drawable facts in `App.Tests`; extractable seams in `App.Core/{Editor,Timing}` | repo evidence | Precedent: `AsyncVoidUnitsAreGuardedTests`, `IconFontCoverageTests`; both `App.Core` directories already exist |
| A5 | The `MASTER-166` latency measurement must be file-backed, not in-memory | repo evidence + judgment | `SqliteInMemoryTestBase.cs` would understate real latency; encoded as a lower-bound fallback rather than a hard requirement |
| A6 | Appium's W3C pointer actions **may or may not** drive native WinUI drag-and-drop | measured, unresolved | `measurement.md` §5: synthetic `mouse_event` demonstrably does not. T5 carries an explicit fallback rather than a criterion resting on it |
| A7 | The three human-observation criteria are executed by the orchestrator with the user and handed to the reviewer | **escalation E5 — open at Gate 2** | The owning task cannot execute them and `dir-reviewer` judges criteria only |

## Disciplines

Composed at session setup from the dir-ect bases alone — there is no `.dir-sdlc/config.json` and no
`.dir-sdlc/disciplines/`, so no project fragments exist:

- **backend** — `dir-ect disciplines/backend.md` (42 lines)
- **database** — `dir-ect disciplines/database.md` (35 lines)
- **frontend** — `dir-ect disciplines/frontend.md` (34 lines)
- **test** — `dir-ect disciplines/test.md` (37 lines)

All four are far under the 500-line composed cap.

## Risks

1. **T1 may not reach a diagnosis.** Four candidates survive; three theories were refuted. The task is
   written to diagnose before fixing, and its criteria do not assume which candidate is right. If none
   confirms, T1 must report the narrowed candidate set rather than ship a speculative fix.
2. **A6 may bite.** If Appium cannot drive native DnD, T5's fallback is the only route and the
   reorder guard stays human-observed.
3. **Four tasks in a serial chain** means the session's wall-clock is the sum, not the max, and a
   blocked T1 blocks everything except nothing — T5 and T6 both depend on it transitively.
