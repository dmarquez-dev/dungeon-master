# Session 6b-xvii -- CORE layer batch

Close eight CORE-layer register rows in one batched session, selected by layer concentration rather than
by severity, and record the corrections the re-measurement pass produced.

## Context

The audit register at `docs/audit/00-master-index.md` holds 179 rows: 133 closed, 42 open, 4 part done,
measured at `00a093a` (see `measurement.md` section 1). **The severity criterion that drove the previous
ten sessions is spent** -- there are no open S1, S2 or S3 rows, only 32 S4 and 10 S5. Recent cadence has
been roughly 1.4 rows per session while 37 of the 42 open rows are `XS` or `S`, because severity selected
isolated rows rather than batchable ones.

This session selects by layer instead. CORE holds 12 open rows, 10 of them `XS` or `S` -- the largest
small-row block in the register -- and it holds the one free win: **`MASTER-111` and `MASTER-171` are the
same defect filed twice**, byte-identical titles, so one fix closes two rows. There are 42 open rows but
41 distinct open defects.

Before any of this became work, every selected row was re-measured. **Seven of the ten carried at least
one false or stale specific**, one row was falsified outright, and one row's central claim about
persistence was false in a way that would have rewritten stored data. `measurement.md` is the evidence
base; the audit entries' own `locations:` fields are not.

## Definition of done

Code changes:

1. `src/DungeonMaster.Domain/Entities/Floors/RotateDirection.cs` is renamed to `RotationDirection.cs` by a
   git-visible rename, so the file name matches the `public enum RotationDirection` it declares. The two
   referencing files in `DungeonMaster.App` still compile and the solution builds at 0 Warning(s),
   0 Error(s) on all three App TFMs.
2. Doc-comment continuation lines in `CanvasPoint.cs`, `PlacedObjectDto.cs` and `PlacedObject.cs` are
   indented `///` + two tabs, and every single-line `<summary>`, `<param>` and `<returns>` tag in those
   three files is expanded to the multi-line form. The population is **14** such tags, not the 10 the
   first measurement implied: `PlacedObjectDto.cs` `:9, :10, :14, :15`; `PlacedObject.cs` `:12, :37,
   :40, :49, :52, :55` and `:81`; `CanvasPoint.cs` `:34, :35, :84`. No file gains a CRLF-to-LF
   conversion. `CanvasPoint.cs`'s `<remarks>` block (`:7-33`) is left untouched.
3. Each of the five `AsReadOnly()` property sites returns a wrapper cached in a `readonly` field rather
   than allocating one per read, and **no public property type changes**. The sites are `Dungeon.cs:49`,
   `Area.cs:61`, `Floor.cs:75`, `Floor.cs:81`, and `App.Core/Map/MapDrawingSession.cs:43`.
4. The parameter `ct` is spelled `cancellationToken` at all 51 declaration sites in
   `DungeonMaster.Application` (44) and `DungeonMaster.Infrastructure` (7). The 27 sites in
   `DungeonMaster.Tests` and the 1 in `DungeonMaster.App` are deliberately untouched.
5. The local `obj` is renamed to a descriptive name at all nine occurrences in `FloorObjectMethods.cs`
   (`:31, :38, :41, :56, :59, :64, :95, :98, :103`), and at the two sites where the enclosing lambda
   parameter is already called `placedObject` (`:56-57` and `:95-96`) the local and the lambda parameter
   end up with **two different** descriptive names. The entry suggests `placedObject` for the local,
   which is the lambda parameter's own name at both of those sites; `dir-architect` compiled a repro and
   established that this shadowing is **not** a compile error on this toolchain, so the compiler will not
   catch it and it has to be checked by reading.
6. All six of `ObjectKind`, `ObjectRotation`, `ObjectScale`, `SnapAnchor`, `RotationDirection` and
   `ScaleDirection` carry a type-level `<summary>`; `SnapAnchor`, `ObjectRotation`, `RotationDirection`
   and `ScaleDirection` carry a `<summary>` on every member; `ObjectScale` carries one on every constant
   and method. `ObjectKind`'s type summary points at its grouping comments rather than documenting its
   38 members individually.
7. The suite passes at its full count with 0 failed and 0 skipped, and the build reports 0 Warning(s),
   0 Error(s), read from the `N Warning(s)` / `N Error(s)` lines rather than from a grep for the words.

Register and audit changes:

8. `CORE-028` in `docs/audit/01-domain-application.md` records that its claim is falsified -- zero of 16
   handlers carry the reported `using`, which exists nowhere in `src` -- names the positive control that
   proves the predicate was not blind, and attributes the cause to `CORE-029`'s move of the repository
   interfaces. `MASTER-113` closes on that evidence with no code change.
9. `CORE-032` records that `MASTER-145` is closed deliberately unchanged, with the entry's own reasoning.
10. `CORE-033` records the corrected count -- three `= default!` properties, not two, with `Dungeon.cs:33`
    previously uncited and `PlacedObject.cs:47` previously cited as `:53` -- and the corrected Domain file
    count of 21. `MASTER-146` closes on that evidence with no code change.
11. Every stale citation the re-measurement found is corrected in its audit entry, following the
    `AMENDED 6b-xvi` precedent at `docs/audit/03-app-shell-and-views.md`, `UI-003`.
