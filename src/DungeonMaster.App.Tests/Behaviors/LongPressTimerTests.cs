using DungeonMaster.App.Behaviors;

using DungeonMaster.App.Tests.Hosting;

namespace DungeonMaster.App.Tests.Behaviors;

/// <summary>
///		Pins every guarantee and every error case the <c>ILongPressTimer</c> contract states, through
///		the test double its consumers share.
/// </summary>
/// <remarks>
///		<para>
///		These facts are host-free and clock-free. <see cref="FakeLongPressTimer.Tick"/> is the only
///		thing that makes a delay elapse, so a fact that passes did so because the timer behaved, not
///		because a wall-clock wait happened to be long enough.
///		</para>
///		<para>
///		Nothing here demonstrates the memory-visibility race UIKIT-017 describes, and no test can:
///		that record says the misfire was never observed, and a reordering cannot be forced. What is
///		demonstrated is the behaviour of the shape that leaves no cross-thread read to reorder.
///		</para>
/// </remarks>
public sealed class LongPressTimerTests
{
	private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(450);

	/// <summary>
	///		One-shot: a single <c>Start</c> yields at most one <c>Elapsed</c>, however many times the
	///		delay is driven afterwards.
	/// </summary>
	[Fact]
	public void ASingleStartRaisesElapsedOnceHoweverManyTimesTheTimerIsTicked()
	{
		using var timer = new FakeLongPressTimer();
		var raised = 0;

		timer.Elapsed += (_, _) => raised++;

		timer.Start(Delay);
		timer.Tick();
		timer.Tick();
		timer.Tick();

		Assert.Equal(1, raised);
	}

	/// <summary>
	///		A press released before the delay elapses must not fire. This is the whole point of the
	///		abstraction for its consumers.
	/// </summary>
	[Fact]
	public void StoppingBeforeTheDelayElapsesRaisesNoElapsed()
	{
		using var timer = new FakeLongPressTimer();
		var raised = 0;

		timer.Elapsed += (_, _) => raised++;

		timer.Start(Delay);
		timer.Stop();
		timer.Tick();

		Assert.Equal(0, raised);
		Assert.False(timer.IsRunning);
	}

	/// <summary>
	///		A <c>Start</c> while running replaces the pending delay rather than queueing beside it,
	///		so two starts still yield exactly one <c>Elapsed</c> - only the last start can elapse.
	/// </summary>
	[Fact]
	public void StartingWhileRunningRestartsSoOnlyTheLastStartCanElapse()
	{
		using var timer = new FakeLongPressTimer();
		var raised = 0;

		timer.Elapsed += (_, _) => raised++;

		timer.Start(Delay);
		timer.Start(Delay);
		timer.Tick();
		timer.Tick();

		Assert.Equal(1, raised);
	}

	/// <summary>
	///		Starting a disposed timer is a consumer bug - a behavior or detector arming a press after
	///		its own teardown - and surfaces rather than silently doing nothing.
	/// </summary>
	[Fact]
	public void StartingAfterDisposeThrowsObjectDisposedException()
	{
		var timer = new FakeLongPressTimer();

		timer.Dispose();

		Assert.Throws<ObjectDisposedException>(() => timer.Start(Delay));
	}

	/// <summary>
	///		Stopping a disposed timer is not a bug: a teardown path that stops before it disposes, or
	///		twice, is ordinary. It is a no-op and does not throw.
	/// </summary>
	[Fact]
	public void StoppingAfterDisposeIsANoOpAndDoesNotThrow()
	{
		var timer = new FakeLongPressTimer();
		var raised = 0;

		timer.Elapsed += (_, _) => raised++;

		timer.Start(Delay);
		timer.Dispose();

		var failure = Record.Exception(timer.Stop);

		Assert.Null(failure);
		Assert.False(timer.IsRunning);

		timer.Tick();

		Assert.Equal(0, raised);
	}

	/// <summary>
	///		A negative delay is rejected at the call, naming the parameter, rather than becoming a
	///		delay that never elapses.
	/// </summary>
	[Fact]
	public void ANegativeDelayThrowsArgumentOutOfRangeExceptionNamingDelay()
	{
		using var timer = new FakeLongPressTimer();

		var failure = Assert.Throws<ArgumentOutOfRangeException>(
			() => timer.Start(TimeSpan.FromMilliseconds(-1)));

		Assert.Equal("delay", failure.ParamName);
	}

