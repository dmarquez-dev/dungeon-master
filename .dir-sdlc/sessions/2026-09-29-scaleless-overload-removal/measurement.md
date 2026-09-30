# Measurement — session 6b-xiii, `MASTER-047` / `CORE-019`

Every number in this file was produced by a command run in this session against
**`b39a65d`**, the tip of `chore/repo-audit` and the base of `dir/2026-09-29-scaleless-overload-removal`.
Nothing here is carried from the register, from the handoff, or from a previous session.

The reason this file exists: `MASTER-047` had never been re-measured by any session, and the handoff
from 6b-xii warned that its "no production caller" claim is the exact shape this project's
instruments get wrong. It also warned, from its own experience with `CORE-005`, that an entry's line
citations can go stale without anyone touching the entry. Both warnings paid off, and a third thing
turned up that the entry does not mention at all — see section 5.

---

## 1. The instrument

`MASTER-047` is a claim about **call sites of specific overloads**. A name grep cannot answer it in
this repository, for two independent reasons measured here:

1. **Every call in this repository splits one argument per line.** No single-line grep can see a
   call's arity, so a grep for `AddPlacedObject` cannot distinguish the live 5-argument overload from
   the dead 4-argument one. Both print the same first line.
2. **`MapHitTesterTests.cs:278` declares a local test helper also named `PlacedObject`**, with
   7 call sites at arities 3 and 5. A name grep counts those as constructor callers. They are not.

The instrument used instead is `scratchpad/arity.py`: it blanks comment bodies and string/char
literal bodies while preserving offsets, then paren-matches each call and counts top-level commas to
get arity. `scratchpad/deadsites.py` drives it, excluding each member's declaration site by line
number and requiring `new` for the constructor. `scratchpad/enclosing.py` then walks back from each
site to its enclosing test method.

**Predicate validation.** A count is only as good as what it counts, so the predicate was checked
three ways rather than by re-running the command:

- **A case that must match:** `AddPlacedObjectCommandHandler.cs:24` — the live production call — is
  reported `arity=5`, and is therefore *excluded* from the dead-site list. Correct.
- **A near-miss that must not match:** the same name at `DungeonMethods.cs:371` is `arity=4`, but it
  is the declaration of the dead overload rather than a call to it, and is excluded by line number.
  Correct.
- **A lookalike population that must be separated:** the `MapHitTesterTests.cs` helper is surfaced by
  the scan and rejected by the `new` requirement, rather than silently absorbed into the total.

---

## 2. `CORE-019`'s six cited locations

Measured by reading each cited range.

| Cited location | Verdict | What is actually there |
|---|---|---|
| `DungeonMethods.cs:323-333` | **STALE** | `UpdateAreaOutline`. The scale-less `AddPlacedObject` is at **`:371-381`** (corrected — see the note below §3) |
| `FloorObjectMethods.cs:10-14` | correct | `Floor.AddObject(kind, position, rotation)` |
| `FloorObjectMethods.cs:96-113` | correct | `Floor.UpdateObject(objectId, newPosition, newRotation)` |
| `PlacedObject.cs:13-18` | correct | the four-argument `PlacedObject` constructor |
| `PlacedObject.cs:62-63` | **STALE** | the `Rotation` property's doc comment. `Update(newPosition, newRotation)` is at **`:69-71`** |
| `AssemblyInfo.cs:3` | correct | `[assembly: InternalsVisibleTo("DungeonMaster.Tests")]` |

**2 of 6 citations are stale.** The one in-prose citation — `FloorObjectMethods.cs:109`, where dead
`Floor.UpdateObject` calls dead `PlacedObject.Update` — **is correct.**

Neither stale citation was caused by work on `MASTER-047`. Both were caused by the ObjectScale
feature work, which inserted the scale-carrying overloads above and beside the scale-less ones. This
is the same mechanism 6b-xii filed as a backlog item after `CORE-006`'s fix falsified `CORE-005`:
**a fix aimed at one entry silently falsifies its neighbours, in prose, at lines that may never
move.** File line counts at `b39a65d`: `DungeonMethods.cs` 519, `FloorObjectMethods.cs` 157,
`PlacedObject.cs` 101, `AssemblyInfo.cs` 3.

