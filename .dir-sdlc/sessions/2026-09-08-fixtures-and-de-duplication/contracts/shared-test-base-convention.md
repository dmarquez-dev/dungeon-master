# Contract — one shape for a shared abstract test base, and the frozen `UnitOfWorkSubstitute` surface

Producer: none singly. Consumers: **T2, T3, T4**.

`MASTER-085` and `MASTER-064` would each introduce an abstract generic test base into
`DungeonMaster.Tests`, in different directories, at the same time. That is **one convention decision,
not two**. Without this contract the session's diff carries two shapes for the same idea and a later
reader cannot tell which is the house style.

`MASTER-064` also has a cross-directory consumer that neither entry declares:
`Application/Features/Dungeons/Validators/CommandValidatorCoverageTests.cs` consumes
`Application/Common/Handlers/UnitOfWorkSubstitute.cs`, which **T2** owns. §5 freezes that surface.

## 1. When a base is extracted at all

**Three or more classes** must share the shape. At two, the duplication stays and the report says so.

This threshold is what settles `MASTER-085`'s second equivalence class. Measured
(`grep -rln "UnitOfWorkSubstitute" --include=*.cs src/DungeonMaster.Tests | grep -v obj`, and the
handler-test file listing): the command classes are **14** and the query classes are **2**. The 14
already share `UnitOfWorkSubstitute`; the 2 share three lines. **T3 does not give the two query
classes a base**, and T3's report states the rule and the count.

`MASTER-064`'s four validator classes and `MASTER-085`'s twelve not-found tests are both above the
threshold.

## 2. Shape

- The base is `public abstract`. It is never `sealed`, never `static`, and never carries `[Collection]`.
  xUnit does not discover an abstract class, so the base's `[Fact]`/`[Theory]` methods run once per
  concrete leaf and never on their own.
- Every concrete leaf is `public sealed` and its file keeps its existing name. One leaf per file.
- **Variation is supplied by an `abstract` or `protected abstract` member the leaf overrides — never
  by a constructor parameter, never by a `[MemberData]` table indexed by type name, never by
  reflection over the leaf.** A leaf that has to be found by a scan is not a leaf.
- The base declares no field a leaf must remember to initialise. If the base needs state, the base's
  own constructor builds it.
- A leaf may add facts of its own. A leaf that overrides an inherited fact to change what it asserts
  is a signal the base is wrong; report it rather than doing it.

## 3. Naming

- Base: `<WhatItEstablishes>TestBase` — for example `NameOnlyValidatorTestBase`,
  `DungeonNotFoundHandlerTestBase`. Not `<Thing>Tests`, which reads as a leaf.
- The base's inherited facts keep names that read correctly on every leaf. A name that only makes
  sense for the class it was extracted from is a name that will mislead eleven other reports.
- Abstract members are named for the value they supply, not for the test that consumes them —
  `CreateCommandWithName(string name)`, not `ArrangeForTheEmptyNameCase()`.

## 4. What the base may not hide

The whole risk of this session's two extractions is that collapsing N tests into one makes a dropped
case invisible. So:

- **Every fact the base owns must be reachable from every leaf that inherits it, with no opt-out.**
  There is no `protected virtual bool SkipX => false`.
- **The leaf count is asserted, not assumed.** Where a base replaces N duplicated tests, the task's
  report states N with the command that produced it, and states the post-task passing count with the
  command that produced it, and reconciles the two. This is the same failure mode
  `contracts/anti-vacuity.md` §5 names: a set that is empty because everything is fine and a set that
  is empty because nothing was examined must not look the same from outside.
- No assertion present in a pre-task test may be absent afterwards. Where a leaf's version of the
  shared test asserted something extra, that assertion survives — in the leaf, or as an additional
  abstract hook — and the report names every such case.

## 5. `UnitOfWorkSubstitute` — surface frozen for this session

`src/DungeonMaster.Tests/Application/Common/Handlers/UnitOfWorkSubstitute.cs` is owned by **T2**.
Measured: `grep -rln "UnitOfWorkSubstitute" --include=*.cs src/DungeonMaster.Tests | grep -v obj`
→ **18 files**, i.e. **17 consumers** plus the declaration. Two consumers lie outside T2's ownership:

| Consumer | Owned by |
|---|---|
| `Application/Features/Dungeons/Validators/CommandValidatorCoverageTests.cs` | T4 |
| `Application/Common/Dispatch/ApplicationServiceRegistrationTests.cs` | **no task** |

Its three properties are frozen exactly as they stand:

```csharp
public IUnitOfWorkFactory Factory { get; }
public IUnitOfWork UnitOfWork { get; }
public IDungeonWriteRepository Dungeons { get; }
```

T2 may add a member. T2 may not rename, remove, or change the type or accessibility of any of the
three, and may not change the class's `internal sealed` accessibility. `ApplicationServiceRegistrationTests.cs`
is owned by nobody and must still compile and pass; a change that breaks it is a blocking finding
even though no task owns the file.

`UnitOfWork` is the property the commit-boundary assertion is asserted through. It already exists —
this session adds call sites, not wiring.

## 6. The commit boundary — where the new assertion goes

`DungeonMutationExtensions.cs:51-58` returns before **both** `unitOfWork.Dungeons.Update(dungeon)`
and `await unitOfWork.CommitAsync(ct)` on the failure path, so
`_unitOfWork.UnitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>())` observes strictly
more than the repository-boundary `Dungeons.DidNotReceive().Update(Arg.Any<Dungeon>())`.

**That is reasoning from read evidence, not measurement.** The spikes did not test it. T2 must
demonstrate it before relying on it, and T2's criteria require the demonstration.

Measured at `850a10b`, in `Application/Features/Dungeons/Commands/`:

- `grep -n "DidNotReceive" *.cs | wc -l` → **13** sites across **10** files.
- Of those, **12 assert `.Update(Arg.Any<Dungeon>())`** and **1 asserts
  `.CommitAsync(Arg.Any<CancellationToken>())`** (`DeleteDungeonCommandHandlerTests.cs:89-90`).
- `grep -c "CommitAsync" *.cs | grep -v ':0'` → the member appears in only **2** of the 14 command
  test files. The commit boundary is very nearly unobserved across this suite.

The new assertion goes **alongside** the existing `.Update(...)` assertion at each of the 12 sites,
and into the extracted not-found base, and **never in place of it**. The spike proved that deleting
the `.Update(...)` assertions removes the only thing in the suite that catches save-on-failure
(injection: 14 failures, all on the mock assertion; control: 6 different failures). Replacing them is
the regression `MASTER-066` was closed for not committing.
