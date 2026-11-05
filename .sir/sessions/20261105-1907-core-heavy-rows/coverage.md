# Coverage

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 No CW90/CW180/CW270 under src; members Clockwise90/180/270 with accurate summaries | T1 | covered | met (reviews/T1.md criteria 1-4) |
| D2 Migration rewrites Rotation Up and Down; snapshot regenerated; no pending model changes | T1 | covered | met (reviews/T1.md criteria 5-11) |
| D3 Test proves the migration on old values (Up) and the reversal (Down) | T1 | covered | met (reviews/T1.md criteria 13-23; red step 21, injected defects 22-23) |
| D4 0-warning build, all tests pass, ratchet at 4 | T1 | covered | met (reviews/T1.md criteria 6, 12, 24, 25) |
| D5 MASTER-130 cell and CORE-024 record the cleared CW90 family; row stays part done | T4 | covered | met (reviews/T4.md criteria 1-4, 13, 16) |
| D6 PathSegmentKind persistence claim corrected | T4 | covered | met (reviews/T4.md criteria 5, 14, 15) |
| D7 MASTER-157 / CORE-031 closed deliberately unchanged | T2 | covered | met (reviews/T2.md criteria 1-19) |
| D8 MASTER-178 / CORE-035 closed deliberately unchanged with census and 12 dispositions | T3 | covered | met (reviews/T3.md criteria 1-31) |
| D9 CRLF kept, no lone CR introduced | T1, T2, T3, T4 | covered | met (reviews/T1.md 26-30; T2.md 11, 19; T3.md 11, 31; T4.md 10, 18) |

<!-- No contract declares a callable operation (C1-C4 are instruments, names, mappings and register text), so the
     operations table has no rows. -->

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
