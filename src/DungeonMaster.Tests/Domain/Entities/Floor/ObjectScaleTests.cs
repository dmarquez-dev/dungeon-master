using DungeonMaster.Domain.Entities.Floors;
using FluentAssertions;

namespace DungeonMaster.Tests.Domain.Entities.Floor;

public class ObjectScaleTests
{
	[Theory]
	[InlineData(1.0f, 1.25f)]
	[InlineData(4.0f, 4.0f)]  // clamped at max
	public void StepUp_ReturnsExpected(float input, float expected)
	{
		ObjectScale.StepUp(input).Should().BeApproximately(expected, 0.001f);
	}

	[Theory]
	[InlineData(1.0f, 0.75f)]
	[InlineData(0.25f, 0.25f)]  // clamped at min
	public void StepDown_ReturnsExpected(float input, float expected)
	{
		ObjectScale.StepDown(input).Should().BeApproximately(expected, 0.001f);
	}

	[Fact]
	public void Default_IsOne()
	{
		ObjectScale.Default.Should().BeApproximately(1.0f, 0.001f);
	}
}
