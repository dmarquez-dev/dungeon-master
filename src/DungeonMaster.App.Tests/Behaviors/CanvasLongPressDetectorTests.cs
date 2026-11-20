using DungeonMaster.App.Behaviors;

namespace DungeonMaster.App.Tests.Behaviors;

/// <summary>
///		Drives the canvas press lifecycle through an injected <c>FakeLongPressTimer</c>, so the
///		long-press decision is observable without a wall clock and without an application host.
/// </summary>
/// <remarks>
///		<para>
///		These tests do NOT demonstrate the memory-visibility race UIKIT-017 / MASTER-062 describes,
///		and no test can: that record says the misfire was never observed, and spike-host-report.md
///		section 4 places forcing a reordering out of reach. What they demonstrate is the behaviour
///		that survives the fix - a released press raises nothing, and the tick arrives on the thread
///		that started it, so there is no cross-thread read left.
///		</para>
/// </remarks>
public sealed class CanvasLongPressDetectorTests
{
	/// <summary>
	///		A press held past the delay raises <c>LongPressed</c> exactly once, on the thread that
	///		made the delay elapse.
	/// </summary>
	[Fact]
	public void APressHeldPastTheDelayRaisesLongPressedOnceOnTheCallingThread()
	{
		var timer = new FakeLongPressTimer();
		using var detector = new CanvasLongPressDetector(timer);

		var raised = 0;
		var handlerThreadId = 0;

		detector.LongPressed += (_, _) =>
		{
			raised++;
			handlerThreadId = Environment.CurrentManagedThreadId;
		};

		detector.TouchStarted();

		var tickThreadId = Environment.CurrentManagedThreadId;

		timer.Tick();

		Assert.Equal(1, raised);
		Assert.Equal(tickThreadId, handlerThreadId);
	}

	/// <summary>
	///		A press released before the delay elapses raises nothing.
	/// </summary>
	[Fact]
	public void APressReleasedBeforeTheDelayElapsesRaisesNothing()
	{
		var timer = new FakeLongPressTimer();
		using var detector = new CanvasLongPressDetector(timer);

		var raised = 0;
		detector.LongPressed += (_, _) => raised++;

		detector.TouchStarted();
		detector.TouchEnded();
		timer.Tick();

		Assert.Equal(0, raised);
	}

	/// <summary>
	///		The released-press guard is the pressed flag, not merely the stopped timer: a tick that
	///		reaches the detector after a release still raises nothing.
	/// </summary>
	/// <remarks>
	///		<para>
	///		This test exists because the obvious one above cannot fail on its own account.
	///		<c>TouchEnded</c> both clears the flag and stops the timer, and a stopped
	///		<c>FakeLongPressTimer</c> ignores <c>Tick</c> - so removing the flag's clear leaves that
	///		test green and proves nothing about the guard. Re-arming the timer after the release
	///		isolates the flag, which is the whole of what MASTER-062 is about: a delivered tick must
	///		not fire a press the user already let go.
	///		</para>
	/// </remarks>
	[Fact]
	public void ATickDeliveredAfterAReleaseRaisesNothingEvenWhenTheTimerIsArmedAgain()
	{
		var timer = new FakeLongPressTimer();
		using var detector = new CanvasLongPressDetector(timer);

		var raised = 0;
		detector.LongPressed += (_, _) => raised++;

		detector.TouchStarted();
		detector.TouchEnded();

		// The tick the release was racing: armed behind the detector's back, so only the pressed
		// flag can suppress it.
		timer.Start(TimeSpan.FromMilliseconds(450));
		timer.Tick();

		Assert.Equal(0, raised);
	}

	/// <summary>
	///		Disposing the detector stops and disposes the timer it owns, and a tick afterwards raises
	///		nothing.
	/// </summary>
	[Fact]
	public void DisposeStopsAndDisposesTheTimerAndALaterTickRaisesNothing()
	{
		var timer = new FakeLongPressTimer();
		var detector = new CanvasLongPressDetector(timer);

		var raised = 0;
		detector.LongPressed += (_, _) => raised++;

		detector.TouchStarted();
		detector.Dispose();

		Assert.False(timer.IsRunning);
		Assert.True(timer.IsDisposed);

		timer.Tick();

		Assert.Equal(0, raised);
	}

	/// <summary>
	///		Disposing twice disposes the timer once and does not throw.
	/// </summary>
	[Fact]
	public void DisposingTwiceDoesNothingFurtherAndDoesNotThrow()
	{
		var timer = new FakeLongPressTimer();
		var detector = new CanvasLongPressDetector(timer);

		detector.Dispose();
		detector.Dispose();

		Assert.True(timer.IsDisposed);
	}

	/// <summary>
	///		A detector cannot be built without a timer: it owns and disposes the one it is given.
	/// </summary>
	[Fact]
	public void ConstructingWithoutATimerThrowsArgumentNullExceptionNamingTheParameter()
	{
		var exception = Assert.Throws<ArgumentNullException>(() => new CanvasLongPressDetector(null!));

		Assert.Equal("timer", exception.ParamName);
	}
}
