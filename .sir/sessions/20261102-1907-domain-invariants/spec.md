# Domain invariants

Close six production-correctness gaps from `.sir/backlog.md` (R1-R6): unbounded text, two
`PathSegment` validation bypasses, `ObjectKind.None` rendered as a real object, and two tests
that cannot catch a regression in the guard they cover.

## Context

Gate 1 selected six backlog rows (`discussion.md`, table R1-R6). Every row was re-measured in
this phase against `fd429e3` (`chore/repo-audit`). **All six are live. Two of their specifics
were wrong:**

- **R1's count is false.** The row says "six `MaximumLength` rules". Six *columns* are
  unbounded, and that part is right, but they reach the Application layer through **eight**
  command properties in **five** validators:
  - `CreateDungeonCommand.Description`
  - `UpdateDungeonCommand.Description` and `.Notes`
  - `UpdateFloorCommand.Description` and `.Notes`
  - `AddAreaCommand.Description`
  - `UpdateAreaCommand.Description` and `.Notes`

  Measured with
  `grep -rln "Description\|Notes" src/DungeonMaster.Application/Features --include=*Command.cs`,
  which found five files, and then by reading each record. `AddFloorCommand` carries only
  `Name`. EF already declares `HasMaxLength` on all six columns
  (`DungeonConfiguration.cs:75,79`, `FloorConfiguration.cs:83,87`,
  `AreaConfiguration.cs:122,126`), so **no migration is involved**.
- **R3 was nearly dropped on a false negative.** Gate 1's grep for a line ending in `with`
  missed the expression because the file is CRLF: the `$` anchor sits before the CR. The
  production `with` is at `src/DungeonMaster.App.Core/Map/AreaOutlineGeometry.cs:42`, and it is
  the only production `with` on a `PathSegment`.

Live evidence for the rest:

- **R2:** the bare `else` is at `src/DungeonMaster.Domain/ValueObjects/PathSegment.cs:61`.
- **R4:** the `_ =>` arms are at `ObjectKindInfo.cs:73,115,138`, and `GetDefaultAnchor`'s
  fallthrough is at `:34`.
- **R5:** no test reads `LastModified` after a failed facade mutation. The repository has
  exactly two `LastModified` test sites (`EntityEqualityTests.cs:173`,
  `FloorMethodsTests.cs:16`), and both are success paths.
- **R6:** `AreaOutlineTests.cs:165` asserts only `HaveCount(3)`.

Two pieces of documentation become false once R1 lands, so this session updates them:

- The comment blocks in the three EF configurations say these columns are "NOT BOUNDED
  ANYWHERE" and that closing the gap "is deliberately not done here".
- `NameOnlyValidatorTestBase`'s summary says each leaf's "only rule" is the Name rule. All four
  of its leaves (AddArea, CreateDungeon, UpdateArea and UpdateDungeon; `AddFloor`'s test class
  is standalone, `AddFloorCommandValidatorTests.cs:9`) would then have more rules than that.
  The architect's re-measurement corrected this from "four of five".
- The comments' own instrument counts are already false at `fd429e3`, because each grep
  counts the comment itself:
  - `grep -rn "MaximumLength" src --include=*.cs` (excluding `obj/` and `bin/`) returns 10,
    against the six-plus-one the comment states.
  - `grep -rn HasCheckConstraint` returns 3, against the 0 it states.

  T1-T3 must make every count left in a comment true, or remove it.

## Definition of done

- [ ] **D1** `CreateDungeonCommandValidator` and `UpdateDungeonCommandValidator` reject a
  `Description` of 2001 characters and accept one of exactly 2000. `UpdateDungeonCommandValidator`
  rejects a `Notes` of 4001 characters and accepts one of 4000. Each rejection carries a single
  error whose `PropertyName` is that property. A `null` value is accepted.
- [ ] **D2** `UpdateFloorCommandValidator` rejects a `Description` and a `Notes` of 2001
  characters, and accepts each at exactly 2000. Each rejection carries a single error on that
  property, and `null` is accepted.
- [ ] **D3** `AddAreaCommandValidator` and `UpdateAreaCommandValidator` reject a `Description`
  of 2001 characters and accept one of 2000. `UpdateAreaCommandValidator` does the same for
  `Notes`. Each rejection carries a single error on that property, and `null` is accepted.
