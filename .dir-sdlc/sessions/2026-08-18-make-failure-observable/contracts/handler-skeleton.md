# Contract: the command handler skeleton

Frozen at Gate 2. **Authored by T4. Consumed by T6 (registration and dispatch), T7 (validators) and
T8 (the App call sites).**

Closes: **MASTER-091** (the duplicated skeleton), **MASTER-021**'s Application half (the command
stops reporting success for a mutation that did not happen), **MASTER-031**, and **MASTER-088**'s
always-true-`bool` half.

Depends on `contracts/result-and-error-surface.md` (T2), `contracts/domain-mutation-outcome.md`
(T1) and `contracts/persistence-write-contract.md` (T3).

---

## 1. The duplication being removed

Twelve of the fourteen handlers repeat one skeleton — load, guard, mutate, save, succeed. The
not-found message is written out twelve times. The command that counts it, and that a reviewer
re-runs to check it fell to zero:

```
grep -rho 'Dungeon {command\.[A-Za-z]*} not found\.' \
  src/DungeonMaster.Application/Features/Dungeons/Commands/*/*Handler.cs | wc -l
```

**12 before this task. 0 after** — the string moves into the helper, where it is written once per
`MutateAsync` overload (§2) and nowhere else.
(`AddPlacedObjectCommandHandler` contains the phrase twice today: once for the dungeon guard and
once inside the opaque `"Failed to add placed object."` sibling. The `grep` above matches only the
interpolated dungeon form, hence 12 and not 13.)

The two handlers that do **not** share the skeleton and do not use the helper are
`CreateDungeonCommandHandler` (constructs, does not load) and `DeleteDungeonCommandHandler`
(deletes by id, does not load). CORE-013 names these two as the exceptions.

---

## 2. `MutateAsync` — the shared prologue, written once

**New file** `src/DungeonMaster.Application/Common/Handlers/DungeonMutationExtensions.cs`.

```csharp
namespace DungeonMaster.Application.Common.Handlers;

/// <summary>
///		Provides the shared load-guard-mutate-save prologue that every dungeon mutation command
///		handler runs.
/// </summary>
public static class DungeonMutationExtensions
{
	/// <summary>
	///		Loads the dungeon, applies the supplied mutation, and saves it when the mutation
	///		succeeded.
	/// </summary>
	public static async Task<Result> MutateAsync(
		this IDungeonWriteRepository writeRepository,
		Guid dungeonId,
		Func<Dungeon, Result> mutate,
		CancellationToken ct)
	{
		var dungeon = await writeRepository.LoadAsync(
			dungeonId,
			ct);

		if (dungeon is null)
		{
			return Result.Failure(
				$"Dungeon {dungeonId} not found.",
				ErrorCategory.NotFound);
		}

		var result = mutate(dungeon);

		if (result.IsFailure)
		{
			return result;
		}

		await writeRepository.UpdateAsync(
			dungeon,
			ct);

		return result;
	}

	/// <summary>
	///		Loads the dungeon, applies the supplied value-producing mutation, and saves it when the
	///		mutation succeeded.
	/// </summary>
	public static async Task<Result<TResult>> MutateAsync<TResult>(
		this IDungeonWriteRepository writeRepository,
		Guid dungeonId,
		Func<Dungeon, Result<TResult>> mutate,
		CancellationToken ct)
	{
		var dungeon = await writeRepository.LoadAsync(
			dungeonId,
			ct);

		if (dungeon is null)
		{
			return Result<TResult>.Failure(
				$"Dungeon {dungeonId} not found.",
				ErrorCategory.NotFound);
		}

		var result = mutate(dungeon);

		if (result.IsFailure)
		{
			return result;
		}

		await writeRepository.UpdateAsync(
			dungeon,
			ct);

		return result;
	}
}
```

