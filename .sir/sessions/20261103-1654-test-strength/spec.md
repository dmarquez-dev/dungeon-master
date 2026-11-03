# Test strength

Make eight tests that cannot fail on the defect they guard able to fail on it. Only test code changes.

## Context

The backlog rows at `.sir/backlog.md:102-105`, `:109`, `:110`, `:112` and `:113` each name a test that stays green when the code it guards breaks. Gate 1 selected them as S1-S8 (`discussion.md`, the selected rows table). The spec phase re-read every one against the code at base `e22840e` (`chore/repo-audit`). **All eight are live**, and none needs a production change:

| # | Measured at base | Why it cannot fail today |
|---|---|---|
| S1 | `AddPlacedObjectCommandValidatorTests.cs:147-168` asserts only that `KindIsInEnumOnlyValidator` (`:170-179`) accepts `None`. No `999` assertion exists in the file. | If `.IsInEnum()` is removed from the throwaway validator, no rule runs and the test still passes. |
| S2 | `ObjectKindGuardTests.cs:21-25` pins `DoorWood == 1` and `DoorIron == 2` only. `ObjectKind.cs:60` declares the last member, `WeaponRack`, which is ordinal 37 (38 members, `None` = 0, no explicit values). | Reordering any member after `DoorIron` leaves the ordinal test and the `HaveCount(38)` test (`:32`) green. |
| S3 | `MutationOutcomeExtensionsTests.cs:128-141` (`ToResult_WithNone_Throws`) asserts `ParamName == "outcome"` and `ActualValue == None`. The `_` arm (`MutationOutcomeExtensions.cs:82-86`) throws with the same ParamName and ActualValue. | Deleting the `None` arm (`:77-81`) routes `None` to `_`, which satisfies both assertions. The two arms differ only in their message: `"No mutation outcome was reported."` against `"Unrecognised mutation outcome."`. |
| S4 | `DispatcherCancellationTests.cs:113-210`: the three behaviour-present facts capture only the behaviour's token. The three handler facts (`:17-108`) register no behaviour. | A defect that drops the handler's token only when a behaviour is registered is seen by none of the six facts. |
| S5 | `DungeonAggregateTests.cs:538-560` passes `ObjectScale.Default` and asserts `ObjectScale.Default`. `DungeonMethods.cs:354-358` forwards `scale` to `floor.AddObject`. | If the aggregate drops the argument and uses the default, the test still passes. |
| S6 | `PlacedObjectTests.cs:95` asserts `ex.Message.Should().Contain("5")`. The guard message is `$"Scale must be between {ObjectScale.Min} and {ObjectScale.Max}."` (`PlacedObject.cs:112`), and `Min` is `0.25f`. | The message contains `5` whatever value was rejected. `ActualValue` is already pinned at `:94`. |
| S7 | `CollectionWrapperCachingTests.cs:87-100` (`Objects_ReadTwiceOnTheSameFloor_...`) places no object and asserts only `BeSameAs`. The `HaveCount(1)` assertions at `:67`, `:84` and `:115` belong to the Floors, Areas and Vertices tests, not Objects. | A property returning one shared static empty collection passes it. |
| S8 | No test in `CollectionWrapperCachingTests.cs` reads a wrapper, mutates the backing list, and reads again. Every existing test mutates before its first read. | A wrapper built lazily as a snapshot copy on first read passes every test in the file. |

## Definition of done

- [ ] **D1 (S1).** A test asserts that `KindIsInEnumOnlyValidator` rejects `(ObjectKind)999`. With `.IsInEnum()` removed from the throwaway validator (test-code injection), that test fails.
- [ ] **D2 (S2).** The ordinal test asserts `(int)ObjectKind.WeaponRack == 37`. With `WeaponRack` moved to a different position in `ObjectKind.cs` (injection), the ordinal test fails and `ExactlyOneMemberCastsToZero_AndItIsNone` still passes.
- [ ] **D3 (S3).** A test asserts the `None` arm's own message. With the `None` arm deleted from `MutationOutcomeExtensions.ToResult` (injection), that test fails and `ToResult_WithUnrecognisedOutcome_Throws` still passes.
- [ ] **D4 (S4).** For each of the three request paths, a fact registers a behaviour and asserts that the handler received the dispatched token. Under an injection that drops the handler's token only when a behaviour is registered, those facts fail and the three handler-only facts still pass.
- [ ] **D5 (S5).** `AddPlacedObject_ReturnsObjectWithGivenKindPositionRotationAndScale` passes and asserts a non-default scale. With `DungeonMethods.AddPlacedObject` forwarding `ObjectScale.Default` instead of `scale` (injection), it fails.
- [ ] **D6 (S6).** `Constructor_InvalidScale_ReportsRejectedValue` no longer contains `Contain("5")` and makes an assertion about the message that a wrong message fails. The injection is chosen so that the old assertion would still pass under it and the new one fails.
- [ ] **D7 (S7).** The `Floor.Objects` caching test places one object and asserts `HaveCount(1)`. With `Floor.Objects` returning one shared static empty collection (injection), it fails.
- [ ] **D8 (S8).** At least one test reads a cached wrapper, mutates the backing list through the aggregate, and asserts that the same wrapper instance reflects the change. With that wrapper built as a snapshot copy on first read (injection), it fails.
- [ ] **D9.** Every injection is reverted before commit. Each restored file is shown byte-identical to its pre-injection copy with `cmp`. No production file is in any task's commit.
- [ ] **D10.** At the final task's state, `dotnet build src/DungeonMaster.sln` prints `Build succeeded.`, `0 Warning(s)` and `0 Error(s)`. `dotnet test src/DungeonMaster.sln` reports 0 failed in all three test assemblies.

