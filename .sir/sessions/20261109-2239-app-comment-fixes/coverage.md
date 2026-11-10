# Coverage

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| `:44` `Paths` summary, no CS1591 for `ObjectAssetCatalogue.cs` | T1 (criteria 2-7) | covered | met (T1 criteria 2-7 met, reviews/T1.md) |
| Gate 2 D1: `ObjectAssetCatalogue.cs:7` cref qualified, no CS1574 for the file | T1 (criterion 23) | uncovered at draft; Gate 2 D1 folded it into T1 as criterion 23 | met (T1 criterion 23 met, reviews/T1.md) |
| `:47` false comment and alias replaced by plain using | T1 (criteria 8-11, 21) | covered | met (T1 criteria 8-11, 21 met, reviews/T1.md) |
| `:48` route and query key by cref; census prints only `ShellRoutes.cs:12` | T1 (criteria 12-19) | covered | met (T1 criteria 12-19 met, reviews/T1.md) |
| `:176` "corrected after", no other byte changed | T2 (criteria 2-4) | covered | met (T2 criteria 2-4 met, reviews/T2.md) |
| `:177` both `DrawString` overloads crefed; no doc-build warning for `RecordingCanvas.cs` | T2 (criteria 5-11) | covered | met (T2 criteria 5-11 met, reviews/T2.md) |
| Solution build 0/0 and suite 217/976/137 after each task | T1 (criteria 21-22), T2 (criteria 13-14) | covered | met (T1 criteria 21-22, T2 criteria 13-14 met; reviews/T1.md, reviews/T2.md) |
| Every edited file all-CRLF, no lone CR | T1 (criteria 11, 20), T2 (criteria 4, 12) | covered | met (T1 criteria 11, 20, T2 criteria 4, 12 met; reviews/T1.md, reviews/T2.md) |
| Five rows removed from `.sir/backlog.md` at Gate 3, no heading emptied | orchestrator: the backlog file is edited at Gate 3, after every task has committed | orchestrator; evidence: the five rows' leading text absent from `.sir/backlog.md`, `git diff --numstat` on it showing only the spec's removals plus any Gate 3 carries, and a Python byte count showing 0 lone CR | met after approval: a Python byte edit removed exactly lines 44, 47, 48, 176, 177 (each asserted by its leading text first); each leading text then counted 0, `git diff --numstat -- .sir/backlog.md` printed `0 5`, the file is 191 CRLF with 0 lone CR, and `## App, UI and UIKIT code` and `## Appended at Gate 3` both keep rows |

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
