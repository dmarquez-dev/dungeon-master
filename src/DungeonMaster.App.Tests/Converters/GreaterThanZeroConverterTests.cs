using System.Globalization;

using DungeonMaster.App.Converters;

namespace DungeonMaster.App.Tests.Converters;

/// <summary>
///		Executes <see cref="GreaterThanZeroConverter"/> directly. UIKIT-004 asserts this converter
///		does not share <see cref="NotNullConverter"/>'s sentinel defect because its
///		<c>value is int n</c> test rejects the sentinel; that assertion was unverified until now
///		(MASTER-007, MASTER-034).
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
}
