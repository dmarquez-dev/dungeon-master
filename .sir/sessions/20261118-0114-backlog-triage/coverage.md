# Coverage

<!-- Definition of done: one row per spec.md item. `Item`, `Covering task`, and
     `Gate 2` are written at Gate 2; `End state` is written at Gate 3. -->

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. 132 open rows, one record block each (`union` 132 keys, 0 missing) | T1-T6 (criterion 1), T7 (criterion 1) | covered | met: T1-T6 criterion 1 (10/10, 10/10, 9/9, 9/9, 9/9, 10/10 met), T7 criterion 1 met; re-run at Gate 3 after the fix-now round (tip e0c782a), `C1-check.py union` printed `132 keys, 0 missing, 0 extra, 0 duplicated, 0 errors` |
| 2. Every record passes `C1-check.py record` with 0 errors | T1-T6 (criterion 1) | covered | met: T1-T6 criterion 1 met, each `C1-check.py record` 0 errors in its final review |
| 3. Every `remove` reproducible in the worktree | T1-T6 (criterion 3) | covered | met: T1-T6 criterion 3 met (reviewer-reproduced first `$ ` command of each remove; amended A2 under E4: a first command reading `.sir/backlog.md` or `.sir/lessons.md` is judged at `git show 4d874be:<path>`) |
| 4. `keep`, `rewrite`, `lesson` justified and shaped | T1-T6 (criteria 4-6) | covered | met: T1-T6 criteria 4-6 met (T1 criterion 5 met at revision round 1 re-review; T3 criterion 5 met at revision round 1 re-review) |
| 5. No triage task decides a `Decide` row | T1-T6 (criterion 2) | covered | met: T1-T6 criterion 2 met |
| 6. Decide rows asked at Gate 2; C2 filled before the freeze; `c2` prints 0 pending, 0 errors | orchestrator | uncovered: orchestrator's (Gate 2 D22); evidence at the freeze: `C1-check.py c2` printed `C1-check c2: 21 entries, 23 decide rows, 25 row sub-blocks, 0 pending, 0 errors` | met (orchestrator row, run at Gate 3): `C1-check.py c2` printed `C1-check c2: 21 entries, 23 decide rows, 25 row sub-blocks, 0 pending, 0 errors`, exit 0 |
| 7. Human/Appium-check rows rewritten as numbered steps with `Pass:` and a commit anchor | T1 (criterion 7), T2 (criterion 7), T6 (criterion 7) | covered | met: T1 criterion 7, T2 criterion 7, T6 criterion 7 met (reviewer-reproduced anchors) |
| 8. No new text names a row by line | T1-T6 (criterion 1, checker), T7 (criterion 2, `c2`) | covered | met: T1-T6 criterion 1 (checker LINE-REF) and T7 criterion 2 (`c2` 0 errors) met |
| 9. `.sir/backlog.md` equals the C1 section 6 construction | T7 (criteria 3, 4) | covered | met: T7 criteria 3, 4 met; re-run at Gate 3, `C1-check.py apply` printed `backlog 0 differing lines, lessons 0 differing lines` |
| 10. Emptied headings leave no doubled or trailing blank | T7 (criterion 4) | covered | met: T7 criterion 4 met (`layout 0 violations`, re-run at Gate 3) |
| 11. `.sir/lessons.md` original bytes plus appended lines only | T7 (criteria 7, 8) | covered | met: T7 criteria 7, 8 met (61480-byte prefix sha256 `6d185c70...adbe`; 18 appended lines after the fix-now round) |
| 12. CRLF = LF = CR in both files | T7 (criteria 5, 6) | covered | met: T7 criteria 5, 6 met (`101 101 101`, `167 167 167` after the fix-now round; re-run at Gate 3 by `apply`) |
| 13. MASTER-158 unchanged | T7 (criterion 9) | covered | met: T7 criterion 9 met |
| 14. Session diff touches only `.sir/` paths | T1-T7 (SCOPE criteria), orchestrator | uncovered: orchestrator's for the session-level half (Gate 2 D22); evidence: `git diff --name-only chore/repo-audit...<merge commit>` run in the main checkout at Gate 3, recorded in the report | met (orchestrator row, run at Gate 3 in the main checkout): `git diff --name-only 4d874be..agent/20261118-0114-backlog-triage` (tip e0c782a) printed 8 paths, all under `.sir/` (`.sir/backlog.md`, `.sir/lessons.md`, six `triage/rows-*.md`); the base `chore/repo-audit` is still `4d874be`, and after the merge `git rev-parse 671dcbf^{tree} e0c782a^{tree}` printed `4f618d36...` twice, so the merged tree is the verified tree; `git diff --name-only 4d874be..671dcbf` lists no path outside `.sir/` (count 0) |

<!-- Contract operations: one row per declared contract operation. `Operation`,
     `Consuming task`, and `Gate 2` are written at Gate 2; `Implementing task`
     and `End state` are written at Gate 3. -->

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| C1 record file (section 3 block format) | none: frozen contract | T7 | covered | applied: by the six T1-T6 record files, checked by `C1-check.py union` (132 keys, 0 errors at Gate 3) |
| `C1-check.py record` | none: frozen contract | T1-T6 | covered | applied: a checker run by command, not called from a file; whole-word `C1-check(.py) record` over the 8 touched files and `git grep C1-check` over the tip outside this session dir found 0 sites; run in every T1-T6 review (reviews/T1.md-T6.md criterion 1) |
| `C1-check.py union` | none: frozen contract | T7 | covered | applied: same search, 0 sites; run in reviews/T7.md criterion 1 and at Gate 3 |
| `C1-check.py apply` | none: frozen contract | T7 | covered | applied: same search, 0 sites; run in reviews/T7.md criterion 3 and at Gate 3 |
| `C1-check.py c2` | none: frozen contract | T7, orchestrator | covered | applied: same search, 0 sites; run in reviews/T7.md criterion 2 and at Gate 3 |
| `C1-check.py populations` | none: frozen contract | T1-T6 reviewers (test seam) | covered | applied: same search, 0 sites; run in every T1-T6 review |
| `C1-check.py self-test` | none: frozen contract | T1-T6 reviewers (test seam) | covered | applied: same search, 0 sites; run in every T1-T6 review and at Gate 3 (`20 passed, 0 failed`) |
| C2 entries D1-D21 | orchestrator (Gate 2) | T1-T6 (`decision` field), T7 (dispositions) | covered | applied: every Decide row recorded `decided` citing its entry (T1-T6 criterion 2); T7 applied the dispositions (`c2` 0 pending, `apply` 0 differing lines) |
