# Coverage

<!-- Definition of done: one row per spec.md item. `Item`, `Covering task`, and
     `Gate 2` are written at Gate 2; `End state` is written at Gate 3. -->

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 FloorObjectMethods.cs gone; PC prints the eight C1 §10 lines | T1 (criteria 1, 5) | covered | met (reviews/T1.md criteria 1, 5) |
| D2 three entity files equal the C2 composition (E) | T1 (criteria 2-4) | covered | met (reviews/T1.md criteria 2-4) |
| D3 no member line of Floor or PlacedObject changed (M) | T1 (criteria 6-7) | covered | met (reviews/T1.md criteria 6-7) |
| D4 ResultOfT.cs, AreaOutlineGeometry.cs reflowed (P2, S empty; W, K, I1, L unchanged) | T2 (criteria 1-12) | covered | met (reviews/T2.md criteria 1-12) |
| D5 ShellRoutes.cs, ObjectPaletteView.xaml.cs reflowed, :309-310 included | T3 (criteria 1-12) | covered | met (reviews/T3.md criteria 1-12) |
| D6 changed and new .cs files CRLF, loneCR 0; new file no BOM | T1 (8-11), T2 (13-14), T3 (13-14) | covered | met (reviews/T1.md 8-11, T2.md 13-14, T3.md 13-14) |
| D7 build 0/0 and tests 187/953/123 after each code task | T1 (12-13), T2 (15-16), T3 (15-16) | covered | met (reviews/T1.md 12-13, T2.md 15-16, T3.md 15-16) |
| D8 MASTER-110 cell done with measured evidence; row shape kept | T4 (criteria 1-11) | covered | met (reviews/T4.md criteria 1-11) |
| D9 CORE-022 AMENDED block, figures classified DRIFT | T4 (criteria 12-26) | covered | met (reviews/T4.md criteria 12-26) |
| D10 commits touch only the ten owned paths | orchestrator | uncovered: orchestrator's -- a task has no git; filled at Gate 3 from `git diff --name-status 11f9562..agent/20261105-0106-core-two-row-sweep` (predicted: covered) | met, prediction confirmed -- orchestrator's own row, run at Gate 3 from the main root: `git diff --name-status 11f9562..agent/20261105-0106-core-two-row-sweep` printed 10 entries (8 M, D `FloorObjectMethods.cs`, A `PlacedObjectMethods.cs`), exactly T1's 4, T2's 2, T3's 2 and T4's 2 owned paths; `git diff --name-only <same range> \| grep -cE '\.csproj$\|Tests?/\|/Floor\.cs$'` printed 0, and its control (a Floor.cs, a Tests/ path, a .csproj and the near-miss FloorMethods.cs) printed 3 |
| D11 backlog rows :9, :34, :65, :191 removed, :7 kept | orchestrator | uncovered: orchestrator's -- the review phase's backlog edit at Gate 3, evidence the diff of `.sir/backlog.md` in the artifact commit | met -- orchestrator's own row, filled after approval and before the artifact commit: `diff` of `.sir/backlog.md` against its pre-edit copy prints `9d8`, `34d32`, `65d62`, `191d187` (the four rows, each confirmed by its text) and `201a198,200` (the three Gate 3 appends under `## Appended at Gate 3`); the `:7` heading `## CORE two-row sweep: MASTER-110, 180` is kept, and one of the two blank lines its emptied section left was removed; byte census CR=199 LF=199 CRLF=199, loneCR 0 |

<!-- Contract operations: one row per declared contract operation. `Operation`,
     `Consuming task`, and `Gate 2` are written at Gate 2; `Implementing task`
     and `End state` are written at Gate 3. -->

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| none -- C1, C2 and C3 declare instruments and target compositions, no callable operation | | | n/a | n/a -- no operation to search for |
