# Coverage

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 no `CancellationToken ct` left; 28 `cancellationToken` declarations in the same 18 files | T1 c2, c3 | covered | met -- T1 c2, c3 (reviews/T1.md) |
| D2 15 tags renamed and paired | T1 c4, c5 | covered | met -- T1 c4, c5 |
| D3 only the NotebookView comment remains as a `ct` word | T1 c6 | covered | met -- T1 c6 |
| D4 11 lambdas and 2 comments renamed `placedObject`; OBJ-WORD prints only `Entity.cs` | T2 c2–c5 | covered | met -- T2 c2-c5 (reviews/T2.md) |
| D5 EF model unchanged, with a must-fail control | T2 c8, c9 | covered | met -- T2 c8, c9 |
| D6 owned files equal the exact word rename (C1 §4.4 digests) | T1 c7, T2 c6 | covered | met -- T1 c7, T2 c6 |
| D7 build 0/0; suite 217/976/137 | T1 c11, c12; T2 c11, c12 | covered | met -- T1 c11, c12; T2 c11, c12 |
| D8 CRLF, 0 lone CRs, first bytes kept | T1 c8; T2 c7; T3 c23 | covered | met -- T1 c8; T2 c7; T3 c23 (reviews/T3.md) |
| D9 row 318 done, figures measured, previous wording whole | T3 c5–c8, c10, c12, c14 | covered | met -- T3 c5-c8, c10, c12, c14 |
| D10 CORE-024 block | T3 c15, c16 | covered | met -- T3 c15, c16 |
| D11 row 269 DELIBERATELY UNCHANGED | T3 c2–c4, c9, c11, c14 | covered | met -- T3 c2-c4, c9, c11, c14 |
| D12 CORE-016 block with moved citations | T3 c17–c20 | covered | met -- T3 c17-c20 |
| D13 nothing else changes; no scratch path | T3 c13, c21, c22, c24, c25 | covered | met -- T3 c13, c21, c22, c24, c25 |
| D14 backlog `:15`, `:16`, `:22`, `:24` removed; one `obj` item appended; the rest byte-identical; `:13` kept | orchestrator: no task owns `.sir/` | uncovered; orchestrator at Gate 3 (Gate 2 decision), evidence = a `python -I` byte comparison showing the post-file equals the startRef file with exactly those four lines removed and the one item appended, plus B (197 → 194 CRLF, 0 lone CR) | met -- orchestrator, after approval: a `python -I` comparison shows post-edit `.sir/backlog.md` lines 1-193 equal `git show HEAD:.sir/backlog.md` (split on LF; 197 lines) with exactly lines 15, 16, 22, 24 removed, then 3 appended rows (the spec obj item plus advisories A1 and A2 that Gate 3 carried, so 196 CRLF, not the criterion's 194), 0 lone CR; heading `:13` kept; control: dropping 23 in place of 24 does not match |
| D15 session diff touches only the 23 owned paths | orchestrator: tasks have no git | uncovered; orchestrator at Gate 3 (Gate 2 decision), evidence = `git diff --name-only 91bbbf4..<tip>` compared to the union of the three `owns` lists | met -- orchestrator: `git diff --name-only 91bbbf4..agent/20261109-1641-part-done-rows` from the main root prints 23 paths; `diff` against the sorted union of the three manifest `owns` lists (18 + 3 + 2 = 23, no globs) prints nothing; control: the first 22 diff paths against the same union differ |

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