	/// <summary>
	///		A subscriber that throws must not be swallowed: the timer is not an error boundary, and a
	///		handler failing silently would hide a broken long-press gesture entirely.
	/// </summary>
	[Fact]
	public void AnElapsedHandlerThatThrowsPropagatesToTheCallerRatherThanBeingSwallowed()
	{
		using var timer = new FakeLongPressTimer();

		timer.Elapsed += (_, _) => throw new InvalidOperationException("handler failed");

		timer.Start(Delay);

		var failure = Assert.Throws<InvalidOperationException>(timer.Tick);

		Assert.Equal("handler failed", failure.Message);
	}
}

/// <summary>
///		Pins the one guarantee of <see cref="DispatcherLongPressTimer"/> that only a real dispatcher
///		can show: the tick arrives on the thread that started the delay.
/// </summary>
/// <remarks>
///		<para>
///		This is what replaces the <c>System.Timers.Timer</c> plus
///		<c>MainThread.BeginInvokeOnMainThread</c> shape UIKIT-017 / MASTER-062 records. The old shape
///		read the pressed flag on a thread-pool thread; this one has no cross-thread read left.
///		</para>
///		<para>
///		It does not, and cannot, demonstrate the memory-visibility race itself. UIKIT-017 records
///		that the misfire was never observed, and spike-host-report.md section 4 places the race at
///		tier 3, meaning no test can force a reordering.
///		</para>
///		<para>
///		A host is required: an <c>IDispatcher</c> comes from a <c>VisualElement</c>, and nothing
///		deriving from <c>Microsoft.Maui.Controls.Element</c> constructs without a started WinUI
///		application (spike-host-report.md section 3).
///		</para>
/// </remarks>
[Collection(MauiHostCollection.Name)]
public sealed class DispatcherLongPressTimerTests(MauiHostFixture host)
{
	/// <summary>
	///		How long the fact waits for the tick before failing. Wall-clock only as a failure bound:
	///		the fact completes as soon as the handler runs, and never sleeps for a fixed duration.
	/// </summary>
	private static readonly TimeSpan ElapsedTimeout = TimeSpan.FromSeconds(10);

	/// <summary>
	///		A null dispatcher is rejected at construction, naming the parameter. No host is needed to
	///		reach this: the guard runs before anything MAUI-typed is touched.
	/// </summary>
	[Fact]
	public void ANullDispatcherThrowsArgumentNullExceptionNamingDispatcher()
	{
		var failure = Assert.Throws<ArgumentNullException>(
			() => new DispatcherLongPressTimer(null!));

		Assert.Equal("dispatcher", failure.ParamName);
	}

	/// <summary>
	///		The tick is delivered on the thread that called <c>Start</c>. Both thread ids are captured
	///		at the moments that matter - one at the start call, one inside the handler - and compared.
	/// </summary>
	[Fact]
	public async Task TheDispatcherTimerRaisesElapsedOnTheThreadThatCalledStart()
	{
		var observed = new TaskCompletionSource<(int StartThread, int ElapsedThread)>(
			TaskCreationOptions.RunContinuationsAsynchronously);

		DispatcherLongPressTimer? timer = null;

		try
		{
			// The start happens on the host's dispatcher thread, and the wait happens off it: a
			// dispatcher timer cannot tick while its own thread is blocked waiting for the tick.
			host.Invoke(() =>
			{
				var created = new DispatcherLongPressTimer(new ContentPage().Dispatcher);
				timer = created;

				var startThread = Environment.CurrentManagedThreadId;

				created.Elapsed += (_, _) => observed.TrySetResult(
					(startThread, Environment.CurrentManagedThreadId));

				created.Start(TimeSpan.FromMilliseconds(20));
			});

			var (startThread, elapsedThread) = await observed.Task.WaitAsync(ElapsedTimeout);

			Assert.Equal(startThread, elapsedThread);
		}
		finally
		{
			if (timer is not null)
			{
				host.Invoke(timer.Dispose);
			}
		}
	}
}
