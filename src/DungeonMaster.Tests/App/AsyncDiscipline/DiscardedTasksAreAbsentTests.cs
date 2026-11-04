using DungeonMaster.Tests.App.Theme;
using FluentAssertions;

namespace DungeonMaster.Tests.App.AsyncDiscipline;

/// <summary>
///		Rule G2 of the session contract: no non-comment line in the application source has a trimmed
///		form starting with the discard-assignment token <c>_ = </c>, the form the contract counts its
///		population with. A switch expression discard arm, <c>_ =&gt; value</c>, is excluded — see
///		<see cref="AsyncDisciplineScanner.ScanG2"/>.
///		<para>
///		These tests READ application source text from disk and NEVER execute application code. The
///		test project targets <c>net10.0</c> and cannot reference the multi-targeted
///		<c>DungeonMaster.App</c> project (audit entry XCUT-002 / MASTER-026, Session 6 work).
///		</para>
///		<para>
///		They are therefore evidence about what the source STATES, not about runtime behaviour.
///		A source with no discard token still proves nothing about whether a faulted task is
///		observed at run time, and nothing here shows a leak, a race or a crash path is absent.
///		</para>
/// </summary>
public class DiscardedTasksAreAbsentTests
{
	private const string SourceDiscardingATask =
		"public partial class SamplePage\n"
		+ "{\n"
		+ "\tprivate void OnRotateClicked(object? sender, EventArgs e)\n"
		+ "\t{\n"
		+ "\t\t_ = RotateObjectAsync(clockwise: true);\n"
		+ "\t}\n"
		+ "}\n";

	private const string SourceAwaitingTheTask =
		"public partial class SamplePage\n"
		+ "{\n"
		+ "\tprivate async Task RotateAsync()\n"
		+ "\t{\n"
		+ "\t\tawait RotateObjectAsync(clockwise: true);\n"
		+ "\t}\n"
		+ "}\n";

	private const string SourceWhoseOnlyDiscardIsCommentedOut =
		"public partial class SamplePage\n"
		+ "{\n"
		+ "\tprivate async Task RotateAsync()\n"
		+ "\t{\n"
		+ "\t\t// _ = RotateObjectAsync(clockwise: true); was the old fire-and-forget call.\n"
		+ "\t\tawait RotateObjectAsync(clockwise: true);\n"
		+ "\t}\n"
		+ "}\n";

	private const string SourceWithASwitchExpressionDiscardArm =
		"public static class ObjectKindInfo\n"
		+ "{\n"
		+ "\tpublic static float Scale(ObjectKind kind) =>\n"
		+ "\t\tkind switch\n"
		+ "\t\t{\n"
		+ "\t\t\tObjectKind.Door => 0.5f,\n"
		+ "\t\t\t_ => 1.0f\n"
		+ "\t\t};\n"
		+ "}\n";

	/// <summary>
	///		A line whose trimmed form starts with the discard token is reported.
	/// </summary>
	[Fact]
	public void ScanG2ReportsALineThatStartsWithADiscardAssignment()
	{
		// Arrange
		var lines = AsyncDisciplineScanner.ToLines(SourceDiscardingATask);

		// Act
		var violations = AsyncDisciplineScanner.ScanG2(lines, "fixture");

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
	///		A source that awaits instead of discarding is not reported.
	/// </summary>
	[Fact]
	public void ScanG2ReportsNothingWhenNoLineStartsWithADiscardAssignment()
	{
		// Arrange
		var lines = AsyncDisciplineScanner.ToLines(SourceAwaitingTheTask);

		// Act
		var violations = AsyncDisciplineScanner.ScanG2(lines, "fixture");

		// Assert
		violations
			.Should()
			.BeEmpty();
	}

	/// <summary>
	///		A discard token inside a comment line is excluded from the scan.
	/// </summary>
	[Fact]
	public void ScanG2ReportsNothingWhenTheOnlyDiscardAssignmentIsInACommentLine()
	{
		// Arrange
		var lines = AsyncDisciplineScanner.ToLines(SourceWhoseOnlyDiscardIsCommentedOut);

		// Act
		var violations = AsyncDisciplineScanner.ScanG2(lines, "fixture");

		// Assert
		violations
			.Should()
			.BeEmpty();
	}

	/// <summary>
	///		A switch expression discard ARM is not a discarded task and is not reported. The
	///		application source states six of these, none of which can be rewritten, so a rule that
	///		reported them could never go green.
	/// </summary>
	[Fact]
	public void ScanG2ReportsNothingForASwitchExpressionDiscardArm()
	{
		// Arrange
		var lines = AsyncDisciplineScanner.ToLines(SourceWithASwitchExpressionDiscardArm);

		// Act
		var violations = AsyncDisciplineScanner.ScanG2(lines, "fixture");

		// Assert
		violations
			.Should()
			.BeEmpty();
	}

	/// <summary>
	///		Applies G2 to the application source text on disk. This asserts what the files SAY.
	/// </summary>
	[Fact]
	public void NoAppSourceLineStatesADiscardedTask()
	{
		// Arrange
		var paths = RepoPaths.AppSourceFiles("*.cs");

		// Act
		var violations = AsyncDisciplineScanner.ScanFiles(
			paths,
			AsyncDisciplineScanner.ScanG2);

		// Assert
		violations
			.Should()
			.BeEmpty(
				"G2 bans the discard token in this layer, but these lines state one: {0}",
				string.Join(" | ", violations.Select(violation => violation.Describe())));
	}
}
