using DungeonMaster.App.Behaviors;
using DungeonMaster.App.Core.Timing;
using DungeonMaster.App.Tests.Hosting;

namespace DungeonMaster.App.Tests.Behaviors;

/// <summary>
///		Pins the teardown UIKIT-003 / MASTER-006 records: every gesture recogniser the behavior adds
///		is taken back out of the view when the behavior detaches.
/// </summary>
/// <remarks>
///		<para>
///		The evidence is the view's own <c>GestureRecognizers</c> collection after a real attach and a
///		real detach, not an assertion over source text. The unfixed behavior left 2 recognizers after
///		attach and 1 after detach (spike-host-report.md section 2c, probe P22 A); 1 is the leak and 0
///		is the fix.
///		</para>
///		<para>
///		A host is required: <c>BoxView</c> derives from <c>Microsoft.Maui.Controls.Element</c> and
///		does not construct without a started WinUI application (spike-host-report.md section 3).
///		</para>
/// </remarks>
[Collection(MauiHostCollection.Name)]
public sealed class HoverOrLongPressBehaviorTeardownTests(MauiHostFixture host)
{
	/// <summary>
	///		Attaching adds exactly the pointer and the pan recogniser, and detaching removes both -
	///		leaving nothing of the behavior behind in the view's collection to keep it alive.
	/// </summary>
	[Fact]
	public void DetachingTheBehaviorRemovesEveryGestureRecogniserItAdded()
	{
		host.Invoke(() =>
		{
			var view = new BoxView();
			var behavior = new HoverOrLongPressBehavior(static _ => new FakeLongPressTimer());

			view.Behaviors.Add(behavior);

			Assert.Equal(2, view.GestureRecognizers.Count);

			view.Behaviors.Remove(behavior);

			// Read into a local so a failure reports how many recognizers were left behind: 1 is
			// the leak UIKIT-003 records and 0 is the fix. Assert.Empty, which the xUnit analyzer
			// prefers over Assert.Equal(0, ...Count), would not say which.
			var remaining = view.GestureRecognizers.Count;

			Assert.Equal(0, remaining);
		});
	}

	/// <summary>
	///		Re-attaching the same behavior instance to the same view leaves 2 recognizers, not 3.
	///		UIKIT-003 names this consequence directly: a re-attach adds a second pan recogniser on top
	///		of the first, and both would arm the long-press timer for one gesture.
	/// </summary>
	[Fact]
	public void ReAttachingTheSameBehaviorLeavesTwoRecognisersRatherThanThree()
	{
		host.Invoke(() =>
		{
			var view = new BoxView();
			var behavior = new HoverOrLongPressBehavior(static _ => new FakeLongPressTimer());

			view.Behaviors.Add(behavior);
			view.Behaviors.Remove(behavior);
			view.Behaviors.Add(behavior);

			Assert.Equal(2, view.GestureRecognizers.Count);

			view.Behaviors.Remove(behavior);
		});
	}

	/// <summary>
	///		Detaching disposes the timer the attach created, so a behavior that outlives its view does
	///		not keep a dispatcher timer armed behind it.
	/// </summary>
	[Fact]
	public void DetachingDisposesTheTimerTheAttachCreated()
	{
		host.Invoke(() =>
		{
			var timer = new FakeLongPressTimer();
			var view = new BoxView();
			var behavior = new HoverOrLongPressBehavior(_ => timer);

			view.Behaviors.Add(behavior);

			Assert.False(timer.IsDisposed);

			view.Behaviors.Remove(behavior);

			Assert.True(timer.IsDisposed);
		});
	}
}

/// <summary>
///		Drives the real pan lifecycle through the attached recogniser and pins what the long-press
///		timer does at each step. This is MASTER-062's behavior half.
/// </summary>
/// <remarks>
///		<para>
///		The gesture is driven by casting the attached <c>PanGestureRecognizer</c> to
///		<c>Microsoft.Maui.Controls.IPanGestureController</c> and calling <c>SendPanStarted</c> /
///		<c>SendPanCompleted</c>, which is how the platform raises <c>PanUpdated</c>. Nothing here
///		asserts over source text.
///		</para>
///		<para>
///		Note on the namespace: this task's acceptance criterion names
///		<c>Microsoft.Maui.Controls.Internals.IPanGestureController</c>. No such type exists in the
///		MAUI version this solution builds against; the interface is public at
///		<c>Microsoft.Maui.Controls.IPanGestureController</c>, and that is what is used here.
///		</para>
///		<para>
///		Nothing in this class demonstrates the memory-visibility race UIKIT-017 describes, and no
///		test can - that record says the misfire was never observed. What these facts show is that the
///		timer tick arrives on the thread that started it, so there is no cross-thread read of the
///		pressed flag left to reorder.
///		</para>
/// </remarks>
[Collection(MauiHostCollection.Name)]
public sealed class HoverOrLongPressBehaviorLongPressTests(MauiHostFixture host)
{
	/// <summary>The gesture id the platform would supply; any stable value serves.</summary>
	private const int GestureId = 1;

