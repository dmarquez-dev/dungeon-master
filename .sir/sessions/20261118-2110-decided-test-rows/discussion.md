# Decided test rows

Clear the seven `.sir/backlog.md` rows whose remedy a 20261118-0114-backlog-triage gate already chose and whose change stays inside the test projects: rows at `.sir/backlog.md` lines 7, 32, 36, 37, 39, 44 and 50 as numbered at `f2aead0`.

## Context

The backlog triage session 20261118-0114-backlog-triage (merged and wrapped at `f2aead0`) re-measured every open row at `4d874be` and settled a remedy for many of them at its Gate 2 (D1-D21) or Gate 3. `grep -n -o -E '(Gate 2 D[0-9]+|Gate 3) of 20261118-0114-backlog-triage' .sir/backlog.md` at `f2aead0` prints 17 rows. The user asked for "the next batch of items"; this session takes the 7 of those 17 whose change stays inside `src/DungeonMaster.Tests` or `src/DungeonMaster.App.Tests` and whose check needs no Appium server.

Left out, by line at `f2aead0`: :18 (App code), :42 (needs the Appium server), :48 (a new shared source reader across three scanning classes), :56 and :58 (build config), :60 (a new source-scan test under the build heading), :92 (extends the AAA rule over the test projects), :94 (renames in App and Tests), :98 (App.Core remark), :101 (XAML re-indent).

Measured at `f2aead0` during discuss (the spec phase re-measures every row before decomposing):

- :7 - `git grep -n Task.Delay -- 'src/DungeonMaster.Tests*'` prints `Domain/Entities/EntityEqualityTests.cs:175` and `Domain/Entities/Floor/FloorMethodsTests.cs:18`.
- :32 - `git grep -n -w -e ThemeFileText -e KeyDefinitionPattern -e LegacyTolerantKeyDefinitionPattern -- src` prints `ThemeSource.cs:91`, `:98`, `:107`, `:239`, `:648` and `ThemeSourceTests.cs:452`, `:458`. Note `:98` and `:648` are doc-comment crefs to the deleted members, which the change must also remove or reword.
- :36 - `FloorMapDrawableDisposalTests.cs:238` is `Assert.NotEmpty(refilled);` and `:237` is blank, so no comment sits above it.
- :37 - not measured; `ThemeHygieneTests.cs` exists under `src/DungeonMaster.Tests/App/Theme/`.
- :39 - `git grep -n unconditional -- src/DungeonMaster.App.Tests/Graphics/FloorMapDrawableAllocationTests.cs` prints nothing.
- :44 - `git ls-files src/DungeonMaster.Tests/Domain/Entities` lists 9 files under the singular `Area/`, `Dungeon/` and `Floor/` folders (1, 2 and 6) plus 2 at the folder root.
- :50 - `Directory.Packages.props:21` pins FluentValidation `12.1.1`; `git grep -n 12.1.1` on `NonFiniteScaleRejectionTests.cs` prints nothing.

## Decisions

- Track: Standard (Gate 1, approved by the user).
- Scope: exactly the seven rows above; the ten others are out of scope.
- Each row's own remedy is the one its triage gate chose, as the row's text records it; this session does not reopen those decisions (D1, D6, D7, D8, D9, D10, and the Gate 3 choice to record the blind spot rather than add a check for row :39).
- Each row's own `check:` clause is the basis of its acceptance criterion, re-measured at the base.
- Row :44's folder rename is ordered after row :7's Task.Delay change, because both touch `Domain/Entities/Floor/FloorMethodsTests.cs`.
- Base: `chore/repo-audit` at `f2aead0`; worktree under `C:/dm-wt/`, branch `agent/20261118-2110-decided-test-rows`, per `.sir/config.json`.
- Architect decomposition on opus; one task per row unless rows sharing a file are merged.
- The seven rows are removed from `.sir/backlog.md` at Gate 3.
- Nothing is pushed unless the user chooses it at Gate 3.
- No human checks: no row changes app behaviour.
