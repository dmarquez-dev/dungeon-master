using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Covers the repository-root walk that every theme fact is built on (MASTER-008 / TEST-003).
///		<para>
///		The defect this class exists for is an ABSENCE: the walk used to accept the first ancestor
///		carrying a <c>.git</c> entry without ever checking that <c>src/DungeonMaster.App</c> sat
///		beneath it, so a checkout nested inside an outer repository resolved to the outer one and
///		every path composed from the result pointed somewhere else.
///		</para>
///		<para>
///		Every layout below is CONSTRUCTED in a temporary directory. Asserting against the real
///		checkout would make the fact depend on an ambient property of the machine it runs on, which
///		is the very thing TEST-003 objects to, and it could not build the rejecting layout at all.
///		</para>
/// </summary>
public sealed class RepoPathsTests : IDisposable
{
	private readonly List<DirectoryInfo> _temporaryRoots = [];

	[Fact]
	public void WalkAcceptsAnAncestorWithAGitDirectoryAndTheAppProjectBeneathIt()
	{
		var root = CreateDirectory(
			NewTemporaryRoot().FullName,
			"clone");

		CreateDirectory(
			root,
			".git");

		CreateAppProject(root);

		var start = CreateBuildOutputDirectory(root);

		var result = RepoPaths.FindRootFrom(
			start,
			RepoPaths.GitEntryExists,
			RepoPaths.AppProjectExists);

		result
			.Should()
			.Be(
				root,
				"a normal clone marks its root with a .git DIRECTORY, and the App project sits beneath it");
	}

	[Fact]
	public void WalkAcceptsAnAncestorWithAGitFileAndTheAppProjectBeneathIt()
	{
		var root = CreateDirectory(
			NewTemporaryRoot().FullName,
			"worktree");

		CreateGitFile(root);
		CreateAppProject(root);

		var start = CreateBuildOutputDirectory(root);

		var result = RepoPaths.FindRootFrom(
			start,
			RepoPaths.GitEntryExists,
			RepoPaths.AppProjectExists);

		result
			.Should()
			.Be(
				root,
				"a linked worktree marks its root with a .git FILE, and that is the layout this "
				+ "repository is actively developed in; narrowing the test back to Directory.Exists "
				+ "takes out every theme fact at once");
	}

	[Fact]
	public void WalkSkipsAnAncestorWithAGitEntryButNoAppProjectAndReturnsTheOuterRoot()
	{
		var outer = CreateDirectory(
			NewTemporaryRoot().FullName,
			"outer");

		CreateDirectory(
			outer,
			".git");

		CreateAppProject(outer);

		var inner = CreateDirectory(
			CreateDirectory(
				outer,
				"vendor"),
			"inner");

		CreateGitFile(inner);

		var start = CreateBuildOutputDirectory(inner);

		var result = RepoPaths.FindRootFrom(
			start,
			RepoPaths.GitEntryExists,
			RepoPaths.AppProjectExists);

		result
			.Should()
			.Be(
				outer,
				"the inner directory carries a .git entry but no src/DungeonMaster.App, so the walk "
				+ "must keep going up; stopping there is exactly the defect MASTER-008 names");
	}

	[Fact]
	public void WalkThrowsNamingTheStartingDirectoryAndBothConditionsWhenNoAncestorSatisfiesBoth()
	{
		var tree = NewTemporaryRoot();

		var start = CreateDirectory(
			CreateDirectory(
				tree.FullName,
				"archive"),
			"src");

		// The predicates are the production ones, fenced to the constructed tree. Without the fence
		// the walk would climb past the temporary directory into the real filesystem and the fact
		// would depend on what happens to sit above %TEMP% on this machine.
		var act = () => RepoPaths.FindRootFrom(
			start,
			Within(tree, RepoPaths.GitEntryExists),
			Within(tree, RepoPaths.AppProjectExists));

		var message = act
			.Should()
			.Throw<InvalidOperationException>(
				"a source archive or a tarball-fetch CI job has no .git at all, and the walk must say "
				+ "so rather than running off the top of the drive")
			.Which
			.Message;

		message
			.Should()
			.Contain(
				start,
				"the message must name the directory the walk started from, not one it never visited");

		message
			.Should()
			.Contain(
				".git",
				"the message must name both conditions it was looking for");

		message
			.Should()
			.Contain(
				"src/DungeonMaster.App",
				"the message must name both conditions it was looking for");
	}

	[Fact]
	public void AppSourceFilesReturnsAnEmptyListWhenThePatternMatchesNothing()
	{
		var files = RepoPaths.AppSourceFiles("*.dm-no-such-extension");

		files
			.Should()
			.BeEmpty(
				"a scan that reaches nothing returns nothing; non-emptiness is the caller's guard, so "
				+ "that a vacuous scan fails against the fact that depended on it");
	}

	[Fact]
	public void AppSourceFilesReturnsAbsolutePathsUnderAppRootExcludingObjAndBinInOrdinalOrder()
	{
		var separator = Path.DirectorySeparatorChar;

		var files = RepoPaths.AppSourceFiles("*.cs");

		files
			.Should()
			.NotBeEmpty(
				"every claim below sits inside an enumeration and would pass on an empty list");

		files
			.Should()
			.AllSatisfy(path =>
			{
				Path.IsPathFullyQualified(path)
					.Should()
					.BeTrue(
						"callers hand these straight to File.ReadAllText");

				path
					.Should()
					.StartWith(RepoPaths.AppRoot)
					.And
					.NotContain($"{separator}obj{separator}")
					.And
					.NotContain($"{separator}bin{separator}");
			});

		files
			.Should()
			.BeInAscendingOrder(
				StringComparer.Ordinal,
				"the order is ordinal and stable across calls within a run");
	}

	public void Dispose()
	{
		foreach (var root in _temporaryRoots)
		{
			try
			{
				root.Delete(recursive: true);
			}
			catch (IOException)
			{
				// A leftover temporary directory is not worth failing a test run over.
			}
			catch (UnauthorizedAccessException)
			{
				// As above.
			}
		}
	}

	private static Func<string, bool> Within(
		DirectoryInfo tree,
		Func<string, bool> predicate) =>
		path =>
			path.StartsWith(
				tree.FullName,
				StringComparison.Ordinal)
			&& predicate(path);

	private static string CreateDirectory(
		string parent,
		string name) =>
		Directory
			.CreateDirectory(
				Path.Combine(
					parent,
					name))
			.FullName;

	private static void CreateAppProject(string root) =>
		Directory.CreateDirectory(
			Path.Combine(
				root,
				"src",
				"DungeonMaster.App"));

	private static void CreateGitFile(string root) =>
		File.WriteAllText(
			Path.Combine(
				root,
				".git"),
			"gitdir: C:/somewhere/.git/worktrees/example");

	/// <summary>
	///		Creates the kind of directory a test assembly actually runs from, so the walk has several
	///		levels to climb before it reaches a candidate.
	/// </summary>
	private static string CreateBuildOutputDirectory(string root) =>
		Directory
			.CreateDirectory(
				Path.Combine(
					root,
					"src",
					"DungeonMaster.Tests",
					"bin",
					"Debug",
					"net10.0"))
			.FullName;

	private DirectoryInfo NewTemporaryRoot()
	{
		var root = Directory.CreateTempSubdirectory("dm-repopaths-");

		_temporaryRoots.Add(root);

		return root;
	}
}
