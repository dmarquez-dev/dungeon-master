using DungeonMaster.App.Core.Editor;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Layout;

/// <summary>
///		The seam that ends the silent discard: which floor card a drop that missed every drop
///		target resolves to — escalation E6.
/// </summary>
/// <remarks>
///		<para>
///			WHAT WAS BROKEN. At <c>b3b1641</c> <c>FloorListContainer</c> carried no
///			<c>DropGestureRecognizer</c> and <c>DungeonDetailPage.xaml</c> declared none at all, so
///			a release landing on the seam between two cards — or on the reserved scrollbar strip,
///			or on any space no handler owned — was discarded with no handler, no error and no
///			trace. The reorder itself works end to end once the drop lands on a card, so that
///			silence is the whole reason the feature read as broken.
///		</para>
///		<para>
///			THE GEOMETRY THESE CASES USE IS THE MEASURED ONE, not a convenient round number. In
///			the running application at 900x640 with 11 floors, each floor card is a
///			<c>ContentView</c> 58 DIU tall whose root <c>Border</c> — the element that carries the
///			card's own drop target — is 54. The remaining 4 DIU are that border's bottom margin,
///			and those 4 DIU ARE the seam. So card <c>i</c> occupies <c>[58i, 58i + 54)</c> and the
///			dead strip is <c>[58i + 54, 58i + 58)</c>.
///		</para>
///		<para>
///			WHY THE TIE MATTERS MORE THAN IT LOOKS. Every card in this list is the same height, so
///			the centre of a seam is exactly equidistant from both neighbours' midpoints. A tie is
///			therefore the COMMON case here rather than a corner one, and which way it falls must
///			be a decision rather than an artefact of iteration order.
///		</para>
/// </remarks>
public class FloorDragHandleDropTargetTests
{
	private const double CardPitch = 58;

	private const double CardHeight = 54;

	/// <summary>
	///		Eleven measured cards, the configuration every observation in this session used.
	/// </summary>
	private static IReadOnlyList<FloorListBand> ElevenCards() =>
		Enumerable
			.Range(0, 11)
			.Select(index => new FloorListBand(
				FloorListBandKind.FloorCard,
				index * CardPitch,
				CardHeight))
			.ToList();

	/// <summary>
	///		A drop that lands inside a card is that card's own handler's, and the list declines it.
	/// </summary>
	/// <remarks>
	///		This is what makes the third drop target safe to add. Whether the platform bubbles a
	///		drop from the card up to the list is not something this code should have to know:
	///		declining every drop that landed inside an existing target means the reorder runs
	///		exactly once whichever answer the platform gives.
	/// </remarks>
	[Theory]
	[InlineData(0, 0)]
	[InlineData(53.9, 0)]
	[InlineData(58, 1)]
	[InlineData(111, 1)]
	[InlineData(580, 10)]
	public void ADropInsideACardBelongsToThatCardAndNotToTheList(double y, int expectedIndex)
	{
		var resolution = FloorReorderNearest.Resolve(ElevenCards(), y);

		resolution
			.Owner
			.Should()
			.Be(
				FloorDropOwner.ExistingTarget,
				"the card's own root Border carries a DropGestureRecognizer, so acting on this "
				+ "drop here as well would reorder twice for one release");

		resolution
			.BandIndex
			.Should()
			.Be(expectedIndex);
	}

	/// <summary>
	///		A drop on the seam between two cards resolves to a card instead of vanishing, and an
	///		exact tie keeps the upper one.
	/// </summary>
	[Theory]
	[InlineData(54, 0)]
	[InlineData(55, 0)]
	[InlineData(56, 0)]
	[InlineData(57, 1)]
	[InlineData(112, 1)]
	[InlineData(114, 1)]
	[InlineData(115, 2)]
	public void ADropOnTheSeamResolvesToACardRatherThanVanishing(double y, int expectedIndex)
	{
		var resolution = FloorReorderNearest.Resolve(ElevenCards(), y);

		resolution
			.Owner
			.Should()
			.Be(
				FloorDropOwner.FloorList,
				"this is the release that used to be discarded with no handler, no error and no "
				+ "trace — the silent failure escalation E6 exists to end");

		resolution
			.BandIndex
			.Should()
			.Be(
				expectedIndex,
				"y {0} is nearest the midpoint of card {1}, and an exact tie keeps the upper card",
				y,
				expectedIndex);
	}