**Two overloads, one for each result shape, and the duplication is deliberate.** `Result` and
`Result<T>` are frozen as siblings with no common base
(`contracts/result-and-error-surface.md` §2.1), so neither overload can delegate to the other
without a conversion that would defeat the point. The bodies are line-for-line identical apart from
the two `Result` / `Result<TResult>` tokens; **a reviewer must not raise the duplication as a
finding**, and an implementer must not introduce a shared base, an interface, or a
`Func`-parameterised core to eliminate it. Ten of the twelve skeleton handlers bind the first
overload; `AddArea` and `AddPlacedObject` bind the second. Overload resolution is unambiguous
because the two delegate types differ.

Binding behaviour, all of it observable and all of it tested:

1. **A missing dungeon is `NotFound` with the id in the message** — unchanged from today, but now
   written in this file only. The interpolated string appears **twice** here, once per overload;
   what matters is that it appears **zero** times under `Features/Dungeons/Commands/`.
2. **A failed mutation short-circuits the save.** Today every handler calls `UpdateAsync`
   unconditionally, which CORE-001 calls "a pointless `UpdateAsync` round-trip". After this change,
   `UpdateAsync` is not called when the mutation reported `NotFound` or `OutOfRange`. This is the
   single most testable consequence of the whole entry: `_writeRepository.DidNotReceive()
   .UpdateAsync(...)`.
3. **The mutation delegate is synchronous.** Domain mutators are synchronous; making the delegate
   `Func<Dungeon, Task<Result<TResult>>>` would invite I/O into the aggregate.
4. `ct` is forwarded to both repository calls — `dotnet-guidelines.md` §1.4.

**Why an extension method and not an abstract base class.** CORE-013's suggested direction names
`writeRepository.MutateAsync(id, dungeon => ..., ct)` explicitly. It also keeps every handler
`sealed` with a primary constructor, which is what `dotnet-format.md` §3 prescribes and what all
fourteen handlers already are. A base class would force a protected-abstract-method shape on all
twelve and change their constructor form for no gain.

---

## 3. `MutationOutcome` to `Result` — the mapping, written once

**New file** `src/DungeonMaster.Application/Common/Handlers/MutationOutcomeExtensions.cs`.

```csharp
namespace DungeonMaster.Application.Common.Handlers;

/// <summary>
///		Maps a Domain mutation outcome onto the Application layer's result type.
/// </summary>
public static class MutationOutcomeExtensions
{
	/// <summary>
	///		The message used when a mutation reports a value outside the permitted range and the
	///		caller supplied no more specific wording.
	/// </summary>
	public const string DefaultOutOfRangeMessage =
		"The requested change is outside the permitted range.";

	/// <summary>
	///		Maps the outcome, using the supplied message for a not-found result.
	/// </summary>
	public static Result ToResult(
		this MutationOutcome outcome,
		string notFoundMessage) =>
			outcome.ToResult(
				notFoundMessage,
				DefaultOutOfRangeMessage);

	/// <summary>
	///		Maps the outcome, using the supplied messages for a not-found and an out-of-range result.
	/// </summary>
	public static Result ToResult(
		this MutationOutcome outcome,
		string notFoundMessage,
		string outOfRangeMessage) =>
			outcome switch
			{
				MutationOutcome.Success =>
					Result.Success(),
				MutationOutcome.NotFound =>
					Result.Failure(
						notFoundMessage,
						ErrorCategory.NotFound),
				MutationOutcome.OutOfRange =>
					Result.Failure(
						outOfRangeMessage,
						ErrorCategory.Validation),
				_ =>
					throw new ArgumentOutOfRangeException(
						nameof(outcome),
						outcome,
						"Unrecognised mutation outcome."),
			};
}
```

`ToResult` returns the **non-generic** `Result`, which is what makes the ten no-payload handlers a
single expression: the mutator reports a `MutationOutcome`, `ToResult` maps it, and the first
`MutateAsync` overload consumes it. There is no `Result<Unit>` and no `Unit.Value` anywhere.

The `NotFound`-to-`ErrorCategory.NotFound` and `OutOfRange`-to-`ErrorCategory.Validation` mapping
is the routing job `backend-conventions.md` §2 gives the category, and it is what MASTER-031
complains is done wrongly today.

