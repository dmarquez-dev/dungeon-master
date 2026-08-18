# Contract: the Domain mutator return-type convention

Frozen at Gate 2. **Authored by T1. Consumed by T4.** T4 maps every value in this file onto an
`ErrorCategory`; if T1 changes a signature after Gate 2, T4 breaks silently.

Closes: **MASTER-021** (the Domain half — the signal), **MASTER-010** (both halves),
**MASTER-003**. Supplies the signal that **MASTER-021**'s Application half (T4) consumes.

---

## 1. The defect being closed

Every `Dungeon` mutator that resolves a child by id returns `void` and returns early on a miss:

```
grep -n 'if (floor is null)' src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs
grep -n 'if (area is null)'  src/DungeonMaster.Domain/Entities/Floor/FloorMethods.cs
```

The handler cannot observe the miss and returns `Success` regardless. `Dungeon.AddArea` is the
single exception and it goes the other way — it **throws** `InvalidOperationException`
(`DungeonMethods.cs:198`), which no handler catches:

```
grep -rn 'InvalidOperationException' src/DungeonMaster.Domain
```

Three reactions to one condition — silent no-op, `null`, and a throw. This contract settles on one.

`Floor.UpdateNumber` (`FloorMethods.cs:13-14`) is the only mutator in the Domain that writes state
without calling `Touch()`.

---

## 2. `MutationOutcome`

**New file** `src/DungeonMaster.Domain/Entities/MutationOutcome.cs`.

```csharp
namespace DungeonMaster.Domain.Entities;

/// <summary>
///		The outcome of an aggregate mutation that resolves a child entity by identifier.
/// </summary>
public enum MutationOutcome
{
	/// <summary>
	///		The mutation was applied, or was a no-op that leaves the aggregate in the requested state.
	/// </summary>
	Success,

	/// <summary>
	///		No child with the supplied identifier exists on the aggregate, so nothing was changed.
	/// </summary>
	NotFound,

	/// <summary>
	///		The requested value lies outside the range the aggregate permits, so nothing was changed.
	/// </summary>
	OutOfRange,
}
```

`Success` deliberately covers the *idempotent* no-op: `ReorderFloor` called with the number the
floor already has changed nothing, and that is a success, not a failure.

**Why an enum and not `bool`.** CORE-001: "`ReorderFloor`'s range rejection is a distinct outcome
from 'floor not found' and is worth distinguishing." A `bool` cannot carry it, and MASTER-031 shows
what happens when a not-found condition is squeezed into the wrong category.

**Why the Domain, not the Application.** The Domain project must not reference the Application
project. `MutationOutcome` therefore knows nothing about `ErrorCategory`; the mapping is T4's, in
`contracts/handler-skeleton.md` §3.

**Why `Entities/` and not `ValueObjects/`.** It is not a value object; it is the return channel of
the entity methods, and it sits beside `Entity.cs` which every entity in that folder derives from.

---

## 3. The convention, stated once

> A Domain mutator that resolves a child entity by identifier reports whether it found it.
> If the mutator **creates** the child, it returns the child, nullable, and `null` means the parent
> was not found. Otherwise it returns `MutationOutcome`.
> A mutator that resolves nothing — one that only writes fields of the entity it is called on —
> keeps returning `void`.
> Every mutator that writes state calls `Touch()` as its last statement.

That last clause is `backend-conventions.md` §3: "Mutable entities record a last-modified timestamp
updated whenever state changes", laid out as `dotnet-format.md` §11 shows it — `Touch()` last,
preceded by a blank line.

### 3.1 Where this sits relative to `DomainException`

`dotnet-guidelines.md` §6.2 reserves exceptions for "domain invariant violations
(`DomainException`)" and excludes "not-found lookups". A missing child id is a **not-found lookup**,
not an invariant violation: the aggregate is perfectly valid before and after. So it becomes an
outcome, and `Dungeon.AddArea` stops throwing. Constructor guards and `ArgumentOutOfRangeException`
inside `PlacedObject` are **unchanged** — those are genuine invariant violations and stay
exceptions. This session does not convert a single existing `throw` other than the one at
`DungeonMethods.cs:198`.

