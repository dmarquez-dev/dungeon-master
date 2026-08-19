# Contract: the persistence ports, the DbContext lifetime, and the unit of work

Frozen at Gate 2. This is the boundary between **T1** (which authors everything in this file) and
**T2** (which configures the EF model behind it) and **T3** (which must not be surprised by it).

Roots, spelled in full because implementers hold two of them:

- **Worktree root** —
  `C:\Users\dmarq\Development\Repos\dungeon-master\.claude-worktrees\2026-08-18-settle-the-data-access-lifetime\`
  Every `src/...` path below is relative to this root.
- **Session directory** —
  `C:\Users\dmarq\Development\Repos\dungeon-master\.dir-sdlc\sessions\2026-08-18-settle-the-data-access-lifetime\`
  Every `contracts/...` path below is relative to this root.

## 0. What was frozen before this contract, and is not open

Three user decisions are recorded in
`C:\Users\dmarq\Development\Repos\dungeon-master\.dir-sdlc\sessions\2026-08-18-settle-the-data-access-lifetime\state.json`
under `escalations`. This contract is their implementation, not a re-litigation of them.

| Id | Decision |
|---|---|
| E1 / `MASTER-023` | `AppDbContext` comes from `IDbContextFactory<AppDbContext>`, **one context per unit of work — explicitly not one per repository call.** |
| E2 / `MASTER-092` | An **explicit commit boundary** on the write side (INFRA-014's first option), coordinated with `MASTER-048`'s relocation of the ports. |
| E3 / `MASTER-029` + `MASTER-103` | Domain timestamps become `DateTime` (UTC). Specified in `contracts/persistence-contract-and-timestamps.md`, not here. |

## 1. The unit of work is the dispatched command

**The unit of work is one dispatched command.** One `AppDbContext` is created when a command handler
begins, is used for the load, the mutation and the commit, and is disposed when the handler returns.
Nothing outside a command handler holds a context across an `await` that returns to the caller.

Three consequences, and each one is why a simpler shape was rejected:

1. **`LoadAsync` and the commit share a context.** This is the whole point of E1. Per-call contexts
   would put `LoadAsync` and the save on different contexts, leaving the aggregate detached at save
   time, which sends `DungeonWriteRepository`'s `Update(dungeon)` branch — added by `MASTER-016` in
   Session 2, at `src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonWriteRepository.cs:40-43`
   on the base commit — down the path that marks the entire aggregate graph `Modified`, so every save
   rewrites every floor, area and object.
2. **A query is its own unit of work.** `IDungeonReadRepository`'s two methods are single, terminal,
   `AsNoTracking` projections. Each creates and disposes its own context. There is no read-side
   commit and no read-side unit of work type.
3. **The change tracker never outlives a command.** That is what closes INFRA-018 / UI-001 /
   XCUT-001, all three of which describe one process-lifetime context accumulating every entity ever
   loaded.

**Not chosen, and not to be substituted:** a dispatcher-level `IServiceScope` per command (INFRA-018's
named alternative). It reaches three layers rather than one and puts persistence knowledge in the
dispatcher that Session 2 has just frozen.

**Pooling is not used.** `AddDbContextFactory`, not `AddPooledDbContextFactory`. `AppDbContext` carries
no custom mutable state today so pooling would be technically legal, but the measured benefit
Microsoft publishes is a server-throughput number and this is a single-user desktop app against a
local SQLite file, where context construction happens a few times per second at most. Not pooling
also keeps the "no custom mutable state" precondition from becoming an invisible tripwire for the
next person who adds a field to `AppDbContext`. This is reversible in one line if it ever matters.

## 2. The ports, after relocation

Both existing port files move — **file and namespace together**, per
`contracts/coding-standards.md` §7:

| From | To |
|---|---|
| `src/DungeonMaster.Application/Features/Dungeons/IDungeonReadRepository.cs` | `src/DungeonMaster.Application/Common/Interfaces/IDungeonReadRepository.cs` |
| `src/DungeonMaster.Application/Features/Dungeons/IDungeonWriteRepository.cs` | `src/DungeonMaster.Application/Common/Interfaces/IDungeonWriteRepository.cs` |

Namespace `DungeonMaster.Application.Features.Dungeons` becomes
`DungeonMaster.Application.Common.Interfaces`. Two new files join them in that directory:
`IUnitOfWork.cs` and `IUnitOfWorkFactory.cs`.

`Application/Common/Interfaces/` **does not exist on the base commit.** This session creates it.

### 2.1 The ripple, measured

```
grep -rln "^using DungeonMaster.Application.Features.Dungeons;" --include=*.cs src/
```

returns **39 files** on the base commit `9903f405dc919f782ba9048cae9623bcf1d79297`, run in the
worktree. Every one of them needs `using DungeonMaster.Application.Common.Interfaces;` instead of, or
in addition to, the line it has. The count is the measured ripple of `MASTER-048` and T1 is expected
to touch all 39; it is not evidence of scope creep.

Note that the two DTO files — `DungeonSummaryDto.cs` and `DungeonDetailDto.cs` — are **not** among the
39. They carry no such `using`. They change for a different reason, specified in
`contracts/persistence-contract-and-timestamps.md`.

## 3. The frozen port surface

This is the exact shape. Signatures are binding; XML documentation is required on every member per
`contracts/coding-standards.md` §3 and is elided here for brevity.

```csharp
namespace DungeonMaster.Application.Common.Interfaces;

