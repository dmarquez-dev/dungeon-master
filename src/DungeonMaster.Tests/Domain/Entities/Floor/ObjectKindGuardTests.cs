using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.Domain.Entities.Floor;

/// <summary>
///		Tests for the <see cref="ObjectKind.None"/> member and the single construction
///		chokepoint on <see cref="PlacedObject"/> that rejects it.
/// </summary>
public class ObjectKindGuardTests
{
	[Fact]
	public void None_IsZero_AndIsTheDefaultObjectKind()
	{
		((int)ObjectKind.None).Should().Be(0);
		default(ObjectKind).Should().Be(ObjectKind.None);
	}

	[Fact]
	public void DeclarationOrderOfTheExistingMembersSurvived()
	{
		((int)ObjectKind.DoorWood).Should().Be(1);
		((int)ObjectKind.DoorIron).Should().Be(2);
	}

	[Fact]
	public void ExactlyOneMemberCastsToZero_AndItIsNone()
	{
		var values = Enum.GetValues<ObjectKind>();

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
	public void FourParameterConstructor_WithNoneKind_Throws()
	{
		var act = () => new PlacedObject(
			Guid.NewGuid(),
			ObjectKind.None,
			new CanvasPoint(0f, 0f),
			ObjectRotation.None);

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("kind");

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ActualValue.Should().Be(ObjectKind.None);
	}

	[Fact]
	public void FiveParameterConstructor_WithUndefinedKind_Throws()
	{
		var act = () => new PlacedObject(
			Guid.NewGuid(),
			(ObjectKind)999,
			new CanvasPoint(0f, 0f),
			ObjectRotation.None,
			ObjectScale.Default);

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("kind");

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ActualValue.Should().Be((ObjectKind)999);
	}

	[Fact]
	public void KindGuardRunsBeforeTheScaleRejection()
	{
		var act = () => new PlacedObject(
			Guid.NewGuid(),
			ObjectKind.None,
			new CanvasPoint(0f, 0f),
			ObjectRotation.None,
			ObjectScale.Max * 2f);

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("kind");
	}

	[Fact]
	public void PositiveControl_ConstructorAcceptsADefinedKind()
	{
		var floorId = Guid.NewGuid();
		var position = new CanvasPoint(120f, 200f);

		var obj = new PlacedObject(
			floorId,
			ObjectKind.DoorWood,
			position,
			ObjectRotation.CW90,
			ObjectScale.Default);

		obj.Kind.Should().Be(ObjectKind.DoorWood);
		obj.FloorId.Should().Be(floorId);
		obj.Position.Should().Be(position);
		obj.Rotation.Should().Be(ObjectRotation.CW90);
	}
}
