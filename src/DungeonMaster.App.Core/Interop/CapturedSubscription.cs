namespace DungeonMaster.App.Core.Interop;

/// <summary>
///		Holds an event subscription against the exact target it was made on, so that unsubscribing
///		never depends on that target still being resolvable.
/// </summary>
/// <typeparam name="T">
///		The type of the object the subscription is made on - in the app, a native platform view
///		reached through a MAUI handler.
/// </typeparam>
/// <remarks>
///		<para>
///		UI-018 / MASTER-055: <c>DungeonDetailPage.DetachScrollZoom</c> used to unsubscribe by
///		re-resolving <c>MapCanvas.Handler?.PlatformView</c>. The null-conditional makes the whole
///		statement a no-op if the canvas handler has already been disconnected, and nothing orders
///		that disconnect against the page's own - leaving a native <c>FrameworkElement</c> holding a
///		delegate onto the page. That record's suggested direction is this type: "Capture the
///		FrameworkElement in a field when subscribing and unsubscribe from the captured reference,
///		clearing the field afterwards, so detach cannot depend on the handler still being
///		resolvable."
///		</para>
///		<para>
///		This lives in DungeonMaster.App.Core, which references no MAUI package, so it is generic
///		over the target and takes the subscribe and unsubscribe steps as delegates. That is also
///		what lets it be tested without an application host: the platform view the real page
///		captures is not constructible in a test process.
///		</para>
///		<para>
///		Single-threaded by design. Attach and detach both run on the UI thread in the app, and
///		nothing here synchronises.
///		</para>
/// </remarks>
public sealed class CapturedSubscription<T>
	where T : class
{
	private readonly Func<T?> _resolveTarget;
	private readonly Action<T> _subscribe;
	private readonly Action<T> _unsubscribe;

	/// <summary>
	///		The target <see cref="Attach"/> subscribed to, held until <see cref="Detach"/> releases
	///		it. This reference, and not a fresh resolution, is what the unsubscribe runs against.
	/// </summary>
	private T? _capturedTarget;

	/// <summary>
	///		Initializes a new instance of the <see cref="CapturedSubscription{T}"/> class.
	/// </summary>
	/// <param name="resolveTarget">
	///		Produces the object to subscribe to, or <see langword="null"/> when there is none yet.
	///		Called only by <see cref="Attach"/>.
	/// </param>
	/// <param name="subscribe">Adds the handler to the resolved target.</param>
	/// <param name="unsubscribe">Removes the handler from the captured target.</param>
	/// <exception cref="ArgumentNullException">Any argument is <see langword="null"/>.</exception>
	public CapturedSubscription(Func<T?> resolveTarget, Action<T> subscribe, Action<T> unsubscribe)
	{
		ArgumentNullException.ThrowIfNull(resolveTarget);
		ArgumentNullException.ThrowIfNull(subscribe);
		ArgumentNullException.ThrowIfNull(unsubscribe);

		_resolveTarget = resolveTarget;
		_subscribe = subscribe;
		_unsubscribe = unsubscribe;
	}

	/// <summary>
	///		Resolves a target and subscribes to it, capturing it for the later detach. A no-op if a
	///		subscription is already held - so a caller that attaches twice subscribes once - and a
	///		no-op if the target does not resolve.
	/// </summary>
	public void Attach()
	{
		if (_capturedTarget is not null)
		{
			return;
		}

		if (_resolveTarget() is not { } target)
		{
			return;
		}

		_subscribe(target);
		_capturedTarget = target;
	}

	/// <summary>
	///		Unsubscribes from the captured target and releases it. A no-op if no subscription is
	///		held, so a second call unsubscribes nothing.
	/// </summary>
	/// <remarks>
	///		The resolver is deliberately not consulted here. Re-resolving is the defect UI-018
	///		records: by the time a page tears down, the resolver can yield <see langword="null"/>
	///		while the object subscribed to is still alive and still holding the handler.
	/// </remarks>
	public void Detach()
	{
		if (_capturedTarget is not { } target)
		{
			return;
		}

		// Cleared before the unsubscribe, so a throwing unsubscribe cannot leave a stale target
		// behind for a later Detach to run against a second time.
		_capturedTarget = null;

		_unsubscribe(target);
	}
}