/// One persistence session with an explicit commit. Created per dispatched command,
/// disposed when that command's handler returns.
public interface IUnitOfWork
	: IAsyncDisposable
{
	IDungeonWriteRepository Dungeons { get; }

	Task<int> CommitAsync(CancellationToken ct);
}

public interface IUnitOfWorkFactory
{
	Task<IUnitOfWork> CreateAsync(CancellationToken ct);
}

public interface IDungeonWriteRepository
{
	Task<Dungeon?> LoadAsync(
		Guid id,
		CancellationToken ct);

	void Add(Dungeon dungeon);

	void Update(Dungeon dungeon);

	Task<bool> RemoveAsync(
		Guid id,
		CancellationToken ct);
}

public interface IDungeonReadRepository
{
	Task<IReadOnlyList<DungeonSummaryDto>> GetSummariesAsync(CancellationToken ct);

	Task<DungeonDetailDto?> GetDetailAsync(
		Guid id,
		CancellationToken ct);
}
```

### 3.1 What changed on `IDungeonWriteRepository`, and why each rename is deliberate

| Base commit | Frozen | Reason |
|---|---|---|
| `Task AddAsync(Dungeon, CancellationToken)` | `void Add(Dungeon)` | With the save moved to `CommitAsync`, nothing asynchronous is left. This is also how `MASTER-033` / INFRA-013 is closed: the underlying call becomes `dbContext.Dungeons.Add(dungeon)`, which the cited `DbSet<T>.AddAsync` remarks prescribe for every model without a database-backed value generator — and all four key properties here are `ValueGeneratedNever`. |
| `Task UpdateAsync(Dungeon, CancellationToken)` | `void Update(Dungeon)` | Same reason. The detached guard survives verbatim (§4.2). |
| `Task<bool> DeleteAsync(Guid, CancellationToken)` | `Task<bool> RemoveAsync(Guid, CancellationToken)` | Stays asynchronous because it still does a `FindAsync`. Renamed because it no longer deletes — it marks for removal, and the deletion happens at `CommitAsync`. `Remove` matches `DbSet<T>.Remove`. Keeping the name `DeleteAsync` for a method that no longer deletes is the single most likely source of a later misread. |

`Task<bool>` on `RemoveAsync` keeps `MASTER-088`'s Session 2 signal and its meaning narrows precisely:
**`true` means an aggregate with that id was found and marked for removal; `false` means no aggregate
with that id exists.** Neither value says anything about whether a commit happened.

`IDungeonReadRepository` is **unchanged in shape.** Only its namespace moves. Its implementation
changes (§4.3).

### 3.2 Error cases — the part contracts most often omit

| Situation | Behaviour |
|---|---|
| `LoadAsync` finds no aggregate | Returns `null`. Never throws. Unchanged from the base commit. |
| `RemoveAsync` finds no aggregate | Returns `false`. Never throws. Nothing is marked. |
| `Add` called twice with the same instance | Undefined; no guard is added. Out of scope for this session. |
| `CommitAsync` with nothing tracked as changed | Returns `0`. Does not throw. This is the observable that `MASTER-015`'s test in T2 turns on. |
| `CommitAsync` when the store rejects the write | The provider exception propagates unwrapped out of `CommitAsync` and out of the handler. **No task in this session adds a `try`/`catch` around a commit.** Turning store failures into `Result` values is `MASTER-006`'s entry and is not in this session; inventing it here would pre-empt a later session and hide the failure the audit wants visible. |
| `CommitAsync` called twice on one unit of work | Legal. The second call sees nothing changed and returns `0`. No guard is added. |
| The unit of work is disposed without `CommitAsync` | **Nothing is written.** This is the transaction boundary: a mutation that fails its domain guard is discarded by disposal, not by a rollback call. `IAsyncDisposable` disposes the context and no more. |
| `CreateAsync` cancelled | `OperationCanceledException` propagates from `IDbContextFactory<AppDbContext>.CreateDbContextAsync`, per that method's documented exceptions. |

## 4. The Infrastructure implementations

### 4.1 `UnitOfWork` and `UnitOfWorkFactory`

New file `src/DungeonMaster.Infrastructure/Persistence/UnitOfWork.cs` holds `UnitOfWork`; new file
`src/DungeonMaster.Infrastructure/Persistence/UnitOfWorkFactory.cs` holds `UnitOfWorkFactory`. Two
types, two files, per `contracts/coding-standards.md` §7.

- `UnitOfWorkFactory` takes `IDbContextFactory<AppDbContext>` and nothing else. `CreateAsync` awaits
  `CreateDbContextAsync(ct)` and hands the context to a new `UnitOfWork`.
- `UnitOfWork` owns the context. Its `Dungeons` property returns a `DungeonWriteRepository` built over
  that context. `CommitAsync(ct)` returns `dbContext.SaveChangesAsync(ct)`. `DisposeAsync` disposes
  the context.
- **`UnitOfWork` owns the context's lifetime and nothing else owns it.** `DungeonWriteRepository` must
  not dispose the context it was handed.

### 4.2 `DungeonWriteRepository`

Continues to take an `AppDbContext` in its primary constructor — it is now constructed by `UnitOfWork`
rather than by the DI container, and is **no longer registered in the container at all.**

Every `SaveChangesAsync` call leaves this file. On the base commit there are three, at
`src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonWriteRepository.cs:32`, `:45` and
`:63`; `grep -c "SaveChangesAsync" src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonWriteRepository.cs`
returns **3** on the base commit and must return **0** after T1.

`Update` keeps its detached guard byte-for-byte:

```csharp
if (dbContext.Entry(dungeon).State is EntityState.Detached)
{
	dbContext.Update(dungeon);
}
```

`MASTER-016` landed this in Session 2 and this session does not touch it. Under the frozen lifetime the
branch is **not** taken on any path a handler drives, because the load and the update share one
context — which is exactly the property E1 was chosen to buy. The guard stays as a backstop for a
future caller that constructs an aggregate in memory.

`LoadAsync` gains `.AsSplitQuery()` — `MASTER-004` / INFRA-011.

### 4.3 `DungeonReadRepository`

Takes `IDbContextFactory<AppDbContext>` in its primary constructor instead of an `AppDbContext`. Each
of its two methods creates a context with `await using var dbContext = await
dbContextFactory.CreateDbContextAsync(ct);`, runs its query, and disposes.

`GetDetailAsync` gains `.AsSplitQuery()` — `MASTER-049` / INFRA-010. `GetSummariesAsync` does **not**:
it projects only scalars and has no collection navigation to split. Adding it there is a review
finding, not a bonus.

**Explicit per query, not global.** `UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)` on
the options — INFRA-011's named alternative — is deliberately not used. `MASTER-004` and `MASTER-049`
are separate audit entries needing separate, greppable evidence, and a global switch would silently
change the summary query too.

## 5. The composition root

`src/DungeonMaster.Infrastructure/DependencyInjection.cs`, inside `AddInfrastructureServices`:

```csharp
services.AddDbContextFactory<AppDbContext>(options =>
	options.UseSqlite(connectionString));

