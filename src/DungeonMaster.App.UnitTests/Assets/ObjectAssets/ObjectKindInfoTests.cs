using DungeonMaster.App.Assets.ObjectAssets;
using DungeonMaster.Domain.Entities.Floors;

using FluentAssertions;

namespace DungeonMaster.App.UnitTests.Assets.ObjectAssets;

/// <summary>
///		Guards <see cref="ObjectKindInfo"/>: every method rejects <see cref="ObjectKind.None"/> and
///		values outside the declared members, and every real kind keeps the output it had before
///		that guard existed.
/// </summary>
/// <remarks>
///		<para>
///		THE EXPECTED VALUES ARE LITERALS READ OFF THE CODE AT fd429e3. None of them is computed by
///		calling <see cref="ObjectKindInfo"/>, so a change to any method's output for a real kind
///		fails a row here rather than moving the expectation with it.
///		</para>
///		<para>
///		ONE TABLE FEEDS ALL FOUR THEORIES. Deleting a kind's row silently drops that kind from
///		every theory, which is why <see cref="TheExpectedTable_CoversEveryDefinedKindExceptNone"/>
///		exists: it fails until a newly declared kind is pinned.
///		</para>
///		<para>
///		THIS CLASS JOINS NO COLLECTION. <see cref="ObjectKindInfo"/> is a plain static class that
///		touches no <c>Microsoft.Maui.Controls</c> type, so it has no process-global state to race.
///		</para>
/// </remarks>
public class ObjectKindInfoTests
{
	/// <summary>
	///		A value outside every declared <see cref="ObjectKind"/> member.
	/// </summary>
	private const ObjectKind UndefinedKind = (ObjectKind)999;

