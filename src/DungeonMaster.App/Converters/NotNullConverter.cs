using System.Globalization;

namespace DungeonMaster.App.Converters;

/// <summary>
///		Converts a value to a <see cref="bool"/> indicating whether the value is present.
///		Returns <see langword="true"/> when the bound value is not null and is not the sentinel the
///		binding engine supplies for a path that did not resolve; otherwise <see langword="false"/>.
///		Typically used in XAML bindings to show or hide elements based on the presence of a value.
/// </summary>
public sealed class NotNullConverter : IValueConverter
{
	/// <summary>
	///		Converts a value to <see langword="true"/> if it is present; otherwise <see langword="false"/>.
	///		<see cref="BindableProperty.UnsetValue"/>, the sentinel the binding engine supplies when a
	///		binding path does not resolve, is a non-null object and is rejected explicitly, so a typo
	///		or rename in a bound path fails quietly rather than showing an empty element.
	/// </summary>
	/// <param name="value">
	///		The value produced by the binding source.
	/// </param>
	/// <param name="targetType">
	///		The type of the binding target property.
	/// </param>
	/// <param name="parameter">
	///		An optional converter parameter.
	/// </param>
	/// <param name="culture">
	///		The culture to use in the converter.
	/// </param>
	/// <returns>
	///		<see langword="true"/> if <paramref name="value"/> is not null and is not
	///		<see cref="BindableProperty.UnsetValue"/>; otherwise <see langword="false"/>.
	/// </returns>
	public object Convert(
		object? value,
		Type targetType,
		object? parameter,
		CultureInfo culture)
	{
		return value is not null && !ReferenceEquals(value, BindableProperty.UnsetValue);
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
		CultureInfo culture)
	{
		return null;
	}
}
