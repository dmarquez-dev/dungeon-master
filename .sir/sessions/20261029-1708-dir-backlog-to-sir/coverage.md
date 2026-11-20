# Coverage

<!-- Definition of done: one row per spec.md item. `Item`, `Covering task`, and
     `Gate 2` are written at Gate 2; `End state` is written at Gate 3. -->

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 181 backlog items, each once, one disposition | T1-T5 (criteria 2, 4), T8 (criterion 1) | covered | met |
| D2 17 register rows, each once, one disposition | T6 (criteria 2, 5), T8 (criterion 1) | covered | met |
| D3 every `drop` cites recorded evidence; undecidable is `carry` | T1-T5 (criteria 4-7), T6 (criteria 5, 11-13) | covered | met |
| D4 one entry line per `carry` / `defer-permanent` row | T8 (criterion 4) | covered | met |
| D5 date, session id, back-pointer and marker on every entry | T8 (criteria 3, 5-7); T1-T6 supply date and session (T1-T5 c3, T6 c4) | covered | met |
| D6 13 bundle headings, each entry under exactly one | T8 (criteria 8-10); T1-T6 assign bundles (T1-T4 c9, T5 c9 and c12, T6 c8-9) | covered | met |
| D7 `MASTER-158` permanently deferred, reason, register untouched | T6 (criteria 5-7), T8 (criterion 11) | covered | met |
| D8 lessons.md, all 18 forms by number | T7 (criteria 1-7, 11-12) | covered | met (criterion 11 met in round 2, after amendment A1) |
| D9 lessons.md, the nine process lessons | T7 (criteria 8-10) | covered | met |
| D10 nothing outside `.sir/` changes | T8 (criterion 13, reviewer read-only git); orchestrator re-measures after T8's commit; all tasks' preservation criterion | covered — predicted `met` | met — prediction confirmed: `git diff --name-only chore/repo-audit...sir/20261029-1708-dir-backlog-to-sir` at tip `ef847fc` lists 8 paths, `grep -c '^\.sir/'` 8, `grep -vc '^\.sir/'` 0 (control `src/b` counts 1) |

<!-- Contract operations: one row per declared contract operation. `Operation`,
     `Consuming task`, and `Gate 2` are written at Gate 2; `Implementing task`
     and `End state` are written at Gate 3. -->

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| `contracts/triage-file.md` — triage-file rows (layout, nine columns, error cases) | T1-T6 (the six `triage/*.md` files) | T8 | covered | called — `.sir/backlog.md` (T8): line 3 names the triage directory and every entry carries its row's key as `source:` |
| `contracts/bundles.md` — bundle ids and membership rules | T1-T6 (the `bundle` column of the six triage files) | T8 | covered | called — `.sir/backlog.md` (T8): 13 headings, verbatim from §1, at lines 5-213 |