	/// <summary>
	///		The pinned output of every method for every defined kind except
	///		<see cref="ObjectKind.None"/>, as the code at fd429e3 produced it.
	/// </summary>
	private static readonly ExpectedOutput[] Expected =
	[
		new(ObjectKind.DoorWood, SnapAnchor.East, 1.00f, 1.00f, "Wood Door", "Doors"),
		new(ObjectKind.DoorIron, SnapAnchor.East, 1.00f, 1.00f, "Iron Door", "Doors"),
		new(ObjectKind.DoorPortcullis, SnapAnchor.East, 1.00f, 1.00f, "Portcullis", "Doors"),
		new(ObjectKind.Arch, SnapAnchor.East, 1.00f, 1.00f, "Arch", "Doors"),
		new(ObjectKind.Window, SnapAnchor.East, 1.00f, 1.00f, "Window", "Doors"),
		new(ObjectKind.TableRound, SnapAnchor.Center, 1.00f, 1.00f, "Round Table", "Furniture"),
		new(ObjectKind.TableRect, SnapAnchor.Center, 2.00f, 1.00f, "Rect Table", "Furniture"),
		new(ObjectKind.TableEnd, SnapAnchor.Center, 0.70f, 0.70f, "Side Table", "Furniture"),
		new(ObjectKind.Desk, SnapAnchor.Center, 1.80f, 0.90f, "Writing Desk", "Furniture"),
		new(ObjectKind.Chair, SnapAnchor.Center, 0.75f, 0.75f, "Chair", "Furniture"),
		new(ObjectKind.Armchair, SnapAnchor.Center, 0.75f, 0.75f, "Armchair", "Furniture"),
		new(ObjectKind.Bench, SnapAnchor.Center, 0.45f, 1.50f, "Bench", "Furniture"),
		new(ObjectKind.Stool, SnapAnchor.Center, 0.55f, 0.55f, "Stool", "Furniture"),
		new(ObjectKind.Throne, SnapAnchor.Center, 0.90f, 0.90f, "Throne", "Furniture"),
		new(ObjectKind.Bed, SnapAnchor.Center, 1.00f, 2.00f, "Bed", "Furniture"),
		new(ObjectKind.StairsUp, SnapAnchor.Center, 1.00f, 1.00f, "Stairs Up", "Furniture"),
		new(ObjectKind.StairsDown, SnapAnchor.Center, 1.00f, 1.00f, "Stairs Down", "Furniture"),
		new(ObjectKind.Chest, SnapAnchor.Center, 0.65f, 0.65f, "Chest", "Storage"),
		new(ObjectKind.Barrel, SnapAnchor.Center, 0.65f, 0.65f, "Barrel", "Storage"),
		new(ObjectKind.Crate, SnapAnchor.Center, 0.65f, 0.65f, "Crate", "Storage"),
		new(ObjectKind.Bookshelf, SnapAnchor.South, 0.45f, 1.10f, "Bookshelf", "Storage"),
		new(ObjectKind.Dresser, SnapAnchor.South, 1.00f, 1.00f, "Dresser", "Storage"),
		new(ObjectKind.Shelf, SnapAnchor.South, 1.10f, 0.45f, "Shelf", "Storage"),
		new(ObjectKind.Vase, SnapAnchor.Center, 0.35f, 0.50f, "Vase", "Storage"),
		new(ObjectKind.WickerBasket, SnapAnchor.Center, 0.50f, 0.50f, "Basket", "Storage"),
		new(ObjectKind.Torch, SnapAnchor.Center, 0.35f, 0.35f, "Torch", "Lighting"),
		new(ObjectKind.Brazier, SnapAnchor.Center, 0.50f, 0.50f, "Brazier", "Lighting"),
		new(ObjectKind.Lantern, SnapAnchor.Center, 0.35f, 0.35f, "Lantern", "Lighting"),
		new(ObjectKind.Chandelier, SnapAnchor.Center, 0.80f, 0.80f, "Chandelier", "Lighting"),
		new(ObjectKind.Candle, SnapAnchor.Center, 0.35f, 0.35f, "Candle", "Lighting"),
		new(ObjectKind.Fountain, SnapAnchor.Center, 1.00f, 1.00f, "Fountain", "Decoration"),
		new(ObjectKind.Altar, SnapAnchor.Center, 0.90f, 0.90f, "Altar", "Decoration"),
		new(ObjectKind.Statue, SnapAnchor.Center, 0.90f, 0.90f, "Statue", "Decoration"),
		new(ObjectKind.Rug, SnapAnchor.Center, 1.80f, 1.20f, "Rug", "Decoration"),
		new(ObjectKind.Cauldron, SnapAnchor.Center, 0.90f, 0.90f, "Cauldron", "Decoration"),
		new(ObjectKind.Oven, SnapAnchor.Center, 0.90f, 0.90f, "Oven", "Decoration"),
		new(ObjectKind.WeaponRack, SnapAnchor.South, 0.45f, 1.10f, "Weapon Rack", "Decoration"),
	];

	/// <summary>
	///		Gets one row per pinned kind and its expected default anchor.
	/// </summary>
	public static IEnumerable<object[]> ExpectedAnchors =>
		Expected.Select(e => new object[] { e.Kind, e.Anchor });

	/// <summary>
	///		Gets one row per pinned kind and its expected logical size.
	/// </summary>
	public static IEnumerable<object[]> ExpectedSizes =>
		Expected.Select(e => new object[] { e.Kind, e.W, e.H });

	/// <summary>
	///		Gets one row per pinned kind and its expected display name.
	/// </summary>
	public static IEnumerable<object[]> ExpectedDisplayNames =>
		Expected.Select(e => new object[] { e.Kind, e.DisplayName });

	/// <summary>
	///		Gets one row per pinned kind and its expected category.
	/// </summary>
	public static IEnumerable<object[]> ExpectedCategories =>
		Expected.Select(e => new object[] { e.Kind, e.Category });

	[Fact]
	public void GetDefaultAnchor_WithNone_ThrowsArgumentOutOfRangeNamingKind()
	{
		FluentActions.Invoking(() => ObjectKindInfo.GetDefaultAnchor(ObjectKind.None))
			.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.WithParameterName("kind");
	}

