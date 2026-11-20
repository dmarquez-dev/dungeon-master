# Contract: the dungeon write-repository contract

Frozen at Gate 2. **Authored by T3. Consumed by T4.**

Closes: **MASTER-016**, and supplies the return value **MASTER-088**'s `DeleteDungeon` half needs.

---

## 1. Session boundary — read this before touching anything

Session 3 ("Settle the data-access lifetime and the persistence contract") owns the `DbContext` and
repository **lifetime** and the rest of the persistence contract: `MASTER-023` (one lifetime across
both composition roots), `MASTER-015` (spurious writes), `MASTER-092` (transaction boundary).

**T3 changes two method bodies and one interface. It changes no DI registration, no lifetime, no
`DbContext` configuration, and no composition root.** Specifically it does not touch:

- `src/DungeonMaster.Infrastructure/DependencyInjection.cs`
- `src/DungeonMaster.App/MauiProgram.cs`
- `src/DungeonMaster.Infrastructure/Persistence/AppDbContext.cs`
- anything under `Persistence/Configurations/` or `Migrations/`

A diff touching any of those is a blocking finding, not a bonus.

---

## 2. `UpdateAsync` — MASTER-016

Today:

```csharp
	public async Task UpdateAsync(Dungeon dungeon, CancellationToken ct)
	{
		if (dbContext.Entry(dungeon).State is EntityState.Detached)
		{
			dbContext.Attach(dungeon);
		}

		await dbContext.SaveChangesAsync(ct);
	}
```

`Attach` walks the graph and marks every entity with a key value set as `Unchanged`
(https://learn.microsoft.com/en-us/ef/core/saving/disconnected-entities). All four entity types use
`ValueGeneratedNever` keys assigned in the constructor, so **every** node is keyed and **every**
node is marked `Unchanged`. `SaveChangesAsync` then writes nothing, returns without error, and the
handler's `Success` flows to the UI.

Today the branch is never taken, because every handler loads through `LoadAsync` on the same
context first. That is precisely why this survived review — and precisely why it fires the moment
Session 3 changes the context lifetime.

**Frozen replacement:**

```csharp
	/// <inheritdoc/>
	public async Task UpdateAsync(
		Dungeon dungeon,
		CancellationToken ct)
	{
		if (dbContext.Entry(dungeon).State is EntityState.Detached)
		{
			dbContext.Update(dungeon);
		}

		await dbContext.SaveChangesAsync(ct);
	}
```

`Update` marks the graph `Modified` rather than `Unchanged`, which is the first of the two options
INFRA-012's suggested direction names. The second option — deleting the branch and throwing on
`Detached` — is **rejected here** because it converts a silent data-loss bug into a crash on a path
Session 3 is about to make reachable, and because `dotnet-guidelines.md` §6.2 does not class this
as an unexpected condition. The choice is recorded so a reviewer does not re-open it.

The tracked case is untouched: an entity already tracked as `Modified` or `Unchanged` skips the
branch entirely and `SaveChangesAsync` behaves exactly as it does today. That is what protects the
292-test baseline.

---

## 3. `DeleteAsync` — the reporting half of MASTER-088

`IDungeonWriteRepository.DeleteAsync` returns `Task`, so `DeleteDungeonCommandHandler` cannot tell
a real delete from a no-op and returns `Success(true)` either way. CORE-006 names this explicitly:
"`DeleteDungeonCommandHandler` returns success even when `DeleteAsync` deleted nothing."

**Frozen interface change** in
`src/DungeonMaster.Application/Features/Dungeons/IDungeonWriteRepository.cs`:

```csharp
	/// <summary>
	///		Deletes the dungeon with the given identifier.
	/// </summary>
	/// <returns>
	///		<see langword="true"/> when a dungeon was found and deleted; otherwise
	///		<see langword="false"/>.
	/// </returns>
	Task<bool> DeleteAsync(
		Guid id,
		CancellationToken ct);
```

**Frozen implementation:**

```csharp
	/// <inheritdoc/>
	public async Task<bool> DeleteAsync(
		Guid id,
		CancellationToken ct)
	{
		var dungeon = await dbContext.Dungeons.FindAsync(
			[id],
			ct);

		if (dungeon is null)
		{
			return false;
		}

		dbContext.Dungeons.Remove(dungeon);
		await dbContext.SaveChangesAsync(ct);

		return true;
	}
}
```

`LoadAsync`, `AddAsync` and `UpdateAsync`'s signatures are otherwise unchanged. No other member of
`IDungeonWriteRepository` moves, and `IDungeonReadRepository` is not touched — moving these ports to
`Common/Interfaces` is `MASTER-048`, which belongs to Session 3.

### 3.1 The one ripple T3 owns

`await writeRepository.DeleteAsync(...)` compiles unchanged when the return value is discarded, so
`DeleteDungeonCommandHandler.cs` does **not** break — T4 rewrites it later per
`contracts/handler-skeleton.md` §5.4.

`DeleteDungeonCommandHandlerTests.cs` **does** break: it stubs
`.DeleteAsync(...).Returns(Task.CompletedTask)` at three call sites (lines 35, 53, 69), which no
longer type-checks against `Task<bool>`. T3 owns that one test file for the minimum change that
restores the build — `.Returns(true)` — and nothing more. T4 owns it afterwards for the substantive
rewrite. That is a sequenced overlap, not a conflict.

---

## 4. Tests T3 owns

`src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs` —
an existing file with the SQLite in-memory fixture already in place. Extend it; do not create a
second fixture.

| Test | Proves |
|---|---|
| `UpdateAsync_WithDetachedAggregate_PersistsTheChange` | **MASTER-016, and it must fail before the change.** Create and save a dungeon through one context. Open a **second** context, `LoadAsync` the dungeon, mutate it, and dispose that context **without** saving so the graph detaches — or construct the aggregate in memory with the same ids. Open a **third** context, call `UpdateAsync`, then verify through a **fourth** that the change landed. Under `Attach` the assertion fails; under `Update` it passes. The implementer states in the report which of the two detachment routes was used and quotes the `Entry(dungeon).State` observed immediately before the `UpdateAsync` call — that value must be `Detached`, or the test is not exercising the branch at all. |
| `UpdateAsync_WithTrackedAggregate_StillPersistsTheChange` | the existing behaviour did not regress. The existing `UpdateAsync_PersistsDomainMutations` covers this; keep it and add the explicit name only if it is not already equivalent. |
| `DeleteAsync_WithExistingDungeon_ReturnsTrue` | |
| `DeleteAsync_WithUnknownId_ReturnsFalseAndSavesNothing` | the reporting half of MASTER-088 |

The `Entry(dungeon).State is Detached` precondition is the part reviewers most often let slide. A
test that passes both before and after the change proves nothing; state the observed state.
