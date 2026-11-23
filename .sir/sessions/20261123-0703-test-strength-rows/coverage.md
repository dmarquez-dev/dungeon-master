# Coverage

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 IconFontFixture XDocument walk, `>`-in-attribute fact, population 13 | T1 (C1.1-C1.6) | covered | met (T1 8/8 met, reviews/T1.md) |
| D2 cmap format-0 discriminated by a sub-0x41 code | T2 (C2.1-C2.4) | covered | met (T2 6/6 met, reviews/T2.md) |
| D3 handler in-place remove and replace caught | T3 (C3.1-C3.4) | covered | met (T3 6/6 met, reviews/T3.md) |
| D4 two element-type assertions require the MAUI namespace | T4 (C4.1-C4.5) | covered | met (T4 7/7 met, reviews/T4.md) |
| D5 no SpinUntil; five NotFound facts and the success fact discriminate | T5 (C5.1-C5.6) | covered | met (T5 9/9 met, reviews/T5.md) |
| D6 test-only diff | orchestrator: no task has git. Evidence is `git -C C:/dm-wt/20261123-0703-test-strength-rows diff --name-only main...HEAD` at the tip, recorded in this file's End state cell; supported by end-hash criteria C2.4, C3.4, C4.5 and C5.5 | uncovered; orchestrator, evidence named | met (orchestrator's row, run at Gate 3 at tip 89e6aae: the named command printed 7 paths, every one under `src/DungeonMaster.Tests/`) |
| D7 solution build 0/0 and tests 217/1014/146 at the tip | T5 (C5.8), last in the chain | covered | met (T5 C5.8 met, reviewer-reproduced, reviews/T5.md) |
| D8 Gate 3 removes the five rows | orchestrator: the backlog edit happens at Gate 3 and no task owns `.sir/backlog.md`. Evidence is the Gate 3 edit, checked by a grep for each row's session and subject, recorded in this file's End state cell | uncovered; orchestrator, evidence named | met (applied after approval: the Gate 3 edit removed backlog lines 28, 29, 30, 37 and 89; `grep -c -F` for each row's subject prints 0, and the control for the carried row prints 1) |

No contracts, so no contract operations.

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
