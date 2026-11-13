using DungeonMaster.App.Core.Timing;

namespace DungeonMaster.App.Behaviors;

/// <summary>
///		Raises <see cref="LongPressed"/> for a canvas press held past the long-press delay and not
///		yet released.
/// </summary>
/// <remarks>
///		<para>
///		The delay runs on an <see cref="ILongPressTimer"/>, whose tick arrives on the thread that
///		started it. That replaces the <c>System.Timers.Timer</c> plus
///		<c>MainThread.BeginInvokeOnMainThread</c> shape UIKIT-017 / MASTER-062 records: the press
///		flag is now written and read on one thread.
///		</para>
///		<para>
///		What that does NOT mean: nothing in this type, and no test of it, demonstrates the
///		memory-visibility race UIKIT-017 describes, and no test can. That record itself says "I have
///		not observed the misfire and cannot state it as fact", and spike-host-report.md section 4
///		places the race at tier 3 - no test can force a reordering. The claim here is only that
///		there is no longer a cross-thread read to reorder.
///		</para>
/// </remarks>
public sealed class CanvasLongPressDetector : IDisposable
{
	/// <summary>How long a press must be held before <see cref="LongPressed"/> is raised.</summary>
	private static readonly TimeSpan LongPressDelay = TimeSpan.FromMilliseconds(450);

	private readonly ILongPressTimer _timer;

	/// <summary>
	///		Whether a press is down. Written by <see cref="TouchStarted"/> / <see cref="TouchEnded"/>
	///		on the UI thread and read in <see cref="OnElapsed"/>, which the timer raises on that same
	///		thread - one thread, both accesses.
	/// </summary>
	private bool _active;

	private bool _disposed;

	/// <summary>
	///		Initializes a new instance of the <see cref="CanvasLongPressDetector"/> class over the
	///		supplied timer, which it owns and disposes.
	/// </summary>
	/// <param name="timer">
	///		The one-shot delay. In the app this is a <c>DispatcherLongPressTimer</c> built on the
	///		page's dispatcher; a test passes a hand-driven double instead of waiting on a clock.
	/// </param>
	/// <exception cref="ArgumentNullException">
	///		<paramref name="timer"/> is null.
	/// </exception>
	public CanvasLongPressDetector(ILongPressTimer timer)
	{
		ArgumentNullException.ThrowIfNull(timer);

		_timer = timer;
		_timer.Elapsed += OnElapsed;
	}

	/// <summary>Raised once for a press held past the long-press delay and not yet released.</summary>
	public event EventHandler? LongPressed;

	/// <summary>Records that a press went down and starts, or restarts, the delay.</summary>
	public void TouchStarted()
	{
		_active = true;
		_timer.Start(LongPressDelay);
	}

	/// <summary>Records that the press was released and cancels the pending delay.</summary>
	public void TouchEnded()
	{
		_active = false;
		_timer.Stop();
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;
		_active = false;

		_timer.Elapsed -= OnElapsed;
		_timer.Stop();
		_timer.Dispose();
	}

	private void OnElapsed(object? sender, EventArgs args)
	{
		// Raised on the thread that called Start, so this read of _active is the same thread's
		// write. No re-marshalling: the timer has already delivered on the UI thread.
		if (!_active)
		{
			return;
		}

		LongPressed?.Invoke(this, EventArgs.Empty);
	}
}
