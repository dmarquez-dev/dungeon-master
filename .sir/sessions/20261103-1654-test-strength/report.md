sir-sdlc 1.0.14 — Gate 3 report — 20261103-1654-test-strength

**Decisions needed:** none. Nothing was escalated and no review found a blocking issue.

## 2. Acceptance criteria

| Task | Domain | Criteria | Met |
|---|---|---|---|
| T1 | test | 10 | 10 |
| T2 | test | 8 | 8 |
| T3 | test | 9 | 9 |
| T4 | test | 14 | 14 |
| T5 | test | 8 | 8 |
| T6 | test | 9 | 9 |
| T7 | test | 20 | 20 |

All 78 criteria are met, and none is `not met`. 53 of them carry a demonstration tag and are listed below:

- T1[3] injected defect: diff is one add-only hunk at base :170; None test green in injected (9 passed) and restored (10 passed) logs
- T1[4] injected defect: one injection run; pre-copy and both logs present; reviewer re-ran `cmp` owned file vs pre-copy, exit 0
- T1[5] injected defect: only code occurrence of `.IsInEnum()`; injected log `Failed: 1, Passed: 9, Total: 10`, only the new test failing
- T1[7] reviewer-reproduced: `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`
- T1[8] reviewer-reproduced: `Passed!  - Failed: 0, Passed: 935, Total: 935`
- T1[9] reviewer-reproduced: `CR=216 LF=216 PAIRS=216`; control `CR=2 LF=2 PAIRS=1`; added lines tab-indented
- T2[3] injected defect: pre-copy and both logs present; reviewer `cmp` pre-copy vs `ObjectKind.cs`, exit 0
- T2[4] injected defect: only the renamed test fails (`…to be 37, but found 3`), `Failed: 1, Passed: 6, Total: 7`; all six controls named Passed
- T2[7] reviewer-reproduced: target `CR=100 LF=100 PAIRS=100`, control `CR=3 LF=3 PAIRS=2`; added line starts `\t\t`
- T3[1] reviewer-reproduced: `:144-145` adds `StartWith("No mutation outcome was reported.")`; the message has exactly one hit in production, `MutationOutcomeExtensions.cs:81`
- T3[2] injected defect: `ToResult_WithUnrecognisedOutcome_Throws` untouched and passed under injection
- T3[3] injected defect: one injection run; reviewer `cmp` pre-copy vs production file, exit 0
- T3[4] injected defect: injected log `Failed: 1, Passed: 7, Total: 8`, only `ToResult_WithNone_Throws`
- T3[6] reviewer-reproduced: `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`
- T3[7] reviewer-reproduced: `Passed!  - Failed: 0, Passed: 935, Total: 935`
- T3[8] reviewer-reproduced: `CR=147 LF=147 PAIRS=147`; control `CR=3 LF=3 PAIRS=2`
- T4[6] injected defect: each injection logged, `cmp`ed and restored (`Failed: 0, Passed: 9`); reviewer `cmp` of `Dispatcher.cs` against all three pre-copies, exit 0
- T4[7] injected defect: `139c139`; only the SendAsync(ICommand) fact fails, 1/8/9
- T4[8] injected defect: `172c172`; only `SendAsyncOfT_…` fails, 1/8/9
- T4[9] injected defect: `205c205`; only `QueryAsync_…` fails, 1/8/9; `Dispatcher.cs` unchanged vs HEAD
- T4[11] reviewer-reproduced: `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`
- T4[12] reviewer-reproduced: `Passed!  - Failed: 0, Passed: 938, Total: 938`
- T4[13] reviewer-reproduced: `CR=478 LF=478 PAIRS=478`; control `CR=3 LF=3 PAIRS=2`; 0 space-indented, 0 trailing whitespace in 128 added lines
- T5[2] injected defect: procedure ran once; first attempt hit compile error CS1061, the reshape was disclosed and stayed in the same defect class; reviewer `cmp` exit 0
- T5[3] injected defect: reshaped `…ObjectScale.Default);` at `DungeonMethods.cs:358`; only the target test red (Failed 1 / Passed 40 / Total 41)
- T5[5] reviewer-reproduced: `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`
- T5[6] reviewer-reproduced: `Passed!  - Failed: 0, Passed: 938, Total: 938`
- T5[7] reviewer-reproduced: `CR=580 LF=580 PAIRS=580`; control `CR=3 LF=3 PAIRS=2`
- T6[2] reviewer-reproduced: :95 `StartWith($"Scale must be between {ObjectScale.Min} and {ObjectScale.Max}.")`; no culture setting found, reviewer control greps returned 2 and 6
- T6[3] injected defect: injected `Failed: 1, Passed: 14, Total: 15`, restored `0/15/15`; reviewer `cmp` exit 0
- T6[4] injected defect: the swap was made at the one message site, :112; only the target test failed, and all 11 scale-rejection siblings passed
- T6[5] injected defect: the injected actual message still contains `5`, both in `0.25` and in `Actual value was 5.`, so the old assertion would have passed
- T6[6] reviewer-reproduced: `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`
- T6[7] reviewer-reproduced: sln test 938/938, 187/187, 139/139
- T6[8] reviewer-reproduced: `CR=142 LF=142 PAIRS=142`; control `CR=3 LF=3 PAIRS=2`
- T7[1] injected defect: S7 test places an object at :96-101 before both reads; `HaveCount(1)` at :109
- T7[2] injected defect: `Floors_ReadAgainAfterAddFloor_…` (:196-211), same instance, countBefore+1
- T7[3] injected defect: `Areas_ReadAgainAfterAddArea_…` (:213-229), same instance, countBefore+1
- T7[4] injected defect: `Objects_ReadAgainAfterAddPlacedObject_…` (:231-253), same instance, countBefore+1
- T7[5] injected defect: `Outline_ReadAgainAfterUpdateAreaOutlineOnAnInMemoryArea_…` (:255-275), `HaveCount(3)`
- T7[6] injected defect: `Vertices_ReadAgainAfterPlaceVertex_…` (:277-292), same instance, countBefore+1
- T7[7] reviewer-reproduced: 12 tests; each of the 11 non-control tests holds a `BeSameAs`
- T7[8] injected defect: all six injections logged with named failures; restored `Failed: 0, Passed: 12`
- T7[9] injected defect: `inject-s7-shared-empty.log` `Failed: 2, Passed: 10`, exactly the two Objects tests
- T7[10] injected defect: every `s8-*` normal log 11 passed / 1 failed; S7, EF Outline and the positive control passed in all five
- T7[11] injected defect: `inject-s8-floors.log` 1 failed, `Floors_ReadAgainAfterAddFloor_…`
- T7[12] injected defect: `inject-s8-areas.log` 1 failed, `Areas_ReadAgainAfterAddArea_…`
- T7[13] injected defect: `inject-s8-objects.log` 1 failed, `Objects_ReadAgainAfterAddPlacedObject_…`; S7 passed
- T7[14] injected defect: `inject-s8-outline.log` 1 failed, `Outline_ReadAgainAfterUpdateAreaOutlineOnAnInMemoryArea_…`
- T7[15] injected defect, reviewer-reproduced: `inject-s8-vertices.log` 1 failed; reviewer `cmp` of all six pre-copies, exit 0
- T7[17] reviewer-reproduced: `dotnet build …/DungeonMaster.sln`, exit 0, `0 Warning(s)`, `0 Error(s)`
- T7[18] reviewer-reproduced: `dotnet test … --no-build`, exit 0: 187/187, 943/943, 139/139, 0 failed
- T7[19] reviewer-reproduced: `CR=293 LF=293 PAIRS=293`; control `CR=3 LF=3 PAIRS=2`; diff `^+ ` 0, `^+<TAB>` 90