services.AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();
services.AddSingleton<IDungeonReadRepository, DungeonReadRepository>();
```

`AddSingleton` is correct here and is not a shortcut: both types hold only
`IDbContextFactory<AppDbContext>`, which `AddDbContextFactory` registers as a singleton by default
(the `lifetime` parameter's default is `ServiceLifetime.Singleton`). Neither holds a context, so
neither carries the thread-affinity that made the old scoped registration wrong.

`IDungeonWriteRepository` is **not registered.** `UnitOfWork` constructs it. A container registration
for it would re-open the very hole this contract closes, by letting a handler obtain a repository with
no unit of work around it.

### 5.1 One scoped registration remains, and it is not ours

`AddDbContextFactory` **also registers `AppDbContext` itself as a scoped service** — the EF Core 10 API
remarks state this verbatim: "For convenience, this method also registers the context type itself as a
scoped service." We do not control that descriptor and it must not be treated as a defect.

The invariant that replaces "there are no scoped registrations" is therefore narrower and checkable:

- `grep -c "AddScoped" src/DungeonMaster.Infrastructure/DependencyInjection.cs` returns **0** (it
  returns 2 on the base commit, at lines 33 and 34).
- **Nothing in `src/` resolves `AppDbContext` from an `IServiceProvider`.** After T1,
  `grep -rn "GetRequiredService<AppDbContext>\|GetService<AppDbContext>" --include=*.cs src/` returns
  nothing. It returns one hit on the base commit, at
  `src/DungeonMaster.Infrastructure/DependencyInjection.cs:48`.
- `grep -rn "CreateScope\|CreateAsyncScope" --include=*.cs src/DungeonMaster.Infrastructure src/DungeonMaster.App src/DungeonMaster.Application`
  returns nothing after T1. On the base commit it returns exactly one production hit,
  `src/DungeonMaster.Infrastructure/DependencyInjection.cs:47`. Hits inside
  `src/DungeonMaster.Tests/` are the test suite building its own containers and are out of scope —
  there are 10 of them across `ApplicationServiceRegistrationTests.cs` and
  `CommandValidatorCoverageTests.cs`, and they stay.

### 5.2 `MigrateDatabaseAsync`

`MigrateDatabase(this IServiceProvider)` becomes:

```csharp
public static async Task MigrateDatabaseAsync(
	this IServiceProvider services,
	CancellationToken ct)
{
	var dbContextFactory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
	await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

	await dbContext.Database.MigrateAsync(ct);
}
```

This closes `MASTER-011` / INFRA-019 on both counts it raises — the async EF API
(`dotnet-guidelines.md` §2.1) and a cancellation token (`backend-conventions.md` §6) — and it is what
removes the last production `CreateScope`.

## 6. `MauiProgram.CreateMauiApp`

`MASTER-012` / UI-002. Two calls are unguarded on the base commit:
`Directory.CreateDirectory(dbFolder)` at `src/DungeonMaster.App/MauiProgram.cs:118` and
`app.Services.MigrateDatabase()` at `:139`. Both move inside one `try`/`catch` whose `catch` writes
the exception to `System.Diagnostics.Debug.WriteLine` naming the startup step that failed, and lets
`CreateMauiApp` return normally so the shell starts.

The migration call becomes:

```csharp
using var startupCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

