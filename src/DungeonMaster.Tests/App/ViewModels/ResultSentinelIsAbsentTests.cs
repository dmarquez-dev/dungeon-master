using System.Text.RegularExpressions;
using DungeonMaster.Tests.App.Theme;
using FluentAssertions;

namespace DungeonMaster.Tests.App.ViewModels;

/// <summary>
///		Source-scanning fact that pins the ABSENCE of a defect: a view model must never try to
///		detect a rejected dispatch by comparing the result's <c>Value</c> against
///		<c>Guid.Empty</c> or <c>default</c> (MASTER-044 / CORE-005).
///		<para>
///		Such a comparison is unreachable dead code in the failure direction — the
///		<c>IsFailure</c> branch above it has already returned — and it is wrong in the success
///		direction, because <c>default(Guid)</c> is a legal success value. <c>IsSuccess</c> and
///		<c>IsFailure</c> are the only failure signals a result carries.
///		</para>
///		<para>
///		The test project targets <c>net10.0</c> and cannot reference the multi-targeted
///		application project, so this reads the view-model sources from disk through
///		<see cref="RepoPaths"/>, exactly as <see cref="FailureIsSurfacedTests"/> does.
///		</para>
/// </summary>
public class ResultSentinelIsAbsentTests
{
	// The must-match case: the exact sentinel form removed from MapCanvasViewModel by this
	// session. The predicate is exercised against it here, on constructed input, so that the
	// scan below is known to be able to fail rather than merely observed to pass.
	private const string RemovedSentinelForm = "if (addResult.Value == Guid.Empty)";

	// NEAR-MISS 1. A result value passed as an argument, with no comparison. Not hypothetical:
	// MapCanvasViewModel still contains this line after the sentinel was removed, so a predicate
	// that flagged it could never pass against the fixed tree.
	private const string BareResultValueRead = "addResult.Value,";

	// NEAR-MISS 2. A Guid.Empty comparison on something that is not a result value.
	// DungeonDetailViewModel contains this line verbatim; a search for Guid.Empty alone would
	// flag it.
	private const string NonResultGuidEmptyComparison = "if (_dungeonId != Guid.Empty)";

	// NEAR-MISS 3. ".Value" only as a substring of an unrelated identifier. This is why a naive
	// ".Value" substring search over these four files returns more hits than there are reads.
	private const string ValueAsSubstringOfAnIdentifier = "using DungeonMaster.Domain.ValueObjects;";

	// The composition root and its three children. Named rather than globbed: this fact must fail
	// loudly if one of them is renamed or removed, not quietly scan whatever it happens to find.
	private static readonly string[] DetailViewModelFileNames =
	[
		"DungeonDetailViewModel.cs",
		"DungeonHeaderViewModel.cs",
		"FloorListViewModel.cs",
		"MapCanvasViewModel.cs",
	];

	// A comparison between a ".Value" member access and Guid.Empty or default, in either operand
	// order. "\.Value\b" is what keeps ".ValueObjects" out: there is no word boundary between the
	// "e" and the "O". The comparison operator is what keeps a bare read out.
	private static readonly Regex ResultValueSentinelPattern =
		new(
			@"\b[A-Za-z_][A-Za-z0-9_]*\.Value\b\s*[!=]=\s*(?:Guid\.Empty\b|default\b)"
			+ @"|(?:Guid\.Empty\b|default\b)\s*[!=]=\s*[A-Za-z_][A-Za-z0-9_]*\.Value\b",
			RegexOptions.Compiled);

