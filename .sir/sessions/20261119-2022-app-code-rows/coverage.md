# Coverage

<!-- Definition of done: one row per spec.md item. `Item`, `Covering task`, and
     `Gate 2` are written at Gate 2; `End state` is written at Gate 3. -->

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. Row 14 ordering comment | T1 (c1-c3) | covered | met: T1 c1-c3, 3/3 met (reviews/T1.md) |
| 2. Row 16 source intent comment | T3 (c1-c2) | covered | met: T3 c1-c2, 2/2 met (reviews/T3.md) |
| 3. Row 16 test remarks | T4 (c1-c3) | covered | met: T4 c1-c3, 3/3 met (reviews/T4.md) |
| 4. Row 17 banner editor removed | T2 (c1-c7) | covered | met: T2 c1-c7, 7/7 met (reviews/T2.md) |
| 5. Row 85 renames | T1 (c4-c6) | covered | met: T1 c4-c6, 3/3 met (reviews/T1.md) |
| 6. Row 86 App renames | T1 (c7-c9) | covered | met: T1 c7-c9, 3/3 met (reviews/T1.md) |
| 7. Row 86 Tests renames | T4 (c4-c6) | covered | met: T4 c4-c6, 3/3 met (reviews/T4.md) |
| 8. Row 86 whole-row check in the main checkout | orchestrator (no task can run in the main checkout after the merge) | uncovered; orchestrator, evidence: `grep -rnw obj src/DungeonMaster.App src/DungeonMaster.Tests --include=*.cs --exclude-dir=obj --exclude-dir=bin` run in the main checkout after the merge prints exactly `RepoPaths.cs:67` and `RepoPathsTests.cs:195`, its output recorded in this cell | met (applied after approval): run in the main checkout at merge 51b935b, the grep printed exactly `src/DungeonMaster.Tests/App/Theme/RepoPaths.cs:67` and `src/DungeonMaster.Tests/App/Theme/RepoPathsTests.cs:195`, and nothing under src/DungeonMaster.App |
| 9. Row 90 FloorDropLanding remark | T3 (c3-c5) | covered | met: T3 c3-c5, 3/3 met (reviews/T3.md) |
| 10. No behaviour change: hashes, build 0/0, tests 217/1012/146 | T1-T4 (hash, BUILD and TESTS criteria) | covered | met: hash criteria T1 c3, c6, c9; T3 c2, c5 (T2 removes code and has none); T4 c3, c6; build T1 c11, T2 c9, T3 c7, T4 c8; tests T1 c12, T2 c10, T3 c8, T4 c9, all met (reviews/T1.md-T4.md) |
| 11. Line endings and BOM preserved | T1-T4 (line-ending criteria) | covered | met: T1 c10, T2 c8, T3 c6, T4 c7, all met (reviews/T1.md-T4.md) |
| 12. Six backlog rows removed | orchestrator (no task may own `.sir/backlog.md`; the removal is Gate 3's own edit) | uncovered; orchestrator, evidence: in the main checkout after the removal, a Python search for each of the six C1 keys prints no line number, and a byte count of `.sir/backlog.md` gives CR = CRLF = LF; both outputs are recorded in this cell | met (applied after approval): in the main checkout, a Python bytes search for each of the six row keys printed count 0 after the edit (each counted exactly 1 before it, asserted); `.sir/backlog.md` byte count CR 90 = CRLF 90 = LF 90 |
| 13. Human check of the banner title and both rotate controls | Gate 3 human-checks block (a person observes the running worktree build) | uncovered; orchestrator presents it at Gate 3 with the launch steps and the predicted appearance in spec item 13 | decided by human check H1 |

<!-- Contract operations: one row per declared contract operation. `Operation`,
     `Consuming task`, and `Gate 2` are written at Gate 2; `Implementing task`
     and `End state` are written at Gate 3. -->

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
