using DungeonMaster.Tests.App.Theme;
using FluentAssertions;

namespace DungeonMaster.Tests.App.AsyncDiscipline;

/// <summary>
///		Rule G3 of the session contract: every async void METHOD DECLARATION in the application
///		source either carries <c>override</c> on its declaration line or names a method matching
///		<c>^On[A-Z]</c>, so async void appears only where a signature forces it.
///		<para>
///		These tests READ application source text from disk and NEVER execute application code. The
///		test project targets <c>net10.0</c> and cannot reference the multi-targeted
///		<c>DungeonMaster.App</c> project (audit entry XCUT-002 / MASTER-026, Session 6 work).
///		</para>
///		<para>
///		They are therefore evidence about what the source STATES, not about runtime behaviour, and
///		nothing here shows that a leak, a race or a crash path is absent.
///		</para>
/// </summary>
public class AsyncVoidIsSignatureForcedTests
{
	private const string SourceWithAnUnforcedAsyncVoid =
		"public partial class SamplePage\n"
		+ "{\n"
		+ "\tprivate async void PlaceObjectAsync(Guid floorId)\n"
		+ "\t{\n"
		+ "\t\tawait PlaceAsync(floorId);\n"
		+ "\t}\n"
		+ "}\n";

	private const string SourceWithOnlyForcedAsyncVoid =
		"public partial class SamplePage\n"
		+ "{\n"
		+ "\tprotected override async void OnAppearing()\n"
		+ "\t{\n"
		+ "\t\tawait LoadAsync();\n"
		+ "\t}\n"
		+ "\n"
		+ "\tprivate async void OnAddFloorClicked(object? sender, EventArgs e)\n"
		+ "\t{\n"
		+ "\t\tawait AddFloorAsync();\n"
		+ "\t}\n"
		+ "}\n";

	private const string SourceWhoseOnlyAsyncVoidIsCommentedOut =
		"public partial class SamplePage\n"
		+ "{\n"
		+ "\t// private async void PlaceObjectAsync(Guid floorId) was converted to a Task.\n"
		+ "\tprivate async Task PlaceObjectAsync(Guid floorId)\n"
		+ "\t{\n"
		+ "\t\tawait PlaceAsync(floorId);\n"
		+ "\t}\n"
		+ "}\n";

	/// <summary>
	///		An async void method that is neither an override nor an <c>On...</c> handler is reported.
	/// </summary>
	[Fact]
	public void ScanG3ReportsTheDeclarationOfAnAsyncVoidMethodThatIsNeitherAnOverrideNorAnOnHandler()
	{
		// Arrange
		var lines = AsyncDisciplineScanner.ToLines(SourceWithAnUnforcedAsyncVoid);

		// Act
		var violations = AsyncDisciplineScanner.ScanG3(lines, "fixture");

		// Assert
		violations
			.Should()
			.ContainSingle()
			.Which
			.LineNumber
			.Should()
			.Be(3);
	}

	/// <summary>
	///		An override and an <c>On...</c> handler are both allowed and are not reported.
	/// </summary>
	[Fact]
	public void ScanG3ReportsNothingWhenEveryAsyncVoidMethodIsAnOverrideOrAnOnHandler()
	{
		// Arrange
		var lines = AsyncDisciplineScanner.ToLines(SourceWithOnlyForcedAsyncVoid);

		// Act
		var violations = AsyncDisciplineScanner.ScanG3(lines, "fixture");

		// Assert
		violations
			.Should()
			.BeEmpty();
	}

	/// <summary>
	///		An async void declaration inside a comment line is excluded from the scan.
	/// </summary>
	[Fact]
	public void ScanG3ReportsNothingWhenTheOnlyAsyncVoidDeclarationIsInACommentLine()
	{
		// Arrange
		var lines = AsyncDisciplineScanner.ToLines(SourceWhoseOnlyAsyncVoidIsCommentedOut);

		// Act
		var violations = AsyncDisciplineScanner.ScanG3(lines, "fixture");

		// Assert
		violations
			.Should()
			.BeEmpty();
	}

	/// <summary>
	///		Applies G3 to the application source text on disk. This asserts what the files SAY.
	/// </summary>
	[Fact]
	public void EveryAsyncVoidMethodDeclaredInAppSourceIsAnOverrideOrAnOnHandler()
	{
		// Arrange
		var paths = RepoPaths.AppSourceFiles("*.cs");

		// Act
		var violations = AsyncDisciplineScanner.ScanFiles(
			paths,
			AsyncDisciplineScanner.ScanG3);

		// Assert
		violations
			.Should()
			.BeEmpty(
				"G3 allows async void only where the signature forces it, but these declarations state neither an override nor an On-handler name: {0}",
				string.Join(" | ", violations.Select(violation => violation.Describe())));
	}
}
