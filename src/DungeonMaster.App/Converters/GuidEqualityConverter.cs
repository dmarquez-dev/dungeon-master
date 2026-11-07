using System.Globalization;

namespace DungeonMaster.App.Converters;

/// <summary>
///		Converts two bound values to a <see cref="bool"/> indicating whether they are the same
///		<see cref="Guid"/>. Used by the sidebar's multi-bindings that derive a row's expanded or
///		selected state from the editor state, so that state is computed from its one owner rather
///		than stored on the row (MASTER-098).
/// </summary>
public sealed class GuidEqualityConverter : IMultiValueConverter
{
	/// <summary>
	///		Converts the bound values to <see langword="true"/> when there are exactly two and both
	///		are equal <see cref="Guid"/> values; otherwise <see langword="false"/>. A
	///		<see langword="null"/>, a <see cref="BindableProperty.UnsetValue"/> or any non-Guid value
	///		is never equal to anything, so an unresolved path or a cleared selection reads as not
	///		selected.
	/// </summary>
	/// <param name="values">
	///		The values produced by the child bindings, in declaration order.
	/// </param>
	/// <param name="targetType">
	///		The type of the binding target property.
	/// </param>
	/// <param name="parameter">
	///		An optional converter parameter; unused.
	/// </param>
	/// <param name="culture">
	///		The culture to use in the converter; unused.
	/// </param>
	/// <returns>
	///		<see langword="true"/> if both values are the same <see cref="Guid"/>; otherwise
	///		<see langword="false"/>.
	/// </returns>
	public object Convert(
		object[] values,
		Type targetType,
		object? parameter,
		CultureInfo culture)
	{
		return values is [Guid first, Guid second] && first == second;
	}

	/// <summary>
	///		Back-conversion is not meaningful for this converter, which is used only in one-way
	///		bindings, so it is inert rather than throwing.
	/// </summary>
	/// <returns>
	///		Always <see langword="null"/>.
	/// </returns>
	public object[]? ConvertBack(
		object? value,
		Type[] targetTypes,
		object? parameter,
		CultureInfo culture)
	{
		return null;
	}
}
