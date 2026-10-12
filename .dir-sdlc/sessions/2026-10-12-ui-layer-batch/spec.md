# Session 6b-xix — UI layer batch

**Track:** Standard. **Base:** `chore/repo-audit` at `c9d9b11`. **Branch:** `dir/2026-10-12-ui-layer-batch`.
**Worktree:** `C:/dm-wt/6b-xix`. **Evidence base:** `measurement.md` in this directory.

## Context

Session 19 of the repository-audit arc. The register `docs/audit/00-master-index.md` holds **181 rows — 150
closed, 26 open, 5 part done, 0 unclassified** at the base commit, measured by one `awk -F'|'` over rows
matching `^\| [0-9]+ \| MASTER-[0-9]+ \|`, classifying column 11 on its **leading token** and carrying an
`unclassified` counter that reads 0.

**The user selected the UI layer, 8 open rows** — the largest open layer, chosen over INFRA (6 rows, better
effort ratio) and TEST (6 rows, all `risk=low`). The deciding factor was a synergy the orientation brief had
not spotted: `MASTER-135` overlaps a carried backlog item measuring **37 braceless control-flow sites across
`DungeonMaster.App`**, so under file-scoped ownership one task can close a register row and a layer-wide
carried population together.

**The session then re-measured all eight rows before planning anything, and that is the finding.**

| Session | Rows selected | Carried a false or stale specific | Falsified outright |
|---|---|---|---|
| 6b-xvii | 10 | 7 | 1 |
| 6b-xviii | 8 | 5 | 2 |
| **6b-xix** | **8** | **8** | **2** |

**8 of 8.** Two rows carry a recorded remedy that breaks the build — `MASTER-134`'s breaks the build *and* 26
tests. `MASTER-135` has **0 of 15 cited line spans pointing at the construct they name**, with drift from +47
to +354 and non-uniform, so no single offset corrects them. Against that, three rows' citations into
`dotnet-format.md` were **verbatim accurate**: this batch was reliable about authority and unreliable about
location, the mirror image of the arc's usual framing.

## Definition of done

1. **`MASTER-118`** — the detail route name and its query-parameter key are read from named constants by both
   live sites; both literal **values** are byte-unchanged, because Shell route matching is by exact string.
2. **`MASTER-119`** — all six `MapTool` member summary comments are in the three-line section 9 form, at the
   correct byte indentation, with all six em dashes intact.
3. **`MASTER-120`** — the **four measured** inline primary-constructor declarations are wrapped per section 3
   `:68` and section 6 `:240`. The register's three cited locations are not the population.
4. **`MASTER-133`** — all **21 measured** undocumented public sites across the three palette views carry XML
   documentation in the section 9 form, and **the truth of the new prose is adjudicated by a named party.**
5. **`MASTER-134`** — all **nine** co-located `*Args` records are extracted to correctly-named files, with
   **zero call-site edits**. `DrawState` and `_dungeonId` are struck as already satisfied.
6. **`MASTER-135`** — all **27 measured** braceless control-flow sites are braced, which also closes the
   carried 37-site backlog item, reconciled as 37 − 10 = 27.
7. **`MASTER-141`** — the three stale citations and the short carve-out span are corrected in the register.
   **No markup change.** The token-family gap is carried forward enumerated.
8. **`MASTER-154`** — closed as `DELIBERATELY UNCHANGED` with the measurement recorded. **No code change.**
9. Every one of the eight rows has a filled status cell in `00-master-index.md` and an `AMENDED 6b-xix` block
   in `03-app-shell-and-views.md`, with **every source line re-derived by content search in the post-change
   tree.**
10. The register survey still reconciles: 181 rows, classification summing to 181, **unclassified 0**, and UI
    open at 0.
11. **Build and suite are clean against the baseline.** Owned by the **orchestrator**, not by any task —
    concurrent `dotnet build` invocations collide on `obj/` and `bin/`.

## Scope

Eight register rows in the UI layer, plus one carried backlog item that the measured population of row 6
closes as a by-product, plus four measured populations carried forward rather than acted on.

### Not in scope

- **`AppShell.xaml:15`'s `Route="DungeonListPage"`** — a third route name in a third place. Excluded by
  architect escalation E4: XAML `Route="..."` cannot read a C# constant without `{x:Static}`, which has
  **zero occurrences anywhere in this repository's XAML**, and `DungeonListPage` is a *different* route from
  the one being centralised, so leaving it literal duplicates nothing. **This is the one Gate 2 decision
  still open.**
- **The `Dm*` dimension token family** `MASTER-141` would need. Deferred by user escalation E2 — it is a
  naming decision with API-like consequences and does not belong in a session scoped to formatting.
- **Any code change for `MASTER-154`.** Closed unchanged by user escalation E3.
- **`DungeonDetailViewModel.cs:20` and `:47`, and `DungeonListViewModel.cs:17`** — real deviations, but
  against section 6 `:240` and section 3 `:70`, **different rules** from the row that cited those files.
  Recorded, not fixed.