---

## 3. The load-bearing claim: "no production caller"

**It holds.** Five members, all `internal` or `public` on an `internal`-reachable path, callable from
the test assembly only through `InternalsVisibleTo("DungeonMaster.Tests")`:

| Member | Declared at | Production calls | Test calls |
|---|---|---|---|
| `Dungeon.AddPlacedObject(floorId, kind, position, rotation)` | `DungeonMethods.cs:371-381` | **0** | 6 |
| `Floor.AddObject(kind, position, rotation)` | `FloorObjectMethods.cs:7-14` | **0** | 7 |
| `Floor.UpdateObject(objectId, newPosition, newRotation)` | `FloorObjectMethods.cs:79-113` | **0** | 2 |
| `PlacedObject.Update(newPosition, newRotation)` | `PlacedObject.cs:69-71` | **1**, see below | 2 |
| `PlacedObject(floorId, kind, position, rotation)` | `PlacedObject.cs:12-18` | **0** | 5 |

> **CORRECTION, 2026-09-29, after T2 — this file introduced a third stale citation and it is
> recorded here rather than quietly fixed.** The first row above originally read **`:371-377`**, in
> this table and in §2. **The member spans `:371-381`**: signature `:371-375`, delegating body
> `:376-381`, doc comment `:353-370`. `:371-377` stops mid-expression, after `=> AddPlacedObject(`
> and `floorId,`.
>
> **Cause, stated plainly: the orchestrator inferred the span's end from two grep hits instead of
> reading the range.** The grep showed a declaration at `:371` and `=> AddPlacedObject(` at `:376`,
> and the end was assumed rather than measured. It then propagated from this table into **frozen
> criteria B1 and C2**, and C2 would have written it into `CORE-019`'s locations list — in the
> session whose entire subject is repairing stale citations in that entry.
>
> **Caught by T2's implementer**, which measured the range before deleting it. Corrected before T3
> was dispatched, by escalation **E2**; see `state.json` amendments **A1**, **A2** and **A3**, and
> criterion **C14**, which carries the finding into the register itself. **The other four members'
> ranges in this table were verified line-for-line by T2 and are exact.**
>
> This correction is written as a note rather than a silent edit for the same reason `CORE-019` is
> being amended rather than rewritten: what was believed at the time is part of the record.

**The single surviving production call site is `FloorObjectMethods.cs:109`**, where dead
`Floor.UpdateObject(3-arg)` calls dead `PlacedObject.Update(2-arg)`. That is exactly what the entry's
own prose says, and it is why the entry treats the five as a cluster rather than five findings. It is
not a counter-example to the claim.

Every production path passes an explicit scale:

- `AddPlacedObjectCommandHandler.cs:24` → `dungeon.AddPlacedObject(..., command.Scale)` — arity 5
- `DungeonMethods.cs:419` → `floor.AddObject(kind, position, rotation, scale)` — arity 4
- `DungeonMethods.cs:504` → `floor.UpdateObject(objectId, newPosition, newRotation, newScale)` — arity 4
- `FloorObjectMethods.cs:40` → `new PlacedObject(Id, kind, position, rotation, scale)` — arity 5

### The 22 dead call sites, all in `DungeonMaster.Tests`

**22 call sites across 7 files.** Every one is in `DungeonMaster.Tests`. No other test project holds
an `InternalsVisibleTo` grant from the Domain, and none appears in the scan.

| File | Sites | Lines |
|---|---|---|
| `Domain/Entities/Floor/FloorObjectMethodsTests.cs` | 8 | 24, 41, 54, 79, 117, 133 (`AddObject`); 140, 158 (`UpdateObject`) |
| `Domain/Entities/Floor/PlacedObjectTests.cs` | 6 | 15, 45, 63, 99 (ctor); 69, 123 (`Update`) |
| `Application/Features/Dungeons/Commands/UpdatePlacedObjectCommandHandlerTests.cs` | 3 | 32, 137, 186 |
| `Domain/Entities/Dungeon/DungeonAggregateTests.cs` | 2 | 439, 462 |
| `Domain/Entities/Area/AreaOutlineTests.cs` | 1 | 67 |
| `Domain/Entities/Floor/ObjectKindGuardTests.cs` | 1 | 50 |
| `Application/Features/Dungeons/Commands/RemovePlacedObjectCommandHandlerTests.cs` | 1 | 32 |

