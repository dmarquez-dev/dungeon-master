# Domain invariants

Close the production-correctness gaps the backlog records in the Domain, Application and App
layers: data that can pass a guard it should hit, or fall through a branch it should not reach.
Also strengthen the two tests that cannot catch regressions in those guards.

## Context

The repo-audit arc on `chore/repo-audit` moved its deferred work into `.sir/backlog.md` in
session `20261029-1708-dir-backlog-to-sir`. That file holds 173 entries at Gate 1, by
`grep -c '^20' .sir/backlog.md`. A second predicate, `grep -c ' -- source: '`, agrees, and
the 13 `## ` headings match the 13 bundles. This is the first session to schedule work from it. The 6b-xxi brief
(`.dir-sdlc/sessions/2026-10-16-infra-layer-batch/BRIEF-session-6b-xxi.md:190-222`) recommended
the TEST style sweep. Gate 1 chose production correctness instead, because the user asked for
high-priority improvements. The brief itself says a test-only session "closes rows without
improving the product".

The selected rows, by their line in `.sir/backlog.md` as read at Gate 1:

| # | Backlog line | Source | Row |
|---|---|---|---|
| R1 | 50 | `.dir-sdlc/backlog.md:515` (xref MASTER-152) | Add six `MaximumLength` rules for `Dungeon.Description` (2000), `Dungeon.Notes` (4000), and `Floor`/`Area` `Description` and `Notes` (2000 each). The backlog says no layer bounds them. |
| R2 | 40 | `.dir-sdlc/backlog.md:328` (xref MASTER-170) | Replace the bare `else` in `PathSegment.ValidateControlPoints` with an explicit `Line` branch plus a final throw, or an exhaustive switch. |
| R3 | 39 | `.dir-sdlc/backlog.md:326` (xref MASTER-170) | Replace the production `with` expression on `PathSegment` in `AreaOutlineGeometry.Translate` with an explicit `new PathSegment(...)` call, plus a test. |
| R4 | 79 | `.dir-sdlc/backlog.md:353` (xref MASTER-073, MASTER-178) | Make the three `_ =>` arms of `ObjectKindInfo` and `GetDefaultAnchor`'s fallthrough reject `ObjectKind.None` and undefined values, instead of rendering them as a 1.00 x 1.00 centre-anchored "Decoration". |
| R5 | 122 | `.dir-sdlc/backlog.md:433` (xref MASTER-107) | Add a test asserting `LastModified` is unchanged after a failed inner operation on `RemoveArea`, `UpdateArea`, `UpdateAreaOutline`, `RemovePlacedObject` and `UpdatePlacedObject`. |
| R6 | 113 | `.dir-sdlc/backlog.md:330` | Strengthen `AreaOutlineTests.UpdateAreaOutline_WithThreeSegments_Succeeds` to compare the stored outline with the supplied segments by value (`Outline.Should().Equal(...)`), not only `HaveCount(3)`. |

These were spot-checks by the orchestrator at Gate 1, not measurements. Each one is a lead for
the spec phase to re-measure:

- `src/DungeonMaster.Domain/ValueObjects/PathSegment.cs:61` held a bare `else` (R2 looks live).
- `src/DungeonMaster.App/Assets/ObjectAssets/ObjectKindInfo.cs` held `_ =>` arms at `:73`,
  `:115` and `:138` (R4 looks live).
- The validators in `src/DungeonMaster.Application/Features/Dungeons/Commands/*/` matched
  `MaximumLength` only as `.MaximumLength(100)`, six times. That was a grep of
  `*Validator.cs` and was not checked to be on `Name` (R1 looks live).
- A grep for `with {` and for a line ending in `with` in `AreaOutlineGeometry.cs` found nothing.
  **R3 may already be stale**, or the expression may be spelled in a form those patterns miss.
  Re-measure it with a second predicate before closing or dropping it.

## Decisions

- Track: **Standard**. The work spans several files and layers and makes many separately
  checkable claims.
- Session name: `domain-invariants`.
- Scope is R1-R6 above and nothing else. The neighbouring CORE doc-comment rows (backlog lines
  43, 44, 47, 48), the enum-zero and rename rows (MASTER-178, MASTER-130) and the TEST sweep
  are out of scope.
- **Re-measure every row before planning around it.** The last four DIR sessions found a false
  or stale specific in 7/10, 5/8, 8/8 and 6/6 of their selected rows (brief `:128-135`). A row
  found already fixed is closed with its evidence and gets no work. A row whose premise is false
  is reported at Gate 2, not quietly re-scoped.
- R1 starts from the backlog's prescription: rules in the **existing FluentValidation
  validators**. Whether the EF configuration should also get `HasMaxLength`, which would need a
  real migration, is open. Settle it in spec from repo evidence (how `Name`'s limit is
  expressed in both layers today). If it would add a migration, raise it as an escalation
  (expensive to reverse).
- R4's rejection behaviour (throw versus a sentinel) is a design choice. Spec settles it from
  how sibling guards in the codebase reject `ObjectKind.None` (MASTER-073's precedent).
- Every changed guard needs a test that **fails against the unguarded code**. Each such
  criterion names an injection that is actually run, with a control that must pass. A test
  whose green state is reachable without the rule under test does not count.
- At Gate 3, the closed rows are recorded against their `.sir/backlog.md` entries.

### Standing constraints carried from `.sir/lessons.md` and memory

- Never run `sed -i` on a tracked file, and never use `Write` on an existing CRLF file. Both
  silently convert the whole file to LF, and under `IDE0055` that fails the build. State this
  in every dispatch, reviewers included.
- `Edit` strips trailing whitespace from `new_string`, so key whitespace-bearing edits on the
  line's first text character.
- Create the worktree under `C:\dm-wt\` (for example `C:\dm-wt\domain-invariants`). A
  scratchpad-length path breaks the WinUI XAML compiler with a misleading `input.json`
  MAX_PATH error.
- The solution is `src/DungeonMaster.sln`. Prove a clean build by quoting its summary lines,
  never with `grep -icE "warning|error"`, which returns 2 on a clean build. Treat
  `dotnet test --no-build` in an unrestored worktree as not run unless the output names a
  test count.
- After the merge, verify build, tests and line endings in the **main checkout**, not only
  the worktree.
- Verify every commit body with `git cat-file -p` against its message file, plus a
  tampered-copy control. Never use `git log --format=%B`.
- Commits on the session branch are authorised through the SIR flow. **Never push.**
