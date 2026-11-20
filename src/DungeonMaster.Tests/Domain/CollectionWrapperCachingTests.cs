using DungeonMaster.App.Core.Map;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using DungeonMaster.Tests.Infrastructure.Persistence;
using FluentAssertions;
using DomainDungeon = DungeonMaster.Domain.Entities.Dungeons.Dungeon;

namespace DungeonMaster.Tests.Domain;

/// <summary>
///		The durable regression guard for the cached read-only collection wrappers (MASTER-114).
///		Each of the five collection properties that were changed to reuse one wrapper has an assertion here that two successive reads on the same instance hand back the <em>same</em> object, so a future edit reinstating a per-call <c>AsReadOnly()</c> body turns red instead of silently restoring the allocation.
///		Each of the five also has a read-mutate-reread test: it reads the property, mutates the backing collection through the public API, reads again, and asserts both the same instance and the changed count. Two reads with no mutation between them cannot tell a live view from a snapshot copy built once on first read, because both are reference-stable; the reread after a mutation can, because only the live view shows the change.
///		<c>AsReadOnly_CalledTwiceOnAPlainList_ReturnsDifferentWrapperInstances</c> is the positive control: it pins the pre-change shape, so the identity assertions cannot pass merely because <c>AsReadOnly()</c> happened to be cheap or because a property was never read.
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
	///		The pre-change shape, asserted so the identity assertions below are known to be discriminating.
	///		<c>List&lt;T&gt;.AsReadOnly()</c> allocates a fresh wrapper per call; if this ever became reference-stable the <c>BeSameAs</c> assertions in every other test in this class would stop distinguishing a cached wrapper from an uncached one.
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

		dungeon.AddPlacedObject(
			floor.Id,
			ObjectKind.DoorWood,
			new CanvasPoint(1.5f, 2.5f),
			ObjectRotation.None,
			1.0f);

		// Act
		var first = floor.Objects;
		var second = floor.Objects;

		// Assert
		first.Should().BeSameAs(second);
		first.Should().HaveCount(1);
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

	[Fact]
	public void Floors_ReadAgainAfterAddFloor_ReturnsTheSameWrapperWithOneMoreFloor()
	{
		// Arrange
		var dungeon = new DomainDungeon("Sunken Temple", null);
		var first = dungeon.Floors;
		var countBefore = first.Count;

		// Act
		dungeon.AddFloor(null);
		var second = dungeon.Floors;

		// Assert
		second.Should().BeSameAs(first);
		second.Should().HaveCount(countBefore + 1);
	}

	[Fact]
	public void Areas_ReadAgainAfterAddArea_ReturnsTheSameWrapperWithOneMoreArea()
	{
		// Arrange
		var dungeon = new DomainDungeon("Sunken Temple", null);
		var floor = dungeon.AddFloor(null);
		var first = floor.Areas;
		var countBefore = first.Count;

		// Act
		dungeon.AddArea(floor.Id, "Nave", null);
		var second = floor.Areas;

		// Assert
		second.Should().BeSameAs(first);
		second.Should().HaveCount(countBefore + 1);
	}

	[Fact]
	public void Objects_ReadAgainAfterAddPlacedObject_ReturnsTheSameWrapperWithOneMoreObject()
	{
		// Arrange
		var dungeon = new DomainDungeon("Sunken Temple", null);
		var floor = dungeon.AddFloor(null);
		var first = floor.Objects;
		var countBefore = first.Count;

		// Act
		dungeon.AddPlacedObject(
			floor.Id,
			ObjectKind.DoorWood,
			new CanvasPoint(1.5f, 2.5f),
			ObjectRotation.None,
			1.0f);

		var second = floor.Objects;

		// Assert
		second.Should().BeSameAs(first);
		second.Should().HaveCount(countBefore + 1);
	}

	[Fact]
	public void Outline_ReadAgainAfterUpdateAreaOutlineOnAnInMemoryArea_ReturnsTheSameWrapperWithThreeSegments()
	{
		// Arrange
		var dungeon = new DomainDungeon("Sunken Temple", null);
		var floor = dungeon.AddFloor(null);
		var area = dungeon.AddArea(floor.Id, "Nave", null)!;
		var first = area.Outline;

		// Act
		dungeon.UpdateAreaOutline(
			floor.Id,
			area.Id,
			Outline());

		var second = area.Outline;

		// Assert
		second.Should().BeSameAs(first);
		second.Should().HaveCount(3);
	}

	[Fact]
	public void Vertices_ReadAgainAfterPlaceVertex_ReturnsTheSameWrapperWithOneMoreVertex()
	{
		// Arrange
		var session = new MapDrawingSession();
		var first = session.Vertices;
		var countBefore = first.Count;

		// Act
		session.PlaceVertex(new TilePoint(2, 2));
		var second = session.Vertices;

		// Assert
		second.Should().BeSameAs(first);
		second.Should().HaveCount(countBefore + 1);
	}
}
