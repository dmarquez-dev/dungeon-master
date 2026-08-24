using System.Text.RegularExpressions;

namespace DungeonMaster.Tests.App.AsyncDiscipline;

/// <summary>
///		Line scanners for rules G1, G2, G3 and G4 of the session contract
///		<c>contracts/async-discipline-rules.md</c>.
///		<para>
///		Every rule is a function over a line list, never over a fixed path, so the same code runs
///		against an inline fixture and against a real application file. The scanners read source
///		TEXT and never execute application code: the test project targets <c>net10.0</c> and cannot
///		reference the multi-targeted <c>DungeonMaster.App</c> project (audit entry XCUT-002 /
///		MASTER-026, Session 6 work). What they report is therefore a statement about what the
///		source text says, and never a statement about what happens at run time.
///		</para>
/// </summary>
public static partial class AsyncDisciplineScanner
{
	/// <summary>
	///		Splits source text into lines, so a fixture written as one string can be scanned by the
	///		same functions that scan <see cref="File.ReadAllLines(string)"/> output.
	/// </summary>
	public static IReadOnlyList<string> ToLines(string source) =>
		source
			.Replace("\r\n", "\n", StringComparison.Ordinal)
			.Split('\n');

	/// <summary>
	///		Reports whether a line is a comment line, meaning its trimmed form starts with
	///		<c>//</c> — which also covers <c>///</c>. Comment lines are excluded from every rule.
	/// </summary>
	public static bool IsCommentLine(string line) =>
		line
			.TrimStart()
			.StartsWith("//", StringComparison.Ordinal);

	/// <summary>
	///		G1 — every async void unit opens a <c>try</c> before its first <c>await</c>.
	///		An async void unit is a non-comment method declaration matching the async-void pattern or
	///		a non-comment event-handler lambda subscription. A unit whose body contains no
	///		<c>await </c> line cannot leave an unobserved exception behind and is not reported.
	/// </summary>
	public static IReadOnlyList<Violation> ScanG1(IReadOnlyList<string> lines, string origin)
	{
		var violations = new List<Violation>();

		foreach (var unit in AsyncVoidUnits(lines))
		{
			var firstAwait = -1;
			var firstTry = -1;

			for (var index = unit.Start; index <= unit.End; index++)
			{
				if (IsCommentLine(lines[index]))
				{
					continue;
				}

				if (firstTry < 0
					&& lines[index]
						.Trim()
						.StartsWith("try", StringComparison.Ordinal))
				{
					firstTry = index;
				}

				if (firstAwait < 0
					&& lines[index].Contains("await ", StringComparison.Ordinal))
				{
					firstAwait = index;
				}
			}

			if (firstAwait < 0)
			{
				continue;
			}

			if (firstTry < 0 || firstTry >= firstAwait)
			{
				violations.Add(
					new Violation(
						origin,
						unit.Start + 1,
						lines[unit.Start]));
			}
		}

		return violations;
	}

	/// <summary>
	///		G2 — no non-comment line has a trimmed form starting with the discard-assignment token
	///		<c>_ = </c>.
	///		<para>
	///		The contract states the token as <c>_ =</c> and counts the population with
	///		<c>grep -rn "_ = "</c>, which has a trailing space. The two disagree: a switch expression
	///		discard arm, <c>_ =&gt; value</c>, also starts with <c>_ =</c> and the application source
	///		states six of them, none of which discards a task and none of which can be rewritten. The
	///		grep is followed and the arrow form is excluded, so this rule reports the sixteen task
	///		discards the contract enumerates and nothing else.
	///		</para>
	/// </summary>
	public static IReadOnlyList<Violation> ScanG2(IReadOnlyList<string> lines, string origin) =>
		NonCommentLines(lines)
			.Where(entry =>
				entry.Text
					.Trim()
					.StartsWith("_ =", StringComparison.Ordinal)
				&& !entry.Text
					.Trim()
					.StartsWith("_ =>", StringComparison.Ordinal))
			.Select(entry =>
				new Violation(
					origin,
					entry.Index + 1,
					entry.Text))
			.ToList();

	/// <summary>
	///		G3 — every async void METHOD DECLARATION either carries <c> override </c> on its
	///		declaration line or names a method matching <c>^On[A-Z]</c>. Event-handler lambdas are
	///		not method declarations and are outside this rule.
	/// </summary>
	public static IReadOnlyList<Violation> ScanG3(IReadOnlyList<string> lines, string origin)
	{
		var violations = new List<Violation>();

		foreach (var entry in NonCommentLines(lines))
		{
			if (!AsyncVoidDeclaration().IsMatch(entry.Text))
			{
				continue;
			}

			if (entry.Text.Contains(" override ", StringComparison.Ordinal))
			{
				continue;
			}

			var name = AsyncVoidMethodName()
				.Match(entry.Text)
				.Groups["name"]
				.Value;

			if (name.Length > 0 && HandlerName().IsMatch(name))
			{
				continue;
			}

			violations.Add(
				new Violation(
					origin,
					entry.Index + 1,
					entry.Text));
		}

		return violations;
	}

