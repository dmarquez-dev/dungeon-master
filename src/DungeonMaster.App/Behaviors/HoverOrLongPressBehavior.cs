using DungeonMaster.App.Core.Timing;

namespace DungeonMaster.App.Behaviors;

/// <summary>
///		Raises <see cref="HoverEntered"/> / <see cref="HoverExited"/> for a pointer, and
///		<see cref="LongPressed"/> for a press held past <see cref="LongPressDelay"/>.
/// </summary>
/// <remarks>
///		<para>
///		Both gesture recognizers this behavior adds are held in fields and removed again in
///		<see cref="OnDetachingFrom"/>. Behaviors "aren't implicitly removed from controls when pages
///		are popped from the navigation stack" (UIKIT-003 / MASTER-006), so a recogniser left in the
///		view's collection keeps the detached view alive through its handler chain, and a re-attach
///		would stack a second recogniser on top of the first.
///		</para>
///		<para>
///		The long-press delay runs on an <see cref="ILongPressTimer"/>, whose tick arrives on the
///		thread that started it. That replaces the <c>System.Timers.Timer</c> plus
///		<c>MainThread.BeginInvokeOnMainThread</c> shape UIKIT-017 / MASTER-062 records, and with it
///		the cross-thread read of <c>_touching</c>: the flag is now written and read on one thread.
///		Nothing here demonstrates the memory-visibility race that record describes, and no test can
///		- UIKIT-017 itself says the misfire was never observed. What is gone is the cross-thread
///		read there was to reorder.
///		</para>
/// </remarks>
public sealed class HoverOrLongPressBehavior : Behavior<View>
{
	/// <summary>How long a press must be held before <see cref="LongPressed"/> is raised.</summary>
	private static readonly TimeSpan LongPressDelay = TimeSpan.FromMilliseconds(450);

	private readonly Func<View, ILongPressTimer> _timerFactory;

	private PointerGestureRecognizer? _pointer;
	private PanGestureRecognizer? _pan;
	private ILongPressTimer? _longPressTimer;
	private bool _touching;

	/// <summary>
	///		Initializes a new instance of the <see cref="HoverOrLongPressBehavior"/> class whose
	///		long-press delay runs on the attached view's dispatcher. This is the constructor XAML and
	///		<c>DmCard</c> use.
	/// </summary>
	public HoverOrLongPressBehavior()
		: this(static bindable => new DispatcherLongPressTimer(bindable.Dispatcher))
	{
	}

	/// <summary>
	///		Initializes a new instance of the <see cref="HoverOrLongPressBehavior"/> class over an
	///		injected timer source, so the press lifecycle can be driven without a wall clock.
	/// </summary>
	/// <param name="timerFactory">
	///		Produces the timer for a view at the moment the behavior attaches to it. Called once per
	///		attach; the result is disposed on detach.
	/// </param>
	/// <exception cref="ArgumentNullException">
	///		<paramref name="timerFactory"/> is null.
	/// </exception>
	public HoverOrLongPressBehavior(Func<View, ILongPressTimer> timerFactory)
	{
		ArgumentNullException.ThrowIfNull(timerFactory);

		_timerFactory = timerFactory;
	}

	/// <summary>Raised when a pointer enters the attached view.</summary>
	public event EventHandler? HoverEntered;

	/// <summary>Raised when a pointer leaves the attached view.</summary>
	public event EventHandler? HoverExited;

	/// <summary>Raised once for a press held past the long-press delay and not yet released.</summary>
	public event EventHandler? LongPressed;

	/// <inheritdoc/>
	protected override void OnAttachedTo(View bindable)
	{
		base.OnAttachedTo(bindable);

		_pointer = new PointerGestureRecognizer();
		_pointer.PointerEntered += OnPointerEntered;
		_pointer.PointerExited += OnPointerExited;
		bindable.GestureRecognizers.Add(_pointer);

		// Held in a field for the same reason as the pointer: OnDetachingFrom has to be able to
		// unsubscribe it and take it back out of the view's collection (UIKIT-003).
		_pan = new PanGestureRecognizer();
		_pan.PanUpdated += OnPanUpdated;
		bindable.GestureRecognizers.Add(_pan);

		_longPressTimer = _timerFactory(bindable);
		_longPressTimer.Elapsed += OnLongPressElapsed;
	}

	/// <inheritdoc/>
	protected override void OnDetachingFrom(View bindable)
	{
		if (_longPressTimer is not null)
		{
			_longPressTimer.Elapsed -= OnLongPressElapsed;
			_longPressTimer.Stop();
			_longPressTimer.Dispose();
			_longPressTimer = null;
		}

		_touching = false;

		if (_pointer is not null)
		{
			_pointer.PointerEntered -= OnPointerEntered;
			_pointer.PointerExited -= OnPointerExited;
			bindable.GestureRecognizers.Remove(_pointer);
			_pointer = null;
		}

		if (_pan is not null)
		{
			_pan.PanUpdated -= OnPanUpdated;
			bindable.GestureRecognizers.Remove(_pan);
			_pan = null;
		}

		base.OnDetachingFrom(bindable);
	}

	private void OnPointerEntered(object? sender, PointerEventArgs e) =>
		HoverEntered?.Invoke(this, EventArgs.Empty);

	private void OnPointerExited(object? sender, PointerEventArgs e) =>
		HoverExited?.Invoke(this, EventArgs.Empty);

	private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
	{
		switch (e.StatusType)
		{
			case GestureStatus.Started:
				_touching = true;
				_longPressTimer?.Start(LongPressDelay);
				break;

			case GestureStatus.Completed:
			case GestureStatus.Canceled:
				_touching = false;
				_longPressTimer?.Stop();
				break;
		}
	}

	private void OnLongPressElapsed(object? sender, EventArgs e)
	{
		// Same thread as the Start call, so this read of _touching is the same thread's write.
		// No re-marshalling here: the timer already delivers on the dispatcher thread.
		if (!_touching)
		{
			return;
		}

		LongPressed?.Invoke(this, EventArgs.Empty);
	}
}
