# Coverage mapping - session 6b-xi

Written at Gate 2, **before** the approval that froze `manifest.json`. The `Gate 2` column was
complete then; the `End state` column was filled at Gate 3.

Produced by `dir-architect` with the draft manifest and written here by the orchestrator, which is
the party holding the user's Gate 2 decisions.

**Uncovered rows are rendered in full below the table.** Covered rows are summarised in one line, per
the framework: rendering every row of a clean decomposition trains its reader to skim, and the rows
that matter are the ones with no task against them.

Definition-of-done end states are a **join** over the per-criterion verdicts in `reviews/T1.md` and
`reviews/T2.md`. No new evidence was gathered at the fill.

---

## 1. Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. `None` cannot reach either consumer comparison | T2 (B1, B3) | covered | **met** |
| 2. Rejection follows the counted shape: dedicated arm, distinct catch-all message, `<exception>` block | T2 (B1, B2) | covered | **met** |
| 3. The dedicated `None` arm is detectably load-bearing | T2 (B4, B7) | covered -- predicted `met` | **met** -- prediction **confirmed** |
| 4. A value outside the declared members is rejected with its own message | T2 (B5) | covered | **met** |
| 5. `Added`, `Closed`, `Ignored` flow through unchanged | T2 (B6, B11) | covered | **met** |
| 6. Nothing else in `DungeonDetailPage.xaml.cs` changes | T2 (B11) | covered | **met** |
| 7. A future insertion or append fails loudly | T1 (A1, A2) | covered | **met** |
| 8. The cardinality guard is shown able to fail | T1 (A2) | covered | **met** |
| 9. `MapDrawingSession.cs` carries no permanent change | T1 (A3, A4) | covered | **met** |
| 10. `App` builds clean on the Windows TFM | T2 (B9) | covered -- B9 reworded at Gate 2, see note | **met** |
| 11. `DungeonMaster.Tests` green | T1 (A5) | covered | **met** |
| 12. `DungeonMaster.App.Tests` green | T2 (B10) | covered | **met** |
| 13. Every new and edited file is CRLF with a final newline | T1, T2 (A3, B8) | covered | **met** |
| U1. Full solution suite green in the main checkout after the merge | *none* | uncovered -- accepted as orchestrator work | see below |
| U2. `MASTER-179`'s index row closed, citing the merge commit | *none* | uncovered -- accepted as orchestrator work | see below |
| U3. `backlog.md:401` recorded as addressed by this session | *none* | uncovered -- accepted as orchestrator work | see below |
| U4. Every `MASTER-179` mention in `docs/audit/` re-read before the closure commit | *none* | uncovered -- accepted as orchestrator work | see below |
| U5. Drawing still works in the running application | *none* | uncovered -- routed to human observation | **met -- observed by the user** |
| U6. The guard's *call site* is itself unguarded | *none* | uncovered -- known residual, reported not closed | **confirmed by measurement** |

**All thirteen covered rows are `met`.** The six uncovered rows in full:

### U1 -- the post-merge suite run

No task builds `App.UnitTests`, and the full-solution run in the main checkout happens after every
task is done. **Accepted as orchestrator work** at Gate 2, declared there rather than discovered
here. Precedent: 6b-x declared the identical row at its Gate 2.

**End state: performed by the orchestrator after the merge.** The baseline it is measured against was
itself measured this session -- 969 passed / 0 failed at `061b28b` in the main checkout, with
`grep -icE "warning|error"` over the whole captured output returning 0 -- rather than carried from
the handoff.

### U2 -- the index closure

`MASTER-179`'s status cell must cite this session's merge commit. **Tasks cannot invoke git**, so no
criterion can produce a commit hash. **Accepted as orchestrator work.**

**End state: performed by the orchestrator after the merge.**

### U3 -- the backlog record

`backlog.md:401`'s cardinality item is addressed by T1 and must be recorded as such, naming the
session and its commit. Same reason as U2, and the file is outside every task's ownership set.

**End state: performed by the orchestrator after the merge.**

### U4 -- the whole-tree grep before the closure commit

