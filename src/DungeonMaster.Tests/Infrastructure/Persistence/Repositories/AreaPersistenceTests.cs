using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Infrastructure.Persistence;
using DungeonMaster.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Repositories;

/// <summary>
///		This class holds the only persistence round-trip coverage of area removal and area rename.
///		Every other <c>RemoveArea</c> or <c>UpdateArea</c> hit in the suite is either an in-memory Domain aggregate test, which never reaches a database, or a handler test against a substituted repository, which never reaches EF; and <c>AreaOutlineChangeTrackingTests</c> covers the different method <c>UpdateAreaOutline</c>.
///		So if these facts are deleted or weakened, nothing anywhere else fails when a removal or a rename stops surviving a reload.
/// </summary>
public sealed class AreaPersistenceTests : SqliteInMemoryTestBase
{
	private async Task<(Guid DungeonId, Guid FloorId, Guid AreaId)> SeedAsync()
	{
		var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");
		var floor = dungeon.AddFloor(null);
		var area = dungeon.AddArea(floor.Id, "New Area", null)!;

		await using var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None);
		unitOfWork.Dungeons.Add(dungeon);
		await unitOfWork.CommitAsync(CancellationToken.None);

		return (dungeon.Id, floor.Id, area.Id);
	}

	[Fact]
	public async Task RemoveArea_WithEmptyOutline_PersistsAcrossReload()
	{
		var (dungeonId, floorId, areaId) = await SeedAsync();

		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			var loaded = await unitOfWork.Dungeons.LoadAsync(dungeonId, CancellationToken.None);
			loaded!.Floors.Single().Areas.Single().Outline.Should().BeEmpty();
			loaded.RemoveArea(floorId, areaId);
			unitOfWork.Dungeons.Update(loaded);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		using var verify = CreateContext();
		var verifyRepo = new DungeonWriteRepository(verify);
		var reloaded = await verifyRepo.LoadAsync(dungeonId, CancellationToken.None);

		reloaded!.Floors.Single().Areas.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateArea_RenamePersistsAcrossReload()
	{
		var (dungeonId, floorId, areaId) = await SeedAsync();

		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			var loaded = await unitOfWork.Dungeons.LoadAsync(dungeonId, CancellationToken.None);
			loaded!.UpdateArea(floorId, areaId, "Chapel", null, null);
			unitOfWork.Dungeons.Update(loaded);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		using var verify = CreateContext();
		var verifyRepo = new DungeonWriteRepository(verify);
		var reloaded = await verifyRepo.LoadAsync(dungeonId, CancellationToken.None);

		reloaded!.Floors.Single().Areas.Single().Name.Should().Be("Chapel");
	}

	[Fact]
	public async Task UpdateArea_SecondRenamePersistsAcrossReload()
	{
		var (dungeonId, floorId, areaId) = await SeedAsync();

		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			var loaded = await unitOfWork.Dungeons.LoadAsync(dungeonId, CancellationToken.None);
			loaded!.UpdateArea(floorId, areaId, "Chapel", null, null);
			unitOfWork.Dungeons.Update(loaded);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			var loaded = await unitOfWork.Dungeons.LoadAsync(dungeonId, CancellationToken.None);
			loaded!.UpdateArea(floorId, areaId, "Crypt", null, null);
			unitOfWork.Dungeons.Update(loaded);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		using var verify = CreateContext();
		var verifyRepo = new DungeonWriteRepository(verify);
		var reloaded = await verifyRepo.LoadAsync(dungeonId, CancellationToken.None);

		reloaded!.Floors.Single().Areas.Single().Name.Should().Be("Crypt");
	}
}
