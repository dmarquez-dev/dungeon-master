# Coverage mapping — Session 4

Produced by `dir-architect` at decomposition, revised after the Gate 2 decisions, and written here by
the orchestrator before the approval that freezes `manifest.json`. The `Gate 2` column is complete;
`End state` is filled at Gate 3, and `Implementing task` for contract operations is derivable only
then.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1 — MASTER-018, three non-handler `async void` become `async Task` | T3, guarded by T1's G3 | covered | met |
| 2 — MASTER-019, every `async void` unit guarded | T3 (5 named + 6 lambdas + those G3 creates), T5 (4 named), guarded by T1's G1 | covered | **met, with a stated limit** — T5 criterion 3 met (guards verified by G1 over source); T5 criterion 7 **not met**, `OnEditClicked` and `OnDeleteClicked` never observed running |
| 3 — MASTER-024, no discarded tasks | T3 (12 sites), T5 (4 sites), guarded by T1's G2 | covered | met |
| 4 — MASTER-025, no file I/O in the `Draw` path | T2 implements, T3 wires the preload and observes it, guarded by T1's G4 | covered | **met in part** — T3 criterion 8 met in part; pack-change path unexercised, one object pack exists |
| 5 — MASTER-013, `ClearObjectImageCache()` half only | T2; T7 records which half closed | covered — partial by design, per escalation E3 | met — partial by design, per E3 |
| 6 — MASTER-038, `DmConfirmPopup` close task observed | T5, guarded by T1's G1 and G2 | covered | met |
| 7 — MASTER-039, cursor reflection cached | T4 | covered | met — by human observation, E4 |
| 8 — MASTER-153, `ConfigureAwait(false)` across Infrastructure | T6 | covered — E1 resolved to apply | met |
| 9 — `App/AsyncDiscipline/` encodes G1–G4 with demonstrated discrimination | T1 | covered | met |
| 10 — every guard test states it reads source text and never executes App code | T1 (App), T6 (Infrastructure) | covered | met |
| 11 — no regression, 448 + new = observed total, arithmetic stated | every task | covered | met — attributed, not re-measured; see note |
| 12 — 0 warnings both App TFMs under `TreatWarningsAsErrors`; tabs and CRLF on new files | every task | covered | met — attributed, not re-measured; see note |
| 13 — audit index and `docs/TODO.md` reconciled, incl. the four-entry reassignment | T7 | covered | met |

**No uncovered rows.**

Four items present at the first Gate 2 draft are **accepted out of scope** and have no covering task:
`MASTER-006`, `MASTER-013` beyond item 5, `MASTER-055` and `MASTER-062`, rescheduled to Session 6 by
the user's answer to escalation E2. T7 owns recording that reassignment; the work itself is Session
6's.

## Contract operations

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| `floor-map-drawable` — `ClearObjectImageCache()` | T2 | T3, in the `ObjectPalette.PackChanged` lambda | covered | **called** — `DungeonDetailPage.xaml.cs:108` |
| `floor-map-drawable` — `PreloadObjectImagesAsync()` | T2 | T3, in `PackChanged` and `OnMapCanvasHandlerChanged` | covered | **called** — `DungeonDetailPage.xaml.cs:109` and `:498` |
| `floor-map-drawable` — `LoadOrGetImage` (private) | T2 | T2, from the `Draw` path | covered | **called** — `FloorMapDrawable.cs:388`, `:481`, both in the implementing task's own file, which is what a private operation means |
| `async-discipline-rules` — G1, guarded `async void` | T1 (encodes) | T3, T5 | covered | **called** — guard green: T3 `DungeonDetailPage.xaml.cs` + `ObjectPaletteView.xaml.cs`, T5 `DungeonListPage.xaml.cs` |
| `async-discipline-rules` — G2, no discarded task | T1 (encodes) | T3, T5 | covered | **called** — guard green: all 16 sites removed |
| `async-discipline-rules` — G3, `async void` only where the signature forces it | T1 (encodes) | T3 | covered | **called** — guard green: 3 conversions |
| `async-discipline-rules` — G4, no sync-over-async | T1 (encodes) | T2 | covered | **called** — guard green: `FloorMapDrawable.cs` |
| `infrastructure-configureawait` — the rule | T6 | T6 | covered | **called** — `ConfigureAwaitDisciplineTests.cs` green over all 4 scoped files |

**No operation is declared without a consumer.** `floor-map-drawable`'s `Dispose()` operation, present
in the first draft, is withdrawn with `MASTER-013`'s deferred half — it is not an uncalled operation,
it no longer exists in the frozen contract.

Ownership: 15 globs, zero intersections, re-verified by the orchestrator against the amended manifest.

## Note on end-state evidence, 2026-08-26

Rows 11 and 12 — the no-regression and 0-warning items — are recorded **as attributed, not as
re-measured at Gate 3**. The pinned .NET SDK is no longer present on this machine: `global.json` pins
`10.0.303` with `rollForward: latestPatch` and only `10.0.400` is installed, which is a different
feature band, so `dotnet --version` fails with "A compatible .NET SDK was not found." The figures —
`Failed: 0, Passed: 469, Total: 469` and 0 warnings for both App target frameworks — were produced on
2026-08-24 while `10.0.303` was present, and are independently reproduced in `reviews/T1.md`,
`reviews/T6.md` and `reviews/T3.md`. Nothing in the session caused the SDK change.

The contract-operation call sites were located in the **session worktree**, not the main checkout: the
branch is unmerged, so the main checkout still holds the pre-session content and searching it would
have reported `PreloadObjectImagesAsync` as **not called** — a false alarm at the gate.