---

## 4. The frozen command result-type table

Every one of the ten `ICommand<bool>` commands becomes the **non-generic `ICommand`**, whose handler
returns `Task<Result>`. The four `ICommand<Guid>` commands are unchanged. Nothing becomes `ICommand`
that was not `ICommand<bool>`, and nothing that returned a `Guid` stops.

The non-generic `ICommand` and `ICommandHandler<TCommand>` are declared by T6 in
`contracts/dispatch-and-pipeline.md` §1. T4 consumes them.

| Command | Before | After |
|---|---|---|
| `AddAreaCommand` | `ICommand<Guid>` | `ICommand<Guid>` |
| `AddFloorCommand` | `ICommand<Guid>` | `ICommand<Guid>` |
| `AddPlacedObjectCommand` | `ICommand<Guid>` | `ICommand<Guid>` |
| `CreateDungeonCommand` | `ICommand<Guid>` | `ICommand<Guid>` |
| `DeleteDungeonCommand` | `ICommand<bool>` | `ICommand` |
| `RemoveAreaCommand` | `ICommand<bool>` | `ICommand` |
| `RemoveFloorCommand` | `ICommand<bool>` | `ICommand` |
| `RemovePlacedObjectCommand` | `ICommand<bool>` | `ICommand` |
| `ReorderFloorCommand` | `ICommand<bool>` | `ICommand` |
| `UpdateAreaCommand` | `ICommand<bool>` | `ICommand` |
| `UpdateAreaOutlineCommand` | `ICommand<bool>` | `ICommand` |
| `UpdateDungeonCommand` | `ICommand<bool>` | `ICommand` |
| `UpdateFloorCommand` | `ICommand<bool>` | `ICommand` |
| `UpdatePlacedObjectCommand` | `ICommand<bool>` | `ICommand` |

Reviewer checks, after T4. The first two are measured on the pre-task tree and must hold as stated:

```
grep -rn 'ICommand<bool>' src/DungeonMaster.Application                      # 0 lines after; non-empty before
grep -rlE ':[[:space:]]*ICommand;' src/DungeonMaster.Application/Features | wc -l   # 10 after; 0 before
grep -rn 'Result<bool>' src/DungeonMaster.Application                        # 0 lines after
grep -rn 'Unit' --include=*.cs src/DungeonMaster.Application                 # 0 lines, before and after
```

The ten files the second command must list are the ten `*Command.cs` files under
`Features/Dungeons/Commands/{DeleteDungeon, RemoveArea, RemoveFloor, RemovePlacedObject,
ReorderFloor, UpdateArea, UpdateAreaOutline, UpdateDungeon, UpdateFloor, UpdatePlacedObject}/`.
Verified on the pre-task tree: `grep -rl 'ICommand<bool>' .../Commands/` returns exactly those ten,
and `grep -rl 'ICommand<Guid>' .../Commands/` returns exactly the other four.

**The `;`-anchored regex matters.** A bare `grep -rl 'ICommand'` also matches `ICommand<Guid>` and
`ICommandHandler`. Every command record in this repository is a positional record whose base list
terminates the declaration, e.g. `DeleteDungeonCommand.cs:13` reads `		: ICommand<bool>;` — so
`:\s*ICommand;` matches the non-generic form and nothing else. Measured on the pre-task tree: the
`ICommand;` form returns **0** files and the `ICommand<bool>;` form returns **10**.

---

## 5. The frozen handler shapes

### 5.1 A no-payload handler with a child lookup — the common case, ten of them

```csharp
public sealed class RemoveFloorCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<RemoveFloorCommand>
{
	/// <inheritdoc/>
	public Task<Result> HandleAsync(
		RemoveFloorCommand command,
		CancellationToken ct) =>
			writeRepository.MutateAsync(
				command.DungeonId,
				dungeon =>
					dungeon
						.RemoveFloor(command.FloorId)
						.ToResult($"Floor {command.FloorId} not found."),
				ct);
}
```

