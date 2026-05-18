namespace DungeonMaster.App.Behaviors;

/// <summary>Cursor shape options for <see cref="HandCursorBehavior"/>.</summary>
public enum BehaviorCursorShape { Hand, IBeam, Arrow }

/// <summary>
///		A MAUI behavior that changes the mouse cursor when hovering over the attached view.
///		Defaults to <see cref="BehaviorCursorShape.Hand"/>. Use <see cref="Shape"/> to select IBeam for text inputs.
///		Supported platforms: Windows (WinUI 3 ProtectedCursor), macCatalyst (NSCursor via UIHoverGestureRecognizer).
///		On iOS (touch-only devices without a pointer) the behavior attaches silently and does nothing.
/// </summary>
public class HandCursorBehavior : Behavior<View>
{
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
			return;

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

	private void SetCursor(bool active)
	{
#if WINDOWS
		if (_view?.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement element)
		{
			var shape = active
				? Shape switch
				{
					BehaviorCursorShape.IBeam => Microsoft.UI.Input.InputSystemCursorShape.IBeam,
					BehaviorCursorShape.Arrow => Microsoft.UI.Input.InputSystemCursorShape.Arrow,
					_                         => Microsoft.UI.Input.InputSystemCursorShape.Hand
				}
				: Microsoft.UI.Input.InputSystemCursorShape.Arrow;

			typeof(Microsoft.UI.Xaml.UIElement)
				.GetProperty(
					"ProtectedCursor",
					System.Reflection.BindingFlags.Instance |
					System.Reflection.BindingFlags.NonPublic |
					System.Reflection.BindingFlags.Public)
				?.SetValue(element, Microsoft.UI.Input.InputSystemCursor.Create(shape));
		}
#endif
	}
}
