using System.Globalization;

using DungeonMaster.App.Converters;

namespace DungeonMaster.App.Tests.Converters;

/// <summary>
///		Executes <see cref="GreaterThanZeroConverter"/> directly. UIKIT-004 asserts this converter
///		does not share <see cref="NotNullConverter"/>'s sentinel defect because its
///		<c>value is int n</c> test rejects the sentinel; that assertion was unverified until now
///		(MASTER-007, MASTER-034). It holds only for the plain direction: under the <c>Invert</c>
///		parameter the negation ran on the rejected answer and returned <see langword="true"/>,
///		which is MASTER-167 and is now guarded and covered here.
/// </summary>
public class GreaterThanZeroConverterTests
{
	private static readonly GreaterThanZeroConverter Converter = new();

	/// <summary>
	///		The sentinel the binding engine supplies for a path that did not resolve is not an
	///		<see cref="int"/>, so it must convert to <see langword="false"/>.
	/// </summary>
	[Fact]
	public void ConvertReturnsFalseForTheUnresolvedBindingSentinel()
	{
		var result = Converter.Convert(
			BindableProperty.UnsetValue,
			typeof(bool),
			null,
			CultureInfo.InvariantCulture);

		Assert.Equal(false, result);
	}

	/// <summary>
	///		Regression cover: a positive <see cref="int"/> with no parameter converts to
	///		<see langword="true"/>.
	/// </summary>
	[Fact]
	public void ConvertReturnsTrueForAPositiveIntWithNoParameter()
	{
		var result = Converter.Convert(
			5,
			typeof(bool),
			null,
			CultureInfo.InvariantCulture);

		Assert.Equal(true, result);
	}

	/// <summary>
	///		Regression cover: zero with no parameter converts to <see langword="false"/>.
	/// </summary>
	[Fact]
	public void ConvertReturnsFalseForZeroWithNoParameter()
	{
		var result = Converter.Convert(
			0,
			typeof(bool),
			null,
			CultureInfo.InvariantCulture);

		Assert.Equal(false, result);
	}

	/// <summary>
	///		Regression cover: a negative <see cref="int"/> with no parameter converts to
	///		<see langword="false"/>.
	/// </summary>
	[Fact]
	public void ConvertReturnsFalseForANegativeIntWithNoParameter()
	{
		var result = Converter.Convert(
			-1,
			typeof(bool),
			null,
			CultureInfo.InvariantCulture);

		Assert.Equal(false, result);
	}

	/// <summary>
	///		Regression cover: a positive <see cref="int"/> under <c>Invert</c> converts to
	///		<see langword="false"/>.
	/// </summary>
	[Fact]
	public void ConvertReturnsFalseForAPositiveIntWhenInverted()
	{
		var result = Converter.Convert(
			5,
			typeof(bool),
			"Invert",
			CultureInfo.InvariantCulture);

		Assert.Equal(false, result);
	}

	/// <summary>
	///		Regression cover: zero under <c>Invert</c> converts to <see langword="true"/>. This is
	///		the case the two live overlay bindings rely on.
	/// </summary>
	[Fact]
	public void ConvertReturnsTrueForZeroWhenInverted()
	{
		var result = Converter.Convert(
			0,
			typeof(bool),
			"Invert",
			CultureInfo.InvariantCulture);

		Assert.Equal(true, result);
	}

	/// <summary>
	///		Regression cover: a negative <see cref="int"/> under <c>Invert</c> converts to
	///		<see langword="true"/>.
	/// </summary>
	[Fact]
	public void ConvertReturnsTrueForANegativeIntWhenInverted()
	{
		var result = Converter.Convert(
			-1,
			typeof(bool),
			"Invert",
			CultureInfo.InvariantCulture);

		Assert.Equal(true, result);
	}

	/// <summary>
	///		Regression cover: the <c>Invert</c> parameter is matched with
	///		<see cref="StringComparison.OrdinalIgnoreCase"/>, so an all-lower-case spelling still
	///		negates.
	/// </summary>
	[Fact]
	public void ConvertMatchesTheInvertParameterInLowerCase()
	{
		var result = Converter.Convert(
			0,
			typeof(bool),
			"invert",
			CultureInfo.InvariantCulture);

		Assert.Equal(true, result);
	}

	/// <summary>
	///		Regression cover: an all-upper-case spelling of the <c>Invert</c> parameter still
	///		negates.
	/// </summary>
	[Fact]
	public void ConvertMatchesTheInvertParameterInUpperCase()
	{
		var result = Converter.Convert(
			0,
			typeof(bool),
			"INVERT",
			CultureInfo.InvariantCulture);

		Assert.Equal(true, result);
	}

