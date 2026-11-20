# Coverage

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| DoD 1: 26 citations re-anchored and labelled | T1 (C1-1, C1-3) | covered | met (reviews/T1.md C1-1, C1-3) |
| DoD 2: no stray citation token | T1 (C1-2) | covered | met (reviews/T1.md C1-2) |
| DoD 3: F1-F5 corrected | T1 (C1-4 to C1-8) | covered | met (reviews/T1.md C1-4 to C1-8) |
| DoD 4: K1-K9 kept and re-measured | T1 (C1-9 to C1-16) | covered | met (reviews/T1.md C1-9 to C1-16) |
| DoD 5: only `///` lines change | T1 (C1-17) | covered | met (reviews/T1.md C1-17) |
| DoD 6: dotnet-format section 9 | T1 (C1-18) | covered | met (reviews/T1.md C1-18) |
| DoD 7: CRLF kept, no lone CR | T1 (C1-19, C1-20) | covered | met (reviews/T1.md C1-19, C1-20) |
| DoD 8: no other file changes | T1 (C1-21) | covered | met (reviews/T1.md C1-21) |
| DoD 9: no `.sir/` path in the test file | T1 (C1-22) | covered | met (reviews/T1.md C1-22) |
| DoD 10: build and three suites | T1 (C1-23 to C1-26) | covered | met (reviews/T1.md C1-23 to C1-26) |
| DoD 11: findings F1-F5 and O1-O4 reach Gate 3 | T1 (C1-27) | covered | met (reviews/T1.md C1-27; rendered in report.md section 2) |
| DoD 12: backlog row removed at Gate 3 | orchestrator (no task owns `.sir/backlog.md`) | uncovered: D1, orchestrator's at Gate 3; evidence `grep -c -F 'class remarks that lie outside' .sir/backlog.md` 1 to 0 and `git diff --numstat` on the backlog showing that one deletion beside any Gate 3 carry, recorded here and in the report; predicted met | met, applied after approval: `grep -c -F 'class remarks that lie outside' .sir/backlog.md` printed 1 before and 0 after; `git diff --numstat -- .sir/backlog.md` printed `2	1`, the one deletion being that row and the two insertions the Gate 3 carries of O1 and O3; CRLF 167 to 168, lone CR 0 |

C1 declares no runtime or contract operations. Its instruments (census.py, check.py, the K
commands) are measurement programs that T1 and its reviewer run, not operations one task
implements for another to call.

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
