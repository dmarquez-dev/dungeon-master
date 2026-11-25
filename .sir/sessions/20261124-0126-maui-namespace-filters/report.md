sir-sdlc 1.0.55 — Gate 3 report — 20261124-0126-maui-namespace-filters

**Decisions needed:** none blocking — Gate 3 asks a disposition on 1 finding in section 4.

## 2. Acceptance criteria

- **T1** · test · 27 criteria · 27 met
- **T2** · test · 19 criteria · 19 met

46 criteria, 46 met, none amended. 32 criteria rest on a demonstration, in the 28 rows below.

- **T1-03** · met · reviewer-reproduced: `.Throw<System.Xml.XmlException>()` for `"x:Style"`; t103.txt Total 1 passed; reviewer confirmed `XName.Get("x:Style", ns)` throws XmlException on .NET 10.0.9.
- **T1-04, T1-05** · met · injected defect: injH-red.txt: IsMauiElementMatchesOnlyTheMauiDefaultNamespace fails, `Failed: 2 ... Total: 2`, site ThemeSource.cs:316; same run: IsMauiElementThrowsXmlExceptionForAPrefixedName fails, "no exception was thrown".
- **T1-08** · met · injected defect: inj382-red.txt `Failed: 1, Total: 1` at ImplicitStyleTests.cs:516.
- **T1-11** · met · injected defect: inj412-red.txt `Failed: 1, Total: 1` at ImplicitStyleTests.cs:549 (only the VisualStateGroup assertion exercised; disclosed).
- **T1-14** · met · injected defect: inj448-red.txt `Failed: 1, Total: 1` at ImplicitStyleTests.cs:588.
- **T1-17** · met · injected defect: inj407-red.txt `Failed: 1, Total: 1` at ThemeDictionaryTests.cs:458.
- **T1-19** · met · reviewer-reproduced: criterion pipeline prints 3; control `grep -cF 'QualifiedElementName(attribute.Name) =='` prints 1.
- **T1-20** · met · reviewer-reproduced: criterion pipeline on ThemeDictionaryTests.cs prints 1.
- **T1-21** · met · reviewer-reproduced: all four `grep -cF` print 1; ThemeSource.cs diff is additions only.
- **T1-22** · met · reviewer-reproduced: doc comments of SetterProperties, XamlNames (summary and `localName` param), IsStyle, MergedThemeSourcesIn state the MAUI default namespace; `grep -c 'spells it'` prints 0.
- **T1-22b** · met · reviewer-reproduced: `grep -cF '.sir/'` 0 on each of the four; control `git show 452b18f:.sir/lessons.md | grep -cF '.sir/'` prints 24.
- **T1-23** · met · reviewer-reproduced: CR=LF=CRLF 801, 510, 592, 527; fixture CR=2 LF=2 CRLF=1.
- **T1-24** · met · reviewer-reproduced: `grep -c '^ '` 0 on all four; App.xaml control 20.
- **T1-25** · met · reviewer-reproduced: reviewer's `dotnet build <sln>` exit 0, `0 Warning(s)`, `0 Error(s)`.
- **T1-26** · met · reviewer-reproduced: reviewer's `dotnet test DungeonMaster.Tests.csproj` `Passed!  - Failed:     0, Passed:  1020, Skipped:     0, Total:  1020`; `[Fact`/`[Theory` counts 14→16, 5→8, 6→7, 1→1 (+6); 0 removed `[Fact`/`[Theory` lines in the diff.
- **T2-01** · met · reviewer-reproduced: signature at ConfirmButtonStateParityTests.cs:322-324 `(XElement element, string localName)`, body :326 returns `ThemeSource.IsMauiElement(element, localName)`; pipeline prints 1, AttributeValue control prints 1.
- **T2-03, T2-04** · met · injected defect: inj1-red.txt `Failed: 1, Total: 1` with the IsNamed body at :326 replaced; Style assertion failed with the clr-namespace Style also accepted; read-back cmp empty; inj1-post.txt Failed 0 / Passed 1 / Total 1.
- **T2-05** · met · reviewer-reproduced: signature at IconButtonStyleParityTests.cs:493, body :497 calls IsMauiElement; pipeline prints 1, AttributeValue control prints 1.
- **T2-07, T2-08** · met · injected defect: inj2-red.txt `Failed: 1, Total: 1` with the IsNamed body at :497 replaced; read-back cmp empty; inj2-post.txt Failed 0 / Passed 1 / Total 1.
- **T2-09** · met · reviewer-reproduced: SemanticDescriptionTests.cs:430-433 selects via `ControlElements.Any(name => ThemeSource.IsMauiElement(element, name))`; pipeline prints 1; maui-URI control prints 1.
- **T2-11, T2-12** · met · injected defect: inj3-red.txt `Failed: 1`, actual {3,4,5,6}, with the 452b18f condition at :430; read-back cmp empty; inj3-post.txt Failed 0 / Passed 1 / Total 1.
- **T2-13** · met · reviewer-reproduced: both IsNamed summaries and the ControlElements summary (:37-41) name "the MAUI default namespace"; `grep -c 'with its XAML prefix restored'` prints 1 and 1 (AttributeValue).
- **T2-13b** · met · reviewer-reproduced: `grep -cF '.sir/'` 0, 0, 0; lessons.md control 24.
- **T2-14** · met · reviewer-reproduced: CR/LF/CRLF 345/345/345, 516/516/516, 600/600/600 by the C2 section 1 commands.
- **T2-15** · met · reviewer-reproduced: `grep -c '^ '` 0, 0, 0.
- **T2-16** · met · reviewer-reproduced: reviewer's solution build `0 Warning(s)`, `0 Error(s)`.
- **T2-17** · met · reviewer-reproduced: reviewer's solution run: DungeonMaster.Tests Failed 0, Passed 1023, Skipped 0, Total 1023; `[Fact]` 3→4, 4→5, 7→8; `grep -c 'Skip'` 0 each.
- **T2-18** · met · reviewer-reproduced: no DungeonMaster.App.exe running (tasklist); `dotnet test <sln> --no-build`, saved in full first: App.UnitTests 217/217, App.Tests 146/146, each Failed 0.

