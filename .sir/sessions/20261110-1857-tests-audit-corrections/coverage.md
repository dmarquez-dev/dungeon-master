# Coverage

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 elided Validators path, R1 and TEST-001 block | T1 (C1-3, C1-6) | covered | met: C1-3, C1-6 (reviews/T1.md) |
| D2 coverlet clause, census recorded on TEST-032 | T1 (C1-7) | covered | met: C1-7 (reviews/T1.md) |
| D3a rename recorded on TEST-002 | T1 (C1-8) | covered | met: C1-8 (reviews/T1.md) |
| D3b rename recorded on XCUT-001 | T2 (C2-6) | covered | met: C2-6 (reviews/T2.md) |
| D3c rename recorded on XCUT-006 | T2 (C2-7) | covered | met: C2-7 (reviews/T2.md) |
| D3d the five citations left as written | T1 (C1-8), T2 (C2-8) | covered | met: C1-8 (reviews/T1.md), C2-8 (reviews/T2.md) |
| D4a TEST-023 reclassified DRIFT | T1 (C1-9) | covered | met: C1-9 (reviews/T1.md) |
| D4b TEST-030 reclassified DRIFT | T1 (C1-10) | covered | met: C1-10 (reviews/T1.md) |
| D4c TEST-029 closer, R2 | T1 (C1-3, C1-4, C1-11) | covered | met: C1-3, C1-4, C1-11 (reviews/T1.md) |
| D5a TEST-012 bold, R3 | T1 (C1-4, C1-12) | covered | met: C1-4, C1-12 (reviews/T1.md) |
| D5b XCUT-015 closer, R4 | T2 (C2-4, C2-9) | covered | met: C2-4, C2-9 (reviews/T2.md) |
| D6 line endings, both files | T1 (C1-1), T2 (C2-1) | covered | met: C1-1 (reviews/T1.md), C2-1 (reviews/T2.md) |
| D7 no other byte changed | T1 (C1-3), T2 (C2-3) | covered | met: C1-3 (reviews/T1.md), C2-3 (reviews/T2.md) |
| D8 house form | T1 (C1-5), T2 (C2-5) | covered | met: C1-5 (reviews/T1.md), C2-5 (reviews/T2.md) |
| D9 bold parity | T1 (C1-4), T2 (C2-4) | covered | met: C1-4 (reviews/T1.md), C2-4 (reviews/T2.md) |
| D10 only the two audit files change | orchestrator: no task has git | uncovered: orchestrator (approved at Gate 2), evidence `git diff --name-only $(git merge-base chore/repo-audit agent/20261110-1857-tests-audit-corrections) agent/20261110-1857-tests-audit-corrections` printing exactly the two audit paths | met (orchestrator, Gate 3, not a join): the Gate 2 command printed exactly `docs/audit/05-tests.md` and `docs/audit/06-cross-cutting.md`; merge-base `07f0512`, tip `453a168` (re-run after the Gate 3 fix) |
| D11 post-merge re-check, main checkout | orchestrator: runs after the merge | uncovered: orchestrator (approved at Gate 2), evidence TAC on both main-checkout files (census, CRLF = base + ADDED_LINES, `BASE_MATCH yes`) and BP `odd 0` | met (orchestrator, after the merge at `c3c2c40`): TAC on main-checkout 05 `CR 2535 CRLF 2535 LONE_CR 0`, `ADDED_LINES 151` (2384 + 151), `BASE_MATCH yes`; 06 `CR 963 CRLF 963 LONE_CR 0`, `ADDED_LINES 82` (881 + 82), `BASE_MATCH yes`; BP `odd 0` on both; control: 06 copy with a byte appended to line 10 printed `BASE_MATCH no` |
| D12 four backlog rows removed | orchestrator: Gate 3 act | uncovered: orchestrator (approved at Gate 2), evidence a lone-CR byte census before and after, the removed lines compared with the four rows' text, and a one-byte must-fail control | met (orchestrator, after approval): `.sir/backlog.md` lone-CR census `CR 186 CRLF 186 LONE_CR 0` before, `CR 187 CRLF 187 LONE_CR 0` after; the four removed lines matched the rows from 2026-09-11 (elided Validators path; coverlet clause), 2026-10-16-infra-layer-batch (DependencyInjection.cs rename) and 20261104-1726-test-hygiene-sweep (TEST-023/030 reclassification, TEST-029 closer); every other line byte-identical in order plus five Gate 3 appends; control: one flipped byte -> `False`; `git diff --numstat` `5 4` |

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| C1 section 2.1 block form | T1, T2 | T1, T2 | covered | applied: T1 (7 blocks), T2 (3 blocks); TAC `INSERTIONS 7` / `3`, `EARLY_CLOSE 0 TABS 0` (C1-2, C2-2) |
| C1 section 2.2 R1, R2, R3 | T1 | T1 | covered | applied: T1; TAC `REPL R1/R2/R3 NEW 1 OLD 0` (C1-3) |
| C1 section 2.2 R4 | T2 | T2 | covered | applied: T2; TAC `REPL R4 NEW 1 OLD 0` (C2-9) |
| C1 section 3 TAC | T1, T2 | T1, T2 (reviewers and orchestrator D11 re-run) | covered | applied: T1 and T2 reviewers (C1-1..3, C2-1..3); orchestrator D11 re-run after approval |
| C1 section 4 BP and `closers` | T1, T2 | T1, T2 | covered | applied: T1 and T2 reviewers; BP `odd 0` on both files (C1-4, C2-4) |
| C1 section 5.0 shared literals | T1, T2 | T1, T2 | covered | applied: T1, T2; ATTR and `as numbered at 07f0512` needles (C1-5, C2-5) |
| C1 sections 5.1-5.7 | T1 | T1 | covered | applied: T1 (C1-6..C1-12) |
| C1 sections 5.8-5.10 | T2 | T2 | covered | applied: T2 (C2-6, C2-7, C2-9) |
