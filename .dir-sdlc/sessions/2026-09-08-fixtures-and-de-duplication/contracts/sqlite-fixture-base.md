# Contract — the shared SQLite in-memory fixture base

Producer: **T1**. Consumers: **T1** (the seven leaf classes it owns) and, transitively, every future
Infrastructure persistence test class.

This contract has one implementing task. It is frozen anyway, because it records a decision the
**user** took at the re-scope gate over an alternative the architect is not permitted to revisit, and
because it fixes the surface that ten existing call sites depend on. An implementer who reshapes the
base is contradicting the user, not the architect.

## 1. The decision this contract records

`MASTER-065` is settled as an **abstract base class**, chosen by the user over
`IClassFixture<SqliteFixture>`, because the base-class route preserves the per-test database
isolation the layer verdict wants and because `IClassFixture` would make `MASTER-127` a no-op for all
seven files.

Consequently, and **not open to re-decision by the implementer**:

- The base is a **new** type. **Promoting one of the seven existing classes to be the base is
  prohibited** — sealing it afterwards would contradict `MASTER-127` directly.
- The base is **not sealed**. The seven leaves are **sealed**.
- The base must keep the `DbContextOptions<AppDbContext>` reachable from derived classes, or the ten
  `TestDbContextFactory` construction sites break.

## 2. Location and identity

```
src/DungeonMaster.Tests/Infrastructure/Persistence/SqliteInMemoryTestBase.cs
namespace DungeonMaster.Tests.Infrastructure.Persistence;
public abstract class SqliteInMemoryTestBase : IDisposable
```

The namespace is deliberate: `...Persistence.Configurations` and `...Persistence.Repositories` are
both nested inside `...Persistence`, so neither leaf directory needs a `using` for the base.

`TestDbContextFactory` is `internal sealed` in `...Persistence.Repositories` and stays there
unmoved; the base file carries `using DungeonMaster.Tests.Infrastructure.Persistence.Repositories;`.

## 3. The surface — frozen

```csharp
protected DbContextOptions<AppDbContext> Options { get; }
protected AppDbContext CreateContext();
protected IDbContextFactory<AppDbContext> CreateDbContextFactory();
protected IUnitOfWorkFactory CreateFactory();
public void Dispose();
protected virtual void Dispose(bool disposing);
```

- `CreateContext()` and `CreateFactory()` are provided **unconditionally**. Measured across the seven
  classes (`grep -n "CreateContext\|CreateFactory" <each file>`): 5 of 7 declare `CreateContext`,
  6 of 7 declare `CreateFactory`, 4 of 7 declare both. A base that provides only the intersection
  breaks leaves; a base that provides only what each leaf had is not a base.
- `CreateDbContextFactory()` exists because `DungeonReadRepositoryTests` needs an
  `IDbContextFactory<AppDbContext>` and not an `IUnitOfWorkFactory` — it constructs
  `new DungeonReadRepository(new TestDbContextFactory(_options))` at three sites.
- `Options` is the replacement for the seven copies of `private readonly DbContextOptions<AppDbContext> _options`.
  It is the member that keeps the ten `TestDbContextFactory` sites reachable and it may not be made
  `private`.

Adding a member is a contract change and needs an escalation. Renaming one certainly is.

## 4. Disposal shape — `MASTER-127`

The base is the one non-sealed `IDisposable` in this set, so it is the one that must carry the full
pattern:

```csharp
public void Dispose()
{
	Dispose(true);
	GC.SuppressFinalize(this);
}

protected virtual void Dispose(bool disposing)
{
	if (disposing)
	{
		_connection.Dispose();
	}
}
```

`Dispose()` is **not** virtual. `Dispose(bool)` **is**. Every leaf is `sealed` and inherits both; a
leaf that needs no extra teardown overrides nothing.

There is no `IAsyncDisposable` and no `IAsyncLifetime` anywhere in `DungeonMaster.Tests`
(`grep -rn "IAsyncDisposable\|IAsyncLifetime" --include=*.cs src/DungeonMaster.Tests | grep -v '/bin/\|/obj/'`
→ nothing, re-run by the implementer). Do not introduce one here.

## 5. Schema construction — `MASTER-014`

The base constructor opens one `SqliteConnection("Data Source=:memory:")`, builds the options over
it, and applies the schema with **`Database.Migrate()`**, not `Database.EnsureCreated()`. After this
task, `grep -rn "EnsureCreated" --include=*.cs src/DungeonMaster.Tests | grep -v '/bin/\|/obj/'`
returns nothing.

This is de-risked, not speculative. The spike proved in a standalone scratch project that
`Database.Migrate()` succeeds against SQLite `:memory:` (3 applied, 0 pending, 6 tables), that
`dotnet ef migrations has-pending-model-changes` reports no model drift, and that the `EnsureCreated`
and `Migrate` schemas are set-identical — 11 objects each, differing only in physical column order
and in the two `__EFMigrations*` bookkeeping tables `Migrate` adds.

Those two extra tables and that column order are the two residual risks. They are criteria on T1, not
discoveries for the reviewer.

## 6. What the leaves look like afterwards

```csharp
public sealed class DungeonWriteRepositoryTests : SqliteInMemoryTestBase
```

No leaf declares `_connection`, `_options`, a constructor that builds either, `Dispose()`,
`CreateContext()` or `CreateFactory()`. Each leaf keeps its own `SeedAsync`-style helpers and its own
facts, unchanged in behaviour.
