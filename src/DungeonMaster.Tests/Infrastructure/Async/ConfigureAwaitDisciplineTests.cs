using System.Runtime.CompilerServices;
using FluentAssertions;

namespace DungeonMaster.Tests.Infrastructure.Async;

/// <summary>
///		Guards the deliberate decision recorded as MASTER-153 / INFRA-015: every <c>await</c> in the
///		<c>DungeonMaster.Infrastructure</c> library code carries <c>.ConfigureAwait(false)</c>.
///		<para>
///			This test reads <b>source text</b>. It is not a behavioural test, because <c>ConfigureAwait</c> has no
///			behavioural effect a test can assert here: the xUnit host installs no
///			<see cref="SynchronizationContext"/>, so a continuation resumes on the thread pool whether or not the
///			call is present. Scanning the source is the only way to observe the convention.
///		</para>
///		<para>
///			<b>This test is the only enforcement of the rule that will exist.</b> CA2007 is not enabled by default
///			in .NET 10, and this repository's <c>AnalysisMode</c> is <c>Recommended</c>, which does not include it.
///			No analyzer, no warning and no <c>TreatWarningsAsErrors</c> failure will catch a future <c>await</c>
///			written without <c>.ConfigureAwait(false)</c> in these files. The build does not have your back here.
///			If this test is deleted, the convention silently stops being a convention.
///		</para>
/// </summary>
public class ConfigureAwaitDisciplineTests
{
	/// <summary>
	///		The Infrastructure source files in scope, relative to the <c>src</c> directory.
	/// </summary>
	private static readonly string[] ScopedFiles =
	[
		"DungeonMaster.Infrastructure/InfrastructureServiceCollectionExtensions.cs",
		"DungeonMaster.Infrastructure/Persistence/UnitOfWorkFactory.cs",
		"DungeonMaster.Infrastructure/Persistence/Repositories/DungeonReadRepository.cs",
		"DungeonMaster.Infrastructure/Persistence/Repositories/DungeonWriteRepository.cs",
	];

	[Fact]
	public void ScopedFiles_EveryAwait_IsConfiguredFalse()
	{
		// Arrange
		var sourceRoot = LocateSourceRoot();

		List<string> unconfigured = [];

		// Act
		foreach (var relativePath in ScopedFiles)
		{
			var fullPath = Path.Combine(
				sourceRoot,
				relativePath.Replace('/', Path.DirectorySeparatorChar));

			File.Exists(fullPath)
				.Should()
				.BeTrue($"the scoped file '{relativePath}' must exist for this guard to mean anything");

			var lines = File.ReadAllLines(fullPath);

			for (var index = 0; index < lines.Length; index++)
			{
				var line = lines[index];

				if (IsComment(line))
				{
					continue;
				}

				if (!line.Contains("await ", StringComparison.Ordinal))
				{
					continue;
				}

				if (line.Contains(".ConfigureAwait(false)", StringComparison.Ordinal))
				{
					continue;
				}

				unconfigured.Add($"{relativePath}:{index + 1}: {line.Trim()}");
			}
		}

		// Assert
		unconfigured
			.Should()
			.BeEmpty(
				"every await in the Infrastructure layer must carry .ConfigureAwait(false), and nothing in the "
					+ "build enforces it. Unconfigured sites:"
					+ Environment.NewLine
					+ string.Join(Environment.NewLine, unconfigured));
	}

	private static bool IsComment(string line)
	{
		var trimmed = line.TrimStart();

		return trimmed.StartsWith("//", StringComparison.Ordinal)
			|| trimmed.StartsWith("/*", StringComparison.Ordinal)
			|| trimmed.StartsWith('*');
	}

	/// <summary>
	///		Resolves the repository's <c>src</c> directory from this file's own compile-time path, which sits at
	///		<c>src/DungeonMaster.Tests/Infrastructure/Async/</c>.
	/// </summary>
	private static string LocateSourceRoot([CallerFilePath] string callerFilePath = "")
	{
		var directory = Path.GetDirectoryName(callerFilePath)!;

		return Path.GetFullPath(Path.Combine(directory, "..", "..", ".."));
	}
}
