using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Repositories;

/// <summary>
///		Proves the lifetime the session settled on: one <see cref="AppDbContext"/> per unit of work, shared by the load and the commit, and never shared between two units of work.
/// </summary>
public sealed class UnitOfWorkTests : SqliteInMemoryTestBase
{
	/// <summary>
	///		Verifies that two units of work track independently: the same dungeon loaded in each comes back as two distinct instances.
	///		Under a single process-lifetime context both loads would return the identical tracked instance, which is the accumulation INFRA-018 describes.
	/// </summary>
	[Fact]
	public async Task CreateAsync_CalledTwice_ProducesTwoIndependentChangeTrackers()
	{
		// Arrange
		var dungeon = new Dungeon("Shared Vault", "Loaded twice.");

		using (var seedCtx = CreateContext())
		{
			await seedCtx.Dungeons.AddAsync(dungeon);
			await seedCtx.SaveChangesAsync();
		}

		var factory = CreateFactory();

		// Act
		Dungeon first;
		Dungeon second;

		await using (var unitOfWork = await factory.CreateAsync(CancellationToken.None))
		{
			first = (await unitOfWork.Dungeons.LoadAsync(dungeon.Id, CancellationToken.None))!;
		}

		await using (var unitOfWork = await factory.CreateAsync(CancellationToken.None))
		{
			second = (await unitOfWork.Dungeons.LoadAsync(dungeon.Id, CancellationToken.None))!;
		}

		// Assert
		first.Should().NotBeNull();
		second.Should().NotBeNull();
		first.Should().NotBeSameAs(second);
	}

	/// <summary>
	///		Verifies that within one unit of work the loaded aggregate is tracked by that unit's context, so the later commit sees exactly the properties the mutation changed.
	/// </summary>
	[Fact]
	public async Task Dungeons_WithinOneUnitOfWork_ReturnsATrackedAggregate()
	{
		// Arrange
		var dungeon = new Dungeon("Tracked Keep");

		using (var seedCtx = CreateContext())
		{
			await seedCtx.Dungeons.AddAsync(dungeon);
			await seedCtx.SaveChangesAsync();
		}

		var dbContext = CreateDbContextFactory().CreateDbContext();

		// Act
		EntityState observedState;

		await using (var unitOfWork = new UnitOfWork(dbContext))
		{
			var loaded = await unitOfWork.Dungeons.LoadAsync(dungeon.Id, CancellationToken.None);

			observedState = dbContext.Entry(loaded!).State;
		}

		// Assert
		observedState.Should().Be(EntityState.Unchanged);
		observedState.Should().NotBe(EntityState.Detached);
	}

	/// <summary>
	///		Verifies that a unit of work disposed without a commit writes nothing, which is the transaction boundary the explicit commit buys.
	/// </summary>
	[Fact]
	public async Task DisposeAsync_WithoutCommit_WritesNothing()
	{
		// Arrange
		var dungeon = new Dungeon("Discarded Hold");

		using (var seedCtx = CreateContext())
		{
			await seedCtx.Dungeons.AddAsync(dungeon);
			await seedCtx.SaveChangesAsync();
		}

		// Act
		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			var loaded = await unitOfWork.Dungeons.LoadAsync(dungeon.Id, CancellationToken.None);
			loaded!.Update("Never Written", null, null);
			unitOfWork.Dungeons.Update(loaded);
		}

		// Assert
		using var verify = CreateContext();
		var persisted = await verify.Dungeons.FindAsync(dungeon.Id);
		persisted!.Name.Should().Be("Discarded Hold");
	}
	/// <summary>
	///		Verifies that a token already cancelled before the call stops the commit with <see cref="OperationCanceledException"/>, and that the staged aggregate is therefore absent from the store.
	/// </summary>
	[Fact]
	public async Task CommitAsync_WithAnAlreadyCancelledToken_ThrowsOperationCanceledExceptionAndWritesNothing()
	{
		// Arrange
		var dungeon = new Dungeon("Cancelled Commit", null);

		using var cts = new CancellationTokenSource();
		await cts.CancelAsync();

		// Act
		Func<Task> act;

		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			unitOfWork.Dungeons.Add(dungeon);

			act = async () =>
				await unitOfWork.CommitAsync(cts.Token);

			// Assert
			await act.Should().ThrowAsync<OperationCanceledException>();
		}

		using var verify = CreateContext();
		(await verify.Dungeons.FindAsync(dungeon.Id)).Should().BeNull();
	}

}