- **The eight inline positional-parameter declarations** among the nine extracted records. They violate
  section 3 `:68` — the same rule row 3 enforces — but were never measured as part of either row, so they are
  carried over verbatim and recorded rather than silently widening the diff.
- **The stale comment at `MapCanvasViewModel.cs:15-19`** and its now-unnecessary alias. Removing it is a
  name-binding change no row covers and no task can build-verify.
- **The 76 single-line summary comments** and **33 undocumented public sites** measured layer-wide.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Is the inherited state as the brief and handoff describe? | **Yes, all of it.** | 1 | `git rev-list --left-right --count chore/repo-audit...origin/chore/repo-audit` → `0 0`; reflog `@{0}` = `c9d9b11 … update by push`; `main` `45495c1` both sides |
| Does `App.Core` have a layer of its own in the register? | **No.** 13 layer strings in use, none of them `App.Core`. `MASTER-119` stays a UI row and its path is stale **by project**. | 1 | `awk -F'\|'` over column 5; precedent at `03-app-shell-and-views.md:1393` and `MASTER-114` at `01-domain-application.md:2313` |
| Where does `ShellRoutes` live? | `Services/Navigation/ShellRoutes.cs`, namespace `DungeonMaster.App.Services.Navigation`. | 1 | that folder already holds `INavigationService.cs` and `ShellNavigationService.cs`; section 2 `:58` requires the folder to mirror the namespace |
| Must files created by this session be CRLF? | **Yes, with a final newline.** | 1 | `.editorconfig:8` `end_of_line = crlf`, `:9` `insert_final_newline = true`; and 6b-xviii's created files measure CRLF — `NotebookTarget.cs` 21 of 21, `BehaviorCursorShape.cs` 4 of 4 |
| Is `.dir-sdlc/backlog.md` task-ownable or orchestrator bookkeeping? | **Task-ownable** — it is a tracked repository artifact at the base commit. Assigned to T7. | 1 | tracked at `c9d9b11`; 487 lines, 495 total CR, 487 CRLF lines, **8 lone CR** |
| Can a task verify its own work with a build? | **No.** Verification is by file-content measurement; the orchestrator builds serially between waves. | 4 | concurrent `dotnet build` collides on `obj/`/`bin/`; 6b-xviii established the serial-build pattern at a cost of 44 seconds |
| Can a grep verify the bracing work? | **No.** Three implementations disagree on the main file at 21, 15 and 32. | 1 | measured; `ObjectPaletteView.xaml.cs:33` is itself a nested-paren header, the defect class both naive routes fail |
| What leading token closes each of the eight cells? | **Seven `**done**`, and `MASTER-154` alone `**DELIBERATELY UNCHANGED**`.** Pinned by the orchestrator after simulating that a novel token returns `UNCLASSIFIED`. | 1 | simulation; and `MASTER-114` is the precedent for `**done**` on a row whose citations were defective, while `**FALSIFIED**` at `:313` is reserved for a row where no code changed |

## Contracts

**None, deliberately.** This is a conformance batch, not a feature: no task produces an interface another
task consumes. The only cross-task artifact is `ShellRoutes`, used only inside its own task's ownership set.
The two documentation tasks consume **reports**, not interfaces, and the one shared format — the register's
cross-reference form — is pinned verbatim in both tasks' criteria rather than in a contract file.

Inventing a contract here would add a frozen artifact with no counterparty, and the previous session's only
frozen false universal came from exactly that impulse. `contracts/` is empty and is meant to be.

## Open questions

**One, and it is Gate 2's decision:** whether `AppShell.xaml:15` is in scope for `MASTER-118`, which decides
whether that row closes `done` or `part done`. The architect excluded it and surfaced the exclusion rather
than absorbing it, which is correct — a row's disposition is user-visible. Measured basis: `{x:Static}` has
**0** occurrences in this repository's XAML, controlled by the same machinery finding `clr-namespace:`
mappings at `App.xaml:4` and `DungeonListPage.xaml:7`.

**A scope addition the user may want and the architect declined to decide:** reformatting the eight inline
positional-parameter declarations among the nine extracted records, which violate the same section 3 rule the
session enforces elsewhere.

## Waves, as frozen

**Wave 1 — T1 through T5, concurrent.** Five code tasks on pairwise-disjoint file sets. Three merges are
collision-driven rather than preference: `MASTER-120` and `MASTER-134` collide on `MapCanvasViewModel.cs` and
`FloorListViewModel.cs`; `MASTER-133` and `MASTER-135` collide on `ObjectPaletteView.xaml.cs`.

**Orchestrator serial build and test**, in the worktree, with nothing else in flight.

**Wave 2 — T6 and T7, concurrent.** Both register documents are touched by all eight rows, so row-wise
splitting is impossible; splitting by **document** is what makes two documentation tasks disjoint. Both depend
on all five code tasks, because their status cells and amendment blocks must cite **post-change** lines.

**Orchestrator serial build and test again**, then Gate 3.