6b-x closed two entries, then found a stale prose claim at `05-tests.md:1422` -- a line that had
never moved, so the index-arithmetic table would have "verified" it. No survey flags prose in a
document body. `grep -rn "MASTER-179" docs/audit/` and read **every** hit before committing, not only
the index row.

**End state: performed by the orchestrator before the closure commit.**

### U5 -- human observation in the running application

That drawing a polygon still adds vertices, still closes on the origin tap after three vertices, and
still ignores a repeated vertex, is a behavioural property of a WinUI window. **B6 and B11 are static
proxies for it** -- they check the guard's return values and the untouched branch text, not that the
canvas still draws. A static proxy can pass while the real property is broken, so this row was named
at Gate 2 and routed to human observation rather than closed on B6.

**End state: MET, by human observation, 2026-09-18.** The orchestrator rebuilt the app from the
committed state -- verifying the binary was newer than every source file, so no stale build was
observed -- launched it directly with `Start-Process` (**no Visual Studio, no debugger attached**),
and the user performed the pass. Confirmed:

- drawing adds vertices ("linear area drawing works as expected");
- a repeated vertex is ignored ("duplicate vertex taps don't visibly create more vertices");
- the polygon closes on the origin tap -- reported as completing, though slowly; see the latency
  finding carried to `backlog.md`;
- **the failure signature did not appear.** Had the guard misfired, the throw would land in the
  enclosing `try`/`catch` and surface as "Could not complete that map action: ...". The user
  confirmed no error message or banner surfaced.

**This row was closed on observation, never on B6 or B11**, which remain static proxies. That
distinction is the reason the row was created.

### U6 -- the call site is inspected, not tested

At Gate 2 this was an argument. **At Gate 3 it is a measurement.**

T2's reviewer deleted `.EnsurePlacementOutcome()` from `DungeonDetailPage.xaml.cs:1471` and ran the
full `App.Tests` suite: **`Failed: 0, Passed: 102, Total: 102`, exit 0, zero diagnostic codes.
Nothing reddens, and no analyser complains either.** The injection was reverted by re-editing
content, and the orchestrator re-verified the call site present at `:1471` before the commit.

So, stated precisely:

- the guard **function** is well covered -- B4, B5 and B6 were each demonstrated discriminating;
- the **application of the guard at the call site** is protected by nothing;
- a future edit deleting that call is invisible to the entire suite.

This is a permanent property of closing `MASTER-179` at a site where `None` is unreachable through
`PlaceVertex`, not a defect in this decomposition. **Reported, not closed.** The only durable
protection would be a static check -- an analyser or a grep-based guard asserting `PlaceVertex` is
never consumed unguarded -- which is carried to the backlog rather than taken here.

### Note on item 10 / B9

B9's original falsifiability command was `grep -icE "warning|error"` over a `dotnet build` output,
required to return 0. **That was unsatisfiable as written**: MSBuild always prints `0 Warning(s)` and
`0 Error(s)`, so the command returns 2 on a perfectly clean build -- verified by orchestrator probe
at Gate 2. The standard is correct for `dotnet test`, where it returned 0 over the full 969-test
baseline run, and had been carried across command types without being measured. **Reworded at Gate 2
before the freeze**, as an ordinary draft edit rather than an amendment, and ratified by the user.
The corrected form returned 0 in T2's run.

---

## 2. Contract operations

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|

**Empty, and that is a finding rather than an omission.** This session declares no contracts: the two
tasks own disjoint files in disjoint projects, and neither calls anything the other writes. T2
depends on T1 only because they share one worktree and one `obj/`/`bin/` and both build --
a serialisation dependency, not an interface one.

`manifest.json` records `"contracts": []`. Git does not track empty directories, so no `contracts/`
directory appears in the commit; that matches 6b-x and is not a missing artifact.

**No contract-operation search was run at Gate 3, because there are no operations to search for.**
That is the correct rendering of an empty declaration, not a skipped step.

The one interface either task touches, `MapDrawingSession.PlaceVertex`, is **unchanged by both** --
T1's A4 pinned it byte-identical by sha256, and the orchestrator confirmed it absent from both
commits -- and its single consumer at `DungeonDetailPage.xaml.cs:1471` is inside T2's ownership set.