12. `MASTER-171` closes in `00-master-index.md` **as a duplicate naming `MASTER-111`**, not as
    independently done. `MASTER-130` closes **part done**, naming the excluded rotation-member rename and
    the excluded Tests and App `ct` sites.

## Scope

In scope: `MASTER-111`, `MASTER-112`, `MASTER-113`, `MASTER-114`, `MASTER-130` (partial), `MASTER-131`,
`MASTER-145`, `MASTER-146`, `MASTER-171`. Eight distinct defects across nine rows.

### Not in scope

- **`MASTER-157`** (`S5`/`L`/`risk=high`, the dispatcher row) and **`MASTER-178`** (`M`, the enum-zero row,
  partly worked already) -- excluded at Gate 1 by user decision.
- **`MASTER-110`** -- excluded by the user's own decision rule (E3). The cleanest remedy is to conform to
  the two-file split at `dotnet-format.md:556-563`, which means folding 112 lines into a 176-line file and
  splitting `PlacedObject` into two partials. That exceeds the row's `XS` estimate, so it goes to the
  backlog for a properly-scoped track.
- **The `ObjectRotation` member rename** (`CW90` -> `Clockwise90` and siblings) -- excluded by E1. The
  enum is persisted by member name, so this is a data migration. Backlog item covers it together with
  `PathSegmentKind`.
- **The `ct` rename in `DungeonMaster.Tests` (27 sites) and `DungeonMaster.App` (1 site)** -- excluded by
  E2. Backlog item covers it.
- **The `dm-conventions` repository.** This session has no mandate to edit it. Any deviation accepted here
  is recorded in this repository's audit entries.
- **The unfiled sentence-boundary deviation** in `CanvasPoint.cs`'s `<remarks>` block -- see Open
  questions. It is a real deviation against `dotnet-format.md:378` but it is not what `MASTER-112` filed.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| For `MASTER-114`, cache the wrapper or change the property type to `IReadOnlyList<T>`? | Cache the wrapper in a `readonly` field; do not change any public property type. | 4 | The entry lists caching first (`docs/audit/01-domain-application.md:1852-1854`) and the row's effort field is `XS`; changing a public property type is wider than `XS` authorizes. |
| Does `MASTER-114` include the `MapDrawingSession` site, which has moved into `DungeonMaster.App.Core` and is outside the CORE layer? | Yes. | 4 | The row title is "**Every** collection property"; `measurement.md` section 3 records the move. Excluding it would close the row `part done`, and part-done rows are the register's worst residue -- all four existing ones are `S3` with `M`/`L` residue. |
| Rename the file to `RotationDirection.cs`, or rename the type to `RotateDirection`? | Rename the file. | 1 | The entry prescribes it (`docs/audit/01-domain-application.md:1600-1601`) and the sibling pair `ScaleDirection.cs` / `ScaleDirection` matches on the noun. |
| How does `MASTER-171` close, given it duplicates `MASTER-111`? | As a duplicate naming `MASTER-111`, not as independently done. | 1 | `.dir-sdlc/sessions/2026-09-30-canvaspoint-finiteness-guard/BRIEF-session-6b-xvii.md`. |
| Does `ObjectKind` need a `<summary>` on each of its 38 members? | No -- a type-level summary pointing at its existing grouping comments. | 1 | The entry's own suggested direction, `docs/audit/01-domain-application.md:1676-1677`. |
| `dotnet-format.md` is the cited authority for four of these rows, but it is in a separate repository and is not in any composed discipline. How does it bind? | Load-bearing rules are encoded as acceptance criteria, quoting the rule and citing its line. Unencoded rules reach implementers as standing instruction only, and are not enforced. | 1 | No `.dir-sdlc/config.json` and no `.dir-sdlc/disciplines/` exist, so no fragment can carry it; `dir-disciplines` states that unencoded is unenforced. |
| Does this session need a project-level `.dir-sdlc/log.md`? | No. The Standard track's `log.md` is the session-level one, which exists. | 1 | `dir` Gate 3 commit list names `.dir-sdlc/sessions/<slug>/log.md`; the project-level file is the Small track's. `ls .dir-sdlc/` returns `backlog.md` and `sessions` only, across 24 prior sessions. |
| Which worktree root? | `C:/dm-wt/6b-xvii`. | 1 | A long path breaks the WinUI XAML compiler with `MAX_PATH`; `C:/dm-wt` exists for this and was empty (`ls -A /c/dm-wt \| wc -l` -> `0`). |

## Contracts

To be set by `dir-architect`. This session introduces no new interfaces, but item 4 changes **parameter
names** on eight interfaces that `DungeonMaster.Infrastructure` implements. If the Application and
Infrastructure halves land in separate tasks, the agreed parameter spelling is a cross-task interface and
belongs in a contract; if one task owns both, no contract is needed.

## Open questions

| Question | Who resolves | When |
|---|---|---|
| `dotnet-format.md:378` requires that line breaks occur only at sentence boundaries. `CanvasPoint.cs`'s `<remarks>` block breaks sentences mid-clause across `///` lines. This is a real deviation against the same section `MASTER-112` cites, but it is **not** what `MASTER-112` filed. Add it to this session's scope, or file it as a new register row? | User | Gate 2 |
| The repo-wide count of files using the space-indent doc form is **not measured** -- the per-file census timed out at 120s. Does closing `MASTER-112` need that figure, or do the three cited files suffice? | User | Gate 2 |
