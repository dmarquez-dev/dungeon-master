# Coverage

<!-- Definition of done: one row per spec.md item. `Item`, `Covering task`, and
     `Gate 2` are written at Gate 2; `End state` is written at Gate 3. -->

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 | T1 | covered | met (T1 criteria 8-13) |
| D2 | T2 | covered | met (T2 criteria 1-10, round 2) |
| D3 | T1 | covered | met (T1 criteria 1-7) |
| D4 | T2 | covered | met (T2 criteria 11-15, round 2) |
| D5 | orchestrator: stale row, no task can close it | uncovered; orchestrator's, from the evidence in `spec.md` Context D5 (`grep -rn 'CHECK' src/DungeonMaster.Infrastructure/Migrations` → 0, control over `.../Configurations` → 6; `git log -S` → 4be3fc0, 34f9feb, 375037d) | met — orchestrator's row, re-run at Gate 3 in the worktree at tip 6e1493f: `grep -rn 'CHECK' .../Migrations \| wc -l` → 0; control `grep -rn 'CHECK' .../Persistence/Configurations \| wc -l` → 6; `grep -rn 'HasCheckConstraint' .../Persistence/Configurations \| wc -l` → 0; `git log --format=%h -S"HasCheckConstraint" d2b84a2 -- .../Persistence/Configurations` → 375037d, 34f9feb, 4be3fc0, e9e5ae3, where `git show` counts e9e5ae3 as +3 (the additions) and each of the other three as −1 |
| Register (MASTER-180, CORE-036) | T3 | covered (kept at Gate 2) | met (T3 criteria 1-13) |
| No executable code change | T1, T2 | covered | met (T1 criteria 14-16; T2 criteria 8, 14, 21) |
| Line endings | T1, T2, T3 | covered | met (T1 17-19; T2 9, 15, 22; T3 7, 13) |
| Build and tests | T1, T2 | covered | met (T1 23-24; T2 23-24, reviewer-reproduced) |
| Backlog: D1-D6 removed, out-of-scope breaks added | orchestrator: `.sir/backlog.md` is written at Gate 3, never by a task | uncovered; orchestrator's, from the `.sir/backlog.md` diff in the record commit | met — orchestrator's row; `git diff --numstat -- .sir/backlog.md` → `2 6`: the six D1-D6 rows removed (base :17, :41, :42, :46, :48, :199), and the spec's out-of-scope sentence breaks plus the T3 `.sir/`-path item appended under `## Appended at Gate 3`; census CR=204 LF=204 CRLF=204 (208 − 6 + 2) |

<!-- Contract operations: one row per declared contract operation. `Operation`,
     `Consuming task`, and `Gate 2` are written at Gate 2; `Implementing task`
     and `End state` are written at Gate 3. -->

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|

C1 declares no operation, so this table has no rows.