---

## 4. Two facts `CORE-019` does not state

**4.1 — The cluster is closed.** Each of the five members' bodies does nothing but delegate inward to
a live overload:

- `Dungeon.AddPlacedObject(4)` → `AddPlacedObject(5)` with `ObjectScale.Default`
- `Floor.AddObject(3)` → `AddObject(4)` with `ObjectScale.Default`
- `Floor.UpdateObject(3)` → `PlacedObject.Update(2)` → `Update(3)` with the object's current `Scale`
- `PlacedObject(4)` → `this(…, ObjectScale.Default)`

Deleting all five dangles nothing: no live production path routes through any of them, and no member
outside the cluster is only reachable via one.

**4.2 — The convenience chain is asymmetric.** `Dungeon` exposes a scale-less `AddPlacedObject` but
**no** scale-less `UpdatePlacedObject`: `UpdatePlacedObject` has exactly one declaration,
`DungeonMethods.cs:489`, and it requires `newScale`. The defaulting is present on one of the two
aggregate mutators and absent on the other, which is evidence the scale-less forms are residue from
the pre-`ObjectScale` API rather than a designed convenience. That bears on `CORE-019`'s own fallback
suggestion of keeping one level of defaulting: there is no existing convention to preserve.

---

## 5. The fact neither the entry nor the handoff anticipated

**Seven of the affected tests are named after the API being deleted, and their subject IS the
defaulting.** For those, appending an argument does not update the test — it destroys what the test
asserts. This is not a mechanical edit, and the disposition is three-way rather than one-way.

`scratchpad/enclosing.py` reports that 9 of the 22 sites sit in a test whose body references
`ObjectScale.Default` or asserts on `.Scale`. Reading those tests gives the following disposition.
All seven are `[Fact]`, so **seven xunit test cases** are in question, not seven files.

### 5.1 — Convert, do not delete (2 tests): coverage exists nowhere else

| Test | Why |
|---|---|
| `DungeonAggregateTests.AddPlacedObject_WithoutScale_…` (`:431-452`) | These are **the only two `AddPlacedObject` tests in `DungeonAggregateTests.cs`**. Deleting them leaves `Dungeon.AddPlacedObject` with no aggregate-level unit test of any arity |
| `DungeonAggregateTests.AddPlacedObject_WithoutScaleOnUnknownFloorId_ReturnsNull` (`:455-470`) | The unknown-floor-id → `null` path has **no other aggregate-level test**. It is covered indirectly at `AddPlacedObjectCommandHandlerTests.cs:118` (`HandleAsync_WithUnknownFloorId_ReturnsNotFoundNamingTheFloor`), but that is a different level and asserts a different thing — `ErrorCategory.NotFound`, not `null` |

Converting means passing `ObjectScale.Default` explicitly **and renaming**, because `_WithoutScale`
in the name would no longer describe what the test does.

### 5.2 — Delete as exact duplicates (2 tests): a scale-carrying twin already exists

| Test to delete | Its surviving twin |
|---|---|
| `FloorObjectMethodsTests.UpdateObject_WithoutScaleOnExistingObject_…` (`:129-151`) | `UpdateObject_AppliesPositionRotationAndScale` (`:76`, arity 4) |
| `FloorObjectMethodsTests.UpdateObject_WithoutScaleOnUnknownId_ReturnsNotFound` (`:152-165`) | `UpdateObject_UnknownId_ReturnsNotFound` (`:98`, arity 4) |

These differ from their twins only in arity. Converting them would produce two literal duplicates.

### 5.3 — Delete as subject-removed (3 tests): the asserted behaviour ceases to exist

