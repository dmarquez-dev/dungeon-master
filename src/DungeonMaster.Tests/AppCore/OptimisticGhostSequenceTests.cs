using DungeonMaster.App.Core.Timing;
using FluentAssertions;

namespace DungeonMaster.Tests.AppCore;

/// <summary>
///		MASTER-166. Guards the ORDER of the tail of an optimistic-visual cycle: the placeholder
///		must be torn down only after the refresh that replaces it has completed, or a frame can
///		show neither the placeholder nor the persisted shape.
///		The order is asserted from recorded calls over the seam, not from the source text, a line
///		number, or elapsed wall-clock time — the last of which would make the assertion depend on
///		the scheduler and go intermittent.
/// </summary>
public class OptimisticGhostSequenceTests
{
	[Fact]
	public async Task RefreshThenClearAsync_ClearsThePlaceholderAfterTheRefreshHasCompleted()
	{
		var calls = new List<string>();

		await OptimisticGhostSequence.RefreshThenClearAsync(
			async () =>
			{
				calls.Add("refresh-started");
				await Task.Yield();
				calls.Add("refresh-completed");
			},
			() => calls.Add("placeholder-cleared"));

		calls.Should().Equal(
			"refresh-started",
			"refresh-completed",
			"placeholder-cleared");
	}

	/// <summary>
	///		The control for the order assertion above. It is deliberately order-INSENSITIVE: it
	///		stays green under an injected swap of the two steps, so a run in which it fails is a
	///		broken harness rather than a caught defect.
	/// </summary>
	[Fact]
	public async Task RefreshThenClearAsync_RunsEachStepExactlyOnce()
	{
		var refreshes = 0;
		var clears = 0;

		await OptimisticGhostSequence.RefreshThenClearAsync(
			() =>
			{
				refreshes++;

				return Task.CompletedTask;
			},
			() => clears++);

		refreshes.Should().Be(1);
		clears.Should().Be(1);
	}

	[Fact]
	public async Task RefreshThenClearAsync_WhenTheRefreshFaults_LeavesThePlaceholderOnScreen()
	{
		var cleared = false;

		var act = async () => await OptimisticGhostSequence.RefreshThenClearAsync(
			() => Task.FromException(new InvalidOperationException("reload failed")),
			() => cleared = true);

		await act.Should().ThrowAsync<InvalidOperationException>();
		cleared.Should().BeFalse();
	}

	[Fact]
	public async Task RefreshThenClearAsync_WithANullRefresh_Throws()
	{
		var act = async () => await OptimisticGhostSequence.RefreshThenClearAsync(
			null!,
			() => { });

		await act.Should().ThrowAsync<ArgumentNullException>();
	}

	[Fact]
	public async Task RefreshThenClearAsync_WithANullClear_Throws()
	{
		var act = async () => await OptimisticGhostSequence.RefreshThenClearAsync(
			() => Task.CompletedTask,
			null!);

		await act.Should().ThrowAsync<ArgumentNullException>();
	}
}
