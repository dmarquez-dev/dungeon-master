using DungeonMaster.Tests.App.Theme;
using FluentAssertions;

namespace DungeonMaster.Tests.App.AsyncDiscipline;

/// <summary>
///		Rule G1 of the session contract: within every async void unit, the source text opens a
///		<c>try</c> at a line above its first <c>await</c>.
///		<para>
///		These tests READ application source text from disk and NEVER execute application code. The
///		test project targets <c>net10.0</c> and cannot reference the multi-targeted
///		<c>DungeonMaster.App</c> project (audit entry XCUT-002 / MASTER-026, Session 6 work), so
///		there is no route from here to running a page, a handler or a drawable.
///		</para>
///		<para>
///		What they are evidence of is therefore what the source STATES, not what happens at run time.
///		Nothing here shows that an unobserved exception cannot occur, that a crash path is closed, or
///		that any leak or race is absent; a guard present in the text can still be wrong at run time.
///		</para>
/// </summary>
public class AsyncVoidUnitsAreGuardedTests
{
	private const string MethodAwaitingWithNoTry =
		"public partial class SamplePage\n"
		+ "{\n"
		+ "\tprivate async void OnSaveClicked(object? sender, EventArgs e)\n"
		+ "\t{\n"
		+ "\t\tawait SaveAsync();\n"
		+ "\t}\n"
		+ "}\n";

	private const string MethodOpeningTryBeforeItsAwait =
		"public partial class SamplePage\n"
		+ "{\n"
		+ "\tprivate async void OnSaveClicked(object? sender, EventArgs e)\n"
		+ "\t{\n"
		+ "\t\ttry\n"
		+ "\t\t{\n"
		+ "\t\t\tawait SaveAsync();\n"
		+ "\t\t}\n"
		+ "\t\tcatch (Exception exception)\n"
		+ "\t\t{\n"
		+ "\t\t\t_viewModel.ErrorMessage = exception.Message;\n"
		+ "\t\t}\n"
		+ "\t}\n"
		+ "}\n";

	private const string LambdaAwaitingWithNoTry =
		"public partial class SamplePage\n"
		+ "{\n"
		+ "\tprivate void Wire(FloorSidebarItem item)\n"
		+ "\t{\n"
		+ "\t\titem.DeleteRequested += async (_, floor) =>\n"
		+ "\t\t{\n"
		+ "\t\t\tawait DeleteAsync(floor);\n"
		+ "\t\t};\n"
		+ "\t}\n"
		+ "}\n";

	private const string MethodWhoseOnlyAwaitIsCommentedOut =
		"public partial class SamplePage\n"
		+ "{\n"
		+ "\tprivate async void OnSaveClicked(object? sender, EventArgs e)\n"
		+ "\t{\n"
		+ "\t\t// await SaveAsync() is what this handler used to do.\n"
		+ "\t\tSave();\n"
		+ "\t}\n"
		+ "}\n";

	/// <summary>
	///		An async void method whose body awaits with no <c>try</c> above the await is reported.
	/// </summary>
	[Fact]
	public void ScanG1_WhenAnAsyncVoidMethodAwaitsWithNoPrecedingTry_ReportsTheDeclaration()
	{
		// Arrange
		var lines = AsyncDisciplineScanner.ToLines(MethodAwaitingWithNoTry);

		// Act
		var violations = AsyncDisciplineScanner.ScanG1(lines, "fixture");

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
	///		An async void method whose <c>try</c> sits above its first await is not reported.
	/// </summary>
	[Fact]
	public void ScanG1_WhenAnAsyncVoidMethodOpensTryBeforeItsFirstAwait_ReportsNothing()
	{
		// Arrange
		var lines = AsyncDisciplineScanner.ToLines(MethodOpeningTryBeforeItsAwait);

		// Act
		var violations = AsyncDisciplineScanner.ScanG1(lines, "fixture");

		// Assert
		violations
			.Should()
			.BeEmpty();
	}

	/// <summary>
	///		An async event-handler lambda is an async void unit too, and is reported on the same terms.
	/// </summary>
	[Fact]
	public void ScanG1_WhenAnAsyncEventHandlerLambdaAwaitsWithNoPrecedingTry_ReportsTheSubscription()
	{
		// Arrange
		var lines = AsyncDisciplineScanner.ToLines(LambdaAwaitingWithNoTry);

		// Act
		var violations = AsyncDisciplineScanner.ScanG1(lines, "fixture");

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
	///		A commented-out await is not an await: comment lines are excluded from the scan.
	/// </summary>
	[Fact]
	public void ScanG1_WhenTheOnlyAwaitInTheBodyIsInACommentLine_ReportsNothing()
	{
		// Arrange
		var lines = AsyncDisciplineScanner.ToLines(MethodWhoseOnlyAwaitIsCommentedOut);

		// Act
		var violations = AsyncDisciplineScanner.ScanG1(lines, "fixture");

		// Assert
		violations
			.Should()
			.BeEmpty();
	}

	/// <summary>
	///		Applies G1 to the application source text on disk. This asserts what the files SAY, and
	///		says nothing about what any handler does when it runs.
	/// </summary>
	[Fact]
	public void AppSource_EveryAsyncVoidUnit_StatesATryAboveItsFirstAwait()
	{
		// Arrange
		var paths = RepoPaths.AppSourceFiles("*.cs");

		// Act
		var violations = AsyncDisciplineScanner.ScanFiles(
			paths,
			AsyncDisciplineScanner.ScanG1);

		// Assert
		violations
			.Should()
			.BeEmpty(
				"G1 requires a try above the first await in every async void unit, but these units do not state one: {0}",
				string.Join(" | ", violations.Select(violation => violation.Describe())));
	}
}
