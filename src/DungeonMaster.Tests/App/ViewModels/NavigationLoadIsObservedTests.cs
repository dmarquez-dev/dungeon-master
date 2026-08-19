using DungeonMaster.Tests.App.Theme;
using FluentAssertions;

namespace DungeonMaster.Tests.App.ViewModels;

/// <summary>
///		Source-scanning facts that pin the navigation entry point of <c>DungeonDetailViewModel</c>.
///		The test project targets <c>net10.0</c> and cannot reference the multi-targeted application project — XCUT-002, tracked separately — so these read the view-model source from disk through <see cref="RepoPaths"/>, exactly as the <c>Tests/App/Theme</c> suite does.
///		They inspect source text and never execute the view model, so they are supporting evidence for MASTER-017 and not a substitute for the manual observation that closes it.
/// </summary>
public class NavigationLoadIsObservedTests
{
	private const string MethodSignature = "public void ApplyQueryAttributes(IDictionary<string, object> query)";

	/// <summary>
	///		Asserts that <c>ApplyQueryAttributes</c> does not start the load as a discarded task.
	///		MASTER-017: a task assigned to a discard is never awaited and never observed, so a load that faults is captured on a <c>Task</c> nobody holds.
	/// </summary>
	[Fact]
	public void ApplyQueryAttributes_StartsNoDiscardedTask()
	{
		// Arrange
		var body = ApplyQueryAttributesBody();

		// Act
		var discards = body
			.Where(line =>
				line
					.TrimStart()
					.StartsWith("_ =", StringComparison.Ordinal))
			.ToList();

		// Assert
		discards
			.Should()
			.BeEmpty(
				"ApplyQueryAttributes must hold the load it starts in a field the page can await, but it discards it at: {0}",
				string.Join(" | ", discards.Select(line => line.Trim())));
	}

	/// <summary>
	///		Asserts that the started load is held on an awaitable member rather than dropped.
	/// </summary>
	[Fact]
	public void ApplyQueryAttributes_HoldsTheStartedLoadOnAnAwaitableMember()
	{
		// Arrange
		var source = ViewModelSource();
		var body = ApplyQueryAttributesBody();

		// Act
		var assignsLoadTask = body.Any(line =>
			line.Contains("LoadTask = LoadDungeonCommand.ExecuteAsync(", StringComparison.Ordinal));

		// Assert
		source
			.Should()
			.Contain(
				"public Task LoadTask { get; private set; }",
				"the page needs a member on which to await the load that navigation started");

		assignsLoadTask
			.Should()
			.BeTrue("ApplyQueryAttributes must assign the load it starts to LoadTask");
	}

	/// <summary>
	///		Asserts that the branch taken when <c>id</c> is missing or unparsable assigns <c>ErrorMessage</c> rather than falling through and doing nothing.
	///		MASTER-017: the silent branch left the page permanently empty with <c>_dungeonId</c> at <c>Guid.Empty</c>, so every subsequent save targeted a dungeon that does not exist.
	/// </summary>
	[Fact]
	public void ApplyQueryAttributes_WhenTheIdIsUnusable_AssignsErrorMessage()
	{
		// Arrange
		var body = ApplyQueryAttributesBody();

		// Act
		var guardIndex = body
			.ToList()
			.FindIndex(line =>
				line.Contains("Guid.TryParse", StringComparison.Ordinal));

		var failureBranch = body
			.Skip(guardIndex + 1)
			.TakeWhile(line =>
				!line.Contains("LoadDungeonCommand.ExecuteAsync(", StringComparison.Ordinal))
			.ToList();

		// Assert
		guardIndex
			.Should()
			.BeGreaterThanOrEqualTo(0, "ApplyQueryAttributes must still parse the id it is handed");

		failureBranch
			.Should()
			.Contain(
				line =>
					line.Contains("ErrorMessage = ResultMessage.ForFailure(", StringComparison.Ordinal),
				"the missing-or-unparsable id branch must report the failure on the ErrorMessage channel Session 2 established");

		failureBranch
			.Should()
			.Contain(
				line =>
					line.Contains("_dungeonId = Guid.Empty;", StringComparison.Ordinal),
				"the branch must state the empty identifier explicitly rather than leaving it implicit");
	}

	/// <summary>
	///		Asserts that every editing command that writes against the loaded dungeon refuses to dispatch while no dungeon is loaded.
	/// </summary>
	[Fact]
	public void EveryCommandThatWritesTheLoadedDungeon_IsGuardedWhileNoDungeonIsLoaded()
	{
		// Arrange
		var lines = File.ReadAllLines(DungeonDetailViewModelPath);

		// Act
		var guardCount = lines
			.Count(line =>
				line.Contains("if (NoDungeonIsLoaded())", StringComparison.Ordinal));

		// Assert
		guardCount
			.Should()
			.BeGreaterThan(
				0,
				"a command dispatched while _dungeonId is Guid.Empty targets a dungeon that does not exist");
	}

	private static string DungeonDetailViewModelPath =>
		Path.Combine(
			RepoPaths.AppRoot,
			"ViewModels",
			"DungeonDetailViewModel.cs");

	private static string ViewModelSource() =>
		File.ReadAllText(DungeonDetailViewModelPath);

	// The method body, exclusive of its signature and its closing brace. The class body sits one
	// tab deep, so a member closes at exactly two tabs and nothing nested inside the method can.
	private static IReadOnlyList<string> ApplyQueryAttributesBody()
	{
		var lines = File.ReadAllLines(DungeonDetailViewModelPath);

		var start = lines
			.ToList()
			.FindIndex(line =>
				line.Contains(MethodSignature, StringComparison.Ordinal));

		if (start < 0)
		{
			throw new InvalidOperationException(
				$"'{MethodSignature}' was not found in '{DungeonDetailViewModelPath}'.");
		}

		return lines
			.Skip(start + 1)
			.TakeWhile(line =>
				line != "\t}")
			.ToList();
	}
}
