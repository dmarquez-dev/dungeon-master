sir-sdlc 1.0.54 — Gate 3 report — 20261123-0703-test-strength-rows

**Decisions needed:** none blocking — Gate 3 asks a disposition on 2 findings in section 4, and on 1 item the spec routed to the backlog.

## 2. Acceptance criteria

- **T1** · test · 8 criteria · 8 met
- **T2** · test · 6 criteria · 6 met
- **T3** · test · 6 criteria · 6 met
- **T4** · test · 7 criteria · 7 met
- **T5** · test · 9 criteria · 9 met

36 criteria, 36 met, none amended. 11 criteria rest on a demonstration, in the 11 rows below.

- **C1.5** · met · red step: Under the e951be1 regex body in the seam, fact (a) failed on FluentAssertions `ContainSingle` ("collection is empty", IconFontFixtureTests.cs:54) and fact (b) passed: `Failed: 1, Passed: 1, Total: 2`. Green: `Passed: 2`.
- **C2.2** · met · injected defect: before the test edit, the injected format-0 loop failed only `AFormat0SubtableCoversExactlyTheCodesWithANonZeroGlyph`, `misses {32, 65, 255}`, `Failed: 1, Passed: 10`.
- **C2.3** · met · injected defect, reviewer-reproduced: `EverySubtableFormatInOneCmapIsRead` failed with `... but it misses {32}`, and the format-0 unit fact failed too: `Failed: 2, Passed: 9, Total: 11`.
- **C3.2** · met · injected defect: saved output `inj1.txt`: `Expected result.Floors to contain exactly 2 items, but it contains 1 items`, `Failed: 1, Passed: 2`.
- **C3.3** · met · injected defect: saved output `inj2.txt`: `Expected floor to refer to ...FloorDto`, `Failed: 1, Passed: 2`, an assertion failure under the `with { }` value-equal copy.
- **C4.2** · met · injected defect: under each injection at :289 and :139 the implementer's runs `run1-gap-label.txt` and `run2-gap-button.txt` printed `Passed: 11, Total: 11` against the unstrengthened tests.
- **C4.3** · met · injected defect, reviewer-reproduced: `run3-disc-label.txt` printed `Failed: 1, Passed: 10`; the reviewer's re-injection gave "Expected handle.Name to be {…/maui}Label … but found {clr-namespace:DungeonMaster.Tests.NotMaui}Label".
- **C4.4** · met · injected defect, reviewer-reproduced: `run4-disc-button.txt` printed `Failed: 1, Passed: 10`, ExpandToggleIsAKeyboardFocusableButton failing at :68; the reviewer's re-injection gave a `{clr-namespace:…NotMaui}Button` mismatch.
- **C5.3** · met · injected defect, reviewer-reproduced: the injection `Touch(); return outcome;` at :464 failed exactly the five `_ReturnsNotFoundAndLeavesLastModifiedUnchanged` facts: `Failed: 5, Passed: 1, Total: 6`.
- **C5.4** · met · injected defect, reviewer-reproduced: the injection `_ = outcome;` at :467 failed only `UpdateArea_WithExistingFloorAndExistingArea_ReturnsSuccessAndAdvancesLastModified`: `Failed: 1, Passed: 5`.
- **C5.8** · met · reviewer-reproduced: the solution build gave `Build succeeded.` / `0 Warning(s)` / `0 Error(s)`; tests passed App.UnitTests 217, DungeonMaster.Tests 1014 and App.Tests 146.

Every row lives in `reviews/T1.md` to `reviews/T5.md`.

**Coverage, as mapped at Gate 2 and checked at Gate 3:** 6 of 8 definition-of-done rows met through their tasks (D1-D5, D7). Exceptions:

- **D6** · uncovered at Gate 2, orchestrator's row with evidence named · met: `git -C C:/dm-wt/20261123-0703-test-strength-rows diff --name-only main...HEAD` at tip 89e6aae printed 7 paths, all under `src/DungeonMaster.Tests/`.
- **D8** · uncovered at Gate 2, orchestrator's row with evidence named · applied after approval: the five-row backlog removal is this gate's own edit.