## Scope

S1-S8 as listed in Context. Each one is closed by a changed or added test in `src/DungeonMaster.Tests/`, plus a demonstrated injection.

### Not in scope

- Any production code change. An injection is temporary and is never committed.
- The second clause of backlog row `:102` ("stop prescribing in criteria test constructions whose green state is reachable without the rule under test"). It is a process rule for criteria authors and has no test to change.
- The neighbouring test-strength rows (backlog lines 87, 88, 89, 91, 92, 101, 107, 108), the TEST style sweep, and the Gate 3 follow-ups from the two previous sessions (`discussion.md`, Decisions).

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Can tasks run in parallel? | No. They run as a strict `dependsOn` chain, T1 to T7, because they share one worktree and one `obj/`/`bin/` tree, and six of the seven tasks temporarily inject a defect into a production file. | repo | `manifest.json` `dependsOn`; every injection names a file under `src/DungeonMaster.Domain`, `Application` or `App.Core` |
| S2: is `WeaponRack` ordinal 37? | Yes. There are 38 members, the first is `None` and the last is `WeaponRack`, and none has an explicit value. | repo | `ObjectKind.cs:13`, `:60`; `ObjectKindGuardTests.cs:32` asserts `HaveCount(38)` and passes at base |
| S2: does moving `WeaponRack` break anything outside `ObjectKindGuardTests`? | No, nothing that depends on stored ordinals. Persistence stores `Kind` as a string, so the injection run is filtered to the one class. | repo | `PlacedObjectConfiguration.cs:34-37` (`HasConversion<string>()`) |
| S2: does the strengthened test catch every reorder? | No. A swap of two middle members still passes, so T2 must rename `DeclarationOrderOfTheExistingMembersSurvived` to name only the positions it pins, or justify keeping the name. | judgment | `ObjectKindGuardTests.cs:21-25` |
| S3: how is the `None` arm told apart from `_`? | By its message, asserted with `StartWith`/`Contain`, because the framework appends the parameter and actual value to `Message`. | repo | `MutationOutcomeExtensions.cs:77-86` |
| S4: what injection drops only the behaviour-path handler token? | A ternary on `GetServices<IPipelineBehavior<…>>().Any()` at the handler's token argument, one invoker per injection (`Dispatcher.cs:139`, `:172`, `:205`). The `BuildPipeline` tokens at `:140`, `:173` and `:206` are left alone. | repo | `Dispatcher.cs:84-109`, `:133-140` |
| S6: which property does the message assertion pin? | The guard's own range sentence, using `StartWith`, with the expected text built from `ObjectScale.Min`/`Max` through the same interpolation. The backlog row's suggestion, `Contain("Actual value was 5")`, was rejected: the framework derives that suffix from `ActualValue`, which `:94` already pins, so no injection that preserves `ActualValue` can make it fail. | judgment | `PlacedObject.cs:109-112`; `PlacedObjectTests.cs:93-94` |
| S6: does the test depend on culture? | No. Both sides interpolate the same bounds in the same current culture, and the test project pins no culture. | repo | `DungeonMaster.Tests.csproj`, `Directory.Build.props` (no `Culture`/`InvariantGlobalization` setting) |
| S8: which wrappers get a reread test? | All five, each with its own snapshot-on-first-read injection. | judgment | `Dungeon.cs:17`, `Floor.cs:22-23`, `Area.cs:76`, `MapDrawingSession.cs:49` |
| S8: does the EF-materialised Outline test survive the snapshot injections? | Yes. EF writes each backing list through its field, so the first read comes after materialisation. Production code reads the fields, not the wrapper properties, so no internal read takes the snapshot early. | repo | `DungeonConfiguration.cs:75-78`, `FloorConfiguration.cs:124-127`, `:136-139`, `AreaConfiguration.cs:130-131`; grep of the wrapper property names over the Domain entity files and `MapDrawingSession.cs` |
| D9: who proves that no production file is in a commit? | The orchestrator does, from each task commit's file list at commit time, because tasks cannot run git. Each task's report also lists the files it modified, which must be exactly its one owned test file. | judgment | `sir-core`, Hard constraints; manifest criteria T1[10] … T7[20] |
| Worktree root | `C:/dm-wt/20261103-1654-test-strength`, not `.claude-worktrees/`. This follows the project lesson that a long path breaks the WinUI XAML compiler on MAX_PATH. | repo | `.sir/lessons.md` (worktree-root); the three previous sessions' `state.json` `worktree` |

## Contracts

None. Every task edits a test file that no other task touches, and no task defines a type another task consumes.

## Open questions

None.
