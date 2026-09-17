# Three missing CORE domain invariants — audit sub-session 6b-vii

**Goal.** Give the Domain three invariants it currently documents but does not enforce, closing `MASTER-168`,
`MASTER-169` and `MASTER-170`.

## Context

The audit has **53 open entries** and **no open S1 or S2** — 6b-vi closed the last one. `CORE` is the largest
open layer at **20**, and these three entries are one coherent cluster inside it: each is a constructor or
setter that accepts a value the domain says is illegal.

They were **filed rather than fixed** by escalation E2 of sub-session 6b-iii, and the reason is the whole
shape of this session. They came out of `MASTER-086`'s re-measurement as *production findings wearing test
clothing*: `TEST-020` clause (d) asked for a test that `UpdateAreaOutline` "rejects or accepts a degenerate
one- or two-segment outline", and **a test cannot assert behaviour that does not exist**. Deciding what the
behaviour should be is a design decision, and 6b-iii correctly refused to make it inside a coverage session.
This session makes those three decisions and then writes the code and the tests together.

Everything below derives from `measurement.md`, produced in this session at fixed commit `dff976d` with every
figure stated alongside the command that produced it. Where it and the audit entry prose disagree, the
measurement is the record — and it disagrees twice, both times in the direction of *less* alarm:

- **The guards cannot change any behaviour a user can drive today.** Tracing the producers, not just the
  validators, shows `MapDrawingSession.BuildOutline` emits 0 or ≥3 segments and never 1 or 2, and the only
  other outline producer translates an existing outline without changing its count. All three entries are
  latent invariant gaps in full.
- **No `CubicBezier` has ever been persisted.** The sole production construction site
  (`MapDrawingSession.cs:122`, `:125`) writes `Line` segments with null control points, which is what bounds
  the one decision in this session that touches the read path.

## Definition of done

1. `Area.SetOutline` rejects a **null** outline with `ArgumentNullException` (`ParamName` `outline`).
2. `Area.SetOutline` rejects an outline of **fewer than three segments** with an exactly-typed
   `ArgumentException` (`ParamName` `outline`). Empty is rejected by this rule, not a separate branch.
3. Both `SetOutline` rejections occur **before `_outline` is touched**, proven by a test asserting the
   previous outline's segment *values* survive a rejected call.
4. No `ClearOutline` affordance is added anywhere in the repository.
5. `Floor`'s internal constructor throws `ArgumentOutOfRangeException` (`ParamName` `number`, offending value
   carried as `ActualValue`) when `number < 1`, before any field assignment.
6. `Floor.UpdateNumber` throws the same, when `number < 1`, before assigning `Number` and before `Touch()`.
7. `Floor`'s private parameterless constructor is unchanged and ungated, so EF materialisation still works.
8. `Dungeon.ReorderFloor` still returns `MutationOutcome.OutOfRange` rather than throwing, proven by a test.
9. No `FloorNumber` value object is introduced.
10. `PathSegment` rejects a `CubicBezier` with either control point null, from its **primary constructor**.
11. `PathSegment` rejects a `Line` carrying either control point, from the same place.
12. Because the guard is in the constructor, it runs on **JSON deserialization**, proven by a round-trip test
    file containing both a positive control (a normal outline still loads) and a corrupt-row case.
13. `PathSegment`'s `Ctrl1` / `Ctrl2` doc comments no longer read "used only when `CubicBezier` is specified"
    — the wording the audit entry names as contradicted — and state the enforced requirement instead.
14. Every `PathSegment` construction site in the repository satisfies the rule, censused with a grep covering
    **both** the explicit `new PathSegment(` form and the target-typed `new(PathSegmentKind.` form.
15. `AreaOutlineGeometryTests.Translate_SegmentWithOnlyCtrl2Null_OffsetsCtrl1AndLeavesCtrl2Null` is **deleted**,
    not repaired: its premise is a half-specified `CubicBezier`, which this session makes unconstructible.
16. The two single-segment outlines in `AreaOutlineChangeTrackingTests` are raised to ≥3 segments while still
    differing from the seeded outline, so its change-tracking assertions still hold. Its three `[Fact]`
    methods are neither deleted nor renamed.
17. Each of the three guards is shown able to **fail**: the guard is removed or weakened, the named filtered
    test run is executed, the specific tests that must fail are observed failing **while a named positive
    control still passes**, and the guard is restored to green.
18. `dotnet build src/DungeonMaster.sln` reports 0 warnings and 0 errors across all three App TFMs.
19. All three test projects pass with 0 failed, and the new totals are reported against the measured
    baseline of 796 / 97 / 23 = 916 at `dff976d`, with any discrepancy reported rather than absorbed.

## Scope

`src/DungeonMaster.Domain/` is the **entire production change surface**: three files —
`Entities/Areas/AreaMethods.cs`, `Entities/Floors/Floor.cs` + `Entities/Floors/FloorMethods.cs`, and
`ValueObjects/PathSegment.cs`. Test changes land in `src/DungeonMaster.Tests/` only.

### Not in scope

- **Any change under `src/DungeonMaster.App`, `src/DungeonMaster.App.Core`, or `src/DungeonMaster.Application`.**
  The measurement traced every producer and consumer; none needs to change.
