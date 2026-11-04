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

	// The second error channel: a child of the dungeon-detail composition root hands its failure
	// to the root, which owns the one ErrorMessage the page binds.
	private const string ContextFailureRouting = "context.ReportFailure(";

	// The composition root and its three children. Named rather than globbed: this fact must fail
	// loudly if one of them is renamed or removed, not quietly scan whatever it happens to find.
	private static readonly string[] DetailViewModelFileNames =
	[
		"DungeonDetailViewModel.cs",
		"DungeonHeaderViewModel.cs",
		"FloorListViewModel.cs",
		"MapCanvasViewModel.cs",
	];

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
	///		Asserts that every dispatch across the dungeon-detail view models captures the result it
	///		is handed and branches on its failure. MASTER-001: a discarded result is a rejected write
	///		the user is never told about, and the unconditional reload that follows repaints the
	///		pre-edit values over the edit that was lost.
	/// </summary>
	/// <remarks>
	///		The editing surface was split out of <c>DungeonDetailViewModel</c> into a composition
	///		root plus three children, so the dispatches this fact judges now live across four files
	///		rather than one. The scan follows them; the fourteen it counts are the same fourteen.
	/// </remarks>
	[Fact]
	public void EveryDungeonDetailViewModelDispatchBranchesOnItsResult()
	{
		var discarded = new List<string>();
		var unbranched = new List<string>();
		var dispatchTotal = 0;

		foreach (var source in DetailViewModelSources())
		{
			var lines = File.ReadAllLines(source.Path);

			// The dispatch count is read from the file, never hard-coded, so this fact measures the
			// files it is given rather than a number that was true when it was written.
			var dispatchLines = lines
				.Select((line, index) =>
					(Line: line, Number: index + 1))
				.Where(entry =>
					entry.Line.Contains(DispatchToken, StringComparison.Ordinal))
				.Select(entry =>
					entry.Number)
				.ToList();

			dispatchTotal += dispatchLines.Count;

			var captured = dispatchLines
				.Where(number =>
					CapturedDispatchPattern.IsMatch(lines[number - 1].TrimStart()))
				.ToList();

			discarded.AddRange(
				dispatchLines
					.Except(captured)
					.Select(number =>
						$"{source.Name}:{number}"));

			unbranched.AddRange(
				captured
					.Where(number =>
						!IsBranchedOn(
							lines,
							number))
					.Select(number =>
						$"{source.Name}:{number}"));
		}

		discarded
			.Should()
			.BeEmpty(
				"all {0} dispatches across the dungeon-detail view models must capture their result, but {1} discard it at {2}",
				dispatchTotal,
				discarded.Count,
				string.Join(", ", discarded));

		unbranched
			.Should()
			.BeEmpty(
				"every captured result must be followed by an IsFailure branch, but {0} is not, at {1}",
				unbranched.Count,
				string.Join(", ", unbranched));

		dispatchTotal
			.Should()
			.Be(
				14,
				"the dungeon-detail view models dispatch fourteen commands between them; a change to that number is a change this fact must be re-read against");
	}

	/// <summary>
	///		Asserts that every view model which dispatches a command also owns a channel on which to
	///		report a rejected one.
	/// </summary>
	/// <remarks>
	///		There are two legitimate channels. A view model either declares
	///		<c>ErrorMessage</c> itself, or it is a child of the dungeon-detail composition root and
	///		routes every rejection through <c>IDungeonEditingContext.ReportFailure</c> to the one
	///		<c>ErrorMessage</c> the root declares — the seam that keeps a single error channel on
	///		screen instead of four competing ones. The second channel is held to a count: a child
	///		must route at least as many failures as it dispatches commands, so removing one routing
	///		site is caught here even though the <c>IsFailure</c> branch around it survives.
	/// </remarks>
	[Fact]
	public void EveryViewModelWithADispatchHasAnErrorMessageChannel()
	{
		var offenders = new List<string>();

		foreach (var source in ViewModelSources())
		{
			var dispatches = Occurrences(
				source.Text,
				DispatchToken);

			if (dispatches == 0 || source.Text.Contains(ErrorMessageDeclaration, StringComparison.Ordinal))
			{
				continue;
			}

			var routings = Occurrences(
				source.Text,
				ContextFailureRouting);

			if (routings >= dispatches)
			{
				continue;
			}

			offenders.Add(
				$"{source.Name}: {dispatches} dispatches, {routings} routed through {ContextFailureRouting}");
		}

		offenders
			.Should()
			.BeEmpty(
				"a view model that dispatches a command must declare '{0}', or route every rejection through '{1}' to the composition root that declares it",
				ErrorMessageDeclaration,
				ContextFailureRouting);
	}

	/// <summary>
	///		Asserts that no view model builds its own failure text. Every message routes through
	///		<c>ResultMessage.ForFailure</c>, which is the single mapping that reads
	///		<c>ErrorCategory</c> — MASTER-045's consumer half.
	/// </summary>
	[Fact]
	public void EveryViewModelErrorAssignmentIsNullOrRoutesThroughResultMessage()
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

	private static IReadOnlyList<(string Name, string Path)> DetailViewModelSources() =>
		DetailViewModelFileNames
			.Select(name =>
				(Name: name, Path: Path.Combine(
					RepoPaths.AppRoot,
					"ViewModels",
					name)))
			.ToList();

	private static int Occurrences(
		string text,
		string token)
	{
		var count = 0;
		var index = text.IndexOf(token, StringComparison.Ordinal);

		while (index >= 0)
		{
			count++;
			index = text.IndexOf(
				token,
				index + token.Length,
				StringComparison.Ordinal);
		}

		return count;
	}

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
