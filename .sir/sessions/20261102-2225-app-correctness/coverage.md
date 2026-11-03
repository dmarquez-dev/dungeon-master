# Coverage

<!-- Definition of done: one row per spec.md item. `Item`, `Covering task`, and
     `Gate 2` are written at Gate 2; `End state` is written at Gate 3. -->

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 well-formed missing id -> Guid.Empty + NotFound error | T1 | covered | met (T1 c1, c2) |
| D2 saves dispatch nothing in that state | T1 | covered | met (T1 c3) |
| D3 reload-after-delete path | T1 | covered | met (T1 c4, c5) |
| D4 create clears stale error | T2 | covered | met (T2 c1, c2, c5) |
| D5 detail-page StartSessionButton disabled | T3 | covered | met (T3 c1-c8) |
| D6 list-page StartSessionButton disabled | T3 | covered (Gate 2: included) | met (T3 c9-c14) |
| D7 reorder chord exact Control | T4 | covered | met (T4 c1-c7) |
| D8 Ctrl+0 exact Control | T4 | covered (Gate 2: included) | met (T4 c8, c9) |
| D9 non-finite zoom rejected | T5 | covered | met (T5 c1-c11) |
| D10 OnMapCanvasHandlerChanged guarded from first statement | T5 | covered; predicted structural-only | met (T5 c12-c17); prediction confirmed — evidence is the structural scan only (reviews/T5.md human observations) |
| D11 mixed indent :616 | T5 | covered (Gate 2: included) | met (T5 c18; line now :617) |
| D12 injections run with controls | T1-T5 | covered | met (T1 c6-c7, T2 c3-c4, T3 c8/c14, T4 c6-c7, T5 c10/c17) |
| D13 clean build, tests, CRLF per task | T1-T5 | covered | met (T1 c10-c13, T2 c6-c9, T3 c15-c18, T4 c10-c13, T5 c19-c22) |

<!-- Contract operations: one row per declared contract operation. `Operation`,
     `Consuming task`, and `Gate 2` are written at Gate 2; `Implementing task`
     and `End state` are written at Gate 3. -->

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| (none: no contracts declared) | | | | |