app.Services
	.MigrateDatabaseAsync(startupCts.Token)
	.GetAwaiter()
	.GetResult();
```

Three things about that, stated here so review does not re-open them:

- **The block is deliberate.** The app has no schema until the migration completes, and
  `CreateMauiApp` is not `async`. `GetAwaiter().GetResult()` here cannot deadlock: it runs before the
  MAUI application loop starts, so there is no `SynchronizationContext` installed to deadlock against.
- **The timeout is the point.** INFRA-019 names a concrete hang: on SQLite the EF 9+ migration lock
  uses an `__EFMigrationsLock` table whose row is not cleaned up if the process is killed
  mid-migration, after which "each attempt will wait indefinitely for the lock to be released". Thirty
  seconds converts an indefinite silent hang into a caught, logged failure.
- **UI-002's other half is deliberately not done.** Moving the migration off the construction path so
  first paint does not wait on it needs a loading state and a user-visible error surface, which is
  view work this session does not do. `MASTER-012` is closed as *startup no longer dies silently and
  no longer waits indefinitely*; the cold-start half is carried to the backlog, and the T1 report says
  so rather than letting a reviewer discover it.

The catch must **not** be bare and must not be `catch (Exception)` without a filter — `MASTER-005`
established that rule in Session 2 and it binds here. Name the types that can actually arise from
directory creation and migration.

## 7. What handlers look like afterwards

### 7.1 `DungeonMutationExtensions`

`src/DungeonMaster.Application/Common/Handlers/DungeonMutationExtensions.cs` is where the unit of work
is opened for twelve of the fourteen commands. Both overloads change their extended type from
`IDungeonWriteRepository` to `IUnitOfWorkFactory` and gain the create/commit bracket. The non-generic
overload, in full:

```csharp
public static async Task<Result> MutateAsync(
	this IUnitOfWorkFactory unitOfWorkFactory,
	Guid dungeonId,
	Func<Dungeon, Result> mutate,
	CancellationToken ct)
{
	await using var unitOfWork = await unitOfWorkFactory.CreateAsync(ct);

	var dungeon = await unitOfWork.Dungeons.LoadAsync(
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

	unitOfWork.Dungeons.Update(dungeon);

	await unitOfWork.CommitAsync(ct);

	return result;
}
```

The `Result<TResult>` overload is the same body with `Result<TResult>` substituted. Session 2 froze
these two as **deliberately duplicated** rather than factored together
(`.dir-sdlc/sessions/2026-08-18-make-failure-observable/contracts/handler-skeleton.md` §2); that
decision stands and is not to be re-litigated here.

Note both early returns: **neither commits**, and the `await using` discards the context. That is the
explicit boundary `MASTER-092` asks for, and it is stronger than the base commit's behaviour, where a
failed mutation had already had nothing saved only because `UpdateAsync` was never reached.

### 7.2 The twelve `MutateAsync` handlers

Mechanical. Each one's primary-constructor parameter changes from
`IDungeonWriteRepository writeRepository` to `IUnitOfWorkFactory unitOfWorkFactory`, and the receiver
of the `MutateAsync` call changes to match. **No handler body gains a statement.**

### 7.3 `CreateDungeonCommandHandler`

```csharp
await using var unitOfWork = await unitOfWorkFactory.CreateAsync(ct);

var dungeon = new Dungeon(
	command.Name,
	command.Description);

unitOfWork.Dungeons.Add(dungeon);

await unitOfWork.CommitAsync(ct);

return Result<Guid>.Success(dungeon.Id);
```

### 7.4 `DeleteDungeonCommandHandler`

```csharp
await using var unitOfWork = await unitOfWorkFactory.CreateAsync(ct);

var removed = await unitOfWork.Dungeons.RemoveAsync(
	command.Id,
	ct);

if (!removed)
{
	return Result.Failure(
		$"Dungeon {command.Id} not found.",
		ErrorCategory.NotFound);
}

await unitOfWork.CommitAsync(ct);

return Result.Success();
```

The not-found path returns **before** committing. The message string is unchanged from the base
commit, so `FailureIsSurfacedTests` and the handler's own tests keep asserting the same text.

### 7.5 The two query handlers

Unchanged apart from their `using` line. They call `IDungeonReadRepository` exactly as before.

## 8. What the tests look like afterwards

### 8.1 Handler tests substitute the factory, not the repository

```
grep -rl "Substitute.For<IDungeonWriteRepository>" --include=*.cs src/DungeonMaster.Tests
```

returns **17 files** on the base commit: the fourteen under
`src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/`, plus
`src/DungeonMaster.Tests/Application/Common/Handlers/DungeonMutationExtensionsTests.cs`,
`src/DungeonMaster.Tests/Application/Common/Dispatch/ApplicationServiceRegistrationTests.cs` and
`src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/CommandValidatorCoverageTests.cs`.
All 17 are T1's.

Each now needs a substituted `IUnitOfWorkFactory` whose `CreateAsync` returns a substituted
`IUnitOfWork` whose `Dungeons` returns the substituted `IDungeonWriteRepository`. Write that wiring
**once**, as one shared internal test helper, and name it in the T1 report. Repeating a three-line
substitute chain in 17 files is a review finding.

Assertions that read `writeRepository.DidNotReceive().UpdateAsync(...)` become
`writeRepository.DidNotReceive().Update(...)`, and the ones that prove no write happened gain a
second, stronger assertion: `await unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>())`.
That second assertion is the one that actually proves `MASTER-092`'s boundary holds, because under the
frozen design `Update` is a tracking no-op and only the commit writes.

### 8.2 Infrastructure tests wrap the existing fixture

`contracts/coding-standards.md` §8 forbids a second fixture. The four files under
`src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/` keep their
`SqliteConnection("Data Source=:memory:")` and their `DbContextOptions<AppDbContext>` field, and gain
a test-local factory over the options they already build:

```csharp
private sealed class TestDbContextFactory(
	DbContextOptions<AppDbContext> options)
		: IDbContextFactory<AppDbContext>
{
	public AppDbContext CreateDbContext() => new(options);
}
```

`CreateDbContext()` is the only member that must be implemented: `IDbContextFactory<TContext>` declares
`CreateDbContextAsync(CancellationToken = default)` as a `virtual` interface member with a default
implementation, so it does not have to be written. Verified against the EF Core 10 API reference for
`Microsoft.EntityFrameworkCore.IDbContextFactory<TContext>`.

Each file holds one open `SqliteConnection` for the class lifetime, so every context the test factory
hands out shares the same in-memory database. That is what makes the multi-context assertions in these
tests keep working.

## 9. Ownership, and the one hand-off that is not obvious

| Belongs to | What |
|---|---|
| **T1** | Everything in this contract. |
| **T2** | The EF model behind it — configurations, `AppDbContext`'s `DbSet` surface, the design-time factory, and **the session's single migration**. |
| **T3** | Nothing here. T3 touches `DungeonDetailViewModel.cs` only and is independent of this contract. |

**The hand-off:** T1 changes CLR types that the EF model is generated from (the timestamp change in
`contracts/persistence-contract-and-timestamps.md`) but T1 adds **no migration and does not touch
`src/DungeonMaster.Infrastructure/Migrations/`**. Between T1 and T2 the model snapshot is deliberately
out of date with the model. T2 owns `Migrations/**` and authors **one** migration covering every model
change T1 and T2 together make. This is stated here, in T1's acceptance criteria and in T2's, so that
no reviewer reads T1's missing migration as an omission.