| Test | What it asserts, and why conversion is vacuous |
|---|---|
| `PlacedObjectTests.Update_WithDefaultScale_KeepsDefaultScale` (`:61-72`) | Asserts the 2-arg `Update` leaves `Scale` at `Default`. With the overload gone there is no scale-omitting call to make; converted, it would assert that passing `Default` yields `Default` |
| `PlacedObjectTests.Update_TwoParam_PreservesCustomScale` (`:114-128`) | Its subject is named in the test name — the two-parameter overload's preservation of an existing scale. Converted, it would assert that passing `2.0f` yields `2.0f` |
| `ObjectKindGuardTests.FourParameterConstructor_WithNoneKind_Throws` (`:47-60`) | Asserts the 4-arg ctor rejects `ObjectKind.None` with `ParamName == "kind"`. **The `None` rejection survives** at `KindGuardRunsBeforeTheScaleRejection` (`:83`), which passes `None` to the 5-arg ctor and asserts the same `ParamName`. Converting would duplicate `FiveParameterConstructor_WithUndefinedKind_Throws` at `:66` |

The third row is the shape this project has recorded before: a guard test that duplicates a
downstream rejection. It survives deletion **only because** a sibling asserts the same `ParamName` on
the same input through the surviving overload. That sibling must be confirmed still present, not
assumed.

### 5.4 — Mechanical (13 sites): append an explicit scale

The remaining 13 sites use the short form for brevity. Their enclosing tests assert nothing about
scale, or assert a *custom* scale set elsewhere in the body:

`RemovePlacedObjectCommandHandlerTests.cs:32`; `UpdatePlacedObjectCommandHandlerTests.cs:32, 137,
186`; `AreaOutlineTests.cs:67`; `FloorObjectMethodsTests.cs:24, 41, 54, 79, 117`;
`PlacedObjectTests.cs:15, 45, 99`.

Two of these need a second look rather than a blind append, because their bodies do touch scale:
`UpdatePlacedObjectCommandHandlerTests.cs:32` references `ObjectScale.Default` later in the body, and
`PlacedObjectTests.cs:15` (`Constructor_SetsAllProperties`) asserts `Scale` equals `Default` — which
after conversion becomes an assertion about the argument just passed. `:15` is arguably a
subject-removed case rather than a mechanical one; it is listed here because
`Constructor_SetsAllProperties` asserts five properties and only one of them is `Scale`.

### 5.5 — The predicted suite delta

Five `[Fact]` deletions (5.2 and 5.3) ⇒ **`DungeonMaster.Tests` 855 → 850**, total **980 → 975**, if
nothing is added. The two conversions in 5.1 keep their test cases. **This is a prediction, not a
measurement**, and it is recorded here so that a different number after the change reads as a
surprise to investigate rather than as a figure to rationalise. Any acceptance criterion phrased as
"the suite count is unchanged" is unsatisfiable by construction.

---

## 6. What the deletion will need, and what it will not disturb

`ObjectScale` (`src/DungeonMaster.Domain/Entities/Floors/ObjectScale.cs`, 12 lines) is a **`public
static class` of `float` constants**, not a value object: `Min` 0.25f, `Max` 4.0f, `Step` 0.25f,
`Default` 1.0f. `Default` is `public`, so the test project can name it without a new grant, and the
scale parameters on the surviving overloads are plain `float`. `CORE-019`'s suggested direction is
viable as written.

**`ObjectScale.Default` does not become dead.** The deletion removes three of its production
references (`DungeonMethods.cs:381`, `FloorObjectMethods.cs:14`, `PlacedObject.cs:18`), leaving **ten**
others, including the default parameter values on `AddPlacedObjectCommand.cs:34` and
`UpdatePlacedObjectCommand.cs:34`, and `FloorMapDrawable.GhostScale`. This was checked because a
deletion that silently orphans a constant would be a new finding, and it does not.

**`PlacedObject.cs:67`'s field initializer `= ObjectScale.Default` stays.** It is not part of the
dead cluster: the private parameterless constructor at `:10` is EF Core's materialization path and
assigns nothing itself, so the initializer is what gives `Scale` a value at the moment that
constructor returns.

