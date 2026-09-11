using System.Globalization;

namespace DungeonMaster.App.Converters;

/// <summary>
///		Converts an integer value to a <see cref="bool"/> indicating whether it is greater than zero.
///		Supports an optional <c>Invert</c> parameter to negate the result.
///		Typically used in XAML bindings to show or hide elements based on a positive count.
/// </summary>
public sealed class GreaterThanZeroConverter : IValueConverter
{
	/// <summary>
	///		Converts an integer value to <see langword="true"/> if it is greater than zero; otherwise <see langword="false"/>.
	///		When <paramref name="parameter"/> is <c>"Invert"</c> the result is negated.
	///		Any value that is not an <see cref="int"/> — including <see langword="null"/>, a
	///		<see cref="double"/>, a <see cref="long"/>, a <see cref="string"/> and
	///		<c>BindableProperty.UnsetValue</c>, the sentinel the binding engine supplies for a path
	///		that did not resolve — returns <see langword="false"/> before the <c>Invert</c>
	///		negation is considered, so an inverted binding on an unresolved path hides its element
	///		rather than showing it (MASTER-167).
	/// </summary>
	/// <param name="value">
	///		The value produced by the binding source. Expected to be an <see cref="int"/>; anything
	///		else converts to <see langword="false"/> regardless of <paramref name="parameter"/>.
	/// </param>
	/// <param name="targetType">
	///		The type of the binding target property.
	/// </param>
	/// <param name="parameter">
	///		An optional converter parameter. Pass <c>"Invert"</c> to negate the result.
	/// </param>
	/// <param name="culture">
	///		The culture to use in the converter.
	/// </param>
	/// <returns>
	///		<see langword="true"/> if <paramref name="value"/> is an integer greater than zero; otherwise <see langword="false"/>.
	///		The result is negated when <paramref name="parameter"/> is <c>"Invert"</c>, but only for
	///		an <see cref="int"/>; a non-<see cref="int"/> always returns <see langword="false"/>.
	/// </returns>
	public object Convert(
		object? value,
		Type targetType,
		object? parameter,
		CultureInfo culture)
	{
		if (value is not int n)
		{
			return false;
		}

		bool result = n > 0;

		if (parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase))
		{
			result = !result;
		}

		return result;
	}

	/// <summary>
	///		Back-conversion is not meaningful for this converter, which is used only in one-way
	///		bindings, so it is inert rather than throwing.
	/// </summary>
	/// <returns>
	///		Always <see langword="null"/>.
	/// </returns>
	public object? ConvertBack(
		object? value,
		Type targetType,
		object? parameter,
		CultureInfo culture) =>
		null;
}