The handler is now its aggregate call and nothing else. No `async`, no `await` — the method returns
the helper's task directly.

### 5.2 `ReorderFloor` — the one that uses both messages

```csharp
			writeRepository.MutateAsync(
				command.DungeonId,
				dungeon =>
					dungeon
						.ReorderFloor(
							command.FloorId,
							command.NewNumber)
						.ToResult(
							$"Floor {command.FloorId} not found.",
							$"Floor number {command.NewNumber} is out of range for this dungeon."),
				ct);
```

### 5.3 A `Guid` handler that creates a child — `AddArea` and `AddPlacedObject`

```csharp
	public Task<Result<Guid>> HandleAsync(
		AddAreaCommand command,
		CancellationToken ct) =>
			writeRepository.MutateAsync(
				command.DungeonId,
				dungeon =>
				{
					var area = dungeon.AddArea(
						command.FloorId,
						command.Name,
						command.Description);

					return area is null
						? Result<Guid>.Failure(
							$"Floor {command.FloorId} not found.",
							ErrorCategory.NotFound)
						: Result<Guid>.Success(area.Id);
				},
				ct);
```

`AddPlacedObjectCommandHandler` takes the identical shape. **That message and that category are
MASTER-031**: the record demands `ErrorCategory.NotFound` with `$"Floor {command.FloorId} not
found."`, replacing today's `Result<Guid>.Failure("Failed to add placed object.",
ErrorCategory.Validation)`. **The class's XML summary must be corrected too** — it currently
documents the behaviour as "a validation failure if the floor is not found", and CORE-014 names the
summary as part of the defect.

### 5.4 The two handlers outside the skeleton

`CreateDungeonCommandHandler` is unchanged apart from formatting.

`DeleteDungeonCommandHandler` becomes:

```csharp
	public async Task<Result> HandleAsync(
		DeleteDungeonCommand command,
		CancellationToken ct)
	{
		var deleted = await writeRepository.DeleteAsync(
			command.Id,
			ct);

		return deleted
			? Result.Success()
			: Result.Failure(
				$"Dungeon {command.Id} not found.",
				ErrorCategory.NotFound);
	}