No contracts, so no contract operations.

**Limits:** none — every verification command ran in the worktree.

**Human checks:** this session asked for none. These checks are still open in `.sir/backlog.md`, each to be run from the main checkout after the merge, following the numbered steps in its own row:

- **2026-09-16-floor-sidebar-keyboard-route** · "Measure the floor-name column in DIU": build the app, read the column width, record it in DIU.
- **2026-09-18-drawsessionresult-none-rejection** · "Profile the 1-2 second UI stalls on closing a polygon, saving a floor rename and closing the dialog": in a Release build.
- **2026-09-16-floor-sidebar-keyboard-route** · "Run the drag-handle Appium ratchet": build the app, start Appium on 127.0.0.1:4723, build and run the UiTests harness.
- **20261103-2239-app-follow-ups** · "Make the solution build and F5 launch work in Visual Studio": open `src/DungeonMaster.sln` in Visual Studio and build, then F5.
- **20261112-0153-floor-name-tooltip** · "Measure the floor expand chevron's narrowed width with the Appium suite": run `FloorExpandToggleMeasuresAtLeast23HighAndAtMost40Wide` and read the logged `FloorExpandToggle[0] size = WxH` line.

## 3. What ran

- **T1** · sir-test-engineer, then sir-reviewer · test · opus / opus · 1 review · 0 rounds · 0 redispatches · e45624d
- **T2** · sir-test-engineer, then sir-reviewer · test · opus / opus · 1 review · 0 rounds · 0 redispatches · 6c30806
- **T3** · sir-test-engineer, then sir-reviewer · test · opus / opus · 1 review · 0 rounds · 0 redispatches · 7bc00d7
- **T4** · sir-test-engineer, then sir-reviewer · test · opus / opus · 1 review · 0 rounds · 0 redispatches · ccc3650
- **T5** · sir-test-engineer, then sir-reviewer · test · opus / opus · 1 review · 0 rounds · 0 redispatches · 89e6aae

Also dispatched: `init-session` (sir-git, haiku), `decomposition` (sir-architect, opus), and five `commit-<task>` operations (sir-git, haiku). No amendments.

**Change surface:** `e951be1..89e6aae` (`git merge-base main test/test-strength-rows` to the branch tip), 5 commits, 7 files changed, 214 insertions, 103 deletions. The full diff is `git diff e951be1..89e6aae`. Under PR mode GitHub merges the pull request after this gate, so there is no local merged tree to compare with the branch tip.

## 4. Advisory findings

- **Correctness risks: 1**
  - `src/DungeonMaster.Tests/App/Theme/IconFontFixture.cs:147-149`: the rethrown `XmlException(message, inner)` has `LineNumber` and `LinePosition` 0, though its message still carries the parser's line and column. No criterion is affected. (T1)
- **Session conduct: 1**
  - T4 reviewer: it sent the post-revert checks of its label and toggle injections in one parallel tool batch, so their order was not guaranteed. The run printed `Passed: 11`, which a live toggle injection would have failed, so only one injection was live at any time. Self-reported.
- **Test and CI concerns: 1**, in `reviews/T3.md`.
- **Cosmetics: 1**, in `reviews/T1.md`.

**Spec routing:** `spec.md` Not in scope routes one item to the backlog. `ThemeSource.QualifiedElementName` (`ThemeSource.cs:272-277` at 89e6aae) returns a bare local name for every namespace but XAML's, so the 8 element-selection filters built on it (`SemanticDescriptionTests.cs:397`, `ConfirmButtonStateParityTests.cs:273`, `IconButtonStyleParityTests.cs:444`, `ImplicitStyleTests.cs:382`, `:412`, `:448`, `ThemeDictionaryTests.cs:407`, `ThemeSource.cs:639`, from `git grep -n "QualifiedElementName(" test/test-strength-rows -- src`) would select a foreign-namespace `Style`, `Setter` or `Label` as a MAUI one.

## 5. Metrics

- **userInteractions** · 3 (Gate 1, Gate 2, Gate 3; the closeout question is inside Gate 3)
- **escalationsRaised** · 0
- **reviewRounds** · 0