	/// <summary>
	///		Regression cover: a string parameter that is not <c>Invert</c> is ignored rather than
	///		treated as a negation request.
	/// </summary>
	[Fact]
	public void ConvertIgnoresAnUnrecognisedStringParameter()
	{
		var result = Converter.Convert(
			5,
			typeof(bool),
			"Something",
			CultureInfo.InvariantCulture);

		Assert.Equal(true, result);
	}

	/// <summary>
	///		Regression cover: a non-string parameter is ignored rather than throwing.
	/// </summary>
	[Fact]
	public void ConvertIgnoresANonStringParameter()
	{
		var result = Converter.Convert(
			0,
			typeof(bool),
			42,
			CultureInfo.InvariantCulture);

		Assert.Equal(false, result);
	}

	/// <summary>
	///		MASTER-167. The sentinel the binding engine supplies for a path that did not resolve
	///		must convert to <see langword="false"/> under <c>Invert</c> as well. Before the
	///		non-<see cref="int"/> guard the negation ran on the "unresolved" answer and returned
	///		<see langword="true"/>, showing the overlay it was written to hide.
	/// </summary>
	[Fact]
	public void ConvertReturnsFalseForTheUnresolvedBindingSentinelWhenInverted()
	{
		var result = Converter.Convert(
			BindableProperty.UnsetValue,
			typeof(bool),
			"Invert",
			CultureInfo.InvariantCulture);

		Assert.Equal(false, result);
	}

	/// <summary>
	///		MASTER-167. <see langword="null"/> is not an <see cref="int"/>, so it converts to
	///		<see langword="false"/> under <c>Invert</c> too.
	/// </summary>
	[Fact]
	public void ConvertReturnsFalseForNullWhenInverted()
	{
		var result = Converter.Convert(
			null,
			typeof(bool),
			"Invert",
			CultureInfo.InvariantCulture);

		Assert.Equal(false, result);
	}

	/// <summary>
	///		MASTER-167. A <see cref="double"/> is not an <see cref="int"/>, so it converts to
	///		<see langword="false"/> under <c>Invert</c> rather than being negated.
	/// </summary>
	[Fact]
	public void ConvertReturnsFalseForADoubleWhenInverted()
	{
		var result = Converter.Convert(
			1.0d,
			typeof(bool),
			"Invert",
			CultureInfo.InvariantCulture);

		Assert.Equal(false, result);
	}

	/// <summary>
	///		MASTER-167. A <see cref="long"/> is not an <see cref="int"/>, so it converts to
	///		<see langword="false"/> under <c>Invert</c> rather than being negated.
	/// </summary>
	[Fact]
	public void ConvertReturnsFalseForALongWhenInverted()
	{
		var result = Converter.Convert(
			1L,
			typeof(bool),
			"Invert",
			CultureInfo.InvariantCulture);

		Assert.Equal(false, result);
	}

	/// <summary>
	///		MASTER-167. A <see cref="string"/> is not an <see cref="int"/>, so it converts to
	///		<see langword="false"/> under <c>Invert</c> rather than being negated.
	/// </summary>
	[Fact]
	public void ConvertReturnsFalseForAStringWhenInverted()
	{
		var result = Converter.Convert(
			"5",
			typeof(bool),
			"Invert",
			CultureInfo.InvariantCulture);

		Assert.Equal(false, result);
	}

	/// <summary>
	///		Back-conversion is inert rather than throwing, so a <c>TwoWay</c> binding attached by
	///		mistake is a no-op instead of a UI-thread exception.
	/// </summary>
	[Fact]
	public void ConvertBackReturnsNullRatherThanThrowing()
	{
		var result = Converter.ConvertBack(
			true,
			typeof(object),
			null,
			CultureInfo.InvariantCulture);

		Assert.Null(result);
	}

	/// <summary>
	///		Back-conversion stays inert for every shape of input, including the values and the
	///		<c>Invert</c> parameter the forward direction treats specially.
	/// </summary>
	[Fact]
	public void ConvertBackReturnsNullForEveryInput()
	{
		Assert.Null(Converter.ConvertBack(null, typeof(object), null, CultureInfo.InvariantCulture));
		Assert.Null(Converter.ConvertBack(false, typeof(object), "Invert", CultureInfo.InvariantCulture));
		Assert.Null(Converter.ConvertBack(5, typeof(object), "Invert", CultureInfo.InvariantCulture));
		Assert.Null(Converter.ConvertBack("5", typeof(object), 42, CultureInfo.InvariantCulture));
		Assert.Null(Converter.ConvertBack(
			BindableProperty.UnsetValue,
			typeof(object),
			"Invert",
			CultureInfo.InvariantCulture));
	}
}
