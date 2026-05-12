namespace DungeonMaster.App.Controls;

/// <summary>
///		A themed card control that displays a title, optional subtitle, and an optional trailing action area.
///		Applies hover visual states and a hand cursor on pointer entry.
/// </summary>
public partial class DmCard : ContentView
{
	/// <summary>
	///		Identifies the <see cref="Title"/> bindable property.
	/// </summary>
	public static readonly BindableProperty TitleProperty =
		BindableProperty.Create(
			nameof(Title),
			typeof(string),
			typeof(DmCard),
			string.Empty);

	/// <summary>
	///		Identifies the <see cref="Subtitle"/> bindable property.
	/// </summary>
	public static readonly BindableProperty SubtitleProperty =
		BindableProperty.Create(
			nameof(Subtitle),
			typeof(string),
			typeof(DmCard),
			null,
			propertyChanged: (bindable, _, _) =>
				((DmCard)bindable).UpdateSubtitleVisibility());

	/// <summary>
	///		Identifies the <see cref="TrailingContent"/> bindable property.
	/// </summary>
	public static readonly BindableProperty TrailingContentProperty =
		BindableProperty.Create(
			nameof(TrailingContent),
			typeof(View),
			typeof(DmCard),
			null,
			propertyChanged: (bindable, _, newValue) =>
			{
				if (bindable is DmCard card)
				{
					card.TrailingPresenter.Content = newValue as View;
				}
			});

	/// <summary>
	///		Gets or sets the primary heading text displayed on the card.
	/// </summary>
	public string Title
	{
		get => (string)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	/// <summary>
	///		Gets or sets the secondary text displayed beneath the title, or <see langword="null"/> to hide the subtitle row.
	/// </summary>
	public string? Subtitle
	{
		get => (string?)GetValue(SubtitleProperty);
		set => SetValue(SubtitleProperty, value);
	}

	/// <summary>
	///		Gets or sets a <see cref="View"/> placed in the card's trailing area, revealed on hover.
	/// </summary>
	public View? TrailingContent
	{
		get => (View?)GetValue(TrailingContentProperty);
		set => SetValue(TrailingContentProperty, value);
	}

	/// <summary>
	///		Initializes a new instance of <see cref="DmCard"/>, sets up hover tracking, and hides the trailing presenter.
	/// </summary>
	public DmCard()
	{
		InitializeComponent();
		WireHover();
		TrailingPresenter.IsVisible = false;
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
		TrailingPresenter.IsVisible = true;
#if WINDOWS
		SetWindowsCursor(Microsoft.UI.Input.InputSystemCursorShape.Hand);
#endif
	}

	private void OnPointerExited(object? sender, PointerEventArgs e)
	{
		Microsoft.Maui.Controls.VisualStateManager.GoToState(CardBorder, "Normal");
		TrailingPresenter.IsVisible = false;
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
				.GetProperty(
					"ProtectedCursor",
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
