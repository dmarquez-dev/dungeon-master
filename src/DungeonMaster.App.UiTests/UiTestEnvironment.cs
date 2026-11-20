using System;
using System.IO;

namespace DungeonMaster.App.UiTests;

/// <summary>
/// Locates the two things every session in this project needs: the Appium server endpoint and
/// the unpackaged <c>DungeonMaster.App.exe</c> under test.
/// </summary>
/// <remarks>
/// The app is unpackaged (<c>WindowsPackageType</c> is <c>None</c>), so it has no package
/// identity and no AUMID. It is addressed by absolute executable path, which is the form the
/// installed <c>windows@6.1.1</c> driver documents for a classic app.
/// </remarks>
internal static class UiTestEnvironment
{
	/// <summary>Environment variable overriding <see cref="ServerUri"/>.</summary>
	internal const string ServerUriVariable = "DM_UITESTS_APPIUM_URL";

	/// <summary>Environment variable overriding <see cref="AppExecutablePath"/>.</summary>
	internal const string AppPathVariable = "DM_UITESTS_APP_PATH";

	/// <summary>The build configuration whose output this project drives by default.</summary>
	internal const string Configuration = "Debug";

	/// <summary>The Windows target framework the app's executable is produced under.</summary>
	internal const string AppTargetFramework = "net10.0-windows10.0.19041.0";

	/// <summary>The runtime identifier folder the app's executable is produced under.</summary>
	internal const string AppRuntimeIdentifier = "win-x64";

	/// <summary>The Appium server endpoint, default <c>http://127.0.0.1:4723/</c>.</summary>
	internal static Uri ServerUri
	{
		get
		{
			var configured = Environment.GetEnvironmentVariable(ServerUriVariable);
			return string.IsNullOrWhiteSpace(configured)
				? new Uri("http://127.0.0.1:4723/")
				: new Uri(configured);
		}
	}

	/// <summary>
	/// Absolute path to the built <c>DungeonMaster.App.exe</c>.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// The executable is not where the repository layout puts it. The message names the path that
	/// was looked for and the command that produces it, because a missing build is by far the most
	/// likely reason a run of this project fails.
	/// </exception>
	internal static string AppExecutablePath
	{
		get
		{
			var configured = Environment.GetEnvironmentVariable(AppPathVariable);
			var path = string.IsNullOrWhiteSpace(configured)
				? Path.Combine(
					RepositoryRoot,
					"src",
					"DungeonMaster.App",
					"bin",
					Configuration,
					AppTargetFramework,
					AppRuntimeIdentifier,
					"DungeonMaster.App.exe")
				: configured;

			if (!File.Exists(path))
			{
				throw new InvalidOperationException(
					$"DungeonMaster.App.exe was not found at '{path}'. Build it first with: "
					+ $"dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj "
					+ $"-f {AppTargetFramework} -c {Configuration}. "
					+ $"Set {AppPathVariable} to point somewhere else.");
			}

			return path;
		}
	}

	/// <summary>
	/// Walks upward from the test assembly until it finds an ancestor carrying BOTH a
	/// <c>.git</c> entry and a <c>src/DungeonMaster.App</c> directory.
	/// </summary>
	/// <remarks>
	/// Both conditions are required for the reason <c>RepoPaths</c> in DungeonMaster.Tests records:
	/// a nested repository with a <c>.git</c> entry and no app beneath it would otherwise stop the
	/// walk at the wrong ancestor. <c>.git</c> is accepted as a directory or as a FILE, because the
	/// linked-worktree layout writes a file.
	/// </remarks>
	/// <exception cref="InvalidOperationException">
	///		No ancestor satisfies both conditions.
	/// </exception>
	internal static string RepositoryRoot
	{
		get
		{
			var start = AppContext.BaseDirectory;
			for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
			{
				var git = Path.Combine(directory.FullName, ".git");
				var app = Path.Combine(directory.FullName, "src", "DungeonMaster.App");
				if ((Directory.Exists(git) || File.Exists(git)) && Directory.Exists(app))
				{
					return directory.FullName;
				}
			}

			throw new InvalidOperationException(
				$"No ancestor of '{start}' carries both a '.git' entry and a 'src/DungeonMaster.App' directory.");
		}
	}
}