- [ ] **D4** No comment in `DungeonConfiguration.cs`, `FloorConfiguration.cs` or
  `AreaConfiguration.cs` still states that these Description and Notes columns are unbounded
  in every layer, or that closing the gap is deferred. `NameOnlyValidatorTestBase`'s summary no
  longer says Name is each leaf's only rule.
- [ ] **D5** `new PathSegment(k, end, null, null)` throws `ArgumentOutOfRangeException` with
  `ParamName` `Kind` when `k` is not a defined `PathSegmentKind`. The existing `Line` and
  `CubicBezier` rules and their `ParamName`s are unchanged.
- [ ] **D6** `AreaOutlineGeometry.Translate` builds every output segment with
  `new PathSegment(...)` and contains no `with` expression. A `Line` segment that carries a
  control point (built with `with` in the test) makes `Translate` throw `ArgumentException`.
  Every existing `Translate` test still passes.
- [ ] **D7** `ObjectKindInfo.GetDefaultAnchor`, `GetObjectSize`, `GetDisplayName` and
  `GetCategory` each throw `ArgumentOutOfRangeException` with `ParamName` `kind`, both for
  `ObjectKind.None` and for an undefined value such as `(ObjectKind)999`.
- [ ] **D8** For every defined `ObjectKind` other than `None`, each of the four methods returns
  the value it returned at `fd429e3` (preserved behaviour).
- [ ] **D9** A test asserts that `Dungeon.LastModified` is unchanged after each of
  `RemoveArea`, `UpdateArea`, `UpdateAreaOutline`, `RemovePlacedObject` and
  `UpdatePlacedObject` returns `NotFound` from its inner floor operation.
- [ ] **D10** `UpdateAreaOutline_WithThreeSegments_Succeeds` asserts the stored outline equals
  the three supplied segments by value, in order (`Outline.Should().Equal(...)`).
- [ ] **D11** Each new or strengthened test in D1-D3 and D5-D10 has been shown able to fail by
  one of two routes, named in its task's criteria:
  - **An injected defect** that removes the behaviour under test, with a control that stays
    green. The injection is reverted before commit, and the file's hash is equal before and
    after.
  - **A red step**, allowed only where the guard and its test land in the same task: the test
    fails against the unchanged `fd429e3` code.

  A test that already passes at `fd429e3` (preserved behaviour) always needs an injection.
- [ ] **D12** `dotnet build src/DungeonMaster.sln` reports `0 Warning(s)` and `0 Error(s)`, and
  `dotnet test src/DungeonMaster.sln` reports no failed test, quoting the summary lines.

## Scope

R1-R6 as above, plus the D4 documentation fixes that R1 makes necessary. There are eight tasks,
run in sequence: T1-T3 cover R1 (Dungeon, Floor and Area), and T4-T8 cover R2-R6 one each.
See `manifest.json`.

### Not in scope

- The neighbouring CORE doc-comment rows (backlog lines 43, 44, 47 and 48), MASTER-178,
  MASTER-130 and the TEST style sweep, per Gate 1.
- A store-level bound (`CHECK` constraint or migration). The EF `HasMaxLength` declarations
  stay as model metadata.
- Hardening `ObjectPackRegistry`'s `Enum.TryParse` against pack entries such as `"None"` or
  `"42"`. See Assumptions.
- The facade's own floor-not-found early return in D9's five methods, because R5 names the
  inner operation.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Does R1 need EF `HasMaxLength` or a migration? | No. Both are already declared on all six columns. SQLite ignores the length, so the validator is the only enforcing layer, and that is the backlog's prescription. Nothing to escalate. | 1 | `DungeonConfiguration.cs:75,79`; `FloorConfiguration.cs:83,87`; `AreaConfiguration.cs:122,126`; the comment at `DungeonConfiguration.cs:30-67` |