	/// <summary>
	///		Asserts that none of the four dungeon-detail view models compares a dispatch result's
	///		<c>Value</c> against <c>Guid.Empty</c> or <c>default</c>.
	/// </summary>
	/// <remarks>
	///		The read is guarded against vacuity: <see cref="RepoPaths"/> does not throw when a path
	///		resolves to nothing, so the count of sources actually read is asserted before anything
	///		is asserted about their contents.
	/// </remarks>
	[Fact]
	public void NoDungeonDetailViewModelComparesAResultValueAgainstGuidEmptyOrDefault()
	{
		var sources = DetailViewModelSources();

		// The composition root and its three children must all have been read before their contents
		// are judged. RepoPaths does not throw when a path resolves to nothing, so a vacuous scan
		// has to fail against the fact that depended on it — here, and not silently pass.
		sources
			.Should()
			.HaveCount(4);

		var findings = sources
			.SelectMany(source =>
				SentinelFindings(
					source.Name,
					source.Text))
			.ToList();

		findings
			.Should()
			.BeEmpty(
				"a rejected dispatch is detected by IsFailure, never by comparing the result's Value against Guid.Empty or default, but {0} such comparison(s) remain at {1}",
				findings.Count,
				string.Join(", ", findings));
	}

	/// <summary>
	///		Asserts that the predicate FLAGS the sentinel form this session removed. Without this,
	///		the scan above could pass because the predicate matches nothing at all.
	/// </summary>
	[Fact]
	public void TheDetectorFlagsTheRemovedSentinelForm()
	{
		SentinelFindings(
				"constructed.cs",
				$"\t\t{RemovedSentinelForm}")
			.Should()
			.ContainSingle(
				"'{0}' is the comparison this fact exists to keep out",
				RemovedSentinelForm);
	}

	/// <summary>
	///		Asserts that the predicate does NOT flag a result value that is merely read and passed
	///		on. This form survives in MapCanvasViewModel and is correct there.
	/// </summary>
	[Fact]
	public void TheDetectorDoesNotFlagABareResultValueRead()
	{
		SentinelFindings(
				"constructed.cs",
				$"\t\t\t\t{BareResultValueRead}")
			.Should()
			.BeEmpty(
				"'{0}' reads the value without comparing it, which is the legitimate use",
				BareResultValueRead);
	}

	/// <summary>
	///		Asserts that the predicate does NOT flag a <c>Guid.Empty</c> comparison against
	///		something that is not a result value.
	/// </summary>
	[Fact]
	public void TheDetectorDoesNotFlagAGuidEmptyComparisonOnANonResultValue()
	{
		SentinelFindings(
				"constructed.cs",
				$"\t\t{NonResultGuidEmptyComparison}")
			.Should()
			.BeEmpty(
				"'{0}' compares a plain field, not a result value; a scan for Guid.Empty alone would flag it",
				NonResultGuidEmptyComparison);
	}

	/// <summary>
	///		Asserts that the predicate does NOT flag a line where <c>.Value</c> appears only as a
	///		substring of a longer identifier.
	/// </summary>
	[Fact]
	public void TheDetectorDoesNotFlagValueAsASubstringOfAnUnrelatedIdentifier()
	{
		SentinelFindings(
				"constructed.cs",
				ValueAsSubstringOfAnIdentifier)
			.Should()
			.BeEmpty(
				"'{0}' contains no member access named Value at all",
				ValueAsSubstringOfAnIdentifier);
	}

	/// <summary>
	///		The detector: a predicate over text, reachable without touching the filesystem so that it
	///		can be exercised against constructed input as well as against the real checkout.
	/// </summary>
	/// <param name="fileName">
	///		The name reported alongside each finding.
	/// </param>
	/// <param name="text">
	///		The source text to scan. Newlines are honoured; findings are reported per line.
	/// </param>
	internal static IReadOnlyList<string> SentinelFindings(
		string fileName,
		string text)
	{
		return text
			.Split('\n')
			.Select((line, index) =>
				(Line: line, Number: index + 1))
			.Where(entry =>
				ResultValueSentinelPattern.IsMatch(entry.Line))
			.Select(entry =>
				$"{fileName}:{entry.Number}: {entry.Line.Trim()}")
			.ToList();
	}

	private static IReadOnlyList<(string Name, string Text)> DetailViewModelSources() =>
		DetailViewModelFileNames
			.Select(name =>
				(Name: name, Text: File.ReadAllText(
					Path.Combine(
						RepoPaths.AppRoot,
						"ViewModels",
						name))))
			.ToList();
}
