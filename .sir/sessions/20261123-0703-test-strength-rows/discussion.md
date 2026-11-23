# test-strength-rows

Close five test-strength rows from `.sir/backlog.md`'s `## Test coverage and test strength` and `## Appended at Gate 3` sections, each a strengthening of an existing test or fixture in `src/DungeonMaster.Tests`, and remove those five rows at Gate 3.

## Context

The user asked to clear backlog work and take the next unit. The five rows were chosen because each strengthens a test or fixture already in `DungeonMaster.Tests`, needs no App host, Appium run or new production seam, and names its own red-on-defect check. Each was re-measured at `e951be1` (main, after preflight) during discuss; the measurements below were read with `git grep -n` or `sed -n` on single files at that commit.

The five rows, by their line in `.sir/backlog.md` at `e951be1`:

1. **`.sir/backlog.md:28`** (2026-09-02-app-sub-suite-rebuild) - `IconFontFixture` tag scan. Still open: `ElementTag` regex declared at `src/DungeonMaster.Tests/App/Theme/IconFontFixture.cs:56`, used at `:128`; `ThemeSource.LineNumber(` at `:146`; `ThemeSource.DecodeCharacterEntities(` at `:150`; `IconFontAttribute` at `:61`, `TextAttribute` at `:66` (both out of the row's scope and may stay). Row's check: `grep -n ElementTag` on that file prints nothing, and an icon-font element with `>` inside an attribute value is still reported.
2. **`.sir/backlog.md:29`** (2026-09-02-app-sub-suite-rebuild, MASTER-101) - `TrueTypeCmapTests.EverySubtableFormatInOneCmapIsRead`. Still open: the fact at `src/DungeonMaster.Tests/App/Theme/TrueTypeCmapTests.cs:172` feeds `Format0(0x41)` (`:175`), and the format-4 `Segments()` (`:274`) first segment covers `0x0041`-`0x0043` (`:279-280`); `AllFormatsExpected` at `:46-47` is `[0x41, 0x42, 0x43, 0x0100, 0xE000, 0xE002, 0x1F600]`. So deleting the format-0 arm leaves the union unchanged. Fix: feed a code below `0x0041` that no other subtable covers. The row carries "records undetermined".
3. **`.sir/backlog.md:30`** (2026-09-08-fixtures-and-de-duplication, MASTER-069) - `GetDungeonDetailQueryHandlerTests.HandleAsync_WithExistingId_ReturnsDtoFromRepository`. Still open: the dto at `src/DungeonMaster.Tests/Application/Features/Dungeons/Queries/GetDungeonDetailQueryHandlerTests.cs:24` passes `[]` for `Floors`; `BeSameAs(dto)` at `:36`. Row's check: a handler injected to remove or replace one element of that list turns the test red.
4. **`.sir/backlog.md:37`** (2026-09-16-floor-sidebar-keyboard-route) - `ThemeSource.QualifiedElementName`. Drifted, still open: the method is now at `src/DungeonMaster.Tests/App/Theme/ThemeSource.cs:272` (the row says `:311` at `4d874be`), the bare-local-name branch at `:274-276`; other uses at `:256` and `:639`, and a record parameter `QualifiedElementName` at `:38`/`:50`. `FloorDragHandleTests` calls it at `src/DungeonMaster.Tests/App/Layout/FloorDragHandleTests.cs:95`, with `"Label"` at `:98`. Row's check: a fixture element named `Label` in a non-MAUI namespace fails the strengthened assertion.
5. **`.sir/backlog.md:89`** (20261118-2110-decided-test-rows) - `DungeonLastModifiedOnFailedMutationTests`. Still open: `CaptureLastModifiedAndWaitForClockToAdvance` at `src/DungeonMaster.Tests/Domain/Entities/Dungeons/DungeonLastModifiedOnFailedMutationTests.cs:175`, `SpinWait.SpinUntil(` at `:179`; six callers at `:19`, `:39`, `:80`, `:101`, `:121`, `:148` (`git grep -n` over `src/DungeonMaster.Tests*`). The row names `EntityEqualityTests` and `Floors/FloorMethodsTests` as the reflection-backdate precedent; the spec phase reads them before citing. Row's check: `git grep -n SpinUntil -- 'src/DungeonMaster.Tests*'` prints nothing and each of the six still fails when a failed mutation updates `LastModified`.

## Decisions

- Track: Standard (Gate 1, user confirmed).
- Branch: `test/test-strength-rows`, `branchType` `test`; integration `pr`; the session reaches `main` by a PR the user opens, per `CLAUDE.md`.
- Scope is exactly the five rows above; no other backlog row is taken.
- Test-only: no production code changes. A row that turns out to need one is escalated, not taken.
- The `QualifiedElementName` strengthening covers the element-type assertions that depend on the bare name; the spec phase counts those call sites (at least `FloorDragHandleTests.cs:95-98`, and `ThemeSource.cs:256` and `:639`) and decides whether to change the helper or the assertions before freezing.
- Every task's discrimination is shown by injecting the defect into the code under test (or fixture input), not by perturbing the expected value, per `.sir/lessons.md:97`.
- Dispatch models: implementers and reviewers on opus, `sir-git` on haiku.
- Worktree under `C:/dm-wt/` per `.sir/config.json` (MAX_PATH).
- Gate 3 removes the five rows from `.sir/backlog.md` (lines 28, 29, 30, 37, 89 at `e951be1`); no heading is emptied, since both `## Test coverage and test strength` and `## Appended at Gate 3` keep other rows.
- No human checks: every claim is checkable by build, test and injected defect.
