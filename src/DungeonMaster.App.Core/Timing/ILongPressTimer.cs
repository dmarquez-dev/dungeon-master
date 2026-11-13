namespace DungeonMaster.App.Core.Timing;

/// <summary>
///		A one-shot delay whose <see cref="Elapsed"/> event is raised on the same thread that
///		called <see cref="Start"/>.
/// </summary>
/// <remarks>
///		<para>
///		Same-thread delivery is the point of the abstraction. It replaces the
///		<c>MainThread.BeginInvokeOnMainThread</c> hop that a <c>System.Timers.Timer.Elapsed</c>
///		handler needed, and with it the cross-thread read of the pressed flag its consumers guard
///		(UIKIT-017 / MASTER-062). Consumers therefore must NOT re-marshal inside their handler.
///		</para>
///		<para>
///		This interface lives in DungeonMaster.App.Core, which references no MAUI package, so a
///		consumer that takes one can be tested without a running application host.
///		</para>
/// </remarks>
public interface ILongPressTimer : IDisposable
{
	/// <summary>
	///		Raised at most once per <see cref="Start"/> call, on the thread that called it.
	///		An exception thrown by a subscriber propagates unchanged; the timer does not swallow it.
	/// </summary>
	event EventHandler? Elapsed;

	/// <summary>
	///		Starts, or restarts, the delay. A call while a delay is running cancels that delay and
	///		begins a new one, so only the last <see cref="Start"/> can produce an <see cref="Elapsed"/>.
	/// </summary>
	/// <param name="delay">How long to wait before raising <see cref="Elapsed"/>.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	///		<paramref name="delay"/> is negative.
	/// </exception>
	/// <exception cref="ObjectDisposedException">
	///		The timer has been disposed.
	/// </exception>
	void Start(TimeSpan delay);

	/// <summary>
	///		Cancels a running delay. A no-op if none is running, and a no-op rather than a throw
	///		after the timer has been disposed. Once this returns on the owning thread, the pending
	///		tick will not be delivered.
	/// </summary>
	void Stop();
}