| How many rules does R1 add? | Eight, across five validators. The row's figure of "six" counts columns. | 1 | `grep -rln "Description\|Notes" src/DungeonMaster.Application/Features --include=*Command.cs` returns 5 files, read individually |
| Is the limit measured in characters? | Yes, `string.Length`, which is FluentValidation `MaximumLength`'s unit and EF `HasMaxLength`'s. | 1 | Existing `.MaximumLength(100)` on Name; the `new string('A', 101)` boundary in `NameOnlyValidatorTestBase` |
| Does a `null` Description or Notes pass? | Yes. Both fields are optional (`string?`), and FluentValidation's length rule passes `null`. D1-D3 assert this, so a wrong assumption fails a test. | 4 | The command records declare `string?` |
| How does R4 reject `None` and undefined values? | It throws `ArgumentOutOfRangeException(nameof(kind), kind, …)` at the top of each method, which is the sibling precedent. | 1 | `PlacedObject.cs:22-28` (MASTER-073) |
| Do the `_ =>` arms go away? | No. They are also the legitimate default for real kinds: doors, stairs and Fountain get 1×1 size, and Fountain, Altar, Statue, Rug, Cauldron, Oven and WeaponRack get "Decoration". The guard runs before the switch, and D8 pins the real-kind outputs. | 1 | `ObjectKindInfo.cs:41-139`; `ObjectKind.cs` |
| Can the R4 throw crash a live render? | Not from stored data. `PlacedObject` refuses `None` and undefined values at construction, and EF stores `Kind` as a string. The ghost path draws only kinds chosen from the palette. The residual risk is a pack manifest entry spelled `"None"` or numerically, which `Enum.TryParse` accepts. The one shipped pack has 29 named kinds and none of those, so it is left out of scope. | 1 | `PlacedObject.cs:22`; `PlacedObjectConfiguration.cs:35-37`; `ObjectPackRegistry.cs:26`; `grep -o '"kind" *: *"[^"]*"' …/2_Minute_Tabletop/pack.json` |
| How does R2 reject an undefined kind? | An explicit `Line` branch, then `throw new ArgumentOutOfRangeException(nameof(Kind), Kind, …)`. The parameter is named `Kind`, so CA2208 holds. | 1 | `PathSegment.cs:37-43`; `PlacedObject.cs:22` |
| What does R3's test exercise? | A segment built with `with` bypasses the constructor. `new PathSegment(...)` re-validates, so `Translate` now throws where the `with` version passed the invalid segment through. That is the discriminating case. | 1 | `PathSegment.cs:35` (validation lives in the `Kind` initializer, which a `with` copy does not re-run) |
| What counts as a "failed inner operation" in R5? | The inner floor method returns `NotFound`, the only non-success outcome those five floor methods produce. | 1 | `FloorMethods.cs:95,133,167`; `FloorObjectMethods.cs:61,100` |
| How does R5 tell "unchanged" apart from a timestamp too coarse to move? | It uses a condition wait with a timeout, `SpinWait.SpinUntil(() => DateTime.UtcNow > captured, …)`, and asserts that the wait succeeded. It never sleeps. A success-path control must show `LastModified` advancing, and the injection (an unconditional `Touch()` in `TouchOnSuccess`) must turn all five cases red. | 1 | The session's test discipline, Determinism ("No sleeping"), which overrides the `Task.Delay(20)` precedent at `EntityEqualityTests.cs:175` |
| Can the eight tasks run concurrently? | No. They are chained T1→T8 with `dependsOn`. All of them build and test the whole solution in one worktree, and T4, T5, T7 and T8 inject into production files that a peer's full-suite run would see. | 1 | Every task's build and test criteria; memory `feedback_criteria_a_task_cannot_satisfy` |
| What happens to `src/DungeonMaster.App/packages.lock.json`? | The worktree's first build rewrote it: `Microsoft.NET.ILLink.Tasks` moved from 10.0.11 to 10.0.14, from the installed SDK. No task owns it. Every commit stages only the task's `owns` paths, so the file stays uncommitted and goes to the backlog at Gate 3. | 1 | `git -C C:/dm-wt/domain-invariants diff --stat` shows 1 file, 3+/3- |
| Where do the `ObjectKindInfo` tests live? | `DungeonMaster.App.UnitTests`, the plain-`net10.0` project that references App without a MAUI host. | 1 | `DungeonMaster.App.UnitTests.csproj` |

## Contracts

None. Every change is local to the type that owns it, and no new member crosses a task
boundary. The `contracts/` directory is empty by design.

## Open questions

None.