---

## 4. The frozen signature table

Every row is binding. A signature not in this table does not change.

### `src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs`

| Member | Before | After | Notes |
|---|---|---|---|
| `Update(name, description, notes)` | `void` | `void` | resolves nothing |
| `AddFloor(name)` | `Floor` | `Floor` | cannot fail |
| `RemoveFloor(floorId)` | `void` | `MutationOutcome` | `NotFound` on miss |
| `ReorderFloor(floorId, newNumber)` | `void` | `MutationOutcome` | `NotFound` on missing floor; `OutOfRange` when `newNumber < 1 \|\| newNumber > _floors.Count`; `Success` when `oldNumber == newNumber` |
| `UpdateFloor(floorId, ...)` | `void` | `MutationOutcome` | `NotFound` on miss |
| `AddArea(floorId, name, description)` | `Area` (throws) | `Area?` | **stops throwing**; `null` means the floor was not found |
| `RemoveArea(floorId, areaId)` | `void` | `MutationOutcome` | `NotFound` for a missing floor **or** a missing area — the caller cannot tell them apart and does not need to |
| `UpdateArea(floorId, areaId, ...)` | `void` | `MutationOutcome` | as above |
| `UpdateAreaOutline(floorId, areaId, outline)` | `void` | `MutationOutcome` | as above |
| `AddPlacedObject(...)` both overloads | `PlacedObject?` | `PlacedObject?` | **unchanged** — already correct |
| `RemovePlacedObject(floorId, objectId)` | `void` | `MutationOutcome` | `NotFound` for a missing floor or object |
| `UpdatePlacedObject(floorId, objectId, ...)` | `void` | `MutationOutcome` | as above |

### `src/DungeonMaster.Domain/Entities/Floor/FloorMethods.cs`

| Member | Before | After | Notes |
|---|---|---|---|
| `UpdateNumber(int number)` | `void`, **no `Touch()`** | `void`, **calls `Touch()`** | this line alone is MASTER-003 |
| `UpdateDetails(...)` | `void` | `void` | resolves nothing |
| `AddArea(...)` | `Area` | `Area` | cannot fail |
| `RemoveArea(areaId)` | `void` | `MutationOutcome` | |
| `UpdateArea(areaId, ...)` | `void` | `MutationOutcome` | |
| `UpdateAreaOutline(areaId, outline)` | `void` | `MutationOutcome` | |

### `src/DungeonMaster.Domain/Entities/Floor/FloorObjectMethods.cs`

| Member | Before | After | Notes |
|---|---|---|---|
| `AddObject(...)` both overloads | `PlacedObject` | `PlacedObject` | cannot fail |
| `RemoveObject(objectId)` | `bool` | `MutationOutcome` | `false` becomes `NotFound` |
| `UpdateObject(...)` both overloads | `bool` | `MutationOutcome` | `false` becomes `NotFound` |

### `src/DungeonMaster.Domain/Entities/Area/AreaMethods.cs`

Unchanged. `Area.Update` and `Area.SetOutline` resolve nothing and already call `Touch()`.

**Uniformity note.** `Floor.RemoveObject` and `Floor.UpdateObject` already returned `bool` and
worked. They are converted anyway, because CORE-002 asks to "settle on one convention for 'child
not found' across the whole aggregate", and two conventions is what produced this entry.

---

## 5. Ripple outside the Domain that T1 owns

Changing `Dungeon.AddArea` from `Area` to `Area?` makes `AddAreaCommandHandler.cs:36`'s `area.Id`
a CS8602 nullable dereference, which is an **error** under `TreatWarningsAsErrors`. T1 therefore
also owns, and must update:

