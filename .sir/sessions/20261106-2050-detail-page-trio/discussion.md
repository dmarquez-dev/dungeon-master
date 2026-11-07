# DetailPage trio: MASTER-094, 098, 108

Close the three part-done audit-register rows that all land on `DungeonDetailPage`: finish the deferred second remedies of `UI-007` (MASTER-094) and `UI-016` (MASTER-108), and move sidebar selection and expansion state ownership out of the page (MASTER-098).

## Context

The backlog heading "Part-done register rows: MASTER-081, 094, 098, 108, 130" (`.sir/backlog.md:13`) is the next scheduled unit after the four headings above it, which are all closed. Its five rows are not one layer: 094, 098 and 108 share `DungeonDetailPage` (App UI, with 094 reaching into Application-layer command results), while 130 (the `ct`/`obj` rename residue in Tests, App and Infrastructure) and 081 (the Domain policy-layer decision) stand apart. Layer concentration has beaten severity as the scheduling rule across recent sessions, so the trio is taken alone.

The backlog rows, verbatim in substance (`.sir/backlog.md`, the heading's lines 15-17, and the duplicate at the "App, UI and UIKIT code" section, source `.dir-sdlc/backlog.md:225`):

- **MASTER-094** — implement `UI-007`'s deferred second remedy: have the mutating commands return or apply the changed projection instead of re-querying the whole dungeon aggregate, which also changes Application-layer command results. xref `.dir-sdlc/backlog.md:225`, MASTER-108.
- **MASTER-098** — move ownership of sidebar selection and expansion state out of `DungeonDetailPage`, which still writes `item.IsExpanded` and pushes area selection through `SetAreaEditing`, by binding the controls' existing `IsSelected` and `IsExpanded` properties to view-model state, starting from 6b-vi's measured breakdown in `MASTER-098-verification.md` rather than re-deriving it.
- **MASTER-108** — implement `UI-016`'s deferred second remedy: convert `RebuildFloorList`'s hand-rolled per-row loop in the detail page's code-behind into a templated `BindableLayout` over `Floors`. xref `.dir-sdlc/backlog.md:225`, MASTER-094.

Indicative measurements taken at Gate 1 on `chore/repo-audit` at `442b6f5`, with **unvalidated predicates** (no must-match / must-not-match control was run), to be re-measured in spec:

- `grep -rn 'RebuildFloorList' src --include=*.cs | wc -l` → 4.
- `grep -rn 'IsExpanded' src/DungeonMaster.App --include=*.xaml.cs | wc -l` → 15 (all App code-behind files, not only `DungeonDetailPage.xaml.cs`).

## Decisions

- Track: Standard.
- Scope: MASTER-094, MASTER-098 and MASTER-108 only.
- MASTER-130 stays in the backlog untouched (indicative Gate 1 figures, unvalidated: 29 `CancellationToken ct` parameters across Tests and App against the row's 28; 15 `<param name="ct">` tags; 11 `obj =>` matches in Infrastructure against the row's 17 *lines* — different units, not a contradiction).
- MASTER-081 stays in the backlog untouched; the user chose "decide later" on the build-or-don't question.
- Every row is re-measured in spec before work is designed on it; 098 starts from `MASTER-098-verification.md`'s breakdown, re-checked at the current ref rather than re-derived.
- Rows close in the audit register (`docs/audit/00-master-index.md` and the owning layer file) and leave `.sir/backlog.md` at Gate 3, per the closing pattern of previous sessions; both backlog entries for 094/108 (the heading row and the `.dir-sdlc/backlog.md:225` duplicate) close together.
