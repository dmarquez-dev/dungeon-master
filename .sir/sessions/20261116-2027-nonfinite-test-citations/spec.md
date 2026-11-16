# nonfinite-test-citations

Make every source-location citation in the doc comments of
`src/DungeonMaster.App.Tests/Graphics/FloorMapDrawableNonFiniteTests.cs` point at the code it
describes as of `40dddd1`, anchored on its member and statement so it can be found again after
the line numbers move, and make every claim those comments rest on true at `40dddd1`.

## Context

The test file's class remarks record a reachability finding: can `ZoomLevel` or `PanOffset` go
non-finite? Their citations come from `4031fc8`, where the comment was written. Session
20261103-2239 corrected the `ZoomLevel` text and left the rest unmeasured, and its backlog row
(the one that begins "Re-measure the line citations in the `FloorMapDrawableNonFiniteTests`
class remarks") asks for that remainder. At `40dddd1` the file carries 26 line citations on 14
`///` lines (C1 section 2, P-CITE, which a second predicate confirms). 22 of them have drifted,
and five claims are false or will become false once this task lands (C1 section 4.2).

## Definition of done

1. Every one of the 26 citations of C1 section 3 is rewritten to its `40dddd1` line, sits on a
   `///` line that also names its member or construct and its needles, and is labelled
   `as numbered at <c>40dddd1</c>` (check.py `P-ANCHOR missing rows 0` and
   `unlabelled citation lines 0`).
2. No citation token outside C1 section 5's allowed set remains (`P-ANCHOR stray tokens 0`).
3. The five false claims F1-F5 of C1 section 4.2 are corrected to what the code does (check.py
   P-CLAIM `OK` on all seven F lines).
4. The true claims K1-K9 of C1 section 4.1 stay in substance and are re-measured at the tip,
   each command printing the output C1 records.
5. Only `///` lines change (check.py `P-CODE` digest `c77569a9…058a`, 95 lines, unchanged).
6. Every rewritten line keeps dotnet-format section 9: a two-tab indent and breaks only at
   sentence boundaries (`P-FORMAT bad lines 0`).
7. The file stays all-CRLF with no lone CR (`P-BYTES` LF equal to CRLF, `loneCR 0`).
8. No file other than the test file changes (`git status --porcelain` in the worktree).
9. The test file carries no `.sir/` path (`grep -c -F '.sir/'` prints 0).
10. The solution build prints `Build succeeded.`, `0 Warning(s)` and `0 Error(s)`. App.Tests
    146/146, App.UnitTests 217/217 and DungeonMaster.Tests 1004/1004 still pass.
11. The findings F1-F5 (with the tip line of each corrected sentence) and the observations
    O1-O4 reach Gate 3.
12. At Gate 3, the backlog row that begins "Re-measure the line citations in the
    `FloorMapDrawableNonFiniteTests` class remarks" is removed from `.sir/backlog.md`, and that
    row is the only line the removal touches.

## Scope

One task, T1. It rewrites the doc comments of the test file per contract C1: the class summary,
the class remarks, and the per-test summaries at base lines 64 and 118.

### Not in scope

- Production code. Observations O1 (the unguarded throw at DDP `:1793`) and O3 (a possibly-NaN
  value reaching `SnapToGrid`) are reported and offered for the backlog at Gate 3. Neither is
  fixed. DoD 8 enforces this with `git status`.
- Test bodies, assertions, attributes and test names. DoD 5 enforces this with P-CODE.
- The other `.sir/backlog.md` rows, and any audit-register entry.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| What did the old `:1437` and `:1442` point to? | `var placeCanvas = _drawable.ScreenToCanvas(placeRaw);` and the `new CanvasPoint(placeCanvas.X, placeCanvas.Y)` argument to `PlaceObjectAsync`, now at DDP `:1595` and `:1600` | 1 repo | `git show 4031fc8:<DDP> \| grep -n` prints both at `:1437`/`:1442`; at `40dddd1`, `grep -n` prints them at `:1595`/`:1600` |
| Was item 5's `:508` wrong when it was written? | No. At `4031fc8` DDP `:508` is `_drawable.PanOffset = PointF.Zero;`. The 20261103-2239 correction renumbered items 3 and 4 and not item 5 | 1 repo | `git show 4031fc8:<test file> \| grep -n -o -E 'xaml.cs:5[0-9][0-9]'` prints `:500`, `:507`, `:508` |
| How does E1 ("enclosing member and directive") apply to the two `#if` spans? | They are class-level regions with no enclosing member, so the anchor names the directive and the member the region declares (`OnMapScrollWheel`, `OnMapPinchUpdated`) | 5 judgment | DDP `:664`, `:677`, `:693` (`grep -n`). This applies E1 by analogy (C1 O4) |
| How is the label written in an XML doc comment? | `as numbered at <c>40dddd1</c>`, because XML doc comments have no backtick markup | 1 repo | The file marks code with `<c>` throughout |
| Is the "no MAUI host" sentence (base line 9) re-checked? | Kept unchanged. It carries no line number and `FloorMapDrawableExecutionTests.cs:37-41` still builds the drawable with an object initializer | 1 repo | C1 K10 |
| Can two tasks build at once? | No: the build tree is shared. There is one task, so nothing needs ordering | 1 repo | Orchestrator's build probe in the worktree |
| What are the baseline test counts? | 217, 1004 and 146, measured at `40dddd1` in this worktree | 1 repo | `state.json` verification probe |
| Can `Edit` be used on the test file? | Yes, with the CRLF rules of C1 section 1. The file has 169 LF, 169 CRLF and 0 lone CR | 1 repo | check.py `P-BYTES` at `40dddd1` |

## Contracts

- [contracts/C1-citations-and-claims.md](contracts/C1-citations-and-claims.md): the editing
  rules, the citation census, the 26-row citation table, the claims re-check (K, F, O), the
  allowed tokens, the instruments (census.py, check.py) and the baseline.

## Open questions

- O3 (a possibly-NaN value reaching `SnapToGrid` in the Draw branch): carry it to the backlog
  or drop it. The user decides at Gate 3.
- O1 (the unguarded throw after the `PanOffset` write): carry it to the backlog or drop it. The
  user decides at Gate 3. Whether the comment must mention O1 is Gate 2's decision D2.
