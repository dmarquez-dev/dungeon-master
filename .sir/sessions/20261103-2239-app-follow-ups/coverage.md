# Coverage

<!-- Definition of done: one row per spec.md item. `Item`, `Covering task`, and
     `Gate 2` are written at Gate 2; `End state` is written at Gate 3.

     Every `|` inside a cell is written `\|` — an evidence cell naming a
     `grep ... | wc -l` otherwise splits the row. After writing, check that
     every data row's field count equals the header's. -->

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 | T1 | covered | met (T1 C1-C4) |
| D2 | orchestrator: a hovered tooltip is not observable by any command | uncovered; orchestrator's, from the user's own hover observation in the running app, asked at Gate 3 and recorded verbatim | met (user, Gate 3, on the worktree build at 1e978a4: "both work as expected") |
| D3 | orchestrator + T2 (c3, handler unchanged) | covered (T2 c3); orchestrator's for the disposition, from `discussion.md` Decisions F2 and the `.sir/backlog.md` diff | met (handler: T2 C3; disposition: backlog row 195 removed at Gate 3 on the F2 decision, `grep -cF 'Confirm that Ctrl+Shift+0' .sir/backlog.md` → 0) |
| D4 | T2 | covered | met (T2 C1) |
| D5 | T2 | covered | met (T2 C2) |
| D6 | orchestrator: a live reset needs the app | uncovered; orchestrator's, from the user's Ctrl+0 observation in the running app, asked at Gate 3 | met (user, Gate 3, on the worktree build at 1e978a4: "both work as expected") |
| D7 | T2 | covered | met (T2 C4-C6) |
| D8 | T3 | covered | met (T3 C1) |
| D9 | T3 | covered | met (T3 C2-C5) |
| D10 | T3 | covered | met (T3 C6-C9) |
| D11 | T4 | covered | met (T4 C1-C2) |
| D12 | T5 | covered (E1: kept narrow at Gate 2) | met (T5 C1-C4) |
| D13 | T1, T2, T3, T5 | covered | met (T1 C6, T2 C9, T3 C5, T5 C7; T5 C7 on the E2 resolution) |
| D14 | T1, T2, T3, T4, T5 | covered | met (T1 C5, T2 C8, T3 C10, T4 C4, T5 C6) |
| D15 | orchestrator: `.sir/backlog.md` is written at Gate 3, never by a task | uncovered; orchestrator's, from `git diff --numstat -- .sir/backlog.md` in the record commit and the five rows' text absent by `grep -c` | met (orchestrator: `git diff --numstat -- .sir/backlog.md` → `5	5`; `grep -cF` on each of the five rows' opening words → 0, control row 199 → 1) |

<!-- Contract operations: one row per declared contract operation. `Operation`,
     `Consuming task`, and `Gate 2` are written at Gate 2; `Implementing task`
     and `End state` are written at Gate 3. -->

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|

No contracts are declared, so this table has no rows.