	[Fact]
	public void GetDefaultAnchor_WithAnUndefinedValue_ThrowsArgumentOutOfRangeNamingKind()
	{
		FluentActions.Invoking(() => ObjectKindInfo.GetDefaultAnchor(UndefinedKind))
			.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.WithParameterName("kind");
	}

	[Fact]
	public void GetObjectSize_WithNone_ThrowsArgumentOutOfRangeNamingKind()
	{
		FluentActions.Invoking(() => ObjectKindInfo.GetObjectSize(ObjectKind.None))
			.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.WithParameterName("kind");
	}

	[Fact]
	public void GetObjectSize_WithAnUndefinedValue_ThrowsArgumentOutOfRangeNamingKind()
	{
		FluentActions.Invoking(() => ObjectKindInfo.GetObjectSize(UndefinedKind))
			.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.WithParameterName("kind");
	}

	[Fact]
	public void GetDisplayName_WithNone_ThrowsArgumentOutOfRangeNamingKind()
	{
		FluentActions.Invoking(() => ObjectKindInfo.GetDisplayName(ObjectKind.None))
			.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.WithParameterName("kind");
	}

	[Fact]
	public void GetDisplayName_WithAnUndefinedValue_ThrowsArgumentOutOfRangeNamingKind()
	{
		FluentActions.Invoking(() => ObjectKindInfo.GetDisplayName(UndefinedKind))
			.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.WithParameterName("kind");
	}

	[Fact]
	public void GetCategory_WithNone_ThrowsArgumentOutOfRangeNamingKind()
	{
		FluentActions.Invoking(() => ObjectKindInfo.GetCategory(ObjectKind.None))
			.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.WithParameterName("kind");
	}

	[Fact]
	public void GetCategory_WithAnUndefinedValue_ThrowsArgumentOutOfRangeNamingKind()
	{
		FluentActions.Invoking(() => ObjectKindInfo.GetCategory(UndefinedKind))
			.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.WithParameterName("kind");
	}

	[Theory]
	[MemberData(nameof(ExpectedAnchors))]
	public void GetDefaultAnchor_ForARealKind_ReturnsThePinnedAnchor(ObjectKind kind, SnapAnchor expected)
	{
		ObjectKindInfo.GetDefaultAnchor(kind).Should().Be(expected);
	}

	[Theory]
	[MemberData(nameof(ExpectedSizes))]
	public void GetObjectSize_ForARealKind_ReturnsThePinnedSize(ObjectKind kind, float expectedW, float expectedH)
	{
		ObjectKindInfo.GetObjectSize(kind).Should().Be((expectedW, expectedH));
	}

	[Theory]
	[MemberData(nameof(ExpectedDisplayNames))]
	public void GetDisplayName_ForARealKind_ReturnsThePinnedName(ObjectKind kind, string expected)
	{
		ObjectKindInfo.GetDisplayName(kind).Should().Be(expected);
	}

	[Theory]
	[MemberData(nameof(ExpectedCategories))]
	public void GetCategory_ForARealKind_ReturnsThePinnedCategory(ObjectKind kind, string expected)
	{
		ObjectKindInfo.GetCategory(kind).Should().Be(expected);
	}

	[Fact]
	public void TheExpectedTable_CoversEveryDefinedKindExceptNone()
	{
		var pinned = Expected.Select(e => e.Kind);
		var defined = Enum.GetValues<ObjectKind>().Where(k => k != ObjectKind.None);

		pinned.Should().OnlyHaveUniqueItems();
		pinned.Should().BeEquivalentTo(defined);
	}

	/// <summary>
	///		One kind's pinned output from each of the four methods.
	/// </summary>
	private sealed record ExpectedOutput(
		ObjectKind Kind,
		SnapAnchor Anchor,
		float W,
		float H,
		string DisplayName,
		string Category);
}