	/// <summary>
	///		A press beginning arms the delay. Without this the long press could never fire at all.
	/// </summary>
	[Fact]
	public void APanStartingStartsTheLongPressTimer()
	{
		host.Invoke(() =>
		{
			var timer = new FakeLongPressTimer();
			var view = AttachTo(timer, out var pan, out _);

			pan.SendPanStarted(view, GestureId);

			Assert.True(timer.IsRunning);
		});
	}

	/// <summary>
	///		A press held past the delay raises <c>LongPressed</c> exactly once, on the thread that
	///		started the delay - no marshalling hop, and no repeat.
	/// </summary>
	[Fact]
	public void APressHeldPastTheDelayRaisesLongPressedOnceOnTheStartingThread()
	{
		host.Invoke(() =>
		{
			var timer = new FakeLongPressTimer();
			var view = AttachTo(timer, out var pan, out var behavior);
			var raised = 0;
			var handlerThreadId = 0;

			behavior.LongPressed += (_, _) =>
			{
				raised++;
				handlerThreadId = Environment.CurrentManagedThreadId;
			};

			pan.SendPanStarted(view, GestureId);

			var startingThreadId = Environment.CurrentManagedThreadId;

			timer.Tick();

			Assert.Equal(1, raised);
			Assert.Equal(startingThreadId, handlerThreadId);
		});
	}

	/// <summary>
	///		A press released before the delay elapses raises nothing. This is the user-visible bug the
	///		guard exists to prevent: a long-press menu for a tap the user already finished.
	/// </summary>
	[Fact]
	public void APressReleasedBeforeTheDelayElapsesRaisesNothing()
	{
		host.Invoke(() =>
		{
			var timer = new FakeLongPressTimer();
			var view = AttachTo(timer, out var pan, out var behavior);
			var raised = 0;

			behavior.LongPressed += (_, _) => raised++;

			pan.SendPanStarted(view, GestureId);
			pan.SendPanCompleted(view, GestureId);
			timer.Tick();

			Assert.Equal(0, raised);
			Assert.False(timer.IsRunning);
		});
	}

	/// <summary>
	///		The released-press guard holds even for a tick the stop could not cancel - the case a
	///		cancellation race would produce. The delay is re-armed after the release to stand in for a
	///		tick already in flight; the behavior must still raise nothing, because the press is over.
	/// </summary>
	[Fact]
	public void ATickArrivingAfterThePressWasReleasedRaisesNothing()
	{
		host.Invoke(() =>
		{
			var timer = new FakeLongPressTimer();
			var view = AttachTo(timer, out var pan, out var behavior);
			var raised = 0;

			behavior.LongPressed += (_, _) => raised++;

			pan.SendPanStarted(view, GestureId);
			pan.SendPanCompleted(view, GestureId);

			// An undeliverable-stop stand-in: the delay is armed again directly on the timer, so the
			// tick below is delivered even though the gesture has ended. Only the released-press
			// guard inside the behavior can keep LongPressed from being raised here.
			timer.Start(TimeSpan.FromMilliseconds(450));
			timer.Tick();

			Assert.Equal(0, raised);
		});
	}

	/// <summary>
	///		A cancelled gesture is a released press too, and must raise nothing for the same reason.
	/// </summary>
	[Fact]
	public void ACancelledPanRaisesNothingWhenTheDelayIsDrivenAfterwards()
	{
		host.Invoke(() =>
		{
			var timer = new FakeLongPressTimer();
			var view = AttachTo(timer, out var pan, out var behavior);
			var raised = 0;

			behavior.LongPressed += (_, _) => raised++;

			pan.SendPanStarted(view, GestureId);
			pan.SendPanCanceled(view, GestureId);

			timer.Start(TimeSpan.FromMilliseconds(450));
			timer.Tick();

			Assert.Equal(0, raised);
		});
	}

	/// <summary>
	///		Attaches a behavior over <paramref name="timer"/> to a fresh <c>BoxView</c> and hands back
	///		the view, the attached pan recogniser as its gesture controller, and the behavior.
	/// </summary>
	private static BoxView AttachTo(
		ILongPressTimer timer,
		out IPanGestureController pan,
		out HoverOrLongPressBehavior behavior)
	{
		var view = new BoxView();

		behavior = new HoverOrLongPressBehavior(_ => timer);
		view.Behaviors.Add(behavior);

		pan = view.GestureRecognizers.OfType<PanGestureRecognizer>().Single();

		return view;
	}
}