- `src/DungeonMaster.Application/Features/Dungeons/Commands/AddArea/AddAreaCommandHandler.cs`
- `src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddAreaCommandHandlerTests.cs`

The handler maps `null` to `Result<Guid>.Failure($"Floor {command.FloorId} not found.",
ErrorCategory.NotFound)` — CORE-002's suggested direction verbatim, and the same message shape
MASTER-031 requires elsewhere. T4 later rewrites this handler onto the shared skeleton; that is a
sequenced overlap, not a conflict.

**Nothing else ripples.** Every other mutator goes `void` to a returned value, and a caller that
ignores a return value still compiles. Verified by inspection of all fourteen handlers: none of
them reads a mutator return today except `AddArea` and `AddPlacedObject`.

---

## 6. Tests T1 owns

`src/DungeonMaster.Tests/Domain/` — the existing files
`Entities/Dungeon/DungeonAggregateTests.cs`, `Entities/Floor/FloorObjectMethodsTests.cs`,
`Entities/Floor/PlacedObjectTests.cs`, `Entities/Floor/ObjectScaleTests.cs`,
`Entities/Area/AreaOutlineTests.cs`, plus whatever new files the table below needs.

Required new tests, one per changed mutator, each **failing before the change**:

| Test | Asserts |
|---|---|
| `RemoveFloor_WithUnknownFloorId_ReturnsNotFound` | the mutator now reports the miss |
| `RemoveFloor_WithKnownFloorId_ReturnsSuccess` | the happy path still works |
| `ReorderFloor_WithUnknownFloorId_ReturnsNotFound` | |
| `ReorderFloor_WithNumberBelowOne_ReturnsOutOfRange` | the distinction CORE-001 asked for |
| `ReorderFloor_WithNumberAboveFloorCount_ReturnsOutOfRange` | |
| `ReorderFloor_WithUnchangedNumber_ReturnsSuccess` | idempotent no-op is a success |
| `UpdateFloor_WithUnknownFloorId_ReturnsNotFound` | |
| `AddArea_WithUnknownFloorId_ReturnsNull` | **and does not throw** — assert with `.Should().BeNull()` on the returned `Area?`, and add an explicit `act.Should().NotThrow()` so the removal of the `throw` is proved, not inferred |
| `RemoveArea_WithUnknownAreaId_ReturnsNotFound` | |
| `UpdateArea_WithUnknownAreaId_ReturnsNotFound` | |
| `UpdateAreaOutline_WithUnknownAreaId_ReturnsNotFound` | |
| `RemovePlacedObject_WithUnknownObjectId_ReturnsNotFound` | |
| `UpdatePlacedObject_WithUnknownObjectId_ReturnsNotFound` | |
| `UpdateNumber_WhenCalled_UpdatesLastModified` | MASTER-003. `Floor.UpdateNumber` is `internal`, so drive it through `Dungeon.ReorderFloor` and assert every renumbered floor's `LastModified` advanced. Capture `LastModified` before, then assert `>` after. |

`UpdateNumber_WhenCalled_UpdatesLastModified` is the one test in this list with a timing hazard:
`Entity.Touch()` writes `DateTimeOffset.UtcNow`, which may not advance between two adjacent
statements. The frozen shape, so this is not re-litigated in review:

```csharp
// Arrange
var dungeon = new Dungeon("Test Dungeon");
var first = dungeon.AddFloor("Entry");
var second = dungeon.AddFloor("Cellar");
var before = second.LastModified;

await Task.Delay(20);

// Act
var outcome = dungeon.ReorderFloor(
	first.Id,
	2);

// Assert
outcome.Should().Be(MutationOutcome.Success);
second.LastModified.Should().BeAfter(before);
```

`ReorderFloor` renumbers the *other* floors through `Floor.UpdateNumber`, so asserting on `second`
exercises exactly the line MASTER-003 names. **Do not** weaken this to "the method contains a
`Touch()` call" — a source assertion is not a behavioural criterion and this session forbids them.
