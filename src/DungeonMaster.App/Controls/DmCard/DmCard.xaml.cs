using DungeonMaster.App.Behaviors;

namespace DungeonMaster.App.Controls;

/// <summary>
///		A themed card control that displays a title, optional subtitle, and an optional trailing action area.
///		Applies hover visual states and a hand cursor on pointer entry.
/// </summary>
public partial class DmCard : ContentView
{
	private const double HiddenOpacity = 0d;
	private const double VisibleOpacity = 1d;

	private bool _pointerOver;
	private int _focusedDescendants;

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
	/// <remarks>
	///		There is deliberately no <c>propertyChanged</c> callback. The subtitle row's visibility
	///		is expressed in <c>DmCard.xaml</c>, by two property <c>Trigger</c>s on
	///		<c>SubtitleLabel</c>'s own <c>Text</c> that hide it for a null value and for an empty
	///		one — together the whole of
	///		<c>!string.IsNullOrEmpty(Subtitle)</c>, which <c>NotNullConverter</c> alone cannot
	///		express because it tests nullness rather than emptiness. Expressing it in markup leaves
	///		the label one writer, where a markup default and a code-behind write were two, and puts
	///		the rule where a theme dictionary can reach it.
	/// </remarks>
	public static readonly BindableProperty SubtitleProperty =
		BindableProperty.Create(
			nameof(Subtitle),
			typeof(string),
			typeof(DmCard),
			null);

	/// <summary>
	///		Identifies the <see cref="TrailingContent"/> bindable property.
	/// </summary>
	public static readonly BindableProperty TrailingContentProperty =
		BindableProperty.Create(
			nameof(TrailingContent),
			typeof(View),
			typeof(DmCard),
			null,
			propertyChanged: (bindable, oldValue, newValue) =>
			{
				if (bindable is DmCard card)
				{
					card.TrailingPresenter.Content = newValue as View;
					card.TrackTrailingFocus(oldValue as View, newValue as View);
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
	///		Gets or sets a <see cref="View"/> placed in the card's trailing area, revealed while the
	///		pointer is over the card or the keyboard focus is inside that area.
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

		// The trailing region is hidden by OPACITY, not by IsVisible, and is laid out at all
		// times. An IsVisible=false element has no platform view, so it cannot take keyboard
		// focus: under the previous pointer-only reveal Tab never reached the Edit or Delete
		// button on a dungeon-list card at all. Measured before this change, the focus ring on
		// DungeonListPage was exactly three stops, repeating -- ListItem, CreateDungeonButton,
		// DungeonSearchEntry.
		TrailingPresenter.Opacity = HiddenOpacity;
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
		_pointerOver = true;
		UpdateTrailingReveal();
	}

	private void OnHoverExited(object? sender, EventArgs e)
	{
		Microsoft.Maui.Controls.VisualStateManager.GoToState(CardBorder, "Normal");
		_pointerOver = false;
		UpdateTrailingReveal();
	}

	/// <summary>
	///		Subscribes focus tracking to every descendant of the trailing content, so the region
	///		reveals itself for a keyboard user exactly as hover reveals it for a pointer user.
	/// </summary>
	private void TrackTrailingFocus(View? oldContent, View? newContent)
	{
		if (oldContent is not null)
		{
			ForEachVisualElement(oldContent, element =>
			{
				element.Focused -= OnTrailingDescendantFocused;
				element.Unfocused -= OnTrailingDescendantUnfocused;
			});
		}

		_focusedDescendants = 0;

		if (newContent is not null)
		{
			ForEachVisualElement(newContent, element =>
			{
				element.Focused += OnTrailingDescendantFocused;
				element.Unfocused += OnTrailingDescendantUnfocused;
			});
		}

		UpdateTrailingReveal();
	}

	private void OnTrailingDescendantFocused(object? sender, FocusEventArgs e)
	{
		_focusedDescendants++;
		UpdateTrailingReveal();
	}

	private void OnTrailingDescendantUnfocused(object? sender, FocusEventArgs e)
	{
		if (_focusedDescendants > 0)
		{
			_focusedDescendants--;
		}

		// Tabbing from one trailing button to the next raises Unfocused on the outgoing button
		// BEFORE Focused on the incoming one, so the count dips to zero in between. Re-evaluating
		// on the next dispatcher turn lets the incoming Focused land first and keeps the region
		// from flickering out and back in as focus crosses it.
		Dispatcher.Dispatch(UpdateTrailingReveal);
	}

	/// <summary>
	///		Reveals the trailing region while the pointer is over the card or the keyboard focus is
	///		inside it, and hides it when neither holds.
	/// </summary>
	private void UpdateTrailingReveal()
	{
		var revealed = _pointerOver || _focusedDescendants > 0;

		TrailingPresenter.Opacity = revealed ? VisibleOpacity : HiddenOpacity;

		// An Opacity=0 element is still hit-testable on Windows, and this region is now laid out
		// at all times rather than collapsed. Without this the invisible strip would sit over the
		// card's own TapGestureRecognizer and could take a pointer event -- or show a tooltip --
		// while the user can see nothing there. Measured: gating InputTransparent this way costs
		// neither route. The buttons remain in the Tab order while hidden (the focus ring still
		// reads ListItem, Edit, StartSession, Delete, CreateDungeon, Search), and a real mouse
		// moved straight onto the Edit button and clicked still reaches OnEditClicked, because
		// pointer-entered lands on the card and reveals the region before the click is hit-tested.
		TrailingPresenter.InputTransparent = !revealed;
	}

	private static void ForEachVisualElement(Element root, Action<VisualElement> action)
	{
		if (root is VisualElement visual)
		{
			action(visual);
		}

		foreach (var child in ((IVisualTreeElement)root).GetVisualChildren())
		{
			if (child is Element childElement)
			{
				ForEachVisualElement(childElement, action);
			}
		}
	}
}