	/// <summary>
	///		A drop past the end of the list, or above its start, still resolves to the nearest card.
	/// </summary>
	/// <remarks>
	///		No distance threshold is applied, and none is needed: the floor list lays out exactly as
	///		tall as its content, so the container's own bounds already bound how far a release can
	///		be from a card and still have landed on the list at all.
	/// </remarks>
	[Theory]
	[InlineData(-40, 0)]
	[InlineData(700, 10)]
	public void ADropBeyondTheListResolvesToTheNearestCard(double y, int expectedIndex)
	{
		var resolution = FloorReorderNearest.Resolve(ElevenCards(), y);

		resolution
			.Owner
			.Should()
			.Be(FloorDropOwner.FloorList);

		resolution
			.BandIndex
			.Should()
			.Be(expectedIndex);
	}

	/// <summary>
	///		The transient placeholder is a drop target that can be landed IN but never resolved TO.
	/// </summary>
	/// <remarks>
	///		The placeholder carries its own <c>DropGestureRecognizer</c>, so a drop inside it is
	///		already handled. It is not a floor, so it has no <c>Number</c> a reorder could name —
	///		snapping a near-miss to it would produce a reorder against a target that does not exist.
	/// </remarks>
	[Fact]
	public void ThePlaceholderIsLandedInButNeverResolvedTo()
	{
		// Card 0, then the placeholder in card 1's slot, then card 2.
		var bands = new List<FloorListBand>
		{
			new(FloorListBandKind.FloorCard, 0, CardHeight),
			new(FloorListBandKind.TransientDropTarget, CardPitch, 44),
			new(FloorListBandKind.FloorCard, 2 * CardPitch, CardHeight),
		};

		FloorReorderNearest
			.Resolve(bands, CardPitch + 20)
			.Should()
			.Be(
				new FloorDropResolution(FloorDropOwner.ExistingTarget, 1),
				"a drop inside the placeholder is the placeholder's own handler's");

		// The seam just past the placeholder: a near-miss that must NOT snap back onto it.
		var resolution = FloorReorderNearest.Resolve(bands, CardPitch + 50);

		resolution
			.Owner
			.Should()
			.Be(FloorDropOwner.FloorList);

		bands[resolution.BandIndex]
			.Kind
			.Should()
			.Be(
				FloorListBandKind.FloorCard,
				"the placeholder has no floor Number to reorder against, so resolving to it would "
				+ "name a target that does not exist");
	}

	/// <summary>
	///		Bands that are not laid out are ignored rather than treated as a zero-height line.
	/// </summary>
	[Fact]
	public void AnUnmeasuredBandIsIgnoredEntirely()
	{
		var bands = new List<FloorListBand>
		{
			new(FloorListBandKind.FloorCard, 0, 0),
			new(FloorListBandKind.FloorCard, 0, CardHeight),
		};

		FloorReorderNearest
			.Resolve(bands, 10)
			.Should()
			.Be(
				new FloorDropResolution(FloorDropOwner.ExistingTarget, 1),
				"a band with no height cannot contain a point, and must not shadow the laid-out "
				+ "band that shares its origin");
	}

	/// <summary>
	///		Nothing to resolve against resolves to nothing, silently.
	/// </summary>
	/// <remarks>
	///		Silence here is the same shape as contract C1 section 1's four silent returns: a drop
	///		that means nothing is not a failure and raises no message. A non-finite position is what
	///		an unresolvable platform coordinate degrades to, and resolving that to card zero would
	///		reorder a floor the user never aimed at.
	/// </remarks>
	[Fact]
	public void NothingToResolveAgainstResolvesToNothing()
	{
		FloorReorderNearest
			.Resolve(null, 10)
			.Should()
			.Be(FloorDropResolution.None);

		FloorReorderNearest
			.Resolve([], 10)
			.Should()
			.Be(FloorDropResolution.None);

		FloorReorderNearest
			.Resolve(ElevenCards(), double.NaN)
			.Should()
			.Be(
				FloorDropResolution.None,
				"an unresolvable platform position must not reorder a floor the user never "
				+ "aimed at");

		FloorReorderNearest
			.Resolve([new FloorListBand(FloorListBandKind.TransientDropTarget, 0, 44)], 100)
			.Should()
			.Be(
				FloorDropResolution.None,
				"a list holding only a placeholder has no card a near-miss could resolve to");

		FloorDropResolution
			.None
			.BandIndex
			.Should()
			.Be(
				FloorReorderNearest.NoBand,
				"the empty resolution must not carry index 0, which is a real card");
	}
}
