namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Resolves repository-relative paths for tests that scan application source files.
/// </summary>
public static class RepoPaths
{
	/// <summary>
	///		The production test for a <c>.git</c> entry. A .git DIRECTORY marks a normal clone; a
	///		.git FILE marks a linked worktree, where it holds a "gitdir: ..." pointer into the main
	///		repository's .git/worktrees. Testing only for the directory walked straight past a
	///		worktree root and either found the wrong root or ran off the top of the drive and threw
	///		— which takes out every theme fact at once, in the one workspace layout this repository
	///		actively uses.
	/// </summary>
	internal static readonly Func<string, bool> GitEntryExists =
		static path =>
			Directory.Exists(path)
			|| File.Exists(path);

	/// <summary>
	///		The production test for the application project directory. A directory carrying a
	///		<c>.git</c> entry is only THIS repository's root if the application project sits beneath
	///		it; without this second condition the walk stops at an outer repository that happens to
	///		contain the checkout, and every path composed from the result points somewhere else.
	/// </summary>
	internal static readonly Func<string, bool> AppProjectExists =
		static path =>
			Directory.Exists(path);

	private static readonly Lazy<string> LazyRoot = new(FindRoot);

	/// <summary>
	///		Gets the absolute path to the repository root.
	/// </summary>
	public static string Root =>
		LazyRoot.Value;

	/// <summary>
	///		Gets the absolute path to the DungeonMaster.App project directory.
	/// </summary>
	public static string AppRoot =>
		AppProjectPathUnder(Root);

	/// <summary>
	///		Returns every file under the application project matching the supplied pattern.
	///		Build output directories are excluded.
	/// </summary>
	/// <param name="searchPattern">
	///		The file glob to match, for example "*.xaml".
	/// </param>
	/// <remarks>
	///		Returns an EMPTY LIST when the pattern matches nothing. It does not throw and it does
	///		not assert: non-emptiness is the caller's guard, so that a vacuous scan fails against the
	///		fact that depended on it rather than against whichever fact happened to run first.
	/// </remarks>
	public static IReadOnlyList<string> AppSourceFiles(string searchPattern)
	{
		var separator = Path.DirectorySeparatorChar;

		return Directory
			.EnumerateFiles(
				AppRoot,
				searchPattern,
				SearchOption.AllDirectories)
			.Where(path =>
				!path.Contains($"{separator}obj{separator}")
				&& !path.Contains($"{separator}bin{separator}"))
			.OrderBy(
				path =>
					path,
				StringComparer.Ordinal)
			.ToList();
	}

	/// <summary>
	///		The walk's decision procedure, lifted clear of the real filesystem so that it can be
	///		exercised against a constructed layout. Walks upward from <paramref name="startDirectory"/>
	///		and returns the first ancestor that satisfies BOTH conditions; an ancestor satisfying only
	///		the first is skipped and the walk continues.
	/// </summary>
	/// <param name="startDirectory">
	///		The directory the walk starts from. In production this is
	///		<see cref="AppContext.BaseDirectory"/>.
	/// </param>
	/// <param name="gitEntryExists">
	///		Tests the composed path of a candidate's <c>.git</c> entry. Must accept both the file and
	///		the directory form; see <see cref="GitEntryExists"/>.
	/// </param>
	/// <param name="appProjectExists">
	///		Tests the composed path of a candidate's <c>src/DungeonMaster.App</c> directory.
	/// </param>
	/// <exception cref="InvalidOperationException">
	///		No ancestor of <paramref name="startDirectory"/> satisfied both conditions.
	/// </exception>
	internal static string FindRootFrom(
		string startDirectory,
		Func<string, bool> gitEntryExists,
		Func<string, bool> appProjectExists)
	{
		var directory = new DirectoryInfo(startDirectory);

		while (directory is not null)
		{
			var gitPath = Path.Combine(
				directory.FullName,
				".git");

			var appProjectPath = AppProjectPathUnder(directory.FullName);

			if (gitEntryExists(gitPath) && appProjectExists(appProjectPath))
			{
				return directory.FullName;
			}

			directory = directory.Parent;
		}

		throw new InvalidOperationException(
			$"Could not locate the repository root above '{startDirectory}'. A repository root is an "
			+ "ancestor that contains BOTH a '.git' entry (a directory in a normal clone, a file in a "
			+ "linked worktree) AND a 'src/DungeonMaster.App' directory. No ancestor of that starting "
			+ "directory contained both.");
	}

	private static string FindRoot() =>
		FindRootFrom(
			AppContext.BaseDirectory,
			GitEntryExists,
			AppProjectExists);

	private static string AppProjectPathUnder(string root) =>
		Path.Combine(
			root,
			"src",
			"DungeonMaster.App");
}
