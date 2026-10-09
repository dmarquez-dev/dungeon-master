using DungeonMaster.App.Core.Map;
using DungeonMaster.Domain.ValueObjects;
using DungeonMaster.Tests.Infrastructure.Persistence;
using FluentAssertions;
using DomainDungeon = DungeonMaster.Domain.Entities.Dungeons.Dungeon;

namespace DungeonMaster.Tests.Domain;

/// <summary>
///		The durable regression guard for the cached read-only collection wrappers (MASTER-114).
///		Each of the five collection properties that were changed to reuse one wrapper has an assertion here that two successive reads on the same instance hand back the <em>same</em> object, so a future edit reinstating a per-call <c>AsReadOnly()</c> body turns red instead of silently restoring the allocation.
///		<c>AsReadOnly_CalledTwiceOnAPlainList_ReturnsDifferentWrapperInstances</c> is the positive control: it pins the pre-change shape, so the five identity assertions cannot pass merely because <c>AsReadOnly()</c> happened to be cheap or because a property was never read.
///		The class derives from <see cref="SqliteInMemoryTestBase"/> because <c>Area.Outline</c> is the one site whose cache must be proven on an EF-materialized instance: the value converter assigns a new <c>List&lt;PathSegment&gt;</c> over the backing field after the constructor runs, so a constructor-captured wrapper reads back empty forever and an in-memory-only assertion would pass against that broken form. The repository keeps one public test class per file, so the in-memory assertions share the fixture rather than moving to a second class.
/// </summary>
public sealed class CollectionWrapperCachingTests : SqliteInMemoryTestBase
{
	private static IReadOnlyList<PathSegment> Outline() =>
	[
		new PathSegment(
			PathSegmentKind.Line,
			new TilePoint(0, 0),
			null,
			null),
		new PathSegment(
			PathSegmentKind.Line,
			new TilePoint(4, 0),
			null,
			null),
		new PathSegment(
			PathSegmentKind.Line,
			new TilePoint(4, 4),
			null,
			null),
	];

	/// <summary>
	///		The pre-change shape, asserted so the five identity assertions below are known to be discriminating.
	///		<c>List&lt;T&gt;.AsReadOnly()</c> allocates a fresh wrapper per call; if this ever became reference-stable the other six tests would stop distinguishing a cached wrapper from an uncached one.
	/// </summary>
	[Fact]
	public void AsReadOnly_CalledTwiceOnAPlainList_ReturnsDifferentWrapperInstances()
	{
		// Arrange
		var backing = new List<int> { 1, 2, 3 };

		// Act
		var first = backing.AsReadOnly();
		var second = backing.AsReadOnly();

		// Assert
		first.Should().NotBeSameAs(second);
	}

	[Fact]
	public void Floors_ReadTwiceOnTheSameDungeon_ReturnsTheSameWrapperInstance()
	{
		// Arrange
		var dungeon = new DomainDungeon("Sunken Temple", "A flooded ruin.");
		dungeon.AddFloor(null);

		// Act
		var first = dungeon.Floors;
		var second = dungeon.Floors;

		// Assert
		first.Should().BeSameAs(second);
		first.Should().HaveCount(1);
	}

	[Fact]
	public void Areas_ReadTwiceOnTheSameFloor_ReturnsTheSameWrapperInstance()
	{
		// Arrange
		var dungeon = new DomainDungeon("Sunken Temple", null);
		var floor = dungeon.AddFloor(null);
		dungeon.AddArea(floor.Id, "Nave", null);

		// Act
		var first = floor.Areas;
		var second = floor.Areas;

		// Assert
		first.Should().BeSameAs(second);
		first.Should().HaveCount(1);
	}

	[Fact]
	public void Objects_ReadTwiceOnTheSameFloor_ReturnsTheSameWrapperInstance()
	{
		// Arrange
		var dungeon = new DomainDungeon("Sunken Temple", null);
		var floor = dungeon.AddFloor(null);

		// Act
		var first = floor.Objects;
		var second = floor.Objects;

		// Assert
		first.Should().BeSameAs(second);
	}

	[Fact]
	public void Vertices_ReadTwiceOnTheSameDrawingSession_ReturnsTheSameWrapperInstance()
	{
		// Arrange
		var session = new MapDrawingSession();
		session.PlaceVertex(new TilePoint(2, 2));

		// Act
		var first = session.Vertices;
		var second = session.Vertices;

		// Assert
		first.Should().BeSameAs(second);
		first.Should().HaveCount(1);
	}

	/// <summary>
	///		Covers the in-memory half of <c>Area.Outline</c> only.
	///		This assertion passes against both the correct lazy form and the constructor-captured form that was measured to break EF materialization, which is why <see cref="Outline_ReadTwiceOnAnEfMaterializedArea_ReturnsTheSameWrapperInstanceOverTheMaterializedList"/> exists alongside it.
	/// </summary>
	[Fact]
	public void Outline_ReadTwiceOnTheSameInMemoryArea_ReturnsTheSameWrapperInstance()
	{
		// Arrange
		var dungeon = new DomainDungeon("Sunken Temple", null);
		var floor = dungeon.AddFloor(null);
		var area = dungeon.AddArea(floor.Id, "Nave", null)!;

		dungeon.UpdateAreaOutline(
			floor.Id,
			area.Id,
			Outline());

		// Act
		var first = area.Outline;
		var second = area.Outline;

		// Assert
		first.Should().BeSameAs(second);
		first.Should().HaveCount(3);
	}

	/// <summary>
	///		The discriminating half of the <c>Area.Outline</c> cover.
	///		The area is written, the context discarded, and the aggregate reloaded, so the <c>Outline</c> read happens on an instance whose backing list EF installed through the value converter after construction.
	///		The <c>HaveCount(3)</c> assertion is what rejects a wrapper captured before materialization: such a wrapper is reference-stable but reads back empty.
	/// </summary>
	[Fact]
	public async Task Outline_ReadTwiceOnAnEfMaterializedArea_ReturnsTheSameWrapperInstanceOverTheMaterializedList()
	{
		// Arrange
		var dungeon = new DomainDungeon("Sunken Temple", "A flooded ruin.");
		var seedFloor = dungeon.AddFloor(null);
		var seedArea = dungeon.AddArea(seedFloor.Id, "Nave", null)!;

		dungeon.UpdateAreaOutline(
			seedFloor.Id,
			seedArea.Id,
			Outline());

		await using (var seeding = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			seeding.Dungeons.Add(dungeon);
			await seeding.CommitAsync(CancellationToken.None);
		}

		// Act
		await using var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None);

		var loaded = await unitOfWork.Dungeons.LoadAsync(
			dungeon.Id,
			CancellationToken.None);

		var materializedArea = loaded!.Floors.Single().Areas.Single();

		var first = materializedArea.Outline;
		var second = materializedArea.Outline;

		// Assert
		first.Should().BeSameAs(second);
		first.Should().HaveCount(3);
		first.Should().Equal(Outline());
	}
}
