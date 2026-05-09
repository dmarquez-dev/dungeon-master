namespace DungeonMaster.App.Behaviors;

public class HandCursorBehavior : Behavior<View>
{
    private View? _view;
    private PointerGestureRecognizer? _gesture;

    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);
        _view = bindable;
        _gesture = new PointerGestureRecognizer();
        _gesture.PointerEntered += OnPointerEntered;
        _gesture.PointerExited += OnPointerExited;
        bindable.GestureRecognizers.Add(_gesture);
    }

    protected override void OnDetachingFrom(View bindable)
    {
        if (_gesture is not null)
        {
            _gesture.PointerEntered -= OnPointerEntered;
            _gesture.PointerExited -= OnPointerExited;
            bindable.GestureRecognizers.Remove(_gesture);
            _gesture = null;
        }
        _view = null;
        base.OnDetachingFrom(bindable);
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e) => SetCursor(isHand: _view?.IsEnabled ?? false);
    private void OnPointerExited(object? sender, PointerEventArgs e) => SetCursor(isHand: false);

    private void SetCursor(bool isHand)
    {
#if WINDOWS
        if (_view?.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement element)
        {
            var shape = isHand
                ? Microsoft.UI.Input.InputSystemCursorShape.Hand
                : Microsoft.UI.Input.InputSystemCursorShape.Arrow;
            typeof(Microsoft.UI.Xaml.UIElement)
                .GetProperty("ProtectedCursor",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Public)
                ?.SetValue(element, Microsoft.UI.Input.InputSystemCursor.Create(shape));
        }
#endif
    }
}