Every row lives in `reviews/T1.md` and `reviews/T2.md`.

**Coverage, as mapped at Gate 2 and checked at Gate 3:** 11 of 13 definition-of-done rows met through their tasks (items 1-11). Exceptions:

- **Item 12** · uncovered at Gate 2, orchestrator's row with evidence named · met: `git diff --name-only 452b18f 87d0090` lists 7 paths, all under `src/DungeonMaster.Tests/`; piped to `grep -vc '^src/DungeonMaster.Tests/'` it prints 0, and the same filter over a control list holding a `.sir/backlog.md` line prints 1.
- **Item 13** · uncovered at Gate 2, orchestrator's row with evidence named · applied after approval: removing the 20261123-0703-test-strength-rows row "Make the 8 element-selection filters ..." is this gate's own backlog edit. That row's eighth site, `ThemeSource.cs:639`, was wrong: its callers pass only `x:` names, so it is not a MAUI-name filter, and the session left it unchanged (item 9, T1-21).

Contract operations: `ThemeSource.IsMauiElement` and `ThemeSource.MauiNamespace` are both `called (test seam)` from T2's files (`SemanticDescriptionTests.cs:431`, `ConfirmButtonStateParityTests.cs:326` and `:178`, `IconButtonStyleParityTests.cs:497` and `:298`). Every caller is in the `DungeonMaster.Tests` project, as this test-only session intends. Both Gate 2 predictions of `called` are confirmed.

**Limits:** none — every verification command ran in the worktree.

**Human checks:** this session asked for none. These checks are still open in `.sir/backlog.md`, each to be run from the main checkout after the merge, following the numbered steps in its own row:

- **2026-09-16-floor-sidebar-keyboard-route** · "Measure the floor-name column in DIU": build the app, read the column width, record it in DIU.
- **2026-09-18-drawsessionresult-none-rejection** · "Profile the 1-2 second UI stalls on closing a polygon, saving a floor rename and closing the dialog": in a Release build.
- **2026-09-16-floor-sidebar-keyboard-route** · "Run the drag-handle Appium ratchet": build the app, start Appium on 127.0.0.1:4723, build and run the UiTests harness.
- **20261103-2239-app-follow-ups** · "Make the solution build and F5 launch work in Visual Studio": open `src/DungeonMaster.sln` in Visual Studio and build, then F5.
- **20261112-0153-floor-name-tooltip** · "Measure the floor expand chevron's narrowed width with the Appium suite": run `FloorExpandToggleMeasuresAtLeast23HighAndAtMost40Wide` and read the logged `FloorExpandToggle[0] size = WxH` line.

## 3. What ran

- **T1** · sir-test-engineer, then sir-reviewer · test · opus / opus · 1 review · 0 rounds · 0 redispatches · a58dbd0
- **T2** · sir-test-engineer, then sir-reviewer · test · opus / opus · 1 review · 0 rounds · 0 redispatches · 87d0090

Also dispatched: `init-session` (sir-git, haiku), `decomposition` (sir-architect, opus), and two `commit-task` operations (sir-git, haiku). No amendments.

**Change surface:** `452b18f..87d0090` (`git merge-base main test/maui-namespace-filters` to the branch tip), 2 commits, 7 files changed, 477 insertions, 26 deletions. The full diff is `git diff 452b18f..87d0090`. Under PR mode GitHub merges the pull request after this gate, so there is no local merged tree to compare with the branch tip.

## 4. Advisory findings

- **Contract gaps: 1**
  - `contracts/C1-maui-element.md:41`: the Errors bullet says `""` throws `System.Xml.XmlException` like `"x:Style"`. It actually throws `System.ArgumentException` ("The value cannot be an empty string"), as the implementer and the reviewer measured and as this gate re-measured under PowerShell 7. No criterion tests `""`, and T1's doc comment documents `ArgumentException` correctly. (T1)
- **Test and CI concerns: 2**, in `reviews/T1.md` and `reviews/T2.md`.
- **Cosmetics: 1**, in `reviews/T2.md`.

## 5. Metrics

- **userInteractions** · 3 (Gate 1, Gate 2, Gate 3; the closeout question is inside Gate 3)
- **escalationsRaised** · 0
- **reviewRounds** · 0
