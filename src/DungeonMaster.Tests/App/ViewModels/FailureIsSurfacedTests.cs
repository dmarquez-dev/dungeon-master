using System.Text.RegularExpressions;
using DungeonMaster.Tests.App.Theme;
using FluentAssertions;

namespace DungeonMaster.Tests.App.ViewModels;

/// <summary>
///		Source-scanning facts that pin the failure channel of the application's view models.
///		The test project targets <c>net10.0</c> and cannot reference the multi-targeted
///		application project, so these read the view-model sources from disk through
///		<see cref="RepoPaths"/>, exactly as the <c>Tests/App/Theme</c> suite does. They are
///		supporting evidence for MASTER-001 and are explicitly not a substitute for the manual
///		observation that closes it.
/// </summary>
public class FailureIsSurfacedTests
{
	private const string DispatchToken = "dispatcher.SendAsync";

	private const string ErrorMessageDeclaration = "public partial string? ErrorMessage { get; set; }";

	// A dispatch whose result is captured. The variable name is captured rather than assumed so
	// the branch below is checked against the name actually used at that call site.
	private static readonly Regex CapturedDispatchPattern =
		new(
			@"^var\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*await\s+dispatcher\.SendAsync",
			RegexOptions.Compiled);

	// An assignment to ErrorMessage, capturing the whole right-hand side. "=(?!=)" keeps an
	// equality comparison out; the [ObservableProperty] declaration carries no "=" at all.
	private static readonly Regex ErrorMessageAssignmentPattern =
		new(
			@"ErrorMessage\s*=(?!=)\s*(?<source>[^;]+);",
			RegexOptions.Compiled);

	/// <summary>
	///		Asserts that every dispatch in <c>DungeonDetailViewModel</c> captures the result it is
	///		handed and branches on its failure. MASTER-001: a discarded result is a rejected write
	///		the user is never told about, and the unconditional reload that follows repaints the
	///		pre-edit values over the edit that was lost.
	/// </summary>
	[Fact]
	public void EveryDispatchInDungeonDetailViewModel_BranchesOnTheResult()
	{
		var lines = File.ReadAllLines(DungeonDetailViewModelPath);

		// The dispatch count is read from the file, never hard-coded, so this fact measures the
		// file it is given rather than a number that was true when it was written.
		var dispatchLines = lines
			.Select((line, index) =>
				(Line: line, Number: index + 1))
			.Where(entry =>
				entry.Line.Contains(DispatchToken, StringComparison.Ordinal))
			.Select(entry =>
				entry.Number)
			.ToList();

		var discarded = dispatchLines
			.Where(number =>
				!CapturedDispatchPattern.IsMatch(lines[number - 1].TrimStart()))
			.ToList();

		var unbranched = dispatchLines
			.Except(discarded)
			.Where(number =>
				!IsBranchedOn(
					lines,
					number))
			.ToList();

		discarded
			.Should()
			.BeEmpty(
				"all {0} dispatches in DungeonDetailViewModel must capture their result, but {1} discard it at lines {2}",
				dispatchLines.Count,
				discarded.Count,
				string.Join(", ", discarded));

		unbranched
			.Should()
			.BeEmpty(
				"every captured result must be followed by an IsFailure branch, but {0} is not, at lines {1}",
				unbranched.Count,
				string.Join(", ", unbranched));

		dispatchLines
			.Should()
			.HaveCount(
				14,
				"DungeonDetailViewModel dispatches fourteen commands; a change to that number is a change this fact must be re-read against");
	}

	/// <summary>
	///		Asserts that every view model which dispatches a command also owns a channel on which to
	///		report a rejected one.
	/// </summary>
	[Fact]
	public void EveryViewModelWithADispatch_DeclaresAnErrorMessageProperty()
	{
		var offenders = ViewModelSources()
			.Where(source =>
				source.Text.Contains(DispatchToken, StringComparison.Ordinal))
			.Where(source =>
				!source.Text.Contains(ErrorMessageDeclaration, StringComparison.Ordinal))
			.Select(source =>
				source.Name)
			.ToList();

		offenders
			.Should()
			.BeEmpty(
				"a view model that dispatches a command must declare '{0}' so a rejection has somewhere to be shown",
				ErrorMessageDeclaration);
	}

	/// <summary>
	///		Asserts that no view model builds its own failure text. Every message routes through
	///		<c>ResultMessage.ForFailure</c>, which is the single mapping that reads
	///		<c>ErrorCategory</c> — MASTER-045's consumer half.
	/// </summary>
	[Fact]
	public void EveryViewModelErrorAssignment_RoutesThroughResultMessage()
	{
		var offenders = new List<string>();

		foreach (var source in ViewModelSources())
		{
			foreach (Match match in ErrorMessageAssignmentPattern.Matches(source.Text))
			{
				var assigned = match
					.Groups["source"]
					.Value
					.Trim();

				if (assigned == "null" || assigned.StartsWith("ResultMessage.ForFailure(", StringComparison.Ordinal))
				{
					continue;
				}

				offenders.Add($"{source.Name}: ErrorMessage = {assigned}");
			}
		}

		offenders
			.Should()
			.BeEmpty("ErrorMessage is assigned either null or ResultMessage.ForFailure(result), never a hand-built string");
	}

	private static string DungeonDetailViewModelPath =>
		Path.Combine(
			RepoPaths.AppRoot,
			"ViewModels",
			"DungeonDetailViewModel.cs");

	private static IReadOnlyList<(string Name, string Text)> ViewModelSources()
	{
		var separator = Path.DirectorySeparatorChar;

		return RepoPaths
			.AppSourceFiles("*ViewModel.cs")
			.Where(path =>
				path.Contains($"{separator}ViewModels{separator}", StringComparison.Ordinal))
			.Select(path =>
				(Name: Path.GetFileName(path), Text: File.ReadAllText(path)))
			.ToList();
	}

	private static bool IsBranchedOn(
		IReadOnlyList<string> lines,
		int dispatchLineNumber)
	{
		var name = CapturedDispatchPattern
			.Match(lines[dispatchLineNumber - 1].TrimStart())
			.Groups["name"]
			.Value;

		var branch = $"{name}.IsFailure";

		for (var index = dispatchLineNumber; index < lines.Count; index++)
		{
			var line = lines[index];

			// One tab followed by a closing brace ends a member: the class body sits one tab
			// deep, so nothing nested inside a method can close at that indent.
			if (line == "\t}" || line.Contains(DispatchToken, StringComparison.Ordinal))
			{
				return false;
			}

			if (line.Contains(branch, StringComparison.Ordinal))
			{
				return true;
			}
		}

		return false;
	}
}
