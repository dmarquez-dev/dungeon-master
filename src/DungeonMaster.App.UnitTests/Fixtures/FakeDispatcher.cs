using Microsoft.Maui.Dispatching;

namespace DungeonMaster.App.UnitTests.Fixtures;

/// <summary>
///		A dispatcher that runs everything inline on the calling thread.
/// </summary>
/// <remarks>
///		<para>
///		This exists for one measured reason. Constructing a XAML control attaches its behaviours,
///		and <c>HoverOrLongPressBehavior.OnAttachedTo</c> calls
///		<c>DispatcherExtensions.FindDispatcher</c>, which throws
///		<c>InvalidOperationException: BindableObject was not instantiated on a thread with a
///		dispatcher nor does the current application have a dispatcher</c> when neither the
///		<c>BindableObject</c> nor the ambient <c>DispatcherProvider</c> can supply one.
///		</para>
///		<para>
///		It is not part of the frozen fixture surface and no consumer names its type. It is reached
///		only through <see cref="AppFixture"/>.
///		</para>
/// </remarks>
internal sealed class FakeDispatcher : IDispatcher
{
	/// <summary>
	///		Always <see langword="false"/>: every thread is the dispatcher's thread here, so
	///		nothing ever needs marshalling.
	/// </summary>
	public bool IsDispatchRequired => false;

	/// <inheritdoc/>
	public IDispatcherTimer CreateTimer() => new FakeDispatcherTimer();

	/// <inheritdoc/>
	public bool Dispatch(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);

		// Inline rather than queued. A unit test asserts on state immediately after the call that
		// produced it, and a queue nothing pumps would leave that state never written.
		action();
		return true;
	}

	/// <summary>
	///		Refuses a delayed dispatch and says so by returning <see langword="false"/>.
	/// </summary>
	/// <param name="delay">The requested delay. Not honoured.</param>
	/// <param name="action">The work. Never invoked.</param>
	/// <returns>Always <see langword="false"/> — the work was not scheduled.</returns>
	/// <remarks>
	///		Running it inline would collapse the delay to zero, which is a different behaviour from
	///		the one the caller asked for and a worse one to debug than a refusal: a long-press timer
	///		would fire the instant a control was constructed. <see langword="false"/> is the
	///		interface's own way of saying the work was not scheduled, so a caller that checks the
	///		result learns the truth. A test that needs delayed work to run needs a real dispatcher,
	///		not this fixture.
	/// </remarks>
	public bool DispatchDelayed(TimeSpan delay, Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		return false;
	}
}

/// <summary>
///		A timer that records its configuration and never ticks.
/// </summary>
/// <remarks>
///		<see cref="Tick"/> is never raised. Behaviours that construct a timer during attachment get
///		a well-formed object; behaviour that depends on a tick ARRIVING is out of reach of this
///		fixture, and a test needing it needs a real dispatcher.
/// </remarks>
internal sealed class FakeDispatcherTimer : IDispatcherTimer
{
	/// <inheritdoc/>
	public TimeSpan Interval { get; set; }

	/// <inheritdoc/>
	public bool IsRepeating { get; set; }

	/// <inheritdoc/>
	public bool IsRunning { get; private set; }

	/// <summary>
	///		Never raised. See the remarks on <see cref="FakeDispatcherTimer"/>.
	/// </summary>
	public event EventHandler? Tick;

	/// <inheritdoc/>
	public void Start() => IsRunning = true;

	/// <inheritdoc/>
	public void Stop() => IsRunning = false;

	/// <summary>
	///		Raises <see cref="Tick"/>, so the event has a caller and the compiler does not report it
	///		as never raised. No fixture code calls this; a test that wants a tick calls it itself.
	/// </summary>
	internal void RaiseTick() => Tick?.Invoke(this, EventArgs.Empty);
}

/// <summary>
///		Hands the same <see cref="FakeDispatcher"/> to every thread that asks.
/// </summary>
/// <remarks>
///		Registered with <c>DispatcherProvider.SetCurrent(...)</c> by <see cref="AppFixture"/>, which
///		is what makes the ambient lookup in <c>DispatcherExtensions.FindDispatcher</c> succeed.
/// </remarks>
internal sealed class FakeDispatcherProvider : IDispatcherProvider
{
	private readonly FakeDispatcher _dispatcher = new();

	/// <inheritdoc/>
	public IDispatcher GetForCurrentThread() => _dispatcher;
}
