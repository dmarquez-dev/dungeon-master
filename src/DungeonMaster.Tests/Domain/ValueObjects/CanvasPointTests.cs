using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.Domain.ValueObjects;

/// <summary>
///		Covers the finiteness invariant <c>CanvasPoint</c> enforces on both coordinates (MASTER-081 / CORE-016).
///		Every rejection is asserted with <c>ThrowExactly</c> and with <c>ParamName</c>, because nothing downstream of the constructor distinguishes the two coordinates: <c>AddPlacedObjectCommandValidator.cs:30-32</c> constrains <c>Position</c> with <c>.NotNull()</c> and nothing else, so a type-only assertion would pass for a guard that named the wrong coordinate.
///		The <c>with</c>-expression cases are the ones that matter most. A record's compiler-generated copy constructor assigns fields directly and does not re-run property initializers, so a guard written as a property initializer — the shape <c>PathSegment.cs:35</c> uses, whose bypass through <c>AreaOutlineGeometry.Translate</c> is the carried MASTER-170 gap — would leave <c>with { X = float.NaN }</c> unguarded while every constructor case above still passed.
/// </summary>
public class CanvasPointTests
{
	[Fact]
	public void Constructor_WithNaNX_ThrowsArgumentOutOfRangeExceptionNamingX()
	{
		var act = () => new CanvasPoint(float.NaN, 0f);

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("X");
	}

	[Fact]
	public void Constructor_WithNaNY_ThrowsArgumentOutOfRangeExceptionNamingY()
	{
		var act = () => new CanvasPoint(0f, float.NaN);

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("Y");
	}

	[Theory]
	[InlineData(float.PositiveInfinity)]
	[InlineData(float.NegativeInfinity)]
	public void Constructor_WithAnInfiniteX_ThrowsArgumentOutOfRangeExceptionNamingX(float x)
	{
		var act = () => new CanvasPoint(x, 0f);

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("X");
	}

	[Theory]
	[InlineData(float.PositiveInfinity)]
	[InlineData(float.NegativeInfinity)]
	public void Constructor_WithAnInfiniteY_ThrowsArgumentOutOfRangeExceptionNamingY(float y)
	{
		var act = () => new CanvasPoint(0f, y);

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("Y");
	}

	/// <summary>
	///		The rejected coordinate must travel on the exception. Without it the failure names the coordinate but not the value that broke it, and a caller reading the log cannot tell a <see cref="float.NaN"/> from an infinity.
	/// </summary>
	[Fact]
	public void Constructor_WithAnInfiniteX_CarriesTheRejectedValueAsActualValue()
	{
		var act = () => new CanvasPoint(float.PositiveInfinity, 0f);

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ActualValue.Should().Be(float.PositiveInfinity);
	}

	[Fact]
	public void Constructor_WithNaNY_CarriesTheRejectedValueAsActualValue()
	{
		var act = () => new CanvasPoint(0f, float.NaN);

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ActualValue.Should().Be(float.NaN);
	}

	[Theory]
	[InlineData(float.NaN)]
	[InlineData(float.PositiveInfinity)]
	[InlineData(float.NegativeInfinity)]
	public void With_SettingXNonFinite_ThrowsArgumentOutOfRangeExceptionNamingX(float x)
	{
		var existing = new CanvasPoint(12.5f, 7.25f);

		var act = () => existing with { X = x };

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("X");
	}

	[Theory]
	[InlineData(float.NaN)]
	[InlineData(float.PositiveInfinity)]
	[InlineData(float.NegativeInfinity)]
	public void With_SettingYNonFinite_ThrowsArgumentOutOfRangeExceptionNamingY(float y)
	{
		var existing = new CanvasPoint(12.5f, 7.25f);

		var act = () => existing with { Y = y };

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("Y");
	}

	/// <summary>
	///		Preserved behaviour, and the control on breadth. <see cref="float.MaxValue"/> and <see cref="float.MinValue"/> are finite, so a guard that tested magnitude rather than finiteness would fail here while passing every rejection case above.
	/// </summary>
	[Theory]
	[InlineData(0f, 0f)]
	[InlineData(12.5f, -7.25f)]
	[InlineData(float.MaxValue, float.MinValue)]
	[InlineData(float.Epsilon, -float.Epsilon)]
	public void Constructor_WithFiniteCoordinates_CarriesTheGivenValues(float x, float y)
	{
		var point = new CanvasPoint(x, y);

		point.X.Should().Be(x);
		point.Y.Should().Be(y);
	}

	[Fact]
	public void With_SettingAFiniteCoordinate_CarriesTheGivenValues()
	{
		var existing = new CanvasPoint(12.5f, 7.25f);

		var moved = existing with { X = float.MaxValue };

		moved.X.Should().Be(float.MaxValue);
		moved.Y.Should().Be(7.25f);
	}
}
