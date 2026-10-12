# coverage.md — session 6b-xix (UI layer batch)

`Gate 2` column written at Gate 2, before the approval that froze `manifest.json`. `End state` filled at
Gate 3 by the orchestrator, as a join over `reviews/<task-id>.md`. Produced by `dir-architect` in its
decomposition report; written here by the orchestrator, which holds the user's Gate 2 decisions.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. `MASTER-118` — route name and query key behind named constants, both literal values byte-unchanged | T1 | covered | **met** |
| 1a. `MASTER-118` — `AppShell.xaml:15` disposition decided deliberately | T1 (report), T6 + T7 (record) | **uncovered as a code change** — architect escalation E4; predicted `out of scope` | **out of scope — prediction CONFIRMED.** Resolved at Gate 2 by E4: `{x:Static}` has zero occurrences in this repository's XAML, controlled by `clr-namespace:` at `App.xaml:4` and `DungeonListPage.xaml:7`, and `DungeonListPage` is a different route so leaving it literal duplicates nothing |
| 2. `MASTER-119` — six member comments in the three-line form, correct byte indent, em dashes intact | T2 | covered | **met** |
| 3. `MASTER-120` — the four **measured** inline primary constructors wrapped per §3 `:68` + §6 `:240` | T3 | covered | **met** |
| 3a. `MASTER-120` — the different-rule deviations recorded, not fixed | T3 (report), T6, T7 | covered | **met** — and a **fourth** was found during review, at `DungeonListViewModel.cs:17` |
| 4. `MASTER-133` — all measured undocumented public sites documented per §9 | T4 | covered | **met** — at **21**, the figure that corrected `measurement.md` §6's omission of the public constructor |
| 4a. `MASTER-133` — **truth of the new prose adjudicated by a named party** | T4 criterion 7 | covered | **met** — 30 assertions tabulated; the reviewer **declined to accept the table** and verified every row against sources it located itself. **None failed** |
| 5. `MASTER-134` — all nine `*Args` records extracted, zero call-site edits | T3 | covered | **met** |
| 5a. `MASTER-134` — `DrawState` and `_dungeonId` struck as already satisfied; field ordering demoted | T6, T7 | covered | **met** |
| 6. `MASTER-135` — all 27 measured braceless sites braced | T5 (26) + T4 (1) | covered | **met** |
| 6a. `MASTER-135` — carried 37-site backlog item closed with the 37 − 10 = 27 reconciliation | T6, T7 | covered | **met** — and `MASTER-121`'s own closure independently re-verified as a by-product |
| 7. `MASTER-141` — citations re-anchored, carve-out widened, **no markup change** | T6, T7 | covered | **met** |
| 7a. `MASTER-141` — token-family gap carried forward enumerated | T7 | covered | **met** |
| 8. `MASTER-154` — closed `DELIBERATELY UNCHANGED`, **no code change** | T6, T7 | covered | **met** |
| 9. All eight status cells and eight entries amended, every source line re-derived post-change | T6, T7 | covered | **met** — after one revision round each; the three wrong anchors found at review are corrected |
| 10. Register survey reconciles — 181 rows, sum 181, **unclassified 0**, UI open 0 | T6 criterion 16 | covered | **met** — 181 / 158 closed / 18 open / 5 part done / **unclassified 0**, UI open **0**, re-measured not subtracted |
| 11. **Build and suite clean against the `c9d9b11` baseline** | **no task — the orchestrator** | **uncovered by design** — concurrent builds collide on `obj/`/`bin/` | **met, ATTRIBUTED to the orchestrator.** Run serially three times with nothing in flight: baseline, after wave 1, and on the final tree. `Build succeeded.`, 0 Warning(s), 0 Error(s), all three TFMs; **1038 passed** — 900 / 30 / 108 — 0 failed, 0 skipped, identical each time |
| 12. Four measured populations carried forward, each with its command | T7 | covered | **met** — carried as **seven** items, the four named plus three the reviews required |

**Exceptions, per the one-summary-line rule.** Eighteen rows, **sixteen clean and `met`**. The two exceptions
are both the rows that were `uncovered` at Gate 2, and both resolved as predicted:

- **Row 1a** — `out of scope`, accepted at Gate 2 under escalation E4. **Its Gate 2 prediction of
  `out of scope` is CONFIRMED**, which is the whole value of writing a prediction before the search: the
  result reads as a confirmation rather than as a late excuse.
- **Row 11** — `uncovered by any task by design`, and **met by the orchestrator**. The architect flagged it
  rather than claiming coverage, which its brief requires of it; it is recorded here so it could not fall
  between the two waves.

**No row is `not met`.**

## Contract operations

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|

**Empty, deliberately, and not a gap.** No contract files were created, so no operations are declared and
there is nothing to search for. The consequence stated plainly: **the sharpest row this table can produce —
an operation with an implementer and no consumer, meaning every task passes while the product cannot do the
thing — is structurally impossible in this session.** It was a conformance batch, not a feature.

The only cross-task artifact was `ShellRoutes`, every consumer of which sits inside its own task's ownership
set, so it is not an interface between tasks. The two documentation tasks consumed reports, not interfaces.
