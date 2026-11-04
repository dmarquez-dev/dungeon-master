using DungeonMaster.Domain.Entities;
using FluentAssertions;

namespace DungeonMaster.Tests.Domain.Entities;

/// <summary>
///		Tests for the <see cref="MutationOutcome.None"/> member added at the zero slot.
/// </summary>
public class MutationOutcomeZeroValueTests
{
	[Fact]
	public void None_IsZero_AndIsTheDefaultMutationOutcome()
	{
		// Act
		var noneValue = (int)MutationOutcome.None;
		var defaultOutcome = default(MutationOutcome);

		// Assert
		noneValue.Should().Be(0);
		defaultOutcome.Should().Be(MutationOutcome.None);
	}

	[Fact]
	public void MutationOutcome_ExistingMembers_KeepValuesOneThroughThree()
	{
		// Act
		var successValue = (int)MutationOutcome.Success;
		var notFoundValue = (int)MutationOutcome.NotFound;
		var outOfRangeValue = (int)MutationOutcome.OutOfRange;

		// Assert
		successValue.Should().Be(1);
		notFoundValue.Should().Be(2);
		outOfRangeValue.Should().Be(3);
	}

	[Fact]
	public void NoneIsADefinedMember_WhichIsWhyTheGuardNeedsItsOwnNoneCondition()
	{
		// Recorded as a test rather than as prose: because None is defined, an
		// IsDefined-only guard would admit it. Hence ToResult rejects the member
		// by name rather than testing for definedness.
		Enum.IsDefined(MutationOutcome.None).Should().BeTrue();
	}
}
