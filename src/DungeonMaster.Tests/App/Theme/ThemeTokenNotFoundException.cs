namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Thrown when a theme token reader cannot find the key it was asked for, or finds it
///		declared by a different element than the one the reader requires.
///		<para>
///		A token reader is the one shape where "return nothing" is not expressible — a
///		<c>double</c> has no empty — so <c>ThemeSource</c>'s three token readers throw this
///		instead of asserting from inside a helper. A typed exception names its own cause; a
///		<c>.Should()</c> in Arrange names whichever test happened to call the helper first.
///		</para>
/// </summary>
internal sealed class ThemeTokenNotFoundException : Exception
{
	/// <summary>
	///		Initialises a new instance of the <see cref="ThemeTokenNotFoundException"/> class.
	/// </summary>
	public ThemeTokenNotFoundException()
	{
	}

	/// <summary>
	///		Initialises a new instance of the <see cref="ThemeTokenNotFoundException"/> class.
	/// </summary>
	/// <param name="message">
	///		A message naming the file, the key, and the element type that was expected.
	/// </param>
	public ThemeTokenNotFoundException(string message)
		: base(message)
	{
	}

	/// <summary>
	///		Initialises a new instance of the <see cref="ThemeTokenNotFoundException"/> class.
	/// </summary>
	/// <param name="message">
	///		A message naming the file, the key, and the element type that was expected.
	/// </param>
	/// <param name="innerException">
	///		The exception that caused this one.
	/// </param>
	public ThemeTokenNotFoundException(
		string message,
		Exception innerException)
		: base(
			message,
			innerException)
	{
	}
}
