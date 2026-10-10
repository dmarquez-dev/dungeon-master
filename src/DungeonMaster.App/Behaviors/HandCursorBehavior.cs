namespace DungeonMaster.App.Behaviors;

/// <summary>
///		A MAUI behavior that changes the mouse cursor when hovering over the attached view.
///		Defaults to <see cref="BehaviorCursorShape.Hand"/>. Use <see cref="Shape"/> to select IBeam for text inputs.
///		Supported platforms: Windows (WinUI 3 ProtectedCursor), macCatalyst (NSCursor via UIHoverGestureRecognizer).
///		On iOS (touch-only devices without a pointer) the behavior attaches silently and does nothing.
/// </summary>
public class HandCursorBehavior : Behavior<View>
{
	/// <summary>
	///		Selects which system cursor is shown while the pointer is over the attached view.
	///		Defaults to <see cref="BehaviorCursorShape.Hand"/>; set <see cref="BehaviorCursorShape.IBeam"/>
	///		for text inputs or <see cref="BehaviorCursorShape.Arrow"/> to show the ordinary arrow.
	/// </summary>
	public BehaviorCursorShape Shape { get; set; } = BehaviorCursorShape.Hand;

	private View? _view;
	private PointerGestureRecognizer? _gesture;

	/// <inheritdoc/>
	protected override void OnAttachedTo(View bindable)
	{
		base.OnAttachedTo(bindable);
		_view = bindable;
		_gesture = new PointerGestureRecognizer();
		_gesture.PointerEntered += OnPointerEntered;
		_gesture.PointerExited += OnPointerExited;
		bindable.GestureRecognizers.Add(_gesture);

#if MACCATALYST
		// On macCatalyst the MAUI PointerGestureRecognizer does not set the system cursor.
		// Wire a native UIHoverGestureRecognizer directly to the platform view once the handler
		// is ready so NSCursor changes are applied when the user hovers over this view.
		bindable.HandlerChanged += OnHandlerChanged;
#endif
	}

	/// <inheritdoc/>
	protected override void OnDetachingFrom(View bindable)
	{
		if (_gesture is not null)
		{
			_gesture.PointerEntered -= OnPointerEntered;
			_gesture.PointerExited -= OnPointerExited;
			bindable.GestureRecognizers.Remove(_gesture);
			_gesture = null;
		}

#if MACCATALYST
		bindable.HandlerChanged -= OnHandlerChanged;
#endif

		_view = null;
		base.OnDetachingFrom(bindable);
	}

	private void OnPointerEntered(object? sender, PointerEventArgs e) =>
		SetCursor(active: _view?.IsEnabled ?? false);

	private void OnPointerExited(object? sender, PointerEventArgs e) =>
		SetCursor(active: false);

#if MACCATALYST
	private UIKit.UIHoverGestureRecognizer? _hoverRecognizer;

	private void OnHandlerChanged(object? sender, EventArgs e)
	{
		if (_view?.Handler?.PlatformView is not UIKit.UIView nativeView)
		{
			return;
		}

		// Remove any previously attached hover recognizer to avoid duplicates on handler recycling.
		if (_hoverRecognizer is not null)
		{
			nativeView.RemoveGestureRecognizer(_hoverRecognizer);
			_hoverRecognizer = null;
		}

		var shape = Shape;
		_hoverRecognizer = new UIKit.UIHoverGestureRecognizer(_ => ApplyNSCursor(shape, _hoverRecognizer!));
		nativeView.AddGestureRecognizer(_hoverRecognizer);
	}

	private static void ApplyNSCursor(BehaviorCursorShape shape, UIKit.UIHoverGestureRecognizer recognizer)
	{
		// UIHoverGestureRecognizer fires on Began, Changed, and Ended.
		// Restore the arrow cursor when the pointer leaves (Ended/Cancelled).
		if (recognizer.State == UIKit.UIGestureRecognizerState.Ended
			|| recognizer.State == UIKit.UIGestureRecognizerState.Cancelled
			|| recognizer.State == UIKit.UIGestureRecognizerState.Failed)
		{
			AppKit.NSCursor.ArrowCursor.Set();
			return;
		}

		switch (shape)
		{
			case BehaviorCursorShape.IBeam:
				AppKit.NSCursor.IBeamCursor.Set();
				break;
			case BehaviorCursorShape.Arrow:
				AppKit.NSCursor.ArrowCursor.Set();
				break;
			default: // Hand
				AppKit.NSCursor.PointingHandCursor.Set();
				break;
		}
	}
#endif

#if WINDOWS
	/// <summary>
	///		The non-public <c>Microsoft.UI.Xaml.UIElement.ProtectedCursor</c> property, resolved once for the
	///		process rather than on every pointer event. Null when the running WinUI version does not expose it;
	///		<see cref="SetCursor"/> then returns without setting a cursor, and because the lookup lives here it
	///		is performed once and never retried per pointer event.
	/// </summary>
	private static readonly System.Reflection.PropertyInfo? ProtectedCursorProperty =
		typeof(Microsoft.UI.Xaml.UIElement)
			.GetProperty(
				"ProtectedCursor",
				System.Reflection.BindingFlags.Instance |
				System.Reflection.BindingFlags.NonPublic |
				System.Reflection.BindingFlags.Public);

	/// <summary>
	///		One system cursor per <see cref="BehaviorCursorShape"/>, created once and shared by every view
	///		carrying this behavior. System cursors are immutable, process-lifetime resources; they are never
	///		closed here and the same instance may be assigned to any number of elements.
	/// </summary>
	private static readonly Microsoft.UI.Input.InputSystemCursor HandCursor =
		Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.Hand);

	/// <inheritdoc cref="HandCursor"/>
	private static readonly Microsoft.UI.Input.InputSystemCursor IBeamCursor =
		Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.IBeam);

	/// <inheritdoc cref="HandCursor"/>
	private static readonly Microsoft.UI.Input.InputSystemCursor ArrowCursor =
		Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.Arrow);

	private static Microsoft.UI.Input.InputSystemCursor CachedCursor(BehaviorCursorShape shape) =>
		shape switch
		{
			BehaviorCursorShape.IBeam => IBeamCursor,
			BehaviorCursorShape.Arrow => ArrowCursor,
			_ => HandCursor
		};
#endif

	private void SetCursor(bool active)
	{
#if WINDOWS
		// The ProtectedCursor lookup failed once, at type initialization. There is nothing to set and
		// nothing to retry, so every later pointer event leaves the cursor alone.
		if (ProtectedCursorProperty is null)
		{
			return;
		}

		if (_view?.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement element)
		{
			ProtectedCursorProperty.SetValue(element, active ? CachedCursor(Shape) : ArrowCursor);
		}
#endif
	}
}
