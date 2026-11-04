# Coverage

<!-- Definition of done: one row per spec.md item. `Item`, `Covering task`, and
     `Gate 2` are written at Gate 2; `End state` is written at Gate 3. -->

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| MASTER-126: P1/P2 print nothing; 7 parameters renamed | T1 (2), T3 (3), T4 (2); T8 closing check | covered | met: T1 c1-c3, T3 c4-c5, T4 c4-c5, T8 c1 and c10 (P1/P2 0 on wt, P1 7 on base) |
| MASTER-128: PA prints nothing; `.Single()` in Arrange; diagnostic half STALE (`cf32061`) | T1; T8 (closing PA, STALE attribution) | covered | met: T1 c5-c7, T8 c2 and c11 (PA 0 on wt; STALE attributed to cf32061); precondition removals under A1/E1, T2 c2 and T3 c2 |
| MASTER-137: aaa.pl prints nothing over AppCore/Domain/Infrastructure | T2, T3, T4; T8 closing check | covered | met: T2 c1, T3 c1, T4 c1, T8 c3 (0/0/0 on wt against 126/63/16 on base); renamed bodies took AAA edits under A3/A4 (E2) |
| MASTER-139: five facts split (+9), injections J1-J10 | T2 (S5), T5 (S1-S4); T8 (952) | covered | met: T2 c6-c10, T5 c1-c19, T8 c13; T8's figure is 953 under A2 (952 plus the one E1 guard fact) |
| MASTER-143: exemption recorded in README | T7; T8 (DOC re-measure, cell) | covered | met: T7 c8-c9, T8 c6 (DOC 18 of 109 on wt and base) and c14 under A7 (E3: scoped "to close this row" claim, 7 session-added summaries disclosed) |
| MASTER-144: N1 = 0, N2 = 0, REF clean | T6 (58), T3 (2), T4 (1); T8 closing N1/N2 | covered | met: T6 c1 and c10, T3 c6-c9, T4 c6-c9, T8 c4, c5 and c15 (N1 0, N2 0, REF 0 on wt against 66 on base) |
| README per C3 | T7 | covered | met: T7 c1-c10 |
| Register: six cells and six AMENDED blocks | T8 | covered | met: T8 c10-c27 |
| Solution build clean; tests 952 / 187 / 123 | T8 (each task also asserts its own delta) | covered | met: T8 c7-c9 (build 0 warnings 0 errors; 953 / 187 / 123, 0 failed, reviewer-reproduced); 953 is 952 plus the E1 guard fact, per A2 |

<!-- Contract operations: none. C1-C5 are measurement and rule documents with no callable operations. -->

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| (none -- all five contracts are documents with no callable operations) | | | no operations declared | n/a |
