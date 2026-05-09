namespace DungeonMaster.App.Controls;

public partial class DmCard : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(DmCard), string.Empty);

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(DmCard), null,
            propertyChanged: (b, _, _) => ((DmCard)b).UpdateSubtitleVisibility());

    public static readonly BindableProperty TrailingContentProperty =
        BindableProperty.Create(nameof(TrailingContent), typeof(View), typeof(DmCard), null,
            propertyChanged: (b, _, newVal) =>
            {
                if (b is DmCard card)
                    card.TrailingPresenter.Content = newVal as View;
            });

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public View? TrailingContent
    {
        get => (View?)GetValue(TrailingContentProperty);
        set => SetValue(TrailingContentProperty, value);
    }

    public DmCard()
    {
        InitializeComponent();
        WireHover();
    }

    private void WireHover()
    {
        var gesture = new PointerGestureRecognizer();
        gesture.PointerEntered += OnPointerEntered;
        gesture.PointerExited += OnPointerExited;
        GestureRecognizers.Add(gesture);
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        Microsoft.Maui.Controls.VisualStateManager.GoToState(CardBorder, "PointerOver");
#if WINDOWS
        SetWindowsCursor(Microsoft.UI.Input.InputSystemCursorShape.Hand);
#endif
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        Microsoft.Maui.Controls.VisualStateManager.GoToState(CardBorder, "Normal");
#if WINDOWS
        SetWindowsCursor(Microsoft.UI.Input.InputSystemCursorShape.Arrow);
#endif
    }

#if WINDOWS
    private void SetWindowsCursor(Microsoft.UI.Input.InputSystemCursorShape shape)
    {
        if (Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement element)
        {
            var cursor = Microsoft.UI.Input.InputSystemCursor.Create(shape);
            var prop = typeof(Microsoft.UI.Xaml.UIElement)
                .GetProperty("ProtectedCursor",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Public);
            prop?.SetValue(element, cursor);
        }
    }
#endif

    private void UpdateSubtitleVisibility() =>
        SubtitleLabel.IsVisible = !string.IsNullOrEmpty(Subtitle);
}
