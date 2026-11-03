# Coverage

<!-- Definition of done: one row per spec.md item. `Item`, `Covering task`, and
     `Gate 2` are written at Gate 2; `End state` is written at Gate 3. -->

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 (S1) | T1 | covered | met (T1 criteria 1-5, 10/10 met) |
| D2 (S2) | T2 | covered | met (T2 criteria 1-4, 8/8 met) |
| D3 (S3) | T3 | covered | met (T3 criteria 1-4, 9/9 met) |
| D4 (S4) | T4 | covered | met (T4 criteria 1-9, 14/14 met) |
| D5 (S5) | T5 | covered | met (T5 criteria 1-3, 8/8 met) |
| D6 (S6) | T6 | covered | met (T6 criteria 1-5, 9/9 met) |
| D7 (S7) | T7 | covered | met (T7 criteria 1, 9) |
| D8 (S8) | T7 | covered | met (T7 criteria 2-6, 10-15) |
| D9, revert and `cmp` clauses | T1-T7 | covered | met (each task's injection/`cmp` criterion met: T1[4], T2[3], T3[3], T4[6], T5[2], T6[3], T7[8,15]) |
| D9, "no production file is in any task's commit" | orchestrator, from each commit's file list | covered by orchestrator; no task can run git | met — `git show --name-only` on 12ad838, 0821885, 08655b7, b30b5ce, 1bce997, 17a6c8f, 4cd3109 at Gate 3: each lists exactly one path, its task's owned test file under `src/DungeonMaster.Tests/` |
| D10 | T7 | covered | met (T7 criteria 17-18, reviewer-reproduced: build 0/0, tests 187/943/139 with 0 failed) |

<!-- Contract operations: one row per declared contract operation. `Operation`,
     `Consuming task`, and `Gate 2` are written at Gate 2; `Implementing task`
     and `End state` are written at Gate 3. -->

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|

No contracts are declared, so this table has no rows.
