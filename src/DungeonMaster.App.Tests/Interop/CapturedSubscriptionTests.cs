using DungeonMaster.App.Core.Interop;

namespace DungeonMaster.App.Tests.Interop;

/// <summary>
///		Pins the unsubscribe UI-018 / MASTER-055 records: the detach runs against the object the
///		attach subscribed to, not against whatever a fresh resolution yields at teardown time.
/// </summary>
/// <remarks>
///		<para>
///		Host-free on purpose. The real target is a native WinUI <c>FrameworkElement</c> reached
///		through <c>MapCanvas.Handler.PlatformView</c>, and spike-host-report.md section 5 records
///		that whether a <c>GraphicsView</c> acquires a real handler under the test host was never
///		settled - so no test here asserts anything about that. What is asserted is the seam the page
///		now goes through, exercised with a target that records the calls made on it.
///		</para>
/// </remarks>
public sealed class CapturedSubscriptionTests
{
	/// <summary>
	///		The defect's exact shape: by teardown the resolver yields null, and an unsubscribe that
	///		re-resolved would do nothing while the still-live target kept the handler. Detaching from
	///		the captured reference removes it.
	/// </summary>
	[Fact]
	public void DetachUnsubscribesFromTheCapturedTargetOnceEvenWhenTheResolverNowYieldsNull()
	{
		var target = new RecordingTarget();
		RecordingTarget? resolved = target;

		var subscription = new CapturedSubscription<RecordingTarget>(
			() => resolved,
			candidate => candidate.Subscribe(),
			candidate => candidate.Unsubscribe());

		subscription.Attach();

		// The canvas handler is disconnected before the page's own: the resolver can no longer
		// produce the element, but the element is alive and still holds the delegate.
		resolved = null;

		subscription.Detach();

		Assert.Equal(1, target.SubscribeCount);
		Assert.Equal(1, target.UnsubscribeCount);
	}

	/// <summary>
	///		A second detach unsubscribes nothing, because the first released the captured target.
	/// </summary>
	[Fact]
	public void ASecondDetachUnsubscribesNothingFurther()
	{
		var target = new RecordingTarget();

		var subscription = new CapturedSubscription<RecordingTarget>(
			() => target,
			candidate => candidate.Subscribe(),
			candidate => candidate.Unsubscribe());

		subscription.Attach();
		subscription.Detach();
		subscription.Detach();

		Assert.Equal(1, target.UnsubscribeCount);
	}

	/// <summary>
	///		Attaching twice subscribes once. The page runs a detach/attach pair every time the canvas
	///		handler changes, and a second subscribe would deliver every scroll notch twice.
	/// </summary>
	[Fact]
	public void AttachingTwiceWithoutADetachSubscribesOnlyOnce()
	{
		var target = new RecordingTarget();

		var subscription = new CapturedSubscription<RecordingTarget>(
			() => target,
			candidate => candidate.Subscribe(),
			candidate => candidate.Unsubscribe());

		subscription.Attach();
		subscription.Attach();

		Assert.Equal(1, target.SubscribeCount);
		Assert.Equal(0, target.UnsubscribeCount);
	}

	/// <summary>
	///		After a detach the seam is reusable: the next attach resolves again and subscribes to
	///		whatever the resolver now yields, which is how the page rehooks a recreated canvas
	///		handler.
	/// </summary>
	[Fact]
	public void AttachAfterADetachSubscribesToTheTargetTheResolverNowYields()
	{
		var first = new RecordingTarget();
		var second = new RecordingTarget();
		var resolved = first;

		var subscription = new CapturedSubscription<RecordingTarget>(
			() => resolved,
			candidate => candidate.Subscribe(),
			candidate => candidate.Unsubscribe());

		subscription.Attach();
		subscription.Detach();

		resolved = second;
		subscription.Attach();

		Assert.Equal(1, first.SubscribeCount);
		Assert.Equal(1, first.UnsubscribeCount);
		Assert.Equal(1, second.SubscribeCount);
		Assert.Equal(0, second.UnsubscribeCount);
	}

	/// <summary>
	///		Attaching while the resolver yields nothing subscribes to nothing and leaves the seam
	///		unattached, so the following detach has nothing to unsubscribe from.
	/// </summary>
	[Fact]
	public void AttachWhileTheResolverYieldsNullSubscribesToNothing()
	{
		var target = new RecordingTarget();

		var subscription = new CapturedSubscription<RecordingTarget>(
			() => null,
			candidate => candidate.Subscribe(),
			candidate => candidate.Unsubscribe());

		subscription.Attach();
		subscription.Detach();

		Assert.Equal(0, target.SubscribeCount);
		Assert.Equal(0, target.UnsubscribeCount);
	}

	/// <summary>
	///		Stands in for the native element the page subscribes to, recording what was done to it.
	/// </summary>
	private sealed class RecordingTarget
	{
		public int SubscribeCount { get; private set; }

		public int UnsubscribeCount { get; private set; }

		public void Subscribe() => SubscribeCount++;

		public void Unsubscribe() => UnsubscribeCount++;
	}
}
