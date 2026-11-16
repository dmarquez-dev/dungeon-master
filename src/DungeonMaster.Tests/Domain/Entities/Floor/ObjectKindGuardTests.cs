using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.Domain.Entities.Floor;

/// <summary>
///		Tests for the <see cref="ObjectKind.None"/> member and the single construction
///		chokepoint on <see cref="PlacedObject"/> that rejects it. Only the ordinal of
///		<see cref="ObjectKind.None"/> (0, the default) is relied on; the other
///		members' ordinals are not persisted, because <c>PlacedObjectConfiguration</c>
///		stores <c>Kind</c> by name, so renumbering them is free.
/// </summary>
public class ObjectKindGuardTests
{
	[Fact]
	public void None_IsZero_AndIsTheDefaultObjectKind()
	{
		// Act
		var noneValue = (int)ObjectKind.None;
		var defaultKind = default(ObjectKind);

		// Assert
		noneValue.Should().Be(0);
		defaultKind.Should().Be(ObjectKind.None);
	}

	[Fact]
	public void ExactlyOneMemberCastsToZero_AndItIsNone()
	{
		// Act
		var values = Enum.GetValues<ObjectKind>();

		// Assert
		values.Should().HaveCount(38);
		values.Where(kind => (int)kind == 0).Should().ContainSingle()
			.Which.Should().Be(ObjectKind.None);
	}

	[Fact]
	public void NoneIsADefinedMember_WhichIsWhyTheGuardNeedsItsOwnNoneCondition()
	{
		// Recorded as a test rather than as prose: because None is defined,
		// an IsDefined-only guard would admit it, and the validator's IsInEnum()
		// rule alone would accept it. Hence the separate None condition and the
		// separate .NotEqual(ObjectKind.None) validator rule.
		Enum.IsDefined(ObjectKind.None).Should().BeTrue();
	}

	[Fact]
	public void FiveParameterConstructor_WithUndefinedKind_Throws()
	{
		// Act
		var act = () => new PlacedObject(
			Guid.NewGuid(),
			(ObjectKind)999,
			new CanvasPoint(0f, 0f),
			ObjectRotation.None,
			ObjectScale.Default);

		// Assert
		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("kind");

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ActualValue.Should().Be((ObjectKind)999);
	}

	[Fact]
	public void FiveParameterConstructor_WithNoneKindAndScaleAboveMax_RejectsTheKindFirst()
	{
		// Act
		var act = () => new PlacedObject(
			Guid.NewGuid(),
			ObjectKind.None,
			new CanvasPoint(0f, 0f),
			ObjectRotation.None,
			ObjectScale.Max * 2f);

		// Assert
		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("kind");

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ActualValue.Should().Be(ObjectKind.None);
	}

	[Fact]
	public void PositiveControl_ConstructorAcceptsADefinedKind()
	{
		// Arrange
		var floorId = Guid.NewGuid();
		var position = new CanvasPoint(120f, 200f);

		// Act
		var obj = new PlacedObject(
			floorId,
			ObjectKind.DoorWood,
			position,
			ObjectRotation.Clockwise90,
			ObjectScale.Default);

		// Assert
		obj.Kind.Should().Be(ObjectKind.DoorWood);
		obj.FloorId.Should().Be(floorId);
		obj.Position.Should().Be(position);
		obj.Rotation.Should().Be(ObjectRotation.Clockwise90);
	}
}