- **Closing the `with`-expression gap.** A record's copy constructor does not re-run property initializers, so
  `segment with { … }` bypasses the `PathSegment` guard. There is exactly one production `with` site,
  `AreaOutlineGeometry.Translate` (`src/DungeonMaster.App.Core/Map/AreaOutlineGeometry.cs:42`), and it
  preserves both `Kind` and the null-ness of each control point, so it cannot turn a valid segment invalid.
  Reported as a finding; see **Open questions**.
- A `FloorNumber` value object, an `Outline` value object, or a `ClearOutline` affordance — all considered and
  rejected at Gate 1.
- `src/DungeonMaster.App.UiTests`, which is deliberately outside `src/DungeonMaster.sln`.
- `FloorMethodsTests.UpdateNumber_WhenCalled_UpdatesLastModified`'s `await Task.Delay(20)`, which violates the
  composed test discipline's no-sleeping rule. Pre-existing, outside this session's surface, and proposed for
  the backlog rather than scoped here.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| How does this Domain reject an out-of-range numeric on an internal member? | `throw new ArgumentOutOfRangeException(nameof(x), x, "…")`, guarded identically in the constructor and the updater | 1 | `src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:30`, `:82` |
| How does it reject an invalid reference argument? | `ArgumentException.ThrowIfNullOrWhiteSpace`; `ArgumentNullException.ThrowIfNull` appears nowhere in the Domain project | 1 | `Area.cs:31`, `AreaMethods.cs:24`, `Dungeon.cs:25`, `DungeonMethods.cs:24`; grep over `src/DungeonMaster.Domain` returns 6 guard sites total |
| Can a test reach `Floor`'s internal constructor and `UpdateNumber`? | Yes — no accessibility change is needed | 1 | `src/DungeonMaster.Domain/AssemblyInfo.cs:3` — `[assembly: InternalsVisibleTo("DungeonMaster.Tests")]` |
| The test namespace `…Entities.Floor` shadows the type `Floor`. How does this repo handle that? | Alias the type in a `using` | 1 | `src/DungeonMaster.Tests/Domain/Entities/Floor/FloorObjectMethodsTests.cs:6` — `using DomainFloor = DungeonMaster.Domain.Entities.Floors.Floor;` |
| Does a guard in `SetOutline` break loading persisted areas? | No — EF materialises into the `_outline` backing field and never calls `SetOutline` | 1 | `AreaConfiguration.cs:95-97` — `.HasField("_outline")`, `PropertyAccessMode.PreferFieldDuringConstruction` |
| Does a guard in `PathSegment`'s constructor run on loading? | Yes — outlines are stored as JSON and `System.Text.Json` constructs a positional record through its constructor | 1 | `AreaConfiguration.cs:82-97` |
| Is that read-path risk bounded? | Yes — production has only ever written `Line` segments with null control points, so no persisted row can violate the rule | 1 | `MapDrawingSession.cs:122`, `:125`; census of 42 construction sites, every non-test `CubicBezier` count is zero |
| Would a `< 1` floor guard break an existing caller? | No — all four `UpdateNumber` call sites pass ≥ 1, verified by reading each filter rather than assuming | 1 | `DungeonMethods.cs:83`, `:132`, `:140`, `:144` |
| Is a degenerate outline reachable from the UI today? | No — `BuildOutline` returns `[]` below three vertices and otherwise emits ≥3; the other producer translates and preserves the count | 1 | `MapDrawingSession.cs:111-127`, `:68`; `MapCanvasViewModel.cs:219`, `:257` |
| Which exception type fits `SetOutline`'s count rejection? | `ArgumentException` rather than `ArgumentOutOfRangeException` — the offending argument is a collection, and `ActualValue` wants the value itself | 4 | Architect's judgment, pinned in `contracts/domain-invariants.md` §1 so all three tasks agree; raised at Gate 2 |
| Which exception type fits `PathSegment`'s rejection? | `ArgumentException` rather than `ArgumentNullException` — the invalid thing is the *combination*, not one missing parameter | 4 | Architect's judgment, pinned in `contracts/domain-invariants.md` §3; raised at Gate 2 |
| Does `System.Text.Json` surface a constructor `ArgumentException` directly or wrap it? | **Undetermined — deliberately not guessed.** T3 must run it, observe the actual type, and pin the observed one | 4 | `manifest.json` T3 criterion 4 requires the observation and requires the report to state what was seen |
| How is a positional record's primary constructor validated in C#? | Redeclare one positional member with an initializer that calls a validating static | 4 | Construction form given in `contracts/domain-invariants.md` §3; the implementer confirms by compiling |

## Contracts

One file: [`contracts/domain-invariants.md`](contracts/domain-invariants.md).

It is not bookkeeping. **T1 and T2 write new `PathSegment` values in their tests, and T3 changes what a legal
`PathSegment` is** — and T1 and T2 run first. §3.2 is therefore binding on them before the code enforcing it
exists; without it they would write segments that are legal today, illegal after T3, and T3 would inherit
failures that are not its own. The contract also pins each exception type exactly, because
`ArgumentNullException` derives from `ArgumentException` and a test written with `Throw<ArgumentException>()`
instead of `ThrowExactly` would pass against the wrong guard.

## Open questions

| Question | Who resolves it | When |
|---|---|---|
| The `with`-expression gap at `AreaOutlineGeometry.cs:42` — accept as a reported finding, close it in-session with a one-line App.Core change, or carry it to the backlog? | The user | Gate 2 |
| The two rung-4 exception-type choices (`ArgumentException` in §1's count rule and in §3) — confirm, or change now? Changing before the freeze is free; changing after is a contract escalation. | The user | Gate 2 |