```

This is the second half of CORE-006's complaint — "`DeleteDungeonCommandHandler` returns success
even when `DeleteAsync` deleted nothing" — and it depends on T3 having changed `DeleteAsync` to
report. See `contracts/persistence-write-contract.md` §3.

---

## 6. The App-side ripple T4 owns

Ten commands changing from `ICommand<bool>` to the non-generic `ICommand` breaks every
`dispatcher.SendAsync<bool>(...)` call site. The measured shape of the call sites, on the pre-task
tree:

```
grep -rno 'SendAsync<[A-Za-z]*>' src/DungeonMaster.App | sort | uniq -c
```

**16 dispatch call sites in total: 11 `SendAsync<bool>` and 5 `SendAsync<Guid>`.** By file:
`DungeonDetailViewModel.cs` has 14 (10 `bool`, 4 `Guid`), `DungeonListViewModel.cs` has 1 (`bool`),
`CreateDungeonViewModel.cs` has 1 (`Guid`). `MapTool.cs` has none.

**The mechanical change is `SendAsync<bool>(x)` to `SendAsync(x)` — the type argument is dropped,
not replaced.** The non-generic overload `IDispatcher.SendAsync(ICommand, CancellationToken)`
(`contracts/dispatch-and-pipeline.md` §1) is the only applicable one for a command that implements
only `ICommand`, so overload resolution is unambiguous and no cast is needed. The five
`SendAsync<Guid>` sites are untouched. No `using` is added — `Unit` no longer exists, so unlike the
superseded draft there is nothing new to import.

Counting command a reviewer re-runs after T4:

```
grep -rc 'SendAsync<bool>' src/DungeonMaster.App/ViewModels/*.cs   # every file 0
grep -rc 'SendAsync<Guid>' src/DungeonMaster.App/ViewModels/*.cs   # DungeonDetail 4, CreateDungeon 1
```

T4 owns `src/DungeonMaster.App/ViewModels/*.cs` for **that one mechanical change and no other**. T4
must not add an `ErrorMessage` property, must not branch on `IsSuccess`, and must not touch any
XAML — that is T8, which runs after it and owns the same files. A T4 diff that changes App
behaviour is a blocking finding.

`src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs` contains **no** dispatcher
call and is not touched. Verified:

```
grep -rn 'SendAsync\|QueryAsync' --include=*.cs src/DungeonMaster.App | grep -v ViewModels/   # no output
```

---

## 7. Tests T4 owns

All 14 files under `src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/`, plus a new
`src/DungeonMaster.Tests/Application/Common/Handlers/` directory.

Existing tests that must be **rewritten, not deleted**:

- `DeleteDungeonCommandHandlerTests.HandleAsync_WithValidId_ReturnsSuccess` asserts
  `result.Value.Should().BeTrue()`. There is no `Value` any more — the handler returns the
  non-generic `Result`, so the assertion does not compile. Replace it with
  `result.IsSuccess.Should().BeTrue()` and add the new
  `HandleAsync_WithUnknownId_ReturnsNotFound` case.

New tests that must exist and must **fail before this task**:

| Test | File | Proves |
|---|---|---|
| `HandleAsync_WithUnknownFloorId_ReturnsNotFoundAndDoesNotSave` | `RemoveFloorCommandHandlerTests` | MASTER-021. `result.ErrorCategory.Should().Be(ErrorCategory.NotFound)` **and** `await _writeRepository.DidNotReceive().UpdateAsync(Arg.Any<Dungeon>(), Arg.Any<CancellationToken>())` |
| the same pair | `RemoveAreaCommandHandlerTests`, `UpdateAreaCommandHandlerTests`, `UpdateAreaOutlineCommandHandlerTests`, `UpdateFloorCommandHandlerTests`, `RemovePlacedObjectCommandHandlerTests`, `UpdatePlacedObjectCommandHandlerTests` | one per silently-no-opping mutator |
| `HandleAsync_WithOutOfRangeNumber_ReturnsValidationFailure` | `ReorderFloorCommandHandlerTests` | the `OutOfRange` distinction survives the mapping |
| `HandleAsync_WithUnknownFloorId_ReturnsNotFoundNamingTheFloor` | `AddPlacedObjectCommandHandlerTests` | MASTER-031: `ErrorCategory.NotFound`, and `result.Error.Should().Contain(command.FloorId.ToString())` |
| `HandleAsync_WithUnknownId_ReturnsNotFound` | `DeleteDungeonCommandHandlerTests` | CORE-006's delete half |
| `MutateAsync_WithUnknownDungeon_ReturnsNotFoundAndDoesNotSave` | `DungeonMutationExtensionsTests` | the helper itself |
| `MutateAsync_WithFailingMutation_DoesNotSave` | `DungeonMutationExtensionsTests` | short-circuit |
| `MutateAsync_WithSucceedingMutation_SavesOnce` | `DungeonMutationExtensionsTests` | |
| `ToResult_WithEachOutcome_MapsToTheContractedCategory` | `MutationOutcomeExtensionsTests` | one `[Theory]` over all three members |

**Both `MutateAsync` overloads are tested.** The three `DungeonMutationExtensionsTests` rows above
exist twice: once bound to the non-generic overload
(`MutateAsync_WithUnknownDungeon_ReturnsNotFoundAndDoesNotSave`) and once to the generic one
(`MutateAsync_WithValue_WithUnknownDungeon_ReturnsNotFoundAndDoesNotSave`, and the matching
`_WithValue_` names for the other two). Six tests, not three. Two independently written bodies is
exactly the risk the duplication in §2 creates, and testing only one of them would leave half the
helper unproven.

The `DidNotReceive().UpdateAsync(...)` assertion is the criterion that distinguishes "the handler
now returns a failure" from "the handler now returns a failure *and stopped writing*". Both halves
are required.
