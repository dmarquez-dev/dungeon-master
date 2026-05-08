using System.Globalization;

namespace DungeonMaster.App.Converters;

/// <summary>
///		Converts a value to a <see cref="bool"/> indicating whether the value is not null.
///		Returns <see langword="true"/> when the bound value is not null; otherwise <see langword="false"/>.
///		Typically used in XAML bindings to show or hide elements based on the presence of a value.
/// </summary>
public sealed class NotNullConverter : IValueConverter
{
	/// <summary>
	///		Converts a value to <see langword="true"/> if it is not null; otherwise <see langword="false"/>.
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
	///		<see langword="true"/> if <paramref name="value"/> is not null; otherwise <see langword="false"/>.
	/// </returns>
	public object Convert(
		object? value,
		Type targetType,
		object? parameter,
		CultureInfo culture)
	{
		return value is not null;
	}

	/// <summary>
	///		Not supported. Throws <see cref="NotSupportedException"/>.
	/// </summary>
	/// <param name="value">
	///		The value that is produced by the binding target.
	/// </param>
	/// <param name="targetType">
	///		The type to convert to.
	/// </param>
	/// <param name="parameter">
	///		An optional converter parameter.
	/// </param>
	/// <param name="culture">
	///		The culture to use in the converter.
	/// </param>
	/// <returns>
	///		This method always throws.
	/// </returns>
	/// <exception cref="NotSupportedException">
	///		Always thrown; back-conversion is not supported.
	/// </exception>
	public object ConvertBack(
		object? value,
		Type targetType,
		object? parameter,
		CultureInfo culture)
	{
		throw new NotSupportedException($"{nameof(NotNullConverter)} does not support back-conversion.");
	}
}
