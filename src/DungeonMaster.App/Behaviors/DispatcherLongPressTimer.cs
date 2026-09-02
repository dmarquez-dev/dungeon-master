using DungeonMaster.App.Core.Timing;

using Microsoft.Maui.Dispatching;

namespace DungeonMaster.App.Behaviors;

/// <summary>
///		<see cref="ILongPressTimer"/> over <c>IDispatcher.CreateTimer()</c>. The tick arrives on the
///		UI thread, so the pressed flag its consumers guard is written and read on one thread.
/// </summary>
/// <remarks>
///		<para>
///		UIKIT-017 / MASTER-062 offered two fix shapes: mark the flags <c>volatile</c>, or move the
///		tick onto the UI thread. This is the second. It removes the cross-thread read outright
///		rather than making it safe, and it is testable through an injected double.
///		</para>
///		<para>
///		Nothing in this type, and no test of it, demonstrates the memory-visibility race UIKIT-017
///		describes. That record itself says the misfire was never observed, and no test can force a
///		reordering. What this shape gives is the absence of a cross-thread read to reorder.
///		</para>
///		<para>
///		<c>NotebookView</c> already builds its debounce on <c>Dispatcher.CreateTimer()</c>, so the
///		pattern is this layer's own.
///		</para>
/// </remarks>
public sealed class DispatcherLongPressTimer : ILongPressTimer
{
	private readonly IDispatcherTimer _timer;
	private bool _disposed;

	/// <summary>
	///		Initializes a new instance of the <see cref="DispatcherLongPressTimer"/> class over the
	///		supplied dispatcher's timer.
	/// </summary>
	/// <param name="dispatcher">
	///		The dispatcher whose thread the tick is delivered on. In the app this is a
	///		<c>VisualElement</c>'s <c>Dispatcher</c>, which is the UI thread's.
	/// </param>
	/// <exception cref="ArgumentNullException"><paramref name="dispatcher"/> is null.</exception>
	public DispatcherLongPressTimer(IDispatcher dispatcher)
	{
		ArgumentNullException.ThrowIfNull(dispatcher);

		_timer = dispatcher.CreateTimer();

		// One-shot. Without this the delay would re-arm itself and a single press would raise
		// Elapsed for as long as nothing stopped it.
		_timer.IsRepeating = false;
		_timer.Tick += OnTick;
	}

	/// <inheritdoc/>
	public event EventHandler? Elapsed;

	/// <inheritdoc/>
	public void Start(TimeSpan delay)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentOutOfRangeException.ThrowIfLessThan(delay, TimeSpan.Zero);

		// Stop first: restarting a running dispatcher timer without stopping it leaves the
		// original deadline in place on some platforms.
		_timer.Stop();
		_timer.Interval = delay;
		_timer.Start();
	}

	/// <inheritdoc/>
	public void Stop()
	{
		if (_disposed)
		{
			return;
		}

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
		_timer.Stop();
		_timer.Tick -= OnTick;
	}

	private void OnTick(object? sender, EventArgs args)
	{
		// Raised straight through, on the dispatcher thread that the Start call was made on.
		// No try: a subscriber's exception propagates unchanged, as the contract requires.
		Elapsed?.Invoke(this, EventArgs.Empty);
	}
}
