# Test strength

Strengthen the tests the backlog records as unable to fail when the code they guard breaks:
assertions that pass on the wrong value, guards that pin only part of what they claim, and
production branches whose deletion leaves the suite green. The tests change; production code
does not.

## Context

The repo-audit arc on `chore/repo-audit` schedules its work from `.sir/backlog.md`. At Gate 1
that file held 167 entries, by `grep -c '^20' .sir/backlog.md`. A second predicate,
`grep -c ' -- source: ' .sir/backlog.md`, also returned 167. The two previous sessions,
`20261102-1907-domain-invariants` and `20261102-2225-app-correctness`, closed the production
correctness rows in Domain, Application and App. Gate 1 offered four units: test strength,
the Gate 3 follow-ups from those two sessions, the TEST style sweep (MASTER-126/128/137/139/143/144),
and CORE doc-comment corrections. The user chose **test strength**.

The selected rows, by their line in `.sir/backlog.md` as read at Gate 1
(`grep -n` on each row's distinctive token):

| # | Backlog line | Source | Row |
|---|---|---|---|
| S1 | 102 | `.dir-sdlc/backlog.md:355` (xref MASTER-073) | Add a second assertion that the throwaway `KindIsInEnumOnlyValidator` in `AddPlacedObjectCommandValidatorTests` rejects `(ObjectKind)999`, so its green result on `None` proves `IsInEnum()` ran rather than that no rule ran. |
| S2 | 103 | `.dir-sdlc/backlog.md:359` | Extend the `ObjectKindGuardTests` ordinal test, which pins only `DoorWood == 1` and `DoorIron == 2`, to also assert the last member's ordinal. |
| S3 | 104 | `.dir-sdlc/backlog.md:373` (xref MASTER-177, MASTER-179) | Add a test that reddens when the dedicated `MutationOutcome.None` switch arm in `MutationOutcomeExtensions.ToResult` is deleted (asserting the arm's distinct message, the route 6b-xi used at `DrawSessionResult`). |
| S4 | 105 | `.dir-sdlc/backlog.md:403` (xref MASTER-172) | Add a `DispatcherCancellationTests` fact that captures the handler's own token on the path where a pipeline behaviour is also registered. |
| S5 | 109 | `.dir-sdlc/backlog.md:429` (xref MASTER-047) | Make `AddPlacedObject_ReturnsObjectWithGivenKindPositionRotationAndScale` in `DungeonAggregateTests.cs` pass a non-default scale such as `2.0f`. |
| S6 | 110 | `.dir-sdlc/backlog.md:445` (xref MASTER-081) | Replace `ex.Message.Should().Contain("5")` in `PlacedObjectTests.cs`, which passes on the `5` inside `0.25`, with an assertion that tests the intended property. |
| S7 | 112 | `.dir-sdlc/backlog.md:463` | Strengthen the `Floor.Objects` assertion in `CollectionWrapperCachingTests.cs` by placing one object and asserting `HaveCount(1)`, so the test cannot pass against a shared static empty collection. |
| S8 | 113 | `.dir-sdlc/backlog.md:465` | Add a read-mutate-reread assertion to `CollectionWrapperCachingTests.cs` so the suite tells a live view over the backing list from a cached snapshot copy. |

These are the orchestrator's Gate 1 spot-checks, not measurements. Each one is a lead for the
spec phase to re-measure:

- `src/DungeonMaster.Tests/Domain/Entities/Floor/PlacedObjectTests.cs:95` still reads
  `ex.Message.Should().Contain("5");`, so S6 looks live.
- `src/DungeonMaster.Tests/Domain/Entities/Floor/ObjectKindGuardTests.cs:24` asserts
  `((int)ObjectKind.DoorIron).Should().Be(2);`. A grep for `== 2` and `DoorIron` found no
  last-member ordinal assertion, so S2 looks live. That was not a full read of the file.
- `KindIsInEnumOnlyValidator` is declared at
  `src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/AddPlacedObjectCommandValidatorTests.cs:170`
  and used at `:151`. Whether a `999` assertion exists was not checked.
- `src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs` passes
  `ObjectScale.Default` at `:370`, `:410` and `:418`. Which of these is the named test was not
  checked, so S5 looks live but is unconfirmed.
- **S7 may already be stale.** `src/DungeonMaster.Tests/Domain/CollectionWrapperCachingTests.cs`
  already has `HaveCount(1)` at `:67`, `:84` and `:115`. Whether one of these covers
  `Floor.Objects` was not checked. Re-measure before working on it, and close it with evidence
  if it is already done.
- S3, S4 and S8 were not spot-checked.

## Decisions

- Track: **Standard**. Eight separate rows, several test files, and each row's closing claim
  (this test now fails on the defect) is a separate claim someone can check.
- Session name: `test-strength`.
- Scope is S1-S8 above and nothing else. Out of scope: the neighbouring test-strength rows
  (backlog lines 87, 88, 89, 91, 92, 107, 108, 101), the TEST style sweep, and the Gate 3
  follow-ups.
- **No production code changes.** If a row turns out to need one (for example, a test that can
  only discriminate once a production message is made distinct), report it at Gate 2 as a
  premise problem. Do not widen scope quietly.
- **Re-measure every row before planning around it.** In the DIR arc, 7/10, 5/8, 8/8 and 6/6
  selected rows carried a false or stale specific. A row found already fixed is closed with
  its evidence and gets no work. A row whose premise is false is reported at Gate 2.
- **Every strengthened or new test must be shown to discriminate.** Inject the defect the row
  names (delete the `None` arm, drop the scale argument, return a shared empty collection, cache
  a snapshot, and so on). Show the test failing on the injection, and show a control that must
  pass. A test whose only demonstration is a changed expected value is the weaker
  demonstration, and gets labelled as one. The injection is reverted and is never committed.
- If a test needs a defect injected into production code, the injection happens in the worktree
  and is reverted before commit. Settle the per-revert build-and-test script at Gate 2 and put
  its saved logs into the criterion, per the `20261102-1907-domain-invariants` lesson.
