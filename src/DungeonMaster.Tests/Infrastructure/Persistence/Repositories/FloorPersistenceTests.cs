using DungeonMaster.Domain.Entities.Dungeons;
using DungeonMaster.Infrastructure.Persistence;
using DungeonMaster.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Repositories;

/// <summary>
///		Round-trip coverage for floor metadata through the real write repository
///		and EF mapping. Confirms that a second update supplying the existing
///		description and notes preserves those values across a reload.
/// </summary>
public sealed class FloorPersistenceTests : SqliteInMemoryTestBase
{
	private async Task<(Guid DungeonId, Guid FloorId)> SeedAsync()
	{
		var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");
		var floor = dungeon.AddFloor("Crypt");

		await using var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None);
		unitOfWork.Dungeons.Add(dungeon);
		await unitOfWork.CommitAsync(CancellationToken.None);

		return (dungeon.Id, floor.Id);
	}

	[Fact]
	public async Task UpdateFloor_DescriptionPersistsAcrossReload()
	{
		var (dungeonId, floorId) = await SeedAsync();

		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			var loaded = await unitOfWork.Dungeons.LoadAsync(dungeonId, CancellationToken.None);
			loaded!.UpdateFloor(floorId, "Crypt", "The air grows cold.", "Ghasts.");
			unitOfWork.Dungeons.Update(loaded);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		using var verify = CreateContext();
		var verifyRepo = new DungeonWriteRepository(verify);
		var reloaded = await verifyRepo.LoadAsync(dungeonId, CancellationToken.None);

		var floor = reloaded!.Floors.Single();
		floor.Description.Should().Be("The air grows cold.");
		floor.Notes.Should().Be("Ghasts.");
	}

	[Fact]
	public async Task UpdateFloor_RenamePreservesDescriptionAndNotes()
	{
		var (dungeonId, floorId) = await SeedAsync();

		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			var loaded = await unitOfWork.Dungeons.LoadAsync(dungeonId, CancellationToken.None);
			loaded!.UpdateFloor(floorId, "Crypt", "The air grows cold.", "Ghasts.");
			unitOfWork.Dungeons.Update(loaded);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		// Asserts the repository and mapping layer preserve description and notes when an
		// update explicitly supplies them. The App-layer path that instead passes hardcoded
		// nulls (DungeonDetailViewModel.UpdateFloorAsync) is not reachable from this test
		// project — DungeonMaster.Tests does not reference DungeonMaster.App — and is covered
		// by manual verification instead.
		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			var loaded = await unitOfWork.Dungeons.LoadAsync(dungeonId, CancellationToken.None);
			var existing = loaded!.Floors.Single();
			loaded.UpdateFloor(floorId, "Ossuary", existing.Description, existing.Notes);
			unitOfWork.Dungeons.Update(loaded);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		using var verify = CreateContext();
		var verifyRepo = new DungeonWriteRepository(verify);
		var reloaded = await verifyRepo.LoadAsync(dungeonId, CancellationToken.None);

		var floor = reloaded!.Floors.Single();
		floor.Name.Should().Be("Ossuary");
		floor.Description.Should().Be("The air grows cold.");
		floor.Notes.Should().Be("Ghasts.");
	}
}
