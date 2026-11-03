using DungeonMaster.Tests.App.Theme;
using FluentAssertions;

namespace DungeonMaster.Tests.App.AsyncDiscipline;

/// <summary>
///		STRUCTURAL evidence about source text: <c>DungeonDetailPage.OnMapCanvasHandlerChanged</c> states no
///		statement between its opening brace and its <c>try</c>, so the scroll-zoom detach and attach sit
///		inside the guarded region rather than above it.
///		<para>
///		These tests READ <c>DungeonDetailPage.xaml.cs</c> from disk through <see cref="RepoPaths"/> and NEVER
///		execute application code, for the reason <see cref="AsyncVoidUnitsAreGuardedTests"/> records: this
///		project cannot reference the multi-targeted <c>DungeonMaster.App</c> project. They are NOT proof
///		that the handler is guarded at run time. Nothing makes <c>AttachScrollZoom</c> throw, so no runtime
///		test and no human can observe the guard; what is checked here is only where the text places the
///		calls.
///		</para>
///		<para>
///		The <c>try</c> line is matched as a whole trimmed line, never as a substring: the comment above it
///		contains the word "try", and a substring match would be satisfied by that prose.
///		</para>
/// </summary>
public class MapCanvasHandlerChangedIsGuardedTests
{
	private const string DeclarationPrefix = "private async void OnMapCanvasHandlerChanged(";

	/// <summary>
	///		Every line between the method's opening brace and its <c>try</c> line is blank or a comment.
	/// </summary>
	[Fact]
	public void StructuralSourceScan_OfOnMapCanvasHandlerChanged_StatesNoStatementAboveItsTry()
	{
		// Arrange
		var method = LocateMethod();

		// Act
		var statementsAboveTry = Enumerable
			.Range(method.OpenBrace + 1, method.TryLine - method.OpenBrace - 1)
			.Where(index =>
				!string.IsNullOrWhiteSpace(method.Lines[index])
				&& !AsyncDisciplineScanner.IsCommentLine(method.Lines[index]))
			.Select(index =>
				$"line {index + 1}: {method.Lines[index].Trim()}")
			.ToList();

		// Assert
		statementsAboveTry
			.Should()
			.BeEmpty(
				"OnMapCanvasHandlerChanged is async void, so every statement it runs belongs inside its try, but these lines stand above it: {0}",
				string.Join(" | ", statementsAboveTry));
	}

	/// <summary>
	///		Control: the method and its <c>try</c> line are found, and the image preload is stated after that
	///		<c>try</c> line within the method's body. This holds whether or not anything stands above the
	///		<c>try</c>, so it shows the scan above is reading the right method.
	/// </summary>
	[Fact]
	public void StructuralSourceScan_OfOnMapCanvasHandlerChanged_FindsPreloadObjectImagesAsyncAfterItsTry()
	{
		// Arrange
		var method = LocateMethod();

		// Act
		var preloadLines = Enumerable
			.Range(method.TryLine + 1, method.BodyEnd - method.TryLine - 1)
			.Where(index =>
				!AsyncDisciplineScanner.IsCommentLine(method.Lines[index])
				&& method.Lines[index].Contains("PreloadObjectImagesAsync", StringComparison.Ordinal))
			.ToList();

		// Assert
		preloadLines
			.Should()
			.NotBeEmpty(
				"the try opened at line {0} of OnMapCanvasHandlerChanged should cover the object-image preload",
				method.TryLine + 1);
	}

	/// <summary>
	///		Reads the page source and finds the method's declaration, opening brace, <c>try</c> line and closing
	///		brace, failing loudly when any of them is missing. All indices are zero-based.
	/// </summary>
	private static (IReadOnlyList<string> Lines, int OpenBrace, int TryLine, int BodyEnd) LocateMethod()
	{
		var path = Path.Combine(
			RepoPaths.AppRoot,
			"Views",
			"DungeonDetailPage",
			"DungeonDetailPage.xaml.cs");

		File
			.Exists(path)
			.Should()
			.BeTrue("the page source must exist at {0}", path);

		var lines = File.ReadAllLines(path);

		var declarations = Enumerable
			.Range(0, lines.Length)
			.Where(index =>
				lines[index].Trim().StartsWith(DeclarationPrefix, StringComparison.Ordinal))
			.ToList();

		declarations
			.Should()
			.ContainSingle("exactly one declaration starting '{0}' must exist in {1}", DeclarationPrefix, path);

		var openBrace = declarations[0] + 1;

		lines[openBrace]
			.Trim()
			.Should()
			.Be("{", "the line after the declaration must open the method body");

		var braceIndent = lines[openBrace][..lines[openBrace].IndexOf('{', StringComparison.Ordinal)];

		var bodyEnd = Enumerable
			.Range(openBrace + 1, lines.Length - openBrace - 1)
			.FirstOrDefault(
				index =>
					lines[index] == braceIndent + "}",
				-1);

		bodyEnd
			.Should()
			.BeGreaterThan(openBrace, "the method body opened at line {0} must close", openBrace + 1);

		var tryLine = Enumerable
			.Range(openBrace + 1, bodyEnd - openBrace - 1)
			.FirstOrDefault(
				index =>
					lines[index].Trim() == "try",
				-1);

		tryLine
			.Should()
			.BeGreaterThan(openBrace, "OnMapCanvasHandlerChanged must contain a line whose trimmed text is exactly 'try'");

		return (lines, openBrace, tryLine, bodyEnd);
	}
}
