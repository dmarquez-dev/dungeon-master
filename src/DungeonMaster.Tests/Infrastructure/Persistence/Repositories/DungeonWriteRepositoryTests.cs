using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.Entities.Dungeons;
using DungeonMaster.Domain.ValueObjects;
using DungeonMaster.Infrastructure.Persistence;
using DungeonMaster.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Repositories;

public sealed class DungeonWriteRepositoryTests : SqliteInMemoryTestBase
{
	[Fact]
	public async Task Add_PersistsDungeon()
	{
		// Arrange
		var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");

		// Act
		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			unitOfWork.Dungeons.Add(dungeon);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		// Assert
		using var verify = CreateContext();
		var persisted = await verify.Dungeons.FindAsync(dungeon.Id);
		persisted.Should().NotBeNull();
		persisted!.Name.Should().Be("Sunken Temple");
	}

	[Fact]
	public async Task RemoveAsync_RemovesDungeon()
	{
		// Arrange
		var dungeon = new Dungeon("Tomb of the Ancients");
		using (var ctx = CreateContext())
		{
			await ctx.Dungeons.AddAsync(dungeon);
			await ctx.SaveChangesAsync();
		}

		// Act
		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			await unitOfWork.Dungeons.RemoveAsync(dungeon.Id, CancellationToken.None);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		// Assert
		using var verify = CreateContext();
		var found = await verify.Dungeons.FindAsync(dungeon.Id);
		found.Should().BeNull();
	}

	[Fact]
	public async Task LoadAsync_HydratesFloorAndAreas()
	{
		// Arrange
		var dungeon = new Dungeon("Castle Ravenloft");
		var floor = dungeon.AddFloor("Main Hall");
		dungeon.AddArea(floor.Id, "Throne Room", "The seat of power.");

		using (var ctx = CreateContext())
		{
			await ctx.Dungeons.AddAsync(dungeon);
			await ctx.SaveChangesAsync();
		}

		using var readCtx = CreateContext();
		var repo = new DungeonWriteRepository(readCtx);

		// Act
		var loaded = await repo.LoadAsync(dungeon.Id, CancellationToken.None);

		// Assert
		loaded.Should().NotBeNull();
		loaded!.Floors.Should().HaveCount(1);
		loaded.Floors[0].Areas.Should().HaveCount(1);
		loaded.Floors[0].Areas[0].Name.Should().Be("Throne Room");
	}

	[Fact]
	public async Task Update_PersistsDomainMutations()
	{
		// Arrange
		var dungeon = new Dungeon("Old Name");
		using (var ctx = CreateContext())
		{
			await ctx.Dungeons.AddAsync(dungeon);
			await ctx.SaveChangesAsync();
		}

		// Act
		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			var loaded = await unitOfWork.Dungeons.LoadAsync(dungeon.Id, CancellationToken.None);
			loaded!.Update("New Name", null, "Some notes");
			unitOfWork.Dungeons.Update(loaded);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		// Assert
		using var verify = CreateContext();
		var updated = await verify.Dungeons.FindAsync(dungeon.Id);
		updated!.Name.Should().Be("New Name");
		updated.Notes.Should().Be("Some notes");
	}

	[Fact]
	public async Task RemoveAsync_WithExistingDungeon_ReturnsTrue()
	{
		// Arrange
		var dungeon = new Dungeon("Doomed Keep");

		using (var seedCtx = CreateContext())
		{
			await seedCtx.Dungeons.AddAsync(dungeon);
			await seedCtx.SaveChangesAsync();
		}

		// Act
		bool removed;

		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			removed = await unitOfWork.Dungeons.RemoveAsync(dungeon.Id, CancellationToken.None);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		// Assert
		removed.Should().BeTrue();

		using var verify = CreateContext();
		var found = await verify.Dungeons.FindAsync(dungeon.Id);
		found.Should().BeNull();
	}

