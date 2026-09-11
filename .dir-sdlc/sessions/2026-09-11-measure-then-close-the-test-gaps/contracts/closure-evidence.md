# Contract — the evidence each code task hands to the record-correction task

Frozen at Gate 2. Produced by T1, T2, T3, T4. **Consumed by T5**, which is the only task that
writes `docs/audit/`. T5 runs after all four and cites what they hand over; it invents no
evidence of its own about code it did not write.

Every anchor below is a `path:line` or a command. The rule the repository has burned three
sessions learning applies to every one of them: **a number is stated with the command that
produced it, or with the artifact it was counted from.** No count is anchored at `HEAD` —
this session's fixed measurement commit is `1407090`.

## Operation — `T1.evidence(MASTER-070)`

T1 hands T5:

1. The fully-qualified name of the new seam declared on
   `src/DungeonMaster.Tests/Application/Common/Handlers/CommandHandlerTestBase.cs`, and its
   signature.
2. The fully-qualified name of every new `[Fact]`, with the file and line it is declared at,
   grouped by which of the six async port methods it proves:
   `IUnitOfWorkFactory.CreateAsync`, `IDungeonWriteRepository.LoadAsync`,
   `IDungeonWriteRepository.RemoveAsync`, `IUnitOfWork.CommitAsync`,
   `IDungeonReadRepository.GetSummariesAsync`, `IDungeonReadRepository.GetDetailAsync`.
3. For the Infrastructure tests: the method each cancels, and the exception type asserted.
4. The injected-defect record for at least one forwarding test — the edit made, the command
   run, the failure observed, and the named control test that stayed green under the same run.
5. The exact `dotnet test ... --filter ...` commands it ran and the pass/fail totals each
   printed.

T5 uses this to replace `TEST-019`'s falsified "no test in the suite passes a cancellable or
cancelled CancellationToken" claim and its two dead per-file anchors
(`AddAreaCommandHandlerTests.cs:37`, `UpdateFloorCommandHandlerTests.cs:37`).

## Operation — `T2.evidence(MASTER-086)`

T2 hands T5:

1. The fully-qualified name, file and line of every new `[Fact]`/`[Theory]`, grouped by the
   clause of `TEST-020` it closes: the aggregate name guards, the `ReorderFloor` negative
   case, the `Dungeon.AddPlacedObject` 4-argument overload, the `Floor.UpdateObject`
   3-argument overload.
2. The injected-defect record for at least one of them, with its control test.
3. The `dotnet test ... --filter ...` command and the totals it printed.

T2 hands over **no** production change. Per escalation **E2**, P1, P2 and P3 are filed, not
fixed (see `new-entries.md`).

## Operation — `T3.evidence(MASTER-167)`

T3 hands T5:

1. The final text of the guard and the line it sits at in
   `src/DungeonMaster.App/Converters/GreaterThanZeroConverter.cs`.
2. The name and line of the new red-first test, the command run **before** the guard existed,
   and the failure message it printed — plus the named control test that passed in that same
   red run.
3. Confirmation that `ConvertReturnsFalseForTheUnresolvedBindingSentinel` passed both before
   and after, which is why it is **not** evidence for this entry.
4. The `dotnet test ... --filter ...` command and the totals it printed.

## Operation — `T4.evidence(MASTER-155)`

T4 hands T5:

1. The four removal sites, each as `path:line` **as they stood at `1407090`**:
   `src/DungeonMaster.Tests/DungeonMaster.Tests.csproj:9`,
   `src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj:26`,
   `src/DungeonMaster.App.UnitTests/DungeonMaster.App.UnitTests.csproj:20`,
   `Directory.Packages.props:20`.
2. The list of `packages.lock.json` files whose content changed, and the command that
   regenerated them.
3. The restore/build command and its result.

T4 changes **no** `.runsettings`, **no** CI file, **no** `xunit.runner.json`, and adds **no**
`[assembly: CollectionBehavior]`. Per escalation **E1** those are out of this session; per the
re-measurement the last of them is a measured regression and T5 records it as such.

## Operation — `T5.close(entry)` — what T5 writes, for each of the five

For each entry, T5 writes **both**:

- an amendment inside the source record in `docs/audit/05-tests.md`, in the established
  in-place format (`**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION n, <what>. Classification:
  DRIFT | MEASUREMENT ERROR | UNDETERMINED.**` … `**]**`), quoting the previous wording
  verbatim before correcting it — the format `e55eb85` established and which
  `docs/audit/05-tests.md:88-116` describes; and
- a `Status` cell in `docs/audit/00-master-index.md`, in the established form
  `**done** (6b-iii) -- <task>, <evidence>`.

`MASTER-167` has no source record in `05-tests.md`; its correction is written into its
narrative entry under `## Suggested fix sessions` in `docs/audit/00-master-index.md`
(currently line 559) instead.

`docs/audit/04-app-controls-and-theme.md` is **not** edited. `UIKIT-004` is not falsified —
the re-measurement confirms the entry's own instruction. The only defect there is that
`00-master-index.md:559` cites the range `:225-226` when the sentence is at `:224-225`
(`:226` is blank), and that citation lives in `00-master-index.md`, which T5 owns.
