# CORE doc-comment corrections

Correct six comments and XML doc blocks that the backlog records as false, self-falsifying,
or incomplete, mostly in the CORE layer (Domain, Application, Infrastructure) with one in
`DungeonMaster.App.Core`. Only comments and docs change. No executable code changes.

## Context

The repo-audit arc on `chore/repo-audit` schedules its work from `.sir/backlog.md`. At Gate 1
that file held 163 entries, by `grep -c '^20' .sir/backlog.md`. A second predicate,
`grep -c ' -- source: ' .sir/backlog.md`, also returned 163. The previous session,
`20261103-1654-test-strength`, closed the S1-S8 test-strength rows. Gate 1 offered four units:
CORE doc-comment corrections, the second half of test strength, the TEST style sweep
(MASTER-126/128/137/139/143/144), and the app-correctness Gate 3 follow-ups. The orchestrator
recommended **CORE doc-comment corrections**, and the user chose it.

The selected rows, by their line in `.sir/backlog.md` as read at Gate 1
(`grep -n "CORE-036\|ResultOfT.cs\|CanvasPoint.cs. doc comment\|concurrent first reads\|FORM 17) to\|AreaOutlineGeometry.Translate" .sir/backlog.md`):

| # | Backlog line | Source | Row (abridged; the backlog line is authoritative) |
|---|---|---|---|
| D1 | 17 | MASTER-180 (xref `.dir-sdlc/backlog.md:443`, `:499`) | Re-break each `///` line that splits a sentence mid-clause at a sentence boundary, without changing any words, across the lines `CORE-036` enumerates in `CanvasPoint.cs`, `PlacedObject.cs` and `ObjectKind.cs`. |
| D2 | 41 | `.dir-sdlc/backlog.md:423` (xref MASTER-044) | Reword the `Result<T>.Value` doc comment in `ResultOfT.cs`, which calls `Result<Guid>` "the only closed construction this application uses". Either scope it to production today or name its survey. |
| D3 | 42 | `.dir-sdlc/backlog.md:443` (xref MASTER-081, MASTER-180) | Correct the `NaN` paragraph of the `CanvasPoint.cs` doc comment so it no longer describes a `DbUpdateException` wrapper arriving at commit time. The finiteness guard made that unreachable. Keep its correct ADO-layer attribution. |
| D4 | 46 | `.dir-sdlc/backlog.md:469` (xref `.dir-sdlc/backlog.md:467`) | Reword the comment on `Area`'s cached `Outline` wrapper, which claims the wrapper is built at most once per `Area`. `??=` does not guarantee that under concurrent first reads. |
| D5 | 48 | `.dir-sdlc/backlog.md:517` (xref MASTER-152, `.dir-sdlc/backlog.md:509`) | Rescope the self-falsifying command annotations in the `Area`, `Dungeon` and `Floor` EF configuration comments (FORM 17) to populations that exclude `*Configuration.cs`, or state them as of a named commit. |
| D6 | 199 | `20261102-1907-domain-invariants` reviews/T5.md A1 | Add an `<exception cref="ArgumentOutOfRangeException">` entry to `AreaOutlineGeometry.Translate`'s XML doc. It can now throw that with ParamName `Kind` for an undefined `PathSegmentKind`, through the `PathSegment` constructor. |

These are the orchestrator's Gate 1 spot-checks, not measurements. Each one is a lead for the
spec phase to re-measure:

- D2: `src/DungeonMaster.Application/Common/Results/ResultOfT.cs:38` reads
  `For <c>Result&lt;Guid&gt;</c>, the only closed`, and the sentence continues past that line.
  A first grep for the literal phrase "only closed construction" returned nothing because
  the phrase is split across lines, so a phrase grep is the wrong instrument here.
- D3: `src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs:18` still says
  `a <see cref="float.NaN"/> throws on save — a <c>DbUpdateException</c> wrapping`.
- D4: `src/DungeonMaster.Domain/Entities/Areas/Area.cs:24` still says
  `it is still built at most once per Area.`
- D5: the annotation `` `grep -rn 'CHECK' src/DungeonMaster.Infrastructure/Migrations` `` is at
  `AreaConfiguration.cs:81`, `DungeonConfiguration.cs:36` and `FloorConfiguration.cs:44`, all
  under `src/DungeonMaster.Infrastructure/Persistence/Configurations/`. The cited population
  is `Migrations/`, not `Configurations/`. So the spec phase must first decide whether the
  comment really falsifies itself (FORM 17 needs the commented file to be inside the command's
  population) or whether the row is stale or misdescribed. The command also says `returns 0`,
  and that should be re-run.
- D6: `src/DungeonMaster.App.Core/Map/AreaOutlineGeometry.cs` declares `Translate` at `:32`.
  Its doc block has exceptions at `:25` (`ArgumentNullException`) and `:29`
  (`ArgumentException`) and no `ArgumentOutOfRangeException`. Whether `PathSegment`'s
  constructor really throws it with ParamName `Kind` was not checked.
- D1: not spot-checked. `CORE-036`'s line list is in `docs/audit/01-domain-application.md`. It
  may be stale, because D3 also edits `CanvasPoint.cs`'s doc block.

## Decisions

- Track: **Standard**. There are six separate rows across four projects, and each row's
  closing claim (this comment is now true and in the house style) is a separate claim someone
  can check.
- Session name: `core-doc-comments`.
- Scope is D1-D6 above and nothing else. Out of scope: backlog line 45 (guard or pin `Area`'s
  `_outline` invariant), which is a code and test change, not a comment change. Also out of
  scope: the App-layer `///` reflow rows (lines 77 and 78) and the remaining test-strength rows.
- **No executable code changes.** Only `//` comments and `///` XML doc text may change. The
  compiled IL must be unaffected apart from the XML documentation output. If a row turns out
  to need a code change, the spec phase records that and routes the row to the backlog. It
  does not widen the scope.
- **D1 and D3 overlap in `CanvasPoint.cs`'s doc block.** The spec phase must put them in one
  task, or order them, so the re-break is applied to the corrected text.
- **D5 may be stale or misdescribed** (see context). If re-measurement shows the comments do
  not falsify themselves, close the row with that evidence instead of editing.
- Every row is re-measured before work. A row found already fixed is closed with evidence,
  not re-done.
- Conventions: `C:\Users\dmarq\Development\Repos\dm-conventions\dotnet-format.md` governs doc
  comments. Its rule that every `///` line is a whole sentence (`dotnet-format.md:378`, per
  backlog line 77) is the one D1 applies. Read it before any edit.
- Line endings: the edited `.cs` files are CRLF. No `sed -i`, no `Write` on an existing file,
  and no `Edit` whose `new_string` ends in whitespace (see `.sir/lessons.md`). Prove CRLF
  preservation with a byte census after the change.
- Worktree under a short root (`C:\dm-wt\<id>`), per `.sir/lessons.md`'s worktree-root lesson.
- On close, remove D1-D6 from `.sir/backlog.md`. The spec phase decides whether the
  register (`docs/audit/00-master-index.md`, MASTER-180 for D1) and `CORE-036` also need
  updating, following what earlier sessions did when they closed a MASTER row. Gate 1 did not
  settle this.
