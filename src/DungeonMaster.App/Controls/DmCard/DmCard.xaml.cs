using DungeonMaster.App.Behaviors;

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
		// The hand cursor comes from HandCursorBehavior, the same one every other hoverable
		// surface in the app uses. This control used to carry a private copy of it — a #if
		// WINDOWS ProtectedCursor reflection block and a #elif MACCATALYST NSCursor block —
		// which is the duplication shape a previous review found in the palette views.
		Behaviors.Add(new HandCursorBehavior());

		var hover = new HoverOrLongPressBehavior();
		hover.HoverEntered += OnHoverEntered;
		hover.HoverExited += OnHoverExited;
		Behaviors.Add(hover);
	}

	private void OnHoverEntered(object? sender, EventArgs e)
	{
		Microsoft.Maui.Controls.VisualStateManager.GoToState(CardBorder, "PointerOver");
		TrailingPresenter.IsVisible = true;
	}

	private void OnHoverExited(object? sender, EventArgs e)
	{
		Microsoft.Maui.Controls.VisualStateManager.GoToState(CardBorder, "Normal");
		TrailingPresenter.IsVisible = false;
	}

	private void UpdateSubtitleVisibility() =>
		SubtitleLabel.IsVisible = !string.IsNullOrEmpty(Subtitle);
}