> **CORRECTION, 2026-09-29, after T2's review — the sentence this paragraph originally carried was
> false, and it reached a frozen criterion.** It read: *"the private parameterless constructor at
> `:10` — EF's materialization path — assigns nothing, so the initializer is **the only thing setting
> `Scale` on that path**."* That last clause is wrong, and criterion **B6** restated it as "removing
> it would silently give every entity materialized from the database a scale of 0".
>
> **What is true:** EF invokes the parameterless constructor and **then assigns `Scale` from its
> mapped column**, so the initializer is overwritten on every load and is not the setter on that
> path. **Removing it is completely inert** — measured by T2's reviewer, which deleted it and ran the
> full suite: **975 passed, 0 failed, 0 skipped.**
>
> **The in-repo proof, which was available the whole time:**
> `src/DungeonMaster.Tests/Infrastructure/Persistence/Configurations/PlacedObjectConfigurationTests.cs:48`
> commits and reloads through real SQLite and asserts `reloaded.Scale` equals the *parameterised*
> scale; `:63-64` assert the property is ordinary data (`TryGetDefaultValue` false,
> `ValueGenerated.Never`). Neither could pass if the initializer were the setter on that path.
>
> **Cause, stated plainly: the orchestrator asserted a mechanism it had not run.** Same failure mode
> as the `:371-377` range corrected above — a confident statement about how something behaves,
> propagated into a frozen criterion unmeasured. **This is the generalisation worth carrying: the
> inherited rule was "run the claim about the tool", and it is too narrow. It is any claim about a
> mechanism.**
>
> **What survives, now measured rather than assumed:** the initializer is a defence-in-depth default
> for non-EF construction paths, and **no test in the suite catches its removal.** Recorded in the
> register by criterion **C15** under escalation **E3**; see `state.json` amendment **A4**.

A separate deferred register item proposes turning `ObjectScale` into a value object. This session
does not touch that, and the deletion does not depend on which way it goes.

---

## 7. Suite baseline

Measured in the **main checkout** at `b39a65d` with `dotnet test src/DungeonMaster.sln --nologo`:

| Assembly | Result | Ran from |
|---|---|---|
| `DungeonMaster.Tests` | 855 passed, 0 failed | `bin/Debug/net10.0` |
| `DungeonMaster.App.UnitTests` | 23 passed, 0 failed | `bin/Debug/net10.0` |
| `DungeonMaster.App.Tests` | 102 passed, 0 failed | `bin/Debug/net10.0-windows10.0.19041.0` |

**Total 980, zero failures**, and `grep -cE "(warning|error) [A-Z]+[0-9]+"` over the output returns
**0**.

The TFM column comes from the `Test run for …` lines, **not** from the `Passed!` lines. The `Passed!`
line prints `(net10.0)` for all three assemblies including `App.Tests`, which demonstrably ran from
the Windows TFM directory. The 6b-xii handoff recorded this as a false check it had inherited; it
reproduces exactly here, so the correction stands.

`DungeonMaster.Tests` is the whole blast radius: all 22 edits land there, and its 855 tests are the
check.

---

## 8. What is measured here and what is not

**Measured:** the six citations; the arity of every call site of the five members; the delegation
target of each member's body; the absence of a scale-less `UpdatePlacedObject`; the enclosing test of
all 22 sites and which of them assert on scale; the surviving twin for each proposed deletion;
`ObjectScale`'s shape, accessibility and remaining reference count; the suite baseline and its
per-assembly TFM.

**Not measured, and no criterion may rest on it:**

- **Whether the five members are reachable by reflection or serialization.** Nothing in the scan
  would see it. EF Core materializes `PlacedObject` through the private parameterless constructor at
  `PlacedObject.cs:10`, not through the 4-argument one, and that was read rather than inferred — but
  a general "nothing reflects over these" claim is not established and must not be written as one.
- **Whether the suite passing after the change proves the deletion safe.** It proves no test
  observed a difference. The production paths were separately shown to route around all five
  members, and that argument, not the green suite, is what carries the claim.
