using System.Globalization;

using DungeonMaster.App.Converters;

namespace DungeonMaster.App.Tests.Converters;

/// <summary>
///		Executes <see cref="NotNullConverter"/> directly. The converter is a plain class with no
///		MAUI application host behind it, so it can be constructed and called here (MASTER-007,
///		MASTER-034).
/// </summary>
public class NotNullConverterTests
{
	private static readonly NotNullConverter Converter = new();

	/// <summary>
	///		The sentinel the binding engine supplies for a path that did not resolve is a non-null
	///		object, so a bare null check reports it as present. It must convert to
	///		<see langword="false"/>.
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
	///		Regression cover: an ordinary non-null value still converts to <see langword="true"/>.
	/// </summary>
	[Fact]
	public void ConvertReturnsTrueForAnOrdinaryNonNullValue()
	{
		var result = Converter.Convert(
			"a message",
			typeof(bool),
			null,
			CultureInfo.InvariantCulture);

		Assert.Equal(true, result);
	}

	/// <summary>
	///		Regression cover: <see langword="null"/> still converts to <see langword="false"/>.
	/// </summary>
	[Fact]
	public void ConvertReturnsFalseForNull()
	{
		var result = Converter.Convert(
			null,
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
