namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Resolves repository-relative paths for tests that scan application source files.
/// </summary>
public static class RepoPaths
{
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
        Path.Combine(
            Root,
            "src",
            "DungeonMaster.App");

    /// <summary>
    ///		Returns every file under the application project matching the supplied pattern.
    ///		Build output directories are excluded.
    /// </summary>
    /// <param name="searchPattern">
    ///		The file glob to match, for example "*.xaml".
    /// </param>
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

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            // A .git DIRECTORY marks a normal clone; a .git FILE marks a linked worktree, where
            // it holds a "gitdir: ..." pointer into the main repository's .git/worktrees. Testing
            // only for the directory walked straight past a worktree root and either found the
            // wrong root or ran off the top of the drive and threw — which takes out every theme
            // fact at once, in the one workspace layout this repository actively uses.
            var gitPath = Path.Combine(
                directory.FullName,
                ".git");

            if (Directory.Exists(gitPath) || File.Exists(gitPath))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"Could not locate the repository root above '{AppContext.BaseDirectory}'.");
    }
}