	/// <summary>
	///		G4 — no non-comment line contains <c>GetAwaiter().GetResult()</c> or <c>.Wait()</c>.
	///		<c>.Result</c> is deliberately outside this rule: <c>NavigationExtensions.cs</c> reads
	///		<c>IPopupResult</c>.<c>Result</c>, an ordinary property a source scan cannot tell apart
	///		from <c>Task.Result</c>.
	/// </summary>
	public static IReadOnlyList<Violation> ScanG4(IReadOnlyList<string> lines, string origin) =>
		NonCommentLines(lines)
			.Where(entry =>
				entry.Text.Contains("GetAwaiter().GetResult()", StringComparison.Ordinal)
				|| entry.Text.Contains(".Wait()", StringComparison.Ordinal))
			.Select(entry =>
				new Violation(
					origin,
					entry.Index + 1,
					entry.Text))
			.ToList();

	/// <summary>
	///		Applies one rule to every supplied file, reading each file text from disk.
	/// </summary>
	public static IReadOnlyList<Violation> ScanFiles(
		IReadOnlyList<string> paths,
		Func<IReadOnlyList<string>, string, IReadOnlyList<Violation>> rule) =>
		paths
			.SelectMany(path =>
				rule(
					File.ReadAllLines(path),
					path))
			.ToList();

	/// <summary>
	///		Locates every async void unit and the extent of its body. A unit starts at its
	///		declaration line and ends at the first following line whose leading-tab count is less
	///		than or equal to the declaration line count and whose trimmed form is <c>}</c> or
	///		<c>};</c>.
	/// </summary>
	public static IReadOnlyList<AsyncVoidUnit> AsyncVoidUnits(IReadOnlyList<string> lines)
	{
		var units = new List<AsyncVoidUnit>();

		for (var index = 0; index < lines.Count; index++)
		{
			var line = lines[index];

			if (IsCommentLine(line))
			{
				continue;
			}

			AsyncVoidUnitKind? kind = null;

			if (AsyncVoidDeclaration().IsMatch(line))
			{
				kind = AsyncVoidUnitKind.MethodDeclaration;
			}
			else if (AsyncLambdaSubscription().IsMatch(line))
			{
				kind = AsyncVoidUnitKind.EventHandlerLambda;
			}

			if (kind is null)
			{
				continue;
			}

			units.Add(
				new AsyncVoidUnit(
					index,
					FindBodyEnd(lines, index),
					kind.Value));
		}

		return units;
	}

	private static int FindBodyEnd(IReadOnlyList<string> lines, int start)
	{
		var depth = LeadingTabs(lines[start]);

		for (var index = start + 1; index < lines.Count; index++)
		{
			var trimmed = lines[index].Trim();

			if (LeadingTabs(lines[index]) <= depth
				&& (trimmed == "}" || trimmed == "};"))
			{
				return index;
			}
		}

		return lines.Count - 1;
	}

	private static int LeadingTabs(string line)
	{
		var count = 0;

		while (count < line.Length && line[count] == '\t')
		{
			count++;
		}

		return count;
	}

	private static IEnumerable<(int Index, string Text)> NonCommentLines(IReadOnlyList<string> lines) =>
		lines
			.Select((text, index) => (Index: index, Text: text))
			.Where(entry => !IsCommentLine(entry.Text));

	[GeneratedRegex(@"\basync void\b")]
	private static partial Regex AsyncVoidDeclaration();

	[GeneratedRegex(@"\+= async\b")]
	private static partial Regex AsyncLambdaSubscription();

	[GeneratedRegex(@"\basync void\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)")]
	private static partial Regex AsyncVoidMethodName();

	[GeneratedRegex("^On[A-Z]")]
	private static partial Regex HandlerName();
}

/// <summary>
///		The kind of async void unit G1 found: a method declaration, or an event-handler lambda.
/// </summary>
public enum AsyncVoidUnitKind
{
	/// <summary>
	///		A non-comment line declaring an async void method.
	/// </summary>
	MethodDeclaration,

	/// <summary>
	///		A non-comment line subscribing an async lambda to an event.
	/// </summary>
	EventHandlerLambda,
}

/// <summary>
///		An async void unit and the zero-based line range of its body.
/// </summary>
public sealed record AsyncVoidUnit(int Start, int End, AsyncVoidUnitKind Kind);

/// <summary>
///		One line of source text that a rule reports, with the file or fixture it came from.
/// </summary>
public sealed record Violation(string Origin, int LineNumber, string Text)
{
	/// <summary>
	///		Renders the violation as origin, line number and trimmed text for a failure message.
	/// </summary>
	public string Describe() =>
		$"{Origin}:{LineNumber}: {Text.Trim()}";
}