	[Fact]
	public async Task RemoveAsync_WithUnknownId_ReturnsFalseAndSavesNothing()
	{
		// Arrange
		var survivor = new Dungeon("Untouched Vault");

		using (var seedCtx = CreateContext())
		{
			await seedCtx.Dungeons.AddAsync(survivor);
			await seedCtx.SaveChangesAsync();
		}

		// Act
		bool removed;

		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			removed = await unitOfWork.Dungeons.RemoveAsync(Guid.NewGuid(), CancellationToken.None);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		// Assert
		removed.Should().BeFalse();

		using var verify = CreateContext();
		var found = await verify.Dungeons.FindAsync(survivor.Id);
		found.Should().NotBeNull();
		found!.Name.Should().Be("Untouched Vault");
	}

	[Fact]
	public async Task Update_WithDetachedAggregate_PersistsTheChange()
	{
		// Arrange
		var dungeon = new Dungeon("Detached Original");

		using (var seedCtx = CreateContext())
		{
			await seedCtx.Dungeons.AddAsync(dungeon);
			await seedCtx.SaveChangesAsync();
		}

		Dungeon detached;

		using (var loadCtx = CreateContext())
		{
			var loadRepo = new DungeonWriteRepository(loadCtx);
			detached = (await loadRepo.LoadAsync(dungeon.Id, CancellationToken.None))!;
			detached.Update("Detached Updated", null, "Detached notes");
		}

		// Act
		EntityState observedState;

		using (var updateCtx = CreateContext())
		{
			observedState = updateCtx.Entry(detached).State;

			var updateRepo = new DungeonWriteRepository(updateCtx);
			updateRepo.Update(detached);
			await updateCtx.SaveChangesAsync();
		}

		// Assert
		observedState.Should().Be(EntityState.Detached);

		using var verify = CreateContext();
		var persisted = await verify.Dungeons.FindAsync(dungeon.Id);
		persisted!.Name.Should().Be("Detached Updated");
		persisted.Notes.Should().Be("Detached notes");
	}

	[Fact]
	public async Task LoadAsync_RoundTripsFloorObjectPositionsUnchanged()
	{
		// Arrange
		var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");
		var floor = dungeon.AddFloor(null);
		var obj = floor.AddObject(
			ObjectKind.Chair,
			new CanvasPoint(123.5f, 456.25f),
			ObjectRotation.Clockwise90,
			1.5f);

		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			unitOfWork.Dungeons.Add(dungeon);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		using var verify = CreateContext();
		var verifyRepo = new DungeonWriteRepository(verify);

		// Act
		var loaded = await verifyRepo.LoadAsync(dungeon.Id, CancellationToken.None);

		// Assert
		var loadedObj = loaded!.Floors.Single().Objects.Single();
		loadedObj.Id.Should().Be(obj.Id);
		loadedObj.Position.Should().Be(new CanvasPoint(123.5f, 456.25f));
		loadedObj.Rotation.Should().Be(ObjectRotation.Clockwise90);
		loadedObj.Scale.Should().Be(1.5f);
		loadedObj.FloorId.Should().Be(floor.Id);
	}
	/// <summary>
	///		Verifies that a token already cancelled before the call stops the aggregate load with <see cref="OperationCanceledException"/> rather than returning the aggregate.
	/// </summary>
	[Fact]
	public async Task LoadAsync_WithAnAlreadyCancelledToken_ThrowsOperationCanceledException()
	{
		// Arrange
		var dungeon = new Dungeon("Cancelled Load", null);

		using (var seedCtx = CreateContext())
		{
			await seedCtx.Dungeons.AddAsync(dungeon);
			await seedCtx.SaveChangesAsync();
		}

		using var cts = new CancellationTokenSource();
		await cts.CancelAsync();

		await using var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None);

		// Act
		var act = async () =>
			await unitOfWork.Dungeons.LoadAsync(dungeon.Id, cts.Token);

		// Assert
		await act.Should().ThrowAsync<OperationCanceledException>();
	}

}
