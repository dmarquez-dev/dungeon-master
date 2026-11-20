using DungeonMaster.Tests.App.Theme;
using FluentAssertions;

namespace DungeonMaster.Tests.App.AsyncDiscipline;

/// <summary>
///		Rule G4 of the session contract: no non-comment line in the application source contains
///		<c>GetAwaiter().GetResult()</c> or <c>.Wait()</c>.
///		<para>
///		These tests READ application source text from disk and NEVER execute application code. The
///		test project targets <c>net10.0</c> and cannot reference the multi-targeted
///		<c>DungeonMaster.App</c> project (audit entry XCUT-002 / MASTER-026, Session 6 work), so no
///		deadlock and no blocked UI thread is ever observed here.
///		</para>
///		<para>
///		They are therefore evidence about what the source STATES, not about runtime behaviour, and
///		nothing here shows that a leak, a race or a crash path is absent.
///		</para>
/// </summary>
public class SyncOverAsyncIsAbsentTests
{
	private const string SourceBlockingOnATask =
		"public sealed class SampleDrawable\n"
		+ "{\n"
		+ "\tprivate Stream Open(string path)\n"
		+ "\t{\n"
		+ "\t\treturn FileSystem.OpenAppPackageFileAsync(path).GetAwaiter().GetResult();\n"
		+ "\t}\n"
		+ "}\n";

	private const string SourceWaitingOnATask =
		"public sealed class SampleDrawable\n"
		+ "{\n"
		+ "\tprivate void Flush(Task pending)\n"
		+ "\t{\n"
		+ "\t\tpending.Wait();\n"
		+ "\t}\n"
		+ "}\n";

	private const string SourceAwaitingInstead =
		"public sealed class SampleDrawable\n"
		+ "{\n"
		+ "\tprivate async Task<Stream> OpenAsync(string path)\n"
		+ "\t{\n"
		+ "\t\treturn await FileSystem.OpenAppPackageFileAsync(path);\n"
		+ "\t}\n"
		+ "}\n";

	private const string SourceWhoseOnlyBlockingCallIsCommentedOut =
		"public sealed class SampleDrawable\n"
		+ "{\n"
		+ "\tprivate async Task<Stream> OpenAsync(string path)\n"
		+ "\t{\n"
		+ "\t\t// TODO(macOS): FileSystem.OpenAppPackageFileAsync().GetAwaiter().GetResult() is sync-over-async.\n"
		+ "\t\treturn await FileSystem.OpenAppPackageFileAsync(path);\n"
		+ "\t}\n"
		+ "}\n";

	private const string SourceReadingAPopupResultProperty =
		"public static class NavigationExtensions\n"
		+ "{\n"
		+ "\tpublic static bool Confirm(IPopupResult<bool> result)\n"
		+ "\t{\n"
		+ "\t\treturn result.Result;\n"
		+ "\t}\n"
		+ "}\n";

	/// <summary>
	///		A line blocking on a task through GetAwaiter and GetResult is reported.
	/// </summary>
	[Fact]
	public void ScanG4ReportsALineThatBlocksOnATaskWithGetAwaiterGetResult()
	{
		// Arrange
		var lines = AsyncDisciplineScanner.ToLines(SourceBlockingOnATask);

		// Act
		var violations = AsyncDisciplineScanner.ScanG4(lines, "fixture");

		// Assert
		violations
			.Should()
			.ContainSingle()
			.Which
			.LineNumber
			.Should()
			.Be(5);
	}

	/// <summary>
	///		A line blocking on a task through Wait is reported.
	/// </summary>
	[Fact]
	public void ScanG4ReportsALineThatBlocksOnATaskWithWait()
	{
		// Arrange
		var lines = AsyncDisciplineScanner.ToLines(SourceWaitingOnATask);

		// Act
		var violations = AsyncDisciplineScanner.ScanG4(lines, "fixture");

		// Assert
		violations
			.Should()
			.ContainSingle()
			.Which
			.LineNumber
			.Should()
			.Be(5);
	}

	/// <summary>
	///		A source that awaits instead of blocking is not reported.
	/// </summary>
	[Fact]
	public void ScanG4ReportsNothingWhenNoLineBlocksOnATask()
	{
		// Arrange
		var lines = AsyncDisciplineScanner.ToLines(SourceAwaitingInstead);

		// Act
		var violations = AsyncDisciplineScanner.ScanG4(lines, "fixture");

		// Assert
		violations
			.Should()
			.BeEmpty();
	}

	/// <summary>
	///		A blocking call inside a comment line is excluded from the scan. This is the shape of
	///		<c>Graphics/FloorMapDrawable.cs</c> line 291, which would otherwise be a false positive.
	/// </summary>
	[Fact]
	public void ScanG4ReportsNothingWhenTheOnlyBlockingCallIsInACommentLine()
	{
		// Arrange
		var lines = AsyncDisciplineScanner.ToLines(SourceWhoseOnlyBlockingCallIsCommentedOut);

		// Act
		var violations = AsyncDisciplineScanner.ScanG4(lines, "fixture");

		// Assert
		violations
			.Should()
			.BeEmpty();
	}

	/// <summary>
	///		Reading an ordinary <c>Result</c> property is outside G4, which bans neither the token nor
	///		the property. This pins the deliberate exclusion recorded in the contract.
	/// </summary>
	[Fact]
	public void ScanG4ReportsNothingForALineReadingAnOrdinaryResultProperty()
	{
		// Arrange
		var lines = AsyncDisciplineScanner.ToLines(SourceReadingAPopupResultProperty);

		// Act
		var violations = AsyncDisciplineScanner.ScanG4(lines, "fixture");

		// Assert
		violations
			.Should()
			.BeEmpty();
	}

	/// <summary>
	///		Applies G4 to the application source text on disk. This asserts what the files SAY.
	/// </summary>
	[Fact]
	public void NoAppSourceLineStatesASyncOverAsyncCall()
	{
		// Arrange
		var paths = RepoPaths.AppSourceFiles("*.cs");

		// Act
		var violations = AsyncDisciplineScanner.ScanFiles(
			paths,
			AsyncDisciplineScanner.ScanG4);

		// Assert
		violations
			.Should()
			.BeEmpty(
				"G4 bans blocking on a task in this layer, but these lines state one: {0}",
				string.Join(" | ", violations.Select(violation => violation.Describe())));
	}
}