Every row is in `reviews/T1.md` … `reviews/T7.md`.

**Coverage** is as mapped at Gate 2 and checked at Gate 3. All 11 definition-of-done rows are covered and met, and none is an exception. That includes D9's commit-content clause: at Gate 3, `git show --name-only` on each of the seven task commits lists exactly one path, that task's owned test file. No contracts were declared, so there are no operation rows.

**Limits:** none — every verification command ran in the worktree.

## 3. What ran

| Task | Agent | Domain | Model | Reviews | Rounds | Redispatches | Commit |
|---|---|---|---|---|---|---|---|
| T1 | sir-test-engineer | test | opus | 1 | 0 | 0 | 12ad838 |
| T2 | sir-test-engineer | test | opus | 1 | 0 | 0 | 0821885 |
| T3 | sir-test-engineer | test | opus | 1 | 0 | 0 | 08655b7 |
| T4 | sir-test-engineer | test | opus | 1 | 0 | 0 | b30b5ce |
| T5 | sir-test-engineer | test | opus | 1 | 0 | 0 | 1bce997 |
| T6 | sir-test-engineer | test | opus | 1 | 0 | 0 | 17a6c8f |
| T7 | sir-test-engineer | test | opus | 1 | 0 | 0 | 4cd3109 |

Each task review was a `sir-reviewer` on opus. Each commit was a `sir-git` on haiku. The other dispatches were `init-session` (`sir-git`, haiku) and the decomposition (`sir-architect`, opus).

**Change surface:** `e22840e..4cd3109` (merge-base of `chore/repo-audit` and `agent/20261103-1654-test-strength`, to the branch tip). The range holds 7 commits that change 7 files, with 274 insertions and 9 deletions. All 7 files are test files under `src/DungeonMaster.Tests/`. Run `git diff e22840e..4cd3109` for the full diff.

## 4. Advisory findings

**Contract gaps (1)**
- T5 criterion 3 froze injection text that cannot compile at `DungeonMethods.cs:358`. Inside `Dungeon`, `Floors` resolves to the `IReadOnlyList<Floor>` property (CS1061). The task's reshape clause covered it. When a criterion prescribes production-side injection text, it should fully qualify names or compile the text first. (reviews/T5.md A1)

**Correctness risks (2)**
- `ObjectKindGuardTests.cs:21-26`: the ordinal test pins 3 of the 38 `ObjectKind` members, so swapping two middle members still passes. (reviews/T2.md A1)
- `PlacedObjectTests.cs:95`: the message assertion covers only the above-Max arm. The NaN, infinity and below-Min arms share the message at `PlacedObject.cs:112` but have no message assertion. (reviews/T6.md A1)

**Test and CI concerns:** 3 — see reviews/T1.md A1, reviews/T5.md A2, reviews/T6.md A2.

**Cosmetics:** 3 — see reviews/T2.md A2, reviews/T4.md A1, reviews/T7.md Notes (the commit 4cd3109 body quotes the wrong phrase).

## 5. Metrics

- userInteractions: 3 (Gates 1, 2 and 3; the push question is not counted yet)
- escalationsRaised: 0
- reviewRounds: 0 (the per-task Rounds column also sums to 0)
