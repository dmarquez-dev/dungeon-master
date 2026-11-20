# Coverage

<!-- Definition of done: one row per spec.md item. `Item`, `Covering task`, and
     `Gate 2` are written at Gate 2; `End state` is written at Gate 3. -->

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 -- `CORE-015` Correction 4 anchor restated (I1) | T1 · C1-2, C1-6, C1-8 | covered | met (reviews/T1.md C1-2, C1-6, C1-8 met) |
| D2 -- `CORE-019` preamble qualifier (I2) | T1 · C1-3, C1-7, C1-8 | covered | met (reviews/T1.md C1-3, C1-7, C1-8 met) |
| D3 -- `CORE-035` twelves named apart, same set recorded (I3) | T1 · C1-4, C1-8, C1-9 | covered | met (reviews/T1.md C1-4, C1-8, C1-9 met) |
| D4 -- CRLF throughout, no lone CR | T1 · C1-1 | covered | met (reviews/T1.md C1-1 met) |
| D5 -- no pre-existing line changed | T1 · C1-5 | covered | met (reviews/T1.md C1-5 met) |
| D6 -- only the audit file changes across task commits | orchestrator: a task has no git | uncovered -- orchestrator's (accepted at Gate 2); evidence `git diff --name-only 9f9ca69..<T1 commit>` printing the one path, recorded in `report.md` section 3 | met -- orchestrator's row, evidence run at Gate 3: `git diff --name-only 9f9ca69..c3554cf` (main checkout) printed exactly `docs/audit/01-domain-application.md` |
| D7 -- ICC re-run on the main-checkout file after merge | orchestrator: post-merge, no task exists | uncovered -- orchestrator's (accepted at Gate 2); evidence ICC (C1 section 3) on `C:/Users/dmarq/Development/Repos/dungeon-master/docs/audit/01-domain-application.md` after the merge, its `CR`, `AT` and `BASE_MATCH` lines recorded in `report.md` | met -- applied after approval; ICC (sha256 71f0e4e7...f874) run at 2026-11-11T17:48Z on the main-checkout file at merge `4394b2f` printed `CR 3608 LF 3608 CRLF 3608 LONE_CR 0`, `AT I1 after base line 842 want 842 ok`, `AT I2 after base line 1418 want 1418 ok`, `AT I3 after base line 3391 want 3391 ok`, `BASE_MATCH yes`, `CITES 33 FAILED 0` (the approved `report.md` renders this row as of the gate) |
| D8 -- three backlog rows removed at Gate 3 | orchestrator: the edit happens at merge | uncovered -- orchestrator's (accepted at Gate 2); evidence `grep -c -F` of each row's text 1 before and 0 after, and a Python byte census CR = CRLF, recorded in this file's End state | met -- applied after approval; a Python line-substring count of each row's text printed 1, 1, 1 before and 0, 0, 0 after; byte census `CR 187 LF 187 CRLF 187 LONE_CR 0` before, `CR 185 LF 185 CRLF 185 LONE_CR 0` after; `git diff --numstat -- .sir/backlog.md` printed `2 4`: the three removals, plus the Gate 3 rewrite of the 20261106-0120-shipping-identity window-title row (check made and failed) and the appended A1 row |

<!-- Contract operations: C1 declares no callable operation. -->

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
