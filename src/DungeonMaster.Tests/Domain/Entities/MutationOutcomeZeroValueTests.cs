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
		((int)MutationOutcome.None).Should().Be(0);
		default(MutationOutcome).Should().Be(MutationOutcome.None);
	}

	[Fact]
	public void DeclarationOrderOfTheExistingMembersSurvived()
	{
		((int)MutationOutcome.Success).Should().Be(1);
		((int)MutationOutcome.NotFound).Should().Be(2);
		((int)MutationOutcome.OutOfRange).Should().Be(3);
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
