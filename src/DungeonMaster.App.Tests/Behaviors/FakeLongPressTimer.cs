using DungeonMaster.App.Core.Timing;

namespace DungeonMaster.App.Tests.Behaviors;

/// <summary>
///		A hand-driven <see cref="ILongPressTimer"/>. Nothing here sleeps or waits: a test calls
///		<see cref="Tick"/> to make the delay elapse, on the calling thread.
/// </summary>
/// <remarks>
///		Frozen in contracts/long-press-timer.md section 3 and shared by the consumers of the
///		abstraction, so their tests need neither a wall clock nor an application host.
/// </remarks>
public sealed class FakeLongPressTimer : ILongPressTimer
{
	/// <inheritdoc/>
	public event EventHandler? Elapsed;

	/// <summary>
	///		Gets a value indicating whether a delay is pending - that is, whether a
	///		<see cref="Tick"/> would raise <see cref="Elapsed"/>.
	/// </summary>
	public bool IsRunning { get; private set; }

	/// <summary>
	///		Gets a value indicating whether <see cref="Dispose"/> has been called.
	/// </summary>
	public bool IsDisposed { get; private set; }

	/// <inheritdoc/>
	public void Start(TimeSpan delay)
	{
		ObjectDisposedException.ThrowIf(IsDisposed, this);
		ArgumentOutOfRangeException.ThrowIfLessThan(delay, TimeSpan.Zero);

		// A restart replaces the pending delay rather than queueing beside it, so however many
		// times Start is called only one Tick can elapse.
		IsRunning = true;
	}

	/// <inheritdoc/>
	public void Stop() => IsRunning = false;

	/// <summary>
	///		Raises <see cref="ILongPressTimer.Elapsed"/> on the calling thread if the timer is
	///		running, and clears <see cref="IsRunning"/>. A no-op if it is not running - which is how
	///		a test asserts that a cancelled press produced nothing.
	/// </summary>
	public void Tick()
	{
		if (!IsRunning)
		{
			return;
		}

		// Cleared BEFORE the raise, so the one-shot guarantee holds even if the handler starts
		// the timer again, and so a handler that throws does not leave a delay pending.
		IsRunning = false;

		Elapsed?.Invoke(this, EventArgs.Empty);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		IsDisposed = true;
		IsRunning = false;
	}
}
