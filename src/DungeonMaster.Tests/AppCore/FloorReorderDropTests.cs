using DungeonMaster.App.Core.Editor;
using FluentAssertions;

namespace DungeonMaster.Tests.AppCore;

/// <summary>
///		Covers every row of contract C1 section 1 over the one owner of the floor-reorder drop
///		decision. Nothing between the gesture and the command had any test at all before this file
///		(contract C1 section 6), and the platform half of the path — whether Windows delivers the
///		drop event to the application in the first place — is not reachable in process, so this
///		covers the decision and says so rather than pretending to cover the gesture.
/// </summary>
public class FloorReorderDropTests
{
	private static readonly Guid Dragged = Guid.Parse("11111111-1111-1111-1111-111111111111");
	private static readonly Guid Target = Guid.Parse("22222222-2222-2222-2222-222222222222");

	[Fact]
	public void Decide_WithANullPayload_DoesNotReorder()
	{
		// Act
		var decision = FloorReorderDrop.Decide(null, Target, targetFloorNumber: 7);

		// Assert
		decision.ShouldReorder.Should().BeFalse();
	}

	[Fact]
	public void Decide_WithTheFloorIdKeyAbsent_DoesNotReorder()
	{
		// Arrange
		var payload = new Dictionary<string, object> { ["SomethingElse"] = Dragged.ToString() };

		// Act
		var decision = FloorReorderDrop.Decide(payload, Target, targetFloorNumber: 7);

		// Assert
		decision.ShouldReorder.Should().BeFalse();
	}

	[Fact]
	public void Decide_WithAFloorIdThatIsNotAString_DoesNotReorder()
	{
		// Arrange
		var payload = new Dictionary<string, object> { [FloorReorderDrop.FloorIdKey] = Dragged };

		// Act
		var decision = FloorReorderDrop.Decide(payload, Target, targetFloorNumber: 7);

		// Assert
		decision.ShouldReorder.Should().BeFalse();
	}

	[Fact]
	public void Decide_WithAFloorIdThatDoesNotParse_DoesNotReorder()
	{
		// Arrange
		var payload = new Dictionary<string, object> { [FloorReorderDrop.FloorIdKey] = "not-a-guid" };

		// Act
		var decision = FloorReorderDrop.Decide(payload, Target, targetFloorNumber: 7);

		// Assert
		decision.ShouldReorder.Should().BeFalse();
	}

	[Fact]
	public void Decide_WithTheDraggedFloorEqualToTheTarget_DoesNotReorder()
	{
		// Arrange
		var payload = new Dictionary<string, object> { [FloorReorderDrop.FloorIdKey] = Target.ToString() };

		// Act
		var decision = FloorReorderDrop.Decide(payload, Target, targetFloorNumber: 7);

		// Assert
		decision.ShouldReorder.Should().BeFalse();
	}

	[Fact]
	public void Decide_WithNoTargetFloorBound_DoesNotReorder()
	{
		// Arrange
		var payload = new Dictionary<string, object> { [FloorReorderDrop.FloorIdKey] = Dragged.ToString() };

		// Act
		var decision = FloorReorderDrop.Decide(payload, targetFloorId: null, targetFloorNumber: 7);

		// Assert
		decision.ShouldReorder.Should().BeFalse();
	}

	/// <summary>
	///		The success row. The number carried out is the TARGET FLOOR'S 1-BASED NUMBER, which is
	///		what <c>ReorderFloorArgs</c> is constructed from at both call sites. The distinction the
	///		assertion is here to pin is number-versus-index: the target below is floor number 7 and
	///		sits at index 6 of an eleven-floor list, so a decision that returned an index would
	///		return 6 and redden this test.
	/// </summary>
	[Fact]
	public void Decide_WithADifferentDraggedFloor_ReordersToTheTargetsNumberAndNotItsIndex()
	{
		// Arrange
		var payload = new Dictionary<string, object> { [FloorReorderDrop.FloorIdKey] = Dragged.ToString() };

		// Act
		var decision = FloorReorderDrop.Decide(payload, Target, targetFloorNumber: 7);

		// Assert
		decision.ShouldReorder.Should().BeTrue();
		decision.DraggedFloorId.Should().Be(Dragged);
		decision.TargetFloorNumber.Should().Be(7);
	}

	[Fact]
	public void TryReadDraggedFloorId_WithANullPayload_ReturnsFalseWithAnEmptyId()
	{
		// Act
		var read = FloorReorderDrop.TryReadDraggedFloorId(null, out var draggedFloorId);

		// Assert
		read.Should().BeFalse();
		draggedFloorId.Should().Be(Guid.Empty);
	}

	[Fact]
	public void TryReadDraggedFloorId_WithTheFloorIdKeyAbsent_ReturnsFalseWithAnEmptyId()
	{
		// Arrange
		var payload = new Dictionary<string, object> { ["SomethingElse"] = Dragged.ToString() };

		// Act
		var read = FloorReorderDrop.TryReadDraggedFloorId(payload, out var draggedFloorId);

		// Assert
		read.Should().BeFalse();
		draggedFloorId.Should().Be(Guid.Empty);
	}

	/// <summary>
	///		The value is a real <see cref="Guid"/> rather than its string form, so a parser that accepted any value through its string form would read it.
	/// </summary>
	[Fact]
	public void TryReadDraggedFloorId_WithAFloorIdThatIsNotAString_ReturnsFalseWithAnEmptyId()
	{
		// Arrange
		var payload = new Dictionary<string, object> { [FloorReorderDrop.FloorIdKey] = Dragged };

		// Act
		var read = FloorReorderDrop.TryReadDraggedFloorId(payload, out var draggedFloorId);

		// Assert
		read.Should().BeFalse();
		draggedFloorId.Should().Be(Guid.Empty);
	}

	[Fact]
	public void TryReadDraggedFloorId_WithAFloorIdThatDoesNotParse_ReturnsFalseWithAnEmptyId()
	{
		// Arrange
		var payload = new Dictionary<string, object> { [FloorReorderDrop.FloorIdKey] = "not-a-guid" };

		// Act
		var read = FloorReorderDrop.TryReadDraggedFloorId(payload, out var draggedFloorId);

		// Assert
		read.Should().BeFalse();
		draggedFloorId.Should().Be(Guid.Empty);
	}

	[Fact]
	public void TryReadDraggedFloorId_WithADFormatFloorId_ReturnsTrueWithThatId()
	{
		// Arrange
		var payload = new Dictionary<string, object> { [FloorReorderDrop.FloorIdKey] = Dragged.ToString("D") };

		// Act
		var read = FloorReorderDrop.TryReadDraggedFloorId(payload, out var draggedFloorId);

		// Assert
		read.Should().BeTrue();
		draggedFloorId.Should().Be(Dragged);
	}
}
