namespace DungeonMaster.App.Behaviors;

public sealed class HoverOrLongPressBehavior : Behavior<View>
{
	private const int LongPressMs = 450;

	private View? _view;
	private PointerGestureRecognizer? _pointer;
	private System.Timers.Timer? _longPressTimer;
	private bool _touching;

	public event EventHandler? HoverEntered;
	public event EventHandler? HoverExited;
	public event EventHandler? LongPressed;

	protected override void OnAttachedTo(View bindable)
	{
		base.OnAttachedTo(bindable);
		_view = bindable;

		_pointer = new PointerGestureRecognizer();
		_pointer.PointerEntered += OnPointerEntered;
		_pointer.PointerExited += OnPointerExited;
		bindable.GestureRecognizers.Add(_pointer);

		var pan = new PanGestureRecognizer();
		pan.PanUpdated += OnPanUpdated;
		bindable.GestureRecognizers.Add(pan);
	}

	protected override void OnDetachingFrom(View bindable)
	{
		_longPressTimer?.Stop();
		_longPressTimer?.Dispose();
		_longPressTimer = null;

		if (_pointer is not null)
		{
			_pointer.PointerEntered -= OnPointerEntered;
			_pointer.PointerExited -= OnPointerExited;
			bindable.GestureRecognizers.Remove(_pointer);
			_pointer = null;
		}

		_view = null;
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
				_longPressTimer = new System.Timers.Timer(LongPressMs) { AutoReset = false };
				_longPressTimer.Elapsed += (_, _) =>
				{
					if (_touching)
						MainThread.BeginInvokeOnMainThread(() =>
							LongPressed?.Invoke(this, EventArgs.Empty));
				};
				_longPressTimer.Start();
				break;

			case GestureStatus.Completed:
			case GestureStatus.Canceled:
				_touching = false;
				_longPressTimer?.Stop();
				_longPressTimer?.Dispose();
				_longPressTimer = null;
				break;
		}
	}
}
