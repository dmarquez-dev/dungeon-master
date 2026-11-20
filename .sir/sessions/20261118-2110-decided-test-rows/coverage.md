# Coverage

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 No Task.Delay in DungeonMaster.Tests | T1 | covered | met (T1 13/13 met, reviews/T1.md) |
| D2 Reflection backdate to a fixed instant | T1 | covered | met (T1 13/13 met, reviews/T1.md) |
| D3 Each test fails with LastModified not updated (injection) | T1 | covered | met (T1 13/13 met, reviews/T1.md) |
| D4 No production clock seam; Entity.cs byte-identical | T1 | covered | met (T1 13/13 met, reviews/T1.md) |
| D5 Only plural folders, same 11 file names | T1 | covered | met (T1 13/13 met, reviews/T1.md) |
| D6 Plural namespaces | T1 | covered | met (T1 13/13 met, reviews/T1.md) |
| D7 No singular-namespace reference in src or docs | T1 | covered | met (T1 13/13 met, reviews/T1.md) |
| D8 Forced aliases dropped | T1 | covered | met (T1 13/13 met, reviews/T1.md) |
| D9 Moved content unchanged apart from listed categories | T1 | covered | met (T1 13/13 met, reviews/T1.md) |
| D10 Three ThemeSource members and their crefs gone | T2 | covered | met (T2 10/10 met, reviews/T2.md) |
| D11 ThemeDirectory and StaticResourceKeys private | T2 | covered | met (T2 10/10 met, reviews/T2.md) |
| D12 Pin test deleted (-1 test) | T2 | covered | met (T2 10/10 met, reviews/T2.md) |
| D13 IsLegacyTolerantKey keeps its reason | T2 | covered | met (T2 10/10 met, reviews/T2.md) |
| D14 ThemeTokenNotFoundException kept with a remark | T2 | covered | met (T2 10/10 met, reviews/T2.md) |
| D15 Comment directly above Assert.NotEmpty(refilled) | T3 | covered | met (T3 6/6 met, reviews/T3.md) |
| D16 Comment carries the vacuous-Assert.All reason | T3 | covered | met (T3 6/6 met, reviews/T3.md) |
| D17 Bare FontSize/FontFamily Setter reported | T4 | covered | met (T4 11/11 met, reviews/T4.md) |
| D18 Synthetic positive control through the shared probe | T4 | covered | met (T4 11/11 met, reviews/T4.md) |
| D19 Negative control silent | T4 | covered | met (T4 11/11 met, reviews/T4.md) |
| D20 Positive control shown able to fail | T4 | covered | met (T4 11/11 met, reviews/T4.md) |
| D21 StillOffends consistent with the widened probe | T4 | covered | met (T4 11/11 met, reviews/T4.md) |
| D22 Attribute-only doc comments corrected | T4 | covered | met (T4 11/11 met, reviews/T4.md) |
| D23 No current XAML fails | T4 | covered | met (T4 11/11 met, reviews/T4.md) |
| D24 unconditional remark inside the remarks block | T5 | covered | met (T5 6/6 met, reviews/T5.md) |
| D25 FluentValidation 12.1.1 and Directory.Packages.props named | T6 | covered | met (T6 6/6 met, reviews/T6.md) |
| D26 CRLF, lone CR and BOM state preserved | T1-T6 | covered | met (T1-T6 52/52 met, reviews/T1.md-T6.md) |
| D27 Per-task build 0/0 and tests pass with stated delta | T1-T6 | covered | met (T1-T6 52/52 met, reviews/T1.md-T6.md) |
| D28 Final full-solution build and test | T6 | covered | met (T6 6/6 met, reviews/T6.md) |
| O1 Seven rows removed from .sir/backlog.md | orchestrator | uncovered, orchestrator (Gate 2 D1): byte-level removal at Gate 3, each row's opening words grep 0 in `.sir/backlog.md` and every other line byte-identical to the pre-edit copy, lone CR counted before and after; recorded here | met: the seven rows removed by a byte-level edit after approval; each row's opening words count 0 in `.sir/backlog.md`, every kept line is byte-identical and in order (prefix check, with a one-byte-flip control that failed), 101 to 97 CRLF lines, lone CR 0 before and after, `git diff --numstat` `3 7` (orchestrator, Gate 3) |
| O2 No production file changes | orchestrator | uncovered, orchestrator (Gate 2 D1): `git diff --name-only f2aead0 <tip> -- src/DungeonMaster.Domain/ src/DungeonMaster.Application/ src/DungeonMaster.Infrastructure/ src/DungeonMaster.App/ src/DungeonMaster.App.Core/` prints nothing, plus `git status --short` clean in the worktree before Gate 3; recorded here | met: at tip 9293ae2 the production-path `git diff --name-only f2aead0 9293ae2 -- <5 production dirs>` printed nothing (control over `src/DungeonMaster.Tests/` printed 16 paths) and `git status --short --untracked-files=all` in the worktree printed nothing (orchestrator, Gate 3) |
| O3 Rename detected as renames | orchestrator | uncovered, orchestrator (Gate 2 D1): `git diff -M --name-status f2aead0 <T1 commit> -- src/DungeonMaster.Tests/Domain/Entities` shows 9 R and 1 M (EntityEqualityTests.cs), run through Python subprocess against MSYS path conversion; recorded in reviews/T1.md | not met: `git diff -M --name-status f2aead0 5724b3d -- src/DungeonMaster.Tests/Domain/Entities` printed 8 `R`, `M` EntityEqualityTests.cs, and `D`+`A` for Floor(s)/FloorMethodsTests.cs; at `-M30%` it pairs as `R047`, 9 `R` in all; `git log --oneline --follow` on Floors/FloorMethodsTests.cs at 9293ae2 \| wc -l prints 1, and 3 with `-M30%`; recorded here, not in reviews/T1.md as the Gate 2 cell said (orchestrator, Gate 3) |
| O4 Row's git ls-files check | orchestrator | uncovered, orchestrator (Gate 2 D1): `git ls-files src/DungeonMaster.Tests/Domain/Entities` at the tip lists only Areas/, Dungeons/, Floors/ and the 2 root files; recorded here | met: `git ls-files src/DungeonMaster.Tests/Domain/Entities` run in the worktree at 9293ae2 lists 11 paths, only Areas/, Dungeons/, Floors/ and the 2 root files; `git ls-tree -r --name-only 9293ae2` agrees (orchestrator, Gate 3) |
| O5 Rows' literal git grep checks | orchestrator | uncovered, orchestrator (Gate 2 D1): at the tip `git grep -n Task.Delay -- 'src/DungeonMaster.Tests*'` and `git grep -n -w -e ThemeFileText -e KeyDefinitionPattern -e LegacyTolerantKeyDefinitionPattern -- src` print nothing; recorded here | met: at 9293ae2 both `git grep` commands printed nothing (exit 1); controls at f2aead0 printed 2 and 7 lines (orchestrator, Gate 3) |
| O6 Two backlog rows appended at Gate 3 (Setter.Value element form; re-anchor 18 audit citations) | orchestrator | uncovered, orchestrator (Gate 2 D2, carry both): appended to `.sir/backlog.md` at Gate 3 as a byte-level append, each row worded as its action and named in the Gate 3 report; recorded here | met: both rows appended under `## Appended at Gate 3` in the same byte-level edit (the Setter.Value element form, and the 18 audit re-anchors), each worded as its action with its check, beside a third row the Gate 3 disposition carried (the SpinWait clock wait) (orchestrator, Gate 3) |

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
